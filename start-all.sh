#!/bin/bash
# PitWall - Start All Services (macOS/Linux)
# Automatically detects and uses Docker or local PostgreSQL

set -e

echo ""
echo "[PitWall] Starting All Services"
echo "==============================="
echo ""

# Colors
GREEN='\033[0;32m'
BLUE='\033[1;34m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Check Node.js
if ! command -v node &> /dev/null; then
  echo -e "${YELLOW}[ERROR] Node.js not found!${NC}"
  echo "Please install from https://nodejs.org/"
  exit 1
fi

NODE_VERSION=$(node --version)
echo -e "${GREEN}[OK] Node.js $NODE_VERSION${NC}"
echo ""

# Check database
echo -e "${BLUE}[STEP 1] Checking database...${NC}"
USE_DOCKER=false

if command -v docker &> /dev/null; then
  if docker info >/dev/null 2>&1; then
    echo -e "${GREEN}[OK] Docker detected and running${NC}"
    USE_DOCKER=true
    
    # Start PostgreSQL container
    echo -e "${BLUE}[STEP 1.1] Starting PostgreSQL container...${NC}"
    cd backend
    if [ -f docker-compose.yml ]; then
      docker-compose up -d >/dev/null 2>&1
      cd ..
      sleep 2
      echo -e "${GREEN}[OK] PostgreSQL started on port 5432${NC}"
    else
      cd ..
    fi
  else
    echo -e "${YELLOW}[WARNING] Docker CLI found but daemon not running${NC}"
    echo "[INFO] Attempting to use local PostgreSQL on port 5432..."
  fi
else
  echo -e "${YELLOW}[INFO] Docker not installed${NC}"
  echo "[INFO] Please ensure PostgreSQL is running on port 5432"
fi
echo ""

# Install dependencies
echo -e "${BLUE}[STEP 2] Installing dependencies...${NC}"

if [ ! -d "backend/node_modules" ]; then
  echo "[2.1] Backend dependencies..."
  cd backend
  npm install --legacy-peer-deps 2>/dev/null || echo "[WARNING] npm install had issues"
  cd ..
fi
echo -e "${GREEN}[OK] Backend ready${NC}"
echo ""

if [ ! -d "frontend/node_modules" ]; then
  echo "[2.2] Frontend dependencies..."
  cd frontend
  npm install --legacy-peer-deps 2>/dev/null || echo "[WARNING] npm install had issues"
  cd ..
fi
echo -e "${GREEN}[OK] Frontend ready${NC}"
echo ""

# Database setup
echo -e "${BLUE}[STEP 3] Setting up database...${NC}"
cd backend
if npx prisma db push --skip-generate 2>/dev/null; then
  echo -e "${GREEN}[OK] Database schema synchronized${NC}"
else
  echo -e "${YELLOW}[WARNING] Database sync may have failed - PostgreSQL must be running${NC}"
fi
cd ..
echo ""

# Start services
echo -e "${BLUE}[STEP 4] Starting services...${NC}"
echo ""
echo "Starting Backend on port 3000..."

# Terminal 1: Backend
if command -v gnome-terminal &> /dev/null; then
  gnome-terminal -- bash -c "cd backend && npm run dev; exec bash"
elif command -v xterm &> /dev/null; then
  xterm -e "cd backend && npm run dev" &
elif command -v open &> /dev/null; then
  # macOS
  open -a Terminal "$(pwd)/backend" 2>/dev/null || (cd backend && npm run dev &)
else
  # Fallback
  (cd backend && npm run dev &)
fi

sleep 2

echo "Starting Frontend on port 5173..."

# Terminal 2: Frontend
if command -v gnome-terminal &> /dev/null; then
  gnome-terminal -- bash -c "cd frontend && npm run web; exec bash"
elif command -v xterm &> /dev/null; then
  xterm -e "cd frontend && npm run web" &
elif command -v open &> /dev/null; then
  # macOS
  open -a Terminal "$(pwd)/frontend" 2>/dev/null || (cd frontend && npm run web &)
else
  # Fallback
  (cd frontend && npm run web &)
fi

echo ""
echo "=========================================="
echo -e "${GREEN}[SUCCESS] Services starting!${NC}"
echo "=========================================="
echo ""
echo "If browsers don't auto-open:"
echo "  - Backend:  http://localhost:3000"
echo "  - Frontend: http://localhost:5173"
echo ""
echo "To stop services: Press Ctrl+C or close the terminal windows"
echo "=========================================="
echo ""
