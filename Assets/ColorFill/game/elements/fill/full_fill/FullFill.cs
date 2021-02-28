using System;
using ColorFill.game.elements.gem;
using ColorFill.helper.object_manager;
using UnityEngine;

namespace ColorFill.game.elements.fill.full_fill
{
    public class FullFill : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            var otherObj = other.gameObject;
            if (otherObj.CompareTag("HalfFill") || other.CompareTag("Deadly"))
            {
                GameObjectManager.Instance.DestroyObject(other.gameObject);
            }
            else if (otherObj.CompareTag("Gem"))
            {
                otherObj.GetComponent<Gem>().Collect();
            }
        }
    }
}
