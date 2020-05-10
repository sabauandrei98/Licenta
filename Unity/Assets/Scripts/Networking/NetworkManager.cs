using UnityEngine;
using System;
using System.Net.Sockets;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Threading;

public class NetworkManager : MonoBehaviour
{
    public Text connection_status;
    public Text ip_text;
    public Text port_text;
    public Button connect_to_server_button;
    public Button ready_button;
    public InputField ip_input_field;
    public InputField port_input_field;

    private Socket client_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private byte[] receive_buffer = new byte[512];

    private const string UNITY_TOKEN = "439b3a25b555b3bc8667a09a036ae70c";
    private int connected = 0;
    private string server_cmd = "";
    private string ide_token = "";


    void Awake()
    {
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
            if (server_cmd != "")
            {
                Debug.Log("Recv: <" + server_cmd + ">");
                CommandHandler(server_cmd);
                server_cmd = "";
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
        connect_to_server_button.onClick.AddListener(delegate { ConnectToServerButton(); });
        ready_button.onClick.AddListener(delegate { ReadyButton(); });
    }

    void CommandHandler(string cmd)
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
            StartCoroutine(HandleInitialData(cmd.Split(':')[1]));
        }

        if (cmd.Split(':')[0] == "ROUND")
        {
            GameManager game_manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
            game_manager.ProcessOneRound(cmd.Split(':')[1]);
            SendData(System.Text.Encoding.Default.GetBytes(game_manager.GetSessionData()));
        }
    }

    private IEnumerator HandleInitialData(string data)
    {
        SceneManager.LoadScene("Game");
        yield return new WaitForSeconds(0.5f);

        GameManager game_manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        game_manager.instance_token = ide_token;
        game_manager.PrepareGame(data, false);
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
                Disconnect();
            }

            Thread.Sleep(1000);
        }
    }

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
        server_cmd = System.Text.Encoding.Default.GetString(recData);

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