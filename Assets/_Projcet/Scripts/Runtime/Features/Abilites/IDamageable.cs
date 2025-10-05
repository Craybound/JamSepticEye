public interface IDamageable
{
    bool IsAlive { get; }
    void ApplyDamage(int amount, UnityEngine.Vector3 hitPoint, UnityEngine.Vector3 hitNormal);
}

