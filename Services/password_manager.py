# -*- coding: utf-8 -*-

import os
import sys
from cryptography.fernet import Fernet
import getpass

PASS_FILE = "host_pass.txt"
KEY_FILE = "host_key.txt"

DEFAULT_PASSWORD = "Admin123"

def generate_key():
    key = Fernet.generate_key()
    with open(KEY_FILE, "wb") as f:
        f.write(key)
    return key

def load_key():
    if not os.path.exists(KEY_FILE):
        return generate_key()
    with open(KEY_FILE, "rb") as f:
        return f.read()

def encrypt_password(password, key):
    fernet = Fernet(key)
    encrypted = fernet.encrypt(password.encode())
    with open(PASS_FILE, "wb") as f:
        f.write(encrypted)

def get_stored_password(key):
    fernet = Fernet(key)
    with open(PASS_FILE, "rb") as f:
        encrypted = f.read()
    return fernet.decrypt(encrypted).decode()

def prompt_password():
    key = load_key()

    if not os.path.exists(PASS_FILE):
        print("🔐 No password found. Storing default password.")
        encrypt_password(DEFAULT_PASSWORD, key)

    stored_password = get_stored_password(key)
    user_input = getpass.getpass("Enter admin password: ").strip()

    if user_input == stored_password:
        print("✅ Access granted.")
        sys.exit(0)
    else:
        print("❌ Access denied.")
        sys.exit(1)

def update_password():
    key = load_key()
    old = getpass.getpass("Enter current password: ").strip()
    if old != get_stored_password(key):
        print("❌ Wrong current password.")
        sys.exit(1)
    new_pass = getpass.getpass("Enter new password: ").strip()
    confirm = getpass.getpass("Confirm new password: ").strip()
    if new_pass != confirm:
        print("❌ Passwords do not match.")
        sys.exit(1)
    encrypt_password(new_pass, key)
    print("✅ Password updated successfully.")
    sys.exit(0)

if __name__ == "__main__":
    if len(sys.argv) > 1 and sys.argv[1] == "set":
        update_password()
    else:
        prompt_password()
