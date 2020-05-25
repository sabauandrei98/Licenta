using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class MapGenerator
{
    public static string[] GenerateMap(int size_of_map, int obstacles, int pumpkins)
    {
        string[] map = new string[size_of_map];
        for (int i = 0; i < size_of_map; i++)
        {
            string row = "";
            for (int j = 0; j < size_of_map; j++)
                row += '_';
            map[i] = row;
        }

        int tries = 0;
        int max_tries = 1000;

        Debug.Log("MAP OBJECTS: " + obstacles.ToString() + " " + pumpkins.ToString());

        while (obstacles > 0 || pumpkins > 0)
        {
            int x = Random.Range(0, size_of_map);
            int y = Random.Range(0, size_of_map);

            if (map[x][y] == '_' &&
                (x != 0 || y != 0) && (x != size_of_map - 1 || y != size_of_map - 1) &&
                (x != size_of_map - 1 || y != 0) && (x != 0 && y != size_of_map - 1))
            {

                int[] x_val = new int[2] { x, size_of_map - 1 - x };
                int[] y_val = new int[2] { y, size_of_map - 1 - y };

                bool has_empty_positions = true;
                for (int i = 0; i < 2; i++)
                    for (int j = 0; j < 2; j++)
                    {
                        if (map[x_val[i]][y_val[j]] != '_')
                            has_empty_positions = false;
                    }

                if (has_empty_positions)
                {
                    bool check_map = false;
                    if (obstacles > 0)
                        check_map = true;

                    string[] aux_map = map.DeepClone();
                    for (int i = 0; i < 2; i++)
                        for (int j = 0; j < 2; j++)
                            if (obstacles > 0)
                            {
                                obstacles--;
                                string row = aux_map[x_val[i]];

                                StringBuilder sb = new StringBuilder(row);
                                sb[y_val[j]] = 'o';
                                row = sb.ToString();

                                aux_map[x_val[i]] = row;
                            }

                    if (check_map)
                    {
                        if (CanFillTheEntireMap(aux_map))
                            map = aux_map.DeepClone();
                        else
                        {
                            obstacles += 4;
                            tries += 1;
                            aux_map = map.DeepClone();
                        }
                    }

                    if (tries >= max_tries)
                        obstacles = 0;

                    for (int i = 0; i < 2; i++)
                        for (int j = 0; j < 2; j++)
                            if (!check_map && pumpkins > 0)
                            {
                                pumpkins--;
                                string row = map[x_val[i]];

                                StringBuilder sb = new StringBuilder(row);
                                sb[y_val[j]] = 'p';
                                row = sb.ToString();

                                map[x_val[i]] = row;
                            }
                }
            }
        }

        return map;
    }


    public static bool IsOnMap(int x, int y, int map_size)
    {
        return x >= 0 && y >= 0 && y < map_size && x < map_size;
    }

    public static bool CanFillTheEntireMap(string[] map)
    {
        int free_spaces = 0;
        for (int i = 0; i < map.Length; i++)
            for (int j = 0; j < map[i].Length; j++)
                if (map[i][j] == '_' || map[i][j] == 'p')
                    free_spaces += 1;

        Stack<Vector2> stack = new Stack<Vector2>();
        stack.Push(new Vector2(0, 0));
        int filled_spaces = 0;
        string[] aux_map = map.DeepClone();

        int[] dx = new int[4] { 0, 1, -1, 0 };
        int[] dy = new int[4] { 1, 0, 0, -1 };

        while (stack.Count > 0)
        {
            Vector2 top = stack.Pop();
            int x = (int)top.x;
            int y = (int)top.y;

            if (aux_map[x][y] != 'X')
            {
                StringBuilder sb = new StringBuilder(aux_map[x]);
                sb[y] = 'X';
                aux_map[x] = sb.ToString();

                filled_spaces += 1;
                for (int i = 0; i < 4; i++)
                {
                    int xx = dx[i] + x;
                    int yy = dy[i] + y;
                    if (IsOnMap(xx, yy, aux_map.Length) && (aux_map[xx][yy] == '_' || aux_map[xx][yy] == 'p'))
                        stack.Push(new Vector2(xx, yy));
                }
            }
        }

        if (filled_spaces == free_spaces)
            return true;

        return false;
    }

}
