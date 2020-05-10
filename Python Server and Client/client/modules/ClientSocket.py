import socket
import sys
import time
import threading
from Game import Game

RECV_SIZE_BYTES = 512

class ClientSocket:

	def __init__(self, server_address, port, token, solve_function):
		self.server_address = server_address
		self.port			= port
		self.token 			= token
		self.solve_function = solve_function
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
			print("You were successfully verified by the server !")
			
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
				self.console_log(function_name + ": Waiting to read from server...")
				self.ide_read = self.client_socket.recv(RECV_SIZE_BYTES)

				if (self.ide_read == ""):
					raise Exception(": Connection with the unity client dropped !")

				self.console_log(function_name + ": Message read from server ! Msg:\n" + self.ide_read + "<END>")
				self.get_player_response(self.ide_read)
					
		except socket.timeout:
			self.console_log(function_name + ": This client has been disconnected due to timeout !")
		except socket.error:
			self.console_log(function_name + ": Oops there was an socket error and connection was closed !")
		except Exception as e:
			self.console_log(function_name + str(e))
		finally:
			self.client_socket.shutdown(socket.SHUT_RDWR)
			self.client_socket.close()


	def socket_writer(self):
		function_name = sys._getframe().f_code.co_name
		try:
			while True:
				self.console_log(function_name + ": Waiting for client to write to server ...")
				while self.ide_write == "":
					time.sleep(0.1)

				self.client_socket.send(self.ide_write)
				self.console_log(function_name + ": Message sent to server ! Msg:\n" + self.ide_write + "<END>")

				self.ide_write = ""
					
		except socket.timeout:
			self.console_log(function_name + ": This client has been disconnected due to timeout !")
		except socket.error:
			self.console_log(function_name + ": Oops there was an socket error and connection was closed !")
		except Exception as e:
			self.console_log(function_name + str(e))
		finally:
			self.client_socket.shutdown(socket.SHUT_RDWR)
			self.client_socket.close()

	def get_ide_address(self):
			try:
				if(self.client_socket != None):
					return str(self.client_socket.getpeername())
			except:
				pass
			return ""

	def console_log(self, message):
		print ("Connection:" + self.get_ide_address() + " " + message)

	def get_player_response(self, server_data):
		print("SERVER DATA:" + server_data)
		if server_data != "SPECTATE":
			players_position, game_map, bombs_position = Game.format_data(server_data)
			self.ide_write = self.solve_function(players_position, game_map, bombs_position)
		else:
			print("You are spectating !")


