using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;
using System.IO;

namespace iRogue
{
    public delegate void RoomGeneratorDelegate(DungeonBSPNode node);
    public delegate void CorridorGeneratorDelegate(DungeonBSPNode node);

    public class StandardLevelGenerator : DungeonLevelGenerator
    {
        public StandardLevelGenerator(Map targetMap)
            : base(targetMap)
        {
        }

        override public void Render(Graphics gr, float x, float y)
        {
            partitionedTree.Render(gr);
            partitionedTree.RenderMousePosition(gr, x, y);
        }


        override public void Generate()
        {
            // Create BSP tree to partition floor randomly (but w/o overlaps)
            partitionedTree = PartitionDungeon();

            // Now that we have partitioned the dungeon into non-overlapping regions, create rooms in each "leaf" region
            // and create corridors between each non-leaf region. 
            // We do this by telling the partitioned dungeon tree to find all nodes from the bottom up at each level.  On leaf
            // nodes (those at the bottom of the tree which have not been partitioned into smaller regions) we call our 
            // "AddRoom" function with each leaf node's coordinates.  On non-leaf nodes, we call our "AddCorridor" function
            // Thanks to Jice for the overview (http://jice.nospam.googlepages.com/basicdungeongeneration).

            // Variety is the spice of life; mix things up a bit.
            RoomGeneratorDelegate roomGenerator;

            // Choose square or round rooms (or mix).  In future, have more complex room generators.
            /*
            switch (Rand.Next(3))
            {
                case 0:
                    roomGenerator = new RoomGeneratorDelegate(SquareRoomGenerator);
                    break;
                case 1:
                    roomGenerator = new RoomGeneratorDelegate(RoundRoomGenerator);
                    break;
                default:
                    roomGenerator = new RoomGeneratorDelegate(RandomShapeRoomGenerator);
                    break;
            }
            */
            roomGenerator = new RoomGeneratorDelegate(SquareRoomGenerator);

            // Choose a process by which we connect rooms
            if (Rand.Next(2) == 1)
            {
                // Do a BSP-based inverted breadth order search, creating rooms and corridors as we go.
                // Semi-"intelligent" approach, in that it gaurantees connections between all rooms and doesn't create "odd" rooms
                // or dead-end corridors
                partitionedTree.BottomsUpByLevelEnumerate(new RoomGeneratorDelegate(roomGenerator), new CorridorGeneratorDelegate(DefaultCorridorGenerator));
            }
            else
            {
                // Mimic DCSS's approach - get a list of rooms; randomize it; then dig corridors between them all, going right through rooms as we go.
                // This approach generates odd (eg doors in the middle of nowhere), but cool looking layouts.

                // 1. Get the list of regions with rooms in them
                // 2. Randomize the list (so we don't connect too many rooms that are right next to each other; counter-intuitive, I know)
                // 3. Connect the lists in order (connect #0 to #1, connect #1 to #2, etc).  brute force it.

                // Generate the list of rooms.  Don't pass a corridor generator since we're handling it ourselves post-processing.
                partitionedTree.BottomsUpByLevelEnumerate(new RoomGeneratorDelegate(roomGenerator), null);

                // Get the list of rooms
                List<DungeonBSPNode> roomRegions = partitionedTree.GetRoomRegions();

                // Randomize the list order (Go go Gadget-C#!)
                roomRegions.Sort(RandomNodeComparer);

                // Connect the room regions in the newly randomized list order
                DungeonBSPNode previousRoom = null;
                foreach (DungeonBSPNode currentRoom in roomRegions)
                {
                    if (previousRoom != null)
                        BruteForceConnectRooms(previousRoom, currentRoom);
                    previousRoom = currentRoom;
                }
            }
        }

        int RandomNodeComparer(DungeonBSPNode node1, DungeonBSPNode node2)
        {
            if (node1 == node2)
                return 0;
            return (Rand.Next(2) == 1) ? -1 : 1;

        }

