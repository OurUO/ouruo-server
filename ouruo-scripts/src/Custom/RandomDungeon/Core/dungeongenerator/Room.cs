using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace iRogue
{
    public class Room
    {
        public static Room LoadRandom()
        {
            // Pick a random room and load it
            string roomName = "Rooms/dungeon6";
            return new Room(roomName);
        }



        public Room(string fileName)
        {
            string dataLine;
            int curCellY = 0;
            RoomWidth = -1;
            RoomHeight = -1;

            StreamReader roomFile = new StreamReader(fileName);

            // Read room metadata and cells
            while ((dataLine = roomFile.ReadLine()) != null)
            {
                string[] splitLine = dataLine.Split(':');
                string cmd = splitLine[0];
                string data = splitLine[1];
                switch (cmd.ToLower())
                {
                    case "s":
                        data = data.Replace(" ", "");
                        RoomWidth = Convert.ToInt32(data.Split(',')[0]);
                        RoomHeight = Convert.ToInt32(data.Split(',')[1]);
                        RoomCells = new int[RoomWidth, RoomHeight];
                        break;

                    case "c":
                        // Read line of cell data
                        data = data.Replace(" ", "");
                        string[] cellData = data.Split(',');
                        for (int x = 0; x < cellData.Length; x++)
                            RoomCells[x, curCellY] = Convert.ToInt32(cellData[x]);
                        curCellY++;
                        break;
                }
            }
            BoundingRect = new Rectangle(0, 0, RoomWidth, RoomHeight);
        }

        public int RoomWidth { get; set; }
        public int RoomHeight { get; set; }
        public Rectangle BoundingRect { get; set; }
        public int[,] RoomCells;
    }
}
