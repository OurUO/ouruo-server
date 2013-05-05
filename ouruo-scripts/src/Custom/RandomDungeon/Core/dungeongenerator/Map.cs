using System;
using System.Drawing;
using System.Diagnostics;
using System.IO;

namespace iRogue
{
    public enum Direction { Left, Right, Up, Down };

    public class Map
    {

        int seed = 22;//54;
        public Random rand;
        public int MapWidth { get; set; }
        public int MapHeight { get; set; }
        public Cell[,] Cells { get; set; }

        private DungeonLevelGenerator _debugLevelGenerator;

        public Map()
        {
            MapWidth = MapHeight = 200;
        }

        public void GenerateLevel()
        {
            // Create our Cell map and fill with Granite (we'll dig rooms out of it)
            Cells = new Cell[MapWidth, MapHeight];
            PaintCellRectangle(0, 0, MapWidth - 1, MapHeight - 1, new Cell_Granite(), true);

            rand = new Random(++seed);

            // Choose which type of Dungeon level we want to create.  Only one type for now, but this can
            // be replaced with a random selection between any number of Generators.  Farther out, we can 
            // also tie generation of levels together (e.g. a "town" level generator can specify that the
            // next level generator should be a "sewer" level generator).
            DungeonLevelGenerator levelGenerator = new StandardLevelGenerator(this);
            levelGenerator.Generate();

            // Keep a pointer to our level generator for debug rendering purposes.
            _debugLevelGenerator = levelGenerator;
            SaveOnHD(m_filename);
        }

        private string m_filename = "Rooms/dungeon";

        public string Filename { get { return m_filename; } }

        public void SaveOnHD(String NomeFile)
        {
           if(NomeFile != null || NomeFile.Length > 0)
            {
                m_filename = NomeFile;
            }
           String NewFileName = m_filename;
           for (int r = 0; r < 20; r++)
           {
               if (File.Exists(NewFileName))
               {
                   //MessageBox.Show("Il file " + FILE_NAME + " esiste già.","Eccezione"); 
                   NewFileName = m_filename + r;
               }
               else 
               {
                   m_filename = NewFileName;
                   break;
               }
           }

           using (StreamWriter sw = File.CreateText(m_filename)) 
           {
               String testo = "s: " + MapHeight + "," + MapWidth + "\n";
               sw.Write(testo);
               for (int y = 0; y < MapHeight; y++)
               {
                   testo = "C:";
                   sw.Write(testo);
                   for (int x = 0; x < MapWidth; x++)
                   {
                       testo = Cells[x, y].ID+"";
                       sw.Write(testo);
                       if (x != MapWidth - 1)
                       {
                            sw.Write(",");
                       }
                   }
                   sw.Write("\n");
               }
               sw.Close(); 
           }  
        }

        public void PaintRoom(int startX, int startY, int[,] roomCells)
        {
            for (int y = 0; y < 19; y++)
                for (int x = 0; x < 18; x++)
                {
                    switch (roomCells[x, y])
                    {
                        case 3:
                            Cells[startX + x, startY + y] = new Cell_Door();
                            break;
                        case 2:
                            Cells[startX + x, startY + y] = new Cell_Floor();
                            break;
                        case 1:
                            Cells[startX + x, startY + y] = new Cell_Wall();
                            break;
                    }
                }
        }

        public void PaintCellRectangle(int xStart, int yStart, int xEnd, int yEnd, Cell cell, bool forceDraw)
        {
            for (int y = yStart; y <= yEnd; y++)
                for (int x = xStart; x <= xEnd; x++)
                {
                    if (forceDraw || Cells[x, y] == null || Cells[x, y].GetType() == typeof(Cell_Granite))
                        Cells[x, y] = cell;
                }
        }

        public void PaintCellEllipse(int xStart, int yStart, int xEnd, int yEnd, Cell cell)
        {
            // Draw an ellipse centered in the passed-in coordinates
            float xCenter = (xEnd + xStart) / 2.0f;
            float yCenter = (yEnd + yStart) / 2.0f;
            float radius = Math.Min(xEnd - xStart, yEnd - yStart) / 2.0f;
            float xAxis = (xEnd - xStart) / 2.0f;
            float yAxis = (yEnd - yStart) / 2.0f;
            float majorAxisSquared = (float)Math.Pow(Math.Max(xAxis, yAxis), 2.0);
            float minorAxisSquared = (float)Math.Pow(Math.Min(xAxis, yAxis), 2.0);

            for (int y = yStart; y <= yEnd; y++)
                for (int x = xStart; x <= xEnd; x++)
                {
                    // Only draw if (x,y) is within the ellipse
                    if (Math.Sqrt((x - xCenter) * (x - xCenter) / majorAxisSquared + (y - yCenter) * (y - yCenter) / minorAxisSquared) <= 1.0f)
                        Cells[x, y] = cell;
                }
        }

        public void PaintCellIfEmpty(int x, int y, Cell cell)
        {
            if (x >= 0 && x < MapWidth && y >= 0 && y < MapHeight)
                if (Cells[x, y].GetType() == typeof(Cell_Granite))
                    Cells[x, y] = cell;
        }

        public void Render(Graphics gr, float mouseX, float mouseY)
        {
            // For debug purposes only - let our level generator render if it wants to (eg a BSP renderer might render the partitions)
            if (_debugLevelGenerator != null)
                _debugLevelGenerator.Render(gr, mouseX, mouseY);
            for (int y = 0; y < MapHeight; y++)
                for (int x = 0; x < MapWidth; x++)
                {
                    if (Cells[x, y].GetType() != typeof(Cell_Granite))
                        Cells[x, y].Render(gr, x, y);
                }

            gr.DrawString("Seed: " + seed, new Font("Arial", 12), Brushes.White, 0, 0);
        }


    }
}
