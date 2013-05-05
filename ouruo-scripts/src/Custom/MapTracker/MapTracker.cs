
//////////////////////////////////////////////////////////////////////
// Automatically generated by Bradley's GumpStudio and roadmaster's 
// exporter.dll,  Special thanks goes to Daegon whose work the exporter
// was based off of, and Shadow wolf for his Template Idea.
//////////////////////////////////////////////////////////////////////
//#define RunUo2_0

using System;
using Server;
using Server.Gumps;
using Server.Network;
using Server.Commands;
using Server.Items;

namespace Bittiez.MapIssueTracker
{
    public class MapIssueGump : Gump
    {
        /////////////////////////////Settings/////////////////////////////

        public bool Show_Map_Issue_Count = true; //This is for the message at the top left of the gump that says "# map issues."

        /////////////////////////////End_Settings/////////////////////////////

        public Mobile caller;
        public static void Initialize()
        {
            CommandSystem.Register("CreateNewMapTracker", AccessLevel.Developer, new CommandEventHandler(CreateNew_MT));
            CommandSystem.Register("MapTracker", AccessLevel.Seer, new CommandEventHandler(MapIssueGump.MapTracker_OnCommand));
            CommandSystem.Register("AddMapIssue", AccessLevel.Seer, new CommandEventHandler(AMP));
        }

        [Usage("[MapTracker")]
        [Description("Displays current map issues.")]
        public static void MapTracker_OnCommand(CommandEventArgs e)
        {
            Mobile caller = e.Mobile;

            if (caller.HasGump(typeof(MapIssueGump)))
                caller.CloseGump(typeof(MapIssueGump));
            caller.SendGump(new MapIssueGump(caller));
        }

        public static void AMP(CommandEventArgs e)
        {
            MapIssueTracker tracker = new MapIssueGump().GetStone();
            if (tracker != null)
            {
                tracker.addLoc(e.Mobile, e.ArgString);
                e.Mobile.SendMessage("Location saved.");
            }
            else
            {
                e.Mobile.SendMessage("No Map Tracker found.");
            }
        }

        public static void CreateNew_MT(CommandEventArgs e)
        {
            Item ee = new MapIssueTracker();
            ee.MoveToWorld(e.Mobile.Location, e.Mobile.Map);
        }

        public MapIssueGump(Mobile from)
            : this()
        {
            caller = from;
        }

        public MapIssueGump()
            : base(50, 50)
        {
            MapIssueTracker tracker = GetStone();
            int total_pages = 0;
            this.Closable = true;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;

            AddPage(0);
            AddBackground(0, 0, 489, 429, 9390);
            AddLabel(200, 10, 137, "Map Issue Tracker");
            AddPage(1);

            decimal c = 0;
            if (tracker == null)
            {
                AddLabel(20, 25, 100, "There was no Map Tracker found, to create one please type .CreateNewMapTracker"); ;
            }
            else
            {
                c = tracker.locs.Count;
                if(Show_Map_Issue_Count)AddLabel(50, 10, 138, c.ToString() + " map issues.");
                decimal tp = c / 10;
                int tpp = (int)Math.Ceiling(tp);
                if (c < 10) total_pages = 1; else total_pages = tpp;
                if (total_pages < 1) total_pages = 1;
                int cc = 0;
                while (cc < total_pages)
                {
                    if ((cc + 1) < total_pages) AddButton(455, 405, 22405, 22405, cc + 2, GumpButtonType.Page, cc + 2);
                    if (cc > 0) AddButton(15, 405, 22402, 22402, cc, GumpButtonType.Page, cc);
                    Page_Of_Locations(cc, tracker);
                    AddPage(cc + 2);
                    cc++;
                }
            }
        }

        public void Page_Of_Locations(int page, MapIssueTracker tracker)
        {
            int c = 0;
            int x = 20;
            int y = 30;
            foreach (MIssue mi in (tracker.locs))
            {
                c++;
                if (c > (10 * page) && c <= ((10 * page) + 10))
                {
                    if (mi.POSTER == null) AddLabel(x, y, 100, mi.DESCRIPTION);
                    else AddLabel(x, y, 100, mi.DESCRIPTION + " [By: " + mi.POSTER.Name + "]");
                    AddLabel(x + 5, y + 15, 110, mi.X + ", " + mi.Y + ", " + mi.Z);
                    AddButton(x + 390, y, 4005, 4006, c, GumpButtonType.Reply, 1);
                    AddButton(x + 420, y, 4017, 4018, c + 4999, GumpButtonType.Reply, 0);
                    y += 36;
                }
            }

        }

        public MapIssueTracker GetStone()
        {
            foreach (Item i in World.Items.Values)
            {
                if (i is MapIssueTracker)
                    return (MapIssueTracker)i;
            }
            return null;
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {

            Mobile from = sender.Mobile;
            MapIssueTracker tracker = GetStone();
            int i = 0;
            if (tracker != null)
            {
                while (i < (tracker.locs.Count) && info.ButtonID != 0)
                {
                    if ((i + 1) == info.ButtonID)
                    {
                        MIssue th = (MIssue)(tracker.locs[i]);
                        sender.Mobile.X = th.X;
                        sender.Mobile.Y = th.Y;
                        sender.Mobile.Z = th.Z;
                        sender.Mobile.Map = th.Map;
                        break;
                    }
                    i++;
                }

                i = 5000;
                while (i < (tracker.locs.Count + 5000) && i > 4999)
                {
                    if (i == info.ButtonID)
                    {
                        tracker.remLoc(i - 5000);
                        from.SendMessage("Location deleted");
                        sender.Mobile.SendGump(new MapIssueGump());
                        break;
                    }
                    i++;
                }
            }
        }
    }
}