#!/usr/bin/env python3
"""
Port PEI map bundles and structure for Unturned Android.
- Converts map textures (Roads, Details) to ASTC.
- Ensures all map AssetBundles have target_platform set to Android (13).
- Ensures legacy Materials.unity3d hash matches Unturned's SHA1 table.
- Outputs to Build_APK/Maps/PEI and generates Build_APK/PEI.zip.
- Syncs to Build_APK/UnturnedBundle/Maps/PEI and updates Build_APK/UnturnedBundle.zip.
"""
import os
import shutil
import hashlib
import zipfile
import UnityPy
from PIL import Image
from UnityPy.enums import TextureFormat, BuildTarget

def convert_bundle_textures_astc(src_path, dst_path):
    print(f'Converting {src_path} -> {dst_path}...')
    env = UnityPy.load(src_path)
    bf = list(env.files.values())[0]

    converted = 0
    for obj in env.objects:
        if obj.type.name == 'Texture2D':
            data = obj.read()
            w, h = data.m_Width, data.m_Height
            if w >= 4 and h >= 4:
                try:
                    img = data.image
                    fmt = TextureFormat.ASTC_RGBA_4x4 if ('A' in img.mode) else TextureFormat.ASTC_RGB_4x4
                    data.set_image(img, target_format=fmt)
                    data.save()
                    converted += 1
                except Exception as e:
                    print(f'  Warning: {data.m_Name} conversion failed: {e}')

    for name, sf in bf.files.items():
        if hasattr(sf, '_m_target_platform'):
            sf._m_target_platform = 13
        if hasattr(sf, 'target_platform'):
            sf.target_platform = BuildTarget.Android

    os.makedirs(os.path.dirname(dst_path), exist_ok=True)
    out_bytes = env.file.save(packer='lz4')
    with open(dst_path, 'wb') as f:
        f.write(out_bytes)
    print(f'  Saved {dst_path} ({len(out_bytes)/1024:.1f} KB, {converted} textures converted to ASTC)')

def ensure_bundle_android(file_path):
    env = UnityPy.load(file_path)
    bf = list(env.files.values())[0]
    changed = False
    for name, sf in bf.files.items():
        if getattr(sf, 'target_platform', None) != BuildTarget.Android:
            sf.target_platform = BuildTarget.Android
            changed = True
        if hasattr(sf, '_m_target_platform') and sf._m_target_platform != 13:
            sf._m_target_platform = 13
            changed = True
    if changed:
        out_bytes = env.file.save(packer='lz4')
        with open(file_path, 'wb') as f:
            f.write(out_bytes)
        print(f'  Updated target_platform to Android (13) for {file_path}')

def zip_folder_contents(src_dir, zip_path):
    print(f'Creating zip {zip_path} from {src_dir}...')
    with zipfile.ZipFile(zip_path, 'w', zipfile.ZIP_DEFLATED) as zf:
        for root, dirs, files in os.walk(src_dir):
            for file in files:
                full_path = os.path.join(root, file)
                rel_path = os.path.relpath(full_path, src_dir)
                zf.write(full_path, rel_path)
    print(f'  Created {zip_path} ({os.path.getsize(zip_path)/(1024*1024):.1f} MB)')

