import socket 
import threading
import time
import os
from modules.Client import Client
from modules.Game import Game

#connection
DEFAULT_HOST = ''
DEFAULT_PORT = 50000
DEFAULT_LISTEN = 5
RECV_SIZE_BYTES = 512
MAX_PLAYERS = 4

WAIT_FOR_CLIENT_TIME = 1

class Server:

	def __init__(self, host = None, port = None):
		self.host = DEFAULT_HOST if not host else host
		self.port = DEFAULT_PORT if not port else port
		self.all_unity_clients_ready = False
		self.all_ide_clients_ready = False
		self.clients = []
		self.tokens = []


	def create_server_socket(self):
		try:
			self.s_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
			self.s_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
			self.s_socket.bind((self.host, self.port))
			self.s_socket.listen(DEFAULT_LISTEN)
			print ("SERVER ON ! " + self.host + ":" + str(self.port))
		except Exception as error:
			print ("Could not create server socket: " + str(error))


	def new_clients_manager(self):
		print ("New Clients Manager Started !\n")

		try:
			while True:
				#wait for a new player
				client_socket, addr = self.s_socket.accept()

				#IN UNITY LOBBY
				if not self.all_unity_clients_ready and len(self.clients) < MAX_PLAYERS:
					client = Client(client_socket)
					if (client.has_valid_token(client_socket, client.UNITY_TOKEN)):
						print("New Client connected ! Verified token !" + str(client_socket.getpeername()))
						client.is_connected = True
						client.start_new_socket_handler(client_socket, "unity")
						self.clients.append(client)
				
				#IN READY MODE, WAITING FOR IDE TO CONNECT
				if self.all_unity_clients_ready and not self.all_ide_clients_ready:
					#search for the token and link unity to one ide
					ide_token = client_socket.recv(RECV_SIZE_BYTES)
					if (ide_token == ""):
						self.disconnect_one(client_socket)
					else:
						client_found = False
						for client in self.clients:
							if client.ide_token == ide_token and not client.is_ide_connected:
								client_found = True
								client.ide_socket = client_socket
								client.is_ide_connected = True
								client.ide_socket.send("Token verified ! You are now connected to the server !")
								client.start_new_socket_handler(client_socket, "ide")
								print ("Ide linked with Unity and ready to play !")

						if not client_found:
							client_socket.send("GAME RUNNING")
							self.disconnect_one(client_socket)

				#IN THE GAME, REJECT ANY CONNECTION
				if self.all_ide_clients_ready:
				   client_socket.send("GAME RUNNING")
				   print("A client tried to connect while playing !")
				   self.disconnect_one(client_socket)
				
		except Exception as e:
			print ("Exception :" + str(e))
		finally:
			self.close_the_server()


	def lobby_manager(self):
		print("Unity Lobby Manager Started !\n")

		while not self.all_unity_clients_ready:
			ready_clients = 0
			for client in self.clients:
				if not client.lobby_ready:
					if client.unity_read == "READY":
						client.lobby_ready = True
						client.unity_write = "READY OK"
						ready_clients += 1
				else:
					ready_clients += 1

			if ready_clients == len(self.clients) and ready_clients > 0:
				self.all_unity_clients_ready = True
				print("##### All unity clients ready ! ####\n")

			time.sleep(1)

		while not self.all_ide_clients_ready:
			ready_ides = 0
			for client in self.clients:
				if client.is_ide_connected:
					ready_ides += 1

			if ready_ides == len(self.clients) and ready_ides > 0:
				self.all_ide_clients_ready = True

			#if no player left, close the server
			if len(self.clients) == 0:
				self.close_the_server()

			time.sleep(1)


	def game_manager(self):

		while True:

			if self.all_ide_clients_ready:
				print("Game Manager Started !\n")

				#send initial data to unity 
				print("Generating initial data..")
				for client in self.clients:
					client.unity_write = Game.initial_data(self.tokens)
				print("Initial data generated !")

				while True:

					#initial data has been sent
					#now unity will create a data packet which will be sent to ide
					#wait for unity response
					time.sleep(WAIT_FOR_CLIENT_TIME)

					#if no player left, close the server
					if len(self.clients) == 0:
						self.close_the_server()

					#UNITY -> SERVER -> IDE
					#now check if all unity clients sent back some info
					#disconnect if no data received from client

					for client in self.clients:
						if client.unity_read != "" and client.unity_read != ".":
							#send raw data to ide because there is no need for server to processing it
							client.ide_write = client.unity_read
							client.unity_read = "."
						else:
							#late response from player
							self.disconnect_one(client)
							#if no player left, close the server
							if len(self.clients) == 0:
								self.close_the_server()


					#at this point one flow is done
					#wait for ide response
					time.sleep(WAIT_FOR_CLIENT_TIME)

					#if no player left, close the server
					if len(self.clients) == 0:
						self.close_the_server()

					#IDE -> SERVER -> UNITY
					ide_commands = []

					#collect the data from all ide (map the ide commands)
					for client in self.clients:
						if client.ide_read != "" and client.ide_read != ".":
							ide_commands.append((client.ide_token, client.ide_read))
							client.ide_read = "."
						else:
							#late response from player
							self.disconnect_one(client)
							#if no player left, close the server
							if len(self.clients) == 0:
								self.close_the_server()

					#generate single packet to send to each unity client
					packet_to_send = Game.pack_ide_data(ide_commands)

					#send packet to each unity client
					for client in self.clients:
						client.unity_write = packet_to_send
						

	def link_tokens_manager(self):

		while True:
			if self.all_unity_clients_ready:

				print("Link Tokens Manager Started !\n")

				#generate some fancy tokens
				self.tokens = ["token1"]

				print("Tokens generated !\n")
			
				for index, client in enumerate(self.clients):
					client.ide_token = self.tokens[index]
					client.unity_write = "IDE_TOKEN:" + self.tokens[index]	

				print("Tokens linked !\n")	
				return		

			time.sleep(1)


	def disconnect_one(self, client):

		#maybe this client tried to reconnect but never been added
		#to the list of clients, make sure we can remove it
		#client param can be Client class instance or socket 

		try:
			if client in self.clients:
				self.clients.remove(client)

				if client.unity_socket != None:
					client.unity_socket.shutdown(socket.SHUT_RDWR)
					client.unity_socket.close()
				if client.ide_socket != None:
					client.ide_socket.shutdown(socket.SHUT_RDWR)
					client.ide_socket.close()
			else:
				#this is a socket, not a client
				client.shutdown(socket.SHUT_RDWR)
				client.close()

		except Exception as ex:
			print ("Client could not be disconnected !" + str(ex))
			return

		print ("One client disconnected !")

	def disconnect_all(self):
		for client in self.clients:
			self.disconnect_one(client)
		print ("All clients disconnected !")


	def disconnected_manager(self):
		print ("Disconnected Manager Started ! \n")
		while True:
			for client in self.clients:
				if client.is_connected == False:
					self.disconnect_one(client)				
			time.sleep(1)


	def close_the_server(self):
		if len(self.clients) != 0:
			self.disconnect_all()
		try:
			self.s_socket.close()
		except:
			print ("Error while closing the server !")	
		print("Server socket closed !")
		os._exit(1)


	def run(self):
		self.create_server_socket()

		#this thread will check for new players
		new_clients_manager_th = threading.Thread(target = self.new_clients_manager, args = ())

		#this thread will check if one client has disconnected
		disconnected_manager_th = threading.Thread(target = self.disconnected_manager, args = ())

		#this thread will check if all the unity & ide clients are ready (while in lobby)
		lobby_manager_th = threading.Thread(target = self.lobby_manager, args = ())

		#this thread will generate and send tokens to unity which will be used by the ides to connect
		link_tokens_manager_th = threading.Thread(target = self.link_tokens_manager, args = ())

		#this thread will first send the initial data for each player to the unity
		#then will manage the flow unity -> server -> ide and backwards
		game_manager_th = threading.Thread(target = self.game_manager, args = ())

		new_clients_manager_th.start()
		disconnected_manager_th.start()
		lobby_manager_th.start()
		link_tokens_manager_th.start()
		game_manager_th.start()
		

s = Server()
s.run()