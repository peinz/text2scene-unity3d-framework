using UnityEngine;
using Unity.Netcode;


[RequireComponent(typeof(Animator))]
public class PlayerXRRigSync : NetworkBehaviour
{
    Animator animator;

    NetworkVariable<int> animationState = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    NetworkVariable<Vector3> leftHandPosition = new NetworkVariable<Vector3>(new Vector3(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    NetworkVariable<Vector3> rightHandPosition = new NetworkVariable<Vector3>(new Vector3(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    void Start()
    {
        animator = GetComponent<Animator>();
        animationState.OnValueChanged += OnAnimatorStateChanged;
        leftHandPosition.OnValueChanged += OnLeftHandPositionChanged;
        rightHandPosition.OnValueChanged += OnRightHandPositionChanged;
    }

    void Update()
    {
        if(!IsOwner) return;

        var XRRig = GameObject.Find("XRRig Mouse Keyboard");

        if(!XRRig) return;

        var cameraOffsetTransform = XRRig.transform.GetChild(0);
        var cameraTransform = cameraOffsetTransform.GetChild(0);
        var leftHandTransform = cameraOffsetTransform.Find("LeftHand");
        var rightHandTransform = cameraOffsetTransform.Find("RightHand");

        // TODO: set animation rigging weights to zero in case no hands were found
        if(leftHandTransform) leftHandPosition.Value = leftHandTransform.localPosition;
        if(rightHandTransform) rightHandPosition.Value = rightHandTransform.localPosition;

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

    void OnLeftHandPositionChanged(Vector3 previous, Vector3 current)
    {
        var leftArmRig = transform.Find("LeftArmRig");
        var armRigTarget = getArmRigTarget(leftArmRig);
        armRigTarget.localPosition = current;
    }

    void OnRightHandPositionChanged(Vector3 previous, Vector3 current)
    {
        var rightArmRig = transform.Find("RightArmRig");
        var armRigTarget = getArmRigTarget(rightArmRig);
        armRigTarget.localPosition = current;
    }

    Transform getArmRigTarget(Transform armRig)
    {
        return armRig.GetChild(0).GetChild(0);
    }

}