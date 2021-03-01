﻿using System;
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

        private Player _player;

        private int _currentLevel;
        private void Awake()
        {
            _gameObjectManager = GameObjectManager.Instance;
            Instance = this;
        }

        


        public void LoadLevel(int levelNum)
        {
            _currentLevel = levelNum;
            ResetLevel();
            LoadStage(levelNum, 0);
            InstantiateObjects(0);
            AdjustCameraFirstStage();
            InstantiatePlayer();
        }

        void ResetLevel()
        {
            foreach (var stageContainer in stageContainers)
            {
                foreach (Transform trans in stageContainer.transform)
                {
                    if (trans.name != "Static" && trans.name != "Level" && trans.name != "Floor")
                    {
                        _gameObjectManager.DestroyObject(trans.gameObject);    
                    }
                }
            }
            _halfFills.Clear();
        }

        void LoadStage(int levelNum,int stage)
        {
            var levelName = $"level/level_{levelNum}_{stage + 1}";
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

            _stageMatrices[stage] = matrix;
            stageModels[stage] = model;
        }

        void InstantiateObjects(int stageNum)
        {
            InstantiateStage(stageNum);
        }

        void InstantiateStage(int stageNum)
        {
            _liveMatrix = new Matrix<LevelMatrixItem>();
            _liveMatrix.SetDimensions(_stageMatrices[stageNum]._width,_stageMatrices[stageNum]._height);
            activeStageIndex = stageNum;
            var tiledStageMatrix = _stageMatrices[stageNum];
            var jsonModel = stageModels[stageNum];
            var properties = jsonModel.layers[0].properties;
            float verticalMoveAmount = 0;
            float horizontalMoveAmount = 0;
            foreach (var property in properties)
            {
                if (property.name == "vertical_move_amount")
                {
                    verticalMoveAmount = Util.ParseFloat(property.value);
                }
                else if (property.name == "horizontal_move_amount")
                {
                    horizontalMoveAmount = Util.ParseFloat(property.value);
                }
            }
            
            //instantiate level objects
            for (int x = 0; x < tiledStageMatrix._width; x++)
            {
                for (int y = 0; y < tiledStageMatrix._height; y++)
                {
                    var levelItem = tiledStageMatrix.GetItem(x, y);
                    var gameObjectType = levelItem.type;
                    if (gameObjectType == GameObjectType.Void)
                    {
                        _liveMatrix.SetItem(x,y,new LevelMatrixItem(GameObjectType.Void));
                        continue;
                    }
                    var gameObj = _gameObjectManager.GetObject(gameObjectType,activeStageContainer.transform);
                    gameObj.transform.localPosition = new Vector3(x, y, 0);
                    switch (gameObjectType)
                    {
                        case GameObjectType.VerticalMover:
                            gameObj.GetComponent<VerticalMover>().SetMoveAmount(verticalMoveAmount);
                            _liveMatrix.SetItem(x,y,new LevelMatrixItem(GameObjectType.Void));
                            break;
                        case GameObjectType.HorizontalMover:
                            gameObj.GetComponent<HorizontalMover>().SetMoveAmount(horizontalMoveAmount);
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
            _player = _gameObjectManager.GetObject(GameObjectType.Player, new Vector3(0,activeStageContainer.transform.position.y,0)).GetComponent<Player>();
            _player.transform.SetParent(activeStageContainer.transform);
            _player.GetComponent<Player>().InitializeData();
        }

        //half fill trail list
        private List<Point> _halfFills = new List<Point>();
        private Point _lastPosition;
        /// <summary>
        /// update live matrix based on player's current cell
        /// returns objecttype at current cell
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="playerStatus"></param>
        /// <returns></returns>
        public GameObjectType PlayerAt(int x,int y,PlayerStatus playerStatus)
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
                        var halfFill = _gameObjectManager.GetObject(GameObjectType.HalfFill, activeStageContainer.transform);
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
                        }
                    }
                    else if (objType == GameObjectType.FullFill)
                    {
                        var lastItem = _liveMatrix.GetItem(_lastPosition.x, _lastPosition.y);
                        if (lastItem.type == GameObjectType.FullFill)
                        {
                            break;
                        }
                        FillHalfFills();
                        voidRegions = GetVoidRegions();
                        FillEligibleRegions(voidRegions);
                    }
                    else if (objType == GameObjectType.HalfFill)
                    {
                        Destroy(_player.gameObject);
                        GameContext.Instance.ShowGameOver();
                    }
                    
                    break;
                case PlayerStatus.Stopped:
                    _halfFills.Add(new Point(x,y));
                    FillHalfFills();

                    //get void regions around hit point when player hits a wall
                    voidRegions = GetVoidRegions();
                    FillEligibleRegions(voidRegions);
                    break;
            }

            _lastPosition = new Point(x, y);
            return _liveMatrix.GetItem(x, y).type;
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

        

        List<HashSet<Point>> GetVoidRegions()
        {
            List<HashSet<Point>> voidRegions = new List<HashSet<Point>>();
            for (int x = 0; x < _liveMatrix._width; x++)
            {
                for (int y = 0; y < _liveMatrix._height; y++)
                {
                    var item = _liveMatrix.GetItem(x, y);
                    var type = item.type;
                    if (type == GameObjectType.Void || type == GameObjectType.Gem)
                    {
                        bool shouldGetVoidRegion = true;
                        foreach (var region in voidRegions)
                        {
                            if (region.Contains(new Point(item.x, item.y)))
                            {
                                shouldGetVoidRegion = false;
                            }
                        }

                        if (shouldGetVoidRegion)
                        {
                            var voidRegion = _liveMatrix.GetSimilarRegion(item.x, item.y);
                            voidRegions.Add(voidRegion);
                        }
                    }
                }
            }

            return voidRegions;
        }

        void FillEligibleRegions(List<HashSet<Point>> voidRegions)
        {
            for (int i = voidRegions.Count - 1;i > -1 ;i--)
            {
                if (CheckIfRegionEligible(voidRegions[i]))
                {
                    FillVoidRegion(voidRegions[i]);
                    voidRegions.RemoveAt(i);
                }

            }
            //if some are not eligible and if there are multiple remaining void regions
            if (voidRegions.Count > 1)
            {
                //reverse sort regions
                voidRegions.Sort((o1,o2) => o2.Count.CompareTo(o1.Count));
                //fill the regions that do not have maximum area
                for(int i = 1;i < voidRegions.Count; i++)
                {
                    FillVoidRegion(voidRegions[i]);    
                }
            }
        }

        bool CheckIfRegionEligible(HashSet<Point> voidRegion)
        {
            //if region is small enough
            if (voidRegion.Count < TotalVoidCounts[activeStageIndex] / (float) 10)
            {
                return true;
            }
            //check if region is enclosed by half fills or full fills or cells are at the edge of level 
            foreach (var point in voidRegion)
            {
                var neighbours = _liveMatrix.GetPlusShape(point);
                foreach (var neighbour in neighbours)
                {
                    if (neighbour == null)
                    {
                        return false;
                    }
                    if(voidRegion.Contains(new Point(neighbour.x,neighbour.y)))
                    {
                        continue;
                    }
                    if (neighbour.type != GameObjectType.FullFill && neighbour.type != GameObjectType.HalfFill)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        void CreateItem(int x,int y,GameObjectType type)
        {
            _liveMatrix.SetItem(x,y,new LevelMatrixItem(type));
            var item = _gameObjectManager.GetObject(type,activeStageContainer.transform);
            item.transform.localPosition = new Vector3(x, y, 0);
            var stageRatio = _gameObjectManager.FullFillCount / (float) TotalVoidCounts[activeStageIndex];
            GameContext.Instance.SetStageRatio(activeStageIndex,stageRatio);
            if (stageRatio >= 1f)
            {
                if (activeStageIndex == 0)
                {
                    ProceedNextStage();    
                }
                else
                {
                    ProceedNextLevel();   
                }
            }
            
        }

        void ProceedNextStage()
        {
            _player.ProceedToNextStage(() =>
            {
                InstantiateObjects(1);
                _player.transform.SetParent(activeStageContainer.transform);
                _gameObjectManager.FullFillCount = 0;
            });
            LoadStage(_currentLevel,1);
            
        }

        void ResetTotalVoidCounts()
        {
            TotalVoidCounts = new []{11 * 11, 17 * 25};
        }

        void ProceedNextLevel()
        {
            ResetTotalVoidCounts();
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

        public void PlayerAtProceedingStage(int x, int y)
        {
            var fullFill = _gameObjectManager.GetObject(GameObjectType.FullFill,firstStageContainer.transform);
            fullFill.transform.localPosition = new Vector3(x, y, 0);
        }

        public void RestartStage()
        {
            ResetLevel();
            InstantiateObjects(activeStageIndex);
            InstantiatePlayer();
            GameContext.Instance.SetStageRatio(activeStageIndex,0);
        }
    }
    
    
    
}