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
    public class Matrix
    {
        public int _width { get; private set; }
        public int _height { get; private set; }
        private LevelElementType[] _items;
        
        public void SetDimensions(int width, int height)
        {
            _width = width;
            _height = height;
            _items = new LevelElementType[width * height];
        }

        public LevelElementType GetItem(int x, int y)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height)
            {
                return default;
            }
            return _items[x + y * _width];
        }

        public void SetItem(int x, int y, LevelElementType item)
        {
            _items[x + y * _width] = item;
        }

        public Matrix(LevelJsonModel model)
        {
            _width = model.width;
            _height = model.height;
            _items = new LevelElementType[_width * _height];
            var data = model.layers[0].data;
            //start is top left, array first scans first row then goes to second row and so on
            for (int i = 0; i < data.Length; i++)
            {
                var y = Math.DivRem(i, _width, out var x);
                y = (_height - 1) - y;
                var gameObjectType = TiledObjectId.ConvertToGameId(data[i]);
                SetItem(x, y, gameObjectType);
            }
        }
    }
}