#!/bin/bash
set -e

# Full Linux build script
# This builds the backend, stages it, and creates the DEB with services

echo "=== Building backend for Linux ==="
RUNTIME=linux-x64 npx --yes ts-node backend/scripts/build.ts

echo "=== Copying backend binaries to staging ==="
mkdir -p frontend/src-tauri/linux-staging/opt/ytmediacontroller/

# Copy backend binaries
cp backend/YTMediaControllerSrv/YTMediaControllerSrv/bin/Alpha/net9.0/linux-x64/publish/YTMediaControllerSrv frontend/src-tauri/linux-staging/opt/ytmediacontroller/
cp backend/YTMediaControllerSrv/YTMediaControllerUpdaterSrv/bin/Alpha/net9.0/linux-x64/publish/YTMediaControllerUpdaterSrv frontend/src-tauri/linux-staging/opt/ytmediacontroller/

# Copy systemd service files
cp backend/systemd/ytmediacontroller.service frontend/src-tauri/linux-staging/opt/ytmediacontroller/
cp backend/systemd/ytmediacontroller-updater.service frontend/src-tauri/linux-staging/opt/ytmediacontroller/

# Copy default config
cp backend/YTMediaControllerSrv/YTMediaControllerSrv/appSettings.json frontend/src-tauri/linux-staging/opt/ytmediacontroller/

# Make binaries executable
chmod +x frontend/src-tauri/linux-staging/opt/ytmediacontroller/YTMediaControllerSrv
chmod +x frontend/src-tauri/linux-staging/opt/ytmediacontroller/YTMediaControllerUpdaterSrv

echo "=== Building frontend and DEB ==="
cd frontend
npm run tauri build
cd ../..

echo "=== Injecting systemd services into DEB ==="
DEB_FILE=$(ls frontend/src-tauri/target/release/bundle/deb/*.deb | head -1)
bash scripts/add-deb-services.sh "$DEB_FILE"

echo "=== Done! ==="
echo "DEB file: $DEB_FILE"
