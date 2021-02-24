using System;
using ColorFill.game.elements;
using ColorFill.game.level;
using ColorFill.helper.level;
using UnityEngine;

namespace ColorFill.helper.matrix
{
    /// <summary>
    /// matrix whose origin is at bottom left. x direction is horizontal y direction is vertical
    /// </summary>
    public class Matrix<T>
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
            _items[x + y * _width] = item;
        }

    }
}