import socket
import sys

# Create a TCP/IP socket
sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

server_address = ("127.0.0.1", 10000)
print >> sys.stderr, 'connecting to %s port %s' % server_address
sock.connect(server_address)

token = raw_input(">>Enter token: ")

def readInput():
    message = raw_input(">>Enter message: ")
    return message

try:
    while True:
        message = readInput()
        sock.sendall(message + " " + token)

        print("Message sent. Waiting for the server ...")
        data = sock.recv(256)
        print("SERVER: " + data)

finally:
    sock.close()