using System;
using LD48.Cutscenes;
using Player;
using Signals;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LD48
{
    public class GameController: MonoBehaviour
    {
        private bool isEnabled = true;

        private void Start()
        {
            #if !UNITY_EDITOR
            SignalsHub.DispatchAsync(new StartCutsceneSignal
            {
                Type = CutsceneType.Intro,
            });
            #endif
        }

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
            #if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.T))
            {
                SignalsHub.DispatchAsync(new StartCutsceneSignal
                {
                    Type = CutsceneType.Intro,
                });
            }
            #endif

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