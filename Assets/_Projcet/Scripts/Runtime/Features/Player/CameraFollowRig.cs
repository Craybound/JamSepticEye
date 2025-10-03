using UnityEngine;

public class CameraFollowRig : MonoBehaviour
{
    [SerializeField] private Transform _target;      //Player transform
    [SerializeField] private Vector3 _offset = Vector3.zero;   //Offset control
    [SerializeField] private float _followLerp = 10f;

    private Quaternion _fixedRotation;

    private void Awake()
    {
        //Keeping the fixed rotation in the editor to 67.5
        _fixedRotation = transform.rotation;
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        //Follow the player
        Vector3 desired = _target.position + _offset;
        transform.position = Vector3.Lerp(transform.position, desired, _followLerp * Time.deltaTime);

        //Locking rig rotation so that it never inherits the player's rotation
        transform.rotation = _fixedRotation;
    }

    //Function to change target at runtime
    public void SetTarget(Transform t) => _target = t;
}
