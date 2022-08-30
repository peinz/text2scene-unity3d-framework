using UnityEngine;
using Unity.Netcode;
using System.Collections;

[RequireComponent(typeof(NetworkManager))]
public class NetworkOrchestrator : MonoBehaviour
{
    NetworkManager networkManager;

    void Start()
    {
        networkManager = GetComponent<NetworkManager>();
        networkManager.OnServerStarted += OnServerStarted;
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