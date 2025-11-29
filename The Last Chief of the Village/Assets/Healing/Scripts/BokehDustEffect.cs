using UnityEngine;
namespace Mythmatic
{
    [RequireComponent(typeof(Camera))]
public class BokehDustEffect : MonoBehaviour
{
    [Header("Distribution Settings")]
    [Tooltip("Controls how far particles can spawn from the center. Higher values create a larger cloud of particles.")]
    [SerializeField] private float distributionVolume = 10f;
    [Tooltip("Moves the entire particle system relative to the camera. Useful for adjusting the particle cloud position.")]
    [SerializeField] private Vector3 volumeOffset = Vector3.zero;
    [Tooltip("Stretches or squishes the particle distribution along each axis (X = width, Y = height, Z = depth).")]
    [SerializeField] private Vector3 volumeScale = Vector3.one;

    [Header("Particle Settings")]
    [Tooltip("Total number of dust particles to spawn. Higher values create denser effects but impact performance.")]
    [SerializeField] private int particleCount = 100;
    [Tooltip("The smallest possible size for individual particles.")]
    [SerializeField] private float minSize = 0.01f;
    [Tooltip("The largest possible size for individual particles.")]
    [SerializeField] private float maxSize = 0.05f;
    [Tooltip("How quickly particles drift upward. Higher values create faster upward movement.")]
    [SerializeField] private float baseSpeed = 0.2f;
    [Tooltip("How far particles can move from their original position when swaying. Higher values create more dramatic movement.")]
    [SerializeField] private float swayStrength = 0.3f;
    [Tooltip("How quickly particles complete a full sway motion. Higher values create faster back-and-forth movement.")]
    [SerializeField] private float swayFrequency = 0.5f;
    [Tooltip("Maximum distance particles can travel before resetting. Also affects the depth-based focus effect range.")]
    [SerializeField] private float depthRange = 5f;

    [Header("Visual Settings")]
    [Tooltip("The material used to render the particles. Should be a particle-compatible material with proper transparency settings.")]
    [SerializeField] private Material dustMaterial;
    [Tooltip("Base color and opacity of the particles. Alpha channel controls overall particle visibility.")]
    [SerializeField] private Color dustColor = new Color(1f, 1f, 1f, 0.5f);
    [Tooltip("How quickly the shimmer effect cycles. Higher values create faster brightness variations.")]
    [SerializeField] private float shimmerSpeed = 1f;
    [Tooltip("How much the shimmer effect affects particle brightness. Higher values create more noticeable shimmer.")]
    [SerializeField] private float shimmerStrength = 0.2f;
    [Tooltip("Controls how particle size changes with distance. Modify this curve to adjust the bokeh depth effect.")]
    [SerializeField] private AnimationCurve focusCurve = AnimationCurve.EaseInOut(0f, 0.5f, 1f, 1f);

    [Header("Flicker and Fade Settings")]
    [Tooltip("How quickly particles fade in and out. Higher values create faster opacity transitions.")]
    [SerializeField] private float fadeSpeed = 0.5f;
    [Tooltip("Random variation in fade speed between particles. Higher values create more irregular fading.")]
    [SerializeField] private float fadeVariation = 0.2f;
    [Tooltip("How quickly the flicker effect updates. Higher values create more rapid brightness variations.")]
    [SerializeField] private float flickerSpeed = 3f;
    [Tooltip("How much the flicker effect affects particle brightness. Higher values create more noticeable flickering.")]
    [SerializeField] private float flickerIntensity = 0.3f;
    [Tooltip("Minimum opacity threshold below which particles become invisible. Helps prevent rendering very faint particles.")]
    [SerializeField] private float visibilityThreshold = 0.2f;

    private ParticleSystem dustSystem;
    private Camera mainCamera;
    private ParticleSystem.Particle[] particles;
    private Vector3[] initialPositions;
    private float[] particlePhases, fadeStates, fadeDirections;

    private void Start()
    {
        mainCamera = GetComponent<Camera>();
        SetupParticleSystem();
        InitializeParticles();
    }

    private Vector3 GetRandomPositionInCameraView(float depth) =>
        mainCamera.ViewportToWorldPoint(new Vector3(Random.value, Random.value, depth));

    private void SetupParticleSystem()
    {
        var dustObj = new GameObject("BokehDust");
        dustObj.transform.parent = transform;
        dustObj.transform.localPosition = volumeOffset;
        dustSystem = dustObj.AddComponent<ParticleSystem>();

        var main = dustSystem.main;
        main.loop = true; main.playOnAwake = true; main.maxParticles = particleCount;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = Mathf.Infinity;

        var emission = dustSystem.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, particleCount) });

        var shape = dustSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = distributionVolume * volumeScale;

        var pRenderer = dustSystem.GetComponent<ParticleSystemRenderer>();
        pRenderer.material = dustMaterial;
        pRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        pRenderer.sortMode = ParticleSystemSortMode.Distance;
        pRenderer.sortingFudge = -1;
        pRenderer.sortingLayerName = "Foreground";
        pRenderer.sortingOrder = 100;

        particles = new ParticleSystem.Particle[particleCount];
        initialPositions = new Vector3[particleCount];
        particlePhases = new float[particleCount];
        fadeStates = new float[particleCount];
        fadeDirections = new float[particleCount];
    }

    private void InitializeParticles()
    {
        dustSystem.GetParticles(particles);
        for (int i = 0; i < particleCount; i++)
        {
            initialPositions[i] = GetRandomPositionInCameraView(Random.Range(mainCamera.nearClipPlane, depthRange)) + volumeOffset;
            particlePhases[i] = Random.value * Mathf.PI * 2;
            fadeStates[i] = Random.value;
            fadeDirections[i] = Random.value > 0.5f ? 1f : -1f;
            particles[i].position = initialPositions[i];
            particles[i].startSize = Random.Range(minSize, maxSize);
            particles[i].startColor = dustColor;
        }
        dustSystem.SetParticles(particles, particles.Length);
    }

    private void Update()
    {
        dustSystem.GetParticles(particles);
        for (int i = 0; i < particles.Length; i++)
        {
            float time = Time.time + particlePhases[i];
            fadeStates[i] += fadeDirections[i] * fadeSpeed * Time.deltaTime * (1f + Random.value * fadeVariation);
            if (fadeStates[i] >= 1f || fadeStates[i] <= 0f)
            {
                fadeDirections[i] *= -1f;
                fadeStates[i] = Mathf.Clamp01(fadeStates[i]);
            }

            Vector3 sway = new Vector3(
                Mathf.Sin(time * swayFrequency) * swayStrength,
                Mathf.Cos(time * swayFrequency * 0.5f) * swayStrength,
                Mathf.Sin(time * swayFrequency * 0.7f) * swayStrength
            );

            particles[i].position = initialPositions[i] + sway + Vector3.up * baseSpeed * Time.deltaTime;

            var viewportPoint = mainCamera.WorldToViewportPoint(particles[i].position);
            if (viewportPoint.x < 0f || viewportPoint.x > 1f || viewportPoint.y < 0f || viewportPoint.y > 1f || viewportPoint.z < 0f || viewportPoint.z > depthRange)
            {
                initialPositions[i] = GetRandomPositionInCameraView(Random.Range(mainCamera.nearClipPlane, depthRange)) + volumeOffset;
                particles[i].position = initialPositions[i];
            }

            float distanceToCamera = Vector3.Distance(particles[i].position, transform.position);
            float normalizedDistance = Mathf.Clamp01(distanceToCamera / depthRange);
            float focusMultiplier = focusCurve.Evaluate(normalizedDistance);

            particles[i].startSize = Mathf.Lerp(minSize, maxSize, focusMultiplier) *
                (1f + Mathf.Sin(time * shimmerSpeed) * shimmerStrength);

            Color particleColor = dustColor;
            particleColor.a *= focusMultiplier * Mathf.SmoothStep(0f, 1f, fadeStates[i]) *
                (1f + Mathf.PerlinNoise(time * flickerSpeed, particlePhases[i]) * flickerIntensity);
            particles[i].startColor = particleColor.a < visibilityThreshold ? new Color(dustColor.r, dustColor.g, dustColor.b, 0f) : particleColor;
        }
        dustSystem.SetParticles(particles, particles.Length);
    }

    private void OnDestroy()
    {
        if (dustSystem != null && dustSystem.gameObject != null) Destroy(dustSystem.gameObject);
    }
}
}