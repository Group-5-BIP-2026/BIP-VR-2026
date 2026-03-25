using UnityEngine;

public class Phase1_CheckController : MonoBehaviour
{
    [Header("Test/Debug")]
    [SerializeField] private bool enableLogging = true;
    [SerializeField] private bool forcePhase1Complete = false;

    private TriagePatient[] allPatients;
    private bool phase1Complete = false;

    private void Start()
    {
        allPatients = FindObjectsOfType<TriagePatient>();
    }

    private void Update()
    {
        if (GameplayManager.Instance == null || GameplayManager.Instance.CurrentPhase != GameplayManager.GamePhase.Phase1_Check)
            return;

        if (forcePhase1Complete)
        {
            CompletePhase1();
            forcePhase1Complete = false;
            return;
        }

        // Check: Are all patients that need to move away from fire done?
        foreach (var patient in allPatients)
        {
            if (patient.NeedsMoveFromFire)
            {
                // For now, manually check via inspector or script
                // In real game: raycast/distance check when hands move patient away from fire
            }
        }
    }

    public void CompletePhase1()
    {
        if (phase1Complete)
            return;

        phase1Complete = true;
        Log("[PHASE 1 COMPLETE] All patients checked for fire danger");

        // Move all patients that needed moving to "phase 1 complete"
        foreach (var patient in allPatients)
        {
            if (patient.NeedsMoveFromFire)
            {
                patient.CompletePhase1_MovedAwayFromFire();
            }
        }

        GameplayManager.Instance.AdvanceToPhase(GameplayManager.GamePhase.Phase2_CPRorRecovery);
    }

    private void Log(string message)
    {
        if (enableLogging)
        {
            Debug.Log($"[Phase1_CheckController] {message}", this);
        }
    }
}
