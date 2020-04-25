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
		self.RECV_TOKEN_TIMEOUT = 100
		self.RECV_TIMEOUT = 200

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


	def start_new_socket_handler(self, client_socket, client_type):
		if client_type == "unity":
			new_reader = threading.Thread(target = self.unity_reader, args = (client_socket, ))
			new_writer = threading.Thread(target = self.unity_writer, args = (client_socket, ))
			new_reader.start()
			new_writer.start()
		
		if client_type == "ide":
			new_thread = threading.Thread(target = self.client_handler, args = (client_socket, client_type))
			new_thread.start()


	def unity_reader(self, client_socket):
		function_name = sys._getframe().f_code.co_name
		err = False

		try:
			while True:
				self.console_log(function_name + ": Waiting to read...")
				self.unity_read = client_socket.recv(self.RECV_SIZE_BYTES)

				if (self.unity_read == ""):
					raise Exception("Connection with the unity client dropped !")

				self.console_log(function_name + ": Message read ! " + self.unity_read)
					
		except socket.timeout:
			self.console_log(function_name + ": This client has been disconnected due to timeout !")
			err = True
			
		except socket.error:
			self.console_log(function_name + ": Oops there was an socket error and connection was closed !")
			err = True

		except Exception as e:
			print ("Exception :" + str(e))
			err = True

		if err:
			print("Shut down socket !")
			client_socket.shutdown(socket.SHUT_RDWR)
			client_socket.close()

	def unity_writer(self, client_socket):
		function_name = sys._getframe().f_code.co_name

		try:
			while True:

					self.console_log(function_name + ": Waiting for server to write ...")
					while self.unity_write == "":
						time.sleep(0.1)

					client_socket.send(self.unity_write)
					self.console_log(function_name + ": Message sent to unity ! " + self.unity_write)

					self.unity_write = ""
					
		except socket.timeout:
			self.console_log(function_name + ": This client has been disconnected due to timeout !")
			client_socket.close()

		except socket.error:
			self.console_log(function_name + ": Oops there was an socket error and connection was closed !")
			client_socket.close()

		except Exception as e:
			print ("Exception :" + str(e))
			client_socket.close()


	def client_handler(self, client_socket, client_type):

		function_name = sys._getframe().f_code.co_name

		try:
			while True:

				if (client_type == "unity"):

					self.console_log(function_name + ": Waiting to read...")
					client_socket.settimeout(RECV_TIMEOUT)
					self.unity_read = client_socket.recv(RECV_SIZE_BYTES).rstrip('\r\n')

					if (len(self.unity_read) == 0):
						raise Exception("Connection with the unity client dropped !")

					self.console_log(function_name + ": Message read !")

					self.console_log(function_name + ": Waiting for server to write ...")
					while self.unity_write == "":
						time.sleep(0.1)

					client_socket.send(self.unity_write)
					self.console_log(function_name + ": Message sent to unity !")

					self.unity_write = ""

				else:
					self.ide_read = client_socket.recv(RECV_SIZE_BYTES)

					while self.ide_write == "":
						time.sleep(0.1)

					print("Message sent to ide !")
					client_socket.send(self.ide_write)
					self.ide_write = ""
				
		except socket.timeout:
			print("This client has been disconnected due to timeout !")

		except socket.error:
			print("Oops there was an socket error and connection was closed !")

		except Exception as e:
			print ("Exception :" + str(e))

		finally:
			client_socket.close()

			print("This socket was closed ! A new unity client can reconnect on this socket!")


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

