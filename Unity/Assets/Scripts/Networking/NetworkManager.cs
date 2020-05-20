using UnityEngine;
using System;
using System.Net.Sockets;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Threading;
using System.Net;

public class NetworkManager : MonoBehaviour
{
    //single player
    private InputField bots_input_field;
    private InputField obstacles_input_field;
    private InputField pumpkins_input_field;
    private Thread server_thread;
    private Socket server_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private float wait_client_time = 1.0f;
    private float wait_client_timer = 0;
    private string single_player_token = "singleplayer";
    private bool single_player_token_validated = false;
    
    //multi player
    private Text ip_text;
    private Text port_text;
    private Button connect_to_server_button;
    private Button ready_button;
    private InputField ip_input_field;
    private InputField port_input_field;
    private const string UNITY_TOKEN = "439b3a25b555b3bc8667a09a036ae70c";
    private string ide_token = "";

    //common
    private Text connection_status;
    private Socket client_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private bool single_player;
    private int connected = 0;
    private string received_cmd = "";
    private byte[] receive_buffer = new byte[512];


    void Awake()
    {
        if (SceneManager.GetActiveScene().name == "Singleplayer")
        {
            single_player = true;
            bots_input_field = GameObject.FindWithTag("Bots_InputField").GetComponent<InputField>();
            obstacles_input_field = GameObject.FindWithTag("Obstacles_InputField").GetComponent<InputField>();
            pumpkins_input_field = GameObject.FindWithTag("Pumpkins_InputField").GetComponent<InputField>();
            connection_status = GameObject.FindWithTag("ConnectionStatus_Text").GetComponent<Text>();
        }
        else
        {
            single_player = false;
            ip_input_field = GameObject.FindWithTag("IpAddress_InputField").GetComponent<InputField>();
            ip_text = GameObject.FindWithTag("Ip_Text").GetComponent<Text>();
            port_input_field = GameObject.FindWithTag("Port_InputField").GetComponent<InputField>();
            port_text = GameObject.FindWithTag("Port_Text").GetComponent<Text>();
            connect_to_server_button = GameObject.FindWithTag("Connect_Button").GetComponent<Button>();
            ready_button = GameObject.FindWithTag("Ready_Button").GetComponent<Button>();
            connection_status = GameObject.FindWithTag("ConnectionStatus_Text").GetComponent<Text>();
        }

        DontDestroyOnLoad(this.gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            GameObject go = GameObject.FindGameObjectWithTag("NetworkManager");
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
            if (single_player && wait_client_timer > 0)
                wait_client_timer -= Time.deltaTime;
            else
            if (received_cmd != "")
            {
                Debug.Log("Recv: <" + received_cmd + ">");
                if (single_player)
                {
                    wait_client_timer = wait_client_time;
                    SinglePlayerCommandHandler(received_cmd);
                }
                else
                    MultiPlayerCommandHandler(received_cmd);

                received_cmd = "";
            }
        }
        else
            if (connected == -1)
        {
            connected = -2;
            SceneManager.LoadScene("MainMenu");
        }
    }

    void Start()
    {
        if (single_player)
        {
            server_thread = new Thread(SetupLocalServer);
            server_thread.IsBackground = true;
            server_thread.Start();
        }
        else
        {
            connect_to_server_button.onClick.AddListener(delegate { ConnectToServerButton(); });
            ready_button.onClick.AddListener(delegate { ReadyButton(); });
        }
    }

    /// <summary>
    /// SINGLE PLAYER SECTION
    /// </summary>

