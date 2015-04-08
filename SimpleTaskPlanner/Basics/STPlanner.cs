using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.API;
using Robotics.Controls;
using Robotics.Mathematics;
using Robotics.API.PrimitiveSharedVariables;
using System.Threading;



namespace SimpleTaskPlanner
{
	public class STPlanner
	{
		public ConnectionManager cnnMan;
		public STPCommandManager cmdMan;
		public TaskPlanner taskPlanner;


		public int connectionPort = 2400;
        bool connectedToBlackboard = false;
        bool sharedVarLoadedFromBlackBoard =false;

        bool sharedVarLoadEvent = false;
        bool successSharedVarSuscription = false;

        bool successSharedVarConnected;
        bool successSharVarHdPos;
        bool successSharVarOdometryPos;
        bool successSharVarSkeletons;
        bool successSharVarTorso;
        bool successSharVarRobotRegion;

		StringSharedVariable sharedVarConnected;
		StringSharedVariable sharedVarRobotRegion;
		StringSharedVariable sharedVarSkeletons;
		DoubleArraySharedVariable sharedVarOdometryPos;
		DoubleArraySharedVariable sharedVarHdPos;
		DoubleArraySharedVariable sharedVarTorso;


		CmdAligneHuman cmdAligneHuman;
		CmdDoPresentation cmdDoPresentation;
		CmdFindHuman cmdFindHuman;
		CmdFindObject cmdFindObject;
        CmdFindOnShelf cmdFindOnshelf;
        CmdTakeFromShelf cmdTakeFromShelf;
		CmdPFAutoDetected cmdPFAutoDetected;
		CmdRememberHuman cmdRememberHuman;
        CmdDeliverObject cmdDeliverObject;
        CmdTakeHandOver cmdTakeHandover;
        CmdTake cmdTake;
        CmdStartLearn cmdStartLearn;
        CmdStopLearn cmdStopLearn;
        CmdExecuteLearn cmdExecuteLearn;
        CmdHolding cmdHolding;
        CmdDrop cmdDrop;
        CmdTakeHumandHands cmdTakeHumanHands;
        CmdDoTechnicalChallenge cmdDoTechnicalChallenge;
        CmdAligneEdge cmdAligneEdge;
        CmdFashionFindObject cmdFashionFindObject;
		CmdTakeTable cmdTakeTable;
        CmdPointAtObject cmdPointAtObject;
		CmdTakeTray cmdTakeTray;
        CmdTakeXYZ cmdTakeXYZ;
        CmdShakeHands cmdShakeHands;
		CmdDetectCubes cmdDetectCubes;
		CmdDropXYZ cmdDropXYZ;

        Thread sharedVarSubscriptionThread;

		public STPlanner()
		{
			TBWriter.Spaced("Simple Task Planner, Version 0.98798");

            TextBoxStreamWriter.DefaultLog.WriteLine(0, "asdasd as asd ");
            TextBoxStreamWriter.DefaultLog.WriteLine(0, "Builded 16/feb/13 , 15:351 p.m.");
            TextBoxStreamWriter.DefaultLog.WriteLine(0, "");

			cmdMan = new STPCommandManager();
			cnnMan = new ConnectionManager(connectionPort, this.cmdMan);

            TBWriter.Write("   Connection Port = " + connectionPort.ToString());

           
            this.cnnMan.ClientConnected += new System.Net.Sockets.TcpClientConnectedEventHandler(cnnMan_ClientConnected);
            TBWriter.Spaced(TBWriter.time() + "      Waiting for Blackboard Connection . . .");

		
			taskPlanner = new TaskPlanner(cmdMan , this);

			LoadCommands();

			sharedVarConnected = new StringSharedVariable("connected");
			sharedVarSkeletons = new StringSharedVariable("skeletons");
			sharedVarOdometryPos = new DoubleArraySharedVariable("odometryPos");
			sharedVarHdPos = new DoubleArraySharedVariable("hd_pos");
			sharedVarTorso = new DoubleArraySharedVariable("torsoPosition");
			sharedVarRobotRegion = new StringSharedVariable("robotRoom");


			//cmdMan.SharedVariablesLoaded += new SharedVariablesLoadedEventHandler(cmdMan_SharedVariablesLoaded);


			cnnMan.Start();
			TBWriter.Write(3, "    >   Connection Manager Started");
			cmdMan.Start();
			TBWriter.Write(3, "    >   Command Manager Started");


            //if(!sharedVarLoadEvent) TBWriter.Spaced("      Waiting for Shared Variables Load from BlackBoard . . .")
        }
 
