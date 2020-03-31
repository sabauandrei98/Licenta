import socket 
import thread
import time

class Server:

	def __init__(self, host = None, port = None):

		if host is None:
			self.host = "127.0.0.1"
		else:
			self.host = host

		if port is None:
			self.port = 10000
		else:
			self.port = port


	def create_server_socket(self):
		self.s_socket = socket.socket()
		self.s_socket.bind((self.host, self.port))
		self.s_socket.listen(5)

	def client_handler(self, clientsocket, addr):
		send_time_ms = time.time()
		recv_time_ms = time.time()

		while True:
			try:
				#wait info from client
				clientsocket.settimeout(5.0)
				msg = clientsocket.recv(1024)
				#save the recive time
				recv_time_ms = time.time()
				#calculate delta
				rtt_in_s = round(recv_time_ms - send_time_ms, 3)

				#debug 
				print ("Message: " + msg + " || Address: " + str(addr) + " || Passed: " + str(rtt_in_s))

				#send some message
				msg = "Message received by the server"
				clientsocket.send(msg)

				#save the send time
				send_time_ms = time.time()

			except socket.timeout:
				print("This client has been disconnected due timeout !")

			except socket.error:
				print("Oops there was an socket error and connection was closed !")

			finally:
				clientsocket.close()
				print("This socket was closed !")

	def run(self):
		self.create_server_socket()

		print 'SERVER ON !'
		print 'Waiting for clients...'

		try:
			while True:
				#wait for a new player
				c, addr = self.s_socket.accept()

				#handle the player on a different thread
				thread.start_new_thread(self.client_handler, (c, addr))

				print("New player conneted: " + str(addr))

		except Exception as e:
			print ("Exception :" + str(e))
			
		finally:
			self.s_socket.close()
			print("Server socket was closed !")

s = Server()
s.run()