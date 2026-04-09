#!/bin/bash
# Start PostgreSQL with Docker

echo ""
echo "Starting PostgreSQL with Docker..."
echo ""

# Check if Docker daemon is running
if ! docker info >/dev/null 2>&1; then
  echo ""
  echo "[ERROR] Docker daemon is not running!"
  echo ""
  echo "Please start Docker Desktop and wait for it to load."
  echo "Once ready, run this script again."
  echo ""
  exit 1
fi

echo "[OK] Docker is running"
echo ""

# Start the database
cd backend
echo "Starting PostgreSQL container..."
docker-compose up -d

if [ $? -eq 0 ]; then
  echo ""
  echo "[SUCCESS] PostgreSQL started!"
  echo ""
  echo "It may take a few seconds to be ready for connections."
  echo "Run: npm run dev"
  echo ""
else
  echo ""
  echo "[ERROR] Failed to start PostgreSQL"
  echo ""
fi

cd ..
