import copy

class DFS:

	@staticmethod
	def is_on_map(x, y, size):
		if (x < 0 or y < 0 or x >= size or y >= size):
			return False
		return True

	@staticmethod
	def can_fill_all_the_map(map):

		free_spaces = 0

		for i in range(len(map)):
			for j in range(len(map[i])):
				if map[i][j] == '_' or map[i][j] == 'p':
					free_spaces += 1

		stack = []
		stack.append((0,0))
		aux_map = copy.deepcopy(map)
		filled_spaces = 0

		dx = [ 0, 1, -1, 0]
		dy = [ 1, 0, 0, -1]

		while len(stack) != 0:

			x,y = stack.pop(0)
			if aux_map[x][y] != 'X':
				aux_map[x][y] = 'X'
				filled_spaces += 1

				for i in range(4):
					xx = dx[i] + x
					yy = dy[i] + y

					if (DFS.is_on_map(xx, yy, len(aux_map)) and (aux_map[xx][yy] == '_' or aux_map[xx][yy] == 'p')):
						stack.append((xx, yy))

		if (filled_spaces == free_spaces):
			return True

		return False


