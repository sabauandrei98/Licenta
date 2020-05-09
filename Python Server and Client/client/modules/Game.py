
class Game:

	@staticmethod
	def format_data(data):
		print("Data from server:\n" + data + "<END>")
		row_list = data.split('\n')
		
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

		print (str(players_position))
		print(str(game_map))
		print(str(bombs_position))

		return players_position, game_map, bombs_position