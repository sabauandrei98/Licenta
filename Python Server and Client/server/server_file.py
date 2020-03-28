import socket
import sys

COLOR_GREEN = '\033[92m'
COLOR_YELLOW = '\033[93m'
COLOR_BLUE = '\033[94m'
COLOR_END = '\033[0m'

# Create a TCP/IP socket
sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

server_name = "127.0.0.1"
server_address = (server_name, 10000)

print >>sys.stderr, 'Starting up the server on %s:%s' % server_address
sock.bind(server_address)
sock.listen(0)

print >> sys.stderr, 'Waiting for a connection...'
connection, client_address = sock.accept()

print(COLOR_GREEN + "Client connected on address" + str(client_address) + COLOR_END)
print ("Received messages:")

def readInput():
    message = raw_input(">>Enter message: ")
    return message

try:
    while True:
        print("Waiting for the client ...")
        data = connection.recv(256)
        print(COLOR_BLUE + "CLIENT: " + data + COLOR_END)

        message = readInput()
        connection.sendall(message)
finally:
    connection.close()