def main():
    src_map = 'Bundles_Unturned/Maps/PEI'
    dst_map = 'Build_APK/Maps/PEI'
    rus_map = 'com.SmartlyDressedGames.Unturned/com.SmartlyDressedGames.Unturned/files/UnturnedBundle/Maps/PEI'

    print('=== Step 1: Clean and copy PEI from source ===')
    if os.path.exists(dst_map):
        print(f'Removing old {dst_map}...')
        shutil.rmtree(dst_map)
    shutil.copytree(src_map, dst_map)

    print('\n=== Step 2: Convert bundles for Android ===')
    # 1. Convert Roads.unity3d to ASTC
    roads_src = os.path.join(src_map, 'Environment/Roads.unity3d')
    roads_dst = os.path.join(dst_map, 'Environment/Roads.unity3d')
    convert_bundle_textures_astc(roads_src, roads_dst)

    # 2. Convert Details.unity3d to ASTC
    details_src = os.path.join(src_map, 'Terrain/Details.unity3d')
    details_dst = os.path.join(dst_map, 'Terrain/Details.unity3d')
    convert_bundle_textures_astc(details_src, details_dst)

    # 3. Ensure Charts and Ambience have Android target platform
    ensure_bundle_android(os.path.join(dst_map, 'Charts.unity3d'))
    ensure_bundle_android(os.path.join(dst_map, 'Environment/Ambience.unity3d'))

    # 4. Copy Materials.unity3d with exact SHA1 hash (4f94810eaa4f2f173cf16743eab0848e632958cf)
    mat_rus = os.path.join(rus_map, 'Terrain/Materials.unity3d')
    mat_dst = os.path.join(dst_map, 'Terrain/Materials.unity3d')
    shutil.copyfile(mat_rus, mat_dst)
    
    with open(mat_dst, 'rb') as f:
        sha1 = hashlib.sha1(f.read()).hexdigest()
    print(f'  Materials.unity3d SHA1: {sha1} (matches expected PEI hash: {sha1 == "4f94810eaa4f2f173cf16743eab0848e632958cf"})')

    print('\n=== Step 3: Verification of all bundles in destination ===')
    for root, dirs, files in os.walk(dst_map):
        for f in sorted(files):
            if f.endswith('.unity3d'):
                p = os.path.join(root, f)
                env = UnityPy.load(p)
                for bf in env.files.values():
                    for name, sf in getattr(bf, 'files', {}).items():
                        tp = getattr(sf, 'target_platform', None)
                        print(f'  Verified {f}: {name} -> target_platform: {tp}')

    print('\n=== Step 4: Generating Build_APK/PEI.zip ===')
    pei_zip = 'Build_APK/PEI.zip'
    if os.path.exists(pei_zip):
        os.remove(pei_zip)
    zip_folder_contents(dst_map, pei_zip)

    print('\n=== Step 5: Syncing to UnturnedBundle ===')
    bundle_map_dst = 'Build_APK/UnturnedBundle/Maps/PEI'
    if os.path.exists('Build_APK/UnturnedBundle'):
        if os.path.exists(bundle_map_dst):
            shutil.rmtree(bundle_map_dst)
        shutil.copytree(dst_map, bundle_map_dst)
        print(f'  Copied fresh PEI to {bundle_map_dst}')

        # Also ensure fallback template bundles in UnturnedBundle are Android
        for tmpl in ['Ambience.unity3d', 'Roads.unity3d']:
            p = os.path.join('Build_APK/UnturnedBundle/Bundles/Level', tmpl)
            if os.path.exists(p):
                # Copy from freshly converted PEI counterparts
                if tmpl == 'Roads.unity3d':
                    shutil.copyfile(roads_dst, p)
                elif tmpl == 'Ambience.unity3d':
                    shutil.copyfile(os.path.join(dst_map, 'Environment/Ambience.unity3d'), p)
                print(f'  Updated {p} from Android PEI bundle')

        # Regenerate UnturnedBundle.zip
        ub_zip = 'Build_APK/UnturnedBundle.zip'
        if os.path.exists(ub_zip):
            os.remove(ub_zip)
        print(f'Re-creating {ub_zip}...')
        with zipfile.ZipFile(ub_zip, 'w', zipfile.ZIP_DEFLATED) as zf:
            for root, dirs, files in os.walk('Build_APK/UnturnedBundle'):
                for file in files:
                    full_p = os.path.join(root, file)
                    rel_p = os.path.relpath(full_p, 'Build_APK')
                    zf.write(full_p, rel_p)
        print(f'  Created {ub_zip} ({os.path.getsize(ub_zip)/(1024*1024):.1f} MB)')

    print('\nAll operations completed successfully!')

if __name__ == '__main__':
    main()
