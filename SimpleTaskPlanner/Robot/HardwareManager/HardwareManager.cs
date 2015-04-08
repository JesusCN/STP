using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleTaskPlanner
{
    public class HardwareManager
    {
        public HardwareManager()
        {
			MobileBase = new MobilBaseHW();
            RightArm = new Arm();
			LeftArm = new Arm();
            
			//Head = new HeadHW();
            //Torso = new TorsoHW();
        }
		
        public MobilBaseHW MobileBase
        { get; set; }

        public Arm RightArm
        { get; set; }

		public Arm LeftArm
		{ get; set; }

		//public HeadHW Head
		//{ get;set;  }

		//public TorsoHW Torso
		//{ get; set; }
    }
}
