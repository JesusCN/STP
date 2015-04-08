using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.Mathematics;
using Robotics.Controls;
using System.Xml.Serialization;
using System.Xml;
using System.Windows.Forms;
using System.IO;

namespace SimpleTaskPlanner
{
	public class Robot
	{
		#region Variables

		public SortedList<string, Module> modules;

		public SensorsManager sensorsMan;
		public HardwareManager hardwareMan;

		public HeadHW head;
		public TorsoHW torso;
		
		private Skeleton[] skeletons;
		private List<Human> humanList;
		public SortedList<string, WorldObject> objectsList;
		public SortedList<string, WorldObject> lastObjectsFounded;
        
        public List<SceneNode> sceneNodes;

        string sceneNodesFilePath = Application.StartupPath + "\\sceneNodes.xml";
        
        #endregion

		public Robot(double centerToFrontDist)
		{
			sensorsMan = new SensorsManager();
			hardwareMan = new HardwareManager();

			modules = new SortedList<string, Module>();

			head = new HeadHW(1, -1, -1.1);
			torso = new TorsoHW(MathUtil.PiOver2, -MathUtil.PiOver2);

			RightArmIsEmpty = true;
			LeftArmIsEmpty = true;

			sceneNodes = new List<SceneNode>();

			skeletons = new Skeleton[0];
			humanList = new List<Human>();
			objectsList = new SortedList<string, WorldObject>();
			lastObjectsFounded = new SortedList<string, WorldObject>();

			this.CenterToFrontDist = centerToFrontDist;

			LoadModules();

			double [] asd = new double[]{0.0, 1.5708};
			this.head.ActualizeHeadPosition(asd);
			Vector3 t = TransHeadKinect2Robot(new Vector3(0.0, 1.0, 1.0));

			Vector3 a = TransLeftArm2Robot(new Vector3(0.0, 0.0, 1.0));
		}

        public double CenterToFrontDist
        { get; set; }

		#region Object List

        public void LastObjFoundedClear()
		{
            lastObjectsFounded.Clear();
		}

		public void LastObjFounded_Add(WorldObject obj)
		{
			int count = 0;

            if (obj.Name == "unknown")
            {
                foreach (string name in lastObjectsFounded.Keys)
                {
                    if (name.Contains("unknown"))
                        count++;
                }

                lastObjectsFounded.Add(obj.Name + count.ToString(), new WorldObject(obj.Name+count.ToString(), obj.Position, obj.DistanceFromTable ));

                return;
            }

			if (lastObjectsFounded.ContainsKey(obj.Name))
				lastObjectsFounded.RemoveAt(lastObjectsFounded.IndexOfKey(obj.Name));

			lastObjectsFounded.Add(obj.Name, obj);
			return;
		}

		#endregion

		#region MobilBase Methods

		public string Region
		{ get { return this.hardwareMan.MobileBase.Region; } }

		public double Orientation
		{ get { return this.hardwareMan.MobileBase.Orientation; } }

		public Vector3 Position
		{ get { return this.hardwareMan.MobileBase.Position; } }

		#endregion

		#region Arms Methods

		public Vector3 RightArmPosition
		{ get; set; }

		public bool RightArmIsEmpty
		{ get; set; }

		public Vector3 LeftArmPosition
		{ get; set; }

		public bool LeftArmIsEmpty
		{ get; set; }

		#endregion
		
		#region Human Methods

		public int HumanCount
			{ get { return this.humanList.Count; } }

		public Human LastHuman
			{
				get
				{
					if (humanList.Count < 1)
						return null;
					else
						return humanList[humanList.Count - 1];
				}
			}

		public Human Human(string humanName)
			{
				foreach (Human h in humanList)
					if (h.Name == humanName) return h;

				return null;
			}

		public Human Human(int index)
			{
				if ((0 <= index) && (index < humanList.Count))
					return this.humanList[index];
				else
					throw new ArgumentOutOfRangeException("Index");
			}

		public void RememberHuman(Human human)
			{
				if (human == null)
					throw new ArgumentNullException("human");

				if (this.Human(human.Name) != null)
					humanList.Remove(this.Human(human.Name));

				humanList.Add(human);
			}
		
		public void ForgetHuman(string humanName)
			{
				if (this.Human(humanName) != null)
					humanList.Remove(this.Human(humanName));

				return;
			}

		#endregion

		#region Skeletons Methods

		public Skeleton[] Skeletons
		{
			get
			{
				if (sensorsMan.Skeletons == null)
					return this.skeletons;

				this.skeletons = new Skeleton[sensorsMan.Skeletons.Length];

				for (int i = 0; i < sensorsMan.Skeletons.Length; i++)
				{
					Vector3 newPos = TransChestKinect2Robot(sensorsMan.Skeletons[i].Head);

					skeletons[i] = new Skeleton(sensorsMan.Skeletons[i].ID, newPos);
				}

				return this.skeletons;
			}
		}

