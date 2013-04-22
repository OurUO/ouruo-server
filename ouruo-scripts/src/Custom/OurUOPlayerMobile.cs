using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
using Server.Items;

namespace Server.Mobiles
{
    public class OurUOPlayerMobile : PlayerMobile
    {
        private string divinity_;

        [CommandProperty(AccessLevel.GameMaster)]
        public string Divinity
        {
            get { return divinity_; }
            set { divinity_ = value; }
        }
        public OurUOPlayerMobile(Serial s)
            : base(s)
        {
        }
        public OurUOPlayerMobile()
            : base()
        {

        }
        public override void OnDeath(Container c)
        {
            Point3D sanctuary = new Point3D(5438, 1154, 0);
            SetLocation(sanctuary, true);

            base.OnDeath(c);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }
    }
}
