using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class DFS : MonoBehaviour
{
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
        int[] dy = new int[4] { 1, 0,  0,-1 };

        while (stack.Count > 0)
        {
            Vector2 top = stack.Pop();
            int x = (int)top.x;
            int y = (int)top.y;

            if(aux_map[x][y] != 'X')
            {
                StringBuilder sb = new StringBuilder(aux_map[x]);
                sb[y] = 'X';
                aux_map[x] = sb.ToString();

                filled_spaces += 1;
                for(int i = 0; i < 4; i++)
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