		private void LoadCommands()
		{
			cmdAligneHuman = new CmdAligneHuman(this.cmdMan, this.taskPlanner);
			cmdMan.CommandExecuters.Add(cmdAligneHuman);

            cmdDeliverObject = new CmdDeliverObject(this.cmdMan, this.taskPlanner);
            cmdMan.CommandExecuters.Add(cmdDeliverObject);

			cmdDoPresentation = new CmdDoPresentation(this.cmdMan, this.taskPlanner);
			cmdMan.CommandExecuters.Add(cmdDoPresentation);

			cmdFindHuman = new CmdFindHuman(this.cmdMan, this.taskPlanner);
			cmdMan.CommandExecuters.Add(cmdFindHuman);

			cmdFindObject = new CmdFindObject(this.cmdMan, this.taskPlanner);
			cmdMan.CommandExecuters.Add(cmdFindObject);

			cmdPFAutoDetected = new CmdPFAutoDetected(this.cmdMan, this.taskPlanner);
			cmdMan.CommandExecuters.Add(cmdPFAutoDetected);

			cmdRememberHuman = new CmdRememberHuman(this.cmdMan, this.taskPlanner);
			cmdMan.CommandExecuters.Add(cmdRememberHuman);

            cmdTakeHandover = new CmdTakeHandOver(this.cmdMan, this.taskPlanner);
            cmdMan.CommandExecuters.Add(cmdTakeHandover);

			cmdTake = new CmdTake(this.cmdMan, this.taskPlanner);
			cmdMan.CommandExecuters.Add(cmdTake);

            cmdFindOnshelf = new CmdFindOnShelf(this.cmdMan, this.taskPlanner);
            cmdMan.CommandExecuters.Add(cmdFindOnshelf);

            cmdTakeFromShelf = new CmdTakeFromShelf(this.cmdMan, this.taskPlanner);
            cmdMan.CommandExecuters.Add(cmdTakeFromShelf);

            cmdStartLearn = new CmdStartLearn(this.cmdMan, this.taskPlanner);
            cmdMan.CommandExecuters.Add(cmdStartLearn);

            cmdStopLearn = new CmdStopLearn(this.cmdMan, this.taskPlanner);
            cmdMan.CommandExecuters.Add(cmdStopLearn);

            cmdExecuteLearn = new CmdExecuteLearn(this.cmdMan, this.taskPlanner);
            cmdMan.CommandExecuters.Add(cmdExecuteLearn);

            cmdHolding = new CmdHolding(this.cmdMan, this.taskPlanner);
            cmdMan.CommandExecuters.Add(cmdHolding);

            cmdDrop = new CmdDrop(this.cmdMan, this.taskPlanner);
            cmdMan.CommandExecuters.Add(cmdDrop);

            cmdTakeHumanHands = new CmdTakeHumandHands(this.cmdMan, this.taskPlanner);
            cmdMan.CommandExecuters.Add(cmdTakeHumanHands);

            cmdDoTechnicalChallenge = new CmdDoTechnicalChallenge(this.cmdMan, this.taskPlanner);
            cmdMan.CommandExecuters.Add(cmdDoTechnicalChallenge);

            cmdAligneEdge = new CmdAligneEdge(this.cmdMan, this.taskPlanner);
            cmdMan.CommandExecuters.Add(cmdAligneEdge);

            cmdFashionFindObject = new CmdFashionFindObject(this.cmdMan, this.taskPlanner);
            cmdMan.CommandExecuters.Add(cmdFashionFindObject);

			cmdTakeTable = new CmdTakeTable(this.cmdMan, this.taskPlanner);
			cmdMan.CommandExecuters.Add(cmdTakeTable);

            cmdPointAtObject = new CmdPointAtObject(this.cmdMan, this.taskPlanner);
            cmdMan.CommandExecuters.Add(cmdPointAtObject);

			cmdTakeTray = new CmdTakeTray(this.cmdMan, this.taskPlanner);
			cmdMan.CommandExecuters.Add(cmdTakeTray);

            cmdShakeHands = new CmdShakeHands(this.cmdMan, this.taskPlanner);
            cmdMan.CommandExecuters.Add(cmdShakeHands);

			cmdDetectCubes = new CmdDetectCubes(this.cmdMan, this.taskPlanner);
			cmdMan.CommandExecuters.Add(cmdDetectCubes);

			cmdTakeXYZ = new CmdTakeXYZ(this.cmdMan, this.taskPlanner);
			cmdMan.CommandExecuters.Add(cmdTakeXYZ);

			cmdDropXYZ = new CmdDropXYZ(this.cmdMan, this.taskPlanner);
			cmdMan.CommandExecuters.Add(cmdDropXYZ);

		}

