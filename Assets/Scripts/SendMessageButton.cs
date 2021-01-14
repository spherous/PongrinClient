using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SendMessageButton : MonoBehaviour
{
    [SerializeField] private Button button;
    private Networker networker;

    private void Start() {
        networker = GameObject.FindObjectOfType<Networker>();
        button.onClick.AddListener(() => MessageServer());
    }

    private void MessageServer()
    {
        if(networker != null)
        {
            networker.MessageServer("Hello Server!");
        }
        else
            Debug.Log("Cannot find networker.");
    }
}
