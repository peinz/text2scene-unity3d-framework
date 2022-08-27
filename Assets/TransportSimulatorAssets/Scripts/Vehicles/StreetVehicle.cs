using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vehicle {

    public class StreetVehicle : MonoBehaviour, Vehicle.IVehicle
    {
        float VehicleMaxSpeed = 100f; 

        public void SetPosition(Vector3 position)
        {       
            transform.position = position;
        }

        public void EnableLights(bool enableLights){
            transform.GetChild(1).gameObject.SetActive(enableLights);
        }

        public IEnumerator MoveTo(Vector3 targetPoint){
            transform.LookAt(targetPoint);
            while(transform.position != targetPoint){
                transform.position = Vector3.MoveTowards(transform.position, targetPoint, Time.deltaTime * VehicleMaxSpeed);
                yield return null;
            }
        }

    }



}

