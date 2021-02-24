using System;
using ColorFill.game.elements;
using ColorFill.helper.level;
using ColorFill.helper.matrix;
using ColorFill.helper.object_manager;
using Newtonsoft.Json;
using UnityEngine;

namespace ColorFill.game.level
{
    public class Level : MonoBehaviour
    {
        private Matrix _firstStageMatrix;
        private Matrix _secondStageMatrix;
        private GameObjectManager _gameObjectManager;
        public static Level Instance { get; private set; }

        private void Awake()
        {
            _gameObjectManager = GameObjectManager.Instance;
            Instance = this;
        }


        public void LoadLevel(int levelNum)
        {
            var firstStage = LoadStage(levelNum, 1);
            var secondStage = LoadStage(levelNum, 1);
            _firstStageMatrix = new Matrix(firstStage);
            _secondStageMatrix = new Matrix(secondStage);
            InstantiateObjects();
        }

        LevelJsonModel LoadStage(int levelNum,int stage)
        {
            var levelName = $"level_{levelNum}_{stage}";
            var textAsset = Resources.Load(levelName) as TextAsset;
            var text =textAsset.text;
            var model = JsonConvert.DeserializeObject<LevelJsonModel>(text);
            return model;
        }

        void InstantiateObjects()
        {
            InstantiateFirstStage();
        }

        void InstantiateFirstStage()
        {
            var xOffset = -_firstStageMatrix._width / 2;
            for (int i = 0; i < _firstStageMatrix._width; i++)
            {
                for (int j = 0; j < _firstStageMatrix._height; j++)
                {
                    var levelElementType = _firstStageMatrix.GetItem(i, j);
                    if (levelElementType == LevelElementType.Void)
                    {
                        continue;
                    }
                    var gameObjectType = LevelElementConvert.Convert(levelElementType); 
                    var gameObj = _gameObjectManager.GetObject(gameObjectType);
                    gameObj.transform.position = new Vector3(xOffset + i, j, 0);
                }
            }
        }
    }
}