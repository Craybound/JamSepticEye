using UnityEngine;
using UnityEngine.UI;

public abstract class AbilitySO : ScriptableObject
{

    [Header("Meta")]
    public Image Icon;
    public string AbilityName = "New Ability";

    [TextArea]
    public string Description;


    public AudioSource audio;
    public AudioClip primarySfx;
    public AudioClip secondarySfx;

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
