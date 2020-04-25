import socket 
import threading
import time
import sys


class Client:

	def __init__(self, unity_socket):

		#NOT UNIQUE UNITY TOKEN
		self.UNITY_TOKEN = "439b3a25b555b3bc8667a09a036ae70c"

		#DATA TRANSFER VALUES
		self.RECV_SIZE_BYTES = 1024
		self.RECV_TOKEN_TIMEOUT = 1
		self.RECV_TIMEOUT = 20

		#SOCKETS TO SEND AND RECEIVE INFO FROM UNITY AND IDE
		self.unity_socket = unity_socket
		self.ide_socket = None

		#DATA RECEIVED
		self.unity_read = ""
		self.ide_read = ""

		#DATA PENDING TO BE SENT
		self.unity_write = ""
		self.ide_write = ""

		#CONNECTION STATES
		self.is_connected = False
		self.is_ide_connected = False
		self.lobby_ready = False

		#IDE TOKEN VALIDATION
		self.ide_token = ""


	def start_new_socket_handler(self, client_socket, client_type):
		new_reader = threading.Thread(target = self.socket_reader, args = (client_socket, client_type))
		new_writer = threading.Thread(target = self.socket_writer, args = (client_socket, client_type))
		new_reader.start()
		new_writer.start()
		

	def socket_reader(self, client_socket, socket_type):
		function_name = sys._getframe().f_code.co_name
		try:
			while True:
				if socket_type == "unity":
					self.console_log(function_name + ": Waiting to read from unity...")
					self.unity_read = client_socket.recv(self.RECV_SIZE_BYTES)

					if (self.unity_read == ""):
						raise Exception("Connection with the unity client dropped !")

					self.console_log(function_name + ": Message read from unity ! " + self.unity_read)

				if socket_type == "ide":
					self.console_log(function_name + ": Waiting to read from ide...")
					self.ide_read = client_socket.recv(self.RECV_SIZE_BYTES)

					if (self.ide_read == ""):
						raise Exception("Connection with the ide client dropped !")

					self.console_log(function_name + ": Message read from ide! " + self.ide_read)
					
		except socket.timeout:
			self.console_log(function_name + ": This client has been disconnected due to timeout !")
			self.is_connected = False
			
		except socket.error:
			self.console_log(function_name + ": Oops there was an socket error and connection was closed !")
			self.is_connected = False

		except Exception as e:
			print ("Exception :" + str(e))
			self.is_connected = False


	def socket_writer(self, client_socket, socket_type):
		function_name = sys._getframe().f_code.co_name
		try:
			while True:
				if socket_type == "unity":
					self.console_log(function_name + ": Waiting for server to write to unity ...")
					while self.unity_write == "":
						time.sleep(0.1)

					client_socket.send(self.unity_write)
					self.console_log(function_name + ": Message sent to unity ! " + self.unity_write)

					self.unity_write = ""

				if socket_type == "ide":
					self.console_log(function_name + ": Waiting for server to write to ide ...")
					while self.ide_write == "":
						time.sleep(0.1)

					client_socket.send(self.ide_write)
					self.console_log(function_name + ": Message sent to ide ! " + self.ide_write)

					self.ide_write = ""
					
		except socket.timeout:
			self.console_log(function_name + ": This client has been disconnected due to timeout !")
			self.is_connected = False

		except socket.error:
			self.console_log(function_name + ": Oops there was an socket error and connection was closed !")
			self.is_connected = False

		except Exception as e:
			print ("Exception :" + str(e))
			self.is_connected = False


	def has_valid_token(self, client_socket, token):
		try:
			#check the token from the client 
			#to verify if it is authorized to join this server
			client_socket.settimeout(self.RECV_TOKEN_TIMEOUT)
			client_token = client_socket.recv(self.RECV_SIZE_BYTES)

			if (client_token == ""):
				raise Exception("Client disconnected !")

			if client_token == token:
				client_socket.send("TOKEN OK")
				client_socket.settimeout(self.RECV_TIMEOUT)
				return True
			else:
				raise Exception ("Wrong token from the client !")
				
		except socket.timeout:
			print("This client has been disconnected due to timeout !")
			self.is_connected = False

		except socket.error:
			print("Oops there was an socket error and connection was closed !")
			self.is_connected = False

		except Exception as exception:
			print (str(exception))
			self.is_connected = False

		return False


	def get_unity_address(self):
		if (self.unity_socket != None):
			return str(self.unity_socket.getpeername())
		return ""

	def get_ide_address(self):
		if(self.ide_socket != None):
			return str(self.ide_socket.getpeername())
		return ""

	def console_log(self, message):
		print (self.get_unity_address() + " " + self.get_ide_address() + " " + message)