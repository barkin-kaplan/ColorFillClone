using ColorFill.game.elements;
using ColorFill.game.level;

namespace ColorFill.helper.matrix
{
    public class TiledObjectId
    {
        public static LevelElementType ConvertToGameId(int tiledId)
        {
            switch (tiledId)
            {
                case 1:
                    return LevelElementType.Wall;
                case 2:
                    return LevelElementType.VerticalMover;
                case 3:
                    return LevelElementType.HorizontalMover;
                case 4:
                    return LevelElementType.Gem;
                default:
                    return LevelElementType.Void;
            }
        }
    }
}