using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Robotics.Controls;
using Robotics.Mathematics;
using System.Threading;
using Robotics.API;
using System.IO; 

namespace SimpleTaskPlanner
{
    public partial class Form1 : Form
    {
       public STPlanner stplanner;

        public Form1()
        {
            InitializeComponent();
        }
		
        private void Form1_Load(object sender, EventArgs e)
        {
            string buidlDate =" - LastBuild: " + File.GetLastWriteTime(System.Reflection.Assembly.GetExecutingAssembly().Location).ToString("G");
            this.Text += buidlDate;

            string logFileName = "ST-PLN " + DateTime.Now.ToString("yyyy-MM-dd hh.mm.ss") + ".log";
            TextBoxStreamWriter.DefaultLog = new TextBoxStreamWriter(txtConsole, logFileName, 1000);
            TextBoxStreamWriter.DefaultLog.DefaultPriority = 0;
            // To use with newer version of Robotics.dll
            
            TextBoxStreamWriter.DefaultLog.VerbosityThreshold = 6;
            //TextBoxStreamWriter.DefaultLog.TextBoxVerbosityThreshold = 6;

            TBWriter.Spaced(" All mighty Simple Task Planner");

            TextBoxStreamWriter.DefaultLog.WriteLine(buidlDate); 


            // To use with newer version of Robotics.dll.
            nupConsoleThreshold.Value = TextBoxStreamWriter.DefaultLog.VerbosityThreshold;
            //nupConsoleThreshold.Value = TextBoxStreamWriter.DefaultLog.TextBoxVerbosityThreshold;
		}

        private void button2_Click(object sender, EventArgs e)
        {
            stplanner.taskPlanner.Cmd_DoPresentation();
        }

        private void nupConsoleThreshold_ValueChanged(object sender, EventArgs e)
        {
            // To use with newer version of Robotics.dll.
            TextBoxStreamWriter.DefaultLog.VerbosityThreshold = (int)nupConsoleThreshold.Value;
            //TextBoxStreamWriter.DefaultLog.TextBoxVerbosityThreshold = (int)nupConsoleThreshold.Value;
        }

		public Human pfAutoHuman
		{
			set { pfAutoHuman = new Human(value.Name, value.Head); }
			get { return this.pfAutoHuman; }
		}

        private void btnTest_Click(object sender, EventArgs e)
        {
			Vector3[] array = new Vector3[100000];

			Vector3[] arrayCopy;
			Vector3[] arrayclone;
			Vector3[] arrayToArray;
			Vector3[] arrayFor;

			Random ran = new Random();
			DateTime initialTime;

			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new Vector3(ran.NextDouble(), ran.NextDouble(), ran.NextDouble());
			}

			initialTime = DateTime.Now;
			array.CopyTo(arrayCopy= new Vector3[array.Length], 0);
			TBWriter.Info1("array.CopyTo TIME : " + DateTime.Now.Subtract(initialTime).TotalMilliseconds);

			initialTime = DateTime.Now;
			arrayclone = (Vector3[])array.Clone();
			TBWriter.Info1("(int[])array.Clone() TIME : " + DateTime.Now.Subtract(initialTime).TotalMilliseconds);

			initialTime = DateTime.Now;
			arrayToArray = array.ToArray();
			TBWriter.Info1("array.ToArray() TIME : " + DateTime.Now.Subtract(initialTime).TotalMilliseconds);

			initialTime = DateTime.Now;
			arrayFor = new Vector3[array.Length];
			for (int i = 0; i < array.Length; i++)
				arrayFor[i] = new Vector3(array[i]);
			TBWriter.Info1("arrayFor TIME : " + DateTime.Now.Subtract(initialTime).TotalMilliseconds);


			TBWriter.Info1("array[1] = " + array[1].ToString());
			
			TBWriter.Info1("arrayCopy[1] = " + arrayCopy[1].ToString());
			arrayCopy[1].X = (ran.NextDouble());
			arrayCopy[1].Y = (ran.NextDouble());
			arrayCopy[1].Z = (ran.NextDouble());
			TBWriter.Info1("arrayCopy[1] = " + arrayCopy[1].ToString());
			TBWriter.Info1("array[1] = " + array[1].ToString());

			TBWriter.Info1("arrayclone[1] = " + arrayclone[1].ToString());
			TBWriter.Info1("arrayToArray[1] = " + arrayToArray[1].ToString());
			TBWriter.Info1("arrayFor[1] = " + arrayFor[1].ToString());

			//SceneMaTBWriter.Info1("arrayToArray[1] = " + arrayToArray[1].ToString());nager map = new SceneManager
            //();

            //map.AddNode( new SceneNode( "myNode" , Vector3.UnitX));

            //map.Nodes[0].AddFrame( 0, 0, 0.8);
            //map.Nodes[0].AddFrame(1, 11, 0.8);

            //map.Nodes.Add( new SceneNode( "myAnotherNode" , new Vector3(2, 2, 2)));

            //map.Nodes[1].AddFrame(2, 3, 1.8);
            //map.Nodes[1].AddFrame(2,1, 0);
            //map.Nodes[1].AddFrame(0, 0, 0);
            
            //map.SaveSceneMap();

        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            //SceneMap map = new SceneMap();
            //map.LoadSceneMap();
        }

        private void btnMove_Click(object sender, EventArgs e)
        {
            if (stplanner.taskPlanner.learnedArmMotionsList.Keys.Count > 0)
                stplanner.taskPlanner.Cmd_ExecuteLearn(stplanner.taskPlanner.learnedArmMotionsList.Keys[0], "right");
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

		private void btnExecute_Click(object sender, EventArgs e)
		{
			return;
			this.stplanner.taskPlanner.Cmd_AlignWithEdge(0.0, 0.15);
		}

		private void cmbUseArm_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.stplanner.taskPlanner.ArmToUse = (useArms)this.cmbUseArm.SelectedIndex;
		}

    }
}
