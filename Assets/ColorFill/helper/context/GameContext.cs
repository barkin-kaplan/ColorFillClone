using System;
using ColorFill.game.level;
using ColorFill.game.ui.player_status;
using ColorFill.helper.data;
using ColorFill.helper.DI;
using UnityEngine;

namespace ColorFill.helper.context
{
    public class GameContext : MonoBehaviour
    {
        [SerializeField] private GameObject _levelPrefab;
        [SerializeField] private GameObject _gameOverOverlay;
        [SerializeField] private PlayerStatusUI _playerStatusUI;
        private Canvas _canvas;
        public static GameContext Instance;
        private PlayerData _playerData;
        private void Awake()
        {
            if (!Util.DontDestroyOnLoad<GameContext>(gameObject))
            {
                return;
            }
            
            Instantiate(_levelPrefab);
            Instance = this;
            _canvas = FindObjectOfType<Canvas>();
            _playerData = DIContainer.GetSingle<PlayerData>();
            _playerStatusUI = FindObjectOfType<PlayerStatusUI>();
            LoadPlayerData();
            LoadNextLevel();
        }

        public void ShowGameOver()
        {
            Instantiate(_gameOverOverlay, _canvas.transform);
        }

        public void LoadNextLevel()
        {
            Level.Instance.LoadLevel(_playerData.CurrentLevel);
        }

        public void RestartStage()
        {
            Level.Instance.RestartStage();
        }

        public void LoadPlayerData()
        {
            _playerStatusUI.SetGemCount(_playerData.GemCount);
            _playerStatusUI.SetCurrentLevel(_playerData.CurrentLevel);
            _playerStatusUI.SetNextLevel(_playerData.CurrentLevel + 1);
            SetStageRatio(0,0);
            SetStageRatio(1,0);
        }

        public void AddGem(int count)
        {
            _playerData.GemCount += count;
            _playerStatusUI.SetGemCount(_playerData.GemCount);
            _playerData.Save();
        }

        public void IterateLevel()
        {
            _playerData.CurrentLevel += 1;
            _playerStatusUI.SetCurrentLevel(_playerData.CurrentLevel);
            _playerStatusUI.SetNextLevel(_playerData.CurrentLevel + 1);
            _playerData.Save();
        }

        public void SetStageRatio(int stage,float ratio)
        {
            _playerStatusUI.SetStageRatio(stage,ratio);
        }
    }
}