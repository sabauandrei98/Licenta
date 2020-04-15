using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;



public class NetworkClient : MonoBehaviour
{
    //HOST & PORT
    public String SERVER_ADDRESS = "localhost";
    public Int32 SERVER_PORT = 50000;

    NetworkStream net_stream;

    //Sockets 
    private TcpClient tcp_socket;
    private StreamWriter socket_writer;
    private StreamReader socket_reader;
    private bool socket_connected = false;
    private bool socket_open = true;

    private string SOCKET_READ_DATA = "";
    private string SOCKET_WRITE_DATA = "";

    private float SOCKETS_LOOP_TIME = 0.05f;
    private float SOCKET_RETRY_READ_TIME = 0.1f;

    //DEBUGGING VARIABLES
    int times = 0;

    void Awake()
    {
        SetupSocket();
    }

    void Start()
    {
        if (socket_connected)
        {
            Thread pipeline_data = new Thread(new ThreadStart(AsyncSendReceive));
            pipeline_data.Start();
        }
    }


    private async void AsyncSendReceive()
    {
        while (socket_connected)
        {

            Debug.Log("socket ready");
            if (socket_open)
            {
                Debug.Log("can read");
                ReadSocket();

                times++;
                Debug.Log("have read");
                Debug.Log(times.ToString());
                Debug.Log(SOCKET_READ_DATA);
                socket_open = false;

            }
            else
            {
                WriteSocket("hello from client");
                socket_open = true;
            }

            await WaitAsync(SOCKETS_LOOP_TIME);
        }
    }

    private async Task WaitAsync(float time)
    {
        await Task.Delay(TimeSpan.FromSeconds(time));
    }

    void Update()
    {
        Debug.Log("Update");
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
            // Something went wrong
            Debug.Log("Socket error: " + e);
        }
    }



    public void WriteSocket(string line)
    {
        socket_writer.Write(line);
        socket_writer.Flush();
    }

    public async void ReadSocket()
    {
        while (true)
        {
            Debug.Log("try to get data");
            if (net_stream.DataAvailable)
            {
                Debug.Log("data available to read");
                SOCKET_READ_DATA = socket_reader.ReadLine();
                return;
            }
            await WaitAsync(SOCKET_RETRY_READ_TIME);
        }
    }

    void OnApplicationQuit()
    {
        Debug.Log("Application ending after " + Time.time + " seconds");

        closeSocket();
    }

    public void closeSocket()
    {
        socket_connected = false;
        socket_writer.Close();
        socket_reader.Close();
        tcp_socket.Close();
    }

}