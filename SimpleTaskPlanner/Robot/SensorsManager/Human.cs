using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.Mathematics;

namespace SimpleTaskPlanner
{
    public class Human
    {
        private string name;
        private Vector3 headPosition;

        public Human(string name)
        {
            this.name = name;
			this.headPosition = new Vector3(0,0,0);
        }

		public Human(string name, Vector3 posiblePosition)
		{
			this.name = name;
			this.headPosition = new Vector3(posiblePosition);
		}

        public string Name
        {
            get { return this.name; }
        }

		public Vector3 Head
		{
			get { return this.headPosition; }
			set { this.headPosition = new Vector3(value); }
		}




		public override string ToString()
		{
			return "Human [ Name= " + this.name.ToUpper() + " , headPos= " + headPosition.ToString() + " ]";
		}
    }
}
