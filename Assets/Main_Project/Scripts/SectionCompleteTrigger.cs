using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SectionCompleteTrigger : MonoBehaviour
{
    [SerializeField] private LevelSection owningSection;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool triggerOnlyOnce = true;

    private bool hasTriggered;

    private void Reset()
    {
        Collider c = GetComponent<Collider>();
        c.isTrigger = true;

        if (owningSection == null)
        {
            owningSection = GetComponentInParent<LevelSection>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnlyOnce && hasTriggered)
        {
            return;
        }

        if (!IsPlayer(other))
        {
            return;
        }

        if (owningSection == null)
        {
            Debug.LogWarning("SectionCompleteTrigger: owningSection is not assigned.", this);
            return;
        }

        Debug.Log("SectionCompleteTrigger: Trigger ausgeloest von " + other.name, this);
        hasTriggered = true;
        owningSection.CompleteSection();
    }

    private bool IsPlayer(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            return true;
        }

        Transform root = other.transform.root;
        return root != null && root.CompareTag(playerTag);
    }
}
