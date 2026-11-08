using UnityEngine;
using UnityEngine.SceneManagement;

public class LaunchLoader : MonoBehaviour
{
    private void Start()
    {
        SceneManager.LoadScene("Bootstrap");
    }
}