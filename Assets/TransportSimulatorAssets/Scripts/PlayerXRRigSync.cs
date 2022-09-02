using UnityEngine;
using Unity.Netcode;


[RequireComponent(typeof(Animator))]
public class PlayerXRRigSync : NetworkBehaviour
{
    Animator animator;

    NetworkVariable<int> animationState = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    void Start()
    {
        animator = GetComponent<Animator>();
        animationState.OnValueChanged += OnAnimatorStateChanged;
    }

    void Update()
    {
        if(!IsOwner) return;

        var XRRig = GameObject.Find("XRRig Mouse Keyboard");

        if(!XRRig) return;

        var cameraTransform = XRRig.transform.GetChild(0).GetChild(0);

        var velocity = XRRig.transform.position - transform.position;
        transform.position += velocity;
        transform.eulerAngles = new Vector3(0, cameraTransform.eulerAngles.y, 0);

        // animation
        if(velocity.sqrMagnitude < 0.1) animationState.Value = 0;
        else animationState.Value = Vector3.Dot(velocity, cameraTransform.position) > 0 ? 1 : -1;

        // deactivate renderer on owner, to prevent seeing it on own screen
        var meshRenders = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach(var meshRenderer in meshRenders){
            meshRenderer.enabled = false;
        }
    }

    void OnAnimatorStateChanged(int previous, int current)
    {
        if(animator){
            animator.SetInteger("state", current);
        }
    }
}