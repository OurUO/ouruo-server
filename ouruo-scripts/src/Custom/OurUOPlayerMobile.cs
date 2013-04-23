///////////////////////////////////////////////////////////////////////////////////////
//                                                                                   //
//	This is OurUO Tech Demo, for more info on this project, please refer to:         //
//	                                                                                 //
// 	https://github.com/OurUO/ouruo-server                                            //
// 		                                                                             //
// 	Copyright (C) 2013  Developer Riker (https://github.com/dev-riker)               //
// 	Copyright (C) 2013  Developer Sebastien (https://github.com/devsebastien)        //
//                                                                                   //
// 	This program is free software; you can redistribute it and/or                    //
// 	modify it under the terms of the GNU General Public License                      //
// 	as published by the Free Software Foundation; either version 2                   //
// 	of the License, or (at your option) any later version.                           //
//                                                                                   //
// 	This program is distributed in the hope that it will be useful,                  //
// 	but WITHOUT ANY WARRANTY; without even the implied warranty of                   //
// 	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the                    //
// 	GNU General Public License for more details.                                     //
//                                                                                   //
// 	You should have received a copy of the GNU General Public License                //
// 	along with this program; if not, write to the Free Software                      //
// 	Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.  //
//                                                                                   //
///////////////////////////////////////////////////////////////////////////////////////

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
