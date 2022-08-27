using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vehicle {

    public interface IVehicle
    {
        void SetPosition(Vector3 position);
        void EnableLights(bool enableLights);
        IEnumerator MoveTo(Vector3 targetPoint);
    }

}

