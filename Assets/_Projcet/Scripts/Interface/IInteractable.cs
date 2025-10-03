using UnityEngine;

public interface IInteractable
{
    void Interact(GameObject interactor);
    Vector3 GetPosition(); // so your spherecast system can order by distance
}
