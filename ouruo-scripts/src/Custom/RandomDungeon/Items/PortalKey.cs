using System;
using System.Collections.Generic;
using Server;
using Server.Mobiles;
using Server.Engines.Quests;

namespace Server.Engines.RandomDungeon
{
    public class PortalKey : QuestItem 
    {
        private DateTime m_creation;
        private DateTime m_EndTime;
        private List<int> m_PortalID = new List<int> ();
        private bool m_Expires = false;
        private TimeSpan m_RemainTime;
        private Timer m_Timer;
        private Point3D m_Destination;
        private Map m_map;
        private bool m_Active = false;

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime creation { get { return m_creation; }}

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime EndTime { get { return m_EndTime; }}

        [CommandProperty( AccessLevel.GameMaster )]
        public List<int> portalID { get { return m_PortalID; } set { m_PortalID = (List<int>) value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Expires { get { return m_Expires; } set { m_Expires = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan RemainTime { get { return m_RemainTime; } set { m_EndTime = m_EndTime + value - m_RemainTime; m_RemainTime = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D Destination { get { return m_Destination; } set { m_Destination = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public Map map { get { return m_map; } set { m_map = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Active { get { return m_Active; } set { m_Active = value; Activate(m_Active); } }

        [Constructable]
		public PortalKey() : base( 0xFC1 )
		{
			Weight = 1.0;
            m_creation = DateTime.Now;
            m_EndTime = DateTime.Now;
            m_RemainTime = new TimeSpan(0, 0, 0);
            m_PortalID.Add(1);
            m_PortalID.Add(2);
		}

        public PortalKey(List<int> PortalID, Point3D loc, Map map, bool Active, bool Expires, TimeSpan RemainTime): this()
        {
            m_PortalID = PortalID;
            m_Expires = Expires;
            m_RemainTime = RemainTime;
            m_Destination = loc;
            m_map = map;
            m_EndTime = DateTime.Now + m_RemainTime;
            Activate(Active);
        }

        public void Activate(bool value)
        {
            if (Deleted)
                return;

            m_Active = value;

            if (value)
            {
                m_EndTime = DateTime.Now + m_RemainTime;
                m_Timer = new InternalTimer(this, m_RemainTime);
                m_Timer.Start();
            }
            else
            {
                if (m_Timer != null)
                {
                    m_RemainTime = m_EndTime - DateTime.Now;
                    m_Timer.Stop();
                }
            }
            InvalidateProperties();
            return;
        }

        public override bool CanDrop(PlayerMobile player)
        {
            return false;
        }

        public PortalKey(Serial serial) : base(serial)
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
            writer.WriteDeltaTime(m_creation);
            writer.WriteDeltaTime(m_EndTime);
            writer.Write((TimeSpan)m_RemainTime);
            writer.Write((Map)m_map);
            writer.Write((Point3D)Destination);
            writer.Write(m_Active);
            writer.Write(m_Expires);
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
            m_creation = reader.ReadDateTime();
            m_EndTime = reader.ReadDateTime();
            m_RemainTime = reader.ReadTimeSpan();
            m_map = reader.ReadMap();
            m_Destination = reader.ReadPoint3D();
            m_Active = reader.ReadBool();
            m_Expires = reader.ReadBool();
		}

        // this controls the list you see when you mouse-over the item
        public override void AddNameProperty(ObjectPropertyList list)
        {
            // make sure the normal mouse-over props show up
            base.AddNameProperty(list);
            TimeSpan timetouse = m_RemainTime;

            if (m_Active)
            {
                // initial variables for use only inside the property list
                m_RemainTime = m_EndTime - DateTime.Now;
                timetouse = m_EndTime - DateTime.Now;
                Console.WriteLine("timetouse = " + timetouse.ToString()+" EndTime = "+ m_EndTime + " Now = "+ DateTime.Now);
            }

            string lisths;

            // determine the info the timer display shows
            if (timetouse.Minutes > 0)
            {
                int min = timetouse.Minutes;
                lisths = String.Format("{0} minutes.", min.ToString());
            }
            else if (timetouse.Seconds > 0)
            {
                int sec = timetouse.Seconds;
                lisths = String.Format("{0} seconds.", sec.ToString());
            }
            else
            {
                lisths = ("<BASEFONT COLOR=#00FF00>Dormant<BASEFONT COLOR=#FFFFFF>"); //FFFFFF
            }

            if (m_Active)
            {
                // adding the timer to the property list
                list.Add("<BASEFONT COLOR=#00FF00>Half Life Remaining: {0}<BASEFONT COLOR=#FFFFFF>", lisths); //FFFFFF
                // because we do not want the server spamming updates, slow down how fast the mouse-over info refreshes
                Timer.DelayCall(TimeSpan.FromSeconds(1.0), new TimerCallback(InvalidateProperties));
            }
            else 
            {
                // adding the timer to the property list
                list.Add("<BASEFONT COLOR=#FF0000>Half Life Remaining: {0}<BASEFONT COLOR=#FFFFFF>", lisths); //FFFFFF
            }
        }

        private class InternalTimer : Timer
        {
            private Item m_Item;

            public InternalTimer(Item item, TimeSpan end): base(end)
            {
                m_Item = item;
            }

            protected override void OnTick()
            {
                m_Item.Delete();
            }
        }

    }
}
