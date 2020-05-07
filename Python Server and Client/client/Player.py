from modules.ClientSocket import ClientSocket

class Player:

	@staticmethod
	def solve(param):

		#You are only allowed to modify the "solve" function !

		cmd = "MOVE 1 1"
		print ("Solver function: " + cmd)





		#The return type MUST be a string describing an action
		#eg: "MOVE X Y"
		#	 "BOMB"
		return cmd




#connect to server info
SERVER_ADDRESS  = "localhost"
SERVER_PORT     = 50000
CLIENT_TOKEN    = "token1"

#connect to server
c = ClientSocket(SERVER_ADDRESS, SERVER_PORT, CLIENT_TOKEN, Player.solve)



