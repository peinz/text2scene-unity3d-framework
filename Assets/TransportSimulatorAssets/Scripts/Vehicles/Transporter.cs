using UnityEngine;

namespace Vehicle {

    public class Transporter : MonoBehaviour
    {

        GameObject objectToTransport;

        void Update()
        {
            if(!objectToTransport) return;

            var objectPosition = transform.position;
            objectPosition.y = 15;
            objectToTransport.transform.position = objectPosition;
        }

        private void OnTriggerEnter(Collider other)
        {
            objectToTransport = other.gameObject;
        }
        public void StopTransporting()
        {
            objectToTransport = null;
        }

    }
} 