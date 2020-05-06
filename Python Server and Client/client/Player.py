from modules.ClientSocket import ClientSocket


class Player:

	@staticmethod
	def solve(param):
		print ("Solver function: " + param)
		return param





def connect_to_server():
	#connect to server info
	SERVER_ADDRESS  = "localhost"
	SERVER_PORT     = 50000
	CLIENT_TOKEN    = "token1"

	#connect to server
	c = ClientSocket(SERVER_ADDRESS, SERVER_PORT, CLIENT_TOKEN, Player.solve)
	
connect_to_server()

