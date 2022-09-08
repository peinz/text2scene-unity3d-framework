using UnityEngine;
using Unity.Netcode;
using System.Collections;
using Unity.Netcode.Transports.UTP;

[RequireComponent(typeof(NetworkManager))]
public class NetworkOrchestrator : MonoBehaviour
{
    public string ServerIp = "127.0.0.1";
    public ushort ServerPort = 7777;
    NetworkManager networkManager;

    void Start()
    {
        networkManager = GetComponent<NetworkManager>();
        networkManager.OnServerStarted += OnServerStarted;

        networkManager.GetComponent<UnityTransport>().SetConnectionData(
            ServerIp,  // The IP address is a string
            ServerPort,  // The port number is an unsigned short
            "0.0.0.0"
        );
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