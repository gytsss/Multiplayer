using System;
using System.Net;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ChatScreen : MonoBehaviourSingleton<ChatScreen>
{
    
    public Text messages;
    public InputField inputMessage;
    
    public UnityEvent<string, int> onChatMessage;
    public UnityEvent<string> onSendChatMessage;

    protected override void Initialize()
    {
        inputMessage.onEndEdit.AddListener(OnEndEdit);

        onChatMessage.AddListener(OnChatMessage);
        this.gameObject.SetActive(false);
    }

    private void OnChatMessage(string msg, int ID)
    {
        messages.text += msg + System.Environment.NewLine;
    }
    

    void OnEndEdit(string str)
    {
        if (inputMessage.text != "")
        {
            onSendChatMessage.Invoke(inputMessage.text);

            inputMessage.ActivateInputField();
            inputMessage.Select();
            inputMessage.text = "";
            
        }
        
    }

}
