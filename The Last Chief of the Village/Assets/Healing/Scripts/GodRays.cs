using UnityEngine;
namespace Mythmatic
{
    [ExecuteInEditMode]
    public class GodRays : MonoBehaviour
    {
        [Header("Ray Settings")]
        [SerializeField] Texture2D rayTexture;
        [SerializeField, Range(1f, 20f)] float rayLength = 8f;
        [SerializeField] Color rayColor = new Color(1f, 0.9f, 0.7f, 0.6f);
        [SerializeField, Range(1f, 10f)] float globalScale = 3f;

        [Header("Rotation")]
        [SerializeField, Range(0f, 360f)] float rayRotationAngle = 0f;

        [Header("Animation")]
        [SerializeField] bool enableAnimation = true;
        [SerializeField, Range(0f, 1f)] float opacityMin = 0.2f;
        [SerializeField, Range(0f, 1f)] float opacityMax = 0.8f;
        [SerializeField, Range(0f, 2f)] float lengthPulseStrength = 0.5f;

        [Header("Rendering")]
        [SerializeField] string sortingLayer = "Default";
        [SerializeField] int sortingOrder = 0;

        Material rayMat;
        MeshFilter meshFilter;
        MeshRenderer meshRend;
        Mesh rayMesh;

        void OnEnable()
        {
            InitializeComponents();
            UpdateRay();
        }

        void InitializeComponents()
        {
            meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();

            meshRend = GetComponent<MeshRenderer>();
            if (meshRend == null) meshRend = gameObject.AddComponent<MeshRenderer>();

            if (rayMat == null)
            {
                rayMat = new Material(Shader.Find("Sprites/Default")) { hideFlags = HideFlags.HideAndDontSave };
            }

            if (rayMesh == null)
            {
                rayMesh = new Mesh { name = "GodRayMesh" };
                rayMesh.MarkDynamic();
                meshFilter.mesh = rayMesh;
            }
        }

        void Update()
        {
            if (Application.isPlaying)
            {
                UpdateRay();
            }
        }

        void OnValidate()
        {
            // Ensure min is never greater than max
            if (opacityMin > opacityMax)
            {
                opacityMin = opacityMax;
            }

            if (enabled)
            {
                InitializeComponents();
                UpdateRay();
            }
        }

        void UpdateRay()
        {
            if (rayTexture == null || rayMat == null || rayMesh == null) return;

            rayMat.mainTexture = rayTexture;
            meshRend.material = rayMat;
            meshRend.sortingLayerName = sortingLayer;
            meshRend.sortingOrder = sortingOrder;

            Vector3 origin = Vector3.zero;
            Quaternion rotation = Quaternion.AngleAxis(rayRotationAngle, Vector3.forward);

            var vertices = new Vector3[4];
            var triangles = new int[6];
            var uvs = new Vector2[4];
            var colors = new Color[4];

            Vector2 dir = rotation * Vector2.up;

            // Calculate animated values
            float currentLength = rayLength;
            float currentOpacity = opacityMax;

            if (Application.isPlaying && enableAnimation)
            {
                float time = Time.time;

                // Length animation
                currentLength *= 1f + (Mathf.Sin(time) * lengthPulseStrength);

                // Opacity animation - convert sine wave (-1 to 1) to 0 to 1 range, then map to min/max
                float sineWave = (Mathf.Sin(time) + 1f) * 0.5f; // Convert to 0-1 range
                currentOpacity = Mathf.Lerp(opacityMin, opacityMax, sineWave);
            }
            else
            {
                currentOpacity = opacityMax;
            }

            Vector3 rayEnd = origin + new Vector3(dir.x, dir.y, 0) * currentLength;

            // Create quad
            Vector3 right = new Vector3(-dir.y, dir.x, 0);

            vertices[0] = origin - right;
            vertices[1] = origin + right;
            vertices[2] = rayEnd - right;
            vertices[3] = rayEnd + right;

            uvs[0] = new Vector2(0, 0);
            uvs[1] = new Vector2(1, 0);
            uvs[2] = new Vector2(0, 1);
            uvs[3] = new Vector2(1, 1);

            colors[0] = colors[1] = colors[2] = colors[3] = new Color(
                rayColor.r, rayColor.g, rayColor.b, rayColor.a * currentOpacity
            );

            triangles[0] = 0;
            triangles[1] = 2;
            triangles[2] = 1;
            triangles[3] = 1;
            triangles[4] = 2;
            triangles[5] = 3;

            rayMesh.Clear();
            rayMesh.vertices = vertices;
            rayMesh.triangles = triangles;
            rayMesh.uv = uvs;
            rayMesh.colors = colors;
            rayMesh.RecalculateBounds();
            rayMat.color = rayColor;
            transform.localScale = Vector3.one * globalScale;
        }

        void OnDestroy()
        {
            if (rayMat) DestroyImmediate(rayMat);
            if (rayMesh) DestroyImmediate(rayMesh);
        }
    }
}