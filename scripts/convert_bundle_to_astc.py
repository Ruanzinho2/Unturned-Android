#!/usr/bin/env python3
"""
Convert PC Unturned AssetBundle textures to Android ASTC hardware compressed textures.
Target: Adreno / Mali GPUs (TargetPlatform = 13 / Android).
"""
import os
import sys
import time
import UnityPy
from PIL import Image
from UnityPy.enums import TextureFormat, BuildTarget

def main():
    src_bundle = 'Bundles_Unturned/Bundles/core.masterbundle'
    dst_bundle = 'Build_APK/core.masterbundle'
    os.makedirs('Build_APK', exist_ok=True)

    print(f'Loading source bundle: {src_bundle}...')
    t_start = time.time()
    env = UnityPy.load(src_bundle)
    bf = list(env.files.values())[0]

    # Collect Texture2D objects
    tex_objs = [obj for obj in env.objects if obj.type.name == 'Texture2D']
    total_tex = len(tex_objs)
    print(f'Found {total_tex} Texture2D objects to process.')

    converted = 0
    skipped = 0
    errors = 0

    t_convert_start = time.time()
    for i, obj in enumerate(tex_objs, 1):
        try:
            data = obj.read()
            w, h = data.m_Width, data.m_Height
            fmt = data.m_TextureFormat

            # If already ASTC/ETC2, skip
            if fmt in (47, 48, 50, 52):
                skipped += 1
                continue

            # Small textures (<4x4) don't need ASTC block compression
            if w < 4 or h < 4:
                skipped += 1
                continue

            img = data.image
            # Pick format based on alpha
            if 'A' in img.mode:
                target_fmt = TextureFormat.ASTC_RGBA_4x4
            else:
                target_fmt = TextureFormat.ASTC_RGB_4x4

            data.set_image(img, target_format=target_fmt)
            data.save()
            converted += 1

            if i % 100 == 0 or i == total_tex:
                elapsed = time.time() - t_convert_start
                rate = converted / elapsed if elapsed > 0 else 0
                print(f'Progress: {i}/{total_tex} ({converted} converted, {skipped} skipped, {errors} errors) - {rate:.1f} tex/s')

        except Exception as e:
            errors += 1
            if errors <= 10:
                print(f'Warning: failed converting texture {i}: {e}')

    print(f'\nFinished texture conversion: {converted} converted, {skipped} skipped, {errors} errors in {time.time() - t_convert_start:.1f}s.')

    # Set target platform to Android (13)
    for name, sf in bf.files.items():
        if hasattr(sf, 'target_platform'):
            sf.target_platform = BuildTarget.Android
            print(f'Set {name} target_platform to Android (13)')

    print(f'Saving Android ASTC bundle with LZ4 compression to {dst_bundle}...')
    t_save = time.time()
    bundle_bytes = env.file.save(packer='lz4')

    with open(dst_bundle, 'wb') as f:
        f.write(bundle_bytes)

    out_size_mb = os.path.getsize(dst_bundle) / (1024 * 1024)
    print(f'Successfully wrote {dst_bundle}: {out_size_mb:.2f} MB in {time.time() - t_save:.1f}s.')
    print(f'Total process completed in {time.time() - t_start:.1f}s.')

if __name__ == '__main__':
    main()
