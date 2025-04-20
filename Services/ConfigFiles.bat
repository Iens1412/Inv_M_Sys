@echo off
setlocal EnableDelayedExpansion

:menu
cls
echo =======================================
echo        Server Maintenance Menu
echo =======================================
echo 1. Run as HOST (Start DB Server)
echo 2. Run as CLIENT (Connect Only)
echo 3. Reset Owner Credentials (Admin Only)
echo 4. Change Admin Menu Password
echo =======================================
set /p choice=Enter your choice (1-4): 

if "%choice%"=="1" (
    call :checkAdmin
    if !auth! == true (
        set ALLOW_RUN=YES
        python run_docker.py
        set ALLOW_RUN=YES
        python test_db_and_encrypt.py
    )
    pause
    goto menu
)

if "%choice%"=="2" (
    set ALLOW_RUN=YES
    python run_client_mode.py
    pause
    goto menu
)

if "%choice%"=="3" (
    call :checkAdmin
    if !auth! == true (
        set ALLOW_RUN=YES
        python reset_owner.py
    )
    pause
    goto menu
)

if "%choice%"=="4" (
    python password_manager.py set
    pause
    goto menu
)

goto :eof

:checkAdmin
python password_manager.py > nul
if %ERRORLEVEL% == 0 (
    set auth=true
) else (
    set auth=false
    echo Access denied.
)
exit /b
