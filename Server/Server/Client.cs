using System;
using System.Net;
using System.Net.Sockets;

namespace DedicatedServer
{
    class Client
    {
        public static int dataBufferSize = 4096;
        public int id;
        public TCP tcp;

        public Client(int _clientId)
        {
            id = _clientId;
            tcp = new TCP(id);
        }

        public class TCP
        {
            public TcpClient socket;

            private readonly int id;
            private NetworkStream stream;
            private byte[] receivedBuffer;

            public TCP(int _id)
            {
                id = _id;
            }

            public void Connect(TcpClient _socket)
            {
                socket = _socket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                stream = socket.GetStream();
                receivedBuffer = new byte[dataBufferSize];
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

                    //handle data
                    stream.BeginRead(receivedBuffer, 0, dataBufferSize, ReceivedCallback, null);
                }
                catch(Exception _ex)
                {
                    Console.WriteLine($"Error receiving TCP data: {_ex}");
                    //disconnect
                }
            }
        }
    }
}
