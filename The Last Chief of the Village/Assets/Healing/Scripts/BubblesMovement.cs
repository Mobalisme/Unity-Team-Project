using UnityEngine;
using System.Collections.Generic;
namespace Mythmatic
{
    public class BubblesMovement : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> objectsToAnimate = new List<GameObject>();

    [SerializeField, Range(0f, 10f)]
    private float turbulenceIntensity = 1f;

    [SerializeField, Range(0.1f, 5f)]
    private float turbulenceSpeed = 1f;

    // Store original positions
    private Dictionary<GameObject, Vector3> originalPositions = new Dictionary<GameObject, Vector3>();
    private float time;

    private void Start()
    {
        // Store the initial positions of all objects
        foreach (GameObject obj in objectsToAnimate)
        {
            if (obj != null)
            {
                originalPositions[obj] = obj.transform.position;
            }
        }
    }

    private void Update()
    {
        time += Time.deltaTime * turbulenceSpeed;

        foreach (GameObject obj in objectsToAnimate)
        {
            if (obj != null)
            {
                Vector3 originalPos = originalPositions[obj];

                // Generate perlin noise for each axis
                float noiseX = Mathf.PerlinNoise(time + obj.GetInstanceID(), 0f) * 2f - 1f;
                float noiseY = Mathf.PerlinNoise(time + obj.GetInstanceID(), 100f) * 2f - 1f;
                float noiseZ = Mathf.PerlinNoise(time + obj.GetInstanceID(), 200f) * 2f - 1f;

                Vector3 noise = new Vector3(noiseX, noiseY, noiseZ);

                // Apply noise to position
                obj.transform.position = originalPos + (noise * turbulenceIntensity);
            }
        }
    }

    // Helper methods to modify the list at runtime
    public void AddObject(GameObject obj)
    {
        if (!objectsToAnimate.Contains(obj))
        {
            objectsToAnimate.Add(obj);
            originalPositions[obj] = obj.transform.position;
        }
    }

    public void RemoveObject(GameObject obj)
    {
        if (objectsToAnimate.Contains(obj))
        {
            objectsToAnimate.Remove(obj);
            originalPositions.Remove(obj);
        }
    }
}
}