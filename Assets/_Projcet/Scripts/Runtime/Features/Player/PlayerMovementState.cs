using UnityEngine;

public class PlayerMovementState : MonoBehaviour
{
    public Vector3 WorldMoveDir { get; private set; }
    public void SetMoveInput(Vector2 input)
    {
        Vector3 v = new Vector3(input.x, 0f, input.y);
        WorldMoveDir = v.sqrMagnitude > 1e-6f ? v.normalized : Vector3.zero;
    }
}

