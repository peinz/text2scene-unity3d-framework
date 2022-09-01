using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class VehicleTransportLineManager : NetworkBehaviour
{
    public NetworkVariable<bool> darkmodeEnabled = new NetworkVariable<bool>(false);

    NetworkVariable<Vehicle.VehicleType> vehicleType = new NetworkVariable<Vehicle.VehicleType>(Vehicle.VehicleType.Bus);
    // FIXME: Setting NetworkList imediatly throws error, but it continues to work (https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/issues/1367)
    //        setting it in Start Method fixes that, but that breaks client
    NetworkList<Vector3> wayPoints = new NetworkList<Vector3>();
    NetworkList<Vector3> lastNodePoints = new NetworkList<Vector3>();
    NetworkList<Vector3> stationPoints = new NetworkList<Vector3>();

    NetworkVariable<int> waypointIndex = new NetworkVariable<int>(1); // skip first point
    NetworkVariable<Vector3> vehiclePosition = new NetworkVariable<Vector3>(new Vector3());

    GameObject vehicleGameObject;
    Vehicle.IVehicle vehicle;
    Vehicle.Transporter transporter;


    public void Initialize(Vehicle.VehicleType vehicleType, List<Vector3> wayPoints, List<Vector3> lastNodePoints, List<Vector3> stationPoints)
    {
        this.vehicleType.Value = vehicleType;
        this.wayPoints = new NetworkList<Vector3>(wayPoints);
        this.lastNodePoints = new NetworkList<Vector3>(lastNodePoints);
        this.stationPoints = new NetworkList<Vector3>(stationPoints);

        this.vehiclePosition.Value = wayPoints[0];
    }

    public override void OnNetworkSpawn()
    {
        spawnVehicle();
        if(IsOwner) StartCoroutine(updateVehiclePosition());
        else StartCoroutine(clientUpdateVehiclePosition());
    }

    void spawnVehicle()
    {

        var vehicleName = "";
        if(vehicleType.Value == Vehicle.VehicleType.Bus) vehicleName = "Bus";
        if(vehicleType.Value == Vehicle.VehicleType.Subway) vehicleName = "Subway";
        if(vehicleType.Value == Vehicle.VehicleType.Train) vehicleName = "Train";
        if(vehicleType.Value == Vehicle.VehicleType.Tram) vehicleName = "Tram";

        if(vehicleType.Value == Vehicle.VehicleType.Bus)
        {
            vehicleGameObject = Instantiate(Resources.Load("Vehicles/Prefabs/" + vehicleName)) as GameObject; // Instantiate bus vehicle.
            vehicle = vehicleGameObject.AddComponent<Vehicle.StreetVehicle>();
        }
        else
        {
            vehicleGameObject = Instantiate(Resources.Load("Vehicles/Prefabs/" + vehicleName)) as GameObject; // Instantiate leading wagon of train vehicle.
            var multiSegmentRailVehicle = vehicleGameObject.AddComponent<Vehicle.MultiSegmentRailVehicle>();
            for(int i = 0; i < 4; i++){
                var wagon = Instantiate(Resources.Load("Vehicles/Prefabs/" + vehicleName + " Wagon")) as GameObject; // Instantiate other wagons of train vehicle.
                multiSegmentRailVehicle.AddSegment(wagon);
            }
            vehicle = multiSegmentRailVehicle;
        }

        vehicle.SetPosition(wayPoints[0]);
        vehicle.EnableLights(darkmodeEnabled.Value);

        // add transporter script
        transporter = vehicleGameObject.AddComponent<Vehicle.Transporter>();
    }

    IEnumerator updateVehiclePosition()
    {
        while(waypointIndex.Value < wayPoints.Count){
            var targetPoint = wayPoints[waypointIndex.Value];

            // move vehicle
            vehicle.SetPosition(vehiclePosition.Value); // this is neccessary to set correct position for late joining clients
            yield return vehicle.MoveTo(targetPoint);
            vehiclePosition.Value = vehicle.GetPosition();

            // wait on station
            if (stationPoints.Contains(targetPoint)){
                transporter.StopTransporting();
                yield return new WaitForSeconds(2f);
            }

            // jump over gaps in path
            if (lastNodePoints.Contains(targetPoint) && waypointIndex.Value < wayPoints.Count-1){
                vehiclePosition.Value = wayPoints[waypointIndex.Value+1];
            }

            waypointIndex.Value++;
        }

        // route finished => destroy lineManager
        if(vehicleGameObject) vehicleGameObject.Destroy();
        GetComponent<NetworkObject>().Despawn();
    }

    IEnumerator clientUpdateVehiclePosition()
    {
        while(true){
            vehicle.SetPosition(vehiclePosition.Value);
            yield return null;
        }

    }
}
