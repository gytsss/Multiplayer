using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatScreenManager : MonoBehaviour
{
    [SerializeField] private GameObject _chatScreenCanvas; 
    
    
    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.Tab))
        {
            TurnChatScreen();
        }
    }
    
    private void TurnChatScreen()
    {
        _chatScreenCanvas.SetActive(!_chatScreenCanvas.activeSelf);
    }
}