        private void BruteForceConnectRooms(DungeonBSPNode regionA, DungeonBSPNode regionB)
        {
            // We don't care if we go through existing rooms; the goal is that we actually get atypical rooms...
            DiggerGnome digger = new DiggerGnome(TargetMap);

            if (regionA.BoundingRect.Bottom < regionB.BoundingRect.Top || regionB.BoundingRect.Bottom < regionA.BoundingRect.Top)
            {
                // Vertical split.  Determine which one is the upper & lower region
                DungeonBSPNode upperRegion = (regionA.BoundingRect.Bottom <= regionB.BoundingRect.Top) ? regionA : regionB;
                DungeonBSPNode lowerRegion = (upperRegion == regionA) ? regionB : regionA;

                // If the nodes' regions overlap horizontally by at least 3 (leaving room for walls), then we can just dig a
                // single vertical tunnel to connect them; otherwise, we need to dig an 'L' shapped corridor to connect them.
                int minOverlappingX = Math.Max(upperRegion.BoundingRect.Left, lowerRegion.BoundingRect.Left);
                int maxOverlappingX = Math.Min(upperRegion.BoundingRect.Right, lowerRegion.BoundingRect.Right);
                if (maxOverlappingX - minOverlappingX >= 3)
                {
                    // The regions overlap; we can dig a single vertical corridor to connect them
                    // Determine the range of X axis values that we can dig at in order to connect the regions
                    int corridorX = minOverlappingX + 1 + Rand.Next(maxOverlappingX - minOverlappingX - 2);

                    // Start at the border between the two regions at X=corridorX and dig towards the outside
                    // edge of each region; we are gauranteed to eventually hit something since the regions' bounding
                    // rects overlapped.
                    digger.Dig(corridorX, upperRegion.BoundingRect.Bottom, Direction.Up, 0, true);
                    digger.Dig(corridorX, upperRegion.BoundingRect.Bottom + 1, Direction.Down, lowerRegion.BoundingRect.Top - (upperRegion.BoundingRect.Bottom + 1), true);
                }
                else
                {
                    // They don't overlap enough; dig an 'L' shaped corridor to connect them.
                    int tunnelMeetX, tunnelMeetY;

                    if (upperRegion.BoundingRect.Left > lowerRegion.BoundingRect.Left)
                    {
                        //        _____
                        //        |   |
                        //        | L |
                        //        |___|
                        //  _____   |
                        //  |   |   |
                        //  | R |___|X
                        //  |___|    
                        tunnelMeetX = RandomValueBetween(Math.Max(upperRegion.BoundingRect.Left + 1, lowerRegion.BoundingRect.Right + 1), upperRegion.BoundingRect.Right);
                        tunnelMeetY = RandomValueBetween(lowerRegion.BoundingRect.Top + 1, lowerRegion.BoundingRect.Bottom);
                        digger.DigUpLeftCorridor(tunnelMeetX, tunnelMeetY, lowerRegion.BoundingRect.Right, upperRegion.BoundingRect.Bottom);
                    }
                    else
                    {
                        //    _____
                        //    |   |
                        //    | L |
                        //    |___|
                        //      |    _____
                        //      |    |   |
                        //     X|____| R |
                        //           |___|    
                        tunnelMeetX = RandomValueBetween(upperRegion.BoundingRect.Left + 1, Math.Min(lowerRegion.BoundingRect.Left, upperRegion.BoundingRect.Right - 1));
                        tunnelMeetY = RandomValueBetween(lowerRegion.BoundingRect.Top + 1, lowerRegion.BoundingRect.Bottom);
                        digger.DigUpRightLCorridor(tunnelMeetX, tunnelMeetY, lowerRegion.BoundingRect.Left, upperRegion.BoundingRect.Bottom);
                    }
                }
            }
            else
            {
                // Horizontal split.  Determine which one is the left & right region
                DungeonBSPNode leftRegion = (regionA.BoundingRect.Right <= regionB.BoundingRect.Left) ? regionA : regionB;
                DungeonBSPNode rightRegion = (leftRegion == regionA) ? regionB : regionA;

                // If the nodes' regions overlap vertically by at least 3 (leaving room for walls), then we can just dig a
                // single horizontal tunnel to connect them; otherwise, we need to dig an 'L' shapped corridor to connect them.
                int minOverlappingY = Math.Max(leftRegion.BoundingRect.Top, rightRegion.BoundingRect.Top);
                int maxOverlappingY = Math.Min(leftRegion.BoundingRect.Bottom, rightRegion.BoundingRect.Bottom);
                if (maxOverlappingY - minOverlappingY >= 3)
                {
                    // The regions overlap; we can dig a single horizontal corridor to connect them
                    // Determine the range of Y axis values that we can dig at in order to connect the regions
                    int corridorY = minOverlappingY + 1 + Rand.Next(maxOverlappingY - minOverlappingY - 2);

                    digger.Dig(leftRegion.BoundingRect.Right, corridorY, Direction.Left, 0, true);
                    digger.Dig(leftRegion.BoundingRect.Right + 1, corridorY, Direction.Right, rightRegion.BoundingRect.Left - (leftRegion.BoundingRect.Right + 1), true);
                }
                else
                {
                    // They don't overlap enough; dig an 'L' shaped corridor to connect them.
                    int tunnelMeetX, tunnelMeetY;

                    if (leftRegion.BoundingRect.Top > rightRegion.BoundingRect.Top)
                    {
                        // Left region's bounding rect is below the Right region's bound rect.
                        //        _____
                        //   X____|   |
                        //    |   | R |
                        //    |   |___|
                        //  __|__
                        //  |   |
                        //  | L |
                        //  |___|
                        tunnelMeetX = RandomValueBetween(leftRegion.BoundingRect.Left + 1, leftRegion.BoundingRect.Right);
                        tunnelMeetY = RandomValueBetween(rightRegion.BoundingRect.Top + 1, Math.Min(rightRegion.BoundingRect.Bottom - 1, leftRegion.BoundingRect.Top));
                        digger.DigDownRightCorridor(tunnelMeetX, tunnelMeetY, rightRegion.BoundingRect.Left, leftRegion.BoundingRect.Top);
                    }
                    else
                    {
                        // Left child region's bounding rect is above the Right child region's bound rect.
                        //    _____
                        //    |   |____X
                        //    | L |   |
                        //    |___|   |
                        //          __|__
                        //          |   |
                        //          | R |
                        //          |___|
                        tunnelMeetX = RandomValueBetween(rightRegion.BoundingRect.Left + 1, rightRegion.BoundingRect.Right);
                        tunnelMeetY = RandomValueBetween(leftRegion.BoundingRect.Top + 1, Math.Min(leftRegion.BoundingRect.Bottom - 1, rightRegion.BoundingRect.Top));
                        digger.DigDownLeftLCorridor(tunnelMeetX, tunnelMeetY, leftRegion.BoundingRect.Right, rightRegion.BoundingRect.Top);
                    }
                }
            }
        }