		#region SharedVar Suscription

        void cnnMan_ClientConnected(System.Net.Sockets.Socket s)
        {

            connectedToBlackboard = true;

            TBWriter.Spaced(TBWriter.time() + "      Connected to Blackboard ");

            sharedVarSubscriptionThread = new Thread(new ThreadStart(sharedVarSubscriptionThreadTask));
            sharedVarSubscriptionThread.IsBackground = true;
            sharedVarSubscriptionThread.Start();
        }
  
        void sharedVarSubscriptionThreadTask()
        {
            bool printConnectedModules = false;

            while (!successSharedVarSuscription)
            {
                if (!sharedVarLoadedFromBlackBoard)
                {
                    TBWriter.Write(9, "Trying to Load SharedVars form Blackboard ");

                    string message;
                    int varsLoaded;

                    varsLoaded = cmdMan.SharedVariables.LoadFromBlackboard(1000, out message);

                    if (varsLoaded > 0)
                    {
                        sharedVarLoadedFromBlackBoard = true;
                        TBWriter.Info1(" Loaded " + varsLoaded.ToString() + " shared Variables : " + message);
                    }
                    else
                    {
                        sharedVarLoadedFromBlackBoard = false;
                        TBWriter.Write(7, "ERROR :  No shared Variables loaded " + message);
                    }

                    continue;
                }

                if (!successSharedVarConnected)
                {
                    successSharedVarConnected = suscribeSharedVarConnected();
                }

                if (!successSharVarHdPos)
                {
                    successSharVarHdPos = suscribeSharVarHdPos();
                }

                if (!successSharVarOdometryPos)
                {
                    successSharVarOdometryPos = suscribeSharVarOdometryPos();
                }

                if (!successSharVarSkeletons)
                {
                    successSharVarSkeletons = suscribeSharVarSkeletons();
                }

                if (!successSharVarTorso)
                {
                    successSharVarTorso = suscribeSharVarTorso();
                }

                if (!successSharVarRobotRegion)
                {
                    successSharVarRobotRegion = suscribeSharVarRobotRegion();
                }

                successSharedVarSuscription = successSharedVarConnected & successSharVarHdPos & successSharVarOdometryPos & successSharVarSkeletons & successSharVarTorso & successSharVarRobotRegion;

                Thread.Sleep(300);

                if (successSharedVarConnected && !printConnectedModules)
				{
					bool success;
					string connectedModules;
					do
					{
						success = sharedVarConnected.TryRead(out connectedModules, 500);
					} while (!success);
					TBWriter.Write(1, String.Empty);
					TBWriter.Write(1, "\tModules Connected :");
					taskPlanner.robot.ActualizeConnectedModules(connectedModules);
					printConnectedModules = true;
				}
            
            }
            TBWriter.Spaced("Simple Task Planner now Ready");
            cmdMan.Ready = true;

            TBWriter.Write(1, "Waiting for Command . . .");
            TBWriter.Write(1, "");
            TBWriter.Write(1, @"\.>");
        }

