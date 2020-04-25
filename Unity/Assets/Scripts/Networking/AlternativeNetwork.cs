using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine.UI;

public class AlternativeNetwork : MonoBehaviour
{
    public Text connection_status;
    public Button connect_to_server_button;
    public Button ready_button;


    private string from_server_info = "";

    private Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private byte[] _recieveBuffer = new byte[512];

    private const string UNITY_TOKEN = "439b3a25b555b3bc8667a09a036ae70c";
    private bool connected = false;

    void Start()
    {
        connect_to_server_button.onClick.AddListener(delegate { ConnectToServerButton(); });
        ready_button.onClick.AddListener(delegate { ReadyButton(); });
    }

    void Update()
    {
        if (from_server_info != "")
        {
            Debug.Log("Recv: <" + from_server_info + ">");
            CommandHandler(from_server_info);
            from_server_info = "";
        }
    }

    void CommandHandler(string cmd)
    {
        if(cmd == "TOKEN OK")
        {
            SetConnectionStatus(Color.green, "Connected to the server! Token verified !");
            ready_button.gameObject.SetActive(true);
        }

        if (cmd == "READY OK")
        {
            SetConnectionStatus(Color.blue, "You are ready !");
        }
    }

    public async void ConnectToServerButton()
    {
        SetupServer();
        
        try
        {
            SendData(System.Text.Encoding.Default.GetBytes(UNITY_TOKEN));
        }
        catch
        {
            SetConnectionStatus(Color.red, "Error validating the token !");
        }        
    }



    public async void ReadyButton()
    {
        try
        {
            SendData(System.Text.Encoding.Default.GetBytes("READY"));
        }
        catch
        {
            SetConnectionStatus(Color.blue, "Error while getting ready !");
        }
    }


    private async void SetupServer()
    {
        try
        {
            _clientSocket.Connect("127.0.0.1", 50000);
            connected = true;
        }
        catch (SocketException ex)
        {
            string error = ex.Message;
            Debug.Log(error);

            if (!error.Contains("A connect request was made on an already connected socket."))
                SetConnectionStatus(Color.red, "Could not connect to the server");
            return;
        }

       _clientSocket.BeginReceive(_recieveBuffer, 0, _recieveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
    }

    private void ReceiveCallback(IAsyncResult AR)
    {
        //Check how much bytes are recieved and call EndRecieve to finalize handshake
        int recieved = _clientSocket.EndReceive(AR);
        
        if (recieved <= 0)
            return;

        //Copy the recieved data into new buffer , to avoid null bytes
        byte[] recData = new byte[recieved];
        Buffer.BlockCopy(_recieveBuffer, 0, recData, 0, recieved);

        //Process data here the way you want , all your bytes will be stored in recData
        from_server_info = System.Text.Encoding.Default.GetString(recData);

        //Start receiving again
        _clientSocket.BeginReceive(_recieveBuffer, 0, _recieveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
    }

    private void SendData(byte[] data)
    {
        SocketAsyncEventArgs socketAsyncData = new SocketAsyncEventArgs();
        socketAsyncData.SetBuffer(data, 0, data.Length);
        _clientSocket.SendAsync(socketAsyncData);
    }


    private async void SetConnectionStatus(Color color, string message)
    {
        connection_status.text = message;
        connection_status.color = color;
    }


    void OnApplicationQuit()
    {
        connected = false;
        
        _clientSocket.Shutdown(SocketShutdown.Both);

        _clientSocket.Disconnect(true);
        if (_clientSocket.Connected)
            Debug.Log("We're still connnected");
        else
            Debug.Log("We're disconnected");


        _clientSocket.Close();
    }
}