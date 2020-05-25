"""
	This class is unique for the current game
	being specific for the current game data format

	Can be adjusted for any other games
"""
class Game:

	@staticmethod
	def get_buffer_size():
		return 512

	"""
	This function splits the data from the server into variables
		@server_data: string, representing rows, separation token: \n

		return: touple, representing:
				list<pair>   - players position
				list<string> - map rows
				list<pair>   - bombs position
	"""
	@staticmethod
	def format_data(server_data):
		print("Data from server:\n" + server_data + "<END>")
		row_list = server_data.split('\n')
		
		players_position = []
		game_map = []
		bombs_position = []	
		read_players_position = True

		for row in row_list:
			if len(row) > 0:
				#first read players positions
				if len(row) < 8 and read_players_position:
					coords = row.split(' ')
					players_position.append((int(coords[0]), int(coords[1])))
				#then read map data
				elif len(row) > 8:
					read_players_position = False
					game_map.append(row)
				#then read bombs positions
				elif len(row) < 8 and not read_players_position:
					coords = row.split(' ')
					bombs_position.append((int(coords[0]), int(coords[1])))

		#return the data topule
		return players_position, game_map, bombs_position