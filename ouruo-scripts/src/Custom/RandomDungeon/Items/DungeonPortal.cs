using System;
using System.Collections.Generic;
using System.Collections;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Engines.Quests;
using Server.Engines.Quests.Necro;

namespace Server.Engines.RandomDungeon
{
	public class DungeonPortal : DynamicTeleporter
	{
        private int m_portalID;
        private PortalKey m_key;

        [CommandProperty(AccessLevel.GameMaster)]
        public int PortalID { get { return m_portalID; } set { m_portalID = value; } }

        public string NoKey { get { return "No key or valid destination"; } }

		[Constructable]
		public DungeonPortal()
		{
            m_portalID = 1;
		}

        public PortalKey checkkey(PlayerMobile player)
        {
            if (player != null)
            {
                List<Item> items = player.Backpack.Items;
                int count = items.Count;
                for (int i = 0; i < items.Count; ++i)
                {
                    Item item = items[i];
                    if (item is PortalKey)
                    {
                        PortalKey p = (PortalKey)item;
                        if (p.portalID.Contains(this.m_portalID))
                        {
                            return p;
                        }
                    }
                }
            }
			return null;
        }


		public override bool GetDestination( PlayerMobile player, ref Point3D loc, ref Map map )
		{
            m_key = checkkey(player);
            if (player != null && m_key!=null)
            {
                loc= m_key.Destination;
                map = m_key.Map;
                return true;
            }
			return false;
		}

        public override bool OnMoveOver(Mobile m)
        {
            PlayerMobile pm = m as PlayerMobile;

            Point3D loc = Point3D.Zero;
            Map map = null;

            if (pm != null && GetDestination(pm, ref loc, ref map))
            {
                if (m_key is DungeonKey)
                {
                    DungeonKey tempkey = (DungeonKey) m_key;
                    pm.SendMessage(tempkey.Dungeon);

                    DungeonGenerator dungeon = new DungeonGenerator(tempkey.Dungeon);
                    dungeon.MoveToWorld(this.Location, this.Map);
                }
            }
            else
            {
                 pm.SendMessage(this.NoKey);
            }

            return base.OnMoveOver(m);
        }


        public DungeonPortal(Serial serial) : base(serial)
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
            writer.Write((int)m_portalID);
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
            m_portalID = reader.ReadInt();
		}
	}
}