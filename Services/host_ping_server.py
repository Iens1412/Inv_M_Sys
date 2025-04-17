import socket

def start_server(port=9999):
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.bind(('', port))
        s.listen(1)
        print("✅ Waiting for client to detect me...")

        conn, addr = s.accept()
        with conn:
            print(f"🔗 Connected by {addr}")
            conn.sendall(b"host-detected")

if __name__ == "__main__":
    start_server()