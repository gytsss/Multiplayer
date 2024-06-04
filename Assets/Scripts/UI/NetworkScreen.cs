using System;
using System.Net;
using UnityEngine.UI;

public class NetworkScreen : MonoBehaviourSingleton<NetworkScreen>
{
    public Button connectBtn;
    public Button startServerBtn;
    public InputField portInputField;
    public InputField addressInputField;
    public InputField nameInputField;
    public ClientNetworkManager clientNetworkManager;
    public ServerNetworkManager serverNetworkManager;

    protected override void Initialize()
    {
        connectBtn.onClick.AddListener(OnConnectBtnClick);
        startServerBtn.onClick.AddListener(OnStartServerBtnClick);
    }

    void OnConnectBtnClick()
    {
        IPAddress ipAddress = IPAddress.Parse(addressInputField.text);
        int port = Convert.ToInt32(portInputField.text);
        string name = nameInputField.text;
        
        clientNetworkManager.StartClient(ipAddress, port, name);
        
        SwitchToChatScreen();
    }

    void OnStartServerBtnClick()
    {
        int port = Convert.ToInt32(portInputField.text);
        serverNetworkManager.StartServer(port);
        SwitchToChatScreen();
    }

    void SwitchToChatScreen()
    {
        ChatScreen.Instance.gameObject.SetActive(true);
        this.gameObject.SetActive(false);
    }
}