		#endregion

		#region Modules Methods

		public void LoadModules()
		{
			modules = new SortedList<string, Module>(20);

			Module ActionPln = new Module(Module.ActionPln);
			modules.Add(ActionPln.Name, ActionPln);

			Module MovingPln = new Module(Module.MovingPln);
			modules.Add(MovingPln.Name, MovingPln);

			Module Torso = new Module(Module.Torso);
			modules.Add(Torso.Name, Torso);

			Module Arms = new Module(Module.Arms);
			modules.Add(Arms.Name, Arms);

			Module Sensors = new Module(Module.Sensors);
			modules.Add(Sensors.Name, Sensors);

			Module SpGenerator = new Module(Module.SpGenerator);
			modules.Add(SpGenerator.Name, SpGenerator);

			Module SpRecorder = new Module(Module.SpRecorder);
			modules.Add(SpRecorder.Name, SpRecorder);

			Module PersonFnd = new Module(Module.PersonFnd);
			modules.Add(PersonFnd.Name, PersonFnd);

			Module HumanFnd = new Module(Module.HumanFnd);
			modules.Add(HumanFnd.Name, HumanFnd);

			Module ObjectFnd = new Module(Module.ObjectFnd);
			modules.Add(ObjectFnd.Name, ObjectFnd);

			Module Head = new Module(Module.Head);
			modules.Add(Head.Name, Head);

			Module Cartographer = new Module(Module.Cartographer);
			modules.Add(Cartographer.Name, Cartographer);
		}

		public bool ActualizeConnectedModules(string SharedVar)
		{
			if (SharedVar == null)
			{
				TBWriter.Write("ERROR : Can't actualize CONNECTED modules, null string");
				return false;
			}

			foreach (string modName in modules.Keys)
			{
				if (SharedVar.Contains(modName))
				{
					if (!modules[modName].IsConnected)
					{
						modules[modName].IsConnected = true;
						TBWriter.Write(5, "        Module " + modName + " is now connected ");
					}
				}
				else
					if (modules[modName].IsConnected)
					{
						modules[modName].IsConnected = false;
						TBWriter.Write(3, "    >   Warning : module " + modName + " is disconnected");
					}
			}
			TBWriter.SharedVarAct("Actualized connected Shared Var");


			return true;
		}

		#endregion

		#region Transfomrs

		public Vector3 TransNeck2Robot(Vector3 vector)
		{
			return new Vector3(robot2neckMatrix().Transform(vector));
		}

		public Vector3 TransRobot2Neck(Vector3 vector)
		{
			HomogeneousM neck2robotMatrix = new HomogeneousM(robot2neckMatrix().Inverse);
			return new Vector3(neck2robotMatrix.Transform(vector));
		}

		public Vector3 TransMinoru2Robot(Vector3 vector)
		{
			HomogeneousM robot2minoruMatrix = new HomogeneousM(robot2neckMatrix().Matrix * neck2minoruMatrix().Matrix);
			return new Vector3(robot2minoruMatrix.Transform(vector));
		}

		public Vector3 TransRightArm2Robot(Vector3 vector)
		{
			HomogeneousM robot2arm = new HomogeneousM(robot2neckMatrix().Matrix * neck2RightArmMatrix().Matrix);
			return robot2arm.Transform(vector);
		}

		public Vector3 TransLeftArm2Robot(Vector3 vector)
		{
			HomogeneousM robot2leftArm = new HomogeneousM(robot2neckMatrix().Matrix * neck2LeftArmMatrix().Matrix);
			return robot2leftArm.Transform(vector);
		}

		public Vector3 TransRobot2RightArm(Vector3 vector)
		{
			HomogeneousM arm2neck = new HomogeneousM(neck2RightArmMatrix().Inverse);
			HomogeneousM neck2robot = new HomogeneousM(robot2neckMatrix().Inverse);

			HomogeneousM arm2robot = new HomogeneousM(arm2neck.Matrix * neck2robot.Matrix);

			return arm2robot.Transform(vector);
		}

		public Vector3 TransRobot2LeftArm(Vector3 vector)
		{
			HomogeneousM leftArm2neck = new HomogeneousM(neck2LeftArmMatrix().Inverse);
			HomogeneousM neck2robot = new HomogeneousM(robot2neckMatrix().Inverse);

			HomogeneousM arm2robot = new HomogeneousM(leftArm2neck.Matrix * neck2robot.Matrix);

			return arm2robot.Transform(vector);
		}

