using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.Netcode.Transports.UTP;

public class GameManager : NetworkBehaviour
{

    public string ServerIp = "127.0.0.1";
    public ushort ServerPort = 7777;

    public GameObject Lobby; // LobbyArea
    NetworkManager networkManager;

    void Start()
    {
        networkManager = NetworkManager.Singleton;
        networkManager.OnServerStarted += OnServerStarted;

        networkManager.GetComponent<UnityTransport>().SetConnectionData(
            ServerIp,  // The IP address is a string
            ServerPort,  // The port number is an unsigned short
            "0.0.0.0"
        );
    }

    public void StartHost()
    {
        networkManager.StartHost();
        Lobby.Destroy();
    }

    public void StartClient()
    {
        networkManager.StartClient();
        Lobby.Destroy();
    }

    void OnServerStarted()
    {
        Debug.Log("Server started");

        // show stationUis only on Host
        var stationUIs = GameObject.FindGameObjectsWithTag("StationUI");
        foreach(var stationUI in stationUIs){
            stationUI.GetComponent<Canvas>().enabled = true;
        }
    }

}