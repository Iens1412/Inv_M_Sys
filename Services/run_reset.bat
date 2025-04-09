@echo off
echo =======================================
echo        Server Maintenance Menu
echo =======================================
echo 1. Reset Owner Credentials
echo 2. Run Docker to Start Database Server
set /p choice=Enter your choice (1 or 2): 

if "%choice%"=="1" (
    python reset_owner.py
) else if "%choice%"=="2" (
    python run_docker.py
) else (
    echo Invalid choice. Exiting...
)

pause