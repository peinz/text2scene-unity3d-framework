using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vehicle {

    public class StreetVehicle : MonoBehaviour, Vehicle.IVehicle
    {
        int targetIndex = 0; // Index of the next point which we are moving to.
        int maxIndex;  

        bool isWaiting = false;

        float VehicleMaxSpeed = 100f; 

        List<Vector3> targetWaypoints = new List<Vector3>();
        List<Vector3> stationCoordinates = new List<Vector3>();
        List<Vector3> pathLastNode = new List<Vector3>();

        public void Initialize(Vector3 position, List<Vector3> _targetWaypoints, List<Vector3> _stationCoordinates, List<Vector3> _pathLastNode)
        {       
            targetWaypoints = _targetWaypoints;
            stationCoordinates = _stationCoordinates;
            pathLastNode = _pathLastNode;

            transform.position = position;

            maxIndex = targetWaypoints.Count - 1; 
        }

        void Update()
        {

            if(targetWaypoints.Count == 0) return;

            if (IngameMenu.DarkModeOn)
            {
                transform.GetChild(1).gameObject.SetActive(true);
            }
            else
            {
                transform.GetChild(1).gameObject.SetActive(false);
            }
            if (isWaiting)
            {
                return; // Vehicle doesnt move uppon reaching a station.
            }

            // The vehicles moves to the next point from the "MoveToTarget" list.
            transform.position = Vector3.MoveTowards(transform.position, targetWaypoints[targetIndex], Time.deltaTime * VehicleMaxSpeed);

            if (transform.position == targetWaypoints[targetIndex])
            {
                if (stationCoordinates.Contains(transform.position))
                {
                    // If the vehicle reaches a station, it stops.
                    StartCoroutine(Waiting());
                }
                if(targetIndex + 1 > maxIndex) 
                {
                    // If the vehicle reaches the last point of the list, it destroys itself.
                    Destroy(gameObject);
                    return;
                }
                else if (pathLastNode.Contains(transform.position))
                {
                    // If the road/railroad consists of more than one part we have to teleport the vehicle to the other part upon reaching.
                    // the end of one part.
                    int index = targetWaypoints.IndexOf(transform.position);
                    transform.position = targetWaypoints[index + 1];
                }
                transform.LookAt(targetWaypoints[targetIndex + 1]); // Rotates te vehicle in the right direction.

                targetIndex += 1;           
            }
        }

        /// <summary>
        /// Stops the vehicle movement for 2 seconds.
        /// </summary>
        /// <returns></returns>
        IEnumerator Waiting()
        {
            isWaiting = true;
            yield return new WaitForSeconds(2);
            isWaiting = false;
        }

    }



}