        public void DefaultCorridorGenerator(DungeonBSPNode dungeonNode)
        {
            DungeonBSPNode leftChild = dungeonNode.LeftChild;
            DungeonBSPNode rightChild = dungeonNode.RightChild;

            DiggerGnome digger = new DiggerGnome(TargetMap);

            if (leftChild == null || !leftChild.RoomBuilt)
                dungeonNode.BoundingRect = rightChild.BoundingRect;
            else if (rightChild == null || !rightChild.RoomBuilt)
                dungeonNode.BoundingRect = leftChild.BoundingRect;
            else
            {
                // Draw a corridor between our child nodes.  We have been keeping track of their bounding rectangles
                // as we've worked our way up the tree, so we can use that ensure we connect corridors to rooms
                if (dungeonNode.SplitOrientation == DungeonBSPNode.Orientation.Horizontal)
                {
                    // child nodes were split horizontally, so draw a horizontal corridor between them.
                    // If the nodes' regions overlap vertically by at least 3 (leaving room for walls), then we can just dig a
                    // single horizontal tunnel to connect them; otherwise, we need to dig an 'L' shapped corridor to connect them.
                    int minOverlappingY = Math.Max(leftChild.BoundingRect.Top, rightChild.BoundingRect.Top);
                    int maxOverlappingY = Math.Min(leftChild.BoundingRect.Bottom, rightChild.BoundingRect.Bottom);
                    if (maxOverlappingY - minOverlappingY >= 3)
                    {
                        // The regions overlap; we can dig a single horizontal corridor to connect them
                        // Determine the range of Y axis values that we can dig at in order to connect the regions
                        int corridorY = minOverlappingY + 1 + Rand.Next(maxOverlappingY - minOverlappingY - 2);

                        // Start at the border between the two regions at Y=corridorY and dig towards the outside
                        // edge of each region; we are gauranteed to eventually hit something since the regions' bounding
                        // rects overlapped.
                        digger.Dig(leftChild.BoundingRect.Right, corridorY, Direction.Left, 0, true);
                        digger.Dig(leftChild.BoundingRect.Right + 1, corridorY, Direction.Right, 0, true);
                    }
                    else
                    {
                        // They don't overlap enough; dig an 'L' shaped corridor to connect them.
                        int tunnelMeetX, tunnelMeetY;

                        // Note that some of the math below (in particular the Mins and Maxs) are because the regions *can* be slightly overlapping in
                        // the appropriate dimension; we don't draw a straight line if they overlap by less than 3 (to minimize the number of odd corridors
                        // that attach to the outside corner of a room)
                        if (leftChild.BoundingRect.Top > rightChild.BoundingRect.Top)
                        {
                            // Left child region's bounding rect is below the Right child region's bound rect.
                            //        _____
                            //   X____|   |
                            //    |   | R |
                            //    |   |___|
                            //  __|__
                            //  |   |
                            //  | L |
                            //  |___|
                            tunnelMeetX = RandomValueBetween(leftChild.BoundingRect.Left + 1, leftChild.BoundingRect.Right);
                            tunnelMeetY = RandomValueBetween(rightChild.BoundingRect.Top + 1, Math.Min(rightChild.BoundingRect.Bottom - 1, leftChild.BoundingRect.Top));
                            digger.DigDownRightCorridor(tunnelMeetX, tunnelMeetY, tunnelMeetX, tunnelMeetY);
                        }
                        else
                        {
                            // Left child region's bounding rect is above the Right child region's bound rect.
                            //    _____
                            //    |   |____X
                            //    | L |   |
                            //    |___|   |
                            //          __|__
                            //          |   |
                            //          | R |
                            //          |___|
                            tunnelMeetX = RandomValueBetween(rightChild.BoundingRect.Left + 1, rightChild.BoundingRect.Right);
                            tunnelMeetY = RandomValueBetween(leftChild.BoundingRect.Top + 1, Math.Min(leftChild.BoundingRect.Bottom - 1, rightChild.BoundingRect.Top));
                            digger.DigDownLeftLCorridor(tunnelMeetX, tunnelMeetY, tunnelMeetX, tunnelMeetY);
                        }
                    }

                    // TBD: Need to set bounding rect for Special Rooms
                }
                else // Vertical split
                {
                    // child nodes were split vertically, so draw a vertical corridor between them.
                    // If the nodes' regions overlap horizontally by at least 3 (leaving room for walls), then we can just dig a
                    // single vertical tunnel to connect them; otherwise, we need to dig an 'L' shapped corridor to connect them.
                    int minOverlappingX = Math.Max(leftChild.BoundingRect.Left, rightChild.BoundingRect.Left);
                    int maxOverlappingX = Math.Min(leftChild.BoundingRect.Right, rightChild.BoundingRect.Right);
                    if (maxOverlappingX - minOverlappingX >= 3)
                    {
                        // The regions overlap; we can dig a single vertical corridor to connect them
                        // Determine the range of X axis values that we can dig at in order to connect the regions
                        int corridorX = minOverlappingX + 1 + Rand.Next(maxOverlappingX - minOverlappingX - 2);

                        // Start at the border between the two regions at X=corridorX and dig towards the outside
                        // edge of each region; we are gauranteed to eventually hit something since the regions' bounding
                        // rects overlapped.
                        digger.Dig(corridorX, leftChild.BoundingRect.Bottom, Direction.Up, 0, true);
                        digger.Dig(corridorX, leftChild.BoundingRect.Bottom + 1, Direction.Down, 0, true);
                    }
                    else
                    {
                        // They don't overlap enough; dig an 'L' shaped corridor to connect them.
                        int tunnelMeetX, tunnelMeetY;

                        if (leftChild.BoundingRect.Left > rightChild.BoundingRect.Left)
                        {
                            //        _____
                            //        |   |
                            //        | L |
                            //        |___|
                            //  _____   |
                            //  |   |   |
                            //  | R |___|X
                            //  |___|    
                            tunnelMeetX = RandomValueBetween(Math.Max(leftChild.BoundingRect.Left + 1, rightChild.BoundingRect.Right + 1), leftChild.BoundingRect.Right);
                            tunnelMeetY = RandomValueBetween(rightChild.BoundingRect.Top + 1, rightChild.BoundingRect.Bottom);
                            digger.DigUpLeftCorridor(tunnelMeetX, tunnelMeetY, tunnelMeetX, tunnelMeetY);
                        }
                        else
                        {
                            //    _____
                            //    |   |
                            //    | L |
                            //    |___|
                            //      |    _____
                            //      |    |   |
                            //     X|____| R |
                            //           |___|    
                            tunnelMeetX = RandomValueBetween(leftChild.BoundingRect.Left, Math.Min(rightChild.BoundingRect.Left, leftChild.BoundingRect.Right - 1));
                            tunnelMeetY = RandomValueBetween(rightChild.BoundingRect.Top + 1, rightChild.BoundingRect.Bottom);
                            digger.DigUpRightLCorridor(tunnelMeetX, tunnelMeetY, tunnelMeetX, tunnelMeetY);
                        }
                    }
                }

                // Determine our bounding rectangle (as the union of our child nodes' rectangles).
                dungeonNode.BoundingRect = Rectangle.Union(leftChild.BoundingRect, rightChild.BoundingRect);
            }

            // TBD: Not quite right - "RoomOrCorridorBuilt" more accurate
            dungeonNode.RoomBuilt = true;
        }


