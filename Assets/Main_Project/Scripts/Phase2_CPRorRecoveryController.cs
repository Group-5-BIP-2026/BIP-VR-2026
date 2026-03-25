using UnityEngine;

public class Phase2_CPRorRecoveryController : MonoBehaviour
{
    [Header("CPR Settings")]
    [SerializeField] private CPRChestRhythm cprScript;
    [SerializeField] private int requiredValidCompressions = 10;

    [Header("Test/Debug")]
    [SerializeField] private bool enableLogging = true;
    [SerializeField] private bool forcePhase2Complete = false;
    [SerializeField] private bool skipCPR_UseRecoveryPosition = false;

    private TriagePatient[] allPatients;
    private TriagePatient cprPatient;
    private bool phase2Complete = false;

    private void Start()
    {
        allPatients = FindObjectsOfType<TriagePatient>();

        // Find patient that requires CPR
        foreach (var patient in allPatients)
        {
            if (patient.RequiresCPR)
            {
                cprPatient = patient;
                Log($"[PHASE 2] CPR Patient assigned: {patient.PatientName}");
                break;
            }
        }

        if (cprPatient == null)
        {
            Log("[PHASE 2] No CPR patient found, will use recovery position for all");
        }
    }

    private void Update()
    {
        if (GameplayManager.Instance == null || GameplayManager.Instance.CurrentPhase != GameplayManager.GamePhase.Phase2_CPRorRecovery)
            return;

        if (forcePhase2Complete)
        {
            CompletePhase2();
            forcePhase2Complete = false;
            return;
        }

        if (skipCPR_UseRecoveryPosition)
        {
            Log("[DEBUG] Skipping CPR, using recovery position");
            CompletePhase2_RecoveryPosition();
            skipCPR_UseRecoveryPosition = false;
            return;
        }

        // Check if CPR is successful
        if (cprPatient != null && cprScript != null && cprScript.ValidCompressionStreak >= requiredValidCompressions)
        {
            Log($"[PHASE 2 SUCCESS - CPR] {requiredValidCompressions} compressions reached!");
            CompletePhase2_CPRSuccess();
        }
    }

    private void CompletePhase2_CPRSuccess()
    {
        if (phase2Complete)
            return;

        phase2Complete = true;

        if (cprPatient != null)
        {
            cprPatient.CompletePhase2_CPRSuccessful();
            Log($"[PHASE 2 COMPLETE - CPR SUCCESS] {cprPatient.PatientName} revived");
        }

        // Complete other patients with recovery position
        foreach (var patient in allPatients)
        {
            if (patient != cprPatient && !patient.RequiresCPR)
            {
                patient.CompletePhase2_RecoveryPosition();
            }
        }

        GameplayManager.Instance.AdvanceToPhase(GameplayManager.GamePhase.Phase3_Aftercare);
    }

    private void CompletePhase2_RecoveryPosition()
    {
        if (phase2Complete)
            return;

        phase2Complete = true;

        // All patients get recovery position
        foreach (var patient in allPatients)
        {
            if (!patient.RequiresCPR)
            {
                patient.CompletePhase2_RecoveryPosition();
                Log($"[PHASE 2 COMPLETE - RECOVERY] {patient.PatientName} in recovery position");
            }
        }

        GameplayManager.Instance.AdvanceToPhase(GameplayManager.GamePhase.Phase3_Aftercare);
    }

    private void CompletePhase2()
    {
        if (phase2Complete)
            return;

        phase2Complete = true;
        Log("[PHASE 2 FORCE COMPLETE]");

        foreach (var patient in allPatients)
        {
            if (patient.RequiresCPR)
                patient.CompletePhase2_CPRSuccessful();
            else
                patient.CompletePhase2_RecoveryPosition();
        }

        GameplayManager.Instance.AdvanceToPhase(GameplayManager.GamePhase.Phase3_Aftercare);
    }

    private void Log(string message)
    {
        if (enableLogging)
        {
            Debug.Log($"[Phase2_CPRorRecoveryController] {message}", this);
        }
    }
}
