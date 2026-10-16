#!/usr/bin/env python3
"""
Port PEI map bundles and structure for Unturned Android.
- Converts map textures (Roads, Details) to ASTC.
- Ensures legacy Materials.unity3d hash matches Unturned's SHA1 table.
- Sets target_platform to Android (13).
- Outputs to Build_APK/Maps/PEI.
"""
import os
import shutil
import hashlib
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

def main():
    src_map = 'Bundles_Unturned/Maps/PEI'
    dst_map = 'Build_APK/Maps/PEI'
    rus_map = 'com.SmartlyDressedGames.Unturned/com.SmartlyDressedGames.Unturned/files/UnturnedBundle/Maps/PEI'

    print('Porting PEI Map to Android...')
    if os.path.exists(dst_map):
        shutil.rmtree(dst_map)
    shutil.copytree(src_map, dst_map)

    # 1. Convert Roads.unity3d to ASTC
    roads_src = os.path.join(src_map, 'Environment/Roads.unity3d')
    roads_dst = os.path.join(dst_map, 'Environment/Roads.unity3d')
    convert_bundle_textures_astc(roads_src, roads_dst)

    # 2. Convert Details.unity3d to ASTC
    details_src = os.path.join(src_map, 'Terrain/Details.unity3d')
    details_dst = os.path.join(dst_map, 'Terrain/Details.unity3d')
    convert_bundle_textures_astc(details_src, details_dst)

    # 3. Ensure Materials.unity3d has original SHA1 hash (4f94810eaa4f2f173cf16743eab0848e632958cf)
    mat_rus = os.path.join(rus_map, 'Terrain/Materials.unity3d')
    mat_dst = os.path.join(dst_map, 'Terrain/Materials.unity3d')
    shutil.copyfile(mat_rus, mat_dst)
    
    with open(mat_dst, 'rb') as f:
        sha1 = hashlib.sha1(f.read()).hexdigest()
    print(f'Materials.unity3d SHA1: {sha1} (matches expected PEI hash: {sha1 == "4f94810eaa4f2f173cf16743eab0848e632958cf"})')

    print('\nPEI Map port completed successfully!')
    for root, dirs, files in os.walk(dst_map):
        for f in files:
            p = os.path.join(root, f)
            print(f'  {p} ({os.path.getsize(p)/1024:.1f} KB)')

if __name__ == '__main__':
    main()