        private void cmdMan_SharedVariablesLoaded(CommandManager cmdMan)
		{
            if (successSharedVarSuscription)
                return;

			TBWriter.Write(3, "    CmdMan : Shared Variable Loaded succesfully, trying to suscribe . . .");
			
			successSharedVarConnected = suscribeSharedVarConnected();
			successSharVarHdPos = suscribeSharVarHdPos();
			successSharVarOdometryPos = suscribeSharVarOdometryPos();
			successSharVarSkeletons = suscribeSharVarSkeletons();
			successSharVarTorso = suscribeSharVarTorso();
			successSharVarRobotRegion = suscribeSharVarRobotRegion();

			taskPlanner.robot.ActualizeConnectedModules(sharedVarConnected.Value);

            successSharedVarSuscription = successSharedVarConnected & successSharVarHdPos & successSharVarOdometryPos & successSharVarSkeletons & successSharVarTorso & successSharVarRobotRegion;

            if (successSharedVarSuscription)
			{
				cmdMan.Ready = true;

				TBWriter.Info2("  All Shared Variable Suscribed Succesfully");
				TBWriter.Write("");
				TBWriter.Write("-----  Systems Ready ----- ");
				TBWriter.Write("");
			}
			else
			{
				cmdMan.Ready = false;
				TBWriter.Error(": Shared Variable Suscription Failed");
				TBWriter.Warning1("System is not fully ready");
				cmdMan.Ready = false;
			}
		}

		private bool suscribeSharedVarConnected()
		{
			TBWriter.Info8("Trying to suscribe to Shared Variable: " + sharedVarConnected.Name);
			try
			{
				if (cmdMan.SharedVariables.Contains(this.sharedVarConnected.Name))
				{
					this.sharedVarConnected = (StringSharedVariable)(this.cmdMan.SharedVariables[this.sharedVarConnected.Name]);
					TBWriter.Info9("Shared Variable already exist, refering to it: " + sharedVarConnected.Name);
				}
				else
				{
					cmdMan.SharedVariables.Add(this.sharedVarConnected);
					TBWriter.Info9("Shared Variable doesnt exist, adding it: " + sharedVarOdometryPos.Name);
				}
				
				sharedVarConnected.Subscribe(SharedVariableReportType.SendContent, SharedVariableSubscriptionType.WriteOthers);
				sharedVarConnected.ValueChanged+=new SharedVariableSubscriptionReportEventHadler<string>(sharedVarConnected_ValueChanged);
				TBWriter.Info1("Suscribed to Shared Variable: " + sharedVarConnected.Name);

				return true;
			}
			catch (Exception e)
			{
				TBWriter.Error("Can't suscribe to Shared Variable: " + sharedVarConnected.Name + " ; Msg= " + e.Message);
				return false;
			}
		}

		private void sharedVarConnected_ValueChanged(SharedVariableSubscriptionReport<string> report)
		{
 			taskPlanner.robot.ActualizeConnectedModules(report.Value);
		}

		private bool suscribeSharVarRobotRegion()
		{
			TBWriter.Info8("Trying to suscribe to Shared Variable: " + sharedVarRobotRegion.Name);
			try
			{
				if (cmdMan.SharedVariables.Contains(this.sharedVarRobotRegion.Name))
				{
					this.sharedVarRobotRegion = (StringSharedVariable)(this.cmdMan.SharedVariables[this.sharedVarRobotRegion.Name]);
					TBWriter.Info9("Shared Variable already exist, refering to it: " + sharedVarRobotRegion.Name);
				}
				else
				{
					cmdMan.SharedVariables.Add(this.sharedVarRobotRegion);
					TBWriter.Info9("Shared Variable doesnt exist, adding it: " + sharedVarRobotRegion.Name);
				}

				sharedVarRobotRegion.Subscribe(SharedVariableReportType.SendContent, SharedVariableSubscriptionType.WriteOthers);
				sharedVarRobotRegion.ValueChanged += new SharedVariableSubscriptionReportEventHadler<string>(sharedVarRobotRegion_ValueChanged);
				TBWriter.Info1("Suscribed to Shared Variable: " + sharedVarRobotRegion.Name);

				return true;
			}
			catch (Exception e)
			{
				TBWriter.Error("Can't suscribe to Shared Variable: " + sharedVarRobotRegion.Name + " ; Msg= " + e.Message);
				return false;
			}
		}

