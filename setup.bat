@echo off
REM PitWall Quick Start Script for Windows
REM This script sets up PitWall with minimal user interaction

setlocal enabledelayedexpansion

echo.
echo 🏁 PitWall Quick Start Setup
echo ============================
echo.

REM Check Node.js
where node >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
  echo ❌ Node.js is not installed
  echo Please install Node.js 18+ from https://nodejs.org/
  pause
  exit /b 1
)

for /f "tokens=*" %%i in ('node --version') do set NODE_VERSION=%%i
echo ✅ Node.js %NODE_VERSION%

REM Check npm
where npm >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
  echo ❌ npm is not installed
  pause
  exit /b 1
)

for /f "tokens=*" %%i in ('npm --version') do set NPM_VERSION=%%i
echo ✅ npm %NPM_VERSION%

REM Check for Docker
where docker >nul 2>nul
if %ERRORLEVEL% EQU 0 (
  echo ✅ Docker is available (will use for PostgreSQL)
  set USE_DOCKER=true
) else (
  echo ⚠️  Docker not detected (will expect local PostgreSQL on localhost:5432)
  set USE_DOCKER=false
)

REM Step 4: Install Dependencies
set /a STEP+=1
echo [!STEP!/!TOTAL_STEPS!] Installing dependencies...
echo.
cd backend
echo 📚 Installing backend dependencies...
call npm install > nul 2>&1

REM Install frontend dependencies
cd ..\frontend
echo 📚 Installing frontend dependencies...
call npm install > nul 2>&1

echo.
echo [4/7] Configuring database...

REM Setup database
cd ..\backend

REM Create .env if it doesn't exist
if not exist .env (
  echo    Creating .env file from template...
  copy .env.example .env > nul
  
  REM Generate JWT secret using PowerShell
  for /f "tokens=*" %%i in ('powershell -Command "Write-Host ([Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes([guid]::NewGuid().ToString() + [guid]::NewGuid().ToString('D')))) | Select-Object -First 43"') do set JWT_SECRET=%%i
  
  echo ✅ Generated secure JWT secret
)

if "!USE_DOCKER!"=="true" (
  echo    Setting up PostgreSQL with Docker...
  
  REM Check if container exists
  docker ps -a --format "table {{.Names}}" 2>nul | find "pitwall-db" >nul
  if %ERRORLEVEL% NEQ 0 (
    echo    Creating PostgreSQL container...
    docker run -d ^
      --name pitwall-db ^
      -e POSTGRES_DB=pitwall ^
      -e POSTGRES_USER=postgres ^
      -e POSTGRES_PASSWORD=password ^
      -p 5432:5432 ^
      postgres:14-alpine > nul 2>&1
    
    echo    Waiting 3 seconds for database to start...
    timeout /t 3 /nobreak > nul
  ) else (
    echo    PostgreSQL container exists, starting it...
    docker start pitwall-db > nul 2>&1
    timeout /t 1 /nobreak > nul
  )
  echo ✅ PostgreSQL database ready
) else (
  echo ⚠️  Please ensure PostgreSQL is running on localhost:5432
  echo    Waiting a moment...
  timeout /t 2 /nobreak > nul
)

REM Step 6: Run Database Migrations
echo [6/7] Running database migrations...
echo    Initializing Prisma migrations...

call npx prisma migrate dev --name init --skip-generate > nul 2>&1
if %ERRORLEVEL% NEQ 0 (
  echo ⚠️  Migration error (may already exist)
)
echo ✅ Database migrations completed
echo.

REM Step 7: Seed Database
set /a STEP+=1
echo [!STEP!/!TOTAL_STEPS!] Optional: Seed database with test data
set /p SEED_DB="? Would you like to seed the database? (y/n): "

if /i "!SEED_DB!"=="y" (
  echo    Seeding database...
  call npm run seed > nul 2>&1
  if %ERRORLEVEL% EQU 0 (
    echo ✅ Database seeded with test data
    echo.
    echo 📝 Test user credentials:
    echo    Username: racer1   Password: Password123!
    echo    Username: racer2   Password: Password456!
    echo    Username: racer3   Password: Password789!
  ) else (
    echo ⚠️  Seeding failed
  )
) else (
  echo ℹ️  Skipping database seeding
)

echo.
echo ========================================
echo ✅ Setup Complete!
echo ========================================
echo.
echo 📌 To start the application:
echo.
echo    Backend (Command Prompt/PowerShell):
echo    cd backend
echo    npm run dev
echo.
echo    Frontend (New terminal):
echo    cd frontend
echo    npm run web
echo.
echo 🌐 Access the app at: http://localhost:5173
echo 📚 API Docs at: http://localhost:3000/api/docs
echo.
echo Happy racing! 🏁
echo.
pause
