#!/bin/bash

# PitWall Backend Startup Script for GitHub Codespaces
# This script starts the backend API and database

set -e

echo ""
echo "🚀 PitWall Backend Startup"
echo "═══════════════════════════════════"
echo ""

# Check if Docker is running
echo "1️⃣  Checking Docker..."
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker daemon is not running"
    echo "   Please ensure Docker is started in your Codespaces"
    exit 1
fi
echo "✓ Docker is running"

# Navigate to backend directory
cd /workspaces/PitWall/backend

# Check if docker-compose.yml exists
if [ ! -f "docker-compose.yml" ]; then
    echo "❌ docker-compose.yml not found"
    exit 1
fi

echo ""
echo "2️⃣  Starting containers..."
echo "   This may take 1-2 minutes on first run"
echo ""

# Start Docker containers
docker compose up --build

echo ""
echo "✅ Backend is running!"
echo ""
echo "3️⃣  Next steps:"
echo "   • Go to the frontend: https://urban-cod-pjr7xjxwxrx3r5jw-5173.app.github.dev/"
echo "   • Check if 'Backend Connection' shows '✓ Connected!'"
echo "   • App is now ready to use!"
echo ""
