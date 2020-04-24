import socket 
import threading
import time
import sys

class Client:

	def __init__(self):

		self.RECV_TIMEOUT = 2.5
		self.RECV_SIZE_BYTES = 64

		self.unity_socket = None
		self.ide_socket = None

		self.unity_read = ""
		self.unity_write = ""

		self.ide_read = ""
		self.ide_write = ""


	def start_new_socket_handler(self, client_socket, client_type):

		if client_type == "unity" or client_type == "unity_lobby":
			self.unity_socket = client_socket

		if client_type == "ide":
			self.ide_socket = client_socket

		if client_type == "unity_lobby":
			new_thread = threading.Thread(target = self.unity_lobby_handler, args = (client_socket, ))
			new_thread.start()
		else:
			new_thread = threading.Thread(target = self.client_handler, args = (client_socket, client_type))
			new_thread.start()


	def unity_lobby_handler(self, client_socket):

		function_name = sys._getframe().f_code.co_name

		try:
			
			self.console_log(function_name + ": Waiting to read...")
			client_socket.settimeout(self.RECV_TIMEOUT)
			self.unity_read = client_socket.recv(self.RECV_SIZE_BYTES).rstrip('\r\n')

			if (len(self.unity_read) == 0):
				raise Exception("Connection with the unity client dropped !")

			self.console_log(function_name + ": Message read !")

			for writings in range(2):
				self.console_log(function_name + ": Waiting for server to write ...")
				while self.unity_write == "":
					time.sleep(0.1)

				client_socket.send(self.unity_write)
				self.console_log(function_name + ": Message sent to unity !")

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
					client_socket.settimeout(self.RECV_TIMEOUT)
					self.unity_read = client_socket.recv(self.RECV_SIZE_BYTES).rstrip('\r\n')

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

