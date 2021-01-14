using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections.Concurrent;

public class Networker : MonoBehaviour
{
    private IPAddress ip;
    public int port = 1234;
    TcpClient client;
    NetworkStream stream;
    byte[] readBuffer;
    ConcurrentQueue<byte[]> writeQueue = new ConcurrentQueue<byte[]>();
    readonly object lockObj = new object();
    bool sending = false;

    ConcurrentQueue<Action> actions = new ConcurrentQueue<Action>();

    private void Awake() => DontDestroyOnLoad(gameObject);

    private void Update() {
        while(actions.TryDequeue(out Action a))
            a.Invoke();
    }
    
    public void TryConnect(string serverIP)
    {            
        Debug.Log($"Attempting to connect to {serverIP}:{port}.");
        ip = IPAddress.Parse(serverIP);
        client = new TcpClient();
        try {
            client.BeginConnect(ip, port, new AsyncCallback(ConnectCallback), this);
        } catch(Exception e) {
            Debug.Log($"Failed to connect to {serverIP}:{port} with error:\n{e}");
        }
    }

    void ConnectCallback(IAsyncResult ar)
    {
        Networker networker = (Networker)ar.AsyncState;
        try {
            networker.client.EndConnect(ar);
            networker.stream = networker.client.GetStream();
            networker.readBuffer = new byte[256];
            networker.stream.BeginRead(networker.readBuffer, 0, 256, new AsyncCallback(RecieveMessage), networker);
        } catch (Exception e) {
            Debug.Log($"Failed to connect with error:\n{e}");
            return;
        }
        Debug.Log($"Sucessfully connected.");
        networker.actions.Enqueue(() => SceneManager.LoadScene("Lobby", LoadSceneMode.Single));
    }

    public void Disconnect()
    {
        Debug.Log("Disconnecting from server.");
        stream.Close();
        client.Close();
        Debug.Log("Sucessfully Disconnected.");
        SceneManager.LoadScene("Connection Scene", LoadSceneMode.Single);
        Destroy(this.gameObject);
    }

    public void EncodeAndMessageServer(string message)
    {
        Debug.Log($"Sending message: {message}");

        // byte[] writeBuffer = message.ObjectToByteArray();
        byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
        
        if(data.Length > byte.MaxValue)
            throw new Exception("Too long.");

        List<byte> metadata = new List<byte>();
        metadata.Add((byte)MessageType.Short_String);
        metadata.Add((byte)data.Length);
        metadata.AddRange(data);

        MessageServer(metadata.ToArray());
    }

    private void MessageServer(byte[] data)
    {
        lock(lockObj)
        {
            if(sending)
                writeQueue.Enqueue(data);
            else
            {
                sending = true;
                try {
                    stream.BeginWrite(data, 0, data.GetLength(0), new AsyncCallback(SendMessage), this);
                } catch (Exception e) {
                    Debug.Log($"Failed to write to socket:\n{e}");
                }
            }
        }
    }

    private void SendMessage(IAsyncResult ar)
    {
        Networker networker = (Networker)ar.AsyncState;
        try{
            networker.stream.EndWrite(ar);
            Debug.Log("Message sent.");
            
            lock(lockObj)
            {
                switch(networker.writeQueue.Count)
                {
                    case 0:
                        networker.sending = false;
                        break;
                    default:
                        if(networker.writeQueue.TryDequeue(out byte[] writeBuffer))
                        {
                            networker.sending = true;
                            stream.BeginWrite(writeBuffer, 0, writeBuffer.GetLength(0), new AsyncCallback(SendMessage), this);
                            return;
                        }
                        networker.sending = false;
                        return;
                }
            }
        } catch (Exception e) {
            Debug.Log($"Failed to write to socket:\n{e}");
        }
    }
     
    private void RecieveMessage(IAsyncResult ar)
    {
        Networker networker = (Networker)ar.AsyncState;
        try{
            int bytesRead = networker.stream.EndRead(ar);
            if(bytesRead == 0)
            {
                networker.stream.BeginRead(networker.readBuffer, 0, 256, new AsyncCallback(RecieveMessage), networker);
                return;
            }

            // string responseData = networker.readBuffer.ByteArrayToObject<string>();
            MessageType type = (MessageType)networker.readBuffer[0];
            int messageLength = networker.readBuffer[1];

            // byte[] buf = new byte[messageLength];
            // for(int i = 2; i < bytesRead; i++)
            //     buf[i-2] = networker.readBuffer[i];

            string responseData = System.Text.Encoding.ASCII.GetString(networker.readBuffer, 2, messageLength);
            Debug.Log($"Response from server: {responseData}");
        } catch (Exception e) {
            Debug.Log($"Failed to read from socket:\n{e}");
        }
    }
}