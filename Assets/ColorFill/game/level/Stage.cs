using System;
using System.Collections.Generic;
using ColorFill.game.elements;
using ColorFill.game.elements.mover.horizontal_mover;
using ColorFill.game.elements.mover.vertical_mover;
using ColorFill.helper;
using ColorFill.helper.context;
using ColorFill.helper.DI;
using ColorFill.helper.level;
using ColorFill.helper.matrix;
using ColorFill.helper.object_manager;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ColorFill.game.level
{
    public class Stage
    {
        private Matrix<LevelMatrixItem> _liveMatrix;
        private Matrix<LevelMatrixItem> _levelMatrix;
        private LevelJsonModel _stageModel;
        private GameObjectManager _gameObjectManager;
        public GameObject _stageContainer { get; private set; }
        private int _totalVoidCount;
        private int _stageNum;
        private Action OnStageFinishCallback;
        private int _fullFillCount;
        private bool stageFinished;
        
        //half fill trail list
        private List<Point> _halfFills = new List<Point>();
        private Point _lastPosition;
        private List<ReviveItemMemory> _halfFillsCreatedSinceLastStop = new List<ReviveItemMemory>();
        //this position is a local position
        private Vector3 _lastStopPosition;

        public Stage(Action onStageFinishCallback)
        {
            OnStageFinishCallback = onStageFinishCallback;
            _gameObjectManager = GameObjectManager.Instance;
        }

        public void InstantiateStageData(string levelName,GameObject stageContainer)
        {
            //set container GameObject
            _stageContainer = stageContainer;
            
            //reset fullFillCount
            _fullFillCount = 0;
            //get stage num from file name
            _stageNum = Util.ParseInt(levelName[levelName.Length - 1]) - 1;
            //set maximum vaoid coun accordingly. void count will be decreased while instantiating objects
            _totalVoidCount = _stageNum == 0 ? 11 * 11 : 17 * 25;
            
            //load text asset
            var textAsset = Resources.Load(levelName) as TextAsset;
            var text = textAsset.text;
            //deserialize json
            _stageModel = JsonConvert.DeserializeObject<LevelJsonModel>(text);
            //initialize level matrix elements
            var matrix = new Matrix<LevelMatrixItem>();
            matrix.SetDimensions(_stageModel.width,_stageModel.height);
            var data = _stageModel.layers[0].data;
            //start is top left, array first scans first row then goes to second row and so on
            for (int i = 0; i < data.Length; i++)
            {
                var y = Math.DivRem(i, _stageModel.width, out var x);
                y = (_stageModel.height - 1) - y;
                var gameObjectType = TiledObjectId.ConvertToGameId(data[i]);
                matrix.SetItem(x, y, new LevelMatrixItem(gameObjectType));
            }

            _levelMatrix = matrix;
            _liveMatrix = new Matrix<LevelMatrixItem>();
            _lastStopPosition = new Vector3(_levelMatrix._width / 2, 0, 0);
        }
        
        public void InstantiateObjects()
        {
            
            _liveMatrix.SetDimensions(_levelMatrix._width,_levelMatrix._height);
            
            var properties = _stageModel.layers[0].properties;
            float verticalMoveAmount = 0;
            float horizontalMoveAmount = 0;
            //set move amounts
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
            for (int x = 0; x < _levelMatrix._width; x++)
            {
                for (int y = 0; y < _levelMatrix._height; y++)
                {
                    var levelItem = _levelMatrix.GetItem(x, y);
                    var gameObjectType = levelItem.type;
                    if (gameObjectType == GameObjectType.Void)
                    {
                        _liveMatrix.SetItem(x,y,new LevelMatrixItem(GameObjectType.Void));
                        continue;
                    }
                    var gameObj = _gameObjectManager.GetObject(gameObjectType,_stageContainer.transform);
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
                            _totalVoidCount--;
                            break;
                        case GameObjectType.Gem:
                            _liveMatrix.SetItem(x,y,new LevelMatrixItem(gameObjectType));
                            break;
                    }
                    
                }
            }
        }

        public void ResetStage()
        {
            foreach(Transform obj in _stageContainer.transform)
            {
                if (obj.name != "Static" && !obj.CompareTag("Player"))
                {
                    _gameObjectManager.DestroyObject(obj.gameObject);    
                }
            }
            _halfFills.Clear();
            //reset fullFillCount
            _fullFillCount = 0;
            _lastStopPosition = new Vector3(_levelMatrix._width / 2, 0, 0);
        }
        
        public void CreateItem(int x,int y,GameObjectType type)
        {
            
            if (type == GameObjectType.FullFill)
            {
                _fullFillCount++;
            }
            var item = _gameObjectManager.GetObject(type,_stageContainer.transform);
            //if type is HalfFill put it to item memory for destruction on revive
            if (type == GameObjectType.HalfFill)
            {
                //put half fills to memory
                var prevObjectType = _liveMatrix.GetItem(x, y).type;
                _halfFillsCreatedSinceLastStop.Add(new ReviveItemMemory()
                {
                    actualGameObject = item,
                    point = new Point(x,y),
                    prevGameObjectType = prevObjectType
                });    
            }
            
            _liveMatrix.SetItem(x,y,new LevelMatrixItem(type));
            item.transform.localPosition = new Vector3(x, y, 0);
            var stageRatio = _fullFillCount / (float) _totalVoidCount;
            
            GameContext.Instance.SetStageRatio(_stageNum,stageRatio);
            if (stageRatio >= 1f && !stageFinished)
            {
                stageFinished = true;
                OnStageFinishCallback();
            }
            
        }
        
        
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
                        CreateItem(x, y, GameObjectType.HalfFill);
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
                        Player.Instance.GameOver();
                    }
                    
                    break;
                case PlayerStatus.Stopped:
                    _halfFillsCreatedSinceLastStop.Clear();
                    _lastStopPosition = new Vector3(x, y, 0);
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
            if (voidRegion.Count < _totalVoidCount / (float) 6)
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

        public void SetPlayerChild()
        {
            Player.Instance.transform.SetParent(_stageContainer.transform);
        }

        public void Revive()
        {
            foreach (var reviveItemMemory in _halfFillsCreatedSinceLastStop)
            {
                var type = reviveItemMemory.prevGameObjectType;
                var point = reviveItemMemory.point;
                Object.Destroy(reviveItemMemory.actualGameObject);
                _liveMatrix.SetItem(point,new LevelMatrixItem(type));
                if (type == GameObjectType.Gem)
                {
                    CreateItem(point.x,point.y,type);
                }
            }
            
            _halfFills.Clear();

            _gameObjectManager.GetObject(GameObjectType.Player, _stageContainer.transform.position + _lastStopPosition);
            SetPlayerChild();
        }
    }
}