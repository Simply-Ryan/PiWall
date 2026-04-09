@echo off
REM PitWall - Start All Services (Windows)
REM Automatically detects and uses Docker or local PostgreSQL

setlocal enabledelayedexpansion

echo.
echo [PitWall] Starting All Services
echo ===============================
echo.

REM --- CHECK NODE.JS ---
where node >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
  echo [ERROR] Node.js not found!
  echo Please install from https://nodejs.org/
  pause
  exit /b 1
)

for /f "tokens=*" %%i in ('node --version') do set NODE_VERSION=%%i
echo [OK] Node.js %NODE_VERSION%
echo.

REM --- CHECK DATABASE ---
echo [STEP 1] Checking database...
set USE_DOCKER=false

where docker >nul 2>nul
if %ERRORLEVEL% EQU 0 (
    docker info >nul 2>nul
    if %ERRORLEVEL% EQU 0 (
        echo [OK] Docker detected and running
        set USE_DOCKER=true
        
        REM Start PostgreSQL container
        echo [STEP 1.1] Starting PostgreSQL container...
        cd backend
        if exist docker-compose.yml (
            docker-compose up -d
            cd ..
            timeout /t 3 /nobreak
            echo [OK] PostgreSQL started on port 5432
        ) else (
            cd ..
        )
    ) else (
        echo [WARNING] Docker CLI found but daemon not running
        echo [INFO] Attempting to use local PostgreSQL on port 5432...
    )
) else (
    echo [INFO] Docker not installed
    echo [INFO] Please ensure PostgreSQL is running on port 5432
)
echo.

REM --- INSTALL DEPENDENCIES ---
echo [STEP 2] Installing dependencies...

if not exist "backend\node_modules" (
  echo [2.1] Backend dependencies...
  cd backend
  call npm install --legacy-peer-deps 2>nul
  if !ERRORLEVEL! NEQ 0 (
    echo [WARNING] npm install had issues, continuing anyway...
  )
  cd ..
)
echo [OK] Backend ready
echo.

if not exist "frontend\node_modules" (
  echo [2.2] Frontend dependencies...
  cd frontend
  call npm install --legacy-peer-deps 2>nul
  if !ERRORLEVEL! NEQ 0 (
    echo [WARNING] npm install had issues, continuing anyway...
  )
  cd ..
)
echo [OK] Frontend ready
echo.

REM --- DATABASE SETUP ---
echo [STEP 3] Setting up database...
cd backend
call npx prisma db push --skip-generate 2>nul
if %ERRORLEVEL% EQU 0 (
    echo [OK] Database schema synchronized
) else (
    echo [WARNING] Database sync may have failed - PostgreSQL must be running
)
cd ..
echo.

REM --- START SERVICES ---
echo [STEP 4] Starting services...
echo.
echo Starting Backend on port 3000...
start "PitWall Backend" cmd /k "cd backend && npm run dev"

timeout /t 2 /nobreak

echo Starting Frontend on port 5173...
start "PitWall Frontend" cmd /k "cd frontend && npm run web"

echo.
echo ==========================================
echo [SUCCESS] Services starting!
echo ==========================================
echo.
echo If browsers don't auto-open:
echo   - Backend:  http://localhost:3000
echo   - Frontend: http://localhost:5173
echo.
echo To stop: Close the terminal windows or press Ctrl+C
echo ==========================================
echo.
pause