		private void sharedVarRobotRegion_ValueChanged(SharedVariableSubscriptionReport<string> report)
		{
			taskPlanner.robot.hardwareMan.MobileBase.ActualizeRobotRegion(report.Value);
		}

		private bool suscribeSharVarSkeletons()
		{
			TBWriter.Info8("Trying to suscribe to Shared Variable: " + sharedVarSkeletons.Name);
			try
			{
				if (cmdMan.SharedVariables.Contains(this.sharedVarSkeletons.Name))
				{

					this.sharedVarSkeletons = (StringSharedVariable)(this.cmdMan.SharedVariables[this.sharedVarSkeletons.Name]);
					TBWriter.Info9("Shared Variable already exist, refering to it: " + sharedVarSkeletons.Name);
				}
				else
				{
					cmdMan.SharedVariables.Add(this.sharedVarSkeletons);
					TBWriter.Info9("Shared Variable doesnt exist, adding it: " + sharedVarSkeletons.Name);
				}

				sharedVarSkeletons.Subscribe(SharedVariableReportType.SendContent, SharedVariableSubscriptionType.WriteOthers);
				sharedVarSkeletons.ValueChanged += new SharedVariableSubscriptionReportEventHadler<string>(sharedVarSkeletons_ValueChanged);
				TBWriter.Info1("Suscribed to Shared Variable: " + sharedVarSkeletons.Name);

				return true;
			}
			catch (Exception e)
			{
				TBWriter.Error("Can't suscribe to Shared Variable: " + sharedVarSkeletons.Name + " ; Msg= " + e.Message);
				return false;
			}
		}

		private void sharedVarSkeletons_ValueChanged(SharedVariableSubscriptionReport<string> report)
		{
			taskPlanner.robot.sensorsMan.ActualizeSkeletonsSharedVar(report.Value);
		}

		private bool suscribeSharVarOdometryPos()
		{
			TBWriter.Info8("Trying to suscribe to Shared Variable: " + sharedVarOdometryPos.Name);
			try
			{
				if (cmdMan.SharedVariables.Contains(this.sharedVarOdometryPos.Name))
				{
					this.sharedVarOdometryPos = (DoubleArraySharedVariable)(this.cmdMan.SharedVariables[this.sharedVarOdometryPos.Name]);
					TBWriter.Info9("Shared Variable already exist, refering to it: " + sharedVarOdometryPos.Name);
				}
				else
				{
					cmdMan.SharedVariables.Add(this.sharedVarOdometryPos);
					TBWriter.Info9("Shared Variable doesnt exist, adding it: " + sharedVarOdometryPos.Name);
				}

				sharedVarOdometryPos.Subscribe(SharedVariableReportType.SendContent, SharedVariableSubscriptionType.WriteOthers);
				sharedVarOdometryPos.ValueChanged += new SharedVariableSubscriptionReportEventHadler<double[]>(sharedVarOdometryPos_ValueChanged);
				TBWriter.Info1("Suscribed to Shared Variable: " + sharedVarOdometryPos.Name);

				return true;
			}
			catch (Exception e)
			{
				TBWriter.Error("Can't suscribe to Shared Variable: " + sharedVarOdometryPos.Name + " ; Msg= " + e.Message);
				return false;
			}
		}

