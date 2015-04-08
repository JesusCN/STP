using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.Mathematics;

namespace SimpleTaskPlanner
{
	public class Arm
	{
		Vector3 position;
		Vector3 orientation;

		private double roll;
		private double pitch;
		private double yaw;
		private double elbow;

		private double gripperAperture;
		private bool gripperOpen;

		public Arm()
		{ }

		public Vector3 Position
		{ get { return this.position; } }

		public Vector3 Orientation
		{ get { return this.orientation; } }

		public double X
		{ get { return this.position.X; } }

		public double Y
		{ get { return this.position.Y; } }

		public double Z
		{ get { return this.position.Z; } }

		public double Roll
		{ get { return this.orientation.X; } }

		public double Pitch
		{ get { return this.orientation.Y; } }

		public double Yaw
		{ get { return this.orientation.Z; } }

		public double Elbow
		{ get { return this.elbow; } }

		public double GripperAperture
		{ get { return this.gripperAperture; } }

		public bool GripperOpen
		{ get { return this.gripperOpen; } }

		public bool ActualizePositionStatus(string armPosition)
		{
			Vector3 armPos;
			Vector3 armOri;
			double elbow;
			bool success;

			success = Parse.ArmPosition(armPosition, out armPos, out armOri, out elbow);

			if (success)
			{
				this.position = new Vector3(armPos);
				this.orientation = new Vector3(armOri);
				this.elbow = elbow;

				TBWriter.Write(7, "Successfully actualized ARM position , orientation and elbow");

				return true;
			}
			else
			{
				TBWriter.Write("ERROR : Can't actualize ARM position, orientation and elbow");
				return false;
			}
		}



		#region Predefined Positions

		public const string ppHome = "home";
		public const string ppNavigation = "navigation";
		public const string ppStandBy = "standby";
		public const string ppHeilHitler = "heilHitler";
		public const string ppShowHead = "showHead";
		public const string ppShowChestKinect = "showChestKinect";
		public const string ppShowHeadKinect = "showHeadKinect";
		public const string ppShowLaser = "showLaser";
		public const string ppShowArm = "showArm";
		public const string ppDeliver = "deliver";

		#endregion

		public const string left = "left";
		public const string right = "right";


	}
}