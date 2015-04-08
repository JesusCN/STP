using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.Mathematics; 

namespace SimpleTaskPlanner
{
    public class Skeleton
    {
        private int id;
        private Vector3 head;

        public Skeleton(int id, Vector3 head)
        {
            this.id = id;
            this.head = new Vector3(head);
        }
        public int ID
        { get { return this.id; } }

        public Vector3 Head
        { get { return this.head; } }

		public override string ToString()
		{
			return "Skeleton: ID=" + ID.ToString() + "; Head=" + head.ToString(); 
		}
    }
}
