using UnityEngine;

public class PlayerMovementState : MonoBehaviour
{
    public Vector3 WorldMoveDir { get; private set; }

    public void SetMoveInput(Vector2 input)
    {
        Vector3 dir = new Vector3(input.x, 0f, input.y);
        WorldMoveDir = dir.sqrMagnitude > 1e-6f ? dir.normalized : Vector3.zero;
    }
}
