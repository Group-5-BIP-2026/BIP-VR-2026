using System.Collections.Generic;
using UnityEngine;

public class EndlessSectionManager : MonoBehaviour
{
    [Header("Section Library")]
    [SerializeField] private List<LevelSection> sectionPrefabs = new List<LevelSection>();
    [SerializeField] private LevelSection firstSectionPrefab;
    [SerializeField] private bool avoidImmediateRepeats = true;

    [Header("Runtime")]
    [SerializeField] private int sectionsAheadAtStart = 1;
    [SerializeField] private int maxActiveSections = 4;
    [SerializeField] private bool usePooling = true;
    [SerializeField] private Transform sectionRoot;

    private readonly List<LevelSection> activeSections = new List<LevelSection>();
    private readonly Dictionary<LevelSection, int> instancePrefabIndex = new Dictionary<LevelSection, int>();
    private readonly Dictionary<int, Queue<LevelSection>> poolByPrefabIndex = new Dictionary<int, Queue<LevelSection>>();

    private int lastPickedPrefabIndex = -1;

    private void Start()
    {
        if (sectionPrefabs.Count == 0)
        {
            Debug.LogError("EndlessSectionManager: sectionPrefabs is empty.", this);
            return;
        }

        SpawnInitialSections();
    }

    public void HandleSectionCompleted(LevelSection completedSection)
    {
        if (!activeSections.Contains(completedSection))
        {
            return;
        }

        LevelSection currentTail = activeSections[activeSections.Count - 1];
        SpawnAfter(currentTail);
        TrimOldSections();
    }

    private void SpawnInitialSections()
    {
        LevelSection first = firstSectionPrefab != null
            ? SpawnSpecific(firstSectionPrefab, transform)
            : SpawnRandom(transform);

        if (first == null)
        {
            return;
        }

        activeSections.Add(first);

        int aheadCount = Mathf.Max(0, sectionsAheadAtStart);
        for (int i = 0; i < aheadCount; i++)
        {
            LevelSection tail = activeSections[activeSections.Count - 1];
            SpawnAfter(tail);
        }

        TrimOldSections();
    }

    private void SpawnAfter(LevelSection previousSection)
    {
        LevelSection spawned = SpawnRandom(previousSection.ExitAnchor);
        if (spawned == null)
        {
            return;
        }

        activeSections.Add(spawned);
    }

    private LevelSection SpawnSpecific(LevelSection prefab, Transform attachTo)
    {
        int prefabIndex = sectionPrefabs.IndexOf(prefab);
        LevelSection instance = GetOrCreateInstance(prefab, prefabIndex);
        if (instance == null)
        {
            return null;
        }

        AlignSection(instance, attachTo);
        instance.Initialize(this);
        return instance;
    }

    private LevelSection SpawnRandom(Transform attachTo)
    {
        int index = PickNextPrefabIndex();
        if (index < 0)
        {
            return null;
        }

        LevelSection prefab = sectionPrefabs[index];
        LevelSection instance = GetOrCreateInstance(prefab, index);
        if (instance == null)
        {
            return null;
        }

        AlignSection(instance, attachTo);
        instance.Initialize(this);
        return instance;
    }

    private LevelSection GetOrCreateInstance(LevelSection prefab, int prefabIndex)
    {
        LevelSection instance = null;

        if (usePooling && prefabIndex >= 0)
        {
            Queue<LevelSection> pool = GetPool(prefabIndex);
            while (pool.Count > 0 && instance == null)
            {
                instance = pool.Dequeue();
            }
        }

        if (instance == null)
        {
            Transform parent = sectionRoot != null ? sectionRoot : transform;
            instance = Instantiate(prefab, parent);
        }
        else
        {
            instance.gameObject.SetActive(true);
        }

        instancePrefabIndex[instance] = prefabIndex;
        return instance;
    }

    private void AlignSection(LevelSection section, Transform attachTo)
    {
        Transform entry = section.EntryAnchor;

        Quaternion targetRotation = attachTo.rotation * Quaternion.Inverse(entry.localRotation);
        Vector3 targetPosition = attachTo.position - (targetRotation * entry.localPosition);

        section.transform.SetPositionAndRotation(targetPosition, targetRotation);
    }

    private int PickNextPrefabIndex()
    {
        if (sectionPrefabs.Count == 0)
        {
            return -1;
        }

        if (sectionPrefabs.Count == 1)
        {
            lastPickedPrefabIndex = 0;
            return 0;
        }

        int picked = Random.Range(0, sectionPrefabs.Count);
        if (avoidImmediateRepeats)
        {
            int guard = 0;
            while (picked == lastPickedPrefabIndex && guard < 8)
            {
                picked = Random.Range(0, sectionPrefabs.Count);
                guard++;
            }
        }

        lastPickedPrefabIndex = picked;
        return picked;
    }

    private void TrimOldSections()
    {
        int maxSections = Mathf.Max(2, maxActiveSections);
        while (activeSections.Count > maxSections)
        {
            LevelSection oldest = activeSections[0];
            activeSections.RemoveAt(0);
            RecycleOrDestroy(oldest);
        }
    }

    private void RecycleOrDestroy(LevelSection section)
    {
        if (section == null)
        {
            return;
        }

        int prefabIndex = -1;
        instancePrefabIndex.TryGetValue(section, out prefabIndex);
        instancePrefabIndex.Remove(section);

        if (usePooling && prefabIndex >= 0)
        {
            section.gameObject.SetActive(false);
            GetPool(prefabIndex).Enqueue(section);
            return;
        }

        Destroy(section.gameObject);
    }

    private Queue<LevelSection> GetPool(int prefabIndex)
    {
        Queue<LevelSection> pool;
        if (!poolByPrefabIndex.TryGetValue(prefabIndex, out pool))
        {
            pool = new Queue<LevelSection>();
            poolByPrefabIndex[prefabIndex] = pool;
        }

        return pool;
    }
}
