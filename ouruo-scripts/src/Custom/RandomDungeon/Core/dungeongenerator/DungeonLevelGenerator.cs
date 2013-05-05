using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace iRogue
{
    public abstract class DungeonLevelGenerator
    {
        public DungeonLevelGenerator(Map map)
        {
            TargetMap = map;
            Rand = map.rand;
        }

        abstract public void Generate();
        abstract public void Render(Graphics gr, float mouseX, float mouseY);

        protected Random Rand { get; set; }
        protected Map TargetMap { get; set; }
    }
}
