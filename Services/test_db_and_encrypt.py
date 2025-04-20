import os
import json
import psycopg2
from cryptography.fernet import Fernet
import sys

if os.environ.get("ALLOW_RUN") != "YES":
    print("❌ This script must be run through the official launcher.")
    sys.exit(1)


CONFIG_PATH = os.path.join("Services", "appsettings.json")
PASSWORD_FILE = "host_pass.txt"
KEY_FILE = "host_key.txt"
DEFAULT_PASSWORD = "Admin123"

def generate_key():
    key = Fernet.generate_key()
    with open(KEY_FILE, "wb") as f:
        f.write(key)
    return key

def load_key():
    if os.path.exists(KEY_FILE):
        with open(KEY_FILE, "rb") as f:
            return f.read()
    return generate_key()

def encrypt_password(password, key):
    fernet = Fernet(key)
    encrypted = fernet.encrypt(password.encode())
    with open(PASSWORD_FILE, "wb") as f:
        f.write(encrypted)

def decrypt_password(key):
    fernet = Fernet(key)
    with open(PASSWORD_FILE, "rb") as f:
        encrypted = f.read()
    return fernet.decrypt(encrypted).decode()

def update_config_host():
    hostname = os.popen("hostname -I").read().strip().split()[0]  # get local IP
    if not hostname:
        print("❌ Failed to get host IP.")
        return

    with open(CONFIG_PATH, "r+") as file:
        data = json.load(file)
        data["DatabaseConfig"]["Host"] = hostname
        file.seek(0)
        json.dump(data, file, indent=4)
        file.truncate()

def test_db_connection():
    try:
        with open(CONFIG_PATH, "r") as file:
            cfg = json.load(file)["DatabaseConfig"]
        conn = psycopg2.connect(
            host=cfg["Host"],
            port=cfg["Port"],
            user=cfg["Username"],
            password=cfg["Password"],
            dbname=cfg["Database"]
        )
        conn.close()
        print("✅ Connection to DB succeeded.")
    except Exception as e:
        print("❌ Failed to connect to DB:", e)

def main():
    key = load_key()

    if not os.path.exists(PASSWORD_FILE):
        print("🔐 Encrypting default password...")
        encrypt_password(DEFAULT_PASSWORD, key)
        print("✅ Password encrypted and stored.")

    update_config_host()
    test_db_connection()

if __name__ == "__main__":
    main()