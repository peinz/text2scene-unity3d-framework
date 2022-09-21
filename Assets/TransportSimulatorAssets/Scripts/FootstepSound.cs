using UnityEngine;

[RequireComponent(typeof(AudioSource))]
class FootstepSound : MonoBehaviour {

    AudioSource audioSource;
    
    Vector3 lastPosition = new Vector3();
    float velocity = 0;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        var newPosition = transform.position;
        velocity = 0.8f*velocity + (0.2f*(newPosition-lastPosition).magnitude / Time.deltaTime);
        lastPosition = newPosition;

        if(velocity > 0.1){
            if(!audioSource.isPlaying) audioSource.Play();
        } else {
            audioSource.Stop();
        }
    }

}