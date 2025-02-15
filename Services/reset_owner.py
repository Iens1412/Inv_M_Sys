import psycopg2
import hashlib
import json
import os

# Load database credentials from appsettings.json
def load_config():
    config_path = os.path.join(os.path.dirname(__file__), "appsettings.json")
    with open(config_path, "r") as file:
        config = json.load(file)
    return config["DatabaseConfig"]

# Hash password using SHA256
def hash_password(password):
    return hashlib.sha256(password.encode()).hexdigest()

def reset_owner():
    db_config = load_config()
    
    try:
        conn = psycopg2.connect(
            host=db_config["Host"],
            port=db_config["Port"],
            user=db_config["Username"],
            password=db_config["Password"],
            dbname=db_config["Database"]
        )
        cursor = conn.cursor()

        new_username = "admin"
        new_password = hash_password("admin123")  # Change this as needed

        cursor.execute("""
            UPDATE Owner 
            SET Username = %s, Password = %s 
            WHERE Id = 1
        """, (new_username, new_password))

        conn.commit()
        print("✅ Owner credentials reset successfully!")
    except Exception as e:
        print(f"❌ Error resetting owner credentials: {e}")
    finally:
        if conn:
            cursor.close()
            conn.close()

if __name__ == "__main__":
    reset_owner()