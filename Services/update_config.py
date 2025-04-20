import socket
import json
import os
import sys

if os.environ.get("ALLOW_RUN") != "YES":
    print("❌ This script must be run through the official launcher.")
    sys.exit(1)

def find_host_on_lan(port=9999):
    base_ip = "192.168.1."  # Adjust to your LAN subnet
    for i in range(1, 255):
        ip = f"{base_ip}{i}"
        try:
            with socket.create_connection((ip, port), timeout=0.5) as conn:
                response = conn.recv(1024)
                if b"host-detected" in response:
                    print(f"✅ Host found at {ip}")
                    return ip
        except:
            continue
    return None

def update_appsettings(ip, filepath="Services/appsettings.json"):
    if not os.path.exists(filepath):
        print("❌ appsettings.json not found!")
        return

    with open(filepath, "r") as file:
        config = json.load(file)

    config["DatabaseConfig"]["Host"] = ip

    with open(filepath, "w") as file:
        json.dump(config, file, indent=2)
    
    print(f"✅ Updated appsettings.json with host IP: {ip}")

if __name__ == "__main__":
    print("🔍 Searching for database host on LAN...")
    host_ip = find_host_on_lan()
    if host_ip:
        update_appsettings(host_ip)
    else:
        print("❌ Could not find host on LAN.")