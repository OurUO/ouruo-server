using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace iRogue
{
    public class Cell
    {
        public Cell()
        {
            RenderColor = Color.Pink;
            ID = -1;
        }
        public void Render(Graphics gr, int x, int y)
        {
            gr.FillRectangle(new SolidBrush(RenderColor), 10 + x * 8, 10 + y * 8,8,8);
        }

        public Color RenderColor { get; set; }
        public CellType Type { get; set; }
        public int ID { get; set; }
//        public VisibilityType Visibility { get; set; }
  //      public List<GameObject> Contents { get; set; }

    }

    public class Cell_Granite : Cell
    {
        public Cell_Granite()
        {
            RenderColor = Color.FromArgb(192, 0, 0, 0);
            ID = 0;
        }
    }

    public class Cell_Wall : Cell
    {
        public Cell_Wall()
        {
            RenderColor = Color.FromArgb(192, 0x80, 0x80, 0x80);
            ID = 1;
        }
    }
    public class Cell_Floor : Cell
    {
        public Cell_Floor()
        {
            RenderColor = Color.FromArgb(192, 0xc0, 0xc0, 0xc0);
            ID = 2;
        }
    }
    public class Cell_Door : Cell
    {
        public Cell_Door()
        {
            RenderColor = Color.Brown;
            ID = 3;
        }
    }

    public class CellTypeGroup
    {
        // There are multiple types of "doors" that each have different Ids, Names, Descriptions, and renderings,
        // yet they are all functionally the same (can be opened/closed/locked/etc)
    }

    public class CellType
    {
        // Unique identifier for this CellType.  We forgo inheritance for CellTypes for performance 
        // reasons (and lack-of-extensibility needs), and hardcode specific cell type handling (e.g.
        // door opening/closing) into the code behind
        public int Id { get; set; }

        // Name and Description - used in info display(s)
        public string Name { get; set; }
        public string Description { get; set; }

        // Each cell can have state information specific to itself; e.g. a Door can be "open" or "closed."  The state
        // can impact rendering (draw open or closed door) as well as functionality (ability to close/open, ability to walkthrough).
        // For now, each cell type gets 
        public int TypeSpecificStateInfo { get; set; }
        public CellTypeRenderInfo RenderInfo { get; set; }

        //public bool CanPlayerEnterCell();
    }

    public class CellTypeRenderInfo
    {
        // List of Rendering options for a specific CellType.
        // CellTypes can have multiple states (eg Doors are Open or Closed), and
        // CellTypes can also have multiple rendering options (eg a collection of tiles to choose
        // from, and a % chance of each one appearing)
    }
}
