using System;
using ColorFill.game.level;

namespace ColorFill.game.elements
{
    public enum GameObjectType
    {
        Player,
        VerticalMover,
        HorizontalMover,
        Gem,
        Wall
    }

    public static class LevelElementConvert
    {
        public static GameObjectType Convert(LevelElementType type)
        {
            switch (type)
            {
                case LevelElementType.Gem:
                    return GameObjectType.Gem;
                case LevelElementType.Player:
                    return GameObjectType.Player;
                case LevelElementType.Wall:
                    return GameObjectType.Wall;
                case LevelElementType.HorizontalMover:
                    return GameObjectType.HorizontalMover;
                case LevelElementType.VerticalMover:
                    return GameObjectType.VerticalMover;
                default:
                    throw new Exception("Can't convert level element type to game object type");
            }
        }
    }
}