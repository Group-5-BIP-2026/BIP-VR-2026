using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ActivateDeformation : MonoBehaviour
{
    public CPRChestRhythm rhythmScript;
    public Transform deformationTarget;

    public Collider leftHandCollider;
    public Collider rightHandCollider;

    Collider triggerCollider;
    Vector3 startPosition;
    bool leftHandInside;
    bool rightHandInside;

    void Awake()
    {
        triggerCollider = GetComponent<Collider>();

        if (deformationTarget)
        {
            startPosition = deformationTarget.position;
        }
    }

    void Start()
    {
        CacheHandCollidersFromRhythm();
    }

    void Update()
    {
        if (!deformationTarget)
        {
            return;
        }

        if (!leftHandCollider || !rightHandCollider)
        {
            CacheHandCollidersFromRhythm();
        }

        if (leftHandInside && rightHandInside && leftHandCollider && rightHandCollider)
        {
            Vector3 midpoint = (leftHandCollider.bounds.center + rightHandCollider.bounds.center) * 0.5f;

            // Keep the deformation target within this trigger's collider shape.
            if (triggerCollider)
            {
                midpoint = triggerCollider.ClosestPoint(midpoint);
            }

            deformationTarget.position = midpoint;
            return;
        }

        deformationTarget.position = startPosition;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other == leftHandCollider)
        {
            leftHandInside = true;
        }
        else if (other == rightHandCollider)
        {
            rightHandInside = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other == leftHandCollider)
        {
            leftHandInside = false;
        }
        else if (other == rightHandCollider)
        {
            rightHandInside = false;
        }
    }

    void CacheHandCollidersFromRhythm()
    {
        if (!rhythmScript)
        {
            return;
        }

        if (!leftHandCollider)
        {
            leftHandCollider = rhythmScript.leftHandCollider;
        }

        if (!rightHandCollider)
        {
            rightHandCollider = rhythmScript.rightHandCollider;
        }
    }
}
