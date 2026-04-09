@echo off
REM Start PostgreSQL with Docker

echo.
echo Starting PostgreSQL with Docker...
echo.

REM Check if Docker daemon is running
docker info >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
  echo.
  echo [ERROR] Docker daemon is not running!
  echo.
  echo Please open Docker Desktop from your Start menu and wait for it to load.
  echo Once you see the Docker icon in the taskbar, run this script again.
  echo.
  pause
  exit /b 1
)

echo [OK] Docker is running
echo.

REM Start the database
cd backend
echo Starting PostgreSQL container...
docker-compose up -d

if %ERRORLEVEL% EQU 0 (
  echo.
  echo [SUCCESS] PostgreSQL started!
  echo.
  echo It may take a few seconds to be ready for connections.
  echo Run: npm run dev
  echo.
) else (
  echo.
  echo [ERROR] Failed to start PostgreSQL
  echo.
)

cd ..
pause
