using ColorFill.game.ui.player_status.progress_bar;
using ColorFill.helper;
using TMPro;
using UnityEngine;

namespace ColorFill.game.ui.player_status
{
    public class PlayerStatusUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _gemText;
        [SerializeField] private TextMeshProUGUI _currentLevelText;
        [SerializeField] private TextMeshProUGUI _nextLevelText;
        [SerializeField] private ProgressBar[] stageProgresses;

        public void SetStageRatio(int stage,float ratio)
        {
            stageProgresses[stage].UpdateRatio(ratio);
        }

        public void SetCurrentLevel(int level)
        {
            _currentLevelText.text = Util.ToString(level);
        }
        
        public void SetNextLevel(int level)
        {
            var text = level == 0 ? "End" : Util.ToString(level);
            _nextLevelText.text = text;
        }

        public void SetGemCount(int count)
        {
            _gemText.text = Util.ToString(count);
        }
    }
}
