using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

public class Network_Client : MonoBehaviour
{
    public Text SERVER_ADDRESS;
    public Text SERVER_PORT;
    public Text connection_status;
    public Button ready_button;
    
    private NetworkStream net_stream;
    private TcpClient tcp_socket;
    private StreamWriter socket_writer;
    private StreamReader socket_reader;

    private string SOCKET_READ_DATA = "";
    private string SOCKET_WRITE_DATA = "";
    private bool socket_connected = false;

    private const string UNITY_TOKEN = "439b3a25b555b3bc8667a09a036ae70c";
    private const float SOCKETS_LOOP_TIME = 0.05f;
    
    /// <summary>
    /// Public method linked to the Connect_Button gameobject
    /// - Creates a socket and validates the client token
    /// - If everything is good it means the client successfully connected to the server
    /// </summary>
    public async void ConnectButtonEvent()
    {
        SetupSocket();

        if (socket_connected)
        {
            
            await ValidateToken();

            //if still connected to the server
            if (socket_connected)
            {
                ready_button.onClick.AddListener(delegate { ReadyButtonEvent(); });
                ready_button.gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    ///  Creates a TCP socket between unity client and python server
    ///  - Connect info is taken from the input fields 
    /// </summary>
    private void SetupSocket()
    {
        try
        {
            if (SERVER_ADDRESS.text == "")
                return;

            if (SERVER_PORT.text == "")
                return;

            tcp_socket = new TcpClient(SERVER_ADDRESS.text, int.Parse(SERVER_PORT.text));
            net_stream = tcp_socket.GetStream();
            socket_writer = new StreamWriter(net_stream);
            socket_reader = new StreamReader(net_stream);

            socket_connected = true;
        }
        catch
        {
            SetConnectionStatus(Color.red, "Error while trying to connect !");
        }
    }

    /// <summary>
    /// Security check of the unity platform by the server
    /// - Unity client sends UNITY_TOKEN to the python server
    /// - the token is the result of md5 encryption of the word "unity"
    /// - this process is done in order to differentiate unity clients and ide clients
    /// </summary>
    private async Task ValidateToken()
    {
        try
        {
            await socket_writer.WriteLineAsync(UNITY_TOKEN);
            socket_writer.AutoFlush = true;

            await socket_writer.WriteLineAsync(UNITY_TOKEN);

            //socket_writer.WriteLine(UNITY_TOKEN);


            SetConnectionStatus(Color.green, "Connected to the server ! \n You have 20 seconds to get ready !");
        }
        catch 
        {
            CloseSocket();
            SetConnectionStatus(Color.red, "Error in token validation !");
        }
    }

    public async void ReadyButtonEvent()
    {
        await ReadyButtonTask();
    }

    private async Task ReadyButtonTask()
    {
        try
        {
            

            /*
            string server_response = await socket_reader.ReadLineAsync();

            Debug.Log("1" + server_response);

            server_response = await socket_reader.ReadLineAsync();
            Debug.Log("2" + server_response);
            SetConnectionStatus(Color.blue, "Player ready ! \n The game will start soon !");
            */
        }
        catch
        {
            SetConnectionStatus(Color.red, "Error while getting ready !");
            CloseSocket();
        }
    }


    private async void ServerSendReceive()
    {
        try
        {
            while (socket_connected)
            {
                await socket_writer.WriteLineAsync("salut");
                await socket_writer.FlushAsync();

                string data = await socket_reader.ReadLineAsync();
                Debug.Log("data: " + data);

                await WaitAsync(SOCKETS_LOOP_TIME);
            }
        }
        catch(Exception e)
        {
            CloseSocket();
            Debug.Log("Socket error: " + e);
        }
    }

    private async Task WaitAsync(float time)
    {
        await Task.Delay(TimeSpan.FromSeconds(time));
    }

    private void SetConnectionStatus(Color color, string message)
    {
        if (connection_status == null)
            return;

        connection_status.color = color;
        connection_status.text = message;
    }


    void OnApplicationQuit()
    {
        Debug.Log("Application ending after " + Time.time + " seconds");
        CloseSocket();
    }

    public async void CloseSocket()
    {
        try
        {
            tcp_socket.Close();
            tcp_socket.Client.Close();
            tcp_socket.GetStream().Close();
            socket_connected = false;
            socket_writer.Close();
            socket_reader.Close();
            

            //if (ReadyButtonTask)

            Debug.Log("Successfully closed the socket !");
        }
        catch(Exception e)
        {
            Debug.Log("Error while trying to close the socket: " + e);
        }
    }
}