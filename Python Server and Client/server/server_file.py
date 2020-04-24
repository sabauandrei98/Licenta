import socket 
import threading
import time

from Client import Client

#connection
DEFAULT_HOST = "localhost"
DEFAULT_PORT = 50000
DEFAULT_LISTEN = 1
UNITY_TOKEN = "439b3a25b555b3bc8667a09a036ae70c"

RECV_TOKEN_TIMEOUT = 20.5
RECV_READY_RESPONSE_TIMEOUT = 20
RECV_SIZE_BYTES = 64

MAX_PLAYERS = 2

class Server:

	def __init__(self, host = None, port = None):
		self.host = DEFAULT_HOST if not host else host
		self.port = DEFAULT_PORT if not port else port

		self.all_unity_clients_ready = False
		self.all_ide_clients_ready = False

		#available tokens for clients
		'''TO DO: generate tokens randomly'''
		self.tokens  = ["token1", "token2"]

		#used tokens (avoid sending info to server on the same token on 2 different sockets)
		self.used_tokens = {key: False for key in self.tokens}

		self.clients = []


	def create_server_socket(self):
		try:
			self.s_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
			self.s_socket.bind((self.host, self.port))
			self.s_socket.listen(DEFAULT_LISTEN)
			print ("SERVER ON ! " + self.host + ":" + str(self.port))

		except Exception as error:
			print ("Could not create server socket: " + str(error))


	def has_valid_token(self, client_socket):
		
		try:
			#check the token from the client 
			#to verify if it is authorized to join this server
			client_socket.settimeout(RECV_TOKEN_TIMEOUT)
			client_token = client_socket.recv(RECV_SIZE_BYTES)

			if client_token in self.tokens and self.used_tokens[client_token] is False or client_token == UNITY_TOKEN:

				if client_token != UNITY_TOKEN:
					self.used_tokens[client_token] = True
				return True
			else:
				print ("Wrong token from the client OR already used in a connection !")
				return False

		except socket.timeout:
			print("This client has been disconnected due to timeout !")
			client_socket.close()

		except socket.error:
			print("Oops there was an socket error and connection was closed !")
			client_socket.close()

		return False


	def is_client_connected(self, client_socket):

		client_ip = ""
		client_port = ""
		try:
			client_ip = client_socket.getpeername()[0]
			client_port = client_socket.getpeername()[1]

		except Exception as error:
			print("Disconnected 1")
			return False


		try:
			fake_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
			fake_socket.connect((client_ip, client_port))
			fake_data = fake_socket.recv(16, socket.MSG_PEEK)

		except Exception as error:
			print("Disconnected 2" + str(error))
			return False


	def new_clients_manager(self):

		print ("New Clients Manager Started !\n")

		try:
			while True:
				#wait for a new player
				client_socket, addr = self.s_socket.accept()

				if self.has_valid_token(client_socket):

					#still in unity lobby
					if not self.all_unity_clients_ready:

						print("New Client connected !" + str(client_socket.getpeername()))

						client = Client()
						client.RECV_TIMEOUT = RECV_READY_RESPONSE_TIMEOUT
						client.start_new_socket_handler(client_socket, "unity_lobby")
						self.clients.append(client)

					else:
						#search for the token and link unity to one ide
						pass
				
		except Exception as e:
			print ("Exception :" + str(e))
			
		finally:
			self.s_socket.close()
			print("Server socket was closed !")



	def unity_lobby_manager(self):

		print("Unity Lobby Manager Started !\n")

		checked_clients = {}
		while not self.all_unity_clients_ready:

			connected = 0
			for client in self.clients:

				if client.unity_read == "READY!":
					connected += 1

			if connected == len(self.clients) and connected != 0:
				self.all_unity_clients_ready = True
				print("All unity clients ready !")
				self.clients[0].unity_socket.send("1salut" + "\n")
				self.clients[0].unity_socket.send("2salut" + "\n")

			time.sleep(0.1)


	def disconnected_manager(self):
		while True:
			print("Checking for disconnected players...")
			for client in self.clients:
				
				if self.is_client_connected(client.unity_socket) == False:
					print("SERVER: Client disconnected from the server !")

			time.sleep(3)


	def run(self):
		self.create_server_socket()
		time.sleep(0.5)

		#this thread will link the clients with the server
		new_clients_manager_th = threading.Thread(target = self.new_clients_manager, args = ())
		
		#this thread will check if one client has disconnected
		disconnected_manager_th = threading.Thread(target = self.disconnected_manager, args = ())

		#this thread will check if all the unity clients are ready (while in lobby)
		unity_lobby_manager_th = threading.Thread(target = self.unity_lobby_manager, args = ())

		#this thread will check if all the ide clients are ready 

		new_clients_manager_th.start()
		#disconnected_manager_th.start()
		#unity_lobby_manager_th.start()



s = Server()
s.run()