using ColorFill.helper.level;
using ColorFill.helper.matrix;

namespace ColorFill.game.level
{
    public class Stage
    {
        private Matrix _matrix;
        public Stage(LevelJsonModel model)
        {
            _matrix = new Matrix(model);
        }

        public void Update()
        {
            
        }

        public void InstantiateObjects()
        {
            
        }
    }
}