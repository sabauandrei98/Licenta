import socket
import sys
import time

RECV_SIZE_BYTES = 512

#connect to server info
SERVER_ADDRESS  = "localhost"
SERVER_PORT     = 50000
CLIENT_TOKEN    = "token1"

class Client:

	def __init__(self, server_address, port, token):
		self.server_address = server_address
		self.port			= port
		self.token 			= token
		self.connect_to_server()

	def connect_to_server(self):
		try:
			self.c_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
			print("Connecting to: " + str(self.server_address) + ":" + str(self.port) + " tkn: " +  self.token)
			self.c_socket.connect((self.server_address, self.port))

			self.c_socket.sendall(self.token)
			message = self.c_socket.recv(RECV_SIZE_BYTES)
			if (len(message) == 0):
				raise Exception("Your token is wrong OR already used in a connection !")

			#at this point, the client successfully connected to the server
			print("The you were successfully verified by the server !")
			
			while True:
				server_data = self.c_socket.recv(RECV_SIZE_BYTES)
				print("Server data: " + server_data)

				start_time = time.time()
				message_to_send = self.solve(server_data)
				time.sleep(2)
				end_time   = time.time()

				rtt_in_s = round(end_time - start_time, 3)
				print("It took: " + str(rtt_in_s) + " seconds to process the information !")

				self.c_socket.sendall(message_to_send)

		except socket.error:
			print("Oops there was an socket error and connection was closed !")
		except Exception as error:
			print (error)
		finally:
			self.c_socket.close()


	def solve(self, server_data):
		return server_data


c = Client(SERVER_ADDRESS, SERVER_PORT, CLIENT_TOKEN)