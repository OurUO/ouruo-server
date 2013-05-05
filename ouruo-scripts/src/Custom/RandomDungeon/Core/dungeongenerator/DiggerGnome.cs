
namespace iRogue
{
    public class DiggerGnome
    {
        public DiggerGnome(Map targetMap)
        {
            _targetMap = targetMap;
        }

        // Note: curDirection is future-proofing; I can imagine a world in which we have nonlinear diggers.  Imagine a drunk diggergnome, for instance :)
        public void Dig(int startX, int startY, Direction startingDirection, int requiredDigDistance, bool continueDiggingAfterRequiredDistanceUntilHitFloor)
        {
            // Foreman: "Hey you, wake up!"
            // Gnome: "*snort* Huh? Wha'?"
            // Foreman: "Stand over there and face this way!"
            // Gnome: "*grumble*" <Starts shuffling over>
            int curX = startX;
            int curY = startY;
            Direction curDirection = startingDirection;


            // Foreman: "Alright you, start digging, and don't stop for anything until you've dug that far!
            //           After that, I may want you to keep digging until you complete the tunnel..."
            // Foreman: "And you're not done until I *say* you're done!"
            bool doneDigging = false;

            // Gnome: "*mutter*" <Picks up pickaxe, starts hacking away>
            do
            {
                // Forman: "Dig here!"
                _targetMap.Cells[curX, curY] = new Cell_Floor();

                // Foreman: "Hey, make sure you shore up these walls, you maggot!"
                // Gnome: <eyes foreman's back, fingers pick edge> "*grumble*"
                if (curDirection == Direction.Left || curDirection == Direction.Right)
                {
                    _targetMap.PaintCellIfEmpty(curX, curY - 1, new Cell_Wall());
                    _targetMap.PaintCellIfEmpty(curX, curY + 1, new Cell_Wall());
                }
                else
                {
                    _targetMap.PaintCellIfEmpty(curX - 1, curY, new Cell_Wall());
                    _targetMap.PaintCellIfEmpty(curX + 1, curY, new Cell_Wall());
                }

                // Foreman: "Alright, ease up - let's see if you've dug far enough yet..."
                // Gnome: *sneers* <plops down and leans against the cave wall>
                requiredDigDistance--;
                if (requiredDigDistance <= 0)
                {
                    // Foreman: <checks tunnel plan on paper> "Hmm, seems like you've dug the required distance - *finally*"
                    // Gnome: <starts to get up and head back to the start of the tunnel>
                    if (continueDiggingAfterRequiredDistanceUntilHitFloor)
                    {
                        // Foreman: "Hey! Where do you think you're going?  I didn't say you were done yet!  See if you broke through yet!"
                        // Gnome: *grumbles*
                        if ((curDirection != Direction.Right && _targetMap.Cells[curX - 1, curY].GetType() == typeof(Cell_Floor)) ||
                            (curDirection != Direction.Left && _targetMap.Cells[curX + 1, curY].GetType() == typeof(Cell_Floor)) ||
                            (curDirection != Direction.Down && _targetMap.Cells[curX, curY - 1].GetType() == typeof(Cell_Floor)) ||
                            (curDirection != Direction.Up && _targetMap.Cells[curX, curY + 1].GetType() == typeof(Cell_Floor)))
                        {
                            // Gnome: <gestures towards the light coming from in front of them and glares up at the foreman>
                            // Foreman: "Huh, we made it.  Must be my great management ability!"  
                            doneDigging = true;
                        }
                    }
                    else
                        doneDigging = true;
                }

                if (!doneDigging)
                {
                    // Foreman: "Alright you - keep moving, we're not done yet!"
                    switch (curDirection)
                    {
                        case Direction.Left: curX--; break;
                        case Direction.Right: curX++; break;
                        case Direction.Up: curY--; break;
                        case Direction.Down: curY++; break;
                    }

                    // <debug>
                    if (curX < 1 || curY < 1 || curX >= _targetMap.MapWidth - 1 || curY >= _targetMap.MapHeight - 1)
                        return;
                }

            } while (!doneDigging);

            // Foreman: "One more thing - make sure there aren't any un-shored up walls here.  Then you can skitter off"
            // Gnome: <climbs up from floor, mutters a curse at the foreman, and looks around>
            for (int x = curX - 1; x <= curX + 1; x++)
                for (int y = curY - 1; y <= curY + 1; y++)
                    _targetMap.PaintCellIfEmpty(x, y, new Cell_Wall());
        }

