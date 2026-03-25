using UnityEngine;
using System.Collections;

public class TriagePatient : MonoBehaviour
{
    public enum PatientNeed { MoveFromFire, CPR, RecoveryPosition, Aftercare, None }
    public enum HealthStatus { Critical, Unstable, Stable, Dead }

    [Header("Patient Properties")]
    [SerializeField] private int patientID = 1;
    [SerializeField] private string patientName = "Patient";
    [SerializeField] private int priority = 1; // 1=highest

    [Header("Patient State")]
    [SerializeField] private bool needsMoveFromFire = false;
    [SerializeField] private bool requiresCPR = false;
    [SerializeField] private bool shouldBeInRecoveryPosition = false;
    [SerializeField] private int dialogID = 0;

    [Header("Hazard")]
    [SerializeField] private GameObject fireObject;
    [SerializeField] private float fireCheckRadius = 2f;

    [Header("Rewards")]
    [SerializeField] private string rewardPrefabName = "Defibrillator";
    [SerializeField] private float defiSpawnChance = 0.5f; // 50% spawn chance
    [SerializeField] private float rewardSpawnDistance = 2f;

    [Header("Test/Debug")]
    [SerializeField] private bool enableLogging = true;
    [SerializeField] private bool startAsPhase1Complete = false;
    [SerializeField] private bool startAsPhase2Complete = false;
    [SerializeField] private bool startAsPhase3Complete = false;

    private HealthStatus healthStatus = HealthStatus.Critical;
    private PatientNeed currentNeed = PatientNeed.MoveFromFire;
    private bool phase1_MovedAway = false;
    private bool phase2_CPROrRecoveryDone = false;
    private bool phase3_AftercareComplete = false;

    public bool IsComplete => phase1_MovedAway && phase2_CPROrRecoveryDone && phase3_AftercareComplete;
    public PatientNeed CurrentNeed => currentNeed;
    public HealthStatus Health => healthStatus;
    public int PatientID => patientID;
    public string PatientName => patientName;
    public bool RequiresCPR => requiresCPR;
    public bool NeedsMoveFromFire => needsMoveFromFire;
    public int DialogID => dialogID;
    public string RewardPrefabName => rewardPrefabName;
    public float DefiSpawnChance => defiSpawnChance;

    private void Start()
    {
        // Debug: Force completion flags
        if (startAsPhase1Complete)
        {
            Log("DEBUG: Starting with Phase 1 complete");
            phase1_MovedAway = true;
        }
        if (startAsPhase2Complete)
        {
            Log("DEBUG: Starting with Phase 2 complete");
            phase2_CPROrRecoveryDone = true;
        }
        if (startAsPhase3Complete)
        {
            Log("DEBUG: Starting with Phase 3 complete");
            phase3_AftercareComplete = true;
        }

        UpdateHealthStatus();
        UpdateCurrentNeed();
        Log($"[PATIENT {patientID} - {patientName}] Initialized | Priority: {priority} | Needs: {currentNeed}");
    }

    public void CompletePhase1_MovedAwayFromFire()
    {
        if (phase1_MovedAway)
            return;

        phase1_MovedAway = true;
        Log($"[PHASE 1 COMPLETE] {patientName} moved away from fire successfully");
        UpdateHealthStatus();
        UpdateCurrentNeed();
    }

    public void CompletePhase2_CPRSuccessful()
    {
        if (phase2_CPROrRecoveryDone)
            return;

        phase2_CPROrRecoveryDone = true;
        healthStatus = HealthStatus.Stable;
        Log($"[PHASE 2 COMPLETE - CPR SUCCESS] {patientName} stabilized via CPR");

        // Spawn reward (defibrillator)
        if (Random.value <= defiSpawnChance)
        {
            Log($"[REWARD] Spawning {rewardPrefabName} ({(defiSpawnChance * 100f):F0}% chance)");
            SpawnReward();
        }
        else
        {
            Log($"[REWARD] No spawn this time ({(defiSpawnChance * 100f):F0}% chance missed)");
        }

        UpdateCurrentNeed();
    }

    public void CompletePhase2_RecoveryPosition()
    {
        if (phase2_CPROrRecoveryDone)
            return;

        phase2_CPROrRecoveryDone = true;
        healthStatus = HealthStatus.Stable;
        Log($"[PHASE 2 COMPLETE - RECOVERY] {patientName} placed in recovery position");
        UpdateCurrentNeed();
    }

    public void CompletePhase3_AftercareSuccessful()
    {
        if (phase3_AftercareComplete)
            return;

        phase3_AftercareComplete = true;
        healthStatus = HealthStatus.Stable;
        Log($"[PHASE 3 COMPLETE - AFTERCARE SUCCESS] {patientName} aftercare done");
        UpdateCurrentNeed();
    }

    public void PatientDies(string reason)
    {
        healthStatus = HealthStatus.Dead;
        Log($"[PATIENT DEATH] {patientName} - Reason: {reason}");
        GameplayManager.Instance.EndGame(GameplayManager.GamePhase.GameOver, $"{patientName} died: {reason}");
    }

    private void UpdateCurrentNeed()
    {
        if (healthStatus == HealthStatus.Dead)
        {
            currentNeed = PatientNeed.None;
            return;
        }

        if (!phase1_MovedAway && needsMoveFromFire)
        {
            currentNeed = PatientNeed.MoveFromFire;
        }
        else if (!phase2_CPROrRecoveryDone)
        {
            currentNeed = requiresCPR ? PatientNeed.CPR : PatientNeed.RecoveryPosition;
        }
        else if (!phase3_AftercareComplete)
        {
            currentNeed = PatientNeed.Aftercare;
        }
        else
        {
            currentNeed = PatientNeed.None;
        }
    }

    private void UpdateHealthStatus()
    {
        if (healthStatus == HealthStatus.Dead)
            return;

        if (!phase1_MovedAway && needsMoveFromFire)
            healthStatus = HealthStatus.Critical;
        else if (!phase2_CPROrRecoveryDone && requiresCPR)
            healthStatus = HealthStatus.Critical;
        else if (!phase3_AftercareComplete)
            healthStatus = HealthStatus.Unstable;
        else
            healthStatus = HealthStatus.Stable;
    }

    private void SpawnReward()
    {
        // Reward spawns 2m in front of patient
        Vector3 spawnPos = transform.position + transform.forward * rewardSpawnDistance;
        spawnPos.y = transform.position.y; // Keep on ground

        // Load reward prefab
        GameObject rewardPrefab = Resources.Load<GameObject>($"Prefabs/{rewardPrefabName}");
        if (rewardPrefab == null)
        {
            // Fallback: spawn a simple cube for testing
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = spawnPos;
            cube.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            cube.name = $"{rewardPrefabName}_TestCube";
            Log($"[REWARD FALLBACK] Created test cube for {rewardPrefabName}");
            return;
        }

        Instantiate(rewardPrefab, spawnPos, Quaternion.identity);
    }

    private void Log(string message)
    {
        if (enableLogging)
        {
            Debug.Log($"[TriagePatient] {message}", this);
        }
    }
}
