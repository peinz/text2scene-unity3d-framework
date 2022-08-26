using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vehicle {

    public class MultiSegmentRailVehicle : MonoBehaviour, Vehicle.IVehicle
    {
        int targetIndex = 0; // Index of the next point which we are moving to.
        List<int> wagonTargetIndexs = new List<int>();
        int maxIndex;  

        bool isWaiting = false;

        float VehicleMaxSpeed = 100f; 
        float sectionLength = 25f;

        List<Vector3> targetWaypoints = new List<Vector3>();
        List<Vector3> stationCoordinates = new List<Vector3>();
        List<Vector3> pathLastNode = new List<Vector3>();
        List<GameObject> wagons = new List<GameObject>();

        public void Initialize(Vector3 position, List<Vector3> _targetWaypoints, List<Vector3> _stationCoordinates, List<Vector3> _pathLastNode)
        {       
            targetWaypoints = _targetWaypoints;
            stationCoordinates = _stationCoordinates;
            pathLastNode = _pathLastNode;

            transform.position = position;
            for(int i=0; i<wagons.Count; i++){
                var wagonPosition = new Vector3();
                wagonPosition.z = -sectionLength * (i+1);
                wagons[i].transform.position = transform.position + wagonPosition;
            }

            maxIndex = targetWaypoints.Count - 1; 
        }

        public void AddSegment(GameObject wagon){
            wagons.Add(wagon);
            wagonTargetIndexs.Add(0);
        }

        public void EnableLights(bool enableLights){
            transform.GetChild(1).gameObject.SetActive(enableLights);
        }

        float distanceSinceLastWaypoint = 0;
        void Update()
        {

            if(targetWaypoints.Count == 0) return;
            if (isWaiting) return; // Vehicle doesnt move uppon reaching a station.

            // The vehicles moves to the next point from the "MoveToTarget" list.
            transform.position = Vector3.MoveTowards(transform.position, targetWaypoints[targetIndex], Time.deltaTime * VehicleMaxSpeed);

            if (transform.position == targetWaypoints[targetIndex])
            {
                distanceSinceLastWaypoint = 0;
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

            // wagons
            for(int i=0; i<wagons.Count; i++){
                var wagonTargetIndex = wagonTargetIndexs[i];
                var wagonVelocity = 1f;
                float distance = Vector3.Distance(transform.GetChild(0).GetChild(1).position, wagons[i].transform.GetChild(0).GetChild(0).position);
                if(Vector3.Distance(transform.position, wagons[i].transform.position) < 30 * (i+1))
                {
                    wagonVelocity = .1f;
                }
                else if(distance > 20 * (i+1))
                {
                    // If the distance to the followed wagon is too big, the following wagon will increase its speed.
                    wagonVelocity = 1.2f;
                }
                else if(distance < 10 * (i+1))
                {
                    // If the distance to the followed wagon is too small, the following wagon will decrease its speed.
                    wagonVelocity = .8f;
                }


                // The wagon vehicle is being moved to the next point of the "MoveToTarget" list
                wagons[i].transform.position = Vector3.MoveTowards(wagons[i].transform.position, targetWaypoints[wagonTargetIndex], Time.deltaTime * VehicleMaxSpeed * wagonVelocity);
                if (wagons[i].transform.position == targetWaypoints[wagonTargetIndex])
                {
                    if (pathLastNode.Contains(wagons[i].transform.position))
                    {
                        // If the road/railroad consists of more than one part we have to teleport the vehicle to the other part upon reaching.
                        // the end of one part.
                        int index = targetWaypoints.IndexOf(wagons[i].transform.position);
                        wagons[i].transform.position = targetWaypoints[index + 1];
                    }
                    wagons[i].transform.LookAt(targetWaypoints[wagonTargetIndex + 1]); 

                    wagonTargetIndexs[i] += 1;
                }
            }
        }

    void OnDestroy()
    {
        for(int i=0; i<wagons.Count; i++){
            wagons[i].Destroy();
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