    private void SetupLocalServer()
    {
        Debug.Log("Waiting for clients ..");
        try
        {
            server_socket.Bind(new IPEndPoint(IPAddress.Any, 50000));
            server_socket.Listen(1);

            while (true)
            {
                client_socket = server_socket.Accept();
                Debug.Log("Client Connected");
                break;
            }
            connected = 1;
            Thread is_connected = new Thread(() => IsSocketConnected(client_socket));
            is_connected.Start();
        }
        catch (SocketException ex)
        {
            string error = ex.Message;
            Debug.Log(error);

            return;
        }

        client_socket.BeginReceive(receive_buffer, 0, receive_buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
    }
    void SinglePlayerCommandHandler(string cmd)
    {
        if (!single_player_token_validated)
        {
            if (cmd == single_player_token)
            {
                single_player_token_validated = true;
                SetConnectionStatus(Color.green, "Connected to the server ! The game will start soon ..");
                SendData(System.Text.Encoding.Default.GetBytes("Token verified !"));
                StartCoroutine(HandleInitialDataSinglePlayer());
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

    private IEnumerator HandleInitialDataSinglePlayer()
    {

        SceneManager.LoadScene("Game");
        Vector3 singleplayer_settings = SinglePlayerSettings();

        yield return new WaitForSeconds(1.5f);

        GameManager game_manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        game_manager.bots_number = (int)singleplayer_settings.x;
        game_manager.obstacles_to_generate = (int)singleplayer_settings.y;
        game_manager.pumpkins_to_generate = (int)singleplayer_settings.z;

        game_manager.PrepareGame("", true);
        SendData(System.Text.Encoding.Default.GetBytes(game_manager.GetSessionData()));
    }


    private Vector3 SinglePlayerSettings()
    {
        int AI_number = 1;
        int obstacles_number = 40;
        int pumpkins_number = 40;
        try
        {
            int aux = int.Parse(bots_input_field.text);
            if (1 <= aux && aux <= 3)
                AI_number = aux;
        }
        catch { }

        try
        {
            int aux = int.Parse(obstacles_input_field.text);
            if (1 <= aux && aux <= 80)
                obstacles_number = aux;

            obstacles_number = (obstacles_number / 4) * 4;
            if (obstacles_number < 4)
                obstacles_number = 4;
        }
        catch { }

        try
        {
            int aux = int.Parse(pumpkins_input_field.text);
            if (1 <= aux && aux <= 80)
                pumpkins_number = aux;

            pumpkins_number = (pumpkins_number / 4) * 4;
            if (pumpkins_number < 4)
                pumpkins_number = 4;
        }
        catch { }

        return new Vector3(AI_number, obstacles_number, pumpkins_number);
    }



    /// <summary>
    /// MULTIPLAYER SECTION
    /// </summary>

    public void ConnectToServerButton()
    {
        SetupConnection();
        SendData(System.Text.Encoding.Default.GetBytes(UNITY_TOKEN));
    }

    public void ReadyButton()
    {
        SendData(System.Text.Encoding.Default.GetBytes("READY"));
    }

    private void SetupConnection()
    {
        try
        {
            if (ip_text.text == "" || port_text.text == "")
            {
                SetConnectionStatus(Color.red, "Empty fields !");
                return;
            }

            client_socket.Connect(ip_text.text, int.Parse(port_text.text));
            ip_input_field.interactable = false;
            port_input_field.interactable = false;
            connect_to_server_button.interactable = false;
            ready_button.interactable = true;
            connected = 1;
            Thread is_connected = new Thread(() => IsSocketConnected(client_socket));
            is_connected.Start();
        }
        catch (SocketException ex)
        {
            string error = ex.Message;
            Debug.Log(error);

            if (!error.Contains("A connect request was made on an already connected socket."))
                SetConnectionStatus(Color.red, "Could not connect to the server");
            return;
        }

        client_socket.BeginReceive(receive_buffer, 0, receive_buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
    }


    void MultiPlayerCommandHandler(string cmd)
    {
        if (cmd == "TOKEN OK")
        {
            SetConnectionStatus(Color.green, "Connected to the server! Token verified !");
            ready_button.gameObject.SetActive(true);
        }

        if (cmd == "READY OK")
        {
            SetConnectionStatus(Color.blue, "You are ready !");
        }

        if (cmd == "GAME RUNNING")
        {
            SetConnectionStatus(Color.red, "Sorry, the game is running !");
            Disconnect();
        }

        if (cmd.Split(':')[0] == "IDE_TOKEN")
        {
            ide_token = cmd.Split(':')[1];
            SetConnectionStatus(Color.white, "This is your ide token: " + "<color=#46CF4E>" + ide_token + "</color>" +
                "\n Put it in your code to connect to the server from ide");
        }

        if (cmd.Split(':')[0] == "INITIAL_DATA")
        {
            StartCoroutine(HandleInitialDataMultiPlayer(cmd.Split(':')[1]));
        }

        if (cmd.Split(':')[0] == "ROUND")
        {
            GameManager game_manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
            game_manager.ProcessOneRound(cmd.Split(':')[1]);
            SendData(System.Text.Encoding.Default.GetBytes(game_manager.GetSessionData()));
        }
    }

    private IEnumerator HandleInitialDataMultiPlayer(string data)
    {
        SceneManager.LoadScene("Game");
        yield return new WaitForSeconds(0.5f);

        GameManager game_manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        game_manager.instance_token = ide_token;
        game_manager.PrepareGame(data, false);
        SendData(System.Text.Encoding.Default.GetBytes(game_manager.GetSessionData()));
    }


    /// <summary>
    /// COMMON SOCKETS FUNCTIONS
    /// </summary>


    private void IsSocketConnected(Socket s)
    {
        while (connected == 1)
        {
            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if (part1 && part2)
            {
                Disconnect();
            }

            Thread.Sleep(1000);
        }
    }

    private void ReceiveCallback(IAsyncResult AR)
    {
        //Check how much bytes are recieved and call EndRecieve to finalize handshake
        int recieved = client_socket.EndReceive(AR);
        
        if (recieved <= 0)
            return;

        //Copy the recieved data into new buffer , to avoid null bytes
        byte[] recData = new byte[recieved];
        Buffer.BlockCopy(receive_buffer, 0, recData, 0, recieved);

        //Process data here the way you want , all your bytes will be stored in recData
        received_cmd = System.Text.Encoding.Default.GetString(recData);

        //Start receiving again
        client_socket.BeginReceive(receive_buffer, 0, receive_buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
    }

    private void SendData(byte[] data)
    {
        try
        {
            SocketAsyncEventArgs socketAsyncData = new SocketAsyncEventArgs();
            socketAsyncData.SetBuffer(data, 0, data.Length);
            client_socket.SendAsync(socketAsyncData);
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
            if (single_player)
                server_socket.Close();

            if (connected == 1)
            {
                connected = -1;
                client_socket.Shutdown(SocketShutdown.Both);

                client_socket.Disconnect(true);
                if (client_socket.Connected)
                    Debug.Log("We're still connnected");
                else
                    Debug.Log("We're disconnected");

                client_socket.Close();
            }

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