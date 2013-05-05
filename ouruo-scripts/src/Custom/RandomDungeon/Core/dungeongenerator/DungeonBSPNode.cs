using System;
using System.Drawing;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

namespace iRogue
{
    public class EdgeRect
    {
        public EdgeRect(float startX, float startY, float endX, float endY)
        {
            Left = startX;
            Right = endX;
            Top = startY;
            Bottom = endY;
        }
        public float Left { get; set; }
        public float Right { get; set; }
        public float Top { get; set; }
        public float Bottom { get; set; }
        public float Width { get { return Right - Left; } }
        public float Height { get { return Bottom - Top; } }
    }

    public class DungeonBSPNode
    {
        // Orientation - Identifies which axis the Node is split upon.
        public enum Orientation { Horizontal, Vertical };

        /// <summary>
        /// Constructor.
        /// </summary>
        public DungeonBSPNode(float startX, float startY, float endX, float endY)
        {
            RegionEdges = new EdgeRect(startX, startY, endX, endY);
        }

        /// <summary>
        /// Partitions (splits) this node into two halves, and then creates child nodes for
        /// each half and recursively partitions both of them (if they are not 'too small').
        /// </summary>
        public void Partition()
        {
            // Choose the axis along which we'll partition (split) this node.  If this is a very
            // narrow node in one axis, then favor the other axis in order to minimize long corridor-like rooms.
            if (RegionEdges.Width / RegionEdges.Height > MaxPartitionSizeRatio)
                SplitOrientation = Orientation.Horizontal;
            else if (RegionEdges.Height / RegionEdges.Width > MaxPartitionSizeRatio)
                SplitOrientation = Orientation.Vertical;
            else
                SplitOrientation = (rand.Next(2) == 1) ? Orientation.Horizontal : Orientation.Vertical;
            
            // Split the Node.
            if (SplitOrientation == Orientation.Horizontal)
            {
                // Pick the location along the XAxis (between the LeftEdge and RightEdge) at which we will split.
                SplitLocation = RegionEdges.Left + HomogenizedRandomValue() * RegionEdges.Width;

                // Create our two child nodes
                LeftChild = new DungeonBSPNode(RegionEdges.Left, RegionEdges.Top, SplitLocation, RegionEdges.Bottom);
                RightChild = new DungeonBSPNode(SplitLocation, RegionEdges.Top, RegionEdges.Right, RegionEdges.Bottom);

                SetDebugNames();

                // Partition child nodes if either is not yet too small
                if (WeShouldSplit(SplitLocation - RegionEdges.Left))
                    LeftChild.Partition();
                if (WeShouldSplit(RegionEdges.Right - SplitLocation))
                    RightChild.Partition();
            }
            else // Vertical split
            {
                // Pick the location along the YAxis (between the TopEdge and BottomEdge) at which we will split.
                SplitLocation = RegionEdges.Top + HomogenizedRandomValue() * RegionEdges.Height;

                // Create our two (Left = upper and Right = lower) child nodes
                LeftChild = new DungeonBSPNode(RegionEdges.Left, RegionEdges.Top, RegionEdges.Right, SplitLocation);
                RightChild = new DungeonBSPNode(RegionEdges.Left, SplitLocation, RegionEdges.Right, RegionEdges.Bottom);

                SetDebugNames();

                // Partition child nodes if either is not yet too small
                if (WeShouldSplit(SplitLocation - RegionEdges.Top))
                    LeftChild.Partition();
                if (WeShouldSplit(RegionEdges.Bottom - SplitLocation))
                    RightChild.Partition();
            }
        }

        private void SetDebugNames()
        {
            DebugValue++;
            LeftChild.DebugName = DebugValue + "L";
            RightChild.DebugName = DebugValue + "R";
        }

