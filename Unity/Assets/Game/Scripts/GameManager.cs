using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    private const int map_size = 16;
    private const int map_obstacles = 36;
    private const int map_pumpkins = 8;

    public GameObject[] obstacles_list;
    public GameObject map_stone;
    public GameObject pumpkin;

    private int[][] game_map = new int[map_size][];

    // Start is called before the first frame update
    void Start()
    {
        GenerateMap();
    }

    // Update is called once per frame
    void Update()
    {

    }


    private void GenerateMap()
    {
        for (int i = 0; i < map_size; i++)
        {
            game_map[i] = new int[map_size];
            for (int j = 0; j < map_size; j++)
                game_map[i][j] = 0;
        }


        int obstacles = map_obstacles;
        int pumpkins = map_pumpkins;

        while (obstacles > 0 || pumpkins > 0)
        {
            int x = Random.Range(0, map_size);
            int y = Random.Range(0, map_size);

            if (game_map[x][y] == 0 &&
                (x != 0 || y != 0) && (x != map_size - 1 || y != map_size - 1) &&
                (x != map_size - 1 || y != 0) && (x != 0 && y != map_size - 1))
            {

                int[] x_val = new int[2] { x, map_size - 1 - x };
                int[] y_val = new int[2] { y, map_size - 1 - y };

                bool ok = true;
                for (int i = 0; i < 2; i++)
                    for (int j = 0; j < 2; j++)
                    {
                        //Debug.Log(x_val[i].ToString() + " " + y_val[j].ToString());
                        if (game_map[x_val[i]][y_val[j]] != 0)
                            ok = false;
                    }

                if (ok)
                {
                    for (int i = 0; i < 2; i++)
                        for (int j = 0; j < 2; j++)
                        {
                            Debug.Log(obstacles.ToString() + " " + pumpkins.ToString());
                            if (obstacles > 0)
                            {
                                obstacles--;
                                game_map[x_val[i]][y_val[j]] = 1;
                                Instantiate(obstacles_list[Random.Range(0, obstacles_list.Length)], new Vector3(x_val[i], 0, y_val[j]), Quaternion.identity);
                            }
                            else
                            if (pumpkins > 0)
                            {
                                pumpkins--;
                                game_map[x_val[i]][y_val[j]] = 2;
                                Instantiate(pumpkin, new Vector3(x_val[i], 0.1f, y_val[j]), Quaternion.identity);
                            }
                        }
                }
            }
        }


        for (int i = 0; i < map_size; i++)
            for (int j = 0; j < map_size; j++)
                if (game_map[i][j] == 0 || game_map[i][j] == 2)
                {
                    Instantiate(map_stone, new Vector3(i, 0, j), Quaternion.identity);
                }
    }




}
