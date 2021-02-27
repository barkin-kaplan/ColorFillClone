using System;
using ColorFill.helper.object_manager;
using UnityEngine;

namespace ColorFill.game.elements.fill.full_fill
{
    public class FullFill : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("HalfFill"))
            {
                GameObjectManager.Instance.DestroyObject(other.gameObject);
            }
        }
    }
}
