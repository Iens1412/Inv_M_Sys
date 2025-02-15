import os
import json

# New credentials (Modify as needed)
NEW_CONFIG = {
    "Host": "host.docker.internal",
    "Port": "5433",
    "Username": "admin",
    "Password": "admin123",
    "Database": "inventory"
}

# File path for appsettings.json
CONFIG_PATH = os.path.join(os.path.dirname(__file__), "appsettings.json")

def reset_server_credentials():
    try:
        # Update appsettings.json
        with open(CONFIG_PATH, "r") as file:
            config = json.load(file)
        
        config["DatabaseConfig"] = NEW_CONFIG

        with open(CONFIG_PATH, "w") as file:
            json.dump(config, file, indent=2)

        print("✅ Server credentials reset successfully in appsettings.json!")

        # Restart Docker container (if running)
        os.system("docker restart inventory_db")
        print("🔄 PostgreSQL container restarted.")
    except Exception as e:
        print(f"❌ Error resetting server credentials: {e}")

if __name__ == "__main__":
    reset_server_credentials()