using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConnectButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_InputField inputField;
    private Networker networker;

    private void Start() {
        networker = GameObject.FindObjectOfType<Networker>();
        button.onClick.AddListener(() => Connect());
    }

    private void Connect()
    {
        if(networker != null)
        {
            string ip = string.IsNullOrEmpty(inputField.text) ? "127.0.0.1" : inputField.text;
            networker.TryConnect(ip);
        }
        else
            Debug.Log("Cannot find networker.");
    }
}