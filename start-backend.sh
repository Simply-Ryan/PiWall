#!/bin/bash

# Start PitWall Backend with Docker Compose

echo "🚀 Starting PitWall Backend..."
echo ""

cd /workspaces/PitWall/backend

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker daemon is not running. Please start Docker and try again."
    exit 1
fi

echo "✓ Docker is running"
echo "📦 Building and starting containers..."
echo ""

# Start containers
docker compose up --build

echo ""
echo "✅ Backend started!"
echo "   - Database: PostgreSQL on localhost:5432"
echo "   - API: http://localhost:3000"
echo "   - Health: curl http://localhost:3000/health"
