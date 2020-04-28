using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int map_size;
    public bool single_player = true;

    private string[] game_map;
    private List<GameObject> players_list = new List<GameObject>();
    private List<GameObject> pumpkins_list = new List<GameObject>();
    private List<GameObject> bombs_list = new List<GameObject>();

    private int map_obstacles_number;
    private int map_pumpkins_number;
    private int number_of_players;
    private int bomb_range = 2;

    private float time_between_rounds = 0.1f;

    //this simulates data flow
    IEnumerator SimulateGame(float time)
    {
        yield return new WaitForSeconds(time);

        ProcessOneRound();
        StartCoroutine(SimulateGame(time_between_rounds));
    }

    //this function will be removed
    void Start()
    {
        string[] map = MapGenerator.GenerateMap(16, 36, 60);
        string[] tokens = new string[2] { "aa", "bb"};
        PrepareGame(map, tokens);

        StartCoroutine(SimulateGame(time_between_rounds));
    }


    public void PrepareGame(string[] map, string[] tokens)
    {
        LoadMap(map);
        LoadPlayers(tokens);
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
            Debug.Log("AI: " + ai_cmd);

            string player_cmd = "MOVE 0 1";
            Debug.Log("PLAYER: " + player_cmd);

            HandleCommand(player_cmd, 0);
            HandleCommand(ai_cmd, 1);
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

    

    private void HandleCommand(string cmd, int player_index)
    {
        //move
        if(cmd.Split(' ')[0] == "MOVE")
        {
            int x = int.Parse(cmd.Split(' ')[1]);
            int y = int.Parse(cmd.Split(' ')[2]);

            if (game_map[x][y] == 'p')
                players_list[player_index].GetComponent<Player>().IncreasePoints();

            if (game_map[x][y] != 'o')
                players_list[player_index].GetComponent<Player>().MovePlayerToPosition(x, y);
        }

        //place bomb
        if (cmd.Split(' ')[0] == "BOMB")
        {
            //WILL BE ADDED TO THE LIST
        }
    }

    private bool EndOfTheGame()
    {
        int killed = 0;
        for (int i = 0; i < players_list.Count; i++)
            if (players_list[i].GetComponent<Player>().IsDead())
                killed++;

        if (killed + 1 == players_list.Count)
            return true;

        if (map_pumpkins_number > 0)
            return false;

        return false;
    }

   
    void ExecuteBombs()
    {
        int[] dx = new int[4] { 0, 1, 0, -1 };
        int[] dy = new int[4] { 1, 0,-1,  0 };

        for (int i = 0; i < map_size; i++)
            for (int j = 0; j < map_size; j++)
            {
                for (int player = 0; player < players_list.Count; player++)
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
                        //remove from the list
                    }

                    if (hit)
                        players_list[player].GetComponent<Player>().Kill();
                }

            }
    }





    private void RemovePumpkinsIfCollected()
    {
        for (int i = 0; i < map_size; i++)
            for (int j = 0; j < map_size; j++)
                if (game_map[i][j] == 'p')
                {
                    for (int player = 0; player < players_list.Count; player++)
                    {
                        float x = players_list[player].gameObject.transform.position.x;
                        float y = players_list[player].gameObject.transform.position.z;

                        if (x == i && y == j)
                        {
                            StringBuilder sb = new StringBuilder(game_map[i]);
                            sb[j] = 'x';
                            game_map[i] = sb.ToString();

                            for (int p = 0; p < pumpkins_list.Count; p++)
                            {
                                if (pumpkins_list[p] != null)
                                {
                                    if (pumpkins_list[p].transform.position.x == x && pumpkins_list[p].transform.position.z == y)
                                    {
                                        Destroy(pumpkins_list[p]);
                                        pumpkins_list[p] = null;
                                    }
                                }
                            }
                        }
                    }
                }
    }

    private bool IsOnMap(int x, int y)
    {
        return x >= 0 && y >= 0 && y < map_size && x < map_size;
    }

    private void LoadPlayers(string[] tokens)
    {
        int[] xPos = new int[4] { 0, map_size - 1, 0, map_size - 1 };
        int[] yPos = new int[4] { 0, map_size - 1, map_size - 1, 0 };

        int AI = 0;
        if (single_player)
            AI = 1;

        string[] races = new string[4] { "Farmer", "Human", "Zombie", "Skeleton" };

        for (int i = 0; i < tokens.Length - AI; i++)
        {
            players_list.Add(Instantiate(ResourceLoader.LoadPlayer(xPos[i], yPos[i], map_size),
                                                       new Vector3(xPos[i], 0.1f, yPos[i]), 
                                                       Quaternion.identity) as GameObject);

            players_list[i].AddComponent<Player>();
            players_list[i].GetComponent<Player>().SetName(tokens[i]);
            players_list[i].GetComponent<Player>().SetRace(races[i]);
            
        }

        if (AI == 1)
        {
            players_list.Add(Instantiate(ResourceLoader.LoadPlayer(xPos[tokens.Length - AI], yPos[tokens.Length - AI], map_size),
                                                       new Vector3(xPos[tokens.Length - AI], 0, yPos[tokens.Length - AI]),
                                                       Quaternion.identity) as GameObject);

            players_list[tokens.Length - AI].AddComponent<AIPlayer>();
            players_list[tokens.Length - AI].GetComponent<AIPlayer>().SetName("AI");
            players_list[tokens.Length - AI].GetComponent<Player>().SetRace(races[AI]);
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
                    pumpkins_list.Add(Instantiate(ResourceLoader.LoadPumpkin(), new Vector3(i, 0.1f, j), Quaternion.identity) as GameObject);
                }
                if (game_map[i][j] == 'o')
                {
                    map_obstacles_number++;
                    //do not store obstacles because they will be not used later
                    Instantiate(ResourceLoader.LoadObstacle(-1), new Vector3(i, 0.1f, j), Quaternion.identity);
                }
            }
    }
    
}
