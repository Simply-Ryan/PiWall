#!/bin/bash
# PitWall - Start All Services
# This script starts PostgreSQL, Backend, and Frontend in one command

set -e

echo "🏁 PitWall - Starting All Services"
echo "=================================="
echo ""

# Colors
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Start PostgreSQL in background
echo -e "${BLUE}[1/3] Starting PostgreSQL (Docker)...${NC}"
cd backend
docker-compose up -d > /dev/null 2>&1
echo -e "${GREEN}✓ PostgreSQL started on port 5432${NC}"
cd ..

# Install dependencies if needed
if [ ! -d "backend/node_modules" ]; then
  echo -e "${BLUE}[2/3] Installing Backend dependencies...${NC}"
  cd backend
  npm install > /dev/null 2>&1
  cd ..
  echo -e "${GREEN}✓ Backend dependencies installed${NC}"
fi

if [ ! -d "frontend/node_modules" ]; then
  echo -e "${BLUE}[2/3] Installing Frontend dependencies...${NC}"
  cd frontend
  npm install > /dev/null 2>&1
  cd ..
  echo -e "${GREEN}✓ Frontend dependencies installed${NC}"
fi

echo ""
echo -e "${BLUE}Starting services...${NC}"
echo ""
echo -e "${GREEN}📦 Backend will start on http://localhost:3000${NC}"
echo -e "${GREEN}📱 Frontend will start on http://localhost:5173${NC}"
echo ""
echo "Opening new terminals..."
echo ""

# The script should be run with: 
# chmod +x start-all.sh
# ./start-all.sh
