using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogSystem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas dialogCanvas;
    [SerializeField] private TextMeshProUGUI situationText;
    [SerializeField] private Button optionAButton;
    [SerializeField] private Button optionBButton;
    [SerializeField] private Button optionCButton;
    [SerializeField] private TextMeshProUGUI optionAText;
    [SerializeField] private TextMeshProUGUI optionBText;
    [SerializeField] private TextMeshProUGUI optionCText;

    [Header("Test/Debug")]
    [SerializeField] private bool enableLogging = true;

    private DialogData currentDialog;
    private TriagePatient currentPatient;
    private bool waitingForAnswer = false;

    private void Start()
    {
        if (dialogCanvas != null)
            dialogCanvas.gameObject.SetActive(false);

        optionAButton?.onClick.AddListener(() => OnOptionSelected(0));
        optionBButton?.onClick.AddListener(() => OnOptionSelected(1));
        optionCButton?.onClick.AddListener(() => OnOptionSelected(2));
    }

    public void ShowDialog(DialogData dialog, TriagePatient patient)
    {
        if (dialog == null || patient == null)
        {
            Log("ERROR: Dialog or Patient is null");
            return;
        }

        currentDialog = dialog;
        currentPatient = patient;
        waitingForAnswer = false;

        Log($"[DIALOG SHOW] Patient: {patient.PatientName} | DialogID: {dialog.dialogID}");

        if (dialogCanvas != null)
        {
            dialogCanvas.gameObject.SetActive(true);
        }

        if (situationText != null)
        {
            situationText.text = dialog.situationText;
        }

        if (optionAText != null) optionAText.text = dialog.optionAText;
        if (optionBText != null) optionBText.text = dialog.optionBText;
        if (optionCText != null) optionCText.text = dialog.optionCText;

        waitingForAnswer = true;
    }

    private void OnOptionSelected(int selectedIndex)
    {
        if (!waitingForAnswer || currentDialog == null || currentPatient == null)
            return;

        waitingForAnswer = false;

        bool isCorrect = (selectedIndex == currentDialog.correctAnswerIndex);

        Log($"[DIALOG ANSWER] Selected: {SelectionIndexToLetter(selectedIndex)} | Correct: {isCorrect}");

        HideDialog();

        if (isCorrect)
        {
            Log($"[DIALOG CORRECT] {currentPatient.PatientName} - Correct decision!");
            currentPatient.CompletePhase3_AftercareSuccessful();
        }
        else
        {
            Log($"[DIALOG WRONG] {currentPatient.PatientName} - Wrong decision! Patient dies!");
            currentPatient.PatientDies("Wrong aftercare decision");
        }
    }

    public void HideDialog()
    {
        waitingForAnswer = false;
        if (dialogCanvas != null)
        {
            dialogCanvas.gameObject.SetActive(false);
        }
        Log("[DIALOG HIDE]");
    }

    private string SelectionIndexToLetter(int index)
    {
        return index switch
        {
            0 => "A",
            1 => "B",
            2 => "C",
            _ => "?"
        };
    }

    private void Log(string message)
    {
        if (enableLogging)
        {
            Debug.Log($"[DialogSystem] {message}", this);
        }
    }
}

[System.Serializable]
public class DialogData
{
    public int dialogID;
    public string patientName;
    public string situationText;
    public string optionAText;
    public string optionBText;
    public string optionCText;
    public int correctAnswerIndex; // 0=A, 1=B, 2=C
}
