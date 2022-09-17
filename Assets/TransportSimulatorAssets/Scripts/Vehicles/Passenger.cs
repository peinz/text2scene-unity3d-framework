using UnityEngine;

namespace Vehicle {

    public class Passenger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            var transporter = other.gameObject.GetComponentInParent<Transporter>();
            if(transporter && GameObject.ReferenceEquals(other.gameObject, transporter.entryPoint)){
                transporter.StartTransporting(this.gameObject);
            }
        }

    }
} 