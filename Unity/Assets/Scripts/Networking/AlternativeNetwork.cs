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


    private bool canSend = true;
    private string received = "";

    private Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private byte[] _recieveBuffer = new byte[8142];

    private const string UNITY_TOKEN = "439b3a25b555b3bc8667a09a036ae70c";

    void Start()
    {
        connect_to_server_button.onClick.AddListener(delegate { ConnectToServerButton(); });
        ready_button.onClick.AddListener(delegate { ReadyButton(); });
    }

    public async void ConnectToServerButton()
    {
        SetupServer();
        await Task.Factory.StartNew(CheckToken);
    }

    private async void ReadyButton()
    {
        SendData(System.Text.Encoding.Default.GetBytes("ping"));

        string result = await ReceiveData();
        Debug.Log("msg:" + result);

        SendData(System.Text.Encoding.Default.GetBytes("ping"));

        result = await ReceiveData();
        Debug.Log("msg:" + result);
    }

    private async Task CheckToken()
    {
        try
        {
            SendData(System.Text.Encoding.Default.GetBytes(UNITY_TOKEN));

            string result = await ReceiveData();
            Debug.Log("SERVER:" + result);

            SetConnectionStatus(Color.green, "Connected to the server !");
        }
        catch
        {
            SetConnectionStatus(Color.red, "Error validating the token !");
        }
    }


    private async Task NetworkFlow()
    {
        string result = await ReceiveData();
        Debug.Log("msg:" + result);

        SendData(System.Text.Encoding.Default.GetBytes("ping"));

        result = await ReceiveData();
        Debug.Log("msg:" + result);

        SendData(System.Text.Encoding.Default.GetBytes("ping"));

        result = await ReceiveData();
        Debug.Log("msg:" + result);

        SendData(System.Text.Encoding.Default.GetBytes("ping"));

        result = await ReceiveData();
        Debug.Log("msg:" + result);

        SendData(System.Text.Encoding.Default.GetBytes("ping"));

        result = await ReceiveData();
        Debug.Log("msg:" + result);

        SendData(System.Text.Encoding.Default.GetBytes("ping"));

        result = await ReceiveData();
        Debug.Log("msg:" + result);

        SendData(System.Text.Encoding.Default.GetBytes("ping"));
    }
 
    


    private void SetupServer()
    {
        try
        {
            _clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, 50000));
        }
        catch (SocketException ex)
        {
            SetConnectionStatus(Color.red, "Could not connect to the server !");
            Debug.Log(ex.Message);
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
        received = System.Text.Encoding.Default.GetString(_recieveBuffer);

        //Start receiving again
        _clientSocket.BeginReceive(_recieveBuffer, 0, _recieveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
    }

    private void SendData(byte[] data)
    {
        SocketAsyncEventArgs socketAsyncData = new SocketAsyncEventArgs();
        socketAsyncData.SetBuffer(data, 0, data.Length);
        _clientSocket.SendAsync(socketAsyncData);
    }

    private async Task<string> ReceiveData()
    {
        while (true)
        {
            if (received != "")
            {
                string result = received;
                received = "";
                return result;
            }
        }
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
        _clientSocket.Close();
    }
}