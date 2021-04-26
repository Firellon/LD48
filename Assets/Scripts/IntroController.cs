using UnityEngine;
using UnityEngine.SceneManagement;

namespace LD48
{
    public class IntroController : MonoBehaviour
    {
        public string nextSceneName;
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.anyKey)
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}
