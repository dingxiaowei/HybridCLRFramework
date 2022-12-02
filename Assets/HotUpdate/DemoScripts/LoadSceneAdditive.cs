using UnityEngine;

namespace libx
{
    public class LoadSceneAdditive : MonoBehaviour
    {
        public string scene; 
        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            { 
                Assets.LoadSceneAsync(ResFormat.GetScene("Level"));
            }
            else if (Input.GetKeyUp(KeyCode.Space))
            {
                Assets.LoadSceneAsync(ResFormat.GetScene(scene), true);   
            }
        }
    }
}
