using UnityEngine;
using UnityEngine.SceneManagement;

public class LakeMiniGamePortal : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene("Lake Mini Game");
        }
    }
}
