using UnityEngine;
using System.Collections.Generic;

public class Phase3_AftercareController : MonoBehaviour
{
    [Header("Dialog System")]
    [SerializeField] private DialogSystem dialogSystem;

    [Header("Dialog Data")]
    [SerializeField] private DialogData[] dialogDataArray;

    [Header("Test/Debug")]
    [SerializeField] private bool enableLogging = true;
    [SerializeField] private bool skipAllDialogs = false;
    [SerializeField] private bool forcePhase3Complete = false;

    private TriagePatient[] allPatients;
    private Queue<TriagePatient> patientsNeedingDialog;
    private bool phase3Complete = false;
    private bool dialogWaitingForAnswer = false;

    private void Start()
    {
        allPatients = FindObjectsOfType<TriagePatient>();
        patientsNeedingDialog = new Queue<TriagePatient>();

        if (dialogSystem == null)
        {
            dialogSystem = FindObjectOfType<DialogSystem>();
        }

        Log($"[PHASE 3] Initialized with {allPatients.Length} patients");
    }

    private void Update()
    {
        if (GameplayManager.Instance == null || GameplayManager.Instance.CurrentPhase != GameplayManager.GamePhase.Phase3_Aftercare)
            return;

        if (forcePhase3Complete)
        {
            CompletePhase3();
            forcePhase3Complete = false;
            return;
        }

        if (skipAllDialogs)
        {
            SkipAllDialogs();
            skipAllDialogs = false;
            return;
        }

        // Start phase 3: populate dialog queue
        if (patientsNeedingDialog.Count == 0 && !phase3Complete)
        {
            PopulateDialogQueue();
        }

        // Process next dialog if none waiting
        if (!dialogWaitingForAnswer && patientsNeedingDialog.Count > 0)
        {
            ShowNextDialog();
        }

        // Check if all dialogs are done
        if (patientsNeedingDialog.Count == 0 && !dialogWaitingForAnswer && !phase3Complete)
        {
            CompletePhase3();
        }
    }

    private void PopulateDialogQueue()
    {
        foreach (var patient in allPatients)
        {
            // Only patients who need aftercare get dialogs
            if (patient.DialogID > 0)
            {
                patientsNeedingDialog.Enqueue(patient);
                Log($"[PHASE 3 QUEUE] Added {patient.PatientName} (DialogID: {patient.DialogID})");
            }
        }

        if (patientsNeedingDialog.Count == 0)
        {
            Log("[PHASE 3] No patients need aftercare dialogs");
            CompletePhase3();
        }
    }

    private void ShowNextDialog()
    {
        if (patientsNeedingDialog.Count == 0)
            return;

        TriagePatient nextPatient = patientsNeedingDialog.Peek(); // Don't dequeue yet
        DialogData dialogData = FindDialogData(nextPatient.DialogID);

        if (dialogData == null)
        {
            Log($"[ERROR] No dialog data found for DialogID {nextPatient.DialogID}");
            patientsNeedingDialog.Dequeue(); // Skip this patient
            return;
        }

        Log($"[PHASE 3 DIALOG] Showing dialog for {nextPatient.PatientName}");

        if (dialogSystem != null)
        {
            dialogSystem.ShowDialog(dialogData, nextPatient);
            dialogWaitingForAnswer = true;
        }
        else
        {
            Log("[ERROR] DialogSystem not assigned!");
            patientsNeedingDialog.Dequeue();
        }
    }

    public void OnDialogAnswered(bool isCorrect)
    {
        if (!dialogWaitingForAnswer || patientsNeedingDialog.Count == 0)
            return;

        dialogWaitingForAnswer = false;
        TriagePatient answeredPatient = patientsNeedingDialog.Dequeue();

        if (isCorrect)
        {
            Log($"[PHASE 3] {answeredPatient.PatientName} - Dialog answered CORRECTLY");
        }
        else
        {
            Log($"[PHASE 3] {answeredPatient.PatientName} - Dialog answered WRONG (should be handled by DialogSystem)");
        }
    }

    private void SkipAllDialogs()
    {
        Log("[DEBUG] Skipping all dialogs");

        foreach (var patient in allPatients)
        {
            patient.CompletePhase3_AftercareSuccessful();
        }

        CompletePhase3();
    }

    private void CompletePhase3()
    {
        if (phase3Complete)
            return;

        phase3Complete = true;
        dialogSystem?.HideDialog();

        Log("[PHASE 3 COMPLETE] All aftercare dialogs processed");

        // Victory!
        GameplayManager.Instance.AdvanceToPhase(GameplayManager.GamePhase.Victory);
    }

    private DialogData FindDialogData(int dialogID)
    {
        foreach (var dialog in dialogDataArray)
        {
            if (dialog.dialogID == dialogID)
                return dialog;
        }
        return null;
    }

    private void Log(string message)
    {
        if (enableLogging)
        {
            Debug.Log($"[Phase3_AftercareController] {message}", this);
        }
    }
}
