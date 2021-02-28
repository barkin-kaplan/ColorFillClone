using UnityEngine;

namespace ColorFill.game.ui.player_status.progress_bar
{
    public class ProgressBar : MonoBehaviour
    {
        [SerializeField] private GameObject fill;
        [SerializeField] private GameObject fillMask;

        public void UpdateRatio(float ratio)
        {
            var clampedRatio = Mathf.Clamp(ratio, 0, 1);
            fillMask.transform.localScale = new Vector3(clampedRatio, 1, 1);
            var fillScale = Mathf.Approximately(clampedRatio, 0) ? 1 : 1 / clampedRatio; 
            fill.transform.localScale = new Vector3(fillScale, 1, 1);
        }
    }
}
