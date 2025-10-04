using UnityEngine;

public abstract class AbilitySO : ScriptableObject
{
    [Header("Meta")]
    public string AbilityName = "New Ability";

    /// <summary>
    /// Called when player presses Primary input.
    /// </summary>
    public abstract void OnPrimary(GameObject owner);

    /// <summary>
    /// Called when player presses Secondary input.
    /// </summary>
    public abstract void OnSecondary(GameObject owner);

    /// <summary>
    /// Optional: called each frame to update cooldowns/timers.
    /// </summary>
    public virtual void Tick(GameObject owner, float deltaTime) { }
}
