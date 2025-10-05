using UnityEngine;
public class PlayerAimSource : MonoBehaviour
{
    [SerializeField] private Transform _aimTransform;
    public Vector3 Forward => (_aimTransform ? _aimTransform.forward : transform.forward);
    public Transform Source => _aimTransform ? _aimTransform : transform;
}

