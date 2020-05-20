import socket
import sys
import time
import threading
from Game import Game

#socket buffer size
RECV_SIZE_BYTES = 512

"""
	Client socket class is responsible for:
		- creating a socket which connects to the server
		- reading data from the server
		- processing the data
		- sending data back to the server
"""
class ClientSocket:

	"""
		__init__ function assigns the parameters and attempts to connect to the server

		@server_address: string, an ip used to connect to the server
		@port:			 int   , a port used to connect to the server
		@token:			 string, an unique session identification code used to connect to the server
		@solve_function: function, used to process the data server and to get an action
	"""
	def __init__(self, server_address, port, token, solve_function):
		#params
		self.server_address = server_address
		self.port			= port
		self.token 			= token
		self.solve_function = solve_function

		#socket which will be used to send and receive data
		self.client_socket = None

		#string, the data that needs to be sent to the server
			# a thread checks if this var has data and if so, it starts sending it
		self.ide_write = ""

		#string, the data that is received from the server
			# a thread checks if socket contains any data and if so, it stores it in this var 
		self.ide_read = ""

		self.connect_to_server()



	"""
		This function is responsible for the connection to the server:
			- creates a socket which will be used to send/receive data
			- tries to connect the socket to the server
			- sends the token to the server to be validated
			- receives a server response
			- creates 2 threads to handle send/receive data

			- if any of above fails, the socket is closed

	"""
	def connect_to_server(self):

		#string, function name, for console log purposes
		function_name = sys._getframe().f_code.co_name

		try:
			#setting up the socket
			self.client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
			self.client_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
	
			self.console_log(function_name + ": Connecting to: " + str(self.server_address) + ":" + str(self.port) + " tkn: " +  self.token)
			
			#connecting to the server
			self.client_socket.connect((self.server_address, self.port))

			#sending the token to be validated
			self.client_socket.send(self.token)

			#string, server respose
			message = self.client_socket.recv(RECV_SIZE_BYTES)
			self.console_log(function_name + ": Server response: " + message)

			#if token not verified, raise exception
			if (message != "Token verified !"):
				raise Exception("Your token is wrong OR already used in a connection !")

			#at this point, the client successfully connected to the server
			self.console_log(function_name + ": You were successfully verified by the server !")
			
			#start some threads to handle sending/receiving data
			new_reader = threading.Thread(target = self.socket_reader, args = ())
			new_writer = threading.Thread(target = self.socket_writer, args = ())
			new_reader.start()
			new_writer.start()

		#catch exceptions
		except socket.error:
			#exception happening when the user was succesfully connected to the server
			#but the server is not reacheable for sending the token
			try:
				self.client_socket.close()
			except:
				pass

			self.console_log(function_name + ": Oops there was an socket error and connection was closed !")

		except Exception as error:
			#exception happening when the token is wrong or
			#the socket tries to connect while the game is running
			try:
				self.client_socket.close()
			except:
				pass

			self.console_log(function_name + ": " + error)



	"""
		This function is running on a separate thread
			- it waits for the server to send data
			- sends the data to the Player class to be processed
			- logs the information

			- in case of any error, the connection to the server is closed
	"""
	def socket_reader(self):

		#string, function name, for console log purposes
		function_name = sys._getframe().f_code.co_name

		try:
			while True:
				self.console_log(function_name + ": Waiting to read from server...")

				#string, store data received from the server
				self.ide_read = self.client_socket.recv(RECV_SIZE_BYTES)

				#if empty data, close the connection
				if (self.ide_read == ""):
					raise Exception(": Connection with the server dropped !")

				self.console_log(function_name + ": Message read from server ! Msg:\n" + self.ide_read + "<END>")

				#send data to the Player class to be processed
				self.get_player_response(self.ide_read)
		
		#handle the possible exceptions			
		except socket.timeout:
			self.console_log(function_name + ": This client has been disconnected due to timeout !")
		except socket.error:
			self.console_log(function_name + ": Oops there was an socket error and connection was closed !")
		except Exception as e:
			self.console_log(function_name + str(e))

		#finally, close the connection
		finally:
			self.client_socket.shutdown(socket.SHUT_RDWR)
			self.client_socket.close()


	"""
		This function is running on a separate thread
			- it check if "ide_write" var has any data and sends it to the server
			- logs the information

			- in case of any error, the connection to the server is closed
	"""
	def socket_writer(self):

		#string, function name, for console log purposes
		function_name = sys._getframe().f_code.co_name

		try:
			while True:
				self.console_log(function_name + ": Waiting for client to write to server ...")

				#string, if the var is empty, sleep and check later
				while self.ide_write == "":
					time.sleep(0.1)

				#if the var has data, send it to the server
				self.client_socket.send(self.ide_write)

				self.console_log(function_name + ": Message sent to server ! Msg:\n" + self.ide_write + "<END>")

				#reset the variable to avoid futher sendings
				self.ide_write = ""
		
		#handle the possible exceptions		
		except socket.timeout:
			self.console_log(function_name + ": This client has been disconnected due to timeout !")
		except socket.error:
			self.console_log(function_name + ": Oops there was an socket error and connection was closed !")
		except Exception as e:
			self.console_log(function_name + str(e))

		#finally, close the connection
		finally:
			self.client_socket.shutdown(socket.SHUT_RDWR)
			self.client_socket.close()


	"""
		This function tries to get the socket details

		return: string, representing the socket details (ip, port)
				""    , if no socket data available
	"""
	def get_ide_address(self):
			try:
				if(self.client_socket != None):
					return str(self.client_socket.getpeername())
			except:
				pass
			return ""


	"""
		This function logs data, printing the socket details, in case of a connected one
			@message: string, message to be printed
	"""
	def console_log(self, message):
		print ("Connection: <" + self.get_ide_address() + "> " + message)



	"""
		This function sends server data to the Player class which will process it
		and will return a value, filling the "ide_write" var which represents the write buffer
			@server_data: string, rows of data from the server separated by \n
	"""
	def get_player_response(self, server_data):

		#string, function name, for console log purposes
		function_name = sys._getframe().f_code.co_name

		#if this player is still playing
		if server_data != "SPECTATE":

			#split server data into variables
			players_position, game_map, bombs_position = Game.format_data(server_data)

			#send data to the Player class and receive an action
			self.ide_write = self.solve_function(players_position, game_map, bombs_position)
		else:
			#if the player is not able to play anymore
			self.console_log(function_name + ": You are spectating !")


