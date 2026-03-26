using UnityEngine;

public class CutsceneSceneExitTrigger : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string nextSceneName = "NextScene";
    [SerializeField] private float fadeToBlackSeconds = 1f;

    private bool triggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered)
            return;

        if (!other.CompareTag(playerTag))
            return;

        triggered = true;

        if (LoadingSceneAutoWalkCutscene.Instance != null)
            LoadingSceneAutoWalkCutscene.Instance.EndCutscene();

        if (PersistentScreenFader.Instance != null)
        {
            PersistentScreenFader.Instance.FadeToBlackAndLoadScene(nextSceneName, fadeToBlackSeconds);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }
}