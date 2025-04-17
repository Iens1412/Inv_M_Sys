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
echo =======================================
set /p choice=Enter your choice (1-3): 

if "%choice%"=="1" (
    call :askPassword
    if "!auth!"=="true" (
        python run_docker.py
        python test_db_and_encrypt.py
    ) else (
        echo Access Denied. Wrong password.
        pause
        goto menu
    )
) else if "%choice%"=="2" (
    python run_client_mode.py
) else if "%choice%"=="3" (
    call :askPassword
    if "!auth!"=="true" (
        python reset_owner.py
    ) else (
        echo Access Denied. Wrong password.
        pause
        goto menu
    )
) else (
    echo Invalid choice. Exiting...
    pause
)

goto :eof

:askPassword
set /p "pass=Enter host password: "
if "%pass%"=="Admin123" (
    set auth=true
) else (
    set auth=false
)
exit /b