using ColorFill.helper;
using ColorFill.helper.context;
using UnityEngine;
using UnityEngine.UI;

namespace ColorFill.game.ui.game_over_overlay
{
    public class GameOverOverlay : MonoBehaviour
    {
        [SerializeField] private Button _reviveButton;
        void Awake()
        {
            if (!Util.DontDestroyOnLoad<GameOverOverlay>(gameObject))
            {
                return;
            }
            
            _reviveButton.interactable = GameContext.Instance.GetGemCount() > 4;
        }

        public void RestartLevel()
        {
            Destroy(gameObject);
            GameContext.Instance.RestartStage();
        }

        public void RevivePlayer()
        {
            Destroy(gameObject);
            GameContext.Instance.RevivePlayer();
        }
    }
}
