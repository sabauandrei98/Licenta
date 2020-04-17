using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

public class NetworkClient : MonoBehaviour
{
    //HOST, PORT & TOKEN
    public string SERVER_ADDRESS = "localhost";
    public int SERVER_PORT = 50000;
    public string SERVER_TOKEN = "token1";
    NetworkStream net_stream;

    //Sockets 
    private TcpClient tcp_socket;
    private StreamWriter socket_writer;
    private StreamReader socket_reader;
    private bool socket_connected = false;

    private string SOCKET_READ_DATA = "";
    private string SOCKET_WRITE_DATA = "";

    private float SOCKETS_LOOP_TIME = 0.05f;

    //DEBUGGING VARIABLES
    int times = 0;

    void Awake()
    {
        SetupSocket();
        ValidateToken();
        AsyncSendReceive();
    }

    private async void ValidateToken()
    {
        if (socket_connected)
        {
            await socket_writer.WriteLineAsync("token1");
            await socket_writer.FlushAsync();

            string data = await socket_reader.ReadLineAsync();
            Debug.Log("data: " + data);
        }
    }

    private async void AsyncSendReceive()
    {
        try
        {
            while (socket_connected)
            {
                string data = await socket_reader.ReadLineAsync();
                Debug.Log("data: " + data);

                await socket_writer.WriteLineAsync("salut");
                await socket_writer.FlushAsync();

                await WaitAsync(SOCKETS_LOOP_TIME);
            }
        }
        catch(Exception e)
        {
            Debug.Log("Socket error: " + e);
        }
    }


    public void SetupSocket()
    {
        try
        {
            tcp_socket = new TcpClient(SERVER_ADDRESS, SERVER_PORT);
            net_stream = tcp_socket.GetStream();
            socket_writer = new StreamWriter(net_stream);
            socket_reader = new StreamReader(net_stream);

            socket_connected = true;
        }
        catch (Exception e)
        {
            Debug.Log("Socket error: " + e);
        }
    }

    private async Task WaitAsync(float time)
    {
        await Task.Delay(TimeSpan.FromSeconds(time));
    }

    void OnApplicationQuit()
    {
        Debug.Log("Application ending after " + Time.time + " seconds");
        closeSocket();
    }

    public void closeSocket()
    {
        try
        {
            socket_connected = false;
            socket_writer.Close();
            socket_reader.Close();
            tcp_socket.Close();
        }
        catch(Exception e)
        {
            Debug.Log("Error while trying to close the socket: " + e);
        }
    }
}