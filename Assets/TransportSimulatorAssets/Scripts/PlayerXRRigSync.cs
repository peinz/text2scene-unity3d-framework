using UnityEngine;
using Unity.Netcode;


public class PlayerXRRigSync : NetworkBehaviour
{
    void Update()
    {
        if(!IsOwner) return;

        var XRRig = GameObject.Find("XRRig Mouse Keyboard");

        if(!XRRig) return;

        var cameraTransform = XRRig.transform.GetChild(0).GetChild(0);

        transform.position = XRRig.transform.position;
        transform.rotation = cameraTransform.rotation;
    }
}