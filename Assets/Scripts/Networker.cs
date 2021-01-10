using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using UnityEngine.SceneManagement;

public class Networker : MonoBehaviour
{
    private IPAddress ip;
    public int port = 1234;
    TcpClient client;

    public string message;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {

        // if(TryConnect())
        // {
        //     Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
        //     NetworkStream stream = client.GetStream();
            
        //     Debug.Log($"Sending message: {message} to server.");
        //     stream.Write(data, 0, data.Length);
        //     Debug.Log("Message sent.");
            
        //     data = new Byte[256];
        //     string responseData = string.Empty;

        //     Debug.Log("Reading response from server...");
        //     int bytes = stream.Read(data, 0, data.Length);
        //     responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
        //     Debug.Log($"Response from server: {responseData}");

        //     Debug.Log("Disconnecting from server.");
        //     stream.Close();
        //     client.Close();
        //     Debug.Log("Sucessfully disconnected from server.");
        // }
    }

    public bool TryConnect(string serverIP)
    {            
        Debug.Log($"Attempting to connect to {serverIP}:{port}.");
        ip = IPAddress.Parse(serverIP);
        client = new TcpClient();
        try {
            client.Connect(ip, port);
        } catch(SocketException e) {
            Debug.Log($"Failed to connect to {serverIP}:{port} with error:\n{e}.");
            return false;
        }
        Debug.Log($"Sucessfully connected to {serverIP}:{port}.");
        SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
        return true;
    }

    public void Disconnect()
    {
        Debug.Log("Disconnecting from server.");
        client.Close();
        Debug.Log("Sucessfully Disconnected.");
        SceneManager.LoadScene("Connection Scene", LoadSceneMode.Single);
        Destroy(this.gameObject);
    }
}