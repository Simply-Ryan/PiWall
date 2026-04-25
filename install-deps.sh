#!/bin/bash
set -e

echo "Installing required system libraries for React Native DevTools..."
apt-get update
apt-get install -y \
  libatk1.0-0 \
  libatk-bridge2.0-0 \
  libgdk-pixbuf2.0-0 \
  libx11-6 \
  libxkbcommon0 \
  libxcb1 \
  libxrender1 \
  libfontconfig1

echo "✅ System libraries installed successfully!"
