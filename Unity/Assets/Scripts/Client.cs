using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

public class Client : MonoBehaviour
{
    public static Client instance;
    public static int dataBufferSize = 4096;

    public string ip = "127.0.0.1";
    public int port = 26950;
    public int myId = 0;
    public TCP tcp;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object");
            Destroy(this);
        }
    }

    private void Start()
    {
        tcp = new TCP();
    }

    public void ConnectToServer()
    {
        tcp.Connect();
    }

    public class TCP
    {
        public TcpClient socket;
        private NetworkStream stream;
        private byte[] receivedBuffer;

        public void Connect()
        {
            socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            receivedBuffer = new byte[dataBufferSize];
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
            
        }

        private void ConnectCallback(IAsyncResult _result)
        {
            if (!socket.Connected)
                Debug.Log("Could not connect to the server !");

            socket.EndConnect(_result);

            if(!socket.Connected)
            {
                return;
            }

            stream = socket.GetStream();

            stream.BeginRead(receivedBuffer, 0, dataBufferSize, ReceivedCallback, null);
        }

        private void ReceivedCallback(IAsyncResult _result)
        {
            try
            {
                int _byteLength = stream.EndRead(_result);
                if(_byteLength <= 0)
                {
                    //disconnect
                    return;
                }

                byte[] _data = new byte[_byteLength];
                Array.Copy(receivedBuffer, _data, _byteLength);

                stream.BeginRead(receivedBuffer, 0, dataBufferSize, ReceivedCallback, null);
            }
            catch
            {
                //disconnect
            }

        }
    }
}