		private void sharedVarOdometryPos_ValueChanged(SharedVariableSubscriptionReport<double[]> report)
		{
			if (taskPlanner.robot.hardwareMan.MobileBase.ActualizeOdometryPosition(report.Value))
				TBWriter.Write(8, "Actualized Shared Var successfully: odometry; " + report.Variable.Name);
			else
				TBWriter.Write("ERROR ! Cant actualize Shared Var: odometry; " + report.Variable.Name);
		}

		private bool suscribeSharVarHdPos()
		{
			TBWriter.Info8("Trying to suscribe to Shared Variable: " + sharedVarHdPos.Name);
			try
			{
				if (cmdMan.SharedVariables.Contains(sharedVarHdPos.Name))
				{
					this.sharedVarHdPos = (DoubleArraySharedVariable)(cmdMan.SharedVariables[sharedVarHdPos.Name]);
					TBWriter.Info9("Shared Variable already exist, refering to it: " + sharedVarSkeletons.Name);
				}
				else
				{
					cmdMan.SharedVariables.Add(this.sharedVarHdPos);
					TBWriter.Info9("Shared Variable doesnt exist, adding it: " + sharedVarHdPos.Name);
				}

				sharedVarHdPos.Subscribe(SharedVariableReportType.SendContent, SharedVariableSubscriptionType.WriteOthers);
				sharedVarHdPos.ValueChanged += new SharedVariableSubscriptionReportEventHadler<double[]>(sharedVarHdPos_ValueChanged);
				TBWriter.Info1("Suscribed to Shared Variable: " + sharedVarHdPos.Name);

				return true;
			}
			catch (Exception e)
			{
				TBWriter.Error("Can't suscribe to Shared Variable: " + sharedVarHdPos.Name + " ; Msg= " + e.Message);
				return false;
			}
		}

		private void sharedVarHdPos_ValueChanged(SharedVariableSubscriptionReport<double[]> report)
		{
			if (taskPlanner.robot.head.ActualizeHeadPosition(report.Value))
				TBWriter.Write(8, "Actualized Shared Var successfully: head; " + report.Variable.Name);
			else
				TBWriter.Write("ERROR ! Cant actualize Shared Var: head; " + report.Variable.Name);
		}

		private bool suscribeSharVarTorso()
		{
			TBWriter.Info8("Trying to suscribe to Shared Variable: " + sharedVarTorso.Name);
			try
			{
				if (cmdMan.SharedVariables.Contains(sharedVarTorso.Name))
				{
					this.sharedVarTorso = (DoubleArraySharedVariable)(cmdMan.SharedVariables[sharedVarTorso.Name]);
					TBWriter.Info9("Shared Variable already exist, refering to it: " + sharedVarTorso.Name);
				}
				else
				{
					cmdMan.SharedVariables.Add(this.sharedVarTorso);
					TBWriter.Info9("Shared Variable doesnt exist, adding it: " + sharedVarTorso.Name);
				}

				this.sharedVarTorso.Subscribe(SharedVariableReportType.SendContent, SharedVariableSubscriptionType.WriteOthers);
				sharedVarTorso.ValueChanged += new SharedVariableSubscriptionReportEventHadler<double[]>(sharedVarTorso_ValueChanged);
				TBWriter.Info1("Suscribed to Shared Variable: " + sharedVarTorso.Name);

				return true;
			}
			catch (Exception ex)
			{
				TBWriter.Error("Can't suscribe to Shared Variable: " + sharedVarTorso.Name + " ; Msg= " + ex.Message);
				return false;
			}
		}

		private void sharedVarTorso_ValueChanged(SharedVariableSubscriptionReport<double[]> report)
		{
			if (taskPlanner.robot.torso.ActualizeTorsoStatus(report.Value))
				TBWriter.Write(8, "Actualized Shared Var successfully torso; " + report.Variable.Name);
			else
				TBWriter.Write("ERROR ! Cant act5ualize Shared Var torso; " + report.Variable.Name);
		}

		#endregion

		public void stopAll()
		{
			cnnMan.Stop();
			cmdMan.Stop();
		}
	}
}