		public Vector3 TransHeadKinect2Robot(Vector3 vector)
		{
			HomogeneousM robot2HeadkinectMatrix = new HomogeneousM(robot2neckMatrix().Matrix * neck2headKinectMatrix().Matrix);
			return robot2HeadkinectMatrix.Transform(vector);
		}

        public Vector3 TransChestKinect2Robot(Vector3 vector)
        {
            HomogeneousM chestKinect2robot = new HomogeneousM(robot2neckMatrix().Matrix * neck2chestKinectMatrix().Matrix);
            return new Vector3(chestKinect2robot.Transform(vector));
        }


		private HomogeneousM robot2neckMatrix()
		{
			HomogeneousM robot2neck;

			HomogeneousM robot2torso;
			HomogeneousM torso2neck;

			double torsoElevation = this.torso.Elevation;
			double torsoPan = this.torso.Pan;

			// -0.107 in X .- wheel To center of the post
			//  0.283 in Z .- floor to base of the post
			robot2torso = new HomogeneousM(0, 0.283, -0.107, 0);
			
			torsoElevation = 0.936; // base of the post to Neck
            torsoPan = 0.0;
            torso2neck = new HomogeneousM(torsoPan, torsoElevation, 0.0, 0);


			return robot2neck = new HomogeneousM(robot2torso.Matrix * torso2neck.Matrix);
		}

		private HomogeneousM neck2minoruMatrix()
		{
			HomogeneousM neck2minoru;

			HomogeneousM neckPan2Tilt;
			HomogeneousM tilt2xTraslation;
			HomogeneousM xTraslation2zTraslation;

			double headPan = this.head.Pan;
			double headTilt = this.head.Tilt;

			// Neck to Head Pan 
			neckPan2Tilt = new HomogeneousM(headPan, .25975, .0417, MathUtil.PiOver2);

			// Head Tilt to Traslation in X ROBOT axis 
			tilt2xTraslation = new HomogeneousM(headTilt, 0, .08135, -MathUtil.PiOver2);

			// X Traslation to Traslation in -Z ROBOT axis
			xTraslation2zTraslation = new HomogeneousM(0, -0.0145, 0, 0);

			return neck2minoru = new HomogeneousM(neckPan2Tilt.Matrix * tilt2xTraslation.Matrix * xTraslation2zTraslation.Matrix);
		}

		private HomogeneousM neck2headKinectMatrix()
		{
			HomogeneousM neck2HeadKinect;

			double headPan = head.Pan;
			double headTilt = head.Tilt;

			HomogeneousM neckPan2Tilt;
			HomogeneousM tilt2xTraslation;
			HomogeneousM xTraslation2zTraslation;
			
			// Neck to Head Pan 
			neckPan2Tilt = new HomogeneousM(headPan, 0.112, 0.0, MathUtil.PiOver2);
			// Head Tilt to Traslation in X ROBOT axis 
			tilt2xTraslation = new HomogeneousM(headTilt, 0, 0.0337, -MathUtil.PiOver2);
			// X Traslation to Traslation in -Z ROBOT axis
			xTraslation2zTraslation = new HomogeneousM(-MathUtil.PiOver2, 0.052, 0, -MathUtil.PiOver2);
			return neck2HeadKinect = new HomogeneousM(neckPan2Tilt.Matrix * tilt2xTraslation.Matrix * xTraslation2zTraslation.Matrix);

			//HomogeneousM transNeckToServo = new HomogeneousM(0.0, 0.0, 0.0, 0.0, 0.0, 0.112);
			//HomogeneousM rotations = new HomogeneousM(headPan, headTilt, 0.0, 0.0, 0.0, 0.0);
			//HomogeneousM transServoToKnct = new HomogeneousM(0.0, 0.0, 0.0, 0.0337, 0.0, 0.052);
			//HomogeneousM rotToKnctRef = new HomogeneousM(-MathUtil.PiOver2, 0.0, -MathUtil.PiOver2, 0.0, 0.0, 0.0);

			//neck2HeadKinect = new HomogeneousM( transNeckToServo.Matrix * 
			//                                    rotations.Matrix * 
			//                                    transServoToKnct.Matrix * 
			//                                    rotToKnctRef.Matrix);

			return neck2HeadKinect;
		}

