using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.API.PrimitiveSharedVariables;

namespace SimpleTaskPlanner
{
    public class HeadHW
    {
        private double pan;
        private double tilt;

        public HeadHW(double maxPan, double minPan, double minTilt)
        {
			this.MaxPan = maxPan;
			this.MinPan = minPan;
            this.MinTilt = minTilt;
        }

        public double Pan
        { get { return this.pan; } }

        public double Tilt
        { get { return this.tilt; } }

		public double MaxPan
		{ get ; set;}

		public double MinPan
		{ get; set; }

        public double MinTilt
        { get; set; }


        public bool ActualizeHeadPosition(double[] panAndTiltAngles)
        {
			if ((panAndTiltAngles == null) || (panAndTiltAngles.Length != 2))
			{
				TBWriter.Error("Can't ActualizeHeadPosition, null value or invalid lenght");
				return false;
			}
			else
			{
				pan = panAndTiltAngles[0];
				tilt = panAndTiltAngles[1];

				TBWriter.Write(9, "Actualized headPos [ pan=" + pan.ToString("0.00") + " , tilt=" + tilt.ToString("0.00") + " ]");

				return true;
			}
        }
    }
}
