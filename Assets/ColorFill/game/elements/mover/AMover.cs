using System;
using ColorFill.helper.context;
using UnityEngine;

namespace ColorFill.game.elements.mover
{
    public class AMover : MonoBehaviour
    {
        [SerializeField] private GameObject _particlePrefab;
        protected void OnDestroy()
        {
            var particle =Instantiate(_particlePrefab);
            particle.transform.position = transform.position + new Vector3(0, 0, -0.5f);
        }


        protected void OnTriggerEnter(Collider other)
        {
            var otherObj = other.gameObject;
            if (otherObj.CompareTag("HalfFill"))
            {
                Destroy(FindObjectOfType<Player>().gameObject);
                GameContext.Instance.ShowGameOver();
            }
        }
    }
}