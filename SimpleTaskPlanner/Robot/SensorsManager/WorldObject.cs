using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.Mathematics;

namespace SimpleTaskPlanner
{
    public class WorldObject
    {
        private string name;

        public WorldObject(string name, Vector3 position)
        {
            this.name = name;
            this.Position = new Vector3(position);
            this.DistanceFromTable = 0.8;
		}

        public WorldObject(string name, Vector3 position, double distanceFromTable)
        {
            this.name = name;
            this.Position = new Vector3(position);
            this.DistanceFromTable = distanceFromTable;
        }

        public WorldObject(WorldObject obj)
        {
            this.name = obj.Name;
            this.Position = new Vector3(obj.Position);
            this.DistanceFromTable = obj.DistanceFromTable;
        }

        public string Name
        { get { return this.name; } }

        public Vector3 Position
        { get; set; }

        public double DistanceFromTable
        { get; set; }

        public override string ToString()
        {
            return Name + " located in " + Position.ToString();
        }
    }
}
