using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
public class HangingDecorationVariant
{
    public GameObject prefab;
    [Min(0.01f)] public float scaleMultiplier = 1f;
    public Vector3 positionOffset;
    public Vector3 rotationOffset;
    public bool applyColor = true;
}

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider))]
public class CheckPointDecoration : MonoBehaviour
{
    private const string GeneratedRootName = "_GeneratedDecorations";
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private System.Random objectRandom;
    private System.Random colorRandom;
    private int decorationSequence;
    private readonly List<LineRenderer> generatedRopes = new List<LineRenderer>();
    private bool hasRopeColorOverride;
    private Color ropeColorOverride;

    [Header("References")]
    public BoxCollider checkpointCollider;
    [Tooltip("Optional hierarchy parent. Its transform does not affect placement.")]
    public Transform DecoParent;
    public Material poleMaterial;
    [Tooltip("Uses Pole Material when left empty.")]
    public Material ropeMaterial;

    [Header("Colors")]
    public Color color1 = new Color(0.93962264f, 0.28897822f, 0.68733203f, 1f);
    public Color color2 = new Color(0.5568628f, 0.9058824f, 0.8862746f, 1f);
    public Color color3 = new Color(0.9547169f, 0.84824276f, 0.24318251f, 1f);
    public Color color4 = new Color(0.014934411f, 0.014489126f, 0.6981132f, 1f);
    public bool randomizeColors = true;

    [Header("Hanging Decoration")]
    [Tooltip("Used when no variants are configured below.")]
    public GameObject girlandePrefab;
    [Tooltip("Optional object variants. Each one can correct its own scale, position, and rotation.")]
    public HangingDecorationVariant[] hangingVariants;
    public bool randomizeObjects = true;
    [Tooltip("The result stays stable between rebuilds. Different checkpoints still get different patterns.")]
    public int randomSeed = 12345;
    [Min(0.01f)] public float hangingScale = 1f;
    [Min(0f)]
    [Tooltip("How far the objects hang below the rope, in collider-local units.")]
    public float hangingDistanceOffset;
    [Tooltip("Applied after the decoration is aimed along the rope.")]
    public Vector3 hangingRotationOffset;

    [Header("Dimensions (collider-local units)")]
    [Min(0.001f)] public float poleDiameter = 0.05f;
    [Min(0.001f)] public float ropeWidth = 0.015f;
    [Min(0f)] public float ropeSag = 0.5f;
    [Min(1)] public int ropeSegments = 16;
    [Min(0.01f)]
    [Tooltip("Fixed world-space distance between hanging objects, measured along the sagged rope.")]
    public float hangingObjectSpacing = 2f;

    private void Start()
    {
        BuildDeco();
    }

    [ContextMenu("Build Decorations")]
    public void BuildDeco()
    {
        if (!TryGetCollider())
            return;

        ClearDeco();
        generatedRopes.Clear();

        int seed = GetStableSeed();
        objectRandom = new System.Random(seed);
        colorRandom = new System.Random(seed ^ 0x5F3759DF);
        decorationSequence = 0;

        Transform generatedRoot = CreateGeneratedRoot();
        Vector3[] bottomCorners = GetBottomCorners();

        float worldUnit = GetSmallestHorizontalWorldUnit();
        float worldPoleDiameter = poleDiameter * worldUnit;
        float worldRopeWidth = ropeWidth * worldUnit;

        for (int i = 0; i < bottomCorners.Length; i++)
        {
            Vector3 bottom = checkpointCollider.transform.TransformPoint(bottomCorners[i]);
            Vector3 top = checkpointCollider.transform.TransformPoint(
                bottomCorners[i] + Vector3.up * checkpointCollider.size.y);

            CreatePole(bottom, top, worldPoleDiameter, generatedRoot);
        }

        for (int i = 0; i < bottomCorners.Length; i++)
        {
            Vector3 start = bottomCorners[i] + Vector3.up * checkpointCollider.size.y;
            Vector3 end = bottomCorners[(i + 1) % bottomCorners.Length]
                          + Vector3.up * checkpointCollider.size.y;

            CreateGarland(start, end, worldRopeWidth, generatedRoot);
        }
    }