        private DungeonBSPNode PartitionDungeon()
        {
            // Initialize a few variables.  These'll eventually be removed
            DungeonBSPNode.Map = TargetMap;

            // I eventually want to share the random # generator across all objects, so that all that's
            // necessary to completely recreate a particular run is the initial Seed & the list of user inputs/
            DungeonBSPNode.rand = TargetMap.rand;

            // Create the root node;  it covers the entire space (0.0,0.0) - (1.0,1.0)
            DungeonBSPNode rootNode = new DungeonBSPNode(0.0f, 0.0f, 1.0f, 1.0f);

            // Add Special Room before partitioning
/*            if (false) // place special room
            {
                Room prefabRoom = Room.LoadRandom();

                int xSpecialRoomStart = 20;
                int ySpecialRoomStart = 20;
                prefabRoom.BoundingRect = new Rectangle(prefabRoom.BoundingRect.Left + xSpecialRoomStart,
                                                        prefabRoom.BoundingRect.Top + ySpecialRoomStart, 
                                                        prefabRoom.BoundingRect.Width, prefabRoom.BoundingRect.Height);

                // Add the room to the Map.
                TargetMap.PaintRoom(xSpecialRoomStart, ySpecialRoomStart, prefabRoom.RoomCells);

                // Partition the remaining dungeon layer around the placed room
                rootNode.PartitionAround(prefabRoom.BoundingRect);
            }
            else
            {
*/                // No special room
                rootNode.Partition();
//            }
            return rootNode;
        }