        private HomogeneousM neck2chestKinectMatrix()
        {
            HomogeneousM neck2ChestKinect;

			double angle = MathUtil.ToRadians(56);

            HomogeneousM neck2rot = new HomogeneousM(0.0, -0.02, 0.05, MathUtil.PiOver2);
            HomogeneousM rot2edgeKnct = new HomogeneousM(-angle, 0.0, 0.09, -MathUtil.PiOver2);
            HomogeneousM edgeKnct2refKnct = new HomogeneousM(-MathUtil.PiOver2, 0.0, 0.0, -MathUtil.PiOver2);
            neck2ChestKinect = new HomogeneousM(neck2rot.Matrix * rot2edgeKnct.Matrix * edgeKnct2refKnct.Matrix);

			return neck2ChestKinect;
        }

		private HomogeneousM neck2chestKinectMatrix_OLD()
		{
			HomogeneousM neck2ChestKinect;
			HomogeneousM xTranslation;
			HomogeneousM zTranslation;

			// Only translation in X and -Z robot axis
			//xzTranslation = new HomogeneousM(0, -0.08175, 0.0479, 0);

			xTranslation = new HomogeneousM(0, 0, 0.0479, 0);
			zTranslation = new HomogeneousM(MathUtil.PiOver2, -0.08175, 0, MathUtil.PiOver2);

			return neck2ChestKinect = new HomogeneousM(xTranslation.Matrix * zTranslation.Matrix);
		}

		private HomogeneousM neck2RightArmMatrix_OLD()
		{
			HomogeneousM neck2armMatrix;

			HomogeneousM translationY;
			HomogeneousM translationX;
			HomogeneousM translationZ;

			translationY = new HomogeneousM(MathUtil.PiOver2, 0, -0.175, MathUtil.PiOver2);
			translationX = new HomogeneousM(-MathUtil.PiOver2, -0.027, 0, MathUtil.PiOver2);
			translationZ = new HomogeneousM(0, 0, 0.0974, 0);

			neck2armMatrix = new HomogeneousM(translationY.Matrix * translationX.Matrix * translationZ.Matrix);

			return neck2armMatrix;
		}

		private HomogeneousM neck2RightArmMatrix()
		{
			HomogeneousM neck2rightArmMatrix;
			HomogeneousM translation = new HomogeneousM(0.0, 0.0, 0.0, 0.045, -0.213, -0.117);
			HomogeneousM rotation = new HomogeneousM(0.0, MathUtil.PiOver2, MathUtil.PiOver2, 0.0, 0.0, 0.0);
			neck2rightArmMatrix = new HomogeneousM(translation.Matrix * rotation.Matrix);
			return neck2rightArmMatrix;
		}

		private HomogeneousM neck2LeftArmMatrix()
		{
			HomogeneousM neck2leftArmMatrix;
			HomogeneousM translation = new HomogeneousM(0.0, 0.0, 0.0, 0.045, 0.213, -0.117);
			HomogeneousM rotation = new HomogeneousM(0.0, MathUtil.PiOver2, MathUtil.PiOver2, 0.0, 0.0, 0.0);
			neck2leftArmMatrix = new HomogeneousM(translation.Matrix * rotation.Matrix);
			return neck2leftArmMatrix;
		}

		private HomogeneousM robot2chestKinectMatrix()
		{
			HomogeneousM robot2chestNeck = new HomogeneousM(this.robot2neckMatrix().Matrix * neck2chestKinectMatrix().Matrix);
			return robot2chestNeck;
		}


		#endregion
        
        public void SaveScenenNodeList(string filePath)
        {
            TBWriter.Info4("Saving ScenenNodeList ...");

            XmlSerializer serializer;

            FileStream fs = File.Open(filePath, FileMode.Create);

            serializer = new XmlSerializer(typeof(SceneNode[]));
            serializer.Serialize(fs, sceneNodes.ToArray());
            fs.Close();

            TBWriter.Info2("Saved SceneNodeList in " + filePath);
        }

        public void SaveScenenNodeList()
        {
            SaveScenenNodeList(sceneNodesFilePath);
        }

        public bool LoadSceneNodeList(string filePath)
        {
            TBWriter.Info8("Loading SceneNodeList ...");

            SceneNode[] sceneNodeAray;
            XmlSerializer serializer;

            if (!File.Exists(filePath))
            {
                TBWriter.Error("Can't SceneNodes file : " + filePath);
                return false;
            }

            FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read);
            serializer = new XmlSerializer(typeof(SceneNode[]));
            sceneNodeAray = (SceneNode[])serializer.Deserialize(fs);
            fs.Close();

            sceneNodes.Clear();

            sceneNodes.AddRange(sceneNodeAray);

            TBWriter.Info1("Successfully loaded ScenenNodeList " + filePath + " . NumberOfNodes = " + sceneNodeAray.Length);

            return true;
        }

        public bool LoadSceneNodeList()
        {
            return LoadSceneNodeList(sceneNodesFilePath);
        }
    }
}