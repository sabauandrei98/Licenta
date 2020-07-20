from modules.ClientSocket import ClientSocket
import math
import copy

"""
Player Class:
	- Clients are only allowed to modify what's inside the class Player
	- You may add helper functions, as shown below 

"""
class Player:

	def __init__(self):
		pass


	def __helper_function():
		#
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


	def is_point_inside_map(self, x, y, map_size):
		if x >= map_size or y >= map_size or x < 0 or y < 0:
			return False
		return True

	def get_closest_point_to_player(self, player_x, player_y, list_of_points):
		closest = None
		min_dist = 9999

		for x, y in list_of_points:
			dist = math.sqrt((player_x - x) * (player_x - x) + (player_y - y) * (player_y - y))
			if dist < min_dist:
				min_dist = dist
				closest = [x, y]


		if len(list_of_points) == 0:
			return [player_x, player_y]

		return closest


	def bfs(self, player_x, player_y, target_x, target_y, map):

		queue = []
		seen = {}
		dist = {}
		dist[str(player_x) + str(player_y)] = 0
		map_copy = copy.deepcopy(map)

		queue.append([player_x, player_y])

		dx = [0, -1, 1, 0]
		dy = [1,  0, 0,-1]

		while len(queue) > 0:

			vec = queue.pop(0)
			seen[str(vec[0]) + str(vec[1])] = True

			for i in range(4):
				x = dx[i] + vec[0]
				y = dy[i] + vec[1]

				if (self.is_point_inside_map(x, y, len(map_copy))):
					if (str(x) + str(y) not in seen):
						if (map_copy[x][y] != 'o'):
							dist[str(x) + str(y)] = dist[str(vec[0]) + str(vec[1])] + 1
							queue.append([x,y])

			if (target_x == vec[0] and target_y == vec[1]):
				break


		path = [[target_x, target_y]]

		pos_x = target_x
		pos_y = target_y

		found = False
		while not found:

			print(str(pos_x) + str(pos_y))
			for i in range(4):
				x = dx[i] + pos_x
				y = dy[i] + pos_y

				if (self.is_point_inside_map(x, y, len(map_copy))):
					if(str(x) + str(y) in dist):
						if (dist[str(x) + str(y)] == dist[str(pos_x) + str(pos_y)] - 1):
							pos_x = x
							pos_y = y

							if (pos_x == player_x and pos_y == player_y):
								found = True
								break
							else:
								path.append([x,y])

		print("Full path:" + str(path))

		if (path[len(path) - 1][0] == player_x and path[len(path) - 1][1] == player_y):
			return path[len(path) - 2]

		return path[len(path) - 1]



	def solve(self, players_position, game_map, bombs_position):

		#Insert your code here 

		print("\n================Solve function log ================\n")

		for x, y in players_position:
			pass

		for row in game_map:
			pass

		if len(bombs_position):
			for x, y in bombs_position:
				pass

		items_list = []
		for x in range (len(game_map)):
			for y in range (len(game_map[x])):
				if game_map[x][y] == 'p':
					items_list.append([x,y])

		print("List:" + str(items_list))

		player_pos = players_position[0]
		player_x = player_pos[0]
		player_y = player_pos[1]
		closest_point = self.get_closest_point_to_player(player_x, player_y, items_list)

		if len(items_list) == 0:
			return "WAIT"

		print("Closest point:" + str(closest_point))

		next_point = self.bfs(player_x, player_y, closest_point[0], closest_point[1], game_map)

		print("Next point:" + str(next_point))

		#return an action, eg: "MOVE X Y", "BOMB"
		return "MOVE" + " " + str(next_point[0]) + " " + str(next_point[1])






#Connection info
SERVER_ADDRESS  = "localhost"
SERVER_PORT     = 50000
CLIENT_TOKEN    = "singleplayer"

#create a player class instance
p = Player()

#Connect to the server
c = ClientSocket(SERVER_ADDRESS, SERVER_PORT, CLIENT_TOKEN, p.solve)