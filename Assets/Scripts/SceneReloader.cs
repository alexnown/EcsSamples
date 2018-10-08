using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReloader : MonoBehaviour
{
    public float TimeBeforeSceneReload;

    private void Update()
    {
        if (TimeBeforeSceneReload <= 0) return;
        TimeBeforeSceneReload -= Time.deltaTime;
        if (TimeBeforeSceneReload < 0) ReloadScene();
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
