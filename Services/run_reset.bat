@echo off
echo Choose an option:
echo 1. Reset Owner Credentials
echo 2. Reset Server Credentials
set /p choice=Enter your choice:

if "%choice%"=="1" (
    python reset_owner.py
)

if "%choice%"=="2" (
    python reset_server.py
)
pause