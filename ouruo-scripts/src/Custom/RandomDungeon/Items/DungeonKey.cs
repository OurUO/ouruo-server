using System;
using System.Collections.Generic;
using Server;
using Server.Mobiles;
using Server.Engines.Quests;
using iRogue;
using randomMap = iRogue.Map;

namespace Server.Engines.RandomDungeon
{
    public class DungeonKey : PortalKey
    {
        private string m_dungeon = "";
        private bool m_create = false;
        private int m_minWidth = 40;
        private int m_maxWidth = 100;
        private int m_minHeight = 40;
        private int m_maxHeight = 100;

        private int m_MapWidth = 50;
        private int m_MapHeight = 50;

        [CommandProperty(AccessLevel.GameMaster)]
        public string Dungeon { get { return m_dungeon; } set { m_dungeon = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Create { 
            get { return m_create; } 
            set { 
                m_create = value;
                if(m_create)
                {
                    Creation();
                }
            } 
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int minWidth { get { return m_minWidth; } set { m_minWidth = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int maxWidth { get { return m_maxWidth; } set { m_maxWidth = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int minHeight { get { return m_minHeight; } set { m_minHeight = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int maxHeight { get { return m_maxHeight; } set { m_maxHeight = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MapHeight { get { return m_MapHeight; } set { m_MapHeight = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MapWidth { get { return m_MapWidth; } set { m_MapWidth = value; } }

        [Constructable]
		public DungeonKey() : base()
		{
        }

        public DungeonKey (bool create): base()
        {
            m_create = create;
            Creation();
        }

        public DungeonKey (String dungeon) : base()
        {
            m_dungeon = dungeon;
        }

        public bool Creation()
        {
            if (checkDungeon(m_dungeon))
            {
                randomMap mappa = new randomMap();
                mappa.GenerateLevel();
                m_dungeon = mappa.Filename;
                m_create = false;
                return true;
            }
            return false;
        }

        public bool checkDungeon(String file)
        {
            if (m_MapHeight >= m_minHeight && m_MapHeight <= m_maxHeight && m_MapWidth >= m_minWidth && m_MapWidth <= m_maxWidth)
                return true;
            return false;
        }

        public DungeonKey(Serial serial) : base(serial)
		{
		}

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write(m_dungeon);
            writer.Write(m_MapWidth);
            writer.Write(m_MapHeight);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            m_dungeon = reader.ReadString();
            m_MapWidth = reader.ReadInt();
            m_MapHeight = reader.ReadInt();
        }

    }
}
