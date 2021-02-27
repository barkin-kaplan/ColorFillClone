using ColorFill.game.elements;
using ColorFill.game.level;

namespace ColorFill.helper.matrix
{
    public class TiledObjectId
    {
        public static GameObjectType ConvertToGameId(int tiledId)
        {
            switch (tiledId)
            {
                case 1:
                    return GameObjectType.Wall;
                case 2:
                    return GameObjectType.VerticalMover;
                case 3:
                    return GameObjectType.HorizontalMover;
                case 4:
                    return GameObjectType.Gem;
                default:
                    return GameObjectType.Void;
            }
        }
    }
}