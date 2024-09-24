using UnityEngine;
using UnityEngine.SceneManagement;

namespace LD48.Cutscenes.SimpleCutscene
{
    public class RestartCutscene : MonoBehaviour, ICutscene
    {
        [SerializeField] private float delay = 1f;

        private float openTime;

        public void OnStart()
        {
            openTime = Time.time;
        }

        private void Update()
        {
            if ((Time.unscaledTime > (openTime + delay)) && Input.anyKeyDown)
            {
                RestartGame();
            }
        }

        private void RestartGame()
        {
            var sceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(sceneName);
        }
    }
}