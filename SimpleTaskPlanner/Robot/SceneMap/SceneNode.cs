using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.Mathematics;
using System.Xml.Serialization;

namespace SimpleTaskPlanner
{
    [Serializable] 
    [XmlRoot("SceneNode", IsNullable = false)]
    public class SceneNode
    {
        List<SceneFrame> frames;

        public SceneNode(string name, Vector3 location)
        {
            this.Name = name;
            this.Location = new Vector3(location);
            this.frames = new List<SceneFrame>();
        }

		public SceneNode()
		{ }

        [XmlAttribute( "name")]
        public string Name
        { get; set; }

        [XmlElement( "location", IsNullable = false)]
        public Vector3 Location
        { get; set; }

        [XmlArray("frames", IsNullable =false)]
        public SceneFrame[] Frames
        {
            get
            {
                return this.frames.ToArray();
            }
            set
            {
                if ((value == null) && (value.Length < 1)) 
                    return;
                else
                    frames = new List<SceneFrame>(value.ToList());
            }
        }

        public void AddFrame(SceneFrame frame)
        {
            if (this.frames == null)
                this.frames = new List<SceneFrame>();

            frames.Add(frame);
        }

        public void DeleteFrame(int index)
        {
            if ((this.frames!=null)&&(index > this.frames.Count))
                return;
            else
                frames.RemoveAt(index);
        }

		public override string ToString()
		{
			return "Node[" + "name{" + Name + " }" +
							" loc{" + Location.X.ToString("0.00") + ", " + Location.Y.ToString("0.00") + "}" + 
						"]";
		}
	}
}