        public void PartitionAround(Rectangle boundingRect)
        {
            float startX = (float)boundingRect.Left / Map.MapWidth;
            float startY = (float)boundingRect.Top / Map.MapHeight;
            float endX = (float)boundingRect.Right/ Map.MapWidth;
            float endY = (float)boundingRect.Bottom/ Map.MapHeight;

            // Create partitions around the carved out space, and don't partition the carved out space further

            // Here is how we create the partitions around the carved out space (marked with " XX ").  The #s
            // represent the splits...
            // ________________________
            // |                      |
            // |                      |
            // |                      |
            // |____1_________________|
            // |        |    |        |
            // |        | XX 4        |
            // |        |____|___3____|
            // |        |             |
            // |        2             |
            // |        |             |
            // |________|_____________|


            // Do first split (#1) horizontally along the top edge of the carved out space
            this.SplitOrientation = Orientation.Vertical;
            this.SplitLocation = startX;
            if (startY == RegionEdges.Top)
                LeftChild = null;    // Carved out partition abuts the TopEdge, so no need to create a 'Left' part
            else
            {
                LeftChild = new DungeonBSPNode(RegionEdges.Left, RegionEdges.Top, RegionEdges.Right, startY);
                if (WeShouldSplit(startY - RegionEdges.Top))
                    LeftChild.Partition();
            }
            RightChild = new DungeonBSPNode(RegionEdges.Left, startY, RegionEdges.Right, RegionEdges.Bottom);

            // Do second split (#2) vertically along the left edge of the carved out space
            RightChild.SplitOrientation = Orientation.Horizontal;
            RightChild.SplitLocation = startY;
            if (startX == RegionEdges.Left)
                RightChild.LeftChild = null;    // Carved out partition abuts the LeftEdge, so no need to create a 'Left' part
            else
            {
                RightChild.LeftChild = new DungeonBSPNode(RegionEdges.Left, startY, startX, RegionEdges.Bottom);
                if (WeShouldSplit(startX - RegionEdges.Left))
                    RightChild.LeftChild.Partition();
            }
            RightChild.RightChild = new DungeonBSPNode(startX, startY, RegionEdges.Right, RegionEdges.Bottom);

            // Do third split (#3) horizontally along the bottom edge of the carved out space
            RightChild.RightChild.SplitOrientation = Orientation.Vertical;
            RightChild.RightChild.SplitLocation = endY;
            if (RegionEdges.Bottom == endY)
                RightChild.RightChild.RightChild = null;    // Carved out partition abuts the BottomEdge, so no need to create a 'Right' part
            else
            {
                RightChild.RightChild.RightChild = new DungeonBSPNode(startX, endY, RegionEdges.Right, RegionEdges.Bottom);
                if (WeShouldSplit(RegionEdges.Bottom - endY))
                    RightChild.RightChild.RightChild.Partition();
            }
            RightChild.RightChild.LeftChild = new DungeonBSPNode(startX, startY, RegionEdges.Right, endY);

            // Do fourth split (#4) vertically along the right edge of the carved out space
            RightChild.RightChild.LeftChild.SplitOrientation = Orientation.Horizontal;
            RightChild.RightChild.LeftChild.SplitLocation = endX;
            if (RegionEdges.Right == endX)    // Carved out partition abuts the RightEdge, so no need to create a 'Right' part
                RightChild.RightChild.LeftChild.RightChild = null;
            else
            {
                RightChild.RightChild.LeftChild.RightChild = new DungeonBSPNode(endX, startY, RegionEdges.Right, endY);
                if (WeShouldSplit(RegionEdges.Right - endX))
                    RightChild.RightChild.LeftChild.RightChild.Partition();
            }

            // Finally, partition the carved out space (and don't further partition it)
            RightChild.RightChild.LeftChild.LeftChild = new DungeonBSPNode(startX, startY, endX, endY);

            // Mark that the carved-out partition is pre-populated (don't add a room to it)
            RightChild.RightChild.LeftChild.LeftChild.RoomBuilt = true;
            RightChild.RightChild.LeftChild.LeftChild.BoundingRect = boundingRect;
        }

