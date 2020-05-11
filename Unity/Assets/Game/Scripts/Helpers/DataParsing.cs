using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class DataParsing : MonoBehaviour
{
    private static int map_size = 16;

    private static string[] ServerDataToRows(string server_data)
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

    public struct InitialData
    {
        public string[] tokens;
        public string[] map;
    }

    public static InitialData SplitInitialData(string server_data)
    {
        InitialData data = new InitialData();
        string[] data_rows = ServerDataToRows(server_data);

        int tokens = 0;
        for (int i = 0; i < data_rows.Length; i++)
            if (data_rows[i].Length < map_size)
                tokens++;

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


    public struct Command
    {
        public string token;
        public string command;
    }

    public static List<Command> SplitCommands(string server_data, bool single_player)
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
            cmd.token = lines[i].Split('=')[0];
            cmd.command = lines[i].Split('=')[1];
            cmds.Add(cmd);
        }
        return cmds;
    }
}
