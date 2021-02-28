using System;
using System.Collections;
using ColorFill.game.elements.gem;
using ColorFill.helper;
using ColorFill.helper.object_manager;
using UnityEngine;

namespace ColorFill.game.elements.fill.full_fill
{
    public class FullFill : MonoBehaviour
    {
        private Vector3 _originalScale;
        private int frameCount = 15;
        void Awake()
        {
            _originalScale = transform.localScale;
            StartCoroutine(CreateAnimation());
        }

        IEnumerator CreateAnimation()
        {
            
            for (int i = 1; i <= frameCount; i++)
            {
                transform.localScale = Vector3.Lerp(Vector3.zero, _originalScale, i / (float) frameCount);
                yield return new WaitForSeconds(Util.FrameWaitAmount);
            }
        }
        
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
