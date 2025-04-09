import subprocess

def run_docker():
    try:
        print("🛠 Starting Docker containers with docker-compose...\n")
        subprocess.run(["docker-compose", "up", "-d"], check=True)
        print("✅ Docker containers started successfully.")
    except subprocess.CalledProcessError as e:
        print(f"❌ Error running docker-compose: {e}")
    except FileNotFoundError:
        print("❌ docker-compose is not installed or not in PATH.")

if __name__ == "__main__":
    run_docker()