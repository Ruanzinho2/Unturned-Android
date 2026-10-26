#!/usr/bin/env python3
"""
Port Russia map bundles and structure for Unturned Android.
- Converts map textures (Roads, Details) to ASTC.
- Ensures all map AssetBundles have target_platform set to Android (13).
- Ensures legacy Materials.unity3d hash matches Unturned's SHA1 table (b07ce5263db5eadef84f2b14d809dffc6680d003).
- Outputs to Build_APK/Maps/Russia and generates Build_APK/Russia.zip.
- Syncs to Build_APK/UnturnedBundle/Maps/Russia.
"""
import os
import sys
import shutil
import hashlib
import zipfile
import time
import UnityPy
from PIL import Image
from UnityPy.enums import TextureFormat, BuildTarget

def convert_bundle_textures_astc(src_path, dst_path):
    print(f'Converting {src_path} -> {dst_path}...', flush=True)
    t0 = time.time()
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
                    print(f'    Converted texture: {data.m_Name} ({w}x{h}, {img.mode}) -> {fmt.name}', flush=True)
                except Exception as e:
                    print(f'    Warning: {data.m_Name} conversion failed: {e}', flush=True)

    for name, sf in bf.files.items():
        if hasattr(sf, '_m_target_platform'):
            sf._m_target_platform = 13
        if hasattr(sf, 'target_platform'):
            sf.target_platform = BuildTarget.Android

    os.makedirs(os.path.dirname(dst_path), exist_ok=True)
    out_bytes = env.file.save(packer='lz4')
    with open(dst_path, 'wb') as f:
        f.write(out_bytes)
    dt = time.time() - t0
    print(f'  Saved {dst_path} ({len(out_bytes)/1024:.1f} KB, {converted} textures converted to ASTC in {dt:.1f}s)', flush=True)

def ensure_bundle_android(file_path):
    print(f'Ensuring Android target platform for {file_path}...', flush=True)
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
        print(f'  Updated target_platform to Android (13) for {file_path}', flush=True)
    else:
        print(f'  Already Android (13): {file_path}', flush=True)

def zip_folder_contents(src_dir, zip_path):
    print(f'Creating zip {zip_path} from {src_dir}...', flush=True)
    t0 = time.time()
    with zipfile.ZipFile(zip_path, 'w', zipfile.ZIP_DEFLATED) as zf:
        for root, dirs, files in os.walk(src_dir):
            for file in files:
                full_path = os.path.join(root, file)
                rel_path = os.path.relpath(full_path, src_dir)
                zf.write(full_path, rel_path)
    dt = time.time() - t0
    sz_mb = os.path.getsize(zip_path) / (1024 * 1024)
    print(f'  Created {zip_path} ({sz_mb:.1f} MB in {dt:.1f}s)', flush=True)

def main():
    src_map = 'Bundles_Unturned/Maps/Russia'
    dst_map = 'Build_APK/Maps/Russia'
    rus_map = 'com.SmartlyDressedGames.Unturned/com.SmartlyDressedGames.Unturned/files/UnturnedBundle/Maps/Russia'

    if not os.path.exists(src_map):
        print(f'Error: Source map not found at {src_map}!')
        sys.exit(1)

    print('=== Step 1: Clean and copy Russia from source ===', flush=True)
    if os.path.exists(dst_map):
        print(f'Removing old {dst_map}...', flush=True)
        shutil.rmtree(dst_map)
    print(f'Copying {src_map} to {dst_map}...', flush=True)
    shutil.copytree(src_map, dst_map)
    print('  Copied map structure.', flush=True)

    print('\n=== Step 2: Convert bundles for Android ===', flush=True)
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

    # 4. Copy Materials.unity3d with exact SHA1 hash (b07ce5263db5eadef84f2b14d809dffc6680d003)
    mat_rus = os.path.join(rus_map, 'Terrain/Materials.unity3d')
    mat_dst = os.path.join(dst_map, 'Terrain/Materials.unity3d')
    if os.path.exists(mat_rus):
        shutil.copyfile(mat_rus, mat_dst)
        with open(mat_dst, 'rb') as f:
            sha1 = hashlib.sha1(f.read()).hexdigest()
        expected_sha1 = 'b07ce5263db5eadef84f2b14d809dffc6680d003'
        print(f'  Materials.unity3d SHA1: {sha1} (matches expected Russia hash: {sha1 == expected_sha1})', flush=True)
        assert sha1 == expected_sha1, "Materials.unity3d SHA1 mismatch!"
    else:
        print(f'  Warning: {mat_rus} not found, verifying existing Materials.unity3d...', flush=True)
        ensure_bundle_android(mat_dst)

    print('\n=== Step 3: Verification of all bundles in destination ===', flush=True)
    for root, dirs, files in os.walk(dst_map):
        for f in sorted(files):
            if f.endswith('.unity3d'):
                p = os.path.join(root, f)
                env = UnityPy.load(p)
                for bf in env.files.values():
                    for name, sf in getattr(bf, 'files', {}).items():
                        tp = getattr(sf, 'target_platform', None)
                        print(f'  Verified {f}: {name} -> target_platform: {tp}', flush=True)

    print('\n=== Step 4: Generating Build_APK/Russia.zip ===', flush=True)
    russia_zip = 'Build_APK/Russia.zip'
    if os.path.exists(russia_zip):
        os.remove(russia_zip)
    zip_folder_contents(dst_map, russia_zip)

    print('\n=== Step 5: Syncing to UnturnedBundle ===', flush=True)
    bundle_map_dst = 'Build_APK/UnturnedBundle/Maps/Russia'
    if os.path.exists('Build_APK/UnturnedBundle'):
        if os.path.exists(bundle_map_dst):
            shutil.rmtree(bundle_map_dst)
        shutil.copytree(dst_map, bundle_map_dst)
        print(f'  Copied fresh Russia to {bundle_map_dst}', flush=True)

    print('\nAll operations completed successfully!', flush=True)

if __name__ == '__main__':
    main()
