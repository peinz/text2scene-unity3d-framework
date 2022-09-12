using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.Netcode.Transports.UTP;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using Mapbox.Unity.Map.TileProviders;

public class GameManager : NetworkBehaviour
{

    public string ServerIp = "127.0.0.1";
    public ushort ServerPort = 7777;
    public string osmMapDataFilePath = "";

    public GameObject Lobby; // LobbyArea
    public GameObject mapBuilder;
    public GameObject MapboxMapGameObject;
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
        generateMapObjects();

        networkManager.StartHost();
        Lobby.Destroy();
    }

    public void StartClient()
    {
        generateMapObjects();

        networkManager.StartClient();
        Lobby.Destroy();
    }

    void generateMapObjects()
    {
        // read osm data
        var osmReader = new MapReader();
        var osmData = osmReader.ParseOsmData(osmMapDataFilePath);
        StartCoroutine(mapBuilder.GetComponent<MapBuilder>().BuildAllObjects(osmData));

        // configure mapBoxMap
        AbstractMap MapboxMap = MapboxMapGameObject.GetComponent<AbstractMap>();
        MapboxMap.Initialize(new Vector2d(osmData.Bounds.CenterLatLon.x, osmData.Bounds.CenterLatLon.y), 15);
        RangeTileProvider tileProvider = new RangeTileProvider();
        RangeTileProviderOptions tileProviderOptions = new RangeTileProviderOptions();
        tileProviderOptions.SetOptions(OsmBounds.LenghtFactor, OsmBounds.WidthFactor, OsmBounds.LenghtFactor, OsmBounds.WidthFactor);
        tileProvider.SetOptions(tileProviderOptions);
        MapboxMap.TileProvider = tileProvider;
        MapboxMap.SetZoom(15.29f);
        MapboxMap.UpdateMap();
        MapboxMap.Terrain.EnableCollider(true);
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