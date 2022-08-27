using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vehicle {

    public class MultiSegmentRailVehicle : MonoBehaviour, Vehicle.IVehicle
    {
        float VehicleMaxSpeed = 100f; 
        float sectionLength = 30f;

        List<GameObject> wagons = new List<GameObject>();
        List<int> wagonTargetIndexs = new List<int>();
        List<Vector3> targetWaypoints = new List<Vector3>();
        public void SetPosition(Vector3 position)
        {       
            transform.position = position;
            for(int i=0; i<wagons.Count; i++){
                var wagonPosition = new Vector3();
                wagonPosition.z = -sectionLength * (i+1);
                wagons[i].transform.position = transform.position + wagonPosition;
            }

            targetWaypoints = new List<Vector3>();
            for(int i=0; i<wagons.Count; i++) wagonTargetIndexs[i] = 0;
        }

        public void AddSegment(GameObject wagon){
            wagons.Add(wagon);
            wagonTargetIndexs.Add(0);
        }

        public void EnableLights(bool enableLights){
            transform.GetChild(1).gameObject.SetActive(enableLights);
        }

        public IEnumerator MoveTo(Vector3 targetPoint){

            transform.LookAt(targetPoint);
            targetWaypoints.Add(targetPoint);

            while(transform.position != targetPoint){
                // vehicle
                transform.position = Vector3.MoveTowards(transform.position, targetPoint, Time.deltaTime * VehicleMaxSpeed);

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


                    wagons[i].transform.position = Vector3.MoveTowards(wagons[i].transform.position, targetWaypoints[wagonTargetIndex], Time.deltaTime * VehicleMaxSpeed * wagonVelocity);

                    if (wagons[i].transform.position == targetWaypoints[wagonTargetIndex]){
                        wagons[i].transform.LookAt(targetWaypoints[wagonTargetIndex + 1]); 
                        wagonTargetIndexs[i] += 1;
                    }
                
                }

                yield return null;
            }

        }

        void OnDestroy()
        {
            for(int i=0; i<wagons.Count; i++){
                wagons[i].Destroy();
            }
        }

    }



}