    [ContextMenu("Clear Decorations")]
    public void ClearDeco()
    {
        generatedRopes.Clear();

        Transform parent = GetDecorationParent();
        Transform generatedRoot = parent.Find(GeneratedRootName);

        if (generatedRoot == null)
            return;

        if (Application.isPlaying)
            Destroy(generatedRoot.gameObject);
        else
            DestroyImmediate(generatedRoot.gameObject);
    }

    private bool TryGetCollider()
    {
        if (checkpointCollider == null)
            checkpointCollider = GetComponent<BoxCollider>();

        if (checkpointCollider != null)
            return true;

        Debug.LogError("Checkpoint decoration requires a BoxCollider.", this);
        return false;
    }

    private Transform GetDecorationParent()
    {
        return DecoParent != null ? DecoParent : transform;
    }

    private Transform CreateGeneratedRoot()
    {
        GameObject root = new GameObject(GeneratedRootName);
        root.transform.SetParent(GetDecorationParent(), false);
        return root.transform;
    }

    private Vector3[] GetBottomCorners()
    {
        Vector3 center = checkpointCollider.center;
        Vector3 half = checkpointCollider.size * 0.5f;

        return new[]
        {
            center + new Vector3(-half.x, -half.y, -half.z),
            center + new Vector3( half.x, -half.y, -half.z),
            center + new Vector3( half.x, -half.y,  half.z),
            center + new Vector3(-half.x, -half.y,  half.z)
        };
    }

    private float GetSmallestHorizontalWorldUnit()
    {
        Transform colliderTransform = checkpointCollider.transform;
        float xScale = colliderTransform.TransformVector(Vector3.right).magnitude;
        float zScale = colliderTransform.TransformVector(Vector3.forward).magnitude;
        return Mathf.Max(0.0001f, Mathf.Min(xScale, zScale));
    }

    private void CreatePole(
        Vector3 bottom,
        Vector3 top,
        float worldDiameter,
        Transform parent)
    {
        Vector3 poleVector = top - bottom;
        float height = poleVector.magnitude;

        if (height <= Mathf.Epsilon)
            return;

        GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole.name = "Pole";
        pole.transform.SetPositionAndRotation(
            Vector3.Lerp(bottom, top, 0.5f),
            Quaternion.FromToRotation(Vector3.up, poleVector));

        // Unity's built-in cylinder is one unit wide and two units tall.
        pole.transform.localScale = new Vector3(
            worldDiameter,
            height * 0.5f,
            worldDiameter);

        MeshRenderer renderer = pole.GetComponent<MeshRenderer>();
        if (poleMaterial != null)
            renderer.sharedMaterial = poleMaterial;

        renderer.shadowCastingMode = ShadowCastingMode.On;
        pole.transform.SetParent(parent, true);

        Collider poleCollider = pole.GetComponent<Collider>();
        poleCollider.enabled = false;

        if (Application.isPlaying)
            Destroy(poleCollider);
        else
            DestroyImmediate(poleCollider);
    }

    private void CreateGarland(
        Vector3 localStart,
        Vector3 localEnd,
        float worldWidth,
        Transform parent)
    {
        GameObject ropeObject = new GameObject("Garland");
        ropeObject.transform.SetParent(parent, false);

        LineRenderer line = ropeObject.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = Mathf.Max(1, ropeSegments) + 1;
        line.widthMultiplier = worldWidth;
        line.numCornerVertices = 2;
        line.numCapVertices = 2;
        line.shadowCastingMode = ShadowCastingMode.Off;
        line.receiveShadows = false;

        Material material = ropeMaterial != null ? ropeMaterial : poleMaterial;
        if (material != null)
            line.sharedMaterial = material;

        generatedRopes.Add(line);
        if (hasRopeColorOverride)
            ApplyColor(line, ropeColorOverride);

        int segmentCount = line.positionCount - 1;
        for (int i = 0; i <= segmentCount; i++)
        {
            float t = i / (float)segmentCount;
            line.SetPosition(i, GetRopeWorldPoint(localStart, localEnd, t));
        }

        CreateHangingDecorations(localStart, localEnd, parent);
    }

    private void CreateHangingDecorations(
        Vector3 localStart,
        Vector3 localEnd,
        Transform parent)
    {
        if (!HasHangingDecoration() || hangingObjectSpacing <= 0f)
            return;

        int sampleCount = Mathf.Max(32, ropeSegments * 4);
        float[] cumulativeDistances = new float[sampleCount + 1];
        Vector3 previousPoint = GetRopeWorldPoint(localStart, localEnd, 0f);

        for (int i = 1; i <= sampleCount; i++)
        {
            float t = i / (float)sampleCount;
            Vector3 point = GetRopeWorldPoint(localStart, localEnd, t);
            cumulativeDistances[i] = cumulativeDistances[i - 1]
                                     + Vector3.Distance(previousPoint, point);
            previousPoint = point;
        }

        float ropeLength = cumulativeDistances[sampleCount];
        if (ropeLength <= Mathf.Epsilon)
            return;

        int decorationCount = Mathf.Max(
            1,
            Mathf.FloorToInt(ropeLength / hangingObjectSpacing));

        // Keep the requested gap exact and split unused rope evenly between both ends.
        float occupiedLength = (decorationCount - 1) * hangingObjectSpacing;
        float firstDistance = (ropeLength - occupiedLength) * 0.5f;

        for (int i = 0; i < decorationCount; i++)
        {
            float distanceAlongRope = firstDistance + i * hangingObjectSpacing;
            float t = GetRopeParameterAtDistance(
                cumulativeDistances,
                distanceAlongRope,
                sampleCount);
            HangingDecorationVariant variant = SelectVariant(decorationSequence);
            GameObject prefab = variant != null ? variant.prefab : girlandePrefab;

            if (prefab == null)
                continue;

            Vector3 localPosition = GetRopeLocalPoint(localStart, localEnd, t)
                                    + Vector3.down * hangingDistanceOffset;
            Vector3 worldPosition = checkpointCollider.transform.TransformPoint(localPosition);
            Vector3 worldDirection = checkpointCollider.transform.TransformVector(
                localEnd - localStart);
            if (worldDirection.sqrMagnitude <= Mathf.Epsilon)
                worldDirection = checkpointCollider.transform.forward;
            else
                worldDirection.Normalize();
            Vector3 worldUp = checkpointCollider.transform.TransformDirection(Vector3.up);

            Vector3 variantRotation = variant != null
                ? variant.rotationOffset
                : Vector3.zero;
            Quaternion rotation = Quaternion.LookRotation(worldDirection, worldUp)
                                  * Quaternion.Euler(hangingRotationOffset + variantRotation);

            if (variant != null)
                worldPosition += rotation * variant.positionOffset;

            GameObject decoration = Instantiate(
                prefab,
                worldPosition,
                rotation);

            decoration.name = prefab.name;
            float variantScale = variant != null ? variant.scaleMultiplier : 1f;
            decoration.transform.localScale *= hangingScale * variantScale;

            if (variant == null || variant.applyColor)
                ApplyColor(decoration, SelectColor(decorationSequence));

            decoration.transform.SetParent(parent, true);
            decorationSequence++;
        }
    }

    private static float GetRopeParameterAtDistance(
        float[] cumulativeDistances,
        float targetDistance,
        int sampleCount)
    {
        for (int i = 1; i <= sampleCount; i++)
        {
            if (cumulativeDistances[i] < targetDistance)
                continue;

            float segmentStartDistance = cumulativeDistances[i - 1];
            float segmentLength = cumulativeDistances[i] - segmentStartDistance;
            float segmentFraction = segmentLength > Mathf.Epsilon
                ? (targetDistance - segmentStartDistance) / segmentLength
                : 0f;

            return (i - 1 + segmentFraction) / sampleCount;
        }

        return 1f;
    }

    private bool HasHangingDecoration()
    {
        if (girlandePrefab != null)
            return true;

        if (hangingVariants == null)
            return false;

        for (int i = 0; i < hangingVariants.Length; i++)
        {
            if (hangingVariants[i] != null && hangingVariants[i].prefab != null)
                return true;
        }

        return false;
    }

