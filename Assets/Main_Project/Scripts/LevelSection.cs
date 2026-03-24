using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSection : MonoBehaviour
{
    [Header("Section Anchors")]
    [SerializeField] private Transform entryAnchor;
    [SerializeField] private Transform exitAnchor;

    [Header("Gate Visual")]
    [SerializeField] private GameObject blockingWall;
    [SerializeField] private float unlockFadeDuration = 1.2f;
    [SerializeField] private AnimationCurve unlockFadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private bool disableWallObjectAfterUnlock = false;

    private readonly List<MaterialData> gateMaterials = new List<MaterialData>();
    private Collider[] gateColliders = Array.Empty<Collider>();

    private EndlessSectionManager owner;
    private Coroutine fadeCoroutine;
    private bool completed;

    private const string BaseColorProperty = "_BaseColor";
    private const string ColorProperty = "_Color";

    private struct MaterialData
    {
        public Material material;
        public string colorProperty;
        public float visibleAlpha;
    }

    public Transform EntryAnchor => entryAnchor != null ? entryAnchor : transform;
    public Transform ExitAnchor => exitAnchor != null ? exitAnchor : transform;
    public bool IsCompleted => completed;

    private void Awake()
    {
        CacheGateData();
    }

    public void Initialize(EndlessSectionManager manager)
    {
        owner = manager;
        completed = false;
        LockGateInstant();
    }

    public void CompleteSection()
    {
        if (completed)
        {
            return;
        }

        completed = true;
        owner?.HandleSectionCompleted(this);
        StartUnlockGate();
    }

    private void CacheGateData()
    {
        gateMaterials.Clear();

        if (blockingWall == null)
        {
            gateColliders = Array.Empty<Collider>();
            return;
        }

        gateColliders = blockingWall.GetComponentsInChildren<Collider>(true);
        Renderer[] renderers = blockingWall.GetComponentsInChildren<Renderer>(true);

        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] materials = renderers[i].materials;
            for (int j = 0; j < materials.Length; j++)
            {
                Material mat = materials[j];
                if (mat == null)
                {
                    continue;
                }

                string colorProperty = GetColorProperty(mat);
                if (string.IsNullOrEmpty(colorProperty))
                {
                    continue;
                }

                Color color = mat.GetColor(colorProperty);
                gateMaterials.Add(new MaterialData
                {
                    material = mat,
                    colorProperty = colorProperty,
                    visibleAlpha = color.a
                });
            }
        }
    }

    private void LockGateInstant()
    {
        if (blockingWall == null)
        {
            return;
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        blockingWall.SetActive(true);
        SetGateCollidersEnabled(true);

        for (int i = 0; i < gateMaterials.Count; i++)
        {
            MaterialData data = gateMaterials[i];
            Color color = data.material.GetColor(data.colorProperty);
            color.a = data.visibleAlpha;
            data.material.SetColor(data.colorProperty, color);
        }
    }

    private void StartUnlockGate()
    {
        if (blockingWall == null)
        {
            return;
        }

        if (gateMaterials.Count == 0)
        {
            CacheGateData();
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeGateOutRoutine());
    }

    private IEnumerator FadeGateOutRoutine()
    {
        float duration = Mathf.Max(0.01f, unlockFadeDuration);
        float elapsed = 0f;

        float[] startAlphas = new float[gateMaterials.Count];
        for (int i = 0; i < gateMaterials.Count; i++)
        {
            MaterialData data = gateMaterials[i];
            startAlphas[i] = data.material.GetColor(data.colorProperty).a;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = unlockFadeCurve.Evaluate(t);

            for (int i = 0; i < gateMaterials.Count; i++)
            {
                MaterialData data = gateMaterials[i];
                float alpha = Mathf.Lerp(startAlphas[i], 0f, eased);

                Color color = data.material.GetColor(data.colorProperty);
                color.a = alpha;
                data.material.SetColor(data.colorProperty, color);
            }

            yield return null;
        }

        SetGateCollidersEnabled(false);

        if (disableWallObjectAfterUnlock)
        {
            blockingWall.SetActive(false);
        }

        fadeCoroutine = null;
    }

    private void SetGateCollidersEnabled(bool enabled)
    {
        for (int i = 0; i < gateColliders.Length; i++)
        {
            gateColliders[i].enabled = enabled;
        }
    }

    private static string GetColorProperty(Material material)
    {
        if (material.HasProperty(BaseColorProperty))
        {
            return BaseColorProperty;
        }

        if (material.HasProperty(ColorProperty))
        {
            return ColorProperty;
        }

        return null;
    }
}