        private int RandomValueBetween(int start, int end)
        {
            Debug.Assert(start <= end);
            return start + Rand.Next(end - start);
        }

        private void SetCellToWallIfNotFloor(int curX, int curY)
        {
            if (TargetMap.Cells[curX, curY].GetType() != typeof(Cell_Floor))
                TargetMap.Cells[curX, curY] = new Cell_Wall();
        }

        // Create a room in the area specified by the regionNode.
        public void SquareRoomGenerator(DungeonBSPNode dungeonRegion)
        {
            int MinIdealRoomSize = 6;

            // Convert from absolute normalized coordinates (0.0-1.0) to Map coordinates (0-(MapWidth-1), 0-(MapHeight-1))
            int xRegionStart = (int)Math.Ceiling((dungeonRegion.RegionEdges.Left * (TargetMap.MapWidth - 1)));
            int yRegionStart = (int)Math.Ceiling((dungeonRegion.RegionEdges.Top * (TargetMap.MapHeight - 1)));
            int xRegionEnd = (int)((dungeonRegion.RegionEdges.Right * (TargetMap.MapWidth - 1)));
            int yRegionEnd = (int)((dungeonRegion.RegionEdges.Bottom * (TargetMap.MapHeight - 1)));
            int regionWidth = xRegionEnd - xRegionStart;
            int regionHeight = yRegionEnd - yRegionStart;

            int roomWidth = RandomValueBetween(Math.Min(MinIdealRoomSize, regionWidth), regionWidth);
            int roomHeight = RandomValueBetween(Math.Min(MinIdealRoomSize, regionHeight), regionHeight);

            int xRoomStart = xRegionStart + Rand.Next(regionWidth - roomWidth);
            int yRoomStart = yRegionStart + Rand.Next(regionHeight - roomHeight);

            // Store the room coordinates in the Dungeon Region Node (we'll want them again later for corridor creation)
            dungeonRegion.BoundingRect = new Rectangle(xRoomStart, yRoomStart, roomWidth, roomHeight);
            dungeonRegion.RoomBuilt = true;

            // "Paint" the room into the Map
            TargetMap.PaintCellRectangle(xRoomStart, yRoomStart, xRoomStart + roomWidth, yRoomStart + roomHeight, new Cell_Wall(), true);
            TargetMap.PaintCellRectangle(xRoomStart + 1, yRoomStart + 1, xRoomStart + roomWidth - 1, yRoomStart + roomHeight - 1, new Cell_Floor(), true);
        }