        private bool WeShouldSplit(float partitionSize)
        {
            // For variety, we don't split ~10% of the partitions which are small.  This allows creation of
            // a few slightly larger rooms in the dungeon (particularly useful later one when we're trying to place 'special' rooms)
            if (partitionSize > SmallestPartitionSize && partitionSize < SmallestPartitionSize * 2.0 && rand.NextDouble() <= .1)
                return false;

            // If the partition is bigger than the "Smallest Partition Size" value, then split.
            return partitionSize > SmallestPartitionSize;
        }
        
        /// <summary>
        /// Returns a random absolute normalized value (between 0.0 and 1.0).  Allows homogenization
        /// of the partitions to be controlled
        /// </summary>
        /// <returns></returns>
        private float HomogenizedRandomValue()
        {
            return (float)(0.5 - (rand.NextDouble() * HomogeneityFactor));
        }
        
        public List<DungeonBSPNode> GetRoomRegions()
        {
            List<DungeonBSPNode> list = new List<DungeonBSPNode>();
            EnumerateLeafNodes(list);
            return list;
        }

        public void EnumerateLeafNodes(List<DungeonBSPNode> list)
        {
            // If this node was partitioned, then recurse into our child nodes; otherwise, call the callback function
            if (LeftChild != null || RightChild != null)
            {
                if (LeftChild != null)
                    LeftChild.EnumerateLeafNodes(list);
                if (RightChild != null)
                    RightChild.EnumerateLeafNodes(list);
            }
            else if (RoomBuilt)
               list.Add(this);// callback(this);
        }

        public void BottomsUpByLevelEnumerate(RoomGeneratorDelegate roomGenerator, CorridorGeneratorDelegate corridorGenerator)
        {
            Stack stack1 = new Stack();
            Stack stack2 = new Stack();
            stack1.Push(this);
            while (stack1.Count > 0)
            {
                DungeonBSPNode currentNode = (DungeonBSPNode)stack1.Pop();
                stack2.Push(currentNode);
                if (currentNode.LeftChild != null)
                    stack1.Push(currentNode.LeftChild);
                if (currentNode.RightChild != null)
                    stack1.Push(currentNode.RightChild);
            }

            while (stack2.Count > 0)
            {
                DungeonBSPNode currentNode = (DungeonBSPNode)stack2.Pop();
                if (currentNode.LeftChild == null && currentNode.RightChild == null)
                {
                    // Leaf node - create a room
                    if (!currentNode.RoomBuilt && roomGenerator != null)
                        roomGenerator(currentNode);
                }
                else
                {
                    // non-leaf node; create corridor
                    if (corridorGenerator != null && (currentNode.LeftChild.RoomBuilt || currentNode.RightChild.RoomBuilt))
                        corridorGenerator(currentNode);
                }
            }
        }

/*
        public void EnumerateSiblingNodes(AddCorridorDelegate addCorridorDelegate)
        {
            if (Left != null)
                Left.EnumerateSiblingNodes(addCorridorDelegate);
            if (Right != null)
                Right.EnumerateSiblingNodes(addCorridorDelegate);

            // If either Left or Right child is null, then obviously no need to connect them!
            if (Left != null && Right != null)
                addCorridorDelegate(Left, Right, SplitOrientation);
        }*/

        /// <summary>
        /// Test code to Render the BSP tree to a graphic context.  Used to validate the BSP tree code.
        /// </summary>
        /// <param name="gr"></param>
        public void Render(Graphics gr)
        {
            float w = Map.MapWidth * 8;
            float h = Map.MapHeight * 8;
            gr.FillRectangle(Brushes.Black, (float)RegionEdges.Left * w + 10, (float)RegionEdges.Top * h + 10, (float)(RegionEdges.Width) * w, (float)(RegionEdges.Height) * h);
            gr.DrawRectangle(Pens.Red, (float)RegionEdges.Left * w + 10, (float)RegionEdges.Top * h + 10, (float)(RegionEdges.Width) * w, (float)(RegionEdges.Height) * h);

            if (LeftChild != null)
                LeftChild.Render(gr);
            if (RightChild != null)
                RightChild.Render(gr);
        }

