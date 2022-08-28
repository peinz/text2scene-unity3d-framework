using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap: MonoBehaviour
{
    [SerializeField] Transform player;

    void LateUpdate(){
        // position
        var newPosition = transform.position;
        newPosition.x = player.position.x;
        newPosition.z = player.position.z;
        transform.position = newPosition;

        // rotation
        transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
    }
}
