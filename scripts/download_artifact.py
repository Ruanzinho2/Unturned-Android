import os
import sys
import urllib.request
import zipfile
from concurrent.futures import ThreadPoolExecutor

TOKEN = os.environ.get("GH_TOKEN", "")
ARTIFACT_URL = "https://api.github.com/repos/Ruanzinho2/Unturned-Android/actions/artifacts/12696733346/zip"
OUTPUT_ZIP = "/tmp/v72_artifact.zip"
TARGET_APK = "Build_APK/Unturned-Android-v72.apk"

class NoRedirect(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, req, fp, code, msg, headers, newurl):
        return None

def get_final_url():
    opener = urllib.request.build_opener(NoRedirect)
    req = urllib.request.Request(ARTIFACT_URL, headers={"Authorization": f"Bearer {TOKEN}"})
    try:
        resp = opener.open(req)
        return resp.geturl()
    except urllib.error.HTTPError as e:
        if e.code in (301, 302, 303, 307):
            return e.headers["Location"]
        raise

def download_chunk_file(url, idx, start, end):
    chunk_path = f"/tmp/chunk_{idx}.bin"
    req = urllib.request.Request(url, headers={"Range": f"bytes={start}-{end}"})
    with urllib.request.urlopen(req, timeout=60) as resp, open(chunk_path, "wb") as f:
        while True:
            buf = resp.read(64 * 1024)
            if not buf:
                break
            f.write(buf)
    size = os.path.getsize(chunk_path)
    print(f"Chunk {idx} done: {size} bytes ({start}-{end})", flush=True)
    return idx, chunk_path

def main():
    print("Getting redirect URL...", flush=True)
    final_url = get_final_url()
    print("Blob URL obtained.", flush=True)

    head_req = urllib.request.Request(final_url, method="HEAD")
    with urllib.request.urlopen(head_req, timeout=30) as resp:
        total_size = int(resp.headers["Content-Length"])
    print(f"Total size: {total_size} bytes ({total_size / (1024*1024):.2f} MB)", flush=True)

    num_chunks = 16
    chunk_size = (total_size + num_chunks - 1) // num_chunks
    ranges = []
    for i in range(num_chunks):
        s = i * chunk_size
        e = min(s + chunk_size - 1, total_size - 1)
        if s <= e:
            ranges.append((i, s, e))

    print(f"Downloading {len(ranges)} chunks in parallel...", flush=True)
    with ThreadPoolExecutor(max_workers=16) as pool:
        futures = [pool.submit(download_chunk_file, final_url, idx, s, e) for idx, s, e in ranges]
        results = [f.result() for f in futures]

    print("Concatenating chunks into zip...", flush=True)
    with open(OUTPUT_ZIP, "wb") as out_f:
        for idx in range(len(ranges)):
            chunk_path = f"/tmp/chunk_{idx}.bin"
            with open(chunk_path, "rb") as in_f:
                while True:
                    buf = in_f.read(1024 * 1024)
                    if not buf:
                        break
                    out_f.write(buf)
            os.remove(chunk_path)

    print(f"Zip created! Size: {os.path.getsize(OUTPUT_ZIP)} bytes. Testing...", flush=True)
    with zipfile.ZipFile(OUTPUT_ZIP, "r") as z:
        print("Zip contents:", z.namelist(), flush=True)
        for name in z.namelist():
            if name.endswith(".apk"):
                print(f"Extracting {name} to {TARGET_APK}...", flush=True)
                os.makedirs(os.path.dirname(TARGET_APK), exist_ok=True)
                with z.open(name) as src, open(TARGET_APK, "wb") as dst:
                    while True:
                        buf = src.read(1024 * 1024)
                        if not buf:
                            break
                        dst.write(buf)
                break

    apk_size = os.path.getsize(TARGET_APK)
    print(f"APK extracted successfully! Size: {apk_size} bytes ({apk_size / (1024*1024):.2f} MB)", flush=True)

if __name__ == "__main__":
    main()
