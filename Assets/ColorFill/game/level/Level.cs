using System;
using System.Collections.Generic;
using System.Diagnostics;
using ColorFill.game.camera;
using ColorFill.game.elements;
using ColorFill.game.elements.mover.vertical_mover;
using ColorFill.game.elements.wall;
using ColorFill.helper;
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
        private Matrix<LevelMatrixItem>[] _stageMatrices = new Matrix<LevelMatrixItem>[2];
        private LevelJsonModel[] stageModels = new LevelJsonModel[2];
        private GameObjectManager _gameObjectManager;
        [SerializeField] private GameObject firstStageContainer;
        [SerializeField] private GameObject secondStageContainer;
        private GameObject activeStageContainer;

        
        /// <summary>
        /// keep record of stationary objects
        /// </summary>
        private Matrix<LevelMatrixItem> _liveMatrix;
        public static Level Instance { get; private set; }

        private void Awake()
        {
            _gameObjectManager = GameObjectManager.Instance;
            Instance = this;
        }

        void InstantiateLiveMatrixFirstStage()
        {
            _liveMatrix = new Matrix<LevelMatrixItem>();
            _liveMatrix.SetDimensions(_stageMatrices[0]._width,_stageMatrices[0]._height);
        }


        public void LoadLevel(int levelNum)
        {
            LoadStage(levelNum, 1);
            LoadStage(levelNum, 2);
            InstantiateObjects();
            AdjustCameraFirstStage();
        }

        void LoadStage(int levelNum,int stage)
        {
            var levelName = $"level_{levelNum}_{stage}";
            var textAsset = Resources.Load(levelName) as TextAsset;
            var text = textAsset.text;
            var model = JsonConvert.DeserializeObject<LevelJsonModel>(text);
            var matrix = new Matrix<LevelMatrixItem>();
            matrix.SetDimensions(model.width,model.height);
            var data = model.layers[0].data;
            //start is top left, array first scans first row then goes to second row and so on
            for (int i = 0; i < data.Length; i++)
            {
                var y = Math.DivRem(i, model.width, out var x);
                y = (model.height - 1) - y;
                var gameObjectType = TiledObjectId.ConvertToGameId(data[i]);
                matrix.SetItem(x, y, new LevelMatrixItem(gameObjectType));
            }

            _stageMatrices[stage - 1] = matrix;
            stageModels[stage - 1] = model;
        }

        void InstantiateObjects()
        {
            InstantiateLiveMatrixFirstStage();
            InstantiateFirstStage();
        }

        void InstantiateFirstStage()
        {
            activeStageContainer = firstStageContainer;
            var _firstStageMatrix = _stageMatrices[0];
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
            
            //instantiate level objects
            for (int x = 0; x < _firstStageMatrix._width; x++)
            {
                for (int y = 0; y < _firstStageMatrix._height; y++)
                {
                    var levelItem = _firstStageMatrix.GetItem(x, y);
                    var gameObjectType = levelItem.type;
                    if (gameObjectType == GameObjectType.Void)
                    {
                        _liveMatrix.SetItem(x,y,new LevelMatrixItem(GameObjectType.Void));
                        continue;
                    }
                    var gameObj = _gameObjectManager.GetObject(gameObjectType,firstStageContainer.transform);
                    gameObj.transform.localPosition = new Vector3(x, y, 0);
                    switch (gameObjectType)
                    {
                        case GameObjectType.VerticalMover:
                            gameObj.GetComponent<VerticalMover>().SetMoveAmount(verticalMoveAmount);
                            _liveMatrix.SetItem(x,y,new LevelMatrixItem(GameObjectType.Void));
                            break;
                        case GameObjectType.Wall:
                            _liveMatrix.SetItem(x,y,new LevelMatrixItem(gameObjectType));
                            break;
                        case GameObjectType.Gem:
                            _liveMatrix.SetItem(x,y,new LevelMatrixItem(gameObjectType));
                            break;
                    }
                    
                }
            }
        }
        
        void AdjustCameraFirstStage()
        {
            GameCamera.Instance.AdjustStage1();
        }


        private List<Point> _halfFills = new List<Point>();
        public void PlayerAt(int x,int y,PlayerStatus playerStatus)
        {
            x = Mathf.Clamp(x, 0, _liveMatrix._width - 1);
            y = Mathf.Clamp(y, 0, _liveMatrix._height - 1);
            if (x == y)
            {
                
            }
            var item = _liveMatrix.GetItem(x, y);
            var objType = item.type;
            switch (playerStatus)
            {
                case PlayerStatus.Moving:
                    if (objType == GameObjectType.Void)
                    {
                        var halfFill = _gameObjectManager.GetObject(GameObjectType.HalfFill, firstStageContainer.transform);
                        halfFill.transform.localPosition = new Vector3(x, y, 0);
                        _liveMatrix.SetItem(x,y,new LevelMatrixItem(GameObjectType.HalfFill));
                        _halfFills.Add(new Point(x,y));
                        if (x == y)
                        {
                            Debug.Log($"Level Added {x},{y}");
                        }
                    }

                    break;
                case PlayerStatus.Stopped:
                    _halfFills.Add(new Point(x,y));
                    //get region of half fills and instantiate full fills
                    //full fills will destroy halffills
                    foreach (var position in _halfFills)
                    {
                        if (_liveMatrix.GetItem(position.x, position.y).type == GameObjectType.FullFill)
                        {
                            continue;
                        }
                        _liveMatrix.SetItem(position.x,position.y,new LevelMatrixItem(GameObjectType.FullFill));
                        var fullFill = _gameObjectManager.GetObject(GameObjectType.FullFill,
                            activeStageContainer.transform);
                        if (position.x == position.y)
                        {
                            Debug.Log($"Created {position.x},{position.y}");
                        }
                        fullFill.transform.localPosition = new Vector3(position.x, position.y, 0);
                    }
                    _halfFills.Clear();
                    
                    // var neighbours = _liveMatrix.GetPlusShape(x, y);
                    // foreach (var neighbour in neighbours)
                    // {
                    //     if (neighbour != null)
                    //     {
                    //         if (neighbour.type == GameObjectType.HalfFill)
                    //         {
                    //             
                    //         }
                    //     }
                    // }

                    break;
            }
        }
        
    }
    
    
    
}