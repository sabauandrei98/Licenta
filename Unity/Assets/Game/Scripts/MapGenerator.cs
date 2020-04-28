using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class MapGenerator
{
    //some BFS here to verify stuff

    public static string[] GenerateMap(int size_of_map, int pumpkins, int obstacles)
    {
        string[] map = new string[size_of_map];
        for (int i = 0; i < size_of_map; i++)
        {
            string row = "";
            for (int j = 0; j < size_of_map; j++)
                row += 'x';
            map[i] = row;
        }

        while (obstacles > 0 || pumpkins > 0)
        {
            int x = Random.Range(0, size_of_map);
            int y = Random.Range(0, size_of_map);

            if (map[x][y] == 'x' &&
                (x != 0 || y != 0) && (x != size_of_map - 1 || y != size_of_map - 1) &&
                (x != size_of_map - 1 || y != 0) && (x != 0 && y != size_of_map - 1))
            {

                int[] x_val = new int[2] { x, size_of_map - 1 - x };
                int[] y_val = new int[2] { y, size_of_map - 1 - y };

                bool ok = true;
                for (int i = 0; i < 2; i++)
                    for (int j = 0; j < 2; j++)
                    {
                        if (map[x_val[i]][y_val[j]] != 'x')
                            ok = false;
                    }

                if (ok)
                {
                    for (int i = 0; i < 2; i++)
                        for (int j = 0; j < 2; j++)
                        {
                            if (obstacles > 0)
                            {
                                obstacles--;
                                string row = map[x_val[i]];

                                StringBuilder sb = new StringBuilder(row);
                                sb[y_val[j]] = 'o';
                                row = sb.ToString();

                                map[x_val[i]] = row;
                            }
                            else
                            if (pumpkins > 0)
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
        }

        return map;
    }

}
