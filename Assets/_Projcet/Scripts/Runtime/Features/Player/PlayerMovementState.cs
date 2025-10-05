// Assets/_Project/Scripts/Gameplay/Player/PlayerMovementState.cs
using UnityEngine;

public class PlayerMovementState : MonoBehaviour
{
    // Latest normalized world-space move direction (XZ only). Zero when idle.
    public Vector3 WorldMoveDir { get; private set; }

    // Call this from your mover's OnMove handler each frame input updates
    public void SetMoveInput(Vector2 input)
    {
        Vector3 dir = new Vector3(input.x, 0f, input.y);
        WorldMoveDir = dir.sqrMagnitude > 1e-6f ? dir.normalized : Vector3.zero;
    }
}
