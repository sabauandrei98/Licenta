import random
import copy
from Helpers import Helpers

"""
	This class is unique for the current game
	being specific for the current game data format

	Can be adjusted for any other games
"""
class Game:

	"""
		This method receives a tokens list and prepares the initial data for the game
			- for security reasons, the map is generated on the server
			- this method combines a list of tokens and a generated map into a bigger packet
			- this packet will be sent to the each Unity client to be rendered

			@tokens_list: list<string>, a list containing unique identification codes

		return: string, rows of data separated by \n
	"""
	@staticmethod
	def initial_data(tokens_list):

		#compute the header of the packet
		packet = "INITIAL_DATA:\n"

		#generate the map and convert it to data rows (a string containing data separated by \n)
		game_map_data_rows = Helpers.char_matrix_to_data_rows(Game.generate_map())

		#convert the tokens list to data rows (a string containing data separated by \n)
		tokens_data_rows = Helpers.string_list_to_data_rows(tokens_list)
		
		#combine the data into a larger packet
		packet += tokens_data_rows + game_map_data_rows

		#log the packet
		print(packet)

		return packet


	"""
		This function formats a list of pairs into a data packet 
			@ide_commands_list: list<token: string, command: string>
				- this pair represents a mapping of form: a specific token executes a specific command

		return: string, rows of data separated by \n
	"""
	@staticmethod
	def pack_ide_data(ide_commands_list):

		#compute the header of the packet
		packet = "ROUND:\n"

		#format the list of commands
		for token, cmd in ide_commands_list:
			packet += token + "=" + cmd + '\n'
		
		#log the packet
		print (packet)

		return packet


	"""
		TO DO
	"""
	@staticmethod
	def generate_map():

		size_of_map = 16
		pumpkins = 40
		obstacles = 40
		
		game_map = [['_' for x in range(size_of_map)] for y in range(size_of_map)] 

		tries = 0
		max_tries = 1000
		while (obstacles > 0 or pumpkins > 0):
		
			x = random.randint(0, size_of_map - 1)
			y = random.randint(0, size_of_map - 1)

			if (game_map[x][y] == '_' and
				(x != 0 or y != 0) and (x != size_of_map - 1 or y != size_of_map - 1) and
				(x != size_of_map - 1 or y != 0) and (x != 0 or y != size_of_map - 1)):
			
				x_val = [x, size_of_map - 1 - x]
				y_val = [y, size_of_map - 1 - y]

				has_empty_positions = True
				for i in range(len(x_val)):
					for j in range(len(y_val)):
						if (game_map[x_val[i]][y_val[j]] != '_'):
							has_empty_positions = False

				if has_empty_positions:

					check_map = False
					if (obstacles > 0):
						check_map = True

					aux_map = copy.deepcopy(game_map)
					for i in range(len(x_val)):
						for j in range(len(y_val)):
							if (obstacles > 0):
								obstacles -= 1
								aux_map[x_val[i]][y_val[j]] = 'o'

					if check_map:
						if Game.can_fill_all_the_map(aux_map):
							game_map = copy.deepcopy(aux_map)
						else:
							obstacles += 4
							tries += 1
							aux_map = copy.deepcopy(game_map)

					if tries >= max_tries:
						obstacles = 0

					for i in range(len(x_val)):
						for j in range(len(y_val)):
								if (not check_map and pumpkins > 0):
									pumpkins -= 1
									game_map[x_val[i]][y_val[j]] = 'p'


		return game_map



	"""
	This method checks if a point is on the map or not
		@x: int, x coord
		@y: int, y coord
		@size: int, representing the zise of the map

		return: bool
	"""
	@staticmethod
	def is_on_map(x, y, size):
		if (x < 0 or y < 0 or x >= size or y >= size):
			return False
		return True

	"""
		This method checks if the map can be filled (has no dead ends)
			@map: list<string>, game map

		return: bool
	"""
	@staticmethod
	def can_fill_all_the_map(map):

		free_spaces = 0

		#calculating the empty spaces on the map
		for i in range(len(map)):
			for j in range(len(map[i])):
				if map[i][j] == '_' or map[i][j] == 'p':
					free_spaces += 1

		#using a stack to check iterative		
		stack = []

		#push point the starting position
		stack.append((0,0))

		#create a copy of the map
		aux_map = copy.deepcopy(map)

		filled_spaces = 0

		# 4 directions coords
		dx = [ 0, 1, -1, 0]
		dy = [ 1, 0, 0, -1]

		while len(stack) != 0:

			#get the element from the top of the stack
			x,y = stack.pop(0)

			#if the element is not visited
			if aux_map[x][y] != 'X':

				#mark it as visited
				aux_map[x][y] = 'X'

				#increment visited elements
				filled_spaces += 1

				#check the neighbours
				for i in range(4):

					#try to compute one neighbour position
					xx = dx[i] + x
					yy = dy[i] + y

					#if point on the map and it is free space or pumpkin we can pass through
					if (Game.is_on_map(xx, yy, len(aux_map)) and (aux_map[xx][yy] == '_' or aux_map[xx][yy] == 'p')):
						#add it to the stack
						stack.append((xx, yy))

		#if no more spaces left
		if (filled_spaces == free_spaces):
			#the map can be filled
			return True

		#the map cannot be filled
		return False


#Game.initial_data(["token1", "token2", "token3", "token4"])