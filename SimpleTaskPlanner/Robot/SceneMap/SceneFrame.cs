using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
 
namespace SimpleTaskPlanner
{
	[Serializable]
	[XmlRoot("sceneFrame", IsNullable = false)]
	public class SceneFrame
	{
		public SceneFrame()
		{ }

		public SceneFrame(double orientation, double headTilt, double torsoElevation)
		{
			this.Orientation = orientation;
			this.HeadTilt = headTilt;
			this.TorsoElevation = torsoElevation;
		}

		[XmlAttribute("orientation")]
		public double Orientation
		{ get; set; }

		[XmlAttribute("headTilt")]
		public double HeadTilt
		{ get; set; }

		[XmlAttribute("torsoElevation")]
		public double TorsoElevation
		{ get; set; }


		public override string ToString()
		{
			return	"Frame[" +	
								"Or{" + Orientation.ToString("0.00") + "}" +
								" HdTilt{" + HeadTilt.ToString("0.00") + "}" +
								" TrsElev{" + TorsoElevation.ToString("0.00") + "}" +
								"]";
		}
	}
}
