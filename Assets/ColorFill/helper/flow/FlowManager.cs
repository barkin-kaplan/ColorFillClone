using UnityEngine.SceneManagement;

namespace ColorFill.helper.flow
{
    public class FlowManager
    {
        public const int SplashIndex = 0;
        public const int GameIndex = 1;

        public static void LoadScene(int index)
        {
            SceneManager.LoadScene(index);
        }
    }
}