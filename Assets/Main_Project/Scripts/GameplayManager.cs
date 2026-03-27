using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class GameplayManager : MonoBehaviour
{
    public enum GamePhase { Phase1_Check, Phase2_CPRorRecovery, Phase3_Aftercare, Victory, GameOver }

    [Header("Game Settings")]
    [SerializeField] private float phase1Duration = 30f;
    [SerializeField] private float phase2Duration = 45f;
    [SerializeField] private float phase3Duration = 60f;

    [Header("Test/Debug")]
    [SerializeField] private bool enableLogging = true;
    [SerializeField] private bool skipToPhase2 = false;
    [SerializeField] private bool skipToPhase3 = false;
    [SerializeField] private bool forceGameOver = false;

    private static GameplayManager instance;
    public static GameplayManager Instance => instance;

    private GamePhase currentPhase = GamePhase.Phase1_Check;
    private float phaseTimer = 0f;
    private float currentPhaseDuration = 0f;
    private bool isGameRunning = false;
    private string gameOverReason = "";

    private List<TriagePatient> allPatients = new List<TriagePatient>();

    public GamePhase CurrentPhase => currentPhase;
    public float TimeRemaining => Mathf.Max(0f, currentPhaseDuration - phaseTimer);
    public bool IsGameRunning => isGameRunning;
    public string GameOverReason => gameOverReason;

    public UnityEvent onPhaseChanged;
    public UnityEvent onGameEnded;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        allPatients.AddRange(FindObjectsOfType<TriagePatient>());
        Log($"GameplayManager initialized with {allPatients.Count} patients.");

        // Debug: Skips
        if (skipToPhase2)
        {
            Log("DEBUG: Skipping to Phase 2");
            AdvanceToPhase(GamePhase.Phase2_CPRorRecovery);
            return;
        }
        if (skipToPhase3)
        {
            Log("DEBUG: Skipping to Phase 3");
            AdvanceToPhase(GamePhase.Phase3_Aftercare);
            return;
        }

        StartGame();
    }

    private void Update()
    {
        if (!isGameRunning)
            return;

        if (forceGameOver)
        {
            EndGame(GamePhase.GameOver, "DEBUG: Force GameOver");
            forceGameOver = false;
            return;
        }

        phaseTimer += Time.deltaTime;

        // Phase timer expired
        if (phaseTimer >= currentPhaseDuration)
        {
            Log($"[PHASE {currentPhase}] Timer expired!");
            EndGame(GamePhase.GameOver, $"Time expired in {currentPhase}");
            return;
        }

        // Check win conditions
        if (currentPhase == GamePhase.Phase3_Aftercare && CheckAllPatientsComplete())
        {
            Log("[VICTORY] All patients treated successfully!");
            EndGame(GamePhase.Victory, "All patients saved");
        }
    }

    public void StartGame()
    {
        isGameRunning = true;
        currentPhase = GamePhase.Phase1_Check;
        phaseTimer = 0f;
        currentPhaseDuration = phase1Duration;

        Log("[GAME START] Phase 1: Check - Are patients in danger (Fire)?");
        onPhaseChanged?.Invoke();
    }

    public void AdvanceToPhase(GamePhase nextPhase)
    {
        if (!isGameRunning)
            return;

        phaseTimer = 0f;
        currentPhase = nextPhase;

        switch (nextPhase)
        {
            case GamePhase.Phase1_Check:
                currentPhaseDuration = phase1Duration;
                Log("[PHASE 1] Check - Move patients away from fire");
                break;

            case GamePhase.Phase2_CPRorRecovery:
                currentPhaseDuration = phase2Duration;
                Log("[PHASE 2] Check - Does patient need CPR or Recovery Position?");
                break;

            case GamePhase.Phase3_Aftercare:
                currentPhaseDuration = phase3Duration;
                Log("[PHASE 3] Aftercare - Dialog decisions for each patient");
                break;

            case GamePhase.Victory:
            case GamePhase.GameOver:
                isGameRunning = false;
                break;
        }

        onPhaseChanged?.Invoke();
    }

    public void EndGame(GamePhase endPhase, string reason)
    {
        if (!isGameRunning)
            return;

        isGameRunning = false;
        currentPhase = endPhase;
        gameOverReason = reason;

        Log($"[GAME END] {endPhase}: {reason}");
        onGameEnded?.Invoke();
    }

    private bool CheckAllPatientsComplete()
    {
        foreach (var patient in allPatients)
        {
            if (!patient.IsComplete)
                return false;
        }
        return true;
    }

    private void Log(string message)
    {
        if (enableLogging)
        {
            Debug.Log($"[GameplayManager] {message}", this);
        }
    }
}
