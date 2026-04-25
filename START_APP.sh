#!/bin/bash

# PitWall Full Stack Startup Script
# This script starts both backend and frontend services

echo "======================================"
echo "PitWall Full Stack Application"
echo "======================================"
echo ""

# Check if backend is already running
echo "Checking backend status..."
cd /workspaces/PitWall/backend

# Start backend in background if not running
if ! docker ps | grep -q pitwall-api; then
    echo "Starting backend services (API + PostgreSQL)..."
    docker compose up --build -d
    echo "Backend services started in background"
    echo "Waiting for services to initialize..."
    sleep 10
else
    echo "Backend already running"
fi

# Start frontend
echo ""
echo "Starting frontend application..."
cd /workspaces/PitWall/frontend

if [ ! -d "node_modules" ]; then
    echo "Installing frontend dependencies..."
    npm install
fi

echo ""
echo "======================================"
echo "Frontend will start on:"
echo "  🌐 http://localhost:5173 (or similar port)"
echo ""
echo "Backend API running on:"
echo "  🔌 http://localhost:3000"
echo ""
echo "Database (PostgreSQL) on:"
echo "  🗄️ localhost:5432"
echo "======================================"
echo ""

npm run web
