using UnityEngine;

public class ExitMarker : MonoBehaviour
{
    public static event System.Action OnExitReached;

    private bool triggered;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (other.GetComponentInParent<CharacterController>() == null) return;

        triggered = true;
        OnExitReached?.Invoke();
    }
}
