import socket 
import threading
import time
import sys
from Game import Game

"""
	This class is packing up an unity socket and an ide socket representing an single Client
		- the class also manages the reading and the writing from and to the server
"""
class Client:

	def __init__(self):

		#reading data buffer size
		self.recv_size_bytes = Game.get_buffer_size()

		#sockets
		self.unity_socket = None
		self.ide_socket = None

		#data received from the unity/ide
		self.unity_read = ""
		self.ide_read = ""

		#data pending to be sent to unity/ide
		self.unity_write = ""
		self.ide_write = ""

		#connection states
		self.is_connected = False
		self.is_ide_connected = False
		self.lobby_ready = False

		#ideo token validation
		self.ide_token = ""


	"""
		This function creates two threads:
			- one for reading and one for writing data

			@client_socket: socket, representing on which socket will the read/write be executed
			@client_type: string, argument which will be passed to the function running on the thread
								 - responsible for knowing on which variable to store the info + console logging
	"""
	def start_new_socket_handler(self, client_socket, client_type):
		new_reader = threading.Thread(target = self.__socket_reader, args = (client_socket, client_type))
		new_writer = threading.Thread(target = self.__socket_writer, args = (client_socket, client_type))
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
	def __socket_reader(self, client_socket, socket_type):

		#string, representing the function name, used for console logging
		function_name = sys._getframe().f_code.co_name

		try:
			while True:
				if socket_type == "unity":
					self.__console_log(function_name + ": Waiting to read from unity...")

					#string, store data received from the server
					self.unity_read = client_socket.recv(self.recv_size_bytes)

					#if empty data, close the connection
					if (self.unity_read == ""):
						raise Exception(": Connection with the unity client dropped !")

					self.__console_log(function_name + ": Message read from unity ! " + self.unity_read)

				if socket_type == "ide":
					self.__console_log(function_name + ": Waiting to read from ide...")

					#string, store data received from the server
					self.ide_read = client_socket.recv(self.recv_size_bytes)

					#if empty data, close the connection
					if (self.ide_read == ""):
						raise Exception(": Connection with the ide client dropped !")

					self.__console_log(function_name + ": Message read from ide! " + self.ide_read)
				
		#handle possible exceptions	and close the connection
		except socket.timeout:
			self.__console_log(function_name + ": This client has been disconnected due to timeout !")
			self.is_connected = False
			
		except socket.error:
			self.__console_log(function_name + ": Oops there was an socket error and connection was closed !")
			self.is_connected = False

		except Exception as e:
			self.__console_log(function_name + str(e))
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
	def __socket_writer(self, client_socket, socket_type):

		#string, representing the function name, used for console logging
		function_name = sys._getframe().f_code.co_name

		try:
			while True:
				if socket_type == "unity":
					self.__console_log(function_name + ": Waiting for server to write to unity ...")

					#string, if the var is empty, sleep and check later
					while self.unity_write == "":
						time.sleep(0.1)

					#if the var has data, send it to the server
					client_socket.send(self.unity_write)

					self.__console_log(function_name + ": Message sent to unity ! " + self.unity_write)

					#reset the variable to avoid further sendings
					self.unity_write = ""

				if socket_type == "ide":
					self.__console_log(function_name + ": Waiting for server to write to ide ...")

					#string, if the var is empty, sleep and check later
					while self.ide_write == "":
						time.sleep(0.1)

					#if the var has data, send it to the server
					client_socket.send(self.ide_write)
					self.__console_log(function_name + ": Message sent to ide ! " + self.ide_write)

					#reset the variable to avoid further sendings
					self.ide_write = ""
			
		#handle possible exceptions	and close the connection		
		except socket.timeout:
			self.__console_log(function_name + ": This client has been disconnected due to timeout !")
			self.is_connected = False

		except socket.error:
			self.__console_log(function_name + ": Oops there was an socket error and connection was closed !")
			self.is_connected = False

		except Exception as e:
			self.__console_log(function_name + str(e))
			self.is_connected = False


	"""
		This function tries to get the unity socket details

		return: string, representing the socket details (ip, port)
				""    , if no socket data available
	"""
	def __get_unity_address(self):
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
	def __get_ide_address(self):
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
	def __console_log(self, message):
		print ("Unity:<" + self.__get_unity_address() + "> Ide: <" + self.__get_ide_address() + "> " + message)