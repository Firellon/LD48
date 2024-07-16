using Player;
using Signals;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LD48
{
    public class GameController: MonoBehaviour
    {
        private bool isEnabled = true;

        private void OnEnable()
        {
            SignalsHub.AddListener<PlayerInputEnabledEvent>(OnPlayerInputEnabled);
        }

        private void OnDisable()
        {
            SignalsHub.RemoveListener<PlayerInputEnabledEvent>(OnPlayerInputEnabled);
        }

        private void Update()
        {
            #if !UNITY_WEBGL && !UNITY_EDITOR
            if (Input.GetKey(KeyCode.Escape) || Input.GetButtonDown("Cancel"))
            {
                Debug.Log("Quit");
                Application.Quit();
            }
            #endif
            
            if (isEnabled && (Input.GetKey(KeyCode.R) || Input.GetButtonDown("Fire3")))
            {
                Debug.Log("Restart");
                var scene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(scene.name);
            }
        }
        
        private void OnPlayerInputEnabled(PlayerInputEnabledEvent evt)
        {
            isEnabled = evt.IsEnabled;
        }
    }
}