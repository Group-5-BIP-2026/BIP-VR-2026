using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ActivateDeformation : MonoBehaviour
{
    public CPRChestRhythm rhythmScript;
    public Transform deformationTarget;
    public Transform leftHandTransform;
    public Transform rightHandTransform;

    public Collider leftHandCollider;
    public Collider rightHandCollider;

    Collider triggerCollider;
    Vector3 startLocalPosition;
    bool leftHandInside;
    bool rightHandInside;

    void Awake()
    {
        triggerCollider = GetComponent<Collider>();

        if (deformationTarget)
        {
            startLocalPosition = deformationTarget.localPosition;
        }
    }

    void Start()
    {
        CacheHandCollidersFromRhythm();
        CacheHandTransformsFromColliders();
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
            CacheHandTransformsFromColliders();
        }

        if ((!leftHandTransform || !rightHandTransform) && (leftHandCollider || rightHandCollider))
        {
            CacheHandTransformsFromColliders();
        }

        if (leftHandInside && rightHandInside && leftHandTransform && rightHandTransform)
        {
            Vector3 midpoint = (leftHandTransform.position + rightHandTransform.position) * 0.5f;

            // Keep the deformation target within this trigger's collider shape.
            if (triggerCollider)
            {
                midpoint = triggerCollider.ClosestPoint(midpoint);
            }

            deformationTarget.position = midpoint;
            return;
        }

        deformationTarget.localPosition = startLocalPosition;
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsFromSameHand(other, leftHandCollider))
        {
            leftHandInside = true;
        }
        else if (IsFromSameHand(other, rightHandCollider))
        {
            rightHandInside = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (IsFromSameHand(other, leftHandCollider))
        {
            leftHandInside = false;
        }
        else if (IsFromSameHand(other, rightHandCollider))
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

    void CacheHandTransformsFromColliders()
    {
        if (!leftHandTransform && leftHandCollider)
        {
            leftHandTransform = leftHandCollider.transform;
        }

        if (!rightHandTransform && rightHandCollider)
        {
            rightHandTransform = rightHandCollider.transform;
        }
    }

    bool IsFromSameHand(Collider other, Collider handCollider)
    {
        if (!other || !handCollider)
        {
            return false;
        }

        Rigidbody otherBody = other.attachedRigidbody;
        Rigidbody handBody = handCollider.attachedRigidbody;

        if (otherBody && handBody)
        {
            return otherBody == handBody;
        }

        return other.transform.root == handCollider.transform.root;
    }
}
