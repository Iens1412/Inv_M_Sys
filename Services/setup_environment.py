# -*- coding: utf-8 -*-

import subprocess
import sys
import os
import platform
import urllib.request

# Only allow running if a special environment variable is set
if os.environ.get("ALLOW_SETUP") != "YES":
    print("❌ Direct access not allowed. Please run setup from the main .bat file.")
    sys.exit(1)

def install_python_packages():
    print("\n📦 Installing required Python packages...")
    subprocess.check_call([sys.executable, "-m", "pip", "install", "cryptography", "psycopg2-binary"])
    print("✅ Python packages installed successfully.")

def check_dotnet_installed():
    print("\n🔎 Checking if .NET Runtime/SDK is installed...")
    try:
        result = subprocess.run(["dotnet", "--version"], capture_output=True, text=True)
        if result.returncode == 0:
            print(f"✅ .NET is already installed (version: {result.stdout.strip()})")
            return True
        else:
            print("⚠️ .NET SDK/Runtime not found.")
            return False
    except FileNotFoundError:
        print("⚠️ .NET SDK/Runtime not found.")
        return False

def download_dotnet_runtime():
    print("\n🌐 Downloading .NET Desktop Runtime Installer...")
    dotnet_runtime_url = "https://download.visualstudio.microsoft.com/download/pr/9b65a517-0eb6-46a7-8ef2-d0c3f4bb4aeb/b32b86e06c4d93e38e25e1e6f70c82ab/windowsdesktop-runtime-8.0.4-win-x64.exe"
    filename = "windowsdesktop-runtime-installer.exe"
    urllib.request.urlretrieve(dotnet_runtime_url, filename)
    print(f"✅ Downloaded '{filename}' successfully.")
    print("👉 Please manually run the installer to complete .NET Runtime installation.")

def main():
    print("🔧 Starting environment setup for Inventory Management System...\n")

    # Step 1: Install Python packages
    install_python_packages()

    # Step 2: Check for .NET SDK/Runtime
    if not check_dotnet_installed():
        download_dotnet_runtime()

    print("\n🚀 Setup finished! Please:")
    print("- Run the downloaded .NET installer manually if needed.")
    print("- Install Docker manually.")
    print("- Then you can run your application.")
    print("\n✅ All environment tools are prepared.")

if __name__ == "__main__":
    if platform.system() != "Windows":
        print("⚠️ Warning: This setup script is designed for Windows only!")
    main()
