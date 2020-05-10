using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : Player
{
    private string player_name;
    private int map_size;
    private GameObject bomb_obj;

    private bool IsOnMap(int x, int y)
    {
        return x >= 0 && y >= 0 && y < map_size && x < map_size;
    }

    public string MoveAI(Vector3 player_position, ref string[] map, List<Vector3> map_bombs)
    {
        if(map_bombs.Count == 0)
        {
            //no bomb active
        }

        //return a move based on the map
        int x = (int)player_position.x;
        int y = (int)player_position.z;
        map_size = map.Length;

        int[] dx = new int[4] { -1, 0, 1, 0 };
        int[] dy = new int[4] { 0, 1, 0, -1 };

        List<string> allowed_moves = new List<string>();
        for(int i = 0; i < 4; i++)
        {
            int xx = dx[i] + x;
            int yy = dy[i] + y;
            if (IsOnMap(xx, yy) && (map[xx][yy] == '_' || map[xx][yy] == 'p'))
                allowed_moves.Add("MOVE" + " " + xx.ToString() + " " + yy.ToString());
        }

        return allowed_moves[Random.Range(0, allowed_moves.Count)];
    }
}
