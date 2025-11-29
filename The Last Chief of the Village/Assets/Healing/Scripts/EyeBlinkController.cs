using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace Mythmatic
{
    public class EyeBlinkController : MonoBehaviour
    {
        [Header("Eye Sprite References")]
        public List<GameObject> eyePieces = new List<GameObject>();    // List of all eye piece GameObjects

        [Header("Sleep Settings")]
        public bool isSleeping = false;          // Toggle for sleep state
        [Range(1f, 60f)]
        public float minSleepDuration = 5f;      // Minimum time to stay asleep
        [Range(1f, 60f)]
        public float maxSleepDuration = 10f;     // Maximum time to stay asleep
        [Range(1f, 60f)]
        public float minAwakeTime = 3f;          // Minimum time to stay awake
        [Range(1f, 60f)]
        public float maxAwakeTime = 8f;          // Maximum time to stay awake

        [Header("Debug Settings")]
        public bool showDebugLogs = true;        // Toggle debug messages

        private float currentStateTimer;         // Current time remaining in state
        private float lastDebugTime;             // Track when we last showed debug message

        private void Start()
        {
            if (eyePieces.Count > 0)
            {
                SetEyePiecesState(!isSleeping);
                // Set initial timer
                currentStateTimer = isSleeping ?
                    Random.Range(minSleepDuration, maxSleepDuration) :
                    Random.Range(minAwakeTime, maxAwakeTime);
            }
            else
            {
                Debug.LogError("[EyeBlink] No eye pieces added to the EyeBlinkController!");
            }
        }

        private void Update()
        {
            // Update timer
            if (currentStateTimer > 0)
            {
                currentStateTimer -= Time.deltaTime;
            }

            // Change state when timer runs out
            if (currentStateTimer <= 0)
            {
                if (isSleeping)
                {
                    WakeUp();
                }
                else
                {
                    StartSleeping();
                }
            }

            // Debug output every second
            if (showDebugLogs && Time.time - lastDebugTime >= 1f)
            {
                lastDebugTime = Time.time;
                string currentState = isSleeping ? "SLEEPING" : "AWAKE";
                string nextState = isSleeping ? "wake up" : "sleep";
                Debug.Log($"Current State: {currentState} - Time until {nextState}: {currentStateTimer:F1} seconds");
            }
        }

        private void SetEyePiecesState(bool state)
        {
            foreach (GameObject piece in eyePieces)
            {
                if (piece != null)
                {
                    piece.SetActive(state);
                }
            }
        }

        public void StartSleeping()
        {
            isSleeping = true;
            SetEyePiecesState(false);
            currentStateTimer = Random.Range(minSleepDuration, maxSleepDuration);
            Debug.Log($"[EyeBlink] Falling asleep for {currentStateTimer:F1} seconds");
        }

        public void WakeUp()
        {
            isSleeping = false;
            SetEyePiecesState(true);
            currentStateTimer = Random.Range(minAwakeTime, maxAwakeTime);
            Debug.Log($"[EyeBlink] Waking up for {currentStateTimer:F1} seconds");
        }
    }
}