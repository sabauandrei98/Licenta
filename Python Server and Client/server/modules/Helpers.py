class Helpers:
	@staticmethod
	def char_matrix_to_string_list(char_matrix):
		result = []
		for i in range(len(char_matrix)):
			row = ""
			for j in range(len(char_matrix)):
				row += char_matrix[i][j]
			result.append(row)
		return result

	@staticmethod
	def char_matrix_to_data_rows(char_matrix):
		result = ""
		for i in range(len(char_matrix)):
			for j in range(len(char_matrix)):
				result += char_matrix[i][j]
			result += '\n'
		return result

	@staticmethod
	def string_list_to_data_rows(string_list):
		result = ""
		for i in range(len(string_list)):
			result += string_list[i] + '\n'
		return result


	@staticmethod
	def print_char_matrix_map(map):
		for i in range(len(map)):
			row = ""
			for j in range(len(map)):
				row += map[i][j] + " "
			print(row)