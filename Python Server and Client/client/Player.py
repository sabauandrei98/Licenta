from modules.ClientSocket import ClientSocket

class Player:

	@staticmethod
	def helper_function():
		pass

	@staticmethod
	def solve(players_position, game_map, bombs_position):

		#You are only allowed to modify what's inside "solve" function!
		#Feel free to add your desires @staticmethod


		return "MOVE 0 1"




#connect to server info
SERVER_ADDRESS  = "localhost"
SERVER_PORT     = 50000
CLIENT_TOKEN    = "token0"

#connect to server
c = ClientSocket(SERVER_ADDRESS, SERVER_PORT, CLIENT_TOKEN, Player.solve)



