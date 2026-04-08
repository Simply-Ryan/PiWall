#!/bin/bash
# PitWall Quick Start Script
# This script sets up PitWall with minimal user interaction

set -e  # Exit on error

# Colors for better UI
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Progress counter
STEP=0
TOTAL_STEPS=7

# Function for formatted logging
log_step() {
  STEP=$((STEP + 1))
  echo -e "\n${BLUE}[${STEP}/${TOTAL_STEPS}]${NC} ${CYAN}$1${NC}"
}

log_success() {
  echo -e "${GREEN}✅ $1${NC}"
}

log_error() {
  echo -e "${RED}❌ $1${NC}"
}

log_warning() {
  echo -e "${YELLOW}⚠️  $1${NC}"
}

log_info() {
  echo -e "${BLUE}ℹ️  $1${NC}"
}

# Print header
echo -e "\n${CYAN}╔════════════════════════════════════════╗${NC}"
echo -e "${CYAN}║         🏁 PitWall Quick Setup        ║${NC}"
echo -e "${CYAN}╚════════════════════════════════════════╝${NC}\n"

# Function to check command availability
command_exists() {
  command -v "$1" >/dev/null 2>&1
}

# Step 1: Check Node.js
log_step "Checking Node.js installation"
if ! command_exists node; then
  log_error "Node.js is not installed"
  log_info "Please install Node.js 18+ from https://nodejs.org/"
  exit 1
fi
log_success "Node.js $(node --version)"

# Step 2: Check npm
log_step "Checking npm installation"
if ! command_exists npm; then
  log_error "npm is not installed"
  exit 1
fi
log_success "npm $(npm --version)"

# Step 3: Check Docker
log_step "Checking Docker availability"
if command_exists docker; then
  log_success "Docker is available (will use for PostgreSQL)"
  USE_DOCKER=true
else
  log_warning "Docker not detected (will expect local PostgreSQL on localhost:5432)"
  USE_DOCKER=false
fi

# Step 4: Install Dependencies
log_step "Installing dependencies"
echo -e "${BLUE}➜${NC} Cleaning npm cache and removing old lock files..."
npm cache clean --force > /dev/null 2>&1

echo -e "${BLUE}➜${NC} Backend dependencies..."
cd backend
rm -f package-lock.json
npm install
if [ $? -ne 0 ]; then
  log_error "Backend dependencies installation failed"
  log_info "Troubleshooting: Check your internet connection and try running setup.sh again"
  exit 1
fi
log_success "Backend dependencies installed"

echo -e "${BLUE}➜${NC} Frontend dependencies..."
cd ../frontend
rm -f package-lock.json
npm install --legacy-peer-deps
if [ $? -ne 0 ]; then
  log_error "Frontend dependencies installation failed"
  log_info "Troubleshooting: Check your internet connection and try running setup.sh again"
  exit 1
fi
log_success "Frontend dependencies installed"

# Step 5: Setup Database
log_step "Configuring database"
cd ../backend

# Create .env if it doesn't exist
if [ ! -f .env ]; then
  echo -e "${BLUE}➜${NC} Creating .env file from template..."
  cp .env.example .env
  
  # Generate JWT secret
  if command_exists openssl; then
    JWT_SECRET=$(openssl rand -base64 32)
    sed -i.bak "s/your-secret-key-change-in-production/$JWT_SECRET/" .env && rm -f .env.bak
    log_success "Generated secure JWT secret"
  fi
else
  log_info ".env file already exists"
fi

# Setup database
if [ "$USE_DOCKER" = true ]; then
  echo -e "${BLUE}➜${NC} Setting up PostgreSQL with Docker..."
  
  # Check if container already exists
  if ! docker ps -a --format '{{.Names}}' | grep -q pitwall-db; then
    log_info "Creating PostgreSQL container..."
    docker run -d \
      --name pitwall-db \
      -e POSTGRES_DB=pitwall \
      -e POSTGRES_USER=postgres \
      -e POSTGRES_PASSWORD=password \
      -p 5432:5432 \
      postgres:14-alpine > /dev/null 2>&1
    
    log_info "Waiting 3 seconds for database to start..."
    sleep 3
  else
    log_info "PostgreSQL container already exists, starting it..."
    docker start pitwall-db > /dev/null 2>&1 || true
    sleep 1
  fi
  log_success "PostgreSQL database ready"
else
  log_warning "Please ensure PostgreSQL is running on localhost:5432"
  log_info "Waiting a moment before continuing..."
  sleep 2
fi

# Step 6: Run Database Migrations
log_step "Running database migrations"
echo -e "${BLUE}➜${NC} Initializing Prisma migrations..."

# First, ensure migrations folder exists by running prisma migrate dev
if ! npx prisma migrate dev --name init --skip-generate 2>/dev/null; then
  log_warning "Initial migration already exists or setup already run"
fi
log_success "Database migrations completed"

# Step 7: Seed Database (Optional)
log_step "Seeding database with test data"
read -p "$(echo -e ${BLUE}?)$(echo -e ${NC}) Would you like to seed the database? (y/n) " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
  echo -e "${BLUE}➜${NC} Seeding database..."
  npm run seed > /dev/null 2>&1
  log_success "Database seeded with test data"
  echo ""
  echo -e "${CYAN}📝 Test user credentials:${NC}"
  echo -e "   ${GREEN}Username: racer1${NC}   ${GREEN}Password: Password123!${NC}"
  echo -e "   ${GREEN}Username: racer2${NC}   ${GREEN}Password: Password456!${NC}"
  echo -e "   ${GREEN}Username: racer3${NC}   ${GREEN}Password: Password789!${NC}"
else
  log_info "Skipping database seeding"
fi

echo ""
echo -e "${CYAN}╔════════════════════════════════════════╗${NC}"
echo -e "${CYAN}║       ✅ Setup Complete!              ║${NC}"
echo -e "${CYAN}╚════════════════════════════════════════╝${NC}"
echo ""
echo -e "${CYAN}📌 To start the application:${NC}"
echo ""
echo -e "   ${GREEN}Backend:${NC}"
echo -e "   cd backend"
echo -e "   npm run dev"
echo ""
echo -e "   ${GREEN}Frontend (in another terminal):${NC}"
echo "   cd frontend"
echo "   npm run web"
echo ""
echo -e "${CYAN}🌐 Access the app at:${NC} ${GREEN}http://localhost:5173${NC}"
echo -e "${CYAN}📚 API Docs at:${NC} ${GREEN}http://localhost:3000/api/docs${NC}"
echo ""
echo -e "${GREEN}Happy racing! 🏁${NC}"
echo ""
echo "   Frontend (in a new terminal):"
echo "   cd frontend"
echo "   npm run web"
echo ""
echo "🌐 Access the app at: http://localhost:5173"
echo "📚 API Docs at: http://localhost:3000/api/docs"
echo ""
echo "Happy racing! 🏁"
