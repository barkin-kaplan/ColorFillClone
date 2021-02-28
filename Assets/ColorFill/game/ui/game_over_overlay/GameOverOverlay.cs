using ColorFill.helper;
using ColorFill.helper.context;
using UnityEngine;

namespace ColorFill.game.ui.game_over_overlay
{
    public class GameOverOverlay : MonoBehaviour
    {
        void Awake()
        {
            if (!Util.DontDestroyOnLoad<GameOverOverlay>(gameObject))
            {
                return;
            }
        }

        public void RestartLevel()
        {
            Destroy(gameObject);
            GameContext.Instance.RestartStage();
        }
    }
}
