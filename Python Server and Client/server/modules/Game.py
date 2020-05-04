import random
from DFS import DFS

class Game:

	@staticmethod
	def initial_data(tokens_list):
		self.tokens_list = tokens_list
		self.players_number = len(self.tokens_list)
		data = "INITIAL_DATA:"
		return data

	@staticmethod
	def generate_map():

		size_of_map = 16
		pumpkins = 20
		obstacles = 32
		
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
					for i in range(2):
						for j in range(2):
							if (obstacles > 0):
								obstacles -= 1
								game_map[x_val[i]][y_val[j]] = 'o'
							else:
								if (pumpkins > 0):
									pumpkins -= 1
									game_map[x_val[i]][y_val[j]] = 'p'


		return game_map
	



#test the map in the console (for now)
game_map = Game.generate_map()

result = ""
for i in range(16):
	for j in range(16):
		result += game_map[i][j] + " "
	result += "\n"

print(result)
DFS.can_fill_all_the_map(game_map)


	