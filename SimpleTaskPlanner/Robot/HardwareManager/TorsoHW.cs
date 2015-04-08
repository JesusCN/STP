using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.Mathematics;

namespace SimpleTaskPlanner
{
    public class TorsoHW
    {
        double pan;
        double elevation;

		public TorsoHW(double maxPan, double minPan )
		{
			this.pan = 0.0;
			this.elevation = 0.8;
			this.MaxPan = maxPan;
			this.MinPan = minPan;
		}

		public double MaxPan
		{ get; set; }

		public double MinPan
		{ get; set; }

        public double Pan
        { get { return this.pan; } }

        public double Elevation
        { get { return this.elevation; } }

        public bool ActualizeTorsoStatus(double[] sharedVar)
        {
            bool success = Parse.TorsoPosition(sharedVar,out this.elevation, out this.pan);

            if (success)
                TBWriter.Write(7, "        Successfully Actualized Torso Position");
            else
                TBWriter.Write("ERROR : Can't actulize TORSO position");

            return success;
        }

		public const double navigationElevation = 0.17;
		public const double navigationPan = 0;
	}
}
