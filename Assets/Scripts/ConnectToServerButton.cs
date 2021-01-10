using UnityEngine;
using UnityEngine.UI;

public class ConnectToServerButton : MonoBehaviour
{
    [SerializeField] private string ip;
    [SerializeField] private Button button;
    private Networker networker;

    private void Start()
    {
        button.onClick.AddListener(() => Connect());
        networker = GameObject.FindObjectOfType<Networker>();
    }

    private void Connect()
    {
        if(networker != null)
            networker.TryConnect(ip);
        else
            Debug.Log("Cannot find networker.");
    }
}