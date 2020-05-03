import socket
import sys
import time
import threading

RECV_SIZE_BYTES = 512

#connect to server info
SERVER_ADDRESS  = "localhost"
SERVER_PORT     = 50000
CLIENT_TOKEN    = "token2"

class Client:

	def __init__(self, server_address, port, token):
		self.server_address = server_address
		self.port			= port
		self.token 			= token
		self.client_socket = None
		self.ide_write = ""
		self.ide_read = ""
		self.connect_to_server()

	def connect_to_server(self):
		try:
			self.client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
			self.client_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
			print("Connecting to: " + str(self.server_address) + ":" + str(self.port) + " tkn: " +  self.token)
			self.client_socket.connect((self.server_address, self.port))

			self.client_socket.send(self.token)
			message = self.client_socket.recv(RECV_SIZE_BYTES)
			if (len(message) == 0):
				raise Exception("Your token is wrong OR already used in a connection !")

			#at this point, the client successfully connected to the server
			print("The you were successfully verified by the server !")
			
			new_reader = threading.Thread(target = self.socket_reader, args = ())
			new_writer = threading.Thread(target = self.socket_writer, args = ())
			new_reader.start()
			new_writer.start()

		except socket.error:
			print("Oops there was an socket error and connection was closed !")
		except Exception as error:
			print (error)


	def socket_reader(self):
		function_name = sys._getframe().f_code.co_name
		try:
			while True:
				self.console_log(function_name + ": Waiting to read from unity...")
				self.ide_read = self.client_socket.recv(RECV_SIZE_BYTES)

				if (self.ide_read == ""):
					raise Exception("Connection with the unity client dropped !")

				self.console_log(function_name + ": Message read from unity ! " + self.ide_read)
				self.solve(self.ide_read)
					
		except socket.timeout:
			self.console_log(function_name + ": This client has been disconnected due to timeout !")
		except socket.error:
			self.console_log(function_name + ": Oops there was an socket error and connection was closed !")
		except Exception as e:
			print ("Exception :" + str(e))
		finally:
			self.client_socket.shutdown(socket.SHUT_RDWR)
			self.client_socket.close()


	def socket_writer(self):
		function_name = sys._getframe().f_code.co_name
		try:
			while True:
				self.console_log(function_name + ": Waiting for server to write to unity ...")
				while self.ide_write == "":
					time.sleep(0.1)

				self.client_socket.send(self.ide_write)
				self.console_log(function_name + ": Message sent to unity ! " + self.ide_write)

				self.ide_write = ""
					
		except socket.timeout:
			self.console_log(function_name + ": This client has been disconnected due to timeout !")
		except socket.error:
			self.console_log(function_name + ": Oops there was an socket error and connection was closed !")
		except Exception as e:
			print ("Exception :" + str(e))
		finally:
			self.client_socket.close()

	def get_ide_address(self):
			if(self.client_socket != None):
				return str(self.client_socket.getpeername())
			return ""

	def console_log(self, message):
		print (self.get_ide_address() + " " + message)


	def solve(self, server_data):
		
		self.ide_write = "LOOP"



c = Client(SERVER_ADDRESS, SERVER_PORT, CLIENT_TOKEN)