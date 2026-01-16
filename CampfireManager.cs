using UnityEngine;

public class CampfireManager : MonoBehaviour
{
    [Header("Progression Settings")]
    public int totalDefenses = 4;
    public GameObject safeZoneParticles;

    public static event System.Action OnFortressSecureEvent;

    private int _currentBuilt = 0;

    public void RegisterDefenseBuilt()
    {
        _currentBuilt++;
        Debug.Log($"Fortress Progress: {_currentBuilt}/{totalDefenses}");

        if (_currentBuilt >= totalDefenses)
        {
            FortressSecure();
        }
    }

    private void FortressSecure()
    {
        Debug.Log("FORTRESS SECURE! PHASE 2 STARTED!");

        OnFortressSecureEvent?.Invoke();

        if (safeZoneParticles != null)
        {
            safeZoneParticles.SetActive(true);
        }
    }
}
