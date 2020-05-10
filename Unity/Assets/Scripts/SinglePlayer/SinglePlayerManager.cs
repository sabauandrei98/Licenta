using UnityEngine;
using System;
using System.Net.Sockets;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Net;
using System.Threading;

public class SinglePlayerManager : MonoBehaviour
{
    public Text connection_status;

    private Thread server_thread;
    private Socket _server_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private Socket _client_socket;
    private byte[] _recieveBuffer = new byte[512];
    private int connected = 0;
    private bool token_validated = false;
    private string server_cmd = "";
    private string token = "singleplayer";
    private float wait_client_time = 1.0f;
    private float timer = 0;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            GameObject go = GameObject.FindGameObjectWithTag("SinglePlayerManager");
            if (go != null)
            {
                Disconnect();
                Destroy(go);
            }
        }
    }

    void Update()
    {
        if (connected == 1)
        {
            if (timer > 0)
                timer -= Time.deltaTime;
            else
            {
                if (server_cmd != "")
                {
                    timer = wait_client_time;
                    Debug.Log("Recv: <" + server_cmd + ">");
                    CommandHandler(server_cmd);
                    server_cmd = "";
                }
            }
        }
        else
        if (connected == -1)
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    void Start()
    {
        server_thread = new Thread(SetupServer);
        server_thread.IsBackground = true;
        server_thread.Start();
    }

    void CommandHandler(string cmd)
    {
        if (!token_validated)
        {
            if (cmd == token)
            {
                token_validated = true;
                SetConnectionStatus(Color.green, "Connected to the server ! The game will start soon ..");
                SendData(System.Text.Encoding.Default.GetBytes("Ok"));
                StartCoroutine(LoadFirstScene());
            }
            else
                SetConnectionStatus(Color.red, "Wrong token !");
        }
        else
        {
            GameManager game_manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
            game_manager.ProcessOneRound(cmd);
            SendData(System.Text.Encoding.Default.GetBytes(game_manager.GetSessionData()));
        }
    }

    private Vector3 SinglePlayerSettings()
    {
        int AI_number = 1;
        int obstacles_number = 40;
        int pumpkins_number = 40;
        try
        {
            int aux = int.Parse(GameObject.FindWithTag("Bots_InputField").GetComponent<InputField>().text);
            if (1 <= aux && aux <= 3)
                AI_number = aux;
        }
        catch { }

        try
        {
            int aux = int.Parse(GameObject.FindWithTag("Obstacles_InputField").GetComponent<InputField>().text);
            if (1 <= aux && aux <= 80)
                obstacles_number = aux;

            obstacles_number = (obstacles_number / 4) * 4;
            if (obstacles_number < 4)
                obstacles_number = 4;
        }
        catch { }

        try
        {
            int aux = int.Parse(GameObject.FindWithTag("Pumpkins_InputField").GetComponent<InputField>().text);
            if (1 <= aux && aux <= 80)
                pumpkins_number = aux;

            pumpkins_number = (pumpkins_number / 4) * 4;
            if (pumpkins_number < 4)
                pumpkins_number = 4;
        }
        catch { }

        return new Vector3(AI_number, obstacles_number, pumpkins_number);
    }

    private IEnumerator LoadFirstScene()
    {

        SceneManager.LoadScene("Game");
        Vector3 singleplayer_settings = SinglePlayerSettings();
        Debug.Log("Settings: " + singleplayer_settings.ToString());

        yield return new WaitForSeconds(1.5f);

        GameManager game_manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        game_manager.bots_number = (int)singleplayer_settings.x;
        game_manager.obstacles_to_generate = (int)singleplayer_settings.y;
        game_manager.pumpkins_to_generate = (int)singleplayer_settings.z;

        game_manager.PrepareGame("", true);
        SendData(System.Text.Encoding.Default.GetBytes(game_manager.GetSessionData()));
    }

    private void IsSocketConnected(Socket s)
    {
        while (connected == 1)
        {
            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if (part1 && part2)
            {
                Debug.Log("Disconnected !");
                Disconnect();
            }
            else
                Debug.Log("Connected !");

            Thread.Sleep(1000);
        }
    }

    private void SetupServer()
    {
        Debug.Log("Waiting for clients ..");
        try
        {
            _server_socket.Bind(new IPEndPoint(IPAddress.Any, 50000));
            _server_socket.Listen(1);

            while(true)
            {
                _client_socket = _server_socket.Accept();
                Debug.Log("Client Connected");
                break;
            }
            connected = 1;
            Thread is_connected = new Thread(()=> IsSocketConnected(_client_socket));
            is_connected.Start();
        }
        catch (SocketException ex)
        {
            string error = ex.Message;
            Debug.Log(error);

            return;
        }

        _client_socket.BeginReceive(_recieveBuffer, 0, _recieveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
    }

    private void ReceiveCallback(IAsyncResult AR)
    {
        //Check how much bytes are recieved and call EndRecieve to finalize handshake
        int recieved = _client_socket.EndReceive(AR);

        if (recieved <= 0)
            return;

        //Copy the recieved data into new buffer , to avoid null bytes
        byte[] recData = new byte[recieved];
        Buffer.BlockCopy(_recieveBuffer, 0, recData, 0, recieved);

        //Process data here the way you want , all your bytes will be stored in recData
        server_cmd = System.Text.Encoding.Default.GetString(recData);

        //Start receiving again
        _client_socket.BeginReceive(_recieveBuffer, 0, _recieveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
    }

    private void SendData(byte[] data)
    {
        try
        {
            SocketAsyncEventArgs socketAsyncData = new SocketAsyncEventArgs();
            socketAsyncData.SetBuffer(data, 0, data.Length);
            _client_socket.SendAsync(socketAsyncData);
        }
        catch
        {
            Debug.Log("Error in sending data!");
        }
    }


    private void SetConnectionStatus(Color color, string message)
    {
        connection_status.text = message;
        connection_status.color = color;
    }

    private void Disconnect()
    {
        try
        {
            if (connected == 1)
            {
                connected = -1;
                _client_socket.Disconnect(true);
                if (_client_socket.Connected)
                    Debug.Log("We're still connnected");
                else
                    Debug.Log("We're disconnected");

                _client_socket.Close();
            }
            _server_socket.Close();
        }
        catch
        {
            Debug.Log("Error in disconnecting !");
        }
    }

    void OnApplicationQuit()
    {
        Disconnect();
    }
}