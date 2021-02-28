using ColorFill.helper.context;
using UnityEngine;

namespace ColorFill.game.elements.gem
{
    public class Gem : MonoBehaviour
    {
        public void Collect()
        {
            GameContext.Instance.AddGem(1);
            Destroy(gameObject);
        }
    }
}