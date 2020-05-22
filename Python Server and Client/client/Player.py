from modules.ClientSocket import ClientSocket

"""
Player Class:
	- Clients are only allowed to modify what's inside the "solve" function!
	- You may add helper functions, as shown below 
	- Make sure all the Player methods are static

"""

class Player:

	@staticmethod
	def helper_function():
		pass

	"""
	This function receives a game session data, process it and returns an action
		@players_position: list<pair>, representing each player position
			- first item list is YOUR position
		@game_map: list<string>, representing the map
		@bombs_position: list<pair>, representing each bomb position
			- it may be empty

		return: string, representing an action eg: "MOVE 0 1", "BOMB"

		WARNING: make sure your output is valid, otherwise it won't be processed by the game
	"""
	@staticmethod
	def solve(players_position, game_map, bombs_position):

		#Insert your code here 

		for x, y in players_position:
			pass

		for row in game_map:
			pass

		if len(bombs_position):
			for x, y in bombs_position:
				pass


		return "MOVE 0 1"





#Connection info
SERVER_ADDRESS  = "localhost"
SERVER_PORT     = 50000
CLIENT_TOKEN    = "singleplayer"

#Connect to the server
c = ClientSocket(SERVER_ADDRESS, SERVER_PORT, CLIENT_TOKEN, Player.solve)



