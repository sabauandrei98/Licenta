import random
import copy
from DFS import DFS
from Helpers import Helpers

class Game:

	@staticmethod
	def initial_data(tokens_list):
		game_map_data_rows = Helpers.char_matrix_to_data_rows(Game.generate_map())
		tokens_data_rows = Helpers.string_list_to_data_rows(tokens_list)
		
		print(tokens_data_rows + game_map_data_rows)

		return tokens_data_rows + game_map_data_rows

	@staticmethod
	def generate_map():

		size_of_map = 16
		pumpkins = 20
		obstacles = 80
		
		game_map = [['_' for x in range(size_of_map)] for y in range(size_of_map)] 

		while (obstacles > 0 or pumpkins > 0):
		
			x = random.randint(0, size_of_map - 1)
			y = random.randint(0, size_of_map - 1)

			if (game_map[x][y] == '_' and
				(x != 0 or y != 0) and (x != size_of_map - 1 or y != size_of_map - 1) and
				(x != size_of_map - 1 or y != 0) and (x != 0 or y != size_of_map - 1)):
			
				x_val = [x, size_of_map - 1 - x]
				y_val = [y, size_of_map - 1 - y]

				ok = True
				for i in range(2):
					for j in range(2):
						if (game_map[x_val[i]][y_val[j]] != '_'):
							ok = False

				if ok:
					aux_map = copy.deepcopy(game_map)
					for i in range(2):
						for j in range(2):
							if (obstacles > 0):
								obstacles -= 1
								aux_map[x_val[i]][y_val[j]] = 'o'


					if DFS.can_fill_all_the_map(aux_map):
						game_map = copy.deepcopy(aux_map)
					else:
						obstacles += 4
						aux_map = copy.deepcopy(game_map)


					for i in range(2):
						for j in range(2):
								if (obstacles == 0 and pumpkins > 0):
									pumpkins -= 1
									game_map[x_val[i]][y_val[j]] = 'p'


		return game_map
	

Game.initial_data(["a", "b", "c", "d"])