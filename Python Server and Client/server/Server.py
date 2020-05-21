import socket 
import threading
import time
import os
import sys
from modules.Client import Client
from modules.Game import Game


class Server:

	def __init__(self, host = None, port = None):

		#default connection details
		self.DEFAULT_HOST = ''
		self.DEFAULT_PORT = 50000
		self.DEFAULT_LISTEN = 5

		#specific game variables used by the server
		self.UNITY_TOKEN 				  = Game.get_unity_token();
		self.RECV_SIZE_BYTES			  = Game.get_buffer_size()
		self.MAX_PLAYERS 		          = Game.get_max_players()
		self.WAIT_FOR_CLIENT              = Game.wait_for_client()
		self.WAIT_FOR_READY_CLIENT        = Game.wait_for_ready_client()
		self.WAIT_FOR_CLIENT_TO_GET_READY = Game.wait_for_client_to_get_ready()

		#address the server will run on
		self.host = self.DEFAULT_HOST if not host else host

		#port the server will run on
		self.port = self.DEFAULT_PORT if not port else port

		#connection state bools
		self.all_unity_clients_ready = False
		self.all_ide_clients_ready = False

		#list of clients, each client packing a unity and an ide socket
		self.clients = []

		#list of tokens that are used on the server
		self.tokens = []


	def create_server_socket(self):
		try:
			self.s_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
			self.s_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
			self.s_socket.bind((self.host, self.port))
			self.s_socket.listen(self.DEFAULT_LISTEN)
			print ("SERVER ON ! " + self.host + ":" + str(self.port))
		except Exception as error:
			print ("Could not create server socket: " + str(error))


	def handle_unity_client(self, client_socket):
				
		#set a timeout
		client_socket.settimeout(self.WAIT_FOR_CLIENT)

		#receive the token from the client
		client_token = client_socket.recv(self.RECV_SIZE_BYTES)

		#if no answer from the client, close the connection
		if (client_token == ""):
			raise Exception("Client lost connection to the server !")

		#if the token is valid
		if client_token == self.UNITY_TOKEN:
			#send client feedback
			client_socket.send("TOKEN OK")

			#reset the socket timeout 
			client_socket.settimeout(self.WAIT_FOR_CLIENT_TO_GET_READY)
		else:
			#send client feedback
			client_socket.send("Wrong token from the client !")

			#raise exception which will close the connection
			raise Exception ("Wrong token from the client !")

		#server console log
		print("New Client connected ! Verified token !" + str(client_socket.getpeername()))

		#create a client instance
		client = Client(client_socket)

		#start a new socket handler which will create threads for sending and receiving data
		client.start_new_socket_handler(client_socket, "unity")

		#update client connection status and add it to the list of clients
		client.is_connected = True
		self.clients.append(client)


	def handle_ide_client(self, client_socket):

		#search for the token and link unity session to an ide session inside client class
		ide_token = client_socket.recv(self.RECV_SIZE_BYTES)

		#if no answer from the client, close the connection
		if (ide_token == ""):
			raise Exception("Client lost connection to the server !")

		else:
			client_found = False

			#iterate through clients list to see if any unity session has the corresponding token
			for client in self.clients:

				#if token was found and there is no ide already connected
				if client.ide_token == ide_token and not client.is_ide_connected:
					client_found = True

					#link the socket to the Client class
					client.ide_socket = client_socket

					#change ide connection status
					client.is_ide_connected = True

					#send client feedback
					client.ide_socket.send("TOKEN OK")

					#server console log
					print ("Ide linked with Unity and ready to play !")

					#start a new socket handler which will create threads for sending and receiving data
					client.start_new_socket_handler(client_socket, "ide")
					
			#if no client found
			if not client_found:
				#send client feedback
				client_socket.send("No unity session with this token !")

				#raise exception which will close the connection
				raise Exception ("No unity session with this token !")


	def new_clients_manager(self):
		print ("New Clients Manager Started !\n")

		try:
			while True:

				#wait for a new player
				client_socket, addr = self.s_socket.accept()

				try:

					#unity lobby phase
					if not self.all_unity_clients_ready and len(self.clients) < self.MAX_PLAYERS:
						self.handle_unity_client(client_socket)
					
					#ide linking phase
					if self.all_unity_clients_ready and not self.all_ide_clients_ready:
						self.handle_ide_client()

					#game running phase
					if self.all_ide_clients_ready:
						client_socket.send("Client tried to connect while the game is running")
						raise Exception("Client tried to connect while the game is running !")

					#server is full
					if not self.all_unity_clients_ready and len(self.clients) == self.MAX_PLAYERS:
						client_socket.send("The server is full !")
						raise Exception("The server is full !")

				#handle possible client exceptions and close the connection
				except Exception as exception:
					print ("Socket: " + str(client_socket.getpeername()) + " disconnected due to: " + str(exception))
					self.disconnect_one(client_socket)

		
		#handle possible server exceptions and close the server
		except Exception as e:
			print ("SERVER EXCEPTION :" + str(e))
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

						try:
							client.unity_socket.settimeout(self.WAIT_FOR_READY_CLIENT)
						except:
							self.disconnect_one(client)
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
					try:
						client.unity_write = Game.initial_data(self.tokens)
						#sockets time
					except:
						self.disconnect_one(client)

				print("Initial data generated !")

				while True:

					#initial data has been sent
					#now unity will create a data packet which will be sent to ide
					#wait for unity response
					time.sleep(self.WAIT_FOR_CLIENT)

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

					#at this point one flow is done
					#wait for ide response
					time.sleep(self.WAIT_FOR_CLIENT)

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
				for index, client in enumerate(self.clients):
					self.tokens.append("token" + str(index))

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
		

def main():
	print("Sys Args: " + str(sys.argv))

	file_argument = 1
	file_ip_port_arguments = 3

	if len(sys.argv) == file_argument:
		s = Server()
		s.run()
	elif len(sys.argv) == file_ip_port_arguments:
		try:
			s = Server(sys.argv[1], int(sys.argv[2]))
			s.run()
		except:
			print("Server could not be started !")
	else:
		print("Server could not be started !")

main()