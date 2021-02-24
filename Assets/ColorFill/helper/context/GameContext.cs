using System;
using ColorFill.game.level;
using UnityEngine;

namespace ColorFill.helper.context
{
    public class GameContext : MonoBehaviour
    {
        [SerializeField] private GameObject _levelPrefab;
        private void Awake()
        {
            Instantiate(_levelPrefab);
            Level.Instance.LoadLevel(1);
        }
    }
}