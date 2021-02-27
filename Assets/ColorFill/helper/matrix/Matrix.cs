using System;
using System.Collections.Generic;
using ColorFill.game.elements;
using ColorFill.game.level;
using ColorFill.helper.level;
using UnityEngine;

namespace ColorFill.helper.matrix
{
    /// <summary>
    /// matrix whose origin is at bottom left. x direction is horizontal y direction is vertical
    /// </summary>
    public class Matrix<T> where T : MatrixItem
    {
        public int _width { get; private set; }
        public int _height { get; private set; }
        private T[] _items;
        
        public void SetDimensions(int width, int height)
        {
            _width = width;
            _height = height;
            _items = new T[width * height];
        }

        public T GetItem(int x, int y)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height)
            {
                return default;
            }
            return _items[x + y * _width];
        }

        public void SetItem(int x, int y, T item)
        {
            if (x == y)
            {
                
            }
            item.x = x;
            item.y = y;
            _items[x + y * _width] = item;
        }
        
        public List<Point> GetSimilarRegion(int x, int y)
        {
            var currentCell = GetItem(x, y);
            currentCell.isConsidered = true;
            List<Point> cells = new List<Point>();
            cells.Add(new Point(x,y));
            //add right cell if it suitable
            if (x != _width - 1)
            {
                var rightCell = GetItem(x + 1, y);
                if (!rightCell.isConsidered && rightCell.Equals(currentCell))
                {
                    foreach (var point in GetSimilarRegion(x + 1, y))
                    {
                        cells.Add(point);
                    }
                }
            }
            //left cell
            if (x != 0)
            {
                var leftCell = GetItem(x - 1, y);
                if (!leftCell.isConsidered && leftCell.Equals(currentCell))
                {
                    foreach (var point in GetSimilarRegion(x - 1, y))
                    {
                        cells.Add(point);
                    }
                }
            }
            //top cell
            if (y != _height - 1)
            {
                var topCell = GetItem(x, y + 1);
                if (!topCell.isConsidered && topCell.Equals(currentCell))
                {
                    foreach (var point in GetSimilarRegion(x, y + 1))
                    {
                        cells.Add(point);
                    }
                }
            }
            //down cell
            if (y != 0)
            {
                var downCell = GetItem(x, y - 1);
                if (!downCell.isConsidered && downCell.Equals(currentCell))
                {
                    foreach (var point in GetSimilarRegion(x, y - 1))
                    {
                        cells.Add(point);
                    }
                }
            }

            return cells;
        }

        public void ResetIsConsidered()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    GetItem(x, y).isConsidered = false;
                }
            }
        }
        
        public T[] GetPlusShape(int x, int y)
        {
            var leftCell = GetItem(x - 1, y);
            var upperCell = GetItem(x, y + 1);
            var rightCell = GetItem(x + 1, y);
            var downCell = GetItem(x, y - 1);
            return new []{leftCell, upperCell, rightCell, downCell};
        }

    }
}