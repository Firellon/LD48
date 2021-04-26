using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LD48
{
    public class GameController: MonoBehaviour
    {
        private void Update()
        {
            #if !UNITY_WEBGL && !UNITY_EDITOR
            if (Input.GetKey(KeyCode.Escape) || Input.GetButtonDown("Cancel"))
            {
                Debug.Log("Quit");
                Application.Quit();
            }
            #endif
            
            if (Input.GetKey(KeyCode.R) || Input.GetButtonDown("Fire3"))
            {
                Debug.Log("Restart");
                var scene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(scene.name);
            }
        }
    }
}