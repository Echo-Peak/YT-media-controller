#!/bin/bash
set -e

# Script to add systemd service installation to DEB package
# Run this after building the DEB

if [ -z "$1" ]; then
    echo "Usage: $0 <path-to-deb-file>"
    exit 1
fi

# Handle glob pattern
DEB_PATTERN="$1"
DEB_FILE=$(ls $DEB_PATTERN 2>/dev/null | head -1)

if [ -z "$DEB_FILE" ] || [ ! -f "$DEB_FILE" ]; then
    echo "Error: No DEB file found matching: $DEB_PATTERN"
    exit 1
fi

echo "Processing DEB: $DEB_FILE"
EXTRACT_DIR="$TEMP_DIR/extracted"
REBUILD_DIR="$TEMP_DIR/rebuild"

echo "Processing DEB: $DEB_FILE"

# Extract the DEB
mkdir -p "$EXTRACT_DIR"
cd "$EXTRACT_DIR"
ar x "$DEB_FILE"

# Extract control.tar.xz
mkdir -p control
tar -xf control.tar.xz -C control/

# Add postinst script
cat > control/postinst << 'POSTINST_EOF'
#!/bin/bash
set -e

# Install systemd services
if [ "$1" = "configure" ]; then
    # Copy service files to /opt
    cp /opt/ytmediacontroller/ytmediacontroller.service /etc/systemd/system/ 2>/dev/null || true
    cp /opt/ytmediacontroller/ytmediacontroller-updater.service /etc/systemd/system/ 2>/dev/null || true
    
    # Reload systemd and enable services
    if command -v systemctl >/dev/null 2>&1; then
        systemctl daemon-reload 2>/dev/null || true
        systemctl enable ytmediacontroller.service 2>/dev/null || true
        systemctl start ytmediacontroller.service 2>/dev/null || true
        
        systemctl enable ytmediacontroller-updater.service 2>/dev/null || true
        systemctl start ytmediacontroller-updater.service 2>/dev/null || true
    fi
    
    echo "YT Media Controller services installed and started"
fi
POSTINST_EOF
chmod 755 control/postinst

# Add prerm script
cat > control/prerm << 'PRERM_EOF'
#!/bin/bash
set -e

if [ "$1" = "remove" ] || [ "$1" = "purge" ]; then
    # Stop and disable services
    if command -v systemctl >/dev/null 2>&1; then
        systemctl stop ytmediacontroller-updater.service 2>/dev/null || true
        systemctl stop ytmediacontroller.service 2>/dev/null || true
        systemctl disable ytmediacontroller-updater.service 2>/dev/null || true
        systemctl disable ytmediacontroller.service 2>/dev/null || true
        
        rm -f /etc/systemd/system/ytmediacontroller.service 2>/dev/null || true
        rm -f /etc/systemd/system/ytmediacontroller-updater.service 2>/dev/null || true
        systemctl daemon-reload 2>/dev/null || true
    fi
    
    echo "YT Media Controller services removed"
fi
PRERM_EOF
chmod 755 control/prerm

# Rebuild control.tar.xz
rm -f control.tar.xz
tar -cf control.tar.xz -C control/ .

# Rebuild the DEB
cd "$TEMP_DIR"
mkdir -p "$REBUILD_DIR"
cp debian-binary "$REBUILD_DIR/"
cp control.tar.xz "$REBUILD_DIR/"
cp data.tar.xz "$REBUILD_DIR/"

# Determine new DEB filename
DIRNAME=$(dirname "$DEB_FILE")
BASENAME=$(basename "$DEB_FILE" .deb)
NEW_DEB="${DIRNAME}/${BASENAME}-with-services.deb"

cd "$REBUILD_DIR"
ar rcs "$NEW_DEB" debian_binary control.tar.xz data.tar.xz

# Copy back to original location
cp "$NEW_DEB" "$DEB_FILE"

# Cleanup
rm -rf "$TEMP_DIR"

echo "Done! Updated DEB: $DEB_FILE"
echo "The package will now automatically install and start systemd services on install."
