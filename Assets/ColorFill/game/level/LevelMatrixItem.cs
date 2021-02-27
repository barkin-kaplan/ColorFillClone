using System;
using ColorFill.game.elements;
using ColorFill.helper.matrix;

namespace ColorFill.game.level
{
    public class LevelMatrixItem : MatrixItem, IEquatable<LevelMatrixItem>
    {
        public GameObjectType type;

        public LevelMatrixItem(GameObjectType type)
        {
            this.type = type;
        }

        public bool Equals(LevelMatrixItem other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (type == GameObjectType.Void && other.type == GameObjectType.Gem)
            {
                return true;
            }
            if (type == GameObjectType.Gem && other.type == GameObjectType.Void)
            {
                return true;
            }
            return type == other.type;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LevelMatrixItem) obj);
        }

        public override int GetHashCode()
        {
            return type.GetHashCode();
        }
    }
}