        public void RenderMousePosition(Graphics gr, float mouseX, float mouseY)
        {
            // Figure out which node we're over, and render it's debugname
            List<DungeonBSPNode> leafNodes = new List<DungeonBSPNode>();
            EnumerateLeafNodes(leafNodes);
            foreach (DungeonBSPNode region in leafNodes)
            {
                if (mouseX >= region.RegionEdges.Left && mouseX < region.RegionEdges.Right &&
                    mouseY >= region.RegionEdges.Top && mouseY < region.RegionEdges.Bottom)
                {
                    gr.DrawString(region.DebugName, new Font("Arial", 12), Brushes.White, new Point(10, 40));
                    break;                        
                }
            }
        }

        // The Edges of this Node in the Map.  Note that we store all Edge values and the SplitLocation
        // in absolute (as compared to to relative) normalized (0.0-1.0) coordinates.  We don't convert
        // to actual 'dungeon ints' until we're placing rooms.
        // TBD: Move to floats?  Move to Rect?
        // TBD: Do a pass and see if it cleans up any math if I move to relative coordinates & just calculate on the way 'down' the tree...
        public EdgeRect RegionEdges;

        // Bounding REct of the room within the region
        public Rectangle BoundingRect;

        // Whether we're Split horizontally or Vertically
        public Orientation SplitOrientation;

        // The absolute normalized location at which this Node will be split.  Will be a value between
        // LeftEdge & RightEdge, or TopEdge & BottomEdge, depending on SplitOrientation's value.
        // TBD: Do I need to store this in the node, or can it just be local to the Partition function?
        public float SplitLocation;

        // Our Left & Right Child Nodes.  "Left" and "Right" refer to the binary tree node positions, and
        // apply to both horizontal and vertical orientations (in the latter case, "Left" --> "Up" and 
        // "Right" --> "Down".
        public DungeonBSPNode LeftChild { get; set; }
        public DungeonBSPNode RightChild { get; set; }

        // TBD: Want our random factor to be completely reproducible.  An entire Dungeon run should be recreatable
        // just from the initial Random Seed and the list of user inputs.
        static public Random rand;

        // The smallest size that a partition can get.  In absolute terms, so "0.1" would equate to 1/10th the
        // size of the map.
        /// TBD: Make SmallestPartitionSize something that is set by the caller to allow different dungeon generation schemas.
        static private float SmallestPartitionSize = .2f;

        // When choosing which axis to split a Node on, we want to optimize the number of "squarish"
        // rooms, and minimize the number of "long corridor rooms."  To do this, when splitting a Node,
        // we look at the ratio of Width to Height, and if it crosses the MaxPartitionSizeRatio threshold
        // in either axis, then we forcibly split on the *other* axis.
        /// TBD: Make MaxPartitionSizeRatio something that is set by the caller to allow different dungeon generation schemas.
        static private float MaxPartitionSizeRatio = 1.5f;

        // The homogeneityFactor determines how "common" split partitions are.  The value is between 0.0 and 0.5.  A small value (e.g. .1)
        // creates a Dungeon with similar size partitions; A large value (e.g. 0.4) creates a Dungeon 
        // with more varied sized partition.  The balance is finding the right number - too small a value == all partitions are the
        // same; too large a value == higher likelihood of long narrow "corridor rooms"
        /// TBD: Make HomogeneityFactor something that is set by the caller to allow different dungeon generation schemas.
        static private float HomogeneityFactor = 0.25f;

        // A pointer back to our Map.  Used to generate rooms
        // TBD: Move room generation out of the BSPNode class, which should only concern itself with Partitioning the space.  Ideally, even lose the 'Map' reference.
        static public Map Map;

        public bool RoomBuilt = false;

        public string DebugName = "unset";
        static public int DebugValue = 0;

    }
}
