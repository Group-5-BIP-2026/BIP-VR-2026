using UnityEngine;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

[RequireComponent(typeof(Collider))]
public class CPRChestRhythm : MonoBehaviour
{
    [Header("Hand Colliders")]
    [SerializeField] private Collider leftHandCollider;
    [SerializeField] private Collider rightHandCollider;
    [SerializeField] private bool requireBothHands = true;

    [Header("Rhythm")]
    [SerializeField, Min(30f)] private float targetBpm = 110f;
    [SerializeField, Min(1f)] private float bpmTolerance = 12f;
    [SerializeField, Min(0.05f)] private float minPressInterval = 0.2f;
    [SerializeField, Min(1)] private int requiredValidCompressions = 30;
    [SerializeField] private bool resetStreakOnInvalidCompression = true;

    [Header("Keyboard Test")]
    [SerializeField] private bool enableKeyboardTest = true;
    [SerializeField] private KeyCode testCompressionKey = KeyCode.Space;

    [Header("Debug")]
    [SerializeField] private bool logDebugMessages = false;

    [Header("Events")]
    [SerializeField] private UnityEvent onValidCompression;
    [SerializeField] private UnityEvent onInvalidCompression;
    [SerializeField] private UnityEvent onTargetReached;

    private bool leftHandInside;
    private bool rightHandInside;
    private bool handsReleasedSinceLastCompression = true;

    private float lastCompressionTime = -999f;
    private int validCompressionStreak;
    private int totalCompressions;
    private bool warnedUnsupportedInputSystemKey;

    public int ValidCompressionStreak => validCompressionStreak;
    public int TotalCompressions => totalCompressions;

    private void Reset()
    {
        Collider c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    private void Update()
    {
        if (enableKeyboardTest && IsKeyboardCompressionPressed())
        {
            TryRegisterCompression(ignoreHandRequirement: true, source: "Keyboard");
        }
    }

    private bool IsKeyboardCompressionPressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return false;
        }

        KeyControl keyControl = GetInputSystemKeyControl(keyboard, testCompressionKey);
        if (keyControl == null)
        {
            if (!warnedUnsupportedInputSystemKey)
            {
                Debug.LogWarning($"CPRChestRhythm: Key '{testCompressionKey}' is not mapped for Input System keyboard test. Using any key as fallback.", this);
                warnedUnsupportedInputSystemKey = true;
            }

            return keyboard.anyKey.wasPressedThisFrame;
        }

        warnedUnsupportedInputSystemKey = false;
        return keyControl.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(testCompressionKey);
#else
        return false;
#endif
    }

#if ENABLE_INPUT_SYSTEM
    private static KeyControl GetInputSystemKeyControl(Keyboard keyboard, KeyCode keyCode)
    {
        switch (keyCode)
        {
            case KeyCode.Space: return keyboard.spaceKey;
            case KeyCode.Return: return keyboard.enterKey;
            case KeyCode.KeypadEnter: return keyboard.numpadEnterKey;
            case KeyCode.T: return keyboard.tKey;
            case KeyCode.E: return keyboard.eKey;
            case KeyCode.F: return keyboard.fKey;
            case KeyCode.C: return keyboard.cKey;
            case KeyCode.R: return keyboard.rKey;
            case KeyCode.Alpha1: return keyboard.digit1Key;
            case KeyCode.Alpha2: return keyboard.digit2Key;
            case KeyCode.Alpha3: return keyboard.digit3Key;
            default: return null;
        }
    }
#endif

    private void OnTriggerEnter(Collider other)
    {
        if (IsLeftHand(other))
        {
            leftHandInside = true;
        }
        else if (IsRightHand(other))
        {
            rightHandInside = true;
        }
        else
        {
            return;
        }

        if (CanAttemptCompressionFromHands())
        {
            TryRegisterCompression(ignoreHandRequirement: false, source: "Hands");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsLeftHand(other))
        {
            leftHandInside = false;
        }
        else if (IsRightHand(other))
        {
            rightHandInside = false;
        }
        else
        {
            return;
        }

        // Require a release between hand-based compressions.
        if (!AreRequiredHandsInside())
        {
            handsReleasedSinceLastCompression = true;
        }
    }

    private void TryRegisterCompression(bool ignoreHandRequirement, string source)
    {
        if (!ignoreHandRequirement && !CanAttemptCompressionFromHands())
        {
            return;
        }

        float now = Time.time;
        float elapsed = now - lastCompressionTime;

        // Debounce to avoid multiple counts on one physical push.
        if (elapsed < minPressInterval)
        {
            return;
        }

        totalCompressions++;

        bool isValid = true;
        if (lastCompressionTime > 0f)
        {
            float measuredBpm = 60f / elapsed;
            float minBpm = targetBpm - bpmTolerance;
            float maxBpm = targetBpm + bpmTolerance;
            isValid = measuredBpm >= minBpm && measuredBpm <= maxBpm;

            if (logDebugMessages)
            {
                Debug.Log($"CPR Compression ({source}): {measuredBpm:F1} BPM | target {targetBpm:F1} +/- {bpmTolerance:F1}", this);
            }
        }
        else if (logDebugMessages)
        {
            Debug.Log($"CPR Compression ({source}): first compression registered.", this);
        }

        if (isValid)
        {
            validCompressionStreak++;
            onValidCompression?.Invoke();

            if (validCompressionStreak >= requiredValidCompressions)
            {
                onTargetReached?.Invoke();
            }
        }
        else
        {
            if (resetStreakOnInvalidCompression)
            {
                validCompressionStreak = 0;
            }

            onInvalidCompression?.Invoke();
        }

        lastCompressionTime = now;

        if (!ignoreHandRequirement)
        {
            handsReleasedSinceLastCompression = false;
        }
    }

    private bool CanAttemptCompressionFromHands()
    {
        if (!AreRequiredHandsInside())
        {
            return false;
        }

        return handsReleasedSinceLastCompression;
    }

    private bool AreRequiredHandsInside()
    {
        if (requireBothHands)
        {
            return leftHandInside && rightHandInside;
        }

        return leftHandInside || rightHandInside;
    }

    private bool IsLeftHand(Collider other)
    {
        return leftHandCollider != null && other == leftHandCollider;
    }

    private bool IsRightHand(Collider other)
    {
        return rightHandCollider != null && other == rightHandCollider;
    }

    public void ResetCPRState()
    {
        validCompressionStreak = 0;
        totalCompressions = 0;
        lastCompressionTime = -999f;
        handsReleasedSinceLastCompression = true;
    }
}
