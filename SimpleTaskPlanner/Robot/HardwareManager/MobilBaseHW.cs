using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.Mathematics;
using Robotics.API.PrimitiveSharedVariables;
using Robotics.API.MiscSharedVariables;

using Robotics.API;
using Robotics.Controls;

namespace SimpleTaskPlanner
{
    public class MobilBaseHW
    {
		private string region;
		private double orientation;
		private Vector3 position;
     
  
		public MobilBaseHW()
        {
        }

		public string Region
		{ get { return this.region; } }

        public Vector3 Position
        { get { return this.position; } }

        public double X
        { get { return this.position.X; } }

        public double Y
        { get { return this.position.Y; } }

        public double Z
        { get { return this.position.Z; } }

        public double Orientation
        { get { return this.orientation; } }

		public bool ActualizeOdometryPosition(double[] positionAndOrientation)
		{
			if ((positionAndOrientation == null) || (positionAndOrientation.Length != 3))
			{
				TBWriter.Warning1("Can't Actualize OdometryPos, value is null or invalid lenght");
				return false;
			}
			else
			{
				this.position = new Vector3();

				this.position.X = positionAndOrientation[0];
				this.position.Y = positionAndOrientation[1];
				this.orientation = positionAndOrientation[2];

				TBWriter.Write(9, "Actualized OdometryPos [ position=" + position.ToString() + " , orientation=" + orientation.ToString("0.00") + " ]");
				return true;
			}
		}

		public bool ActualizeOdometryPosition(string parameters)
		{
			bool success = Parse.OdometryPosition(parameters, out this.position, out this.orientation);

			if (success)
				TBWriter.SharedVarAct("Actualized odometryPosition [ position=" + this.position.ToString() + ", orientation= " + this.orientation + " ]");
			else
				TBWriter.Warning1("Can't actulize odometry");

			return success;
		}


		public bool ActualizeRobotRegion(string robotRegion)
		{
			if (string.IsNullOrEmpty(robotRegion))
			{
				TBWriter.Warning1(" Cant actualize robotRegion, string is null or empty");
				return false;
			}
			else
			{
				this.region = robotRegion;
				TBWriter.SharedVarAct("Actualized shared variable; robotRegion [ " + this.region+ " ]");
				return true;
			}
		}
    }
}
