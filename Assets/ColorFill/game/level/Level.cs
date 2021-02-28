using System;
using System.Collections.Generic;
using System.Diagnostics;
using ColorFill.game.camera;
using ColorFill.game.elements;
using ColorFill.game.elements.mover.vertical_mover;
using ColorFill.game.elements.wall;
using ColorFill.helper;
using ColorFill.helper.context;
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
        [SerializeField] private GameObject[] stageContainers;

        private GameObject firstStageContainer
        {
            get
            {
                return stageContainers[0];
            }
        }
        private GameObject secondStageContainer
        {
            get
            {
                return stageContainers[1];
            }
        }
        
        private int activeStageIndex;

        private GameObject activeStageContainer
        {
            get
            {
                return stageContainers[activeStageIndex];
            }
        }

        
        /// <summary>
        /// keep record of stationary objects
        /// </summary>
        private Matrix<LevelMatrixItem> _liveMatrix;
        public static Level Instance { get; private set; }

        private int[] TotalVoidCounts = new int [2]{11 * 11,17 * 25};

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
            ResetLevel();
            LoadStage(levelNum, 1);
            LoadStage(levelNum, 2);
            InstantiateObjects();
            AdjustCameraFirstStage();
            InstantiatePlayer();
        }

        void ResetLevel()
        {
            if (activeStageContainer == null)
            {
                return;
            }
            foreach (Transform trans in activeStageContainer.transform)
            {
                if (trans.name != "Static" && trans.name != "Level" && trans.name != "Floor")
                {
                    _gameObjectManager.DestroyObject(trans.gameObject);    
                }
            }
            _halfFills.Clear();
            _neighbourVoid.Clear();
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
            activeStageIndex = 0;
            var firstStageMatrix = _stageMatrices[0];
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
            for (int x = 0; x < firstStageMatrix._width; x++)
            {
                for (int y = 0; y < firstStageMatrix._height; y++)
                {
                    var levelItem = firstStageMatrix.GetItem(x, y);
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
                            TotalVoidCounts[activeStageIndex]--;
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

        void InstantiatePlayer()
        {
            var player = _gameObjectManager.GetObject(GameObjectType.Player, new Vector3(0,activeStageContainer.layer,0));
            player.transform.SetParent(activeStageContainer.transform);
            player.GetComponent<Player>().InitializeData();
        }

        //half fill trail list
        private List<Point> _halfFills = new List<Point>();
        //void point that are neighbouring to trail
        private List<Point> _neighbourVoid = new List<Point>();
        private Point _lastPosition;
        public void PlayerAt(int x,int y,PlayerStatus playerStatus)
        {
            x = Mathf.Clamp(x, 0, _liveMatrix._width - 1);
            y = Mathf.Clamp(y, 0, _liveMatrix._height - 1);
            var item = _liveMatrix.GetItem(x, y);
            var objType = item.type;
            List<HashSet<Point>> voidRegions = new List<HashSet<Point>>();
            switch (playerStatus)
            {
                case PlayerStatus.Moving:
                    if (objType == GameObjectType.Void || objType == GameObjectType.Gem)
                    {
                        var halfFill = _gameObjectManager.GetObject(GameObjectType.HalfFill, firstStageContainer.transform);
                        halfFill.transform.localPosition = new Vector3(x, y, 0);
                        _liveMatrix.SetItem(x,y,new LevelMatrixItem(GameObjectType.HalfFill));
                        _halfFills.Add(new Point(x,y));
                        var neighbours = _liveMatrix.GetPlusShape(x, y);
                        foreach (var neighbour in neighbours)
                        {
                            if (neighbour == null)
                            {
                                continue;
                            }
                            if (neighbour.type == GameObjectType.Void || neighbour.type == GameObjectType.Gem)
                            {
                                _neighbourVoid.Add(new Point(neighbour.x,neighbour.y));
                            }
                        }
                    }
                    else if (objType == GameObjectType.FullFill)
                    {
                        var lastItem = _liveMatrix.GetItem(_lastPosition.x, _lastPosition.y);
                        if (lastItem.type == GameObjectType.FullFill)
                        {
                            break;
                        }

                        foreach (var neighbourVoidCell in _neighbourVoid)
                        {
                            var neighbourItem = _liveMatrix.GetItem(neighbourVoidCell);
                            if (neighbourItem.type != GameObjectType.Void && neighbourItem.type != GameObjectType.Gem)
                            {
                                continue;
                            }
                            bool shouldAdd = true;
                            foreach (var region in voidRegions)
                            {
                                if (region.Contains(neighbourVoidCell))
                                {
                                    shouldAdd = false;
                                    break;
                                }
                            }

                            if (shouldAdd)
                            {
                                voidRegions.Add(_liveMatrix.GetSimilarRegion(neighbourVoidCell.x,neighbourVoidCell.y));  
                                _liveMatrix.ResetIsConsidered();
                            }
                        }
                        voidRegions.AddRange(GetNeighbouringVoidRegions(x, y));
                        //reverse sorted
                        voidRegions.Sort((o1,o2) => o2.Count.CompareTo(o1.Count));
                        
                        if (voidRegions.Count > 0)
                        {
                            //if there is only one region and it is circled with half fil, then fill it
                            if (voidRegions.Count == 1)
                            {
                                FillVoidRegion(voidRegions[0]);
                            }
                            else
                            {
                                //fill all the regions but the maximum
                                for (int i = 1; i < voidRegions.Count; i++)
                                {
                                    FillVoidRegion(voidRegions[i]);
                                }
                            }
                            
                        }
                        _neighbourVoid.Clear();
                        FillHalfFills();
                    }
                    
                    break;
                case PlayerStatus.Stopped:
                    _halfFills.Add(new Point(x,y));
                    FillHalfFills();


                    voidRegions = GetNeighbouringVoidRegions(x, y);

                    if (voidRegions.Count > 1)
                    {
                        var region1 = voidRegions[0];
                        var region2 = voidRegions[1];
                        if (!Util.HaveInstersection(region1, region2))
                        {
                            HashSet<Point> toBeFilled;
                            if (region1.Count < region2.Count)
                            {
                                toBeFilled = region1;
                            }
                            else
                            {
                                toBeFilled = region2;
                            }
                            FillVoidRegion(toBeFilled);
                        }
                    }
                    _neighbourVoid.Clear();
                    break;
            }

            _lastPosition = new Point(x, y);
        }

        void FillHalfFills()
        {
            //get region of half fills and instantiate full fills
            //full fills will destroy halffills
            foreach (var position in _halfFills)
            {
                if (_liveMatrix.GetItem(position.x, position.y).type == GameObjectType.FullFill)
                {
                    continue;
                }

                CreateItem(position.x, position.y, GameObjectType.FullFill);
            }
            _halfFills.Clear();
        }

        List<HashSet<Point>> GetNeighbouringVoidRegions(int x,int y)
        {
            var neighbours = _liveMatrix.GetPlusShape(x, y);
            List<HashSet<Point>> voidRegions = new List<HashSet<Point>>();
            foreach (var neighbour in neighbours)
            {
                if (neighbour != null)
                {
                    bool shouldAdd = true;
                    foreach (var region in voidRegions)
                    {
                        if (region.Contains(new Point(neighbour.x,neighbour.y)))
                        {
                            shouldAdd = false;
                            break;
                        }
                    }

                    if (shouldAdd)
                    {
                        if (neighbour.type == GameObjectType.Void || neighbour.type == GameObjectType.Gem)
                        {
                            var region = _liveMatrix.GetSimilarRegion(neighbour.x, neighbour.y);
                            voidRegions.Add(region);
                            _liveMatrix.ResetIsConsidered();
                        }
                    }
                }
            }

            return voidRegions;
        }

        void CreateItem(int x,int y,GameObjectType type)
        {
            _liveMatrix.SetItem(x,y,new LevelMatrixItem(type));
            var item = _gameObjectManager.GetObject(type,activeStageContainer.transform);
            item.transform.localPosition = new Vector3(x, y, 0);
            GameContext.Instance.SetStageRatio(activeStageIndex,_gameObjectManager.FullFillCount / (float)TotalVoidCounts[activeStageIndex]);    
            
        }

        void FillVoidRegion(HashSet<Point> region)
        {
            foreach (var point in region)
            {
                if (_liveMatrix.GetItem(point.x, point.y).type != GameObjectType.FullFill)
                {
                    CreateItem(point.x,point.y,GameObjectType.FullFill);    
                }
            }
        }
        
    }
    
    
    
}