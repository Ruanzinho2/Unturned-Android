import os
import sys
import argparse
import subprocess
import json
import urllib.request
import zipfile
from concurrent.futures import ThreadPoolExecutor

class NoRedirect(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, req, fp, code, msg, headers, newurl):
        return None

def get_token():
    token = os.environ.get("GH_TOKEN") or os.environ.get("GITHUB_TOKEN")
    if not token:
        try:
            token = subprocess.check_output(["gh", "auth", "token"]).decode().strip()
        except Exception as e:
            print(f"Error getting gh token: {e}", file=sys.stderr)
    return token

def get_artifact_id(run_id, artifact_name, token):
    url = f"https://api.github.com/repos/Ruanzinho2/Unturned-Android/actions/runs/{run_id}/artifacts"
    req = urllib.request.Request(url, headers={"Authorization": f"Bearer {token}", "Accept": "application/vnd.github+json"})
    with urllib.request.urlopen(req) as resp:
        data = json.loads(resp.read().decode())
    for art in data.get("artifacts", []):
        if art.get("name") == artifact_name:
            return art.get("id")
    if data.get("artifacts"):
        return data["artifacts"][0]["id"]
    raise RuntimeError(f"No artifact found for run {run_id}")

def get_final_url(artifact_id, token):
    artifact_url = f"https://api.github.com/repos/Ruanzinho2/Unturned-Android/actions/artifacts/{artifact_id}/zip"
    opener = urllib.request.build_opener(NoRedirect)
    req = urllib.request.Request(artifact_url, headers={"Authorization": f"Bearer {token}"})
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
    return idx, chunk_path

def main():
    parser = argparse.ArgumentParser(description="Fast parallel artifact downloader")
    parser.add_argument("--run-id", type=str, default="41199293748")
    parser.add_argument("--artifact-id", type=str, default=None)
    parser.add_argument("--name", type=str, default="Unturned-Android-APK")
    parser.add_argument("--output", type=str, default="Build_APK/Unturned-Android-v73.apk")
    args = parser.parse_args()

    token = get_token()
    if not token:
        print("No GitHub token available!", file=sys.stderr)
        sys.exit(1)

    artifact_id = args.artifact_id
    if not artifact_id:
        print(f"Finding artifact for run {args.run_id}...", flush=True)
        artifact_id = get_artifact_id(args.run_id, args.name, token)

    print(f"Using artifact ID: {artifact_id}", flush=True)
    print("Getting redirect URL...", flush=True)
    final_url = get_final_url(artifact_id, token)
    print("Direct blob URL obtained.", flush=True)

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

    print(f"Downloading {len(ranges)} chunks in parallel with 16 threads...", flush=True)
    with ThreadPoolExecutor(max_workers=16) as pool:
        futures = [pool.submit(download_chunk_file, final_url, idx, s, e) for idx, s, e in ranges]
        results = [f.result() for f in futures]

    output_zip = f"/tmp/artifact_{artifact_id}.zip"
    print("Concatenating chunks into zip...", flush=True)
    with open(output_zip, "wb") as out_f:
        for idx in range(len(ranges)):
            chunk_path = f"/tmp/chunk_{idx}.bin"
            with open(chunk_path, "rb") as in_f:
                while True:
                    buf = in_f.read(1024 * 1024)
                    if not buf:
                        break
                    out_f.write(buf)
            try:
                os.remove(chunk_path)
            except OSError:
                pass

    print(f"Zip created! Size: {os.path.getsize(output_zip)} bytes. Extracting APK...", flush=True)
    with zipfile.ZipFile(output_zip, "r") as z:
        print("Zip contents:", z.namelist(), flush=True)
        for name in z.namelist():
            if name.endswith(".apk"):
                print(f"Extracting {name} to {args.output}...", flush=True)
                os.makedirs(os.path.dirname(os.path.abspath(args.output)), exist_ok=True)
                with z.open(name) as src, open(args.output, "wb") as dst:
                    while True:
                        buf = src.read(1024 * 1024)
                        if not buf:
                            break
                        dst.write(buf)
                break

    try:
        os.remove(output_zip)
    except OSError:
        pass

    if os.path.exists(args.output):
        apk_size = os.path.getsize(args.output)
        print(f"SUCCESS: APK extracted to {args.output}! Size: {apk_size} bytes ({apk_size / (1024*1024):.2f} MB)", flush=True)
    else:
        print("ERROR: APK was not found in artifact zip!", file=sys.stderr)
        sys.exit(1)

if __name__ == "__main__":
    main()
