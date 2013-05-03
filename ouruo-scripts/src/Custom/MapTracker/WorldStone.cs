using System;
using Server.Items;
using Server.Gumps;
using Server.Network;
using System.Collections;
using Server.Misc;
using Server.Mobiles;
using Server.Commands;
using Server;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text;
//using Bittiez.RemoteChat;

namespace Bittiez.MapIssueTracker
{
    public class MapIssueTracker : Item
    {
        public int verSion = 1;
        public Item self;
        public ArrayList locs;
        
        [Constructable]
        public MapIssueTracker()
            : base(0xED4)
        {
            self = this;
            Movable = false;
            Hue = 0x476;
            Name = "World Map Issue Tracker";
            locs = locs = new ArrayList();
        }
        public MapIssueTracker(Serial serial) : base(serial) { }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);
            list.Add("<BASEFONT COLOR=#1EFF00>{0} Current map issues. <BASEFONT COLOR=#FFFFFF>| <BASEFONT COLOR=#1DF0101>Do not delete this or all saved map issues will be lost.<BASEFONT COLOR=#FFFFFF>", locs.Count);
            //list.Add("<BASEFONT COLOR=#1DF0101>{0} Current map issues.", locs.Count);
        }

        public void AMP(CommandEventArgs e)
        {
            addLoc(e.Mobile, e.ArgString);
            e.Mobile.SendMessage("Location saved.");
        }


       public void addLoc(Mobile from, String description)
        {
            IPoint3D location = from.Location;
            MIssue iss = new MIssue();
            
            iss.X = location.X;
            iss.Y = location.Y;
            iss.Z = location.Z;
            iss.Map = from.Map;
            iss.DESCRIPTION = description;
            iss.POSTER = from;
            locs.Add(iss);

        }

       public void remLoc(int remove_at)
       {
           locs.RemoveAt(remove_at);
       }

        public bool FindStone(Item me)
        {
            int c = 0;
            foreach (Item i in World.Items.Values)
            {
                if (i is MapIssueTracker)
                {
                    c++;
                }
                if (c > 0)
                {
                    return true;
                }
                if (i is MapIssueTracker && !(i == me) || me == null)
                {
                    return true;
                }
            }
            return false;
        }



        public override void OnDoubleClick(Mobile from)
        {
            this.InvalidateProperties();
            from.SendMessage("Refreshed Issue Count.");
        }

        public override void OnDoubleClickDead(Mobile from)
        {
            OnDoubleClick(from);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(verSion); // version 
            switch (verSion)
            {
                case 1:
                    writer.WriteItemList(locs);
                    break;
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            verSion = reader.ReadInt();
            switch (verSion)
            {
                case 1:
                    locs = reader.ReadItemList();
                    break;
            }
        }
    }


    public class MIssue : Item
    {
        private int x, y, z;
        private Map map;
        private string description;
        private IPoint3D ipoint;
        private Mobile poster;

        public Mobile POSTER { get { return poster; } set { poster = value; } }
        public IPoint3D Ipoint { get { return ipoint; } set { ipoint = value; } }
        public new int X { get { return x; } set { x = value; } }
        public new int Y { get { return y; } set { y = value; } }
        public new int Z { get { return z; } set { z = value; } }
        public new Map Map { get { return map; } set { map = value; } }
        public string DESCRIPTION { get { return description; } set { description = value; } }

        public MIssue() : base()
        {
            map = Map.Felucca;
            x = 0;
            y = 0;
            z = 0;
            description = "";
            ipoint = null;
            poster = null;
        }
        public MIssue(Serial serial) : base(serial) { }


        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1);

            writer.Write(x);
            writer.Write(y);
            writer.Write(z);
            writer.Write(description);
            writer.Write(map);

            writer.Write(poster);


        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int ver = reader.ReadInt();

            x = reader.ReadInt();
            y = reader.ReadInt();
            z = reader.ReadInt();
            description = reader.ReadString();
            map = reader.ReadMap();

            if(ver >= 1) poster = reader.ReadMobile();

        }
    }
}
