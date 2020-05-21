import socket 
import threading
import time
import sys


class Client:

	def __init__(self, unity_socket):


		#DATA TRANSFER VALUES
		self.RECV_SIZE_BYTES = 512

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
		

	"""
		This function is running on two separate threads
			- one thread reading data from unity client
			- one thread reading data from ide client

			@client_socket: socket, representing a socket (unity/ide)
			@socket_type: string, representing a type of socket function will read data for

			- in case of any error, the connection to the server is closed
	"""
	def socket_reader(self, client_socket, socket_type):

		#string, representing the function name, used for console logging
		function_name = sys._getframe().f_code.co_name

		try:
			while True:
				if socket_type == "unity":
					self.console_log(function_name + ": Waiting to read from unity...")

					#string, store data received from the server
					self.unity_read = client_socket.recv(self.RECV_SIZE_BYTES)

					#if empty data, close the connection
					if (self.unity_read == ""):
						raise Exception(": Connection with the unity client dropped !")

					self.console_log(function_name + ": Message read from unity ! " + self.unity_read)

				if socket_type == "ide":
					self.console_log(function_name + ": Waiting to read from ide...")

					#string, store data received from the server
					self.ide_read = client_socket.recv(self.RECV_SIZE_BYTES)

					#if empty data, close the connection
					if (self.ide_read == ""):
						raise Exception(": Connection with the ide client dropped !")

					self.console_log(function_name + ": Message read from ide! " + self.ide_read)
				
		#handle possible exceptions	and close the connection
		except socket.timeout:
			self.console_log(function_name + ": This client has been disconnected due to timeout !")
			self.is_connected = False
			
		except socket.error:
			self.console_log(function_name + ": Oops there was an socket error and connection was closed !")
			self.is_connected = False

		except Exception as e:
			self.console_log(function_name + str(e))
			self.is_connected = False



	"""
		This function is running on two separate threads
			- one thread writing data to unity client
			- one thread writing data to ide client
			- it checks if "ide_write" or "unity_write" vars have any data and sends it to the server
			- logs the information

			@client_socket: socket, representing a socket (unity/ide)
			@socket_type: string, representing a type of socket function will send data to

			- in case of any error, the connection to the server is closed
	"""
	def socket_writer(self, client_socket, socket_type):

		#string, representing the function name, used for console logging
		function_name = sys._getframe().f_code.co_name

		try:
			while True:
				if socket_type == "unity":
					self.console_log(function_name + ": Waiting for server to write to unity ...")

					#string, if the var is empty, sleep and check later
					while self.unity_write == "":
						time.sleep(0.1)

					#if the var has data, send it to the server
					client_socket.send(self.unity_write)

					self.console_log(function_name + ": Message sent to unity ! " + self.unity_write)

					#reset the variable to avoid further sendings
					self.unity_write = ""

				if socket_type == "ide":
					self.console_log(function_name + ": Waiting for server to write to ide ...")

					#string, if the var is empty, sleep and check later
					while self.ide_write == "":
						time.sleep(0.1)

					#if the var has data, send it to the server
					client_socket.send(self.ide_write)
					self.console_log(function_name + ": Message sent to ide ! " + self.ide_write)

					#reset the variable to avoid further sendings
					self.ide_write = ""
			
		#handle possible exceptions	and close the connection		
		except socket.timeout:
			self.console_log(function_name + ": This client has been disconnected due to timeout !")
			self.is_connected = False

		except socket.error:
			self.console_log(function_name + ": Oops there was an socket error and connection was closed !")
			self.is_connected = False

		except Exception as e:
			self.console_log(function_name + str(e))
			self.is_connected = False


	"""
		This function tries to get the unity socket details

		return: string, representing the socket details (ip, port)
				""    , if no socket data available
	"""
	def get_unity_address(self):
		try:
			if (self.unity_socket != None):
				return str(self.unity_socket.getpeername())
		except:
			pass
		return ""


	"""
		This function tries to get the ide client socket details

		return: string, representing the socket details (ip, port)
				""    , if no socket data available
	"""
	def get_ide_address(self):
		try:
			if(self.ide_socket != None):
				return str(self.ide_socket.getpeername())
		except:
			pass
		return ""


	"""
		This function logs data, printing the socket details, in case of a connected one
			@message: string, message to be printed
	"""
	def console_log(self, message):
		print ("Unity:<" + self.get_unity_address() + "> Ide: <" + self.get_ide_address() + "> " + message)