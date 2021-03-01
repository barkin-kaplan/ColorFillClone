using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using ColorFill.game.camera;
using ColorFill.game.elements;
using ColorFill.game.elements.mover.horizontal_mover;
using ColorFill.game.elements.mover.vertical_mover;
using ColorFill.game.elements.wall;
using ColorFill.helper;
using ColorFill.helper.context;
using ColorFill.helper.data;
using ColorFill.helper.DI;
using ColorFill.helper.level;
using ColorFill.helper.matrix;
using ColorFill.helper.object_manager;
using Newtonsoft.Json;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ColorFill.game.level
{
    public class Level : MonoBehaviour
    {
        private Stage[] _stages = new Stage[2];
        private GameObjectManager _gameObjectManager;
        [SerializeField] private GameObject[] _stageContainers;

        private Stage _firstStage => _stages[0];

        private Stage _secondStage => _stages[1];
        private Stage _activeStage => _stages[activeStageIndex];

        private int activeStageIndex;
        public static Level Instance { get; private set; }

        private int _currentLevel;
        private void Awake()
        {
            _gameObjectManager = GameObjectManager.Instance;
            Instance = this;
        }

        


        public void LoadLevel(int levelNum)
        {
            activeStageIndex = 0;
            _currentLevel = levelNum;
            ResetLevel();
            LoadStage(levelNum, 0);
            InstantiateObjects(0);
            AdjustCameraFirstStage();
            InstantiatePlayer();
        }

        void ResetLevel()
        {
            foreach (var stage in _stages)
            {
                if (stage != null)
                {
                    stage.ResetStage();    
                }
            }
        }

        void LoadStage(int levelNum,int stageNum)
        {
            lastGemCount = GameContext.Instance.GetGemCount();
            Stage stage;
            if (stageNum == 0)
            {
                stage = new Stage(ProceedNextStage);
            }
            else
            {
                stage = new Stage(ProceedNextLevel);
            }
            _stages[stageNum] = stage;
            stage.InstantiateStageData($"level/level_{levelNum}_{stageNum + 1}",_stageContainers[stageNum]);
        }

        void InstantiateObjects(int stageNum)
        {
            _stages[stageNum].InstantiateObjects();
        }
        
        void AdjustCameraFirstStage()
        {
            GameCamera.Instance.AdjustStage1();
        }

        void InstantiatePlayer()
        {
            var activeStageContainer = _stages[activeStageIndex]._stageContainer;
            var playerPosition = new Vector3(0, activeStageContainer.transform.position.y, 0);
            if (Player.Instance != null)
            {
                Player.Instance.transform.position = playerPosition;
            }
            else
            {
                _gameObjectManager.GetObject(GameObjectType.Player, playerPosition ).GetComponent<Player>();
            }
            
            _activeStage.SetPlayerChild();
            Player.Instance.GetComponent<Player>().InitializeData();
        }

        
        public GameObjectType PlayerAt(int x,int y,PlayerStatus playerStatus)
        {
            return _stages[activeStageIndex].PlayerAt(x, y, playerStatus);
        }

        void ProceedNextStage()
        {
            Player.Instance.ProceedToNextStage(() =>
            {
                InstantiateObjects(1);
                _activeStage.SetPlayerChild();
            });
            activeStageIndex = 1;
            LoadStage(_currentLevel,1);
            
        }

        void ProceedNextLevel()
        {
            _gameObjectManager.FullFillCount = 0;
            Util.ThreadStart(() =>
            {
                Thread.Sleep(1000);
                MainContext.Instance.Invoke(() =>
                {
                    GameContext.Instance.IterateLevel();
                });
            });
        }

        public void PlayerAtProceedingStage(int x, int y)
        {
            var fullFill = _gameObjectManager.GetObject(GameObjectType.FullFill,_firstStage._stageContainer.transform);
            fullFill.transform.localPosition = new Vector3(x, y, 0);
        }

        private int lastGemCount;

        public void RestartStage()
        {
            GameContext.Instance.SetGem(lastGemCount);
            _activeStage.ResetStage();
            _activeStage.InstantiateObjects();
            InstantiatePlayer();
            GameContext.Instance.SetStageRatio(activeStageIndex,0);
        }

        public void Revive()
        {
            _activeStage.Revive();
        }
    }
    
    
    
}