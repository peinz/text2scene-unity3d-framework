using UnityEngine;

namespace Vehicle {

    public class Transporter : MonoBehaviour
    {
        public GameObject entryPoint;
        public GameObject seatLocation;
        public bool stopTransportingOnNextHalt {get; private set; } = false;

        public GameObject haltOnNextStopSign;
        public GameObject haltButtonUI;

        GameObject objectToTransport;

        Vector3 lastPosition = new Vector3(0, 0, 0);
        Vector3 lastForwardDirection = new Vector3(0, 0, 0);
        double velocity = 0;

        void Start(){
            UpdateUI();
        }

        void Update()
        {

            // FIXME: find out why this is necessary and make it not necessary anymore
            var ui = transform.Find("UI");
            ui.gameObject.SetActive(true);

            var position = transform.position;
            var newVelocity = (position - lastPosition).magnitude / Time.deltaTime;
            velocity = 0.2 * newVelocity + 0.8 * velocity;
            lastPosition = position;

            entryPoint.SetActive(velocity < 0.1); // deactivate entry while moving

            if(!objectToTransport) return;

            objectToTransport.transform.position = seatLocation.transform.position;

            var deltaRotation = Quaternion.FromToRotation(lastForwardDirection, transform.forward);
            lastForwardDirection = transform.forward;

            objectToTransport.transform.rotation *= deltaRotation;
        }

        public void StartTransporting(GameObject objectToTransport)
        {
            this.objectToTransport = objectToTransport;
            stopTransportingOnNextHalt = false;
            UpdateUI();
        }

        public void StopTransporting()
        {
            objectToTransport = null;
            UpdateUI();
        }

        public void StopTransportingOnNextHalt()
        {
            stopTransportingOnNextHalt = true;
            UpdateUI();
        }

        void UpdateUI()
        {
            var hasPassenger = objectToTransport != null;
            haltOnNextStopSign.SetActive(hasPassenger && stopTransportingOnNextHalt);
            haltButtonUI.SetActive(hasPassenger && !stopTransportingOnNextHalt);
        }

    }
} 