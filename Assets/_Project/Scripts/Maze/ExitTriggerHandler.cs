using UnityEngine;

public class ExitTriggerHandler : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EndScreen endScreen = FindFirstObjectByType<EndScreen>(FindObjectsInactive.Include);

            if (endScreen != null)
            {
                endScreen.ShowWin();
            }
        }
    }
}