    private HangingDecorationVariant SelectVariant(int sequence)
    {
        int validCount = GetValidVariantCount();
        if (validCount == 0)
            return null;

        int wantedIndex = randomizeObjects
            ? objectRandom.Next(validCount)
            : sequence % validCount;

        for (int i = 0; i < hangingVariants.Length; i++)
        {
            HangingDecorationVariant variant = hangingVariants[i];
            if (variant == null || variant.prefab == null)
                continue;

            if (wantedIndex == 0)
                return variant;

            wantedIndex--;
        }

        return null;
    }

    private int GetValidVariantCount()
    {
        if (hangingVariants == null)
            return 0;

        int count = 0;
        for (int i = 0; i < hangingVariants.Length; i++)
        {
            if (hangingVariants[i] != null && hangingVariants[i].prefab != null)
                count++;
        }

        return count;
    }

    private Color SelectColor(int sequence)
    {
        int colorIndex = randomizeColors
            ? colorRandom.Next(4)
            : sequence % 4;

        switch (colorIndex)
        {
            case 0: return color1;
            case 1: return color2;
            case 2: return color3;
            default: return color4;
        }
    }

    private static void ApplyColor(GameObject decoration, Color color)
    {
        Renderer[] renderers = decoration.GetComponentsInChildren<Renderer>(true);
        MaterialPropertyBlock properties = new MaterialPropertyBlock();

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            renderer.GetPropertyBlock(properties);
            properties.SetColor(BaseColorId, color);
            properties.SetColor(ColorId, color);
            renderer.SetPropertyBlock(properties);
            properties.Clear();
        }
    }

    public void SetRopeColor(Color color)
    {
        hasRopeColorOverride = true;
        ropeColorOverride = color;

        for (int i = 0; i < generatedRopes.Count; i++)
        {
            if (generatedRopes[i] != null)
                ApplyColor(generatedRopes[i], color);
        }
    }

    private static void ApplyColor(Renderer renderer, Color color)
    {
        MaterialPropertyBlock properties = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(properties);
        properties.SetColor(BaseColorId, color);
        properties.SetColor(ColorId, color);
        renderer.SetPropertyBlock(properties);
    }

    private Vector3 GetRopeWorldPoint(
        Vector3 localStart,
        Vector3 localEnd,
        float t)
    {
        return checkpointCollider.transform.TransformPoint(
            GetRopeLocalPoint(localStart, localEnd, t));
    }

    private Vector3 GetRopeLocalPoint(
        Vector3 localStart,
        Vector3 localEnd,
        float t)
    {
        float sag = -ropeSag * 4f * t * (1f - t);
        return Vector3.Lerp(localStart, localEnd, t) + Vector3.up * sag;
    }

    private int GetStableSeed()
    {
        Vector3 position = checkpointCollider.transform.position;

        unchecked
        {
            int seed = randomSeed;
            seed = seed * 397 ^ Mathf.RoundToInt(position.x * 10f);
            seed = seed * 397 ^ Mathf.RoundToInt(position.y * 10f);
            seed = seed * 397 ^ Mathf.RoundToInt(position.z * 10f);
            return seed;
        }
    }

    private void OnValidate()
    {
        ropeSegments = Mathf.Max(1, ropeSegments);
        hangingObjectSpacing = Mathf.Max(0.01f, hangingObjectSpacing);
        poleDiameter = Mathf.Max(0.001f, poleDiameter);
        ropeWidth = Mathf.Max(0.001f, ropeWidth);
        hangingScale = Mathf.Max(0.01f, hangingScale);
        hangingDistanceOffset = Mathf.Max(0f, hangingDistanceOffset);

        if (hangingVariants == null)
            return;

        for (int i = 0; i < hangingVariants.Length; i++)
        {
            if (hangingVariants[i] != null)
                hangingVariants[i].scaleMultiplier = Mathf.Max(
                    0.01f,
                    hangingVariants[i].scaleMultiplier);
        }
    }
}