        public void DigDownRightCorridor(int tunnelMeetX, int tunnelMeetY, int endX, int endY)
        {
            // Dig down and right from the specified meeting point until we meet something (note: caller is tasked with gauranteeing that
            // there is something there to meet!)
            Dig(tunnelMeetX, tunnelMeetY + 1, Direction.Down, endY - tunnelMeetY, true);
            Dig(tunnelMeetX + 1, tunnelMeetY, Direction.Right, endX - (tunnelMeetX + 1), true);

            // Draw the corner piece
            _targetMap.Cells[tunnelMeetX, tunnelMeetY] = new Cell_Floor();
            _targetMap.PaintCellIfEmpty(tunnelMeetX - 1, tunnelMeetY, new Cell_Wall());
            _targetMap.PaintCellIfEmpty(tunnelMeetX, tunnelMeetY - 1, new Cell_Wall());
            _targetMap.PaintCellIfEmpty(tunnelMeetX - 1, tunnelMeetY - 1, new Cell_Wall());
        }

        public void DigUpLeftCorridor(int tunnelMeetX, int tunnelMeetY, int endX, int endY)
        {
            // Dig up and left from the specified meeting point until we meet something (note: caller is tasked with gauranteeing that
            // there is something there to meet!)
            Dig(tunnelMeetX, tunnelMeetY - 1, Direction.Up, tunnelMeetY - endY, true);
            Dig(tunnelMeetX - 1, tunnelMeetY, Direction.Left, tunnelMeetX - endX, true);

            // Draw the corner piece
            _targetMap.Cells[tunnelMeetX, tunnelMeetY] = new Cell_Floor();
            _targetMap.PaintCellIfEmpty(tunnelMeetX + 1, tunnelMeetY, new Cell_Wall());
            _targetMap.PaintCellIfEmpty(tunnelMeetX, tunnelMeetY + 1, new Cell_Wall());
            _targetMap.PaintCellIfEmpty(tunnelMeetX + 1, tunnelMeetY + 1, new Cell_Wall());
        }

        public void DigDownLeftLCorridor(int tunnelMeetX, int tunnelMeetY, int xEnd, int yEnd)
        {
            // Dig down and left from the specified meeting point until we meet something (note: caller is tasked with gauranteeing that
            // there is something there to meet!)
            // Dig the two corridors, but don't draw the actual corner piece where they meet (the DigCorridor algorithm doesn't handle it well).
            Dig(tunnelMeetX, tunnelMeetY + 1, Direction.Down, yEnd - tunnelMeetY, true);
            Dig(tunnelMeetX - 1, tunnelMeetY, Direction.Left, tunnelMeetX - xEnd, true);

            // Draw the corner piece
            _targetMap.Cells[tunnelMeetX, tunnelMeetY] = new Cell_Floor();
            _targetMap.PaintCellIfEmpty(tunnelMeetX, tunnelMeetY - 1, new Cell_Wall());
            _targetMap.PaintCellIfEmpty(tunnelMeetX + 1, tunnelMeetY - 1, new Cell_Wall());
            _targetMap.PaintCellIfEmpty(tunnelMeetX + 1, tunnelMeetY, new Cell_Wall());
        }

        public void DigUpRightLCorridor(int tunnelMeetX, int tunnelMeetY, int endX, int endY)
        {
            // Dig up and right from the specified meeting point until we meet something (note: caller is tasked with gauranteeing that
            // there is something there to meet!)
            Dig(tunnelMeetX, tunnelMeetY - 1, Direction.Up, tunnelMeetY - endY, true);
            Dig(tunnelMeetX + 1, tunnelMeetY, Direction.Right, endX - tunnelMeetX, true);

            // Draw the corner piece
            _targetMap.Cells[tunnelMeetX, tunnelMeetY] = new Cell_Floor();
            _targetMap.PaintCellIfEmpty(tunnelMeetX - 1, tunnelMeetY, new Cell_Wall());
            _targetMap.PaintCellIfEmpty(tunnelMeetX - 1, tunnelMeetY + 1, new Cell_Wall());
            _targetMap.PaintCellIfEmpty(tunnelMeetX, tunnelMeetY + 1, new Cell_Wall());
        }

        Map _targetMap;
    }
}
