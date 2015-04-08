using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.Mathematics;

namespace SimpleTaskPlanner
{
    public class ArmMotion
    {
        string name;
        bool rightArm;



        public ArmMotion(string name, bool rightArm)
        {
            this.name = name;
            this.rightArm = rightArm;
            this.UseSpin = false;
        }

        public string Name
        {
            get { return this.name; }
        }

        public bool RightArm
        {
            get { return this.rightArm; }
            set { rightArm = value; }
        }

        public bool UseSpin
        {
            get;
            set;
        }

        public int SpinStartIndex
        {
            get;
            set;
        }

        public int SpinStopIndex
        {
            get;
            set;
        }

        public Vector3[] RightArmPositions
        {
            get;
            set;
        }

        public Vector3[] LeftArmPositions
        {
            get;
            set;
        }
    }
}