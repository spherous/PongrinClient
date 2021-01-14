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
        } catch (Exception e) {
            Debug.Log($"Failed to connect with error:\n{e}");
            return;
        }
        networker.stream = networker.client.GetStream();
        Debug.Log($"Sucessfully connected.");
        networker.actions.Enqueue(() => SceneManager.LoadScene("Lobby", LoadSceneMode.Single));
        networker.readBuffer = new byte[256];
        networker.stream.BeginRead(networker.readBuffer, 0, 256, new AsyncCallback(RecieveMessage), networker);
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

    public void MessageServer(string message)
    {
        Debug.Log($"Sending message: {message} to server.");
        byte[] writeBuffer = System.Text.Encoding.ASCII.GetBytes(message);
        
        lock(lockObj)
        {
            if(sending)
                writeQueue.Enqueue(writeBuffer);
            else
            {
                sending = true;
                stream.BeginWrite(writeBuffer, 0, writeBuffer.GetLength(0), new AsyncCallback(SendMessage), this);
            }
        }
    }

    private void SendMessage(IAsyncResult ar)
    {
        Networker networker = (Networker)ar.AsyncState;
        networker.stream.EndWrite(ar);
        Debug.Log("Message sent.");
        
        lock(lockObj)
        {
            switch(writeQueue.Count)
            {
                case 0:
                    sending = false;
                    break;
                default:
                    if(networker.writeQueue.TryDequeue(out byte[] writeBuffer))
                    {
                        sending = true;
                        stream.BeginWrite(writeBuffer, 0, writeBuffer.GetLength(0), new AsyncCallback(SendMessage), this);
                        return;
                    }
                    sending = false;
                    return;
            }
        }
    }
     
    private void RecieveMessage(IAsyncResult ar)
    {
        Networker networker = (Networker)ar.AsyncState;
        int bytesRead = networker.stream.EndRead(ar);

        if(bytesRead == 0)
        {
            networker.stream.BeginRead(networker.readBuffer, 0, 256, new AsyncCallback(RecieveMessage), networker);
            return;
        }

        string responseData = System.Text.Encoding.ASCII.GetString(networker.readBuffer, 0, bytesRead);
        Debug.Log($"Response from server: {responseData}");
    }
}