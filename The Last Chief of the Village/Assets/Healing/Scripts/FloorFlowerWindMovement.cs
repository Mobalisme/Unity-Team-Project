using UnityEngine;
using System.Collections.Generic;
namespace Mythmatic
{
    public class FloorFlowerWindMovement : MonoBehaviour
    {
        [Header("Objects")]
        [Tooltip("List of objects to apply the wind effect to")]
        public List<GameObject> objectsToAnimate = new List<GameObject>();

        [Header("Wind Settings")]
        [Tooltip("How fast the objects rotate")]
        public float windSpeed = 2.0f;

        [Header("Rotation Angles")]
        [Tooltip("Maximum rotation angle around X axis (in degrees)")]
        [Range(0f, 45f)]
        public float maxRotationAngleX = 15f;

        [Tooltip("Maximum rotation angle around Y axis (in degrees)")]
        [Range(0f, 45f)]
        public float maxRotationAngleY = 15f;

        [Tooltip("Maximum rotation angle around Z axis (in degrees)")]
        [Range(0f, 45f)]
        public float maxRotationAngleZ = 15f;

        [Header("Effects")]
        [Tooltip("Adds randomness to the movement")]
        [Range(0f, 2f)]
        public float turbulence = 1.0f;

        [Header("Axis Settings")]
        [Tooltip("Enable rotation around X axis")]
        public bool rotateX = false;

        [Tooltip("Enable rotation around Y axis")]
        public bool rotateY = false;

        [Tooltip("Enable rotation around Z axis")]
        public bool rotateZ = true;

        // Private variables for internal calculations
        private Dictionary<GameObject, Vector3> startRotations = new Dictionary<GameObject, Vector3>();
        private Dictionary<GameObject, float> timeOffsets = new Dictionary<GameObject, float>();

        void Start()
        {
            // Initialize each object in the list
            foreach (GameObject obj in objectsToAnimate)
            {
                if (obj != null)
                {
                    // Store the initial rotation for each object
                    startRotations[obj] = obj.transform.localEulerAngles;

                    // Add random offset for each object to make them move differently
                    timeOffsets[obj] = Random.Range(0f, 1000f);
                }
            }
        }

        void Update()
        {
            foreach (GameObject obj in objectsToAnimate)
            {
                if (obj != null)
                {
                    // Calculate the base wind movement using sine wave
                    float time = (Time.time + timeOffsets[obj]) * windSpeed;

                    // Add some turbulence using additional sine waves
                    float turbulenceFactor = 1f + (Mathf.Sin(time * 1.3f) * 0.5f * turbulence);

                    // Calculate rotation angles for each axis with different frequencies
                    float xAngle = rotateX ? Mathf.Sin(time) * maxRotationAngleX * turbulenceFactor : 0f;
                    float yAngle = rotateY ? Mathf.Sin(time * 0.9f) * maxRotationAngleY * turbulenceFactor : 0f;
                    float zAngle = rotateZ ? Mathf.Sin(time * 1.1f) * maxRotationAngleZ * turbulenceFactor : 0f;

                    // Apply the rotation to each object
                    obj.transform.localEulerAngles = startRotations[obj] + new Vector3(xAngle, yAngle, zAngle);
                }
            }
        }

        // Helper method to add objects at runtime
        public void AddObject(GameObject obj)
        {
            if (obj != null && !objectsToAnimate.Contains(obj))
            {
                objectsToAnimate.Add(obj);
                startRotations[obj] = obj.transform.localEulerAngles;
                timeOffsets[obj] = Random.Range(0f, 1000f);
            }
        }

        // Helper method to remove objects at runtime
        public void RemoveObject(GameObject obj)
        {
            if (obj != null && objectsToAnimate.Contains(obj))
            {
                objectsToAnimate.Remove(obj);
                startRotations.Remove(obj);
                timeOffsets.Remove(obj);
            }
        }
    }
}