import socket
import sys
import time

RECV_SIZE_BYTES = 512

#connect to server info
SERVER_ADDRESS  = "127.0.0.1"
SERVER_PORT     = 10000
CLIENT_TOKEN    = "token1"

class Client:

	def __init__(self, server_address, port, token):
		self.server_address = server_address
		self.port			= port
		self.token 			= token

		self.connect_to_server()
		self.recv_send()

	def connect_to_server(self):
		
		try:
			self.sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
			print("Connecting to: " + str(self.server_address) + ":" + str(self.port) + " tkn: " +  self.token)
			self.sock.connect((self.server_address, self.port))

			self.sock.sendall(self.token)
			message = self.sock.recv(RECV_SIZE_BYTES)
			print(message)

		except socket.error:
			print("Oops there was an socket error and connection was closed !")

			#exit in case we get any socket error
			exit()

	def recv_send(self):
		try:

		    while True:
		    	server_data = self.sock.recv(RECV_SIZE_BYTES)
		        
		        start_time = time.time()
		        message_to_send = self.solve(server_data)
		        time.sleep(2)
		        end_time   = time.time()

		        rtt_in_s = round(end_time - start_time, 3)

		        print("It took: " + str(rtt_in_s) + " seconds to process the information !")

		        self.sock.sendall(message_to_send)

		except socket.error:
			print("Oops there was an socket error and connection to the server was closed !")

		finally:
		    self.sock.close()


	def solve(self, server_data):
		return server_data


c = Client(SERVER_ADDRESS, SERVER_PORT, CLIENT_TOKEN)