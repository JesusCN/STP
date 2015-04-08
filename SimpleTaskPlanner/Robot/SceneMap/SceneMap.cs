using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using Robotics.Mathematics;

namespace SimpleTaskPlanner
{
	public class SceneMap
	{
		SortedList<string, SceneNode> nodesList;
		string sceneMapfile;

		public SceneMap()
		{
			this.nodesList = new SortedList<string, SceneNode>();
			this.sceneMapfile = "imageMap.xml";
		}

		public string FileSceneMap
		{
			get { return this.sceneMapfile; }
		}
		
		public bool AddNode(SceneNode node)
		{
			if (nodesList.ContainsKey(node.Name))
				return false;
			else
				nodesList.Add(node.Name, node);

			return true;

		}

		public bool AddNode(string nodeName, Vector3 nodeLocation)
		{
			if (this.nodesList.ContainsKey(nodeName))
				return false;
			else
				this.nodesList.Add(nodeName, new SceneNode(nodeName, nodeLocation));

			return true;
		}

		public void DeleteNode(string nodeName)
		{
			if (this.nodesList.ContainsKey(nodeName))
				this.nodesList.Remove(nodeName);
		}

		public bool ContainsNode(string nodeName)
		{
			return this.nodesList.ContainsKey(nodeName);
		}

		public bool AddSceneFrame(string nodeName, SceneFrame frame)
		{
			if (!nodesList.ContainsKey(nodeName))
				return false;
			else
				nodesList[nodeName].AddFrame(frame);

			return true;
		}

		public Vector3 GetNodeLocation(string nodeName)
		{
			Vector3 nodeLocation = null;

			if (nodesList.ContainsKey(nodeName))
				nodeLocation = new Vector3(nodesList[nodeName].Location);

			return nodeLocation;
		}

		public SceneFrame[] GetNodeFrames(string nodeName)
		{
			SceneFrame[] nodeFrames = null;

			if (nodesList.ContainsKey(nodeName))
				nodeFrames = nodesList[nodeName].Frames.Clone() as SceneFrame[];

			return nodeFrames;
		}	

		public bool SaveSceneMap(string filePath)
		{
			TBWriter.Info4("Saving SceneMap ...");

			XmlSerializer serializer;
			FileStream fs = File.Open(filePath, FileMode.Create);
			serializer = new XmlSerializer(typeof(SceneNode[]));
			serializer.Serialize(fs, nodesList.Values.ToArray());
			fs.Close();

			TBWriter.Info4("SceneMap saved in "+ filePath);

			return true;
		}

		public bool SaveSceneMap()
		{
			return this.SaveSceneMap(sceneMapfile);
		}

		public bool LoadSceneMap(string filePath)
		{
			TBWriter.Info4("Loading SceneMap file ["+filePath+"] ...");

			if (!File.Exists(filePath))
			{
				TBWriter.Error("SceneMap file ["+filePath+"] doesn't exist");
				return false;
			}

			FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read);
			XmlSerializer serializer = new XmlSerializer(typeof(SceneNode[]));
			SceneNode[] tempNodeArray = (SceneNode[])serializer.Deserialize(fs);
			fs.Close();
			
			this.nodesList.Clear();
			foreach (SceneNode node in tempNodeArray)
				this.nodesList.Add(node.Name, node);

			this.sceneMapfile = filePath;
			
			TBWriter.Info4("SceneMap loaded successfully");
			return true;
		}

		public bool LoadSceneMap()
		{
			return this.LoadSceneMap(this.sceneMapfile);
		}
	}
}