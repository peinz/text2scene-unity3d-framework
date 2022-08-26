using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vehicle {

    public interface IVehicle
    {
        void Initialize(Vector3 position, List<Vector3> _targetWaypoints, List<Vector3> _stationCoordinates, List<Vector3> _pathLastNode);
        void EnableLights(bool enableLights);
    }

}

