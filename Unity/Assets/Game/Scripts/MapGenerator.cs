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
                        if (DFS.CanFillTheEntireMap(aux_map))
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

}
