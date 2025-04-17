import json
import os
import socket

def get_local_ip():
    """Get the local IP address of this machine (client)."""
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    try:
        # Connect to a non-routable address, to get the local interface IP
        s.connect(("10.255.255.255", 1))
        IP = s.getsockname()[0]
    except Exception:
        IP = "127.0.0.1"
    finally:
        s.close()
    return IP

def update_appsettings(host_ip):
    """Update appsettings.json with the new host IP."""
    settings_path = os.path.join("Services", "appsettings.json")
    if not os.path.exists(settings_path):
        print("❌ appsettings.json not found.")
        return False

    with open(settings_path, "r") as f:
        config = json.load(f)

    config["DatabaseConfig"]["Host"] = host_ip

    with open(settings_path, "w") as f:
        json.dump(config, f, indent=2)

    return True

def main():
    print("===================================")
    print("     Client Configuration Mode     ")
    print("===================================")
    local_ip = get_local_ip()
    print(f"🖥 Your local IP address: {local_ip}")
    host_ip = input("📡 Enter the Host PC IP address (where DB is running): ").strip()

    if update_appsettings(host_ip):
        print(f"✅ appsettings.json updated with Host IP: {host_ip}")
    else:
        print("❌ Failed to update configuration.")

if __name__ == "__main__":
    main()