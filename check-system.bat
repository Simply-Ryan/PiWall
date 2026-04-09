@echo off
REM PitWall - System Check
REM Validates that all prerequisites are installed and configured correctly

setlocal enabledelayedexpansion

echo.
echo [PitWall] System Requirements Check
echo ====================================
echo.

set ERRORS=0
set WARNINGS=0

REM Check Node.js
echo Checking: Node.js...
where node >nul 2>nul
if %ERRORLEVEL% EQU 0 (
  for /f "tokens=*" %%i in ('node --version') do set NODE_VERSION=%%i
  echo   [OK] Found: %NODE_VERSION%
) else (
  echo   [ERROR] Node.js not installed
  echo   Please install from https://nodejs.org/
  set /a ERRORS=!ERRORS!+1
)
echo.

REM Check npm
echo Checking: npm...
where npm >nul 2>nul
if %ERRORLEVEL% EQU 0 (
  for /f "tokens=*" %%i in ('npm --version') do set NPM_VERSION=%%i
  echo   [OK] Found: npm v!NPM_VERSION!
) else (
  echo   [ERROR] npm not installed
  set /a ERRORS=!ERRORS!+1
)
echo.

REM Check Git
echo Checking: Git...
where git >nul 2>nul
if %ERRORLEVEL% EQU 0 (
  for /f "tokens=*" %%i in ('git --version') do set GIT_VERSION=%%i
  echo   [OK] Found: !GIT_VERSION!
) else (
  echo   [WARNING] Git not found (optional for development)
  set /a WARNINGS=!WARNINGS!+1
)
echo.

REM Check Docker
echo Checking: Docker...
where docker >nul 2>nul
if %ERRORLEVEL% EQU 0 (
  echo   [OK] Docker CLI found
  docker info >nul 2>nul
  if !ERRORLEVEL! EQU 0 (
    echo   [OK] Docker daemon is running
  ) else (
    echo   [WARNING] Docker CLI found but daemon not running
    echo   [INFO] Local PostgreSQL must be running on port 5432
    set /a WARNINGS=!WARNINGS!+1
  )
) else (
  echo   [INFO] Docker not installed - using local PostgreSQL
  echo   Please ensure PostgreSQL is running on port 5432
)
echo.

REM Check if project directories exist
echo Checking: Project structure...
if exist backend (
  echo   [OK] backend/ directory found
) else (
  echo   [ERROR] backend/ directory not found
  set /a ERRORS=!ERRORS!+1
)

if exist frontend (
  echo   [OK] frontend/ directory found
) else (
  echo   [ERROR] frontend/ directory not found
  set /a ERRORS=!ERRORS!+1
)
echo.

REM Check PostgreSQL (if not using Docker)
where docker >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
  echo Checking: Local PostgreSQL...
  echo   [INFO] PostgreSQL Server must be running on localhost:5432
  echo   If installed locally, verify it's started in Services or use:
  echo   - Windows: pg_ctl start -D "C:\Program Files\PostgreSQL\data"
  echo   - macOS: brew services start postgresql
  echo   - Linux: sudo systemctl start postgresql
  echo.
)

REM Summary
echo ====================================
echo Summary:
if %ERRORS% EQU 0 (
  if %WARNINGS% EQU 0 (
    echo [SUCCESS] All systems ready! Run 'start-all.bat' to begin
  ) else (
    echo [WARNING] System ready with %WARNINGS% warning(s). Run 'start-all.bat' to begin
  )
) else (
  echo [ERROR] %ERRORS% error(s) found. Please fix above issues and try again.
)
echo ====================================
echo.
pause
