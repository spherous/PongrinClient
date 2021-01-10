using UnityEngine;
using UnityEngine.UI;

public class DisconnectButton : MonoBehaviour
{
    [SerializeField] private Button button;
    private Networker networker;

    private void Start() 
    {
        networker = GameObject.FindObjectOfType<Networker>();
        button.onClick.AddListener(() => Disconnect());
    }

    private void Disconnect()
    {
        if(networker != null)
            networker.Disconnect();
        else
            Debug.Log("Cannot find networker.");
    }
}
