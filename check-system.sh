#!/bin/bash
# PitWall - System Check
# Validates that all prerequisites are installed and configured correctly

echo ""
echo "[PitWall] System Requirements Check"
echo "===================================="
echo ""

ERRORS=0
WARNINGS=0

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[1;34m'
NC='\033[0m'

# Check Node.js
echo "Checking: Node.js..."
if command -v node &> /dev/null; then
  NODE_VERSION=$(node --version)
  echo -e "  ${GREEN}[OK]${NC} Found: $NODE_VERSION"
else
  echo -e "  ${RED}[ERROR]${NC} Node.js not installed"
  echo "  Please install from https://nodejs.org/"
  ((ERRORS++))
fi
echo ""

# Check npm
echo "Checking: npm..."
if command -v npm &> /dev/null; then
  NPM_VERSION=$(npm --version)
  echo -e "  ${GREEN}[OK]${NC} Found: npm v$NPM_VERSION"
else
  echo -e "  ${RED}[ERROR]${NC} npm not installed"
  ((ERRORS++))
fi
echo ""

# Check Git
echo "Checking: Git..."
if command -v git &> /dev/null; then
  GIT_VERSION=$(git --version)
  echo -e "  ${GREEN}[OK]${NC} Found: $GIT_VERSION"
else
  echo -e "  ${YELLOW}[WARNING]${NC} Git not found (optional for development)"
  ((WARNINGS++))
fi
echo ""

# Check Docker
echo "Checking: Docker..."
if command -v docker &> /dev/null; then
  echo -e "  ${GREEN}[OK]${NC} Docker CLI found"
  if docker info >/dev/null 2>&1; then
    echo -e "  ${GREEN}[OK]${NC} Docker daemon is running"
  else
    echo -e "  ${YELLOW}[WARNING]${NC} Docker CLI found but daemon not running"
    echo "  [INFO] Local PostgreSQL must be running on port 5432"
    ((WARNINGS++))
  fi
else
  echo -e "  ${BLUE}[INFO]${NC} Docker not installed - using local PostgreSQL"
  echo "  Please ensure PostgreSQL is running on port 5432"
fi
echo ""

# Check project structure
echo "Checking: Project structure..."
if [ -d backend ]; then
  echo -e "  ${GREEN}[OK]${NC} backend/ directory found"
else
  echo -e "  ${RED}[ERROR]${NC} backend/ directory not found"
  ((ERRORS++))
fi

if [ -d frontend ]; then
  echo -e "  ${GREEN}[OK]${NC} frontend/ directory found"
else
  echo -e "  ${RED}[ERROR]${NC} frontend/ directory not found"
  ((ERRORS++))
fi
echo ""

# Check PostgreSQL (if not using Docker)
if ! command -v docker &> /dev/null; then
  echo "Checking: Local PostgreSQL..."
  echo "  [INFO] PostgreSQL Server must be running on localhost:5432"
  echo "  If installed locally, verify it's started:"
  echo "  - macOS: brew services start postgresql@14"
  echo "  - Linux: sudo systemctl start postgresql"
  echo ""
fi

# Summary
echo "===================================="
echo "Summary:"
if [ $ERRORS -eq 0 ]; then
  if [ $WARNINGS -eq 0 ]; then
    echo -e "${GREEN}[SUCCESS]${NC} All systems ready! Run './start-all.sh' to begin"
  else
    echo -e "${YELLOW}[WARNING]${NC} System ready with $WARNINGS warning(s). Run './start-all.sh' to begin"
  fi
else
  echo -e "${RED}[ERROR]${NC} $ERRORS error(s) found. Please fix above issues and try again."
fi
echo "===================================="
echo ""
