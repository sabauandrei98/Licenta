using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject[] obstacles_list_obj_model;
    public GameObject map_stone_obj_model;
    public GameObject pumpkin_obj_model;
    public int map_size;
    public bool single_player = true;

    private string[] game_map;
    private GameObject[] players_list;
    private GameObject[] pumpkins_list;
    private GameObject[] bombs_list;

    private int map_obstacles_number;
    private int map_pumpkins_number;
    private int number_of_players;
    private int bomb_range = 2;


    //this function will be removed
    void Start()
    {
        string[] map = GenerateMap(16, 8, 8);
        string[] tokens = new string[4] { "aa", "bb", "cc", "dd" };
        PrepareGame(map, tokens);

        //import all the models from resources
    }


    public void PrepareGame(string[] map, string[] tokens)
    {
        LoadMap(map);
        SpawnPlayers(tokens);
    }

   
    //some cmd param here
    void ProcessOneRound()
    {
        //first execute bombs
        //ExecuteBombs();

        //check if end of the game
        //if (EndOfTheGame())
        {
            //notify network
        }

        if (single_player)
        {
            string ai_cmd = players_list[1].GetComponent<AIPlayer>().MoveAI(game_map);

            //execute each cmd
        }
        else
        {
            //kinda split the commands and execute each of 

            
        }

        //check if end of the game
        //if (EndOfTheGame())
        {
            //notify network
        }

        RemovePumpkinsIfCollected();
    }

    private void RemovePumpkinsIfCollected()
    {
        for(int i = 0; i < map_size; i++)
            for(int j = 0; j < map_size; j++)
                if (game_map[i][j] == 'p')
                {
                    for(int player = 0; player < players_list.Length; player++)
                    {
                        float x = players_list[player].gameObject.transform.position.x;
                        float y = players_list[player].gameObject.transform.position.z;

                        if (x == i && y == j)
                        {
                            StringBuilder sb = new StringBuilder(game_map[i]);
                            sb[j] = 'x';
                            game_map[i] = sb.ToString();
                        }
                    }
                }
    }

    private void HandleCommand(string cmd, int player_index)
    {
        //move
        if(cmd.Split(' ')[0] == "MOVE")
        {
            int x = int.Parse(cmd.Split(' ')[1]);
            int y = int.Parse(cmd.Split(' ')[2]);

            if (game_map[x][y] == 'p')
                players_list[player_index].GetComponent<Player>().pumpkins_collected++;

            if (game_map[x][y] != 'o')
                players_list[player_index].GetComponent<Player>().MovePlayerToPosition(x, y);
        }

        //place bomb
        if (cmd.Split(' ')[0] == "BOMB")
        {

        }
    }

    private bool EndOfTheGame()
    {
        int killed = 0;
        for (int i = 0; i < players_list.Length; i++)
            if (players_list[i].GetComponent<Player>().killed)
                killed++;

        if (killed + 1 == players_list.Length)
            return true;

        if (map_pumpkins_number > 0)
            return false;

        return false;
    }

    private bool IsOnMap(int x, int y)
    {
        return x >= 0 && y >= 0 && y < map_size && x < map_size;
    }

    void ExecuteBombs()
    {
        int[] dx = new int[4] { 0, 1, 0, -1 };
        int[] dy = new int[4] { 1, 0,-1,  0 };

        for (int i = 0; i < map_size; i++)
            for (int j = 0; j < map_size; j++)
            {
                for (int player = 0; player < players_list.Length; player++)
                {
                    bool hit = false;
                    float x = players_list[player].gameObject.transform.position.x;
                    float y = players_list[player].gameObject.transform.position.z;

                    if (game_map[i][j] == '0')
                    {
                        if (i == x && j == y)
                            hit = true;

                        for (int range = 1; range <= bomb_range && !hit; range++)
                            for (int ii = 0; ii < 4; ii++)
                            {
                                int new_pos_x = dx[ii] * range + i;
                                int new_pos_y = dy[ii] * range + j;

                                if (IsOnMap(new_pos_x, new_pos_y))
                                {
                                    if (x == new_pos_x && y == new_pos_y)
                                        hit = true;
                                }
                            }

                        //detonate
                        //remove 0
                    }

                    if (hit)
                        players_list[player].GetComponent<Player>().Kill();
                }

            }
    }

    private void SpawnPlayers(string[] tokens)
    {
        int[] xPos = new int[4] { 0, map_size - 1, 0, map_size - 1 };
        int[] yPos = new int[4] { 0, map_size - 1, map_size - 1, 0 };

        int AI = 0;
        if (single_player)
            AI = 1;

        players_list = new GameObject[tokens.Length + AI];

        for (int i = 0; i < tokens.Length - AI; i++)
        {
            players_list[i] = CreatePlayer(xPos[i], yPos[i]);
            players_list[i].AddComponent<Player>();
            players_list[i].GetComponent<Player>().SetName(tokens[i]);
        }

        if (AI == 1)
        {
            players_list[tokens.Length + AI - 1] = CreatePlayer(xPos[tokens.Length + AI - 1], yPos[tokens.Length + AI - 1]);
            players_list[tokens.Length + AI - 1].AddComponent<AIPlayer>();
            players_list[tokens.Length + AI - 1].GetComponent<AIPlayer>().SetName("AI");
        }
    }

    private void LoadMap(string[] map)
    {
        game_map = map;
        map_size = map[0].Length;

        for (int i = 0; i < game_map.Length; i++)
            for (int j = 0; j < game_map[i].Length; j++)
            {
                if (game_map[i][j] == 'p')
                {
                    map_pumpkins_number++;
                    Instantiate(pumpkin_obj_model, new Vector3(i, 0.1f, j), Quaternion.identity);
                }
                if (game_map[i][j] == 'o')
                {
                    map_obstacles_number++;
                    Instantiate(obstacles_list_obj_model[Random.Range(0, obstacles_list_obj_model.Length)], new Vector3(i, 0, j), Quaternion.identity);
                }

                if (game_map[i][j] == 'x')
                {
                    Instantiate(map_stone_obj_model, new Vector3(i, 0, j), Quaternion.identity);
                }
            }
    }

    private GameObject CreatePlayer(int x, int y)
    {
        GameObject go = null;

        //0 - farmer
        if (x == 0 && y == 0)
            go = Resources.Load<GameObject>("KenneyGraveyard/digger");

        //1 - zombie 
        if (x == 0 && y == map_size - 1)
            go = Resources.Load<GameObject>("KenneyGraveyard/zombie");

        //2 - skeleton
        if (x == map_size - 1 && y == 0)
            go = Resources.Load<GameObject>("KenneyGraveyard/skeleton");

        //3 - human
        if (x == map_size - 1 && y == map_size - 1)
            go = Resources.Load<GameObject>("KenneyGraveyard/vampire");

        if (go == null)
            Debug.Log("wrooooong");

        return Instantiate(go, new Vector3(x, 0.1f, y), Quaternion.identity) as GameObject;
    }

    private string[] GenerateMap(int size_of_map, int pumpkins, int obstacles)
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
