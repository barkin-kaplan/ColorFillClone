using System;
using System.Collections.Generic;
using System.Linq;
using ColorFill.game.level;
using ColorFill.helper.context;
using ColorFill.helper.geometry;
using ColorFill.helper.input_helper;
using UnityEngine;

namespace ColorFill.game.elements
{
    public class Player : MonoBehaviour
    {
        #region turn related data structures
        private Queue<TurnQueueItem> _turnQueue = new Queue<TurnQueueItem>();
        private Level _level;

        private struct TurnQueueItem
        {
            public Vector2 direction;
            public Vector3 turnPosition;
            /// <summary>
            /// difference vector at the frame this struct is initialized. if object passes unaware of threshold because of low framerate check unit dot product
            /// note that this vector is multiplied by -1 if turn position already passed at the frame of turn call
            /// </summary>
            public Vector3 origDiffVector;
        }
        private Vector3 _nextTurnPosition;
        #endregion
        
        private Swipeable _swipeable;
        private Vector2 _currentMoveDirection = Vector2.zero;
        private Vector3 _velocity = Vector3.zero;
        private bool isInsideFill;
        private const float turnThreshold = 0.07f;

        

        
        

        void Awake()
        {
            _swipeable = GetComponent<Swipeable>();
            SetSwipeEvent();
            _level = Level.Instance;
        }

        void SetSwipeEvent()
        {
            _swipeable.SetOnSwipe(args =>
            {
                Turn(args.Direction);
            });
        }

        void Turn(Vector2 direction)
        {
            if (_turnQueue.Count > 0)
            {
                //check if last turn and this turn is opposite
                if ((_turnQueue.Last().direction + direction).Approximately(Vector2.zero) && !isInsideFill)
                {
                    return;
                }
            }
            // if turn queue is empty check current direction
            //can't go opposite way
            if ((_currentMoveDirection + direction).Approximately(Vector2.zero) && ! isInsideFill)
            {
                return;
            }

            var nextTurnPosition = GetNextTurnPosition(out var diffVector); 
            _turnQueue.Enqueue(new TurnQueueItem()
            {
                turnPosition = nextTurnPosition,
                direction = direction,
                origDiffVector = diffVector
            });
            
            
        }

        Vector3 GetNextTurnPosition(out Vector3 diffVector)
        {
            var position = transform.position;
            var nextPosition = position + 8 * (Vector3)_currentMoveDirection / 10;
            var nextPositionInt = new Vector3((int) nextPosition.x, (int) nextPosition.y, nextPosition.z);
            diffVector = nextPositionInt - position;
            if(position.CompareIntegerEqual(nextPositionInt) && !position.Approximately(nextPositionInt))
            {
                diffVector *= -1;
            }
            return nextPositionInt;
        }

        void Update()
        {
            transform.position += _velocity * Time.deltaTime;
            if (CheckIfEligibleForTurn())
            {
                TurnUpdate();
            }

            if (CheckMoveNewCell())
            {
                SetLastCell();
            }
        }

        void TurnUpdate()
        {
            var nextTurn = _turnQueue.Dequeue();
            transform.position = nextTurn.turnPosition;
            _currentMoveDirection = nextTurn.direction;
            _velocity = _currentMoveDirection * 6f;
        }

        bool CheckIfEligibleForTurn()
        {
            if (_turnQueue.Count > 0)
            {
                var firstItem = _turnQueue.First();
                var diff = firstItem.turnPosition - transform.position;
                //if object is near the turn position or passed the turn position unaware
                return (diff).magnitude < turnThreshold ||
                       Mathf.Approximately(firstItem.origDiffVector.DotUnit(diff), -1f);
            }

            return false;
        }

        private Vector3 _lastCell = new Vector3(-1,-1,-1);
        private Vector3 halfUnit = new Vector3(0.5f, 0.5f);
        bool CheckMoveNewCell()
        {
            //coordanites are from 0,0 to 10,10 in case of localPosition in firststage
            //0,0 to 16,24 in second stage
            var position = transform.localPosition + halfUnit;
            return !position.CompareIntegerEqual(_lastCell);
        }

        private void OnTriggerEnter(Collider other)
        {
            var otherObj = other.gameObject;
            if (otherObj.CompareTag("Wall"))
            {
                ResetVelocity();
                transform.localPosition = _lastCell;
                _level.PlayerAt((int)_lastCell.x,(int)_lastCell.y,PlayerStatus.Stopped);
            }
            else if (otherObj.CompareTag("Deadly") )
            {
                Destroy(gameObject);
                GameContext.Instance.ShowGameOver();
            }
        }

        void SetLastCell()
        {
            _lastCell = new Vector3((int) (transform.localPosition.x + 0.5f), (int) (transform.localPosition.y + 0.5f), 0f);
            _level.PlayerAt((int)_lastCell.x,(int)_lastCell.y,PlayerStatus.Moving);
        }

        void ResetVelocity()
        {
            _currentMoveDirection = Vector3.zero;
            _velocity = Vector3.zero;
        }

        public void InitializeData()
        {
            SetLastCell();
        }
    }
}