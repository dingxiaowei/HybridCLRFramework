using UnityEngine;

namespace libx
{
    public class SceneLoader : MonoBehaviour
    {
        public string scene;
        public float delay;

        void Start()
        {
            if (delay > 0)
            {
                Invoke("LoadScene", 3);
                return;
            }
            LoadScene();
        }

        void LoadScene()
        {
            Assets.LoadSceneAsync(scene);
        }
    }
}
