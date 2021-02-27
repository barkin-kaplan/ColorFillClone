using System;
using ColorFill.game.level;
using UnityEngine;

namespace ColorFill.helper.context
{
    public class GameContext : MonoBehaviour
    {
        [SerializeField] private GameObject _levelPrefab;
        [SerializeField] private GameObject _gameOverOverlay;
        private Canvas _canvas;
        public static GameContext Instance;
        private void Awake()
        {
            if (!Util.DontDestroyOnLoad<GameContext>(gameObject))
            {
                return;
            }
            
            Instantiate(_levelPrefab);
            Instance = this;
            _canvas = FindObjectOfType<Canvas>();
            LoadNextLevel();
        }

        public void ShowGameOver()
        {
            Instantiate(_gameOverOverlay, _canvas.transform);
        }

        public void LoadNextLevel()
        {
            Level.Instance.LoadLevel(1);
        }
    }
}