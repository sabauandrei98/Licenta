using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int map_size;
    public bool single_player = true;
    public string instance_token;

    private string[] game_map;
    private string[] races = new string[4] { "Farmer", "Human", "Zombie", "Skeleton" };
    private List<GameObject> players_list = new List<GameObject>();
    private List<GameObject> pumpkins_list = new List<GameObject>();
    private List<GameObject> bombs_list = new List<GameObject>();
    private List<Vector3> bombs_details = new List<Vector3>();
    
    private int map_obstacles_number = 0;
    private int map_pumpkins_number = 0;
    private int number_of_players = 0;
    private int bomb_range = 2;
    private int bomb_detonate_time = 8;
    private float time_between_rounds = 0.5f;
    private bool once = false;
    private bool game_over = false;
    

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
        LoadMapObjects(map);
        LoadPlayers(tokens);
        UpdateRacesOnMap();
    }

   
    //some cmd param here
    public void ProcessOneRound()
    {
        if (game_over)
            return;

        //first execute bombs
        ExecuteBombs();

        //check if end of the game
        if (EndOfTheGame())
        {
            Debug.Log("GAME OVER !");
            game_over = true;
            return;
        }

        if (single_player)
        {
            string ai_cmd = players_list[1].GetComponent<AIPlayer>().MoveAI(players_list[1].transform.position, ref game_map, bombs_details);
            //Debug.Log("AI: " + ai_cmd);


            string player_cmd = "MOVE 0 1";
            if (!once)
            {
                player_cmd = "BOMB";
                once = true;
            }
            //Debug.Log("PLAYER: " + player_cmd);

            HandleCommand(player_cmd, 0);
            HandleCommand(ai_cmd, 1);
        }
        else
        {
            //kinda split the commands and execute each of 

            
        }

        RemovePumpkinsIfCollected();
        UpdateRacesOnMap();
        gameObject.GetComponent<DebugManager>().Notify(ref game_map, ref bombs_details);
    }

    public string GetSessionData()
    {
        //if the player is dead enter the spectate mode
        if(IsPlayerDead(instance_token))
            return "SPECTATE";

        //first store the position of the instance
        string result = GetPlayerPosition(instance_token).ToString() + " ";

        //then store the other players position
        for (int i = 0; i < players_list.Count; i++)
            if (players_list[i].GetComponent<Player>().GetName() != instance_token)
                result += players_list[i].GetComponent<Player>().transform.position.ToString() + " ";

        //store the map data
        for (int i = 0; i < map_size; i++)
            result += game_map[i] + " ";

        //store the bombs position if any
        for (int i = 0; i < bombs_details.Count; i++)
            result += bombs_details[i] + " ";

        return result;
    }

    private bool IsPlayerDead(string token)
    {
        for (int i = 0; i < players_list.Count; i++)
            if (players_list[i].GetComponent<Player>().GetName() == token)
                if (players_list[i].GetComponent<Player>().IsDead())
                    return true;
        return false;
    }

    private Vector3 GetPlayerPosition(string token)
    {
        for (int i = 0; i < players_list.Count; i++)
            if (players_list[i].GetComponent<Player>().GetName() == token)
                    return players_list[i].GetComponent<Player>().transform.position;

        return new Vector3();
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
            {
                players_list[player_index].GetComponent<Player>().MovePlayerToPosition(x, y);
            }
                
        }

        //place bomb
        if (cmd.Split(' ')[0] == "BOMB")
        {
            bombs_list.Add(Instantiate(ResourceLoader.LoadBomb(), players_list[player_index].transform.position, Quaternion.identity) as GameObject);
            bombs_list[bombs_list.Count - 1].AddComponent<DetonateObject>();
            bombs_details.Add(new Vector3(players_list[player_index].transform.position.x, players_list[player_index].transform.position.z, bomb_detonate_time));
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
   
    private void ExecuteBombs()
    {
        int[] dx = new int[4] { 0, 1, 0, -1 };
        int[] dy = new int[4] { 1, 0,-1,  0 };

        for(int bomb_index = 0; bomb_index < bombs_details.Count; bomb_index++)
            if (bombs_details[bomb_index].z == 0)
            {
                int bomb_x = (int)bombs_details[bomb_index].x;
                int bomb_y = (int)bombs_details[bomb_index].y;
 
                for (int player = 0; player < players_list.Count; player++)
                {
                    int x = (int)players_list[player].gameObject.transform.position.x;
                    int y = (int)players_list[player].gameObject.transform.position.z;

                    if (bomb_x == x && bomb_y == y)
                    {
                        players_list[player].GetComponent<Player>().Kill();
                    }
                    else
                        for (int range = 1; range <= bomb_range; range++)
                            for (int ii = 0; ii < 4; ii++)
                            {
                                int new_pos_x = dx[ii] * range + bomb_x;
                                int new_pos_y = dy[ii] * range + bomb_y;

                                if (IsOnMap(new_pos_x, new_pos_y))
                                {
                                    if (x == new_pos_x && y == new_pos_y)
                                    {
                                        players_list[player].GetComponent<Player>().Kill();
                                    }
                                }
                            }
                }

                bombs_list[bomb_index].GetComponent<DetonateObject>().Detonate("bomb");
                bombs_list[bomb_index] = null;

                bombs_details.RemoveAt(bomb_index);
                bomb_index--;
            }
            else
            {
                bombs_details[bomb_index] = new Vector3(bombs_details[bomb_index].x, bombs_details[bomb_index].y, bombs_details[bomb_index].z - 1);
            }

    }

    private void RemovePumpkinsIfCollected()
    {
        for (int i = 0; i < map_size; i++)
            for (int j = 0; j < map_size; j++)
                if (game_map[i][j] == 'p')
                {
                    bool found_pumpkin = false;
                    for (int player = 0; player < players_list.Count; player++)
                    {
                        float x = players_list[player].gameObject.transform.position.x;
                        float y = players_list[player].gameObject.transform.position.z;

                        if (x == i && y == j)
                        {
                            found_pumpkin = true;
                            StringBuilder sb = new StringBuilder(game_map[i]);
                            sb[j] = '_';
                            game_map[i] = sb.ToString();

                            for (int p = 0; p < pumpkins_list.Count; p++)
                            {
                                if (pumpkins_list[p] != null)
                                {
                                    if (pumpkins_list[p].transform.position.x == x && pumpkins_list[p].transform.position.z == y)
                                    {
                                        pumpkins_list[p].GetComponent<DetonateObject>().Detonate("pumpkin");
                                        pumpkins_list[p] = null;
                                    }
                                }
                            }
                        }
                    }

                    if (found_pumpkin)
                        map_pumpkins_number--;
                }
    }

    private bool IsOnMap(int x, int y)
    {
        return x >= 0 && y >= 0 && y < map_size && x < map_size;
    }

    private void UpdateRacesOnMap()
    {
        for (int player_index = 0; player_index < players_list.Count; player_index++)
        {
            for (int i = 0; i < map_size; i++)
                for (int j = 0; j < map_size; j++)
                {
                    if (game_map[i][j] == races[player_index][0])
                    {
                        StringBuilder s = new StringBuilder(game_map[i]);
                        s[j] = '_';
                        game_map[i] = s.ToString();
                    }
                }

            int x = (int)players_list[player_index].transform.position.x;
            int y = (int)players_list[player_index].transform.position.z;

            StringBuilder sb = new StringBuilder(game_map[x]);
            sb[y] = races[player_index][0];
            game_map[x] = sb.ToString();
        }
    }

    private void LoadPlayers(string[] tokens)
    {
        int[] xPos = new int[4] { 0, map_size - 1, 0, map_size - 1 };
        int[] yPos = new int[4] { 0, map_size - 1, map_size - 1, 0 };

        int AI = 0;
        if (single_player)
            AI = 1;

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

    private void LoadMapObjects(string[] map)
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
                    pumpkins_list[pumpkins_list.Count - 1].AddComponent<DetonateObject>();
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
