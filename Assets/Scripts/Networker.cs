using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;

public class Networker : MonoBehaviour
{
    public string serverIP = "127.0.0.1";
    private IPAddress ip;
    public int port = 1234;
    TcpClient client;

    public string message;

    void Start()
    {
        ip = IPAddress.Parse(serverIP);
        TryConnect(out client);

        Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
        NetworkStream stream = client.GetStream();
        
        Debug.Log($"Sending message: {message} to server.");
        stream.Write(data, 0, data.Length);
        Debug.Log("Message sent.");
        
        data = new Byte[256];
        string responseData = string.Empty;

        Debug.Log("Reading response from server...");
        int bytes = stream.Read(data, 0, data.Length);
        responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
        Debug.Log($"Response from server: {responseData}");

        Debug.Log("Disconnecting from server.");
        stream.Close();
        client.Close();
        Debug.Log("Sucessfully disconnected from server.");
    }

    bool TryConnect(out TcpClient c)
    {
        Debug.Log($"Attempting to connect to {serverIP}:{port}.");
        TcpClient newClient = new TcpClient();
        try {
            newClient.Connect(ip, port);
        } catch(SocketException e) {
            Debug.Log($"Failed to connect to {serverIP}:{port} with error:\n{e}.");
            c = null;
            return false;
        }
        Debug.Log($"Sucessfully connected to {serverIP}:{port}.");
        c = newClient;
        return true;
    }
}