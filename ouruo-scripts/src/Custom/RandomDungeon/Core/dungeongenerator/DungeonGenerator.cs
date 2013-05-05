namespace Server.Engines.RandomDungeon
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Diagnostics;
    using Server.Multis;
    using iRogue;
    using randomMap = iRogue.Map;

    public class DungeonGenerator : BaseCamp
    {
        public string m_roomfile;
        private Room prefabRoom;

        [Constructable]
        public DungeonGenerator(): base(0x10EE) // dummy garbage at center
		{
        }

        public DungeonGenerator(string roomfile): this()
        {
            m_roomfile = roomfile;
            prefabRoom = new Room(m_roomfile);
        }

        public override void AddItem(Item item, int xOffset, int yOffset, int zOffset)
        {
            if (item != null)
                item.Movable = false;

            base.AddItem(item, xOffset, yOffset, zOffset);
        }

		public override void AddComponents()
		{
            for (int x = 0; x < prefabRoom.RoomWidth; x++)
            {
                for (int y = 0; y < prefabRoom.RoomHeight; y++)
                {
                    switch (prefabRoom.RoomCells[x,y])
                    {
                        case 1:
                            AddItem( new Item( 0x0080 ), x, y, 0 ); // stonewall
                            break;
                        case 2:
                            AddItem(new Item(0x0514), x, y, 0); // marble floor
                            break;
                        default:
                            break;
                    }    
                }
            } 
		}

        public DungeonGenerator(Serial serial) : base(serial)
		{
		}

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }

    }
}

