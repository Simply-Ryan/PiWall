@echo off
REM PitWall - Start All Services (Windows)
REM This batch file starts PostgreSQL, Backend, and Frontend

setlocal enabledelayedexpansion

echo.
echo 🏁 PitWall - Starting All Services
echo ==================================
echo.

REM Check if Docker is installed
docker --version >nul 2>&1
if errorlevel 1 (
  echo ERROR: Docker is not installed or not in PATH
  echo Please install Docker Desktop from https://www.docker.com/
  pause
  exit /b 1
)

REM Start PostgreSQL
echo [1/3] Starting PostgreSQL (Docker)...
cd backend
docker-compose up -d
cd ..
echo.
echo ✓ PostgreSQL started on port 5432
echo.

timeout /t 3 /nobreak

REM Install dependencies if needed
if not exist "backend\node_modules" (
  echo [2/3] Installing Backend dependencies...
  cd backend
  call npm install
  cd ..
  echo ✓ Backend dependencies installed
  echo.
)

if not exist "frontend\node_modules" (
  echo [2/3] Installing Frontend dependencies...
  cd frontend
  call npm install
  cd ..
  echo ✓ Frontend dependencies installed
  echo.
)

REM Start Backend in a new terminal
echo Starting Backend on port 3000...
start cmd /k "cd backend && npm run dev"

REM Wait for backend to start
timeout /t 3 /nobreak

REM Start Frontend in a new terminal
echo Starting Frontend on port 5173...
start cmd /k "cd frontend && npm run web"

echo.
echo ========================================
echo ✓ All services started!
echo.
echo 📦 Backend: http://localhost:3000
echo 📱 Frontend: http://localhost:5173
echo.
echo Close this window when done.
echo ========================================
pause
