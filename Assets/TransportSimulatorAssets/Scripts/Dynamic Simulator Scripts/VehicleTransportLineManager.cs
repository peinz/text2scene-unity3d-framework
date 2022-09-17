using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class VehicleTransportLineManager : NetworkBehaviour
{
    public NetworkVariable<bool> darkmodeEnabled = new NetworkVariable<bool>(false);

   [System.Serializable]
   public class VehicleEntry
   {
       public Vehicle.VehicleType type;
       public Vehicle.Vehicle vehicle;
   }

   public VehicleEntry[] vehiclePrefabs;    

    NetworkVariable<Vehicle.VehicleType> vehicleType = new NetworkVariable<Vehicle.VehicleType>(Vehicle.VehicleType.Bus);
    // FIXME: Setting NetworkList imediatly throws error, but it continues to work (https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/issues/1367)
    //        setting it in Start Method fixes that, but that breaks client
    NetworkList<Vector3> wayPoints = new NetworkList<Vector3>();
    NetworkList<Vector3> lastNodePoints = new NetworkList<Vector3>();
    NetworkList<Vector3> stationPoints = new NetworkList<Vector3>();

    NetworkVariable<int> waypointIndex = new NetworkVariable<int>(1); // skip first point
    NetworkVariable<Vector3> vehiclePosition = new NetworkVariable<Vector3>(new Vector3());

    Vehicle.Vehicle vehicle;
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
        foreach(var entry in vehiclePrefabs){
            if(entry.type == vehicleType.Value){
                vehicle = Instantiate(entry.vehicle);
                transporter = vehicle.gameObject.GetComponent<Vehicle.Transporter>();
                break;
            }
        }

        if(!vehicle){
            throw new System.Exception("missing vehiclePrefab for: " + vehicleType.Value);
        }

        vehicle.SetPosition(wayPoints[0]);
        vehicle.EnableLights(darkmodeEnabled.Value);
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
                if(transporter && transporter.stopTransportingOnNextHalt){
                    transporter.StopTransporting();
                }
                yield return new WaitForSeconds(2f);
            }

            // jump over gaps in path
            if (lastNodePoints.Contains(targetPoint) && waypointIndex.Value < wayPoints.Count-1){
                vehiclePosition.Value = wayPoints[waypointIndex.Value+1];
            }

            waypointIndex.Value++;
        }

        // route finished => destroy lineManager
        gameObject.Destroy();
    }

    IEnumerator clientUpdateVehiclePosition()
    {
        while(true){
            vehicle.SetPosition(vehiclePosition.Value);
            var targetPoint = wayPoints[waypointIndex.Value];
            yield return vehicle.MoveTo(targetPoint);
        }

    }

    void OnDestroy()
    {
        if(vehicle){
            vehicle.gameObject.Destroy();
        }
    }
}
