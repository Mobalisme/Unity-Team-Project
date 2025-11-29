using UnityEngine;

namespace Mythmatic
{
    [ExecuteInEditMode]

public class SteamGenerator : MonoBehaviour
{
    [Range(1, 50)] public float length = 10f;
    [Range(1, 50)] public float width = 5f;
    [Range(2, 500)] public int lengthDivisions = 10;
    [Range(2, 500)] public int widthDivisions = 2;
    [Range(0, 2)] public float waveAmplitude = .5f;
    [Range(.1f, 10)] public float waveFrequency = 1f;
    [Range(.1f, 10)] public float uvTiling = 1f;
    [Range(-5, 5)] public float uvAnimationSpeed = 1f;
    [Range(0, 360)] public float phaseOffset = 0f;
    public bool reverseUVDirection = false;
    public int sortingOrder = 0;

    [Header("Start Edge Fading")]
    [Range(0f, 1f)] public float fadeStartAmount = 0f;
    [Range(.1f, 5f)] public float fadeStartFalloff = 1f;

    [Header("End Edge Fading")]
    [Range(0f, 1f)] public float fadeEndAmount = 0f;
    [Range(.1f, 5f)] public float fadeEndFalloff = 1f;

    public Material material;
    Mesh mesh;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    Vector3[] vertices;
    Vector2[] uvs;
    Color32[] colors;
    int[] triangles;
    float uvOffset = 0f;
    Vector3 initialLocalPosition;
    Quaternion initialLocalRotation;
    Vector3 initialLocalScale;
    bool hasInitialized;

    void Awake()
    {
        if (!hasInitialized)
        {
            initialLocalPosition = transform.localPosition;
            initialLocalRotation = transform.localRotation;
            initialLocalScale = transform.localScale;
            hasInitialized = true;
        }
    }

    void OnEnable()
    {
        mesh = new Mesh();
        if (!(meshFilter = GetComponent<MeshFilter>())) meshFilter = gameObject.AddComponent<MeshFilter>();
        if (!(meshRenderer = GetComponent<MeshRenderer>())) meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshFilter.mesh = mesh;
        if (transform.localRotation == Quaternion.identity)
        {
            transform.localRotation = Quaternion.Euler(0, -90, 90);
            initialLocalRotation = transform.localRotation;
        }
        if (material) meshRenderer.sharedMaterial = material;
        UpdateRenderQueue();
    }

    void Update()
    {
        if (Application.isPlaying) uvOffset += Time.deltaTime * uvAnimationSpeed;
        if (!Application.isPlaying)
        {
            initialLocalPosition = transform.localPosition;
            initialLocalRotation = transform.localRotation;
            initialLocalScale = transform.localScale;
        }
        GenerateMesh();
    }

    void UpdateRenderQueue()
    {
        if (meshRenderer)
        {
            meshRenderer.sortingOrder = sortingOrder;
            if (meshRenderer.sharedMaterial)
            {
                meshRenderer.sharedMaterial = new Material(meshRenderer.sharedMaterial);
                meshRenderer.sharedMaterial.renderQueue = 3000 + sortingOrder;
            }
        }
    }

    float CalculateEdgeFade(float t)
    {
        float startFade = 1f;
        float endFade = 1f;

        if (fadeStartAmount > 0) startFade = Mathf.Clamp01(Mathf.Pow(t / fadeStartAmount, fadeStartFalloff));
        if (fadeEndAmount > 0) endFade = Mathf.Clamp01(Mathf.Pow((1f - t) / fadeEndAmount, fadeEndFalloff));

        return Mathf.Min(startFade, endFade);
    }

    void GenerateMesh()
    {
        int vertexCount = (lengthDivisions + 1) * (widthDivisions + 1);
        vertices = new Vector3[vertexCount];
        uvs = new Vector2[vertexCount];
        colors = new Color32[vertexCount];
        triangles = new int[lengthDivisions * widthDivisions * 6];

        for (int i = 0; i <= lengthDivisions; i++)
        {
            float t = i / (float)lengthDivisions;
            float sideOffset = Mathf.Sin((t * waveFrequency * Mathf.PI * 2) + (phaseOffset * Mathf.Deg2Rad)) * waveAmplitude;
            float fadeAlpha = CalculateEdgeFade(t);

            for (int j = 0; j <= widthDivisions; j++)
            {
                float widthT = j / (float)widthDivisions;
                int vertexIndex = i * (widthDivisions + 1) + j;

                vertices[vertexIndex] = new Vector3(length * t, 0, Mathf.Lerp(-width / 2, width / 2, widthT) + sideOffset);
                uvs[vertexIndex] = new Vector2(reverseUVDirection ? 1 - widthT : widthT, (t + uvOffset) * uvTiling);
                colors[vertexIndex] = new Color32(255, 255, 255, (byte)(fadeAlpha * 255));

                if (i < lengthDivisions && j < widthDivisions)
                {
                    int triangleIndex = (i * widthDivisions + j) * 6;
                    int currentVertex = i * (widthDivisions + 1) + j;
                    triangles[triangleIndex] = currentVertex;
                    triangles[triangleIndex + 1] = currentVertex + widthDivisions + 1;
                    triangles[triangleIndex + 2] = currentVertex + 1;
                    triangles[triangleIndex + 3] = currentVertex + 1;
                    triangles[triangleIndex + 4] = currentVertex + widthDivisions + 1;
                    triangles[triangleIndex + 5] = currentVertex + widthDivisions + 2;
                }
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.colors32 = colors;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    void OnValidate() => UpdateRenderQueue();

#if UNITY_EDITOR
    void OnDisable()
    {
        if (!Application.isPlaying && mesh != null) DestroyImmediate(mesh);
    }
#endif
}
}