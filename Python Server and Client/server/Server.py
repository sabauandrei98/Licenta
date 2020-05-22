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


	"""
		This function is repsonsible for creating the server socket 
	"""	
	def create_server_socket(self):
		try:

			#create a new tcp socket
			self.s_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

			#
			self.s_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)

			#bind it to an address and a port
			self.s_socket.bind((self.host, self.port))

			#
			self.s_socket.listen(self.DEFAULT_LISTEN)

			#console log
			print ("SERVER ON ! " + self.host + ":" + str(self.port))
		except Exception as error:

			#console log
			print ("Could not create server socket: " + str(error))


	"""
		This function is responsible for checking a new unity client connection
		and if the client passes the verification process, it is added to the server
			@client_socket: socket, unity socket
	"""
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


	"""
		This function is responsible for checking a new ide client connection
		and if the client passes the verification process, it is added to the server
			@client_socket: socket, ide socket
	"""
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


	"""
		This is function is the server clients listener
			- it accepts the clients
			- sorts the socket type
			- passes the socket for processing to a client handler
			- refuses connection if the server is full or the game is running
	"""
	def new_clients_manager(self):

		#server console log
		print ("New Clients Manager Started !\n")

		try:
			while True:

				#accept a new player
				client_socket, addr = self.s_socket.accept()

				try:

					#unity lobby phase
					if not self.all_unity_clients_ready and len(self.clients) < self.MAX_PLAYERS:
						#handle unity player to see if it is eligible
						self.handle_unity_client(client_socket)
					
					#ide linking phase
					if self.all_unity_clients_ready and not self.all_ide_clients_ready:
						#handle ide player to see if it is eligible
						self.handle_ide_client(client_socket)

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




	"""
		This function is running on a separate thread:
			- it contains a waterfall process that:
				- waits and checks if all the unity clients are ready
				- waits and checks if all the ide clients are ready
	"""
	def lobby_manager(self):

		#console log
		print("Unity Lobby Manager Started !\n")


		#checking if all the unity client are ready to play
		while not self.all_unity_clients_ready:

			#ready clients counter
			ready_clients = 0

			#lock the thread
			lock = threading.Lock()
			with lock:

				for client in self.clients:
					#check if client has no READY
					if not client.lobby_ready:
						#check for any unity READY response
						if client.unity_read == "READY":

							#label the client as ready
							client.lobby_ready = True

							#send an feedback
							client.unity_write = "READY OK"

							#increment the number of ready clients
							ready_clients += 1

							#if a client is ready as labeled, reset the socket timeout
							try:
								client.unity_socket.settimeout(self.WAIT_FOR_READY_CLIENT)
							except:
								# if the client cannot be reached, disconnect
								self.disconnect_one(client)

					#if client already ready, just count it
					else:
						ready_clients += 1

			#if all the unity clients are ready and there is at least one client
			#the game can start
			if ready_clients == len(self.clients) and ready_clients > 0:
				self.all_unity_clients_ready = True
				print("##### All unity clients ready ! ####\n")

			#sleep the thread
			time.sleep(1)


		#checking if all the ide client area ready to play
		while not self.all_ide_clients_ready:

			#ready client counter
			ready_ides = 0

			#lock the thread
			lock = threading.Lock()
			with lock:
				for client in self.clients:
					#if ide connected, count it
					if client.is_ide_connected:
						ready_ides += 1

			#if all the unity clients are ready and there is at least one client
			#the game can start
			if ready_ides == len(self.clients) and ready_ides > 0:
				self.all_ide_clients_ready = True

			#sleep the thread
			time.sleep(1)



	"""
		This function is running on a separate thread:
			- it sends the initial data to the clients
			- it ensures the following flows of data:
					unity -> server -> ide
					ide   -> server -> unity

			- if one of the client is not reachable, disconnect it
	"""
	def game_manager(self):

		while True:

			#run this only all the ides are ready -> the game is running
			if self.all_ide_clients_ready:

				#console log
				print("Game Manager Started !\n")

				#send initial data to unity 
				print("Generating initial data..")

				#locke the thread
				lock = threading.Lock()
				with lock:

					print("Generating and sending initial data..")
					for client in self.clients:
						try:
							#generate initial data and put in in the buffer to be sent to unity
							client.unity_write = Game.initial_data(self.tokens)
						except:
							#disconnect if client not reachable
							self.disconnect_one(client)


				print("Initial data sent !")

				while True:

					#initial data has been sent
					#now unity will create a data packet which will be sent to ide
					#wait for unity response
					time.sleep(self.WAIT_FOR_CLIENT)

					#UNITY -> SERVER -> IDE
					#now check if all unity clients sent back some info
					#disconnect if no data received from client

					#lock the thread
					lock = threading.Lock()
					with lock:
						for client in self.clients:
							try:
								#try to get a response from the unity
								if client.unity_read != "" and client.unity_read != ".":
									#send raw data to ide because there is no need for server to processing it
									client.ide_write = client.unity_read
									client.unity_read = "."
								else:
									#late response from player
									self.disconnect_one(client)
							except:
								self.disconnect_one(client)

					#at this point one flow is done
					#wait for ide response
					time.sleep(self.WAIT_FOR_CLIENT)


					#IDE -> SERVER -> UNITY
					ide_commands = []

					#lock the thread
					lock = threading.Lock()
					with lock:
						#collect the data from all ide (map the ide commands)
						for client in self.clients:
							try:
								#try to get and response from the ide
								if client.ide_read != "" and client.ide_read != ".":
									#append the response to the commands list
									ide_commands.append((client.ide_token, client.ide_read))
									client.ide_read = "."
								else:
									#late response from player
									self.disconnect_one(client)
							except:
								self.disconnect_one(client)

					#generate single packet to send to each unity client
					packet_to_send = Game.pack_ide_data(ide_commands)

					#send packet to each unity client
					for client in self.clients:
						client.unity_write = packet_to_send
						

	"""
		This function is running on a separate thread:
			- it checks if all the unity clients are ready to play
			- sends to each of them an individual token that will be used to connect the ide
	"""
	def link_tokens_manager(self):

		#run this regulatedly
		while True:

			#check if the unity clients are ready to play
			if self.all_unity_clients_ready:

				#console log
				print("Link Tokens Manager Started !\n")

				#lock the thread
				lock = threading.Lock()
				with lock:

					#generate some fancy tokens
					for index, client in enumerate(self.clients):
						self.tokens.append("token" + str(index))

					print("Tokens generated !\n")
					
					#link the tokens with the unity clients
					for index, client in enumerate(self.clients):
						#set the token to the client instance
						client.ide_token = self.tokens[index]

						#write the token on the buffer to be sent to the unity
						client.unity_write = "IDE_TOKEN:" + self.tokens[index]	

				print("Tokens linked !\n")	

				#exit the thread
				return		

			#sleep the thread
			time.sleep(1)


	"""
		This function disconnects a client
			@client: socket or instance of Client class
	"""
	def disconnect_one(self, client):

		#maybe this client tried to reconnect but never been added
		#to the list of clients, make sure we can remove it
		#client param can be Client class instance or socket 

		#lock the thread
		lock = threading.Lock()
		with lock:
			try:
				#check if the client can be found on the clients list
				if client in self.clients:
					self.clients.remove(client)

					if client.unity_socket != None:
						client.unity_socket.shutdown(socket.SHUT_RDWR)
						client.unity_socket.close()
					if client.ide_socket != None:
						client.ide_socket.shutdown(socket.SHUT_RDWR)
						client.ide_socket.close()

				#this is a socket, not a client
				else:
					client.shutdown(socket.SHUT_RDWR)
					client.close()

			except Exception as ex:
				pass

			#if the game is running and there are no clients left, close the server
			if len(self.clients) == 0 and self.all_unity_clients_ready:
				self.close_the_server()

		print ("One client disconnected !")


	"""
		This function is responsible for removing all the clients from the server
	"""
	def disconnect_all(self):

		#while there are clients on the server
		while len(self.clients) > 0:

			#disconnect first client from the list of clients
			self.disconnect_one(self.clients[0])

		#console log
		print ("All clients disconnected !")



	"""
		This function is running on a separate thread:
			- it regulatedly checks if any client is labeled as disconnected
			- if so, this function removes the client from the server
	"""
	def disconnected_manager(self):

		#console log
		print ("Disconnected Manager Started ! \n")

		#run this regulatedly
		while True:

			#lock the thread
			lock = threading.Lock()
			with lock:

				#check if any client is labeled as disconnected
				for client in self.clients:
					if client.is_connected == False:

						#disconnect that client
						self.disconnect_one(client)		

			#put the thread to sleep	
			time.sleep(1)


	"""
		This function is responsible for closing the server connection safely
	"""
	def close_the_server(self):

		#check if there are any clients left on the server
		if len(self.clients) != 0:
			#disconnect that clients
			self.disconnect_all()

		#try to close the server socket
		try:
			self.s_socket.close()
		except:
			print ("Error while closing the server !")	

		print("Server socket closed !")

		#stop the application, closing all the remaining running threads
		os._exit(1)



	"""
		This is the start function on the server class
			- it calls the server creation function
			- creates and runs the principal threads
	"""
	def run(self):

		#create the server
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

		#start all the threads
		new_clients_manager_th.start()
		disconnected_manager_th.start()
		lobby_manager_th.start()
		link_tokens_manager_th.start()
		game_manager_th.start()
		

"""
	This is the main function of the server
		- splits the command line arguments
		- creates a specific server: with or without arguments
"""
def main():

	#console log the arguments
	print("Sys Args: " + str(sys.argv))

	#if only the file argument was given
	file_argument = 1

	#if the file, ip, port arguments were given
	file_ip_port_arguments = 3

	#check the arguments length and start a specific server
	if len(sys.argv) == file_argument:
		s = Server()
		s.run()
	elif len(sys.argv) == file_ip_port_arguments:

		#try to start the server with the given arguments
		try:
			s = Server(sys.argv[1], int(sys.argv[2]))
			s.run()
		except:
			print("Server could not be started !")
	else:
		print("Server could not be started !")


#call the main function
main()