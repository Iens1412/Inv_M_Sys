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

def main():
    print("🔧 Starting environment setup for Inventory Management System...\n")

    # Step 1: Install Python packages
    install_python_packages()

    print("\n🚀 Setup finished! Please:")
    print("- Run the downloaded .NET installer manually if needed.")
    print("- Install Docker manually.")
    print("- Then you can run your application.")
    print("\n✅ All environment tools are prepared.")

if __name__ == "__main__":
    if platform.system() != "Windows":
        print("⚠️ Warning: This setup script is designed for Windows only!")
    main()