        // Create a room in the area specified by the regionNode.
        public void RoundRoomGenerator(DungeonBSPNode dungeonRegion)
        {
            // Increase the minimum ideal room size since round rooms take up less space than square rooms
            int MinIdealRoomSize = 6;

            // Convert from absolute normalized coordinates (0.0-1.0) to Map coordinates (0-(MapWidth-1), 0-(MapHeight-1))
            int xRegionStart = (int)Math.Ceiling((dungeonRegion.RegionEdges.Left * (TargetMap.MapWidth - 1)));
            int yRegionStart = (int)Math.Ceiling((dungeonRegion.RegionEdges.Top * (TargetMap.MapHeight - 1)));
            int xRegionEnd = (int)((dungeonRegion.RegionEdges.Right * (TargetMap.MapWidth - 1)));
            int yRegionEnd = (int)((dungeonRegion.RegionEdges.Bottom * (TargetMap.MapHeight - 1)));
            int regionWidth = xRegionEnd - xRegionStart;
            int regionHeight = yRegionEnd - yRegionStart;

            int roomWidth = RandomValueBetween(Math.Min(MinIdealRoomSize, regionWidth), regionWidth);
            int roomHeight = RandomValueBetween(Math.Min(MinIdealRoomSize, regionHeight), regionHeight);

            int xRoomStart = xRegionStart + Rand.Next(regionWidth - roomWidth);
            int yRoomStart = yRegionStart + Rand.Next(regionHeight - roomHeight);

            // Store the room coordinates in the Dungeon Region Node (we'll want them again later for corridor creation)
            dungeonRegion.BoundingRect = new Rectangle(xRoomStart, yRoomStart, roomWidth, roomHeight);
            dungeonRegion.RoomBuilt = true;

            // "Paint" the room into the Map
            TargetMap.PaintCellEllipse(xRoomStart, yRoomStart, xRoomStart + roomWidth, yRoomStart + roomHeight, new Cell_Wall());
            TargetMap.PaintCellEllipse(xRoomStart + 1, yRoomStart + 1, xRoomStart + roomWidth - 1, yRoomStart + roomHeight - 1, new Cell_Floor());
        }

        public void RandomShapeRoomGenerator(DungeonBSPNode dungeonRegion)
        {
            if (Rand.Next(2) == 0)
                RoundRoomGenerator(dungeonRegion);
            else
                SquareRoomGenerator(dungeonRegion);
        }

        DungeonBSPNode partitionedTree;
    }
}
