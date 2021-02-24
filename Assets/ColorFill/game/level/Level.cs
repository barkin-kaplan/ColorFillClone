using System;
using ColorFill.game.elements;
using ColorFill.game.elements.mover.vertical_mover;
using ColorFill.helper;
using ColorFill.helper.level;
using ColorFill.helper.matrix;
using ColorFill.helper.object_manager;
using Newtonsoft.Json;
using UnityEngine;

namespace ColorFill.game.level
{
    public class Level : MonoBehaviour
    {
        private Matrix<LevelElementType>[] _stageMatrices = new Matrix<LevelElementType>[2];
        private LevelJsonModel[] stageModels = new LevelJsonModel[2];
        private GameObjectManager _gameObjectManager;
        public static Level Instance { get; private set; }

        private void Awake()
        {
            _gameObjectManager = GameObjectManager.Instance;
            Instance = this;
        }


        public void LoadLevel(int levelNum)
        {
            LoadStage(levelNum, 1);
            LoadStage(levelNum, 2);
            InstantiateObjects();
        }

        void LoadStage(int levelNum,int stage)
        {
            var levelName = $"level_{levelNum}_{stage}";
            var textAsset = Resources.Load(levelName) as TextAsset;
            var text = textAsset.text;
            var model = JsonConvert.DeserializeObject<LevelJsonModel>(text);
            var matrix = new Matrix<LevelElementType>();
            matrix.SetDimensions(model.width,model.height);
            var data = model.layers[0].data;
            //start is top left, array first scans first row then goes to second row and so on
            for (int i = 0; i < data.Length; i++)
            {
                var y = Math.DivRem(i, model.width, out var x);
                y = (model.height - 1) - y;
                var gameObjectType = TiledObjectId.ConvertToGameId(data[i]);
                matrix.SetItem(x, y, gameObjectType);
            }

            _stageMatrices[stage - 1] = matrix;
            stageModels[stage - 1] = model;
        }

        void InstantiateObjects()
        {
            InstantiateFirstStage();
        }

        void InstantiateFirstStage()
        {
            var _firstStageMatrix = _stageMatrices[0];
            var xOffset = -_firstStageMatrix._width / 2;
            var jsonModel = stageModels[0];
            var properties = jsonModel.layers[0].properties;
            float verticalMoveAmount = 0;
            foreach (var property in properties)
            {
                if (property.name == "vertical_move_amount")
                {
                    verticalMoveAmount = Util.ParseFloat(property.value);
                }
            }
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
                    if (gameObjectType == GameObjectType.VerticalMover)
                    {
                        gameObj.GetComponent<VerticalMover>().SetMoveAmount(verticalMoveAmount);
                    }
                    
                }
            }
        }
    }
}