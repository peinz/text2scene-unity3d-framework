using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vehicle {

    public abstract class Vehicle : MonoBehaviour
    {
        public abstract void SetPosition(Vector3 position);
        public abstract Vector3 GetPosition();
        public abstract void EnableLights(bool enableLights);
        public abstract IEnumerator MoveTo(Vector3 targetPoint);
    }

}

