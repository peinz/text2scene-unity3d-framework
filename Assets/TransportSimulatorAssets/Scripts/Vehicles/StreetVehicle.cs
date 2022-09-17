using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vehicle {

    public class StreetVehicle : Vehicle
    {
        float VehicleMaxSpeed = 100f; 

        public override void SetPosition(Vector3 position)
        {       
            transform.position = position;
        }

        public override Vector3 GetPosition()
        {       
            return transform.position;
        }

        public override void EnableLights(bool enableLights){
            transform.GetChild(1).gameObject.SetActive(enableLights);
        }

        public override IEnumerator MoveTo(Vector3 targetPoint){
            transform.LookAt(targetPoint);
            while(transform.position != targetPoint){
                transform.position = Vector3.MoveTowards(transform.position, targetPoint, Time.deltaTime * VehicleMaxSpeed);
                yield return null;
            }
        }

    }



}

