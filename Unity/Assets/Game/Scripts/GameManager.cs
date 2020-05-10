using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GameManager : MonoBehaviour
{
    public string instance_token;

    //used while singleplayer
    public int bots_number = 1;
    public int obstacles_to_generate;
    public int pumpkins_to_generate;

    private int map_obstacles_number = 0;
    private int map_pumpkins_number = 0;
    private int map_size = 16;
    private bool game_over = false;
    private bool single_player;
    private string[] game_map;
    private string[] races = new string[4] { "Farmer", "Human", "Zombie", "Skeleton" };
    private List<GameObject> players_list = new List<GameObject>();
    private List<GameObject> pumpkins_list = new List<GameObject>();
    private List<GameObject> bombs_list = new List<GameObject>();
    private List<Vector3> bombs_details = new List<Vector3>();

    //can be modified
    private int bomb_range = 2;
    private int bomb_detonate_time = 5;


    private struct InitialData
    {
        public string[] tokens;
        public string[] map;
    }

    private string[] ServerDataToRows(string server_data)
    {
        string[] rows = server_data.Split(
                new[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None
            );

        int lines = 0;
        for (int i = 0; i < rows.Length; i++)
            if (rows[i].Length > 1)
                lines++;

        string[] result = new string[lines];

        int cnt = 0;
        for (int i = 0; i < rows.Length; i++)
            if (rows[i].Length > 1)
            {
                result[cnt] = rows[i];
                cnt++;
            }

        return result;
    }

    private InitialData SplitInitialData(string server_data)
    {
        InitialData data = new InitialData();
        string[] data_rows = ServerDataToRows(server_data);

        int tokens = 0;
        for (int i = 0; i < data_rows.Length; i++)
            if (data_rows[i].Length < map_size)
                tokens++;

        //Debug.Log("TOKENsize:" + tokens.ToString());
        data.tokens = new string[tokens];
        data.map = new string[map_size];

        for (int i = 0; i < data_rows.Length; i++)
            if (i < tokens)
            {
                data.tokens[i] = data_rows[i];
            }
            else
                data.map[i - tokens] = data_rows[i];

        return data;
    }

    public void PrepareGame(string server_data, bool single_player)
    {
        this.single_player = single_player;

        if (single_player)
        {
            string[] tokens = new string[1 + bots_number];
            tokens[0] = "singleplayer";
            for (int i = 1; i < tokens.Length; i++)
                tokens[i] = "AI";

            string[] map = MapGenerator.GenerateMap(map_size, obstacles_to_generate, pumpkins_to_generate);
            LoadMapObjects(map);
            LoadPlayers(tokens);
            UpdateRacesOnMap();
        }
        else
        {
            InitialData data = SplitInitialData(server_data);
            LoadMapObjects(data.map);
            LoadPlayers(data.tokens);
            UpdateRacesOnMap();
        }
    }


    private struct Command
    {
        public string token;
        public string command;
    }
    private List<Command> SplitCommands(string server_data)
    {
        List<Command> cmds = new List<Command>();

        if (single_player == true)
        {
            Command cmd = new Command();
            cmd.token = "singleplayer";
            cmd.command = server_data;
            cmds.Add(cmd);
            return cmds;
        }
                
        string[] lines = ServerDataToRows(server_data);

        for (int i = 0; i < lines.Length; i++)
        {
            Command cmd = new Command();
            cmd.token   = lines[i].Split('=')[0];
            cmd.command = lines[i].Split('=')[1];
            cmds.Add(cmd);
        }
        return cmds;
    }
   

    public void ProcessOneRound(string server_data)
    {
        if (game_over)
            return;

        List<Command> cmds = SplitCommands(server_data);
        ExecuteBombs();

        if (EndOfTheGame())
        {
            Debug.Log("GAME OVER !");
            game_over = true;
            return;
        }

        //execute cmds for players
        for(int cmd_ind = 0; cmd_ind < cmds.Count; cmd_ind++)
            for(int player_ind = 0; player_ind < players_list.Count; player_ind++)
            {
                if (players_list[player_ind].GetComponent<Player>().GetName() == cmds[cmd_ind].token)
                {
                    Debug.Log("Command handled by player");
                    HandleCommand(cmds[cmd_ind].command, player_ind);
                }
            }

        //execute cmds for bots (if any)
        for (int AI_ind = 0; AI_ind < players_list.Count; AI_ind++)
        {
            if (players_list[AI_ind].GetComponent<Player>().GetName() == "AI")
            {
                string ai_cmd = players_list[AI_ind].GetComponent<AIPlayer>().MoveAI(players_list[AI_ind].transform.position, ref game_map, bombs_details);
                Debug.Log("Command handled by AI");
                HandleCommand(ai_cmd, AI_ind);
            }
        }

        RemovePumpkinsIfCollected();
        UpdateRacesOnMap();
        gameObject.GetComponent<DebugManager>().Notify(ref game_map, ref bombs_details);
    }


    private void HandleCommand(string cmd, int player_index)
    {

        if (players_list[player_index].GetComponent<Player>().IsDead())
            return;

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

    public string GetSessionData()
    {
        //if the player is dead enter the spectate mode
        if (IsPlayerDead(instance_token))
            return "SPECTATE";

        //first store the position of the instance
        Vector3 current_player_pos = GetPlayerPosition(instance_token);
        string result = current_player_pos.x.ToString() + " " + current_player_pos.z.ToString() + '\n';

        //then store the other players position
        //Debug.Log("GetSession:" + players_list.Count.ToString());
        for (int i = 0; i < players_list.Count; i++)
            if (players_list[i].GetComponent<Player>().GetName() != instance_token)
            {
                
                Vector3 other_player_pos = players_list[i].GetComponent<Player>().transform.position;
                //Debug.Log("GetSessionADD:" + other_player_pos.ToString());
                //Debug.Log("GetSessionToken:" + players_list[i].GetComponent<Player>().GetName().ToString());
                result += other_player_pos.x.ToString() + " " + other_player_pos.z.ToString() + '\n';
            }

        //store the map data
        for (int i = 0; i < map_size; i++)
            result += game_map[i] + '\n';

        //store the bombs position if any
        for (int i = 0; i < bombs_details.Count; i++)
        {
            Vector3 bomb_pos = bombs_details[i];
            result += bomb_pos.x.ToString() + " " + bomb_pos.z.ToString() + '\n';
        }

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

        for (int i = 0; i < tokens.Length; i++)
        {
            players_list.Add(Instantiate(ResourceLoader.LoadPlayer(xPos[i], yPos[i], map_size),
                                                       new Vector3(xPos[i], 0.1f, yPos[i]), 
                                                       Quaternion.identity) as GameObject);

            if (tokens[i] == "AI")
            {
                players_list[i].AddComponent<AIPlayer>();
                players_list[i].GetComponent<AIPlayer>().SetName(tokens[i]);
                players_list[i].GetComponent<AIPlayer>().SetRace(races[i]);
            }
            else
            {
                players_list[i].AddComponent<Player>();
                players_list[i].GetComponent<Player>().SetName(tokens[i]);
                players_list[i].GetComponent<Player>().SetRace(races[i]);
            }
        }
    }

    private void LoadMapObjects(string[] map)
    {
        game_map = map;

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
