import socket 
import thread
import time

#connection
DEFAULT_HOST = "localhost"
DEFAULT_PORT = 50000
DEFAULT_LISTEN = 5

RECV_TIMEOUT = 2.5
RECV_SIZE_BYTES = 64

MAX_PLAYERS = 2

class Server:

	def __init__(self, host = None, port = None):
		self.host = DEFAULT_HOST if not host else host
		self.port = DEFAULT_PORT if not port else port

		#available tokens for clients
		'''TO DO: generate tokens randomly'''
		self.tokens  = ["token1", "token2"]

		#used tokens (avoid sending info to server on the same token on 2 different sockets)
		self.used_tokens = {key: False for key in self.tokens}


	def create_server_socket(self):
		self.s_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
		self.s_socket.bind((self.host, self.port))
		self.s_socket.listen(DEFAULT_LISTEN)


	def client_handler(self, client_socket, addr):

		client_token = ""
		successfully_connected = False

		try:
			#check the token from the client 
			#to verify if it is authorized to join this server
			client_socket.settimeout(RECV_TIMEOUT)
			client_token = client_socket.recv(RECV_SIZE_BYTES).rstrip('\r\n')

			print(client_token)
			if client_token in self.tokens and self.used_tokens[client_token] is False:
				self.used_tokens[client_token] = True
				client_socket.send("Ok!")
			else:
				raise Exception ("Wrong token from the client OR already used in a connection !")

			#at this point, the client successfully connected to the server
			print("The token from this client was successfully verified !")

			successfully_connected = True

			while True:
				client_socket.send("this is a string")
				send_time_ms = time.time()

				msg = client_socket.recv(RECV_SIZE_BYTES)
				recv_time_ms = time.time()
				
				rtt_in_s = round(recv_time_ms - send_time_ms, 3)

				#debug 
				print (client_token + ": " + msg + " || " + str(addr) + " || " + str(rtt_in_s)) + " seconds"

		except socket.timeout:
			print("This client has been disconnected due timeout !")

		except socket.error:
			print("Oops there was an socket error and connection was closed !")

		except Exception as warning:
			print (warning)

		finally:
			client_socket.close()

			#if client was connected to server, free the socket on that token
			if successfully_connected:
				self.used_tokens[client_token] = False

			print("This socket was closed ! A new player can reconnect on this socket and token !")

	def run(self):
		self.create_server_socket()

		print ("SERVER ON ! Waiting for clients ...")

		try:
			while True:
				#wait for a new player
				c, addr = self.s_socket.accept()

				#handle the player on a different thread
				thread.start_new_thread(self.client_handler, (c, addr))

				print("New player tries to connect: " + str(addr) + '\n')

		except Exception as e:
			print ("Exception :" + str(e))
			
		finally:
			self.s_socket.close()
			print("Server socket was closed !")

s = Server()
s.run()