﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Robotics.Controls;
using System.Threading;
using Robotics.API;
using Robotics.Mathematics;
using System.Diagnostics;
namespace SimpleTaskPlanner
{
	public enum useArms { useBoth, useLeft, useRight, };
	public delegate bool TakeObjArm(WorldObject obj);
	
    public class TaskPlanner
    {
        double takeDistanceIncrement = 0.04;

        #region General Vars

		private useArms armToUse = useArms.useBoth;

        public STPCommandManager cmdMan;
        public Robot robot;

        public int unknowHumans = 0;
        public SortedList<string, Human> personsList;
        public SortedList<string, WorldObject> objectsList;
        public SortedList<string, ArmMotion> learnedArmMotionsList;
        public List< string> predefLocation;

        Random randomNum = new Random();

		SceneMap sceneMap;


        public bool stop = false;
        public bool pause =false;
        public int pauseTimeOut_ms = 20000;
        public int pauseSleepTime_ms = 200;
		// public double pauseSteps = pauseTimeOut_ms / pauseSleepTime_ms;

		private bool usePFAuto = false;
		private bool pfAutoEnabled = false;
		public Human personPFAutoDetected = null;		

        double hdonShelfTilt = - MathUtil.ToRadians( 55);

        Thread timerThread;
        bool technicalChallengeTimeOut = false;


        double torsoElevationForNavigation = 0.20;

        #endregion 

		public useArms ArmToUse
		{
			get
			{ return this.armToUse; }
			set
			{
				this.armToUse = value;
				string arm2Use = "";

				if (armToUse == useArms.useBoth)
					arm2Use = "Both ARMS";
				if (armToUse == useArms.useLeft)
					arm2Use = "Left ARM";
				if (armToUse == useArms.useRight)
					arm2Use = "Right ARM";

				TBWriter.Info1("Changed Arm to use to " + arm2Use);
			}
		}

        #region Command Variables

        #region Cmd_LearningMotion

        Thread learnMotion;
        public bool learnMotionRunning;
        public SortedList<string, ArmMotion> armMotionsLearned;


        int nullVectorCountForSpim = 4;
        double maxDistanceForNoMotion = 0.03;
        int learnMotion_timeForRotation_ms = 4000;
        int learnMotion_stepsOfNoMotionForDetectRotation;
        int learnMotion_cantFindHandWaits_ms = 3000;
        int learnMotion_maxSteps = 100;
        int learnMotion_sleepInAdquisition = 200;
        public bool learnMotion_rightArm;
        public string learnMotion_name;

        #endregion

        #region Cmd_TakeObject Variables

        Thread takeRightArmThread;
		Thread takeLeftArmThread;
		
		bool takeRightArmRunning = false;
		bool takeLeftArmRunning = false;
		
		string takeRightResult = "";
		string takeLeftResult = "";

		WorldObject takeRightObject;
		WorldObject takeLeftObject;
		
		public string rightArmObject = "";
		public string leftArmObject = "";
		
		#endregion

		#region Cmd_FindHuman Variables

		bool cmdFindHuman_Running;



		#endregion

		#region Cmd_AutoLocalization Variables

		private double distanceThreashold = 0.5;
		private bool useRegions = true;
		private bool autoLocalization = false;
		private bool autoLocalizationRunning = false;
		private Thread autoLocalizationThread;
		

		#endregion

		#region Command Parameters

		const string paramsDev_Kinect = "knt";
		const string paramsDev_Torso  = "tor";
		const string paramsDev_HdPan  = "hdpan";
		const string paramsDev_HdTilt = "hdtilt";
		
        #endregion

        #endregion

        #region Constructor

        public TaskPlanner(STPCommandManager cmdMan, STPlanner stPlanner)
        {
            this.cmdMan = cmdMan ;
			this.robot = new Robot(0.11);

            personsList = new SortedList<string, Human>();
            objectsList = new SortedList<string, WorldObject>();
            learnedArmMotionsList = new SortedList<string, ArmMotion>();

            Vector3 asd1 = this.robot.TransNeck2Robot( new Vector3(0.0, 0.0, 1.0));
            Vector3 asd2 =  this.robot.TransChestKinect2Robot(new Vector3(0, 0, 1.0));

			this.sceneMap = new SceneMap();
		}

		#endregion

		#region Properties

        //public bool AutoLocalization
        //{
        //    get
        //    {
        //        return this.autoLocalization;
        //    }
        //    set
        //    {
        //        if (this.autoLocalization == value)
        //            return ;

        //        if (value)
        //        {
        //            try
        //            {
        //                this.imageMap = ImageMap.LoadFromXml("ImageMap.xml");
        //            }
        //            catch ( Exception ex)
        //            {
        //                TBWriter.Error("Cant load imageMap.xml , " + ex.Message );
        //                this.autoLocalization = false;
        //                return;
        //            }

        //            if (this.autoLocalizationThread == null)
        //                this.autoLocalizationThread = new Thread(new ThreadStart(autoLocalizationThreadTask));

        //            this.autoLocalizationThread.IsBackground = true;
        //            this.autoLocalizationRunning = true;
        //            this.autoLocalizationThread.Start();

        //            TBWriter.Info1("AutoLocalization is now enable");
        //            this.autoLocalization = true;
        //        }
        //        else
        //        {
        //            this.autoLocalizationRunning = false;

        //            if (autoLocalizationThread.IsAlive)
        //                this.autoLocalizationThread.Join(20);

        //            if (autoLocalizationThread.IsAlive)
        //                this.autoLocalizationThread.Abort();

        //            TBWriter.Info1("AutoLocalization is now disabled");
        //        }
        //    }
		//}

		#endregion

        #region Commands


        public bool Cmd_ShakeHand(string armToPerform)
        {
            bool success;
            int msToWait = 1000;

            if (armToPerform == "right")
            {
                
                if (!cmdMan.ARMS_ra_goto("navigation", 5000))
                {
                    TBWriter.Error("ARMS_ra_goto(navigation) returned false");
                    return false;
                }

                if (!cmdMan.ARMS_ra_goto("hello", 5000))
                {
                    TBWriter.Error("ARMS_ra_goto(handShake) returned false");
                    return false;
                }

				if (!cmdMan.ARMS_ra_handGoTo("greeting", 3000))
                {
					TBWriter.Error("ARMS_ra_handGoTo(greeting) returned false");
                    return false;
                }

                Thread.Sleep(msToWait);

                if (!cmdMan.ARMS_ra_goto("hello1", 5000))
                {
                    TBWriter.Error("ARMS_ra_relpos(0.05, 0, 0, 0, 0, 0, 0, 5000); returned false");
                    return false;
                }
				if (!cmdMan.ARMS_ra_goto("hello2", 5000))
                {
                    TBWriter.Error("ARMS_ra_relpos(-0.05, 0, 0, 0, 0, 0, 0, 5000); returned false");
                    return false;
                }
				if (!cmdMan.ARMS_ra_goto("hello1", 5000))
                {
                    TBWriter.Error("ARMS_ra_relpos(0.05, 0, 0, 0, 0, 0, 0, 5000); returned false");
                    return false;
                }
				if (!cmdMan.ARMS_ra_goto("hello2", 5000))
                {
                    TBWriter.Error("ARMS_ra_relpos(-0.05, 0, 0, 0, 0, 0, 0, 5000); returned false");
                    return false;
                }

                Thread.Sleep(msToWait);

				if (!cmdMan.ARMS_ra_goto("hello", 5000))
				{
					TBWriter.Error("ARMS_ra_goto(navigation) returned false");
					return false;
				}

                if (!cmdMan.ARMS_ra_goto("navigation", 5000))
                {
                    TBWriter.Error("ARMS_ra_goto(navigation) returned false");
                    return false;
                }

				if (!cmdMan.ARMS_ra_goto("home", 5000))
				{
					TBWriter.Error("ARMS_ra_goto(navigation) returned false");
					return false;
				}

                return true; 
            }
            return false; 
        }

        public bool Cmd_TakeXYZ(Vector3 CentroidInHDKnt, out string takedObject)
        {
            bool success;

            TBWriter.Info1("    ---  Starting Cmd_TakeXYZ Command  ---");
            TBWriter.Spaced("    ---  Params received: ( " + CentroidInHDKnt.X.ToString("0.00") + " , " + CentroidInHDKnt.Y.ToString("0.00") + " , " + CentroidInHDKnt.Z.ToString("0.00") + " ) in HDKnt_Coor");

            ActualizeHeadPos();
            ActualizeTorsoPos();

            Vector3 centroidInRobots = robot.TransHeadKinect2Robot(CentroidInHDKnt);

            TBWriter.Info1("Taking object with centroid ( " + centroidInRobots.X.ToString("0.00") + " , " + centroidInRobots.Y.ToString("0.00") + " , " + centroidInRobots.Z.ToString("0.00") + " ) in Robot_Coor");

            WorldObject objToTake = new WorldObject("obj2take", centroidInRobots, 0.08);

            takedObject = "empty empty";

            robot.lastObjectsFounded.Add(objToTake.Name, objToTake);

            success = Cmd_TakeObject(objToTake.Name, "", "", out takedObject);

            robot.lastObjectsFounded.Remove(objToTake.Name);

            return success;
        }

        public bool Cmd_PointAtObject(string ObjectName)
        {
            TBWriter.Info1("    ---  Starting TakeObject Command  ---");
            TBWriter.Spaced("    ---  Params received : " + ObjectName);

            WorldObject objToPoint;

            double distToPoint = 0.20;

            if (robot.lastObjectsFounded.ContainsKey(ObjectName))
            {
                objToPoint = new WorldObject(robot.lastObjectsFounded[ObjectName]);
                TBWriter.Info1("                  > Object founded in object List, preparing to point at" + objToPoint.Name);
            }
            else
            {
                TBWriter.Info1("                  > " + ObjectName + " not founded in objectsList, can't point at it");
                return false;
            }

            Vector3 armPos;

            Vector3 errorVec;
            double errorAng;
            double errorDis;

            Vector3 nextPos;
            double nextPitch;

            Vector3 nextPosInArms;

            bool pointed = false;

            double distanceFromTableToPoint = 0.25;

            objToPoint.Position.Z += -objToPoint.DistanceFromTable + distanceFromTableToPoint;

            // Pointing with right arm
            if (objToPoint.Position.Y < 0)
            {
                TBWriter.Info1("Pointing with right hand");

                if (!cmdMan.ARMS_ra_goto(Arm.ppNavigation, 10000))
                {
                    TBWriter.Error("ARMS_ra_goto can´t go to Navigation, cant point at object");
                    return false;
                }

                while (!pointed)
                {
                    ActualizeRightArmPosition();
                    armPos = new Vector3(robot.RightArmPosition);

                    errorVec = objToPoint.Position - armPos;
                    errorAng = Math.Atan2(errorVec.Y, errorVec.X);
                    errorDis = errorVec.Magnitude;

                    nextPos = objToPoint.Position - errorVec.Unitary * distToPoint;
                    nextPos.Z = objToPoint.Position.Z - objToPoint.DistanceFromTable + distanceFromTableToPoint;

                    nextPitch = errorAng;

                    nextPosInArms = robot.TransRobot2RightArm(nextPos);

                    if (nextPosInArms.Y < 0.05)
                    {
                        TBWriter.Error("Object to close, cant point at object");
                        return false;
                    }

                    if (cmdMan.ARMS_ra_reachable(nextPosInArms.X, nextPosInArms.Y, nextPosInArms.Z, MathUtil.PiOver2, nextPitch, 0, -nextPitch, 2000))
                    {
                        cmdMan.ARMS_ra_abspos(nextPosInArms, MathUtil.PiOver2, nextPitch, 0, -nextPitch, 10000);
                        Thread.Sleep(3000);
                        cmdMan.ARMS_ra_goto(Arm.ppNavigation);
                        pointed = true;
                    }
                    else
                    {
                        distToPoint += 0.01;
                        TBWriter.Info1("Object to far, incrementing distance to point at: " + distToPoint.ToString("0.00"));
                    }
                }
            }
            else
            {
                TBWriter.Info1("Pointing with left hand");

                if (!cmdMan.ARMS_la_goto(Arm.ppNavigation, 10000))
                {
                    TBWriter.Error("ARMS_ra_goto can´t go to Navigation, cant point at object");
                    return false;
                }

                while (!pointed)
                {
                    ActualizeLeftArmPosition();
                    armPos = new Vector3(robot.LeftArmPosition);

                    errorVec = objToPoint.Position - armPos;
                    errorAng = Math.Atan2(errorVec.Y, errorVec.X);
                    errorDis = errorVec.Magnitude;

                    nextPos = objToPoint.Position - errorVec.Unitary * distToPoint;
                    nextPos.Z = objToPoint.Position.Z - objToPoint.DistanceFromTable + distanceFromTableToPoint;
                    nextPitch = errorAng;

                    nextPosInArms = robot.TransRobot2LeftArm(nextPos);

                    if (nextPosInArms.Y < 0.05)
                    {
                        TBWriter.Error("Object to close, cant point at object");
                        return false;
                    }

                    if (cmdMan.ARMS_la_reachable(nextPosInArms.X, nextPosInArms.Y, nextPosInArms.Z, MathUtil.PiOver2, nextPitch, 0, -nextPitch, 2000))
                    {
                        cmdMan.ARMS_la_abspos(nextPosInArms, MathUtil.PiOver2, nextPitch, 0, -nextPitch, 10000);
                        Thread.Sleep(3000);
                        cmdMan.ARMS_la_goto(Arm.ppNavigation);
                        pointed = true;
                    }
                    else
                    {
                        distToPoint += 0.01;
                        TBWriter.Info1("Object to far, incrementing distance to point at: " + distToPoint.ToString("0.00"));
                    }
                }
            }

            return true;
        }

        public bool Cmd_GetCloseToSkeleton(string ID )
        {
            return true;

        }

        public bool Cmd_AligneHuman()
        {
            TBWriter.Spaced("    ---  Starting AligneHuman Command  ---");

            ActualizeHeadPos();
            ActualizeTorsoPos();

            if (robot.LastHuman == null)
            {
                TBWriter.Error("LastHuman is null");
                TBWriter.Spaced("    ---  Finished AligneHuman Command  ---");
                return false;
            }

            TBWriter.Info2(" Aligning with " + robot.LastHuman.Name);

            double angle;
            double elevation;

            angle = Math.Atan2(robot.LastHuman.Head.Y, robot.LastHuman.Head.X);
            Vector3 minoruPos = robot.TransMinoru2Robot(Vector3.Zero);

            elevation = robot.torso.Elevation + robot.LastHuman.Head.Z - minoruPos.Z - 0.1;
           

			if( (robot.head.MinPan < angle) && (angle < robot.head.MaxPan))
			{
				cmdMan.HEAD_lookatobject( robot.TransRobot2Neck( robot.LastHuman.Head));
			}
			//if (elevation < 0.70)
			//{
			//    cmdMan.ARMS_la_goto("standby");
			//    cmdMan.ARMS_ra_goto("standby", 7000);
			//    cmdMan.WaitForResponse(JustinaCommands.ARMS_la_goto, 2000);

			//    if (elevation < 0.5)
			//        elevation = 0.5;
			//}

			//if ((robot.torso.MinPan < angle) && (angle < robot.torso.MaxPan))
			//{
			//    cmdMan.HEAD_lookat(0, 0);
			//    cmdMan.TRS_abspos(elevation, angle, 10000);
			//    cmdMan.WaitForResponse(JustinaCommands.HEAD_lookat, 2000);
			//}
            else
            {
                cmdMan.HEAD_lookat(0, 0);
                cmdMan.TRS_abspos(elevation, 0);
                cmdMan.MVN_PLN_move(0, angle, 10000);
                cmdMan.WaitForResponse(JustinaCommands.TRS_abspos, 5000);
                cmdMan.WaitForResponse(JustinaCommands.HEAD_lookat, 2000);
            }

            TBWriter.Spaced("    ---  Finished AligneHuman Command  ---");
            return true;
        }

        public bool Cmd_DeliverObject(string objectName)
        {
            TBWriter.Spaced("    ---  Starting DeliverObject Command  ---");

            //if (rightArmObject == objectName)
            //{
                TBWriter.Info2(" Delivering " + objectName + "with RIGHT arm");

                cmdMan.ARMS_ra_goto("navigation",7000);

                cmdMan.ARMS_ra_goto("deliver", 7000);
                cmdMan.SPG_GEN_say("I will open my gripper, please, hold the " + objectName, 5000);
                Thread.Sleep(3000);
                cmdMan.ARMS_ra_opengrip(100, 3000);
                rightArmObject = "";
                robot.RightArmIsEmpty = true;

                Thread.Sleep(1000);

                cmdMan.ARMS_ra_goto("navigation", 7000);
                cmdMan.ARMS_ra_opengrip(30, 3000);

                TBWriter.Spaced("    ---  Finished DeliverObject Command  ---");
                return true;
            //}

            if (leftArmObject == objectName)
            {
                TBWriter.Info2("Delivering " + objectName + "with LEFT arm");

                cmdMan.ARMS_la_goto("navigation", 7000);

                cmdMan.ARMS_la_goto("deliver", 7000);
                cmdMan.SPG_GEN_say("I will open my gripper, please, hold the " + objectName, 5000);
                Thread.Sleep(3000);
                cmdMan.ARMS_la_opengrip(100, 3000);
                leftArmObject = "";
                robot.LeftArmIsEmpty = true;

                Thread.Sleep(1000);

                cmdMan.ARMS_la_goto("navigation", 7000);
                cmdMan.ARMS_la_opengrip(30, 3000);


                TBWriter.Spaced("    ---  Finished DeliverObject Command  ---");
                return true;
            }

            TBWriter.Warning1("No Arm contains " + objectName);
            TBWriter.Spaced("    ---  Finished DeliverObject Command  ---");
            return false;
        }

        public bool Cmd_DoPresentation()
        {
            TBWriter.Spaced("    ---  Starting DoPresentation Command  ---");


            bool rightArmSuccess = false;
            bool leftArmSuccess = false;
            bool headSuccess = false;
            bool torsoSuccess = false;
            bool mobilBaseSuccess = false;

            double torsoIniElevation = 0.8;
            double torsoIniPan = 0.0;


            if (!robot.modules[Module.SpGenerator].IsConnected)
            {
                TextBoxStreamWriter.DefaultLog.WriteLine(1, "DoPresentation Warning: SpGen is not Connected, can't do DoPresentation");
            }

            //Initial Conditions
            if (robot.modules[Module.Arms].IsConnected) rightArmSuccess = this.cmdMan.ARMS_ra_goto(Arm.ppHome, 7000);
            if (robot.modules[Module.Arms].IsConnected) leftArmSuccess = this.cmdMan.ARMS_la_goto(Arm.ppHome, 7000);
            if (robot.modules[Module.Head].IsConnected) headSuccess = this.cmdMan.HEAD_lookat(0, 0, 5000);
            if (robot.modules[Module.Torso].IsConnected) torsoSuccess = this.cmdMan.TRS_abspos(torsoIniElevation, torsoIniPan, 5000);
            if (robot.modules[Module.MovingPln].IsConnected) mobilBaseSuccess = this.cmdMan.MVN_PLN_move(0, 0, 1000);


            //Introduction
            cmdMan.SPG_GEN_say("Hello. I am Robot Justina. Please, let me show you my design", 8000);

            if (cmdMan.IsResponseReceived(JustinaCommands.SP_GEN_say))
                sleep(800);
            else
                if (cmdMan.WaitForResponse(JustinaCommands.SP_GEN_say, 2000)) sleep(800);


            //Head Introduction
            cmdMan.SPG_GEN_say("I have a Mechatronic Head. This has two degrees of freedom for Pan and tilt");

            cmdMan.ARMS_ra_goto(Arm.ppShowHead, 7000);

            if (headSuccess)
            {
                cmdMan.HEAD_lookat(1, 1, 4000);
                cmdMan.HEAD_lookat(0, 0, 4000);
            }

            if (cmdMan.IsResponseReceived(JustinaCommands.SP_GEN_say))
                sleep(800);
            else
                if (cmdMan.WaitForResponse(JustinaCommands.SP_GEN_say, 2000)) sleep(800);


            // STEREO CAMERA
            cmdMan.SPG_GEN_say("In my face, i have a stereo camera for face recognition", 6000);

            if (cmdMan.IsResponseReceived(JustinaCommands.SP_GEN_say))
                sleep(800);
            else
                if (cmdMan.WaitForResponse(JustinaCommands.SP_GEN_say, 2000)) sleep(800);


            //KINECT intro
            cmdMan.SPG_GEN_say("I have two kinects devices. One in my chest to identify Human Movements");
            cmdMan.ARMS_ra_goto(Arm.ppShowChestKinect, 7000);
            cmdMan.WaitForResponse(JustinaCommands.SP_GEN_say, 2000);


            cmdMan.SPG_GEN_say("and one my head for object recognition");
            //cmdMan.ARMS_ra_goto(Arm.ppShowHeadKinect, 7000);
			cmdMan.ARMS_ra_goto(Arm.ppShowHead, 7000);

            if (cmdMan.IsResponseReceived(JustinaCommands.SP_GEN_say))
                sleep(800);
            else
                if (cmdMan.WaitForResponse(JustinaCommands.SP_GEN_say, 2000)) sleep(800);



            // LASER
            this.cmdMan.SPG_GEN_say("I have a laser rangeFinder sensor for scanning unknown obstacles");
            cmdMan.ARMS_ra_goto(Arm.ppShowLaser, 7000);

            if (cmdMan.IsResponseReceived(JustinaCommands.SP_GEN_say))
                sleep(800);
            else
                if (cmdMan.WaitForResponse(JustinaCommands.SP_GEN_say, 2000)) sleep(800);



            // ARM
            this.cmdMan.SPG_GEN_say("I have two arms. These are anthropomorphic seven degrees of freedom manipulators");

            cmdMan.ARMS_ra_goto(Arm.ppShowArm);
            cmdMan.ARMS_la_goto(Arm.ppShowArm, 7000);

            cmdMan.WaitForResponse(JustinaCommands.ARMS_ra_goto, 2000);

            if (cmdMan.IsResponseReceived(JustinaCommands.SP_GEN_say))
                sleep(800);
            else
                if (cmdMan.WaitForResponse(JustinaCommands.SP_GEN_say, 2000)) sleep(800);


            // TORSO
			//this.cmdMan.SPG_GEN_say("I also have a mechanic torso that allows me to control the elevation and the rotation of my chest");

			//cmdMan.ARMS_ra_goto(Arm.ppShowArm, 7000);

			//if (torsoSuccess)
			//{
			//    cmdMan.TRS_relpos(0.2, 0, 5000);
			//    cmdMan.TRS_relpos(-0.3, 0, 5000);

			//    cmdMan.TRS_relpos(0, .3, 5000);
			//    cmdMan.TRS_relpos(0, -.6, 5000);

			//    cmdMan.TRS_relpos(0, 0.3, 5000);

			//    cmdMan.TRS_abspos(torsoIniElevation, torsoIniPan, 6000);
			//}

			//if (cmdMan.IsResponseReceived(JustinaCommands.SP_GEN_say))
			//    sleep(800);
			//else
			//    if (cmdMan.WaitForResponse(JustinaCommands.SP_GEN_say, 40000)) sleep(800);



            // BASE
            this.cmdMan.SPG_GEN_say("Finally, I have a differential pair mobile base for navigation");

            //cmdMan.ARMS_ra_goto(Arm.ppHome);
            //cmdMan.ARMS_la_goto(Arm.ppHome);


            if (mobilBaseSuccess)
            {
                cmdMan.MVN_PLN_move(0, 0.3, 3000);
                cmdMan.MVN_PLN_move(0, -0.3, 3000);
            }

            if (cmdMan.IsResponseReceived(JustinaCommands.SP_GEN_say))
                sleep(800);
            else
                if (cmdMan.WaitForResponse(JustinaCommands.SP_GEN_say, 4000)) sleep(800);


            // GOODBYE
            this.cmdMan.SPG_GEN_say("Thats all, I have finish my presentation", 6000);

            cmdMan.ARMS_ra_goto(Arm.ppHome);
            cmdMan.ARMS_la_goto(Arm.ppHome);

            cmdMan.TRS_abspos(torsoIniElevation, torsoIniPan, 6000);


            if (cmdMan.WaitForResponse(JustinaCommands.ARMS_ra_goto, 3000)) sleep(1000);
            if (cmdMan.WaitForResponse(JustinaCommands.ARMS_la_goto, 3000)) sleep(1000);


            robot.LeftArmIsEmpty = false;
            leftArmObject = "registrationform";


            TBWriter.Spaced("    ---  Finished DoPresentation Command  ---");

            return true;
        }

		public bool Cmd_Drop(string hand)
		{
			TBWriter.Spaced("    ---  Starting Cmd_Drop Command  ---");

			if (armToUse == useArms.useLeft)
				hand = Arm.left;

			if (armToUse == useArms.useRight)
				hand = Arm.right;

			bool success = false;

			if (hand == Arm.right)
			{
				TBWriter.Info2(" Dropping with RIGHT arm");

				cmdMan.ARMS_ra_goto(Arm.ppNavigation, 7000);
				cmdMan.ARMS_ra_goto(Arm.ppDeliver, 7000);
				cmdMan.ARMS_ra_opengrip(100, 3000);
				rightArmObject = "";
				robot.RightArmIsEmpty = true;

				Thread.Sleep(3000);

			
				cmdMan.ARMS_ra_goto(Arm.ppNavigation, 7000);
				cmdMan.ARMS_ra_goto(Arm.ppHome, 7000);
				cmdMan.ARMS_ra_opengrip(20, 3000);


				TBWriter.Spaced("    ---  Finished Cmd_Drop Command  ---");
				return true;
			}

			if (hand == Arm.left)
			{
				TBWriter.Info2(" Dropping with LEFT arm");

				cmdMan.ARMS_la_goto(Arm.ppNavigation, 7000);
				cmdMan.ARMS_la_goto(Arm.ppDeliver, 7000);
				cmdMan.ARMS_la_opengrip(100, 3000);
				leftArmObject = "";
				robot.LeftArmIsEmpty = true;

				Thread.Sleep(3000);

				cmdMan.ARMS_la_goto(Arm.ppNavigation, 7000);
				cmdMan.ARMS_la_goto(Arm.ppHome, 7000);
				cmdMan.ARMS_la_opengrip(20, 3000);

				TBWriter.Spaced("    ---  Finished Cmd_Drop Command  ---");
				return true;
			}

			TBWriter.Warning1("Drop Command WRONG parameter: " + hand + " ; Must be " + Arm.left + " or " + Arm.right);
			TBWriter.Spaced("    ---  Finished Cmd_Drop Command  ---");
			return false;
		}

        public bool Cmd_ExecuteLearn(string learnedMotionName, string armToUse)
        {
            TBWriter.Spaced("   === Starting ExecuteLearnMotion");
            TBWriter.Info1(" Received : execute " + learnedMotionName);

            if (!learnedArmMotionsList.ContainsKey(learnedMotionName))
            {
                TBWriter.Error("Can't start ExecuteLearnMotion, " + learnedMotionName + " is not in learnedMotionsList");
                TBWriter.Spaced("   === Finishing ExecuteLearnMotion");
                return false;
            }
            if (!robot.modules[Module.Arms].IsConnected)
            {
                TBWriter.Error("Can't start ExecuteLearnMotion, ARMS is not connected");
                TBWriter.Spaced("   === Finishing ExecuteLearnMotion");
                return false;
            }
            if ((armToUse != "right") && (armToUse != "left"))
            {
                TBWriter.Error("Can't start ExecuteLearnMotion, parameter armToUse is not valid" + armToUse);
                TBWriter.Spaced("   === Finishing ExecuteLearnMotion");
                return false;
            }

            int timeForSpin_ms = 3000;
            bool useRight = true;


            if (armToUse == "right")
                useRight = true;

            if (armToUse == "left")
                useRight = false;


            useRight = false;


            if (useRight)
            {
                TBWriter.Info1("sending " + armToUse + "to navigation");
                cmdMan.ARMS_ra_goto(Arm.ppNavigation, 7000);

                if (learnedArmMotionsList[learnedMotionName].UseSpin)
                {
                    int i;

                    for (i = learnedArmMotionsList[learnedMotionName].RightArmPositions.Length - 1; i > learnedArmMotionsList[learnedMotionName].SpinStopIndex; --i)
                        this.cmdMan.ARMS_ra_abspos(learnedArmMotionsList[learnedMotionName].RightArmPositions[i], MathUtil.PiOver2, 0, 0, 0, 7000);

                    this.cmdMan.ARMS_ra_abspos(learnedArmMotionsList[learnedMotionName].RightArmPositions[i], MathUtil.PiOver2, 0, 0, 0, 7000);
                    this.cmdMan.ARMS_ra_relpos(0, 0, 0, 0, 0, -1.65, 7000);

                    Thread.Sleep(timeForSpin_ms);
                    i--;

                    for (int j = i; j == 0; j--)
                        this.cmdMan.ARMS_ra_abspos(learnedArmMotionsList[learnedMotionName].RightArmPositions[j], MathUtil.PiOver2, 0, 0, 0.7, 7000);
                }
                else
                {
                    int i;

                    for (i = learnedArmMotionsList[learnedMotionName].RightArmPositions.Length - 1; i >= 0; i--)
                    {
                        this.cmdMan.ARMS_ra_abspos(learnedArmMotionsList[learnedMotionName].RightArmPositions[i], MathUtil.PiOver2, 0, 0, 0, 7000);
                    }
                }

                cmdMan.ARMS_ra_goto(Arm.ppNavigation, 7000);
            }
            else
            {
                cmdMan.ARMS_la_goto(Arm.ppNavigation, 7000);

                if (learnedArmMotionsList[learnedMotionName].UseSpin)
                {
                    int i;

                    for (i = learnedArmMotionsList[learnedMotionName].LeftArmPositions.Length - 1; i < learnedArmMotionsList[learnedMotionName].SpinStopIndex; i--)
                    {
                        this.cmdMan.ARMS_la_abspos(learnedArmMotionsList[learnedMotionName].LeftArmPositions[i], MathUtil.PiOver2, 0, 0, 0.7, 7000);
                    }

                    i--;
                    this.cmdMan.ARMS_la_abspos(learnedArmMotionsList[learnedMotionName].LeftArmPositions[i], MathUtil.PiOver2, 0, MathUtil.ToRadians(100), 0.7, 7000);
                    Thread.Sleep(timeForSpin_ms);
                    i--;

                    for (int j = i; i == 0; i--)
                    {
                        this.cmdMan.ARMS_la_abspos(learnedArmMotionsList[learnedMotionName].LeftArmPositions[i], MathUtil.PiOver2, 0, 0, 0.7, 7000);
                    }

                }
                else
                {
                    int i;

                    for (i = learnedArmMotionsList[learnedMotionName].LeftArmPositions.Length - 1; i >= 0; i--)
                    {
                        this.cmdMan.ARMS_la_abspos(learnedArmMotionsList[learnedMotionName].LeftArmPositions[i], MathUtil.PiOver2, 0, 0, 0.7, 7000);
                    }
                }

                cmdMan.ARMS_la_goto(Arm.ppNavigation, 7000);
            }

            TBWriter.Spaced("   === Finishing ExecuteLearnMotion");
            return true;
        }

		public bool Cmd_FindHuman(string humanName, string devices, out Human human)
		{
			string nameToSearch;

			TBWriter.Info3("Analizing CmdFindHuman Parameters");

			//if (!robot.modules[Module.PersonFnd].IsConnected)
			//{
			//    TBWriter.Error("Module " + Module.PersonFnd + " is not conected, can't execute find human");
			//    human = null;
			//    return false;
			//}

			if (humanName == "")
			{
				TBWriter.Info4("Parameter \" humanName \" is empty, looking for any human");
				nameToSearch = "human";
			}
			else
			{
				TBWriter.Info4("Parameter \" humanName \" is " + humanName);
				nameToSearch = humanName;
			}


			double hdPanInitial = 0;
	//		int hdPanSteps = 3;
			int hdPanSteps = 3;
	//		double hdPanMaxAngle = MathUtil.ToRadians(30);
			double hdPanMaxAngle = MathUtil.ToRadians(60);
			double hdPanIncrement = hdPanMaxAngle / hdPanSteps;

			double hdTiltInitial = 0;
			int hdTiltSteps = 2;
//			int hdTiltSteps = 3;
//			double hdTiltMaxAngle = MathUtil.ToRadians(35);
			double hdTiltMaxAngle = MathUtil.ToRadians(30);
			double hdTiltIncrement = hdTiltMaxAngle / hdTiltSteps;

			double humanMaxPan = 0.15;
			double humanMaxTilt = 0.15;
			double humanPan;
			double humanTilt;

			bool usePFAuto = false;

			bool humanIsFounded = false;
			Human humanFounded = null;

			int i;
			int j;
			int k;

			//cmdMan.SPG_GEN_playloop("tickshort.mp3");

			double hdPanValue;
			double hdTiltValue;

			// Positive Pan
			hdPanValue = hdPanInitial;
			for (i = 0; i < hdPanSteps; i++)
			{
				hdPanValue = i * hdPanIncrement;

				///////////////////////////////
				//hdTiltValue = hdTiltInitial;
				//for (j = 0; j < hdTiltSteps; j++)
				//{
				//    hdTiltValue = j * hdTiltIncrement;
				//    cmdMan.HEAD_lookat(hdPanValue, hdTiltValue, 5000);
				//    Thread.Sleep(500);
				//    if (LookForHuman(nameToSearch, usePFAuto, out humanFounded, out humanPan, out humanTilt))
				//    {
				//        //if ((Math.Abs(humanPan) < humanMaxPan) && (Math.Abs(humanTilt) < humanMaxTilt))
				//        //{
				//        //	cmdMan.HEAD_lookatREL(humanPan, humanTilt, 5000);

				//            human = new Human(humanFounded.Name, humanFounded.Head);
				//            robot.RememberHuman(human);

				//            TBWriter.Spaced("    ---  Finished FindHuman Command  ---");
				//            return true;
				//        //}
				//    }
				//}

				hdTiltValue = hdTiltInitial;
				for (k = 0; k > -hdTiltSteps; k--)
				{
					hdTiltValue = k * hdTiltIncrement;
					cmdMan.HEAD_lookat(hdPanValue, hdTiltValue, 5000);
					Thread.Sleep(500);
					if (LookForHuman(nameToSearch, usePFAuto, out humanFounded, out humanPan, out humanTilt))
					{
						//if ((Math.Abs(humanPan) < humanMaxPan) && (Math.Abs(humanTilt) < humanMaxTilt))
						//{
						//	cmdMan.HEAD_lookatREL(humanPan, humanTilt, 5000);

							human = new Human(humanFounded.Name, humanFounded.Head);
							robot.RememberHuman(human);

							TBWriter.Spaced("    ---  Finished FindHuman Command  ---");
							return true;
						//}
					}

				}
			}

			// Negative Pan
			hdPanValue = hdPanInitial;
			for (i = 0; i > -hdPanSteps; i--)
			{
				hdPanValue = i * hdPanIncrement;
				
				///////////////////////////////
				//hdTiltValue = hdTiltInitial;
				//for (j = 0; j < hdTiltSteps; j++)
				//{
				//    hdTiltValue = j * hdTiltIncrement;
				//    cmdMan.HEAD_lookat(hdPanValue, hdTiltValue, 5000);
				//    Thread.Sleep(500);
				//    if (LookForHuman(nameToSearch, usePFAuto, out humanFounded, out humanPan, out humanTilt))
				//    {
				//        //if ((Math.Abs( humanPan )< humanMaxPan) && (Math.Abs( humanTilt) < humanMaxTilt))
				//        //{
				//            human = new Human(humanFounded.Name, humanFounded.Head);
				//            robot.RememberHuman(human);

				//            TBWriter.Spaced("    ---  Finished FindHuman Command  ---");
				//            return true;
				//        //}
				//    }
				//}

				hdTiltValue = hdTiltInitial;
				for (k = 0; k > -hdTiltSteps; k--)
				{
					hdTiltValue = k * hdTiltIncrement;
					cmdMan.HEAD_lookat(hdPanValue, hdTiltValue, 5000);
					Thread.Sleep(500);
					if (LookForHuman(nameToSearch, usePFAuto, out humanFounded, out humanPan, out humanTilt))
					{
						//if ((Math.Abs(humanPan) < humanMaxPan) && (Math.Abs(humanTilt) < humanMaxTilt))
						//{
							human = new Human(humanFounded.Name, humanFounded.Head);
							robot.RememberHuman(human);

							TBWriter.Spaced("    ---  Finished FindHuman Command  ---");
							return true;
						//}
					}

				}
			}

			cmdMan.HEAD_lookat(hdPanInitial, hdTiltInitial, 5000);

			human = null;
			TBWriter.Spaced("    ---  Finished FindHuman Command  ---");
			return false;
		}

		//public bool Cmd_FindHuman(string humanName, string devices, out Human human)
		//{
		//    TBWriter.Spaced("    ---  Starting FindHuman Command  ---  ");

		//    string nameToSearch;

		//    bool useKinect;
		//    bool useTorso;
		//    bool useHeadPan;
		//    bool useHeadTilt;


		//    #region Analizing CmdFindHuman Parameters

		//    TBWriter.Info3("Analizing CmdFindHuman Parameters");

		//    if (!robot.modules[Module.PersonFnd].IsConnected)
		//    {
		//        TBWriter.Error("Module " + Module.PersonFnd + " is not conected, can't execute find human");
		//        human = null;
		//        return false;
		//    }

		//    if (humanName == "")
		//    {
		//        TBWriter.Info4("Parameter \" humanName \" is empty, looking for any human");
		//        nameToSearch = "human";
		//    }
		//    else
		//    {
		//        TBWriter.Info4("Parameter \" humanName \" is " + humanName);
		//        nameToSearch = humanName;
		//    }

		//    if (devices == "")
		//    {
		//        TBWriter.Info4("Parameter \" devices \" is empty, verifying available Modules");

		//        useKinect = robot.modules[Module.HumanFnd].IsConnected;
		//        useTorso = robot.modules[Module.Torso].IsConnected;
		//        useHeadPan = robot.modules[Module.Head].IsConnected;
		//        useHeadTilt = robot.modules[Module.Head].IsConnected;
		//    }
		//    else
		//    {
		//        TBWriter.Info4("Parameter \" devices \" is " + devices + " , verifying available Modules");

		//        useKinect = ((robot.modules[Module.HumanFnd].IsConnected) && (devices.Contains(paramsDev_Kinect)));
		//        useTorso = ((robot.modules[Module.Torso].IsConnected) && (devices.Contains(paramsDev_Torso)));
		//        useHeadPan = ((robot.modules[Module.Head].IsConnected) && (devices.Contains(paramsDev_HdPan)));
		//        useHeadTilt = ((robot.modules[Module.Head].IsConnected) && (devices.Contains(paramsDev_HdTilt)));
		//    }


		//    TBWriter.Info1("Looking for   [ " + nameToSearch.ToUpper() + " ] using:  KINECT=" + useKinect.ToString() + " , TORSO=" + useTorso.ToString() + " , HEADPAN = " + useHeadPan.ToString() + " , HEADTILT = " + useHeadTilt.ToString());

		//    #endregion

		//    #region Variables

		//    bool usePFAuto = false;

		//    bool isFounded = false;
		//    Human humanFounded = null;

		//    double hdPanValue;
		//    double hdTiltValue;


		//    double torInitialElev = 0.80;
		//    double torsoMaxElevatio = 0.90;
		//    double torsoMinElevatio = 0.70;
		//    double torElevValue = torInitialElev;


		//    double hdPanInitial = 0;
		//    int hdPanSteps = 2;
		//    double hdPanMaxAngle = MathUtil.ToRadians(45);
		//    double hdPanIncrement = hdPanMaxAngle / hdPanSteps;

		//    double hdTiltInitial = 0;
		//    int hdTiltSteps = 1;
		//    double hdTiltMaxAngle = MathUtil.ToRadians(20);
		//    double hdTiltIncrement = hdTiltMaxAngle / hdTiltSteps;

		//    int nextState = 0;

		//    int i;
		//    int j;
		//    int k;

		//    #endregion

		//    cmdFindHuman_Running = true;

		//    cmdMan.SPG_GEN_playloop("tickshort.mp3");

		//    useTorso = false;

		//    while (cmdFindHuman_Running)
		//    {
				
		//        switch (nextState)
		//        {

		//            #region Validating Parameters

		//            case 0:

		//                if ((isFounded) || (humanFounded != null))
		//                    nextState = 100;
		//                else if (useKinect)
		//                    nextState = 10;
		//                else if (useTorso)
		//                    nextState = 20;
		//                else if (useHeadPan)
		//                    nextState = 30;
		//                else if (useHeadTilt)
		//                    nextState = 40;

		//                break;

		//                #endregion 

		//            #region Kinect Search

		//            case 10:

		//                ActualizeHeadPos();
		//                ActualizeTorsoPos();

		//                Human[] hum;
		//                if (findHumanWithKinect(nameToSearch, out hum))
		//                {
		//                    humanFounded = hum[0];
		//                }


		//                if ((isFounded) || (humanFounded != null))
		//                    nextState = 100;
		//                else if (useTorso)
		//                    nextState = 20;
		//                else if (useHeadPan)
		//                    nextState = 30;
		//                else if (useHeadTilt)
		//                    nextState = 40;

		//                break;

		//            #endregion

		//            #region Torso Elevation
                       
		//            case 20:

		//                ActualizeHeadPos();
		//                ActualizeTorsoPos();

		//                if ((isFounded) || (humanFounded != null))
		//                {
		//                    nextState = 100;
		//                    break;
		//                }

		//                if (torElevValue == torInitialElev)
		//                {
		//                    cmdMan.TRS_abspos(torElevValue, 0, 10000);
		//                    isFounded = LookForHuman(nameToSearch, usePFAuto, out humanFounded);

		//                    if ((isFounded) || (humanFounded != null))
		//                        nextState = 100;
		//                    else if (useHeadPan)
		//                        nextState = 30;
		//                    else if (useHeadTilt)
		//                        nextState = 40;
		//                    else if (useTorso)
		//                        nextState = 20;

		//                    torElevValue = torsoMaxElevatio;
		//                }
		//                else if (torElevValue == torsoMaxElevatio)
		//                {
		//                    cmdMan.TRS_abspos(torElevValue, 0, 10000);
		//                    isFounded = LookForHuman(nameToSearch, usePFAuto, out humanFounded);

		//                    if ((isFounded) || (humanFounded != null))
		//                        nextState = 100;
		//                    else if (useHeadPan)
		//                        nextState = 30;
		//                    else if (useHeadTilt)
		//                        nextState = 40;
		//                    else if (useTorso)
		//                        nextState = 20;


		//                    torElevValue = torsoMinElevatio;
		//                }
		//                else if (torElevValue == torsoMinElevatio)
		//                {

		//                    cmdMan.TRS_abspos(torElevValue, 0, 10000);
		//                    isFounded = LookForHuman(nameToSearch, usePFAuto, out humanFounded);

		//                    if ((isFounded) || (humanFounded != null))
		//                        nextState = 100;
		//                    else if (useHeadPan)
		//                        nextState = 30;
		//                    else if (useHeadTilt)
		//                        nextState = 40;
		//                    else if (useTorso)
		//                        nextState = 20;

		//                    torElevValue = double.NaN;
		//                }
		//                else if (double.IsNaN(torElevValue))
		//                {
		//                    nextState = 100;
		//                    break;
		//                }


		//                break;

		//            #endregion

		//            #region Using CENTER Head Pan
				
		//            case 30:

		//                ///// -----   Using CENTER Head Pan

		//                ActualizeHeadPos();
		//                ActualizeTorsoPos();

		//                if ((isFounded) || (humanFounded != null))
		//                    nextState = 100;

		//                cmdMan.HEAD_lookat(hdPanInitial, hdTiltInitial, 4000);
		//                isFounded = LookForHuman(nameToSearch, usePFAuto, out humanFounded);


		//                if ((isFounded) || (humanFounded != null))
		//                {
		//                    nextState = 100;
		//                    break;
		//                }


		//                ///// -----   Using POSITIVE Head Pan

		//                hdPanValue = hdPanInitial;
		//                for (i = 0; i < hdPanSteps; i++)
		//                {

		//                    hdPanValue += hdPanIncrement;
		//                    cmdMan.HEAD_lookat(hdPanValue, hdTiltInitial, 4000);

		//                    isFounded = LookForHuman(nameToSearch, usePFAuto, out humanFounded);

		//                    if ((isFounded) || (humanFounded != null))
		//                    {
		//                        nextState = 100;
		//                        break;
		//                    }

		//                    if (useHeadTilt)
		//                    {

		//                        /// -- Positive Tilt

		//                        hdTiltValue = hdTiltInitial;
		//                        for ( j = 0; j < hdTiltSteps; j++)
		//                        {
		//                            hdTiltValue += hdTiltIncrement;
		//                            cmdMan.HEAD_lookat(hdPanValue, hdTiltValue, 4000);

		//                            if (LookForHuman(nameToSearch, usePFAuto, out humanFounded))
		//                                break;
		//                        }

		//                        if ((isFounded) || (humanFounded != null))
		//                        {
		//                            nextState = 100;
		//                            break;
		//                        }


		//                        // Negative Tilt

		//                        hdTiltValue = hdTiltInitial;
		//                        for ( k = 0; k < hdTiltSteps; k++)
		//                        {
		//                            hdTiltValue -= hdTiltIncrement;
		//                            cmdMan.HEAD_lookat(hdPanValue, hdTiltValue, 4000);

		//                            if (LookForHuman(nameToSearch, usePFAuto, out humanFounded))
		//                                break;
		//                        }

		//                        if ((isFounded) || (humanFounded != null))
		//                        {
		//                            nextState = 100;
		//                            break;
		//                        }
		//                    }
		//                }


		//                if ((isFounded) || (humanFounded != null))
		//                {
		//                    nextState = 100;
		//                    break;
		//                }



		//                ///// -----   Using CENTER Head Pan


		//                cmdMan.HEAD_lookat(hdPanInitial, hdTiltInitial, 5000);
		//                isFounded = LookForHuman(nameToSearch, usePFAuto, out humanFounded);

		//                if ((isFounded) || (humanFounded != null))
		//                {
		//                    nextState = 100;
		//                    break;
		//                }


		//                ///// -----   Using NEGATIVE Head Pan

		//                hdPanValue = hdPanInitial;
		//                for (i = 0; i < hdPanSteps; i++)
		//                {

		//                    hdPanValue -= hdPanIncrement;
		//                    cmdMan.HEAD_lookat(hdPanValue, hdTiltInitial, 4000);

		//                    isFounded = LookForHuman(nameToSearch, usePFAuto, out humanFounded);

		//                    if ((isFounded) || (humanFounded != null))
		//                    {
		//                        nextState = 100;
		//                        break;
		//                    }


		//                    if (useHeadTilt)
		//                    {
		//                        /// -- Positive Tilt

		//                        hdTiltValue = hdTiltInitial;
		//                        for (j = 0; j < hdTiltSteps; j++)
		//                        {
		//                            hdTiltValue += hdTiltIncrement;
		//                            cmdMan.HEAD_lookat(hdPanValue, hdTiltValue, 4000);

		//                            isFounded = LookForHuman(nameToSearch, usePFAuto, out humanFounded);

		//                            if ((isFounded) || (humanFounded != null))
		//                            {
		//                                nextState = 100;
		//                                break;
		//                            }
		//                        }

		//                        if ((isFounded) || (humanFounded != null))
		//                        {
		//                            nextState = 100;
		//                            break;
		//                        }


		//                        // Negative Tilt

		//                        hdTiltValue = hdTiltInitial;
		//                        for (k = 0; k < hdTiltSteps; k++)
		//                        {
		//                            hdTiltValue -= hdTiltIncrement;
		//                            cmdMan.HEAD_lookat(hdPanValue, hdTiltValue, 4000);

		//                            isFounded = LookForHuman(nameToSearch, usePFAuto, out humanFounded);
		//                            break;


		//                            if ((isFounded) || (humanFounded != null))
		//                            {
		//                                nextState = 100;
		//                                break;
		//                            }
		//                        }
		//                    }

		//                    if ((isFounded) || (humanFounded != null))
		//                    {
		//                        nextState = 100;
		//                        break;
		//                    }
		//                }

		//                if ((isFounded) || (humanFounded != null))
		//                    nextState = 100;
		//                else if (useTorso)
		//                    nextState = 20;
		//                else
		//                    nextState = 100;


		//                break;

		//            #endregion

		//            #region Using ONLY TILT Head Pan

		//            case 40:
		//                ///// -----   Using ONLY TILT Head Pan

		//                /// -- Positive Tilt

		//                ActualizeHeadPos();
		//                ActualizeTorsoPos();


		//                if ((isFounded) || (humanFounded != null))
		//                {
		//                    nextState = 100;
		//                    break;
		//                }

		//                cmdMan.HEAD_lookat(hdPanInitial, hdTiltInitial, 4000);
		//                isFounded = LookForHuman(nameToSearch, usePFAuto, out humanFounded);

		//                if ((isFounded) || (humanFounded != null))
		//                {
		//                    nextState = 100;
		//                    break;
		//                }

		//                hdTiltValue = hdTiltInitial;
		//                for (i = 0; i < hdTiltSteps; i++)
		//                {
		//                    hdTiltValue += hdTiltIncrement;
		//                    cmdMan.HEAD_lookat(hdPanInitial, hdTiltValue, 4000);

		//                    if (LookForHuman(nameToSearch, usePFAuto, out humanFounded))
		//                        break;
		//                }

		//                if ((isFounded) || (humanFounded != null))
		//                {
		//                    nextState = 100;
		//                    break;
		//                }


		//                // Negative Tilt

		//                hdTiltValue = hdTiltInitial;
		//                for (i = 0; i < hdTiltSteps; i++)
		//                {
		//                    hdTiltValue -= hdTiltIncrement;
		//                    cmdMan.HEAD_lookat(hdPanInitial, hdTiltValue, 4000);

		//                    if (LookForHuman(nameToSearch, usePFAuto, out humanFounded))
		//                        break;
		//                }

		//                if ((isFounded) || (humanFounded != null))
		//                {
		//                    nextState = 100;
		//                    break;
		//                }


		//                if ((isFounded) || (humanFounded != null))
		//                    nextState = 100;
		//                else if (useTorso)
		//                    nextState = 20;
		//                else
		//                    nextState = 100;
		//                break;

		//            #endregion

		//            #region Finished

		//            case 100:

		//                cmdMan.SP_GEN_shutdown();

		//                if (humanFounded == null)
		//                {
		//                    cmdFindHuman_Running = false;
		//                    human = null;

		//                    TBWriter.Spaced("    ---  Finished FindHuman Command  ---");
		//                    return false;

		//                }
		//                else
		//                {
		//                    cmdFindHuman_Running = false;
		//                    human = new Human(humanFounded.Name, humanFounded.Head);

		//                    robot.RememberHuman(human);

		//                    //cmdMan.HEAD_lookatobject( robot.TransRobot2Neck( human.Head), 4000);
							
		//                    TBWriter.Spaced("    ---  Finished FindHuman Command  ---");
		//                    return true;
		//                }
		//                break;

		//            default:

		//                cmdMan.SP_GEN_shutdown();

		//                if (humanFounded == null)
		//                {
		//                    cmdFindHuman_Running = false;
		//                    human = null;

		//                    TBWriter.Spaced("    ---  Finished FindHuman Command  ---");
		//                    return false;

		//                }
		//                else
		//                {
		//                    cmdFindHuman_Running = false;
		//                    human = new Human(humanFounded.Name, humanFounded.Head);

		//                    robot.RememberHuman(human);

		//                    //cmdMan.HEAD_lookatobject( robot.TransRobot2Neck( human.Head), 4000);

		//                    TBWriter.Spaced("    ---  Finished FindHuman Command  ---");
		//                    return true;
		//                }
		//                break;
					
		//                #endregion

		//        }
		//    }

		//    cmdMan.SP_GEN_shutdown();

		//    if (humanFounded == null)
		//    {
		//        cmdFindHuman_Running = false;
		//        human = null;

		//        TBWriter.Spaced("    ---  Finished FindHuman Command  ---");
		//        return false;

		//    }
		//    else
		//    {
		//        cmdFindHuman_Running = false;
		//        human = new Human(humanFounded.Name, humanFounded.Head);

		//        robot.RememberHuman(human);

		//        //cmdMan.HEAD_lookatobject( robot.TransRobot2Neck( human.Head), 4000);

		//        TBWriter.Spaced("    ---  Finished FindHuman Command  ---");
		//        return true;
		//    }

		//    cmdMan.SP_GEN_shutdown();

		//    TBWriter.Spaced("    ---  Finished FindHuman Command  --- success:");
		//    return false;
		//}

        public bool Cmd_FashionFindObject(out WorldObject[] objectArray)
        {
            TBWriter.Spaced("    ---  Starting FindObject Command  ---");

            objectArray = null;

            if (!robot.modules[Module.ObjectFnd].IsConnected)
            {
                TBWriter.Error("Module " + Module.ObjectFnd + " is not connected");
                TBWriter.Spaced("    ---  Finished FindObject Command  ---");
                return false;
            }


            double headPan = 0;
            double headTilt = -0.9;

            cmdMan.SPG_GEN_say("I am looking for Objects");

            TBWriter.Info2("Moving Head, pan=" + headPan.ToString() + " tilt=" + headTilt.ToString());
            cmdMan.HEAD_lookat(headPan, headTilt, 7000);

			Thread.Sleep(1500);

			if (!this.cmdMan.OBJ_FND_findOnTable((int)10000))
            {
                cmdMan.SPG_GEN_shutup();
                TBWriter.Error("OBJ_FND_findontable " + " timeOut reached");
                TBWriter.Spaced("    ---  Finished FindObject Command  ---");
                cmdMan.SPG_GEN_shutup();

                return false;
            }

            string objectsInfo = cmdMan.JustinaCmdAndResps[(int)JustinaCommands.OBJ_FND_findOnTable].Response.Parameters;

            WorldObject[] objKinect;
            if (!Parse.FindObjectOnTableInfo(objectsInfo, out objKinect))
            {
                cmdMan.SPG_GEN_shutup();
                TBWriter.Error(" Cant parse ObjectsOnTableInfo");
                TBWriter.Spaced("    ---  Finished FindObject Command  ---");
                cmdMan.SPG_GEN_shutup();
                return false;
            }


            //cmdMan.SPG_GEN_say("Haa! i found " + objKinect.Length + " object" + ((objKinect.Length > 1) ? "s" : string.Empty));
            TBWriter.Info1("Number of objects founded " + objKinect.Length);

            ActualizeHeadPos();
            ActualizeTorsoPos();

            Vector3 newPos;
            WorldObject newObj;

            robot.lastObjectsFounded.Clear();

            SortedList<double, WorldObject> orderedList = new SortedList<double, WorldObject>();

            int i = 0;
            foreach (WorldObject obj in objKinect)
            {
                newPos = robot.TransHeadKinect2Robot(obj.Position);
                newObj = new WorldObject(obj.Name, newPos, obj.DistanceFromTable);

                robot.LastObjFounded_Add(newObj);

                i++;
            }

            foreach (WorldObject wo in this.robot.lastObjectsFounded.Values)
            {
                double dist = wo.Position.Magnitude;

                while (orderedList.ContainsKey(dist))
                    dist += 0.001;

                orderedList.Add(dist, wo);
            }

            objectArray = new WorldObject[orderedList.Count];
            int j = 0;
            foreach (WorldObject obj in orderedList.Values)
            {
                objectArray[j] = orderedList.Values[j];
                TBWriter.Info2("Object founded No." + (j).ToString() + " = " + orderedList.Values[j].ToString());// . ToString());
                j++;
            }
            
            TBWriter.Spaced("    ---  Finished FindObject Command  ---");
            return true;
        }


        public bool Cmd_FashionFindObject__(out WorldObject[] objectArray)
        {
            TBWriter.Spaced("    ---  Starting FindObject Command  ---");

            objectArray = null;

            if (!robot.modules[Module.ObjectFnd].IsConnected)
            {
                TBWriter.Error("Module " + Module.ObjectFnd + " is not connected");
                TBWriter.Spaced("    ---  Finished FindObject Command  ---");
                return false;
            }


            double headPan = 0;
            double headTilt = -1.1;

            cmdMan.SPG_GEN_say("I am looking for Objects");

            TBWriter.Info2("Moving Head, pan=" + headPan.ToString() + " tilt=" + headTilt.ToString());
            cmdMan.HEAD_lookat(headPan, headTilt, 7000);


            cmdMan.SPG_GEN_playWait();
            

            TBWriter.Info2("Remembering objects Scene");
            
			if( !cmdMan.OBJ_FND_rememberForObjects(7000));
                TBWriter.Info2("Aunque ABEL dijo que esto nunca sucederia, remember regreso 0");
            
            TBWriter.Info2("Looking objects in scene");
            cmdMan.OBJ_FND_findRememberedObjects(headTilt);


            while (!cmdMan.IsResponseReceived(JustinaCommands.OBJ_FND_findOnTable))
            {
                double incTitl = -MathUtil.ToRadians( randomNum.Next( 30, 60));
                double incPan = MathUtil.ToRadians( randomNum.Next( -30, 31));

                cmdMan.HEAD_lookat(incPan, incTitl, 7000);

                Thread.Sleep( randomNum.Next( 400, 1000));
            }

            TBWriter.Info2("findonTable response received");

            TBWriter.Info2("Moving Head, pan=" + headPan.ToString() + " tilt=" + headTilt.ToString());
            cmdMan.HEAD_lookat(headPan, headTilt, 7000);
            cmdMan.SPG_GEN_shutup();
             
            if (!cmdMan.JustinaCmdAndResps[(int)JustinaCommands.OBJ_FND_findOnTable].Response.Success)
            {
                cmdMan.SPG_GEN_shutup();
                TBWriter.Error("OBJ_FND_findontable " + " timeOut reached");
                TBWriter.Spaced("    ---  Finished FindObject Command  ---");
                cmdMan.SPG_GEN_shutup();
                
                return false;
            }

            string objectsInfo = cmdMan.JustinaCmdAndResps[(int)JustinaCommands.OBJ_FND_findOnTable].Response.Parameters;

            WorldObject[] objKinect;
            if (!Parse.FindObjectOnTableInfo(objectsInfo, out objKinect))
            {
                cmdMan.SPG_GEN_shutup();
                TBWriter.Error(" Cant parse ObjectsOnTableInfo");
                TBWriter.Spaced("    ---  Finished FindObject Command  ---");
                cmdMan.SPG_GEN_shutup();
                return false;
            }

            cmdMan.SPG_GEN_shutup();
            Thread.Sleep(200);
            cmdMan.SPG_GEN_shutup();
            Thread.Sleep(200);
            cmdMan.SPG_GEN_shutup();
            Thread.Sleep(200);



            //cmdMan.SPG_GEN_say("Haa! i found " + objKinect.Length + " object" + ((objKinect.Length > 1) ? "s" : string.Empty));
            TBWriter.Info1("Number of objects founded " + objKinect.Length);

            ActualizeHeadPos();
            ActualizeTorsoPos();

            Vector3 newPos;
            WorldObject newObj;

            robot.lastObjectsFounded.Clear();

			SortedList<double, WorldObject> orderedList = new SortedList<double, WorldObject>();

            int i = 0;
            foreach (WorldObject obj in objKinect)
            {
                newPos = robot.TransHeadKinect2Robot(obj.Position);
                newObj = new WorldObject(obj.Name, newPos, obj.DistanceFromTable);

                robot.LastObjFounded_Add(newObj);
       		
				i++;
            }

			foreach (WorldObject wo in this.robot.lastObjectsFounded.Values)
			{
				double dist = wo.Position.Magnitude;

				while (orderedList.ContainsKey(dist))
					dist += 0.001;

				orderedList.Add(dist, wo);
			}

            objectArray = new WorldObject[orderedList.Count];
            int j = 0;
            foreach (WorldObject obj in orderedList.Values)
            {
                objectArray[j] = orderedList.Values[j];
                TBWriter.Info2("Object founded No." + (j).ToString() + " = " + orderedList.Values[j].Name);
                j++;
            }

			//objectArray = new WorldObject[robot.lastObjectsFounded.Values.Count];
			//int j = 0;
			//foreach (string objNam in robot.lastObjectsFounded.Keys)
			//{
			//    objectArray[j] = robot.lastObjectsFounded[objNam];
			//    TBWriter.Info2("Object founded No." + (j).ToString() + " = " + objNam);
			//    j++;
			//}

            TBWriter.Spaced("    ---  Finished FindObject Command  ---");
            return true;
        }
        
        public bool Cmd_FindObject(out WorldObject[] objectArray)
        {
            TBWriter.Spaced("    ---  Starting FindObject Command  ---");

            objectArray = null;

            if (!robot.modules[Module.ObjectFnd].IsConnected)
            {
                TBWriter.Error("Module " + Module.ObjectFnd + " is not connected");
                TBWriter.Spaced("    ---  Finished FindObject Command  ---");
                return false;
            }
            if (!robot.modules[Module.Head].IsConnected)
            {
                TBWriter.Error("Module " + Module.Head + " is not connected");
                TBWriter.Spaced("    ---  Finished FindObject Command  ---");
                return false;
            }


            double headPan = 0;
            double headTilt = -1.1;


            cmdMan.SPG_GEN_say("I am looking for Objects");

            TBWriter.Info2("Moving Head, pan=" + headPan.ToString() + " tilt=" + headTilt.ToString());
            cmdMan.HEAD_lookat(headPan, headTilt, 7000);


            cmdMan.SPG_GEN_playWait();

            int timeOutFindOnTable_ms = 120000;
            TBWriter.Info2("Finding objects on table");
            if (!cmdMan.OBJ_FND_findontable(headTilt, timeOutFindOnTable_ms))
            {
                cmdMan.SPG_GEN_shutup();
                TBWriter.Error("OBJ_FND_findontable " + timeOutFindOnTable_ms + "ms timeOut reached");
                TBWriter.Spaced("    ---  Finished FindObject Command  ---");
                return false;
            }

            string objectsInfo = cmdMan.JustinaCmdAndResps[(int)JustinaCommands.OBJ_FND_findOnTable].Response.Parameters;

            WorldObject[] objKinect;
            if (!Parse.FindObjectOnTableInfo(objectsInfo, out objKinect))
            {
                cmdMan.SPG_GEN_shutup();
                TBWriter.Error(" Cant parse ObjectsOnTableInfo");
                TBWriter.Spaced("    ---  Finished FindObject Command  ---");
                return false;
            }

            cmdMan.SPG_GEN_shutup();
            cmdMan.SPG_GEN_say("Haa! i found " + objKinect.Length + " object" + ((objKinect.Length > 1) ? "s" : string.Empty));
            TBWriter.Info1("Number of objects founded " + objKinect.Length);

            ActualizeHeadPos();
            ActualizeTorsoPos();

            Vector3 newPos;
            WorldObject newObj;

            robot.lastObjectsFounded.Clear();

            int i = 0;
            foreach (WorldObject obj in objKinect)
            {
                newPos = robot.TransHeadKinect2Robot(obj.Position);
                newObj = new WorldObject(obj.Name, newPos, obj.DistanceFromTable);

                robot.LastObjFounded_Add(newObj);
                i++;
            }

            objectArray = new WorldObject[robot.lastObjectsFounded.Values.Count];
            int j = 0;
            foreach (string objNam in robot.lastObjectsFounded.Keys)
            {
                objectArray[j] = robot.lastObjectsFounded[objNam];
                TBWriter.Info2("Object founded No." + (j).ToString() + " = " + objNam);
                j++;
            }

            TBWriter.Spaced("    ---  Finished FindObject Command  ---");
            return true;
        }

        public bool Cmd_FindObjOnShelf(out string[] objectsFoundedName)
        {
            TBWriter.Spaced("    ---  Starting FindObjectOnShelf Command  ---");

            objectsFoundedName = null;

            if (!robot.modules[Module.ObjectFnd].IsConnected)
            {
                TBWriter.Error("Module " + Module.ObjectFnd + " is not connected");
                TBWriter.Spaced("    ---  Finished FindObjectOnShelf Command  ---");
                return false;
            }
            if (!robot.modules[Module.Head].IsConnected)
            {
                TBWriter.Error("Module " + Module.Head + " is not connected");
                TBWriter.Spaced("    ---  Finished FindObjectOnShelf Command  ---");
                return false;
            }


            double headPan = 0;
            double headTilt = -MathUtil.ToRadians(55);


            cmdMan.SPG_GEN_say("I am looking for Objects on this shelf");


            TBWriter.Info2("Moving Head, pan=" + headPan.ToString() + " tilt=" + headTilt.ToString());
            cmdMan.HEAD_lookat(headPan, headTilt, 4000);


            cmdMan.SPG_GEN_playWait();


            TBWriter.Info2("Finding objects on shelf");
            if (!cmdMan.OBJ_FND_findObjectsOnshelf(headTilt, 90000))
            {
                cmdMan.SPG_GEN_shutup();
                TBWriter.Error(" OBJ_FND_findObjectsOnshelf returned false");
                TBWriter.Spaced("    ---  Finished FindObjectOnShelf Command  ---");
                return false;
            }


            string objectsInfo = cmdMan.JustinaCmdAndResps[(int)JustinaCommands.OBJ_FND_findObjectsOnshelf].Response.Parameters;

            TBWriter.Info2("Parsing objects on shelf");
            WorldObject[] objKinect;
            if (!Parse.FindObjectOnTableInfo(objectsInfo, out objKinect))
            {
                cmdMan.SPG_GEN_shutup();
                TBWriter.Error(" Cant Parse.FindObjonSHELFInfo");
                TBWriter.Spaced("    ---  Finished FindObjectOnShelf Command  ---");
                return false;
            }



            cmdMan.SPG_GEN_shutup();
            cmdMan.SPG_GEN_say("Haa! i found " + objKinect.Length + " object" + ((objKinect.Length > 1) ? "s" : string.Empty + "on the shelf"));
            TBWriter.Info1("Number of objects founded " + objKinect.Length);

            ActualizeHeadPos();
            ActualizeTorsoPos();


            //// Obtaining planes
            ////


            Vector3 newPos;
            WorldObject newObj;

            robot.LastObjFoundedClear();

            int i = 0;
            foreach (WorldObject obj in objKinect)
            {
                newPos = robot.TransHeadKinect2Robot(obj.Position);
                newObj = new WorldObject(obj.Name, newPos);

                robot.LastObjFounded_Add(newObj);
                i++;
            }


            objectsFoundedName = new string[robot.lastObjectsFounded.Keys.Count];
            robot.lastObjectsFounded.Keys.CopyTo(objectsFoundedName, 0);

            for (int j = 0; j < robot.lastObjectsFounded.Keys.Count; j++)
            {
                TBWriter.Info2("Object founded No." + j + " = " + robot.lastObjectsFounded.Keys[j]);
            }

            TBWriter.Spaced("    ---  Finished FindObjectOnShelf Command  ---");
            return true;
        }

        public bool Cmd_Holding(out string holdingInfo)
        {
            TBWriter.Spaced("    ---  Starting Holding Command  ---");

            holdingInfo = "";
            string leftholding = "";
            string rightholding = "";

            if (robot.LeftArmIsEmpty)
            {
                leftholding += "empty" + " ";
            }
            else
            {
                leftholding += leftArmObject + " ";
            }

            if (robot.RightArmIsEmpty)
            {
                rightholding += "empty" + " ";
            }
            else
            {
                rightholding += rightArmObject + " "; ;
            }


            holdingInfo = leftholding + rightholding;

            TBWriter.Info4("Lefthand object=" + leftholding + " ; RightHand object =" + rightholding);
            TBWriter.Spaced("    ---  Finished Holding Command  ---");
            
            
            
            return true;
        }

		public bool Cmd_RememberHuman(string humanName)
		{
            TBWriter.Spaced("    ---  Starting RememberHummand Command  --- ");

            if (!robot.modules[Module.PersonFnd].IsConnected)
            {
                TBWriter.Error("Cant execute RememberHummand," + Module.PersonFnd + " is not connected");
                TBWriter.Spaced("    ---  Finished RememberHummand Command  ---");
                return false;
            }

			ActualizeHeadPos();
			ActualizeTorsoPos();

			
			bool success = false;


			bool rememberSuccess;


			cmdMan.SPG_GEN_say("Please don't move.", 10000);
			Thread.Sleep(2500);

			// Casual face
			//cmdMan.SPG_GEN_say("Please, try to look into my eyes. I will remember your face", 5000);


			for (int i = 0; i < 3; i++)
			{
				//Thread.Sleep(100);
				rememberSuccess = (cmdMan.PRS_FND_rememeber(humanName, 5000));
				
				if (rememberSuccess)
				{
                    cmdMan.SPG_GEN_play("snapshot.mp3");
					TBWriter.Info2("Casual face remembered  " + humanName);
					success = true;
					Thread.Sleep(1800);
					break;
				}

				TBWriter.Warning2("No person detected, attempt No. " + (i + 1).ToString());
				cmdMan.SPG_GEN_say(humanName + "Please don't move");
				Thread.Sleep(1800);
			}



			// Smiley face;

			cmdMan.SPG_GEN_say(humanName+ ", smile", 500);
			for (int i = 0; i < 2; i++)
			{
				if (cmdMan.PRS_FND_rememeber(humanName, 5000))
				{
					cmdMan.SPG_GEN_play("snapshot.mp3");
					TBWriter.Info2("Smiley face remembered  " + humanName);
					success = true;
					Thread.Sleep(1800);
					break;
				}

				TBWriter.Warning2("No smile person detected, attempt No. " + (i + 1).ToString());
				//cmdMan.SPG_GEN_say("One more time, smile and look at my face, don't move", 5000);
				cmdMan.SPG_GEN_say("Please, smile and don't move");
				Thread.Sleep(1800);
			}


			// Serious face;

			//cmdMan.SPG_GEN_say("Last one " + humanName + " , but now, a serious face",5000);
			cmdMan.SPG_GEN_say("Now, a serious face", 500);
			for (int i = 0; i < 2; i++)
			{
				if (cmdMan.PRS_FND_rememeber(humanName, 5000))
				{
                    cmdMan.SPG_GEN_play("snapshot.mp3");
					TBWriter.Info2("Serious face remembered  " + humanName);
					success = true;
					break;
				}

				TBWriter.Warning2("No person detected, attempt No. " + (i + 1).ToString());
				cmdMan.SPG_GEN_say("Again, and don't move",5000);
				Thread.Sleep(2000);
			}


			TBWriter.Spaced("    ---  Finished RememberHummand Command  ---");
	
			return success;
		}

        public bool Cmd_StartLearn(string motionName)
        {
            TBWriter.Spaced("    ---  Starting StartLearnArmMotion Command  ---");
            TBWriter.Info1("    ->  MotionName: " + motionName);

            learnMotion_name = motionName;

            learnMotion = new Thread(new ThreadStart(learnArmMotionThreadTask));
            learnMotion.IsBackground = true;

            learnMotionRunning = true;
            learnMotion.Start();

            return true;
        }

        public bool Cmd_StopLearn()
        {
            TBWriter.Spaced("    ---  Starting StopLearnArmMotion Command  ---");

            learnMotionRunning = false;
            return true;
        }

        public bool Cmd_TakeObjFromShelf(string firstObject, string secondObject, string armSelection, out string objectsTaked)
        {
            TBWriter.Spaced("    ---  Starting TakeObjectFromShelf Command  ---");
            TBWriter.Info1("    ---  Params received : " + firstObject + " " + secondObject + ", " + armSelection);

            objectsTaked = "";

            if (string.IsNullOrEmpty(firstObject) && string.IsNullOrEmpty(secondObject))
            {
                TBWriter.Error("Obbjects are null or empty");
                TBWriter.Spaced("    ---  Finishing TakeObjectFromShelf Command  ---");
                return false;
            }
            if (!robot.modules[Module.Arms].IsConnected)
            {
                TBWriter.Error("Can't execute TakeObjectFromShelf, [ Arms ] is not connected");
                TBWriter.Spaced("    ---  Finishing TakeObjectFromShelf Command  ---");
                return false;
            }


            WorldObject obj1 = null;
            WorldObject obj2 = null;

            TBWriter.Info1("Searching objects in objectsInShelfList : ");

            if (robot.lastObjectsFounded.ContainsKey(firstObject))
            {
                obj1 = new WorldObject(robot.lastObjectsFounded[firstObject]);
                TBWriter.Info1("                  > Obj 1. founded in object List, preparing to take it" + obj1.Name);
            }
            else
            {
                TBWriter.Info1("                  > " + firstObject + " not founded in objectsList, can't take it");
            }
            if (robot.lastObjectsFounded.ContainsKey(secondObject))
            {
                obj2 = new WorldObject(robot.lastObjectsFounded[secondObject]);
                TBWriter.Info1("                  > Obj 2. founded in object List, preparing to take it" + obj1.Name);
            }
            else
            {
                TBWriter.Info1("                  > " + secondObject + " not founded in objectsList, can't take it");
            }


            cmdMan.ARMS_la_goto(Arm.ppNavigation);
            if (!cmdMan.ARMS_ra_goto(Arm.ppNavigation, 8000))
            {
                TBWriter.Error("Arms can't go to navigation");
                TBWriter.Spaced("    ---  Finishing TakeObjectFromShelf Command  ---");
                return false;
            }

            double[] torsoDefaultPos = new double[2];
            torsoDefaultPos[0] = 0.7;
            torsoDefaultPos[1] = 0.0;

            ActualizeHeadPos();

            if (!ActualizeTorsoPos())
                robot.torso.ActualizeTorsoStatus(torsoDefaultPos);

            double torsoInitialElevation = robot.torso.Elevation;


            double torsoPan = 0;
            double torsoElevation = robot.torso.Elevation;
            double hdPan = 0;
            double hdTilt = MathUtil.ToRadians(55);

            Vector3 errorVec;
            double errorAngle;

            Vector3 armRightRobot;
            Vector3 armLeftRobot;

            double idealheightFromArmToObj = 0.4;

            Vector3 obj1RightArmPos;
            Vector3 obj1LeftArmPos;
            double obj1distToRight = double.MaxValue;
            double obj1distToLeft = double.MaxValue;
            bool obj1isRightReach = false;
            bool obj1isLeftReach = false;
            bool obj1isBothReach = false;
            bool obj1isClosestToRight = false;

            Vector3 obj2RightArmPos;
            Vector3 obj2LeftArmPos;
            double obj2disToRight = double.MaxValue;
            double obj2disToLeft = double.MaxValue;
            bool obj2isRightReach = false;
            bool obj2isLeftReach = false;
            bool obj2isBothReach = false;
            bool obj2isClosestToRight = false;

            bool obj1gotClose = false;
            bool obj2gotClose = false;

            int nextState = 0;
            bool cmd_TakeObjectRunning = true;

            while (cmd_TakeObjectRunning)
            {
                switch (nextState)
                {
                    case 0:

                        #region State 0 : Verifying Parameters

                        TBWriter.Info1("State 0 : Verifying Parameters");

                        if ((obj1 == null) && (obj2 == null))
                        {
                            TBWriter.Info1(" Both objects are null");
                            nextState = 100;
                            break;
                        }

                        ActualizeHeadPos();
                        ActualizeTorsoPos();

                        if ((obj1 != null) && (!obj1gotClose))
                        {
                            nextState = 80;
                            break;
                        }
                        if ((obj2 != null) && (!obj2gotClose))
                        {
                            nextState = 90;
                            break;
                        }



                        #region Cheking Arms Reachability and closest

                        armRightRobot = robot.TransRightArm2Robot(Vector3.Zero);
                        armLeftRobot = robot.TransLeftArm2Robot(Vector3.Zero);

                        #region Checking Obj1

                        if (obj1 != null)
                        {
                            TBWriter.Info2("Checking Obj1 : " + obj1.Name);

                            /////////////

                            TBWriter.Info8("Checking Obj1 for RightArm");

                            errorVec = obj1.Position - armRightRobot;
                            errorAngle = Math.Atan2(errorVec.Y, errorVec.X);

                            obj1distToRight = errorVec.Magnitude;
                            obj1RightArmPos = robot.TransRobot2RightArm(obj1.Position);
                            obj1isRightReach = cmdMan.ARMS_ra_reachable(obj1RightArmPos.X, obj1RightArmPos.Y, obj1RightArmPos.Z, MathUtil.PiOver2, errorAngle, 0, -errorAngle, 5000);

                            if (obj1isRightReach)
                                TBWriter.Info8("Obj1 reachable for RightArm");
                            else
                                TBWriter.Info8("Obj1 is not reachable for RightArm");

                            /////////////

                            TBWriter.Info1("Checking Obj1 for LeftArm");

                            errorVec = obj1.Position - armLeftRobot;
                            errorAngle = Math.Atan2(errorVec.Y, errorVec.X);

                            obj1distToLeft = errorVec.Magnitude;
                            obj1LeftArmPos = robot.TransRobot2LeftArm(obj1.Position);
                            obj1isLeftReach = cmdMan.ARMS_la_reachable(obj1LeftArmPos.X, obj1LeftArmPos.Y, obj1LeftArmPos.Z, MathUtil.PiOver2, errorAngle, 0, -errorAngle, 5000);

                            if (obj1isLeftReach)
                                TBWriter.Info8("Obj1 reachable for LeftArm");
                            else
                                TBWriter.Info8("Obj1 is not reachable for LeftArm");

                            /////////////

                            if (obj1isRightReach && obj1isLeftReach)
                            {
                                TBWriter.Info8("Obj1 is both Reachable");
                                obj1isBothReach = true;
                            }

                            if (obj1distToRight <= obj1distToLeft)
                            {
                                TBWriter.Info8("Obj1 is closest to Right");
                                obj1isClosestToRight = true;
                            }
                        }

                        #endregion

                        #region Checking Obj2

                        if (obj2 != null)
                        {
                            TBWriter.Info2("Checking Obj2 : " + obj2.Name);

                            /////////////

                            TBWriter.Info8("Checking Obj2 for RightArm");

                            errorVec = obj2.Position - armRightRobot;
                            errorAngle = Math.Atan2(errorVec.Y, errorVec.X);

                            obj2disToRight = errorVec.Magnitude;
                            obj2RightArmPos = robot.TransRobot2RightArm(obj2.Position);
                            obj2isRightReach = cmdMan.ARMS_ra_reachable(obj2RightArmPos.X, obj2RightArmPos.Y, obj2RightArmPos.Z, MathUtil.PiOver2, errorAngle, 0, -errorAngle, 5000);

                            if (obj2isRightReach)
                                TBWriter.Info8("Obj2 reachable for RightArm");
                            else
                                TBWriter.Info8("Obj2 is not reachable for RightArm");

                            /////////////

                            TBWriter.Info8("Checking Obj2 for LeftArm");

                            errorVec = obj2.Position - armLeftRobot;
                            errorAngle = Math.Atan2(errorVec.Y, errorVec.X);

                            obj2disToLeft = errorVec.Magnitude;
                            obj2LeftArmPos = robot.TransRobot2LeftArm(obj2.Position);
                            obj2isLeftReach = cmdMan.ARMS_la_reachable(obj2LeftArmPos.X, obj2LeftArmPos.Y, obj2LeftArmPos.Z, MathUtil.PiOver2, errorAngle, 0, -errorAngle, 5000);

                            if (obj2isLeftReach)
                                TBWriter.Info8("Obj2 reachable for LeftArm");
                            else
                                TBWriter.Info8("Obj2 is not reachable for LeftArm");

                            /////////////

                            if (obj2isRightReach && obj2isLeftReach)
                            {
                                TBWriter.Info8("Obj2 is both Reachable");
                                obj2isBothReach = true;
                            }

                            if (obj2disToRight <= obj2disToLeft)
                            {
                                TBWriter.Info8("Obj2 is closest to Right");
                                obj2isClosestToRight = true;
                            }
                        }

                        #endregion

                        #endregion



                        if (armSelection == "right")
                            nextState = 10;
                        else if (armSelection == "left")
                            nextState = 15;
                        else if ((obj1 != null) && (obj2 != null))
                            nextState = 30;
                        else if ((obj1 == null) && (obj2 == null))
                            nextState = 100;
                        else
                            nextState = 40;

                        break;


                        #endregion

                    case 10:

                        #region State 10: Using Right Arm to take obj1

                        TBWriter.Info1("State 10: Using Right Arm to take obj1");

                        if (obj1isRightReach)
                        {
                            if (takeRightArm(obj1))
                            {
                                //if (!cmdMan.OBJ_FND_findObjectsOnshelf(obj1.Name, robot.head.Tilt, 30000))
                                //{
                                //    TBWriter.Info1("Taked obj1 " + obj1.Name + " with rightHand");
                                //    objectsTaked += obj1.Name + " ";
                                //    rightArmObject = obj1.Name;
                                //    robot.RightArmIsEmpty = false;
                                //}
                                //else
                                //{
                                //    TBWriter.Info1("Obj1 " + obj1.Name + " not taked ");
                                //    rightArmObject = "";
                                //    robot.RightArmIsEmpty = true;
                                //}

                                TBWriter.Info1("Taked obj1 " + obj1.Name + " with rightHand");
                                objectsTaked += obj1.Name + " ";
                                rightArmObject = obj1.Name;
                                robot.RightArmIsEmpty = false;
                            }
                            else
                            {
                                TBWriter.Error("takeRightArm returned false");
                                TBWriter.Info1("Obj1 " + obj1.Name + " not taked ");
                                rightArmObject = "";
                                robot.RightArmIsEmpty = true;
                            }

                            obj1 = null;
                            nextState = 0;
                        }
                        else
                        {
                            TBWriter.Info1("Obj1 " + obj1.Name + " is not reachable  with rightArm, trying getting close");
                            nextState = 80;
                        }

                        TBWriter.Info1(" >> Finishing State 10: Using Right Arm to take obj1");
                        break;

                        #endregion

                    case 15:

                        #region State 15: Using Left Arm to take obj1

                        TBWriter.Info1(" >> State 15: Using Left Arm to take obj1");

                        if (obj1isLeftReach)
                        {
                            if (takeLeftArm(obj1))
                            {
                                //if (!cmdMan.OBJ_FND_findObjectsOnshelf(obj1.Name, robot.head.Tilt, 30000))
                                //{
                                //    TBWriter.Info1("Taked obj1 " + obj1.Name + " with leftHand ");
                                //    objectsTaked += obj1.Name;
                                //    leftArmObject = obj1.Name;
                                //    robot.LeftArmIsEmpty = false;
                                //}
                                //else
                                //{
                                //    TBWriter.Info1("Obj1 " + obj1.Name + " not taked, ");
                                //    leftArmObject = "";
                                //    robot.LeftArmIsEmpty = true;
                                //}

                                TBWriter.Info1("Taked obj1 " + obj1.Name + " with leftHand ");
                                objectsTaked += obj1.Name;
                                leftArmObject = obj1.Name;
                                robot.LeftArmIsEmpty = false;
                            }
                            else
                            {
                                TBWriter.Error("takeLeftArm returned false");
                                TBWriter.Info1("Obj1 " + obj1.Name + " not taked, ");
                                leftArmObject = "";
                                robot.LeftArmIsEmpty = true;
                            }

                            obj1 = null;
                            nextState = 0;

                        }
                        else
                        {
                            TBWriter.Info1("Obj1 " + obj1.Name + " is not reachable  with leftArm, trying getting close");
                            nextState = 80;

                        }

                        TBWriter.Info1(" >> Finishing State 15: Using Left Arm to take obj1");

                        break;

                        #endregion

                    case 20:

                        #region State 20: Using Right Arm to take obj2

                        TBWriter.Info1("State 20: Using Right Arm to take obj2");

                        if (obj2isRightReach)
                        {
                            if (takeRightArm(obj2))
                            {
                                //if (!cmdMan.OBJ_FND_findObjectsOnshelf(obj2.Name, robot.head.Tilt, 30000))
                                //{
                                //    TBWriter.Info1("Taked obj2 " + obj2.Name + " with rightHand");
                                //    objectsTaked += obj2.Name + " ";
                                //    rightArmObject = obj2.Name;
                                //    robot.RightArmIsEmpty = false;
                                //}
                                //else
                                //{
                                //    TBWriter.Info1("Obj2 " + obj2.Name + " not taked ");
                                //    rightArmObject = "";
                                //    robot.RightArmIsEmpty = true;
                                //}

                                TBWriter.Info1("Taked obj2 " + obj2.Name + " with rightHand");
                                objectsTaked += obj2.Name + " ";
                                rightArmObject = obj2.Name;
                                robot.RightArmIsEmpty = false;
                            }
                            else
                            {
                                TBWriter.Error("takeRightArm returned false");
                                TBWriter.Info1("Obj2 " + obj2.Name + " not taked ");
                                rightArmObject = "";
                                robot.RightArmIsEmpty = true;
                            }

                            obj2 = null;
                            nextState = 0;
                        }
                        else
                        {
                            TBWriter.Info1("Obj2 " + obj2.Name + " is not reachable with rightArm, trying getting close");
                            nextState = 90;
                        }

                        TBWriter.Info1(" >> Finishing State 20: Using Right Arm to take obj2");
                        break;

                        #endregion

                    case 25:

                        #region State 25: Using Left Arm to take obj2

                        TBWriter.Info1(" >> State 15: Using Left Arm to take obj2");

                        if (obj2isLeftReach)
                        {
                            if (takeLeftArm(obj2))
                            {
                                //if (!cmdMan.OBJ_FND_findObjectsOnshelf(obj2.Name, robot.head.Tilt, 30000))
                                //{
                                //    TBWriter.Info1("Taked obj2 " + obj2.Name + " with leftHand ");
                                //    objectsTaked += obj2.Name;
                                //    leftArmObject = obj2.Name;
                                //    robot.LeftArmIsEmpty = false;
                                //}
                                //else
                                //{
                                //    TBWriter.Info1("Obj2 " + obj2.Name + " not taked, ");
                                //    leftArmObject = "";
                                //    robot.LeftArmIsEmpty = true;
                                //}

                                TBWriter.Info1("Taked obj2 " + obj2.Name + " with leftHand ");
                                objectsTaked += obj2.Name;
                                leftArmObject = obj2.Name;
                                robot.LeftArmIsEmpty = false;
                            }
                            else
                            {
                                TBWriter.Error("takeLeftArm returned false");
                                TBWriter.Info1("Obj2 " + obj2.Name + " not taked, ");
                                leftArmObject = "";
                                robot.LeftArmIsEmpty = true;
                            }

                            obj2 = null;
                            nextState = 0;

                        }
                        else
                        {
                            TBWriter.Info1("Obj2" + obj2.Name + " is not reachable  with leftArm, trying getting close");
                            nextState = 80;
                        }

                        TBWriter.Info1(" >> Finishing State 25: Using Left Arm to take obj2");

                        break;

                        #endregion

                    case 30:

                        #region State 30 : Taking Both objects

                        TBWriter.Info1("State 30 : Taking Both objects");

                        #region Both objects are both reachable

                        if (obj1isBothReach && obj2isBothReach)
                        {
                            TBWriter.Info2("Both objects are both reachable");

                            if (obj1isClosestToRight && obj2isClosestToRight)
                            {
                                if (obj1distToLeft <= obj2disToLeft)
                                    takeBothArmsOnShelf(obj2, obj1, out objectsTaked);
                                else
                                    takeBothArmsOnShelf(obj1, obj1, out objectsTaked);
                            }

                            else if (!obj1isClosestToRight && !obj2isClosestToRight) // >> Then both are closest to Left
                            {
                                if (obj1distToRight <= obj2disToRight)
                                    takeBothArmsOnShelf(obj1, obj2, out objectsTaked);
                                else
                                    takeBothArmsOnShelf(obj2, obj1, out objectsTaked);
                            }

                            else if (obj1isClosestToRight)
                            {
                                takeBothArmsOnShelf(obj1, obj2, out objectsTaked);
                            }

                            else if (obj2isClosestToRight)
                            {
                                takeBothArmsOnShelf(obj2, obj1, out objectsTaked);
                            }

                            TBWriter.Spaced("    ---  Finishing FindObject Command  ---");
                            return true;
                        }

                        #endregion

                        #region Only Obj1 is Both Reachable

                        else if (obj1isBothReach)
                        {
                            TBWriter.Info2("Only Obj1 is Both Reachable");

                            if (obj2isRightReach)
                            {
                                takeBothArmsOnShelf(obj2, obj1, out objectsTaked);
                            }

                            else if (obj2isLeftReach)
                            {
                                takeBothArmsOnShelf(obj1, obj2, out objectsTaked);
                            }

                            else
                            {
                                if (obj1isClosestToRight)
                                {
                                    nextState = 10;
                                    break;
                                }
                                else
                                {
                                    nextState = 15;
                                    break;
                                }
                            }

                            TBWriter.Spaced("    ---  Finishing FindObject Command  ---");
                            return true;
                        }

                        #endregion

                        #region Only Obj2 is Both Reachable

                        else if (obj2isBothReach)
                        {
                            TBWriter.Info2("Only Obj2 is Both Reachable");

                            if (obj1isRightReach)
                            {
                                takeBothArmsOnShelf(obj1, obj2, out objectsTaked);
                            }

                            else if (obj1isLeftReach)
                            {
                                takeBothArmsOnShelf(obj2, obj1, out objectsTaked);
                            }

                            else
                            {
                                if (obj2isClosestToRight)
                                {
                                    nextState = 20;
                                    break;
                                }
                                else
                                {
                                    nextState = 25;
                                    break;
                                }
                            }

                            TBWriter.Spaced("    ---  Finishing FindObject Command  ---");
                            return true;
                        }

                        #endregion

                        #region None is Both Reachable

                        else if ((!obj1isBothReach) && (!obj2isBothReach))
                        {
                            TBWriter.Info2("None is Both Reachable");

                            if (obj1isRightReach)
                            {
                                if (obj2isLeftReach)
                                {
                                    takeBothArmsOnShelf(obj1, obj2, out objectsTaked);
                                    TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                    return true;
                                }
                                else
                                {
                                    nextState = 10;
                                    break;
                                }
                            }

                            else if (obj2isRightReach)
                            {
                                if (obj1isLeftReach)
                                {
                                    takeBothArmsOnShelf(obj2, obj1, out objectsTaked);
                                    TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                    return true;
                                }
                                else
                                {
                                    nextState = 20;
                                    break;
                                }
                            }

                            // If non is reachable
                            else
                            {
                                if (obj1.Position.Magnitude <= obj2.Position.Magnitude)
                                {
                                    TBWriter.Info3(" getting close to obj1");
                                    nextState = 80;
                                    break;
                                }
                                else
                                {
                                    TBWriter.Info3(" getting close to obj2");
                                    nextState = 80;
                                    break;
                                }
                            }
                        }

                        #endregion

                        break;

                        #endregion

                    case 40:

                        #region State 40 : Checking which arm to use

                        TBWriter.Info1("State 40 : Checking which arm to use");

                        #region Taking obj1

                        if (obj1 != null)
                        {
                            TBWriter.Info1("obj1 " + obj1.Name + " is not null");

                            #region If BOTH arms are empty Or NONE is empty

                            if ((robot.RightArmIsEmpty && robot.LeftArmIsEmpty) || (!robot.RightArmIsEmpty && !robot.LeftArmIsEmpty))
                            {
                                if (obj1isBothReach)
                                {
                                    if (obj1isClosestToRight)
                                    {
                                        nextState = 10;
                                        break;
                                    }
                                    else
                                    {
                                        nextState = 15;
                                        break;
                                    }
                                }
                                else if (obj1isRightReach)
                                {
                                    nextState = 10;
                                    break;
                                }
                                else if (obj1isLeftReach)
                                {
                                    nextState = 15;
                                    break;
                                }
                                else
                                {
                                    nextState = 80;
                                    break;
                                }
                            }

                            #endregion

                            #region If Right Arm is Empty

                            else if (robot.RightArmIsEmpty)
                            {
                                if (obj1isRightReach)
                                {
                                    nextState = 10;
                                    break;
                                }
                                else
                                {
                                    nextState = 80;
                                    break;
                                }
                            }

                            #endregion

                            #region If Left arm is empty

                            else if (robot.LeftArmIsEmpty)
                            {
                                if (obj1isLeftReach)
                                {
                                    nextState = 15;
                                    break;
                                }
                                else
                                {
                                    nextState = 80;
                                    break;
                                }

                                TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                return true;
                            }
                            #endregion
                        }

                        #endregion

                        #region taking obj2

                        if (obj2 != null)
                        {

                            #region If BOTH Arms are empty or NONE is empty

                            if ((robot.RightArmIsEmpty && robot.LeftArmIsEmpty) || (!robot.RightArmIsEmpty && !robot.LeftArmIsEmpty))
                            {
                                if (obj2isBothReach)
                                {
                                    if (obj2isClosestToRight)
                                    {
                                        nextState = 20;
                                        break;
                                    }
                                    else
                                    {
                                        nextState = 25;
                                        break;
                                    }
                                }
                                else if (obj2isRightReach)
                                {
                                    nextState = 20;
                                    break;
                                }
                                else if (obj2isLeftReach)
                                {
                                    nextState = 25;
                                    break;
                                }
                                else
                                {
                                    nextState = 90;
                                    break;
                                }
                            }

                            #endregion

                            #region If right arm is empty

                            else if (robot.RightArmIsEmpty)
                            {
                                if (obj2isRightReach)
                                {
                                    nextState = 20;
                                    break;
                                }
                                else
                                {
                                    nextState = 90;
                                    break;
                                }
                            }

                            #endregion

                            #region If left arm is empty

                            else if (robot.LeftArmIsEmpty)
                            {
                                if (obj2isLeftReach)
                                {
                                    nextState = 25;
                                    break;
                                }
                                else
                                {
                                    nextState = 90;
                                    break;
                                }
                            }

                            #endregion
                        }

                        #endregion

                        break;

                        #endregion

                    case 80:

                        #region State 80 : Getting close to Obj1

                        TBWriter.Info1("State 80 : Getting Close to Obj1");

                        if (obj1gotClose)
                        {
                            TBWriter.Warning1("I have already got Close to obj1" + obj1.Name + ", I will not try again");
                            obj1 = null;
                        }
                        else
                        {
                            double heightError = obj1.Position.Z - robot.TransRightArm2Robot(Vector3.Zero).Z + idealheightFromArmToObj;
                            TBWriter.Info4(" Adjusting Torso Elevation to" + heightError.ToString("0.00") + " , for reach obj1 " + obj1.Name);
                            cmdMan.TRS_relpos(heightError, torsoPan, 10000);
                            obj1gotClose = true;
                        }

                        TBWriter.Info2("Returning to State Zero for recalculate reachability");
                        nextState = 0;
                        break;

                        #endregion

                    case 90:

                        #region State 90 : Getting close to Obj2

                        TBWriter.Info1("State 90 : Getting Close to Obj2");

                        if (obj2gotClose)
                        {
                            TBWriter.Warning1("I have already got Close to obj2" + obj2.Name + ", I will not try again");
                            obj2 = null;
                        }
                        else
                        {
                            double heightError = obj1.Position.Z - robot.TransRightArm2Robot(Vector3.Zero).Z + idealheightFromArmToObj;
                            TBWriter.Info1("Adjusting Torso Elevation " + heightError.ToString("0.00") + " , for reach obj2 " + obj1.Name);
                            cmdMan.TRS_relpos(heightError, torsoPan, 10000);
                            obj2gotClose = true;
                        }

                        TBWriter.Info2("Returning to State Zero for recalculate reachability");
                        nextState = 0;
                        break;

                        #endregion

                    case 100:

                        #region State 100 : Finishing TakeObjectFromShelf

                        TBWriter.Info1("State 100 : Finishing TakeObjectFromShelf");
                        TBWriter.Info1(" ");
                        TBWriter.Info1("Objects taked : " + objectsTaked);
                        TBWriter.Info1("Object in leftHand : " + leftArmObject);
                        TBWriter.Info1("Object in rightHand : " + rightArmObject);

                        cmdMan.HEAD_lookat(0.0, 0.0, 3000);
                        cmdMan.TRS_abspos(torsoInitialElevation, 0.0, 7000);

                        cmd_TakeObjectRunning = false;

                        TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                        return true;

                        break;

                        #endregion

                    default:

                        #region State Default : Something unknown hapened

                        TBWriter.Info1("State Default : Something unknown hapened");

                        cmd_TakeObjectRunning = false;
                        TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                        return false;
                        break;

                        #endregion

                }
            }

            TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
            return false;
        }

        public bool Cmd_TakeHandOver(string obj1, string obj2)
        {
            TBWriter.Spaced("    ---  Starting TakeHandOver Command  ---");

            ActualizeTorsoPos();

            Vector3 leftPos = new Vector3(0.25, 0.30, 0);
            double leftRoll = MathUtil.PiOver2;
            double leftPitch = -0.3;
            double leftYaw = 0;
            double leftElbow = 0.5;

            Vector3 rightPos = new Vector3(0.25, 0.30, 0);
            double rightRoll = MathUtil.PiOver2;
            double rightPitch = 0.3;
            double rightYaw = 0;
            double rightElbow = -0.5;

            //if (!string.IsNullOrEmpty(obj1) && !string.IsNullOrEmpty(obj2))
            //{
            //    cmdMan.SPG_GEN_say("Please, put the " + obj1 + " in my left gripper and the " + obj2 + " in my right gripper");

            //    cmdMan.ARMS_ra_goto("navigation");
            //    cmdMan.ARMS_la_goto("navigation", 10000);
            //    cmdMan.WaitForResponse(JustinaCommands.ARMS_ra_goto, 2000);

            //    TBWriter.Info4("Sending LeftArm to deliver");
            //    TBWriter.Info4("Sending RightArm to deliver");

            //    cmdMan.ARMS_la_abspos(leftPos, leftRoll, leftPitch, leftYaw, leftElbow);
            //    cmdMan.ARMS_ra_abspos(rightPos, rightRoll, rightPitch, rightYaw, rightElbow, 10000);
            //    cmdMan.WaitForResponse(JustinaCommands.ARMS_la_abspos, 5000);


            //    TBWriter.Info4("Opening LeftArm gripper");
            //    TBWriter.Info4("Opening RightArm gripper");
            //    cmdMan.ARMS_la_opengrip(100.0);
            //    cmdMan.ARMS_ra_opengrip(100.0, 3000);
            //    cmdMan.WaitForResponse(JustinaCommands.ARMS_la_opengrip, 5000);

            //    TBWriter.Info3("Closing gripers in : 3seg");
            //    Thread.Sleep(3000);

            //    TBWriter.Info4("Closing LeftArm gripper");
            //    TBWriter.Info4("Closing RightArm gripper");
            //    cmdMan.ARMS_la_closegrip();
            //    cmdMan.ARMS_ra_closegrip(5000);
            //    cmdMan.WaitForResponse(JustinaCommands.ARMS_la_closegrip, 3000);


            //    robot.LeftArmIsEmpty = false;
            //    leftArmObject = obj1;
            //    robot.RightArmIsEmpty = false;
            //    rightArmObject = obj2;


            //    TBWriter.Info4("Sending LeftArm to navigatio");
            //    TBWriter.Info4("Sending RightArm to navigation");
            //    cmdMan.ARMS_la_goto("navigation");
            //    cmdMan.ARMS_ra_goto("navigation", 7000);
            //    cmdMan.WaitForResponse(JustinaCommands.ARMS_la_goto, 5000);


            //    cmdMan.SPG_GEN_say("Taked with LeftArm " + obj1 + " ; Taked with RightHand " + obj2);

            //    TBWriter.Spaced("    ---  Finished TakeHandOver Command  ---");
            //    return true;
            //}

           // if (robot.LeftArmIsEmpty)
           // {
           //     TBWriter.Info1("Taking with LeftArm");

           //     cmdMan.ARMS_la_goto("navigation", 10000);
           //     cmdMan.SPG_GEN_say("Please, put the " + obj1 + " on my left gripper");
           //     TBWriter.Info4("Sending LeftArm to deliver");
           //     cmdMan.ARMS_la_abspos(leftPos, leftRoll, leftPitch, leftYaw, leftElbow, 10000);

           //     TBWriter.Info4("Opening LeftArm gripper");
           //     cmdMan.ARMS_la_opengrip(3000);
           //     Thread.Sleep(3000);
           //     TBWriter.Info4("Closing LeftArm gripper");
           //     cmdMan.ARMS_la_closegrip(3000);

           //     robot.LeftArmIsEmpty = false;
           //     leftArmObject = obj1;


           //     TBWriter.Info4("Sending LeftArm to navigation");
           //     cmdMan.ARMS_la_goto("navigation");


           //     TBWriter.Info1("Taked with LeftArm" + obj1);
           //     TBWriter.Spaced("    ---  Finished TakeHandOver Command  ---");
           //     return true;
           //} 

            //if (robot.RightArmIsEmpty)
            //{
                TBWriter.Info1("Taking with RightArm");
                cmdMan.ARMS_ra_goto("navigation", 10000);
                cmdMan.SPG_GEN_say("Please, put the " + obj1 + " on my right gripper");
                TBWriter.Info4("Sending RightArm to deliver");
                cmdMan.ARMS_ra_abspos(rightPos, rightRoll, rightPitch, rightYaw, rightElbow, 10000);

                TBWriter.Info4("Opening RightArm gripper");
                cmdMan.ARMS_ra_opengrip(3000);
                Thread.Sleep(3000);
                TBWriter.Info4("Closing RightArm gripper");
                cmdMan.ARMS_ra_closegrip(3000);


                robot.RightArmIsEmpty = false;
                rightArmObject = obj1;


                TBWriter.Info4("Sending RightArm to navigation");
                cmdMan.ARMS_ra_goto("navigation");


                TBWriter.Info1("Taked with RightArm" + obj1);
                TBWriter.Spaced("    ---  Finished TakeHandOver Command  ---");
                return true;
            //}

            TBWriter.Warning1("No arms empty. leftArm = " + leftArmObject + " rightArm = " + rightArmObject);


            TBWriter.Info1("Taking with LeftArm");

            cmdMan.ARMS_la_goto("navigation", 10000);
            cmdMan.SPG_GEN_say("Please, put the " + obj1 + " on my left gripper");
            TBWriter.Info4("Sending LeftArm to deliver");
            cmdMan.ARMS_la_abspos(leftPos, leftRoll, leftPitch, leftYaw, leftElbow, 10000);

            TBWriter.Info4("Opening LeftArm gripper");
            cmdMan.ARMS_la_opengrip(3000);
            Thread.Sleep(3000);
            TBWriter.Info4("Closing LeftArm gripper");
            cmdMan.ARMS_la_closegrip(3000);

            robot.LeftArmIsEmpty = false;
            leftArmObject = obj1;

            TBWriter.Info4("Sending LeftArm to navigation");
            cmdMan.ARMS_la_goto("navigation");

            TBWriter.Info1("Taked with LeftArm" + obj1);
            TBWriter.Spaced("    ---  Finished TakeHandOver Command  ---");
            return true;









            //TBWriter.Info1("Taking with LeftArm");
            //cmdMan.SPG_GEN_say("Please, put the " + obj1 + " on my left gripper");
            //TBWriter.Info4("Sending LeftArm to deliver");
            //cmdMan.ARMS_la_goto("deliver", 10000);
            //TBWriter.Info4("Opening LeftArm gripper");
            //cmdMan.ARMS_la_opengrip(3000);
            //Thread.Sleep(3000);
            //TBWriter.Info4("Closing LeftArm gripper");
            //cmdMan.ARMS_la_closegrip(3000);

            //robot.LeftArmIsEmpty = false;
            //leftArmObject = obj1;

            //TBWriter.Info4("Sending LeftArm to navigation");
            //cmdMan.ARMS_la_goto("navigation");

            TBWriter.Info1("Taked with LeftArm" + obj1);
            TBWriter.Spaced("    ---  Finished TakeHandOver Command  ---");
            return true;
        }

        public bool Cmd_TakeHumanHands()
        {
            TBWriter.Spaced("    === Starting TakeHumanHands ");

            #region Validating

            if (!robot.modules[Module.Arms].IsConnected)
            {
                TBWriter.Error("Can't execute TakeHumanHands, Module [ Arms ] is not connected");
                return false;
            }
            if (!robot.modules[Module.ObjectFnd].IsConnected)
            {
                TBWriter.Error("Can't execute TakeHumanHands, Module [ ObjectFnd ] is not connected");
                return false;
            }

            #endregion


            Vector3 leftHandinKinect;
            Vector3 rightHandinKinect;
            Vector3 leftHand;
            Vector3 rightHand;

            ActualizeHeadPos();
            ActualizeTorsoPos();

            cmdMan.SPG_GEN_say("I will look for your Hands, stretch your arms please");

            if (!cmdMan.OBJ_FND_findHumanHands(robot.head.Tilt, out leftHandinKinect, out rightHandinKinect, 2000))
            {
                cmdMan.SPG_GEN_say("I will look for your Hands, stretch your arms please");

                if (!cmdMan.OBJ_FND_findHumanHands(robot.head.Tilt, out leftHandinKinect, out rightHandinKinect, 2000))
                {
                    TBWriter.Error("Can't execute TakeHumanHands, can't find Humands Hands");
                    TBWriter.Spaced("    === Finished TakeHumanHands");
                    return false;
                }
                else
                {
                    leftHand = robot.TransHeadKinect2Robot(leftHandinKinect);
                    rightHand = robot.TransRightArm2Robot(rightHandinKinect);
                }
            }
            else
            {
                leftHand = robot.TransHeadKinect2Robot(leftHandinKinect);
                rightHand = robot.TransRightArm2Robot(rightHandinKinect);
            }

            cmdMan.SPG_GEN_say("Haa, I found your hands");

            bool useVisionFeedBack = true;
            bool useCloseStart = true;

            double distIncrement = 0.05;
            double distanceToTake = 0.08;
            int maxSteps = 40;


            Vector3 findRightGripperPos;
            Vector3 findLeftGripperPos;

            Vector3 rightNextPos;
            Vector3 leftNextPos;

            Vector3 rightError;
            Vector3 leftError;

            double rightPitch;
            double leftPitch;

            double rightDist;
            double leftDist;


            ActualizeTorsoPos();
            ActualizeHeadPos();


            TBWriter.Info2("Sending Arms to standby");
            cmdMan.ARMS_ra_goto("standby");
            cmdMan.ARMS_la_goto("standby", 7000);
            cmdMan.WaitForResponse(JustinaCommands.ARMS_ra_goto, 2000);


            ActualizeRightArmPosition();
            ActualizeLeftArmPosition();


            rightPitch = 0.4;
            leftPitch = -0.4;


            rightNextPos = rightHand + new Vector3(0, -0.15, -0.20);
            leftNextPos = leftHand + new Vector3(0, 0.15, -0.20);


            cmdMan.SPG_GEN_say("Haa, I will take your hands, please dont move.");

            TBWriter.Info3("Sending to Initial Position");

            cmdMan.ARMS_ra_abspos(robot.TransRobot2RightArm(rightNextPos), MathUtil.Pi, rightPitch, 0, -rightPitch);
            cmdMan.ARMS_la_abspos(robot.TransRobot2LeftArm(leftNextPos), MathUtil.Pi, leftPitch, 0, -leftPitch, 7000);
            cmdMan.WaitForResponse(JustinaCommands.ARMS_ra_abspos, 2000);


            cmdMan.ARMS_ra_opengrip(100.0);
            cmdMan.ARMS_la_opengrip(100.0, 3000);
            cmdMan.WaitForResponse(JustinaCommands.ARMS_ra_opengrip, 2000);

            ActualizeRightArmPosition();
            ActualizeLeftArmPosition();

            rightError = rightHand - robot.RightArmPosition;
            leftError = leftHand - robot.LeftArmPosition;

            rightDist = rightError.Magnitude;
            leftDist = leftError.Magnitude;

            rightNextPos = robot.RightArmPosition + rightError.Unitary * distIncrement;
            leftNextPos = robot.LeftArmPosition + leftError.Unitary * distIncrement;

            rightPitch = Math.Atan2(rightError.X, rightError.Y);
            leftPitch = Math.Atan2(leftError.X, leftError.Y);

            int steps = 0;
            while ((rightDist > distanceToTake) && (leftDist > distanceToTake) && (steps < maxSteps))
            {
                if (rightDist > distanceToTake)
                    cmdMan.ARMS_ra_abspos(robot.TransRobot2RightArm(rightNextPos), MathUtil.Pi, rightPitch, 0, -rightPitch);
                if (leftDist > distanceToTake)
                    cmdMan.ARMS_la_abspos(robot.TransRobot2LeftArm(leftNextPos), MathUtil.Pi, leftPitch, 0, -leftPitch, 7000);

                if (rightDist > distanceToTake)
                    cmdMan.WaitForResponse(JustinaCommands.ARMS_ra_abspos, 2000);

                ActualizeRightArmPosition();
                ActualizeLeftArmPosition();

                rightError = rightHand - robot.RightArmPosition;
                leftError = leftHand - robot.LeftArmPosition;


                if (useVisionFeedBack)
                {
                    if (cmdMan.OBJ_FND_findRightArm(500, out findRightGripperPos))
                        rightError = rightHand - robot.TransHeadKinect2Robot(findRightGripperPos);

                    if (cmdMan.OBJ_FND_findLeftArm(500, out findLeftGripperPos))
                        leftError = leftHand - robot.TransHeadKinect2Robot(findLeftGripperPos);
                }

                rightNextPos = robot.RightArmPosition + rightError.Unitary * distIncrement;
                rightPitch = Math.Atan2(rightError.Y, rightError.X);
                rightDist = rightError.Magnitude;

                leftNextPos = robot.RightArmPosition + rightError.Unitary * distIncrement;
                leftPitch = Math.Atan2(leftError.Y, leftError.X);
                leftDist = leftError.Magnitude;

                steps++;
            }

            if (steps > maxSteps)
            {
                TBWriter.Warning1(" >>> Max Steeps Reached");
                cmdMan.ARMS_la_opengrip(70.0);
                cmdMan.ARMS_ra_opengrip(70.0, 5000);
                cmdMan.WaitForResponse(JustinaCommands.ARMS_la_abspos, 2000);
            }

            cmdMan.ARMS_ra_closegrip();
            cmdMan.ARMS_la_closegrip(4000);

            TBWriter.Spaced("    === Finished TakeHumanHands");
            return true;
        }

        public bool _OLD_Cmd_TakeObject(string firstObject, string secondObject, string armSelection, out string objectsTaked)
        {
            TBWriter.Info1("    ---  Starting TakeObject Command  ---");
            TBWriter.Spaced("    ---  Params received : " + firstObject + " " + secondObject + ", " + armSelection);

            objectsTaked = "";

            if (!robot.modules[Module.Arms].IsConnected)
            {
                TBWriter.Error("Can't execute TakeObject, [ Arms ] is not connected");
                TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                return false;
            }


            WorldObject obj1 = null;
            WorldObject obj2 = null;

            TBWriter.Info1("Searching objects in onbjectsList : ");

            if (robot.lastObjectsFounded.ContainsKey(firstObject))
            {
                obj1 = new WorldObject(robot.lastObjectsFounded[firstObject]);
                TBWriter.Info1("                  > Obj 1. founded in object List, preparing to take it" + obj1.Name);
            }
            else
            {
                TBWriter.Info1("                  > " + firstObject + " not founded in objectsList, can't take it");
            }
            if (robot.lastObjectsFounded.ContainsKey(secondObject))
            {
                obj2 = new WorldObject(robot.lastObjectsFounded[secondObject]);
                TBWriter.Info1("                  > Obj 2. founded in object List, preparing to take it" + obj1.Name);
            }
            else
            {
                TBWriter.Info1("                  > " + secondObject + " not founded in objectsList, can't take it");
            }


            double[] torsoDefaultPos = new double[2];
            torsoDefaultPos[0] = 0.7;
            torsoDefaultPos[1] = 0.0;

            ActualizeHeadPos();
            if (!ActualizeTorsoPos())
                robot.torso.ActualizeTorsoStatus(torsoDefaultPos);

            double torsoInitialElevation = robot.torso.Elevation;

            double torsoPan = 0;
            double torsoElevation = robot.torso.Elevation;
            double hdPan = 0;
            double hdTilt = -1.1;

            Vector3 errorVec;
            double errorAngle;

            Vector3 armRightRobot;
            Vector3 armLeftRobot;

            Vector3 obj1RightArmPos;
            Vector3 obj1LeftArmPos;
            double obj1distToRight = double.MaxValue;
            double obj1distToLeft = double.MaxValue;
            bool obj1isRightReach = false;
            bool obj1isLeftReach = false;
            bool obj1isBothReach = false;
            bool obj1isClosestToRight = false;

            Vector3 obj2RightArmPos;
            Vector3 obj2LeftArmPos;
            double obj2disToRight = double.MaxValue;
            double obj2disToLeft = double.MaxValue;
            bool obj2isRightReach = false;
            bool obj2isLeftReach = false;
            bool obj2isBothReach = false;
            bool obj2isClosestToRight = false;

            bool searchedObj1 = false;
            bool searchedObj2 = false;

            int nextState = 0;
            bool cmd_TakeObjectRunning = true;

            //////////// para el video 

			if (!CheckObjectReachableRightArm(obj1))
			{
				TBWriter.Warning1( "Object " + obj1.Name + "is not reachable for right Arm  !!!");
				return false;
			}

            return takeRightArm(obj1);


			///////////

            while (cmd_TakeObjectRunning)
            {
                switch (nextState)
                {
                    case 0:

                        #region State 0 : Verifying Parameters

                        TBWriter.Info1("State 0 : Verifying Parameters");

                        if ((obj1 == null) && (obj2 == null))
                        {
                            TBWriter.Info1(" Both objects are null");
                            TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                            return true;
                        }


                        #region Cheking Arms Reachability and closest

                        armRightRobot = robot.TransRightArm2Robot(Vector3.Zero);
                        armLeftRobot = robot.TransLeftArm2Robot(Vector3.Zero);

                        #region Checking Obj1

                        if (obj1 != null)
                        {
                            TBWriter.Info2("Checking Obj1 : " + obj1.Name);

                            /////////////

                            TBWriter.Info8("Checking Obj1 for RightArm");

                            errorVec = obj1.Position - armRightRobot;
                            errorAngle = Math.Atan2(errorVec.Y, errorVec.X);

                            obj1distToRight = errorVec.Magnitude;
                            obj1RightArmPos = robot.TransRobot2RightArm(obj1.Position);
                            obj1isRightReach = cmdMan.ARMS_ra_reachable(obj1RightArmPos.X, obj1RightArmPos.Y, obj1RightArmPos.Z, MathUtil.PiOver2, errorAngle, 0, -errorAngle, 5000);

                            if (obj1isRightReach)
                                TBWriter.Info8("Obj1 reachable for RightArm");
                            else
                                TBWriter.Info8("Obj1 is not reachable for RightArm");

                            /////////////

                            TBWriter.Info1("Checking Obj1 for LeftArm");

                            errorVec = obj1.Position - armLeftRobot;
                            errorAngle = Math.Atan2(errorVec.Y, errorVec.X);

                            obj1distToLeft = errorVec.Magnitude;
                            obj1LeftArmPos = robot.TransRobot2LeftArm(obj1.Position);
                            obj1isLeftReach = cmdMan.ARMS_la_reachable(obj1LeftArmPos.X, obj1LeftArmPos.Y, obj1LeftArmPos.Z, MathUtil.PiOver2, errorAngle, 0, -errorAngle, 5000);

                            if (obj1isLeftReach)
                                TBWriter.Info8("Obj1 reachable for LeftArm");
                            else
                                TBWriter.Info8("Obj1 is not reachable for LeftArm");

                            /////////////

                            if (obj1isRightReach && obj1isLeftReach)
                            {
                                TBWriter.Info8("Obj1 is both Reachable");
                                obj1isBothReach = true;
                            }

                            if (obj1distToRight <= obj1distToLeft)
                            {
                                TBWriter.Info8("Obj1 is closest to Right");
                                obj1isClosestToRight = true;
                            }
                        }

                        #endregion

                        #region Checking Obj2

                        if (obj2 != null)
                        {
                            TBWriter.Info2("Checking Obj2 : " + obj2.Name);

                            /////////////

                            TBWriter.Info8("Checking Obj2 for RightArm");

                            errorVec = obj2.Position - armRightRobot;
                            errorAngle = Math.Atan2(errorVec.Y, errorVec.X);

                            obj2disToRight = errorVec.Magnitude;
                            obj2RightArmPos = robot.TransRobot2RightArm(obj2.Position);
                            obj2isRightReach = cmdMan.ARMS_ra_reachable(obj2RightArmPos.X, obj2RightArmPos.Y, obj2RightArmPos.Z, MathUtil.PiOver2, errorAngle, 0, -errorAngle, 5000);

                            if (obj2isRightReach)
                                TBWriter.Info8("Obj2 reachable for RightArm");
                            else
                                TBWriter.Info8("Obj2 is not reachable for RightArm");

                            /////////////

                            TBWriter.Info8("Checking Obj2 for LeftArm");

                            errorVec = obj2.Position - armLeftRobot;
                            errorAngle = Math.Atan2(errorVec.Y, errorVec.X);

                            obj2disToLeft = errorVec.Magnitude;
                            obj2LeftArmPos = robot.TransRobot2LeftArm(obj2.Position);
                            obj2isLeftReach = cmdMan.ARMS_la_reachable(obj2LeftArmPos.X, obj2LeftArmPos.Y, obj2LeftArmPos.Z, MathUtil.PiOver2, errorAngle, 0, -errorAngle, 5000);

                            if (obj2isLeftReach)
                                TBWriter.Info8("Obj2 reachable for LeftArm");
                            else
                                TBWriter.Info8("Obj2 is not reachable for LeftArm");

                            /////////////

                            if (obj2isRightReach && obj2isLeftReach)
                            {
                                TBWriter.Info8("Obj2 is both Reachable");
                                obj2isBothReach = true;
                            }

                            if (obj2disToRight <= obj2disToLeft)
                            {
                                TBWriter.Info8("Obj2 is closest to Right");
                                obj2isClosestToRight = true;
                            }
                        }

                        #endregion

                        #endregion


                        if (armSelection == "right")
                            nextState = 10;
                        else if (armSelection == "left")
                            nextState = 20;
                        else if ((obj1 != null) && (obj2 != null))
                            nextState = 30;
                        else
                            nextState = 40;

                        break;

                        #endregion

                    case 10:

                        #region State 10: Using Right Arm to take obj1

                        TBWriter.Info1("State 10: Using Right Arm to take obj1");

                        if (obj1isRightReach)
                        {
                            if (takeRightArm(obj1))
                            {
                                TBWriter.Info1("Taking with right Arm : " + obj1.Name);

                                if (!verifyObjectsOnTable(obj1.Name))
                                {
                                    objectsTaked += obj1.Name;
                                    rightArmObject = obj1.Name;
                                    robot.RightArmIsEmpty = false;
                                }
                                else
                                    robot.RightArmIsEmpty = true;
                            }

                            TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                            return true;
                        }
                        else
                        {
                            if (!getClose(obj1.Position))
                            {
                                TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                return false;
                            }

                            cmdMan.TRS_abspos(torsoInitialElevation, torsoPan, 10000);
                            cmdMan.HEAD_lookat(hdPan, hdTilt, 5000);
                            //cmdMan.OBJ_FND_removeTable(30000);

                            WorldObject objfounded;
                            if (!findObject(obj1.Name, out objfounded))
                            {
                                TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                return false;
                            }

                            if (takeRightArm(objfounded))
                            {
                                if (!verifyObjectsOnTable(obj1.Name))
                                {
                                    objectsTaked += obj1.Name;
                                    rightArmObject = obj1.Name;
                                    robot.RightArmIsEmpty = false;
                                }
                                else
                                    robot.RightArmIsEmpty = true;
                            }

                        }

                        TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                        return true;

                        break;

                        #endregion

                    case 20:

                        #region State 20: Using Left Arm to take obj1

                        TBWriter.Info1("State 20: Using Left Arm to take obj1");

                        if (obj1isLeftReach)
                        {
                            if (takeLeftArm(obj1))
                            {
                                if (!verifyObjectsOnTable(obj1.Name))
                                {
                                    objectsTaked += obj1.Name;
                                    leftArmObject = obj1.Name;
                                    robot.LeftArmIsEmpty = false;
                                }
                                else
                                    robot.LeftArmIsEmpty = true;
                            }

                            TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                            return true;
                        }
                        else
                        {
                            if (!getClose(obj1.Position))
                            {
                                TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                return false;
                            }

                            cmdMan.TRS_abspos(torsoInitialElevation, torsoPan, 10000);
                            cmdMan.HEAD_lookat(hdPan, hdTilt, 5000);
                            //cmdMan.OBJ_FND_removeTable(30000);

                            WorldObject objfounded;
                            if (!findObject(obj1.Name, out objfounded))
                            {
                                TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                return false;
                            }

                            if (takeLeftArm(objfounded))
                            {
                                if (!verifyObjectsOnTable(obj1.Name))
                                {
                                    objectsTaked += obj1.Name;
                                    leftArmObject = obj1.Name;
                                    robot.LeftArmIsEmpty = false;
                                }
                                else
                                    robot.LeftArmIsEmpty = true;
                            }

                            TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                            return true;
                        }

                        TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                        return true;

                        break;

                        #endregion

                    case 30:

                        #region State 30 : Taking Both objects

                        TBWriter.Info1("State 30 : Taking Both objects");

                        #region Both objects are both reachable

                        if (obj1isBothReach && obj2isBothReach)
                        {
                            TBWriter.Info2("Both objects are both reachable");

                            if (obj1isClosestToRight && obj2isClosestToRight)
                            {
                                if (obj1distToLeft <= obj2disToLeft)
                                    takeBothArms(obj2, obj1, out objectsTaked);
                                else
                                    takeBothArms(obj1, obj1, out objectsTaked);
                            }

                            else if (!obj1isClosestToRight && !obj2isClosestToRight) // >> Then both are closest to Left
                            {
                                if (obj1distToRight <= obj2disToRight)
                                    takeBothArms(obj1, obj2, out objectsTaked);
                                else
                                    takeBothArms(obj2, obj1, out objectsTaked);
                            }

                            else if (obj1isClosestToRight)
                            {
                                takeBothArms(obj1, obj2, out objectsTaked);
                            }

                            else if (obj2isClosestToRight)
                            {
                                takeBothArms(obj2, obj1, out objectsTaked);
                            }

                            TBWriter.Spaced("    ---  Finishing FindObject Command  ---");
                            return true;
                        }

                        #endregion

                        #region Only Obj1 is Both Reachable

                        else if (obj1isBothReach)
                        {
                            TBWriter.Info2("Only Obj1 is Both Reachable");

                            if (obj2isRightReach)
                            {
                                takeBothArms(obj2, obj1, out objectsTaked);
                            }

                            else if (obj2isLeftReach)
                            {
                                takeBothArms(obj1, obj2, out objectsTaked);
                            }

                            else
                            {
                                if (obj1isClosestToRight)
                                {
                                    if (takeRightArm(obj1))
                                    {
                                        if (!verifyObjectsOnTable(obj1.Name))
                                        {
                                            objectsTaked += obj1.Name + " ";
                                            rightArmObject = obj1.Name;
                                            robot.RightArmIsEmpty = false;
                                        }
                                        else
                                            robot.RightArmIsEmpty = true;
                                    }

                                    if (!getClose(obj2.Position))
                                    {
                                        TBWriter.Spaced("    ---  Finishing FindObject Command  ---");
                                        return false;
                                    }

                                    cmdMan.TRS_abspos(torsoInitialElevation, torsoPan, 10000);
                                    cmdMan.HEAD_lookat(hdPan, hdTilt, 5000);
                                   // cmdMan.OBJ_FND_removeTable(30000);

                                    WorldObject objfounded;
                                    if (!findObject(obj2.Name, out objfounded))
                                    {
                                        TBWriter.Spaced("    ---  Finishing FindObject Command  ---");
                                        return false;
                                    }

                                    if (takeLeftArm(objfounded))
                                    {
                                        if (!verifyObjectsOnTable(obj2.Name))
                                        {
                                            objectsTaked += obj2.Name + " ";
                                            leftArmObject = obj2.Name;
                                            robot.LeftArmIsEmpty = false;
                                        }
                                        else
                                            robot.LeftArmIsEmpty = true;
                                    }
                                }
                                else
                                {
                                    if (takeLeftArm(obj1))
                                    {
                                        if (!verifyObjectsOnTable(obj1.Name))
                                        {
                                            objectsTaked += obj1.Name + " ";
                                            leftArmObject = obj1.Name;
                                            robot.LeftArmIsEmpty = false;
                                        }
                                        else
                                            robot.LeftArmIsEmpty = true;
                                    }

                                    if (!getClose(obj2.Position))
                                    {
                                        TBWriter.Spaced("    ---  Finishing FindObject Command  ---");
                                        return false;
                                    }

                                    cmdMan.TRS_abspos(torsoInitialElevation, torsoPan, 10000);
                                    cmdMan.HEAD_lookat(hdPan, hdTilt, 5000);
                                    //cmdMan.OBJ_FND_removeTable(30000);

                                    WorldObject objfounded;
                                    if (!findObject(obj2.Name, out objfounded))
                                    {
                                        TBWriter.Spaced("    ---  Finishing FindObject Command  ---");
                                        return false;
                                    }

                                    if (takeRightArm(objfounded))
                                    {
                                        if (!verifyObjectsOnTable(obj2.Name))
                                        {
                                            objectsTaked += obj2.Name + " ";
                                            rightArmObject = obj2.Name;
                                            robot.RightArmIsEmpty = false;
                                        }
                                        else
                                            robot.RightArmIsEmpty = true;
                                    }
                                }
                            }

                            TBWriter.Spaced("    ---  Finishing FindObject Command  ---");
                            return true;
                        }

                        #endregion

                        #region Only Obj2 is Both Reachable

                        else if (obj2isBothReach)
                        {
                            TBWriter.Info2("Only Obj2 is Both Reachable");

                            if (obj1isRightReach)
                            {
                                takeBothArms(obj1, obj2, out objectsTaked);
                            }

                            else if (obj1isLeftReach)
                            {
                                takeBothArms(obj2, obj1, out objectsTaked);
                            }

                            else
                            {
                                if (obj2isClosestToRight)
                                {
                                    if (takeRightArm(obj2))
                                    {
                                        if (!verifyObjectsOnTable(obj2.Name))
                                        {
                                            objectsTaked += obj2.Name + " ";
                                            rightArmObject = obj2.Name;
                                            robot.RightArmIsEmpty = false;
                                        }
                                        else
                                            robot.RightArmIsEmpty = true;
                                    }

                                    if (!getClose(obj1.Position))
                                    {
                                        TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                        return false;
                                    }

                                    cmdMan.TRS_abspos(torsoInitialElevation, torsoPan, 10000);
                                    cmdMan.HEAD_lookat(hdPan, hdTilt, 5000);
                                    //cmdMan.OBJ_FND_removeTable(30000);

                                    WorldObject objfounded;
                                    if (!findObject(obj1.Name, out objfounded))
                                    {
                                        TBWriter.Spaced("    ---  Finishing FindObject Command  ---");
                                        return false;
                                    }

                                    if (takeLeftArm(objfounded))
                                    {
                                        if (!verifyObjectsOnTable(obj1.Name))
                                        {
                                            objectsTaked += obj1.Name + " ";
                                            leftArmObject = obj1.Name;
                                            robot.LeftArmIsEmpty = false;
                                        }
                                        else
                                            robot.LeftArmIsEmpty = true;
                                    }
                                }
                                else
                                {
                                    if (takeLeftArm(obj2))
                                    {
                                        if (!verifyObjectsOnTable(obj2.Name))
                                        {
                                            objectsTaked += obj2.Name + " ";
                                            leftArmObject = obj2.Name;
                                            robot.LeftArmIsEmpty = false;
                                        }
                                        else
                                            robot.LeftArmIsEmpty = true;
                                    }

                                    if (!getClose(obj1.Position))
                                    {
                                        TBWriter.Spaced("    ---  Finishing FindObject Command  ---");
                                        return false;
                                    }

                                    cmdMan.TRS_abspos(torsoInitialElevation, torsoPan, 10000);
                                    cmdMan.HEAD_lookat(hdPan, hdTilt, 5000);
                                    //cmdMan.OBJ_FND_removeTable(30000);

                                    WorldObject objfounded;
                                    if (!findObject(obj1.Name, out objfounded))
                                    {
                                        TBWriter.Spaced("    ---  Finishing FindObject Command  ---");
                                        return false;
                                    }

                                    if (takeRightArm(objfounded))
                                    {
                                        if (!verifyObjectsOnTable(obj1.Name))
                                        {
                                            objectsTaked += obj1.Name + " ";
                                            rightArmObject = obj1.Name;
                                            robot.RightArmIsEmpty = false;
                                        }
                                        else
                                            robot.RightArmIsEmpty = true;
                                    }
                                }
                            }

                            TBWriter.Spaced("    ---  Finishing FindObject Command  ---");
                            return true;
                        }

                        #endregion

                        #region None is Both Reachable

                        else if ((!obj1isBothReach) && (!obj2isBothReach))
                        {
                            TBWriter.Info2("None is Both Reachable");

                            if (obj1isRightReach)
                            {
                                if (obj2isLeftReach)
                                {
                                    takeBothArms(obj1, obj2, out objectsTaked);
                                    TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                    return true;
                                }
                                else
                                {
                                    if (takeRightArm(obj1))
                                    {
                                        if (!verifyObjectsOnTable(obj1.Name))
                                        {
                                            objectsTaked += obj1.Name + " ";
                                            rightArmObject = obj1.Name;
                                            robot.RightArmIsEmpty = false;
                                        }
                                        else
                                            robot.RightArmIsEmpty = true;
                                    }

                                    if (!getClose(obj2.Position))
                                    {
                                        TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                        return false;
                                    }

                                    cmdMan.TRS_abspos(torsoInitialElevation, torsoPan, 10000);
                                    cmdMan.HEAD_lookat(hdPan, hdTilt, 5000);
                                    //cmdMan.OBJ_FND_removeTable(30000);

                                    WorldObject objfounded;
                                    if (!findObject(obj2.Name, out objfounded))
                                    {
                                        TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                        return false;
                                    }

                                    if (takeLeftArm(objfounded))
                                    {
                                        if (!verifyObjectsOnTable(obj2.Name))
                                        {
                                            objectsTaked += obj2.Name + " ";
                                            leftArmObject = obj2.Name;
                                            robot.LeftArmIsEmpty = false;
                                        }
                                        else
                                            robot.LeftArmIsEmpty = true;
                                    }
                                }
                            }

                            else if (obj2isRightReach)
                            {
                                if (obj1isLeftReach)
                                {
                                    takeBothArms(obj2, obj1, out objectsTaked);
                                    TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                    return true;
                                }
                                else
                                {
                                    if (takeRightArm(obj2))
                                    {
                                        if (!verifyObjectsOnTable(obj2.Name))
                                        {
                                            objectsTaked += obj2.Name + " ";
                                            rightArmObject = obj2.Name;
                                            robot.RightArmIsEmpty = false;
                                        }
                                        else
                                            robot.RightArmIsEmpty = true;
                                    }

                                    if (!getClose(obj1.Position))
                                    {
                                        TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                        return false;
                                    }

                                    cmdMan.TRS_abspos(torsoInitialElevation, torsoPan, 10000);
                                    cmdMan.HEAD_lookat(hdPan, hdTilt, 5000);
                                    //cmdMan.OBJ_FND_removeTable(30000);

                                    WorldObject objfounded;
                                    if (!findObject(obj1.Name, out objfounded))
                                    {
                                        TBWriter.Spaced("    ---  Finishing FindObject Command  ---");
                                        return false;
                                    }

                                    if (takeLeftArm(objfounded))
                                    {
                                        if (!verifyObjectsOnTable(obj1.Name))
                                        {
                                            objectsTaked += obj1.Name + " ";
                                            leftArmObject = obj1.Name;
                                            robot.LeftArmIsEmpty = false;
                                        }
                                        else
                                            robot.LeftArmIsEmpty = true;
                                    }
                                }
                            }

                            // If non is reachable
                            else
                            {
                                if (obj1.Position.Magnitude <= obj2.Position.Magnitude)
                                {
                                    if (!searchedObj1)
                                    {
                                        if (getClose(obj1.Position))
                                        {
                                            cmdMan.TRS_abspos(torsoInitialElevation, torsoPan, 10000);
                                            cmdMan.HEAD_lookat(hdPan, hdTilt, 5000);
                                            //cmdMan.OBJ_FND_removeTable(30000);

                                            WorldObject newObj1;

                                            findObject(obj1.Name, out newObj1);
                                            obj1 = newObj1;
                                        }

                                        searchedObj1 = true;
                                        nextState = 0;
                                        break;
                                    }
                                    else
                                    {
                                        TBWriter.Warning1(" Already Searched Obj1 ");
                                        TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                        return true;
                                    }


                                    if (!searchedObj2)
                                    {
                                        if (getClose(obj2.Position))
                                        {
                                            cmdMan.TRS_abspos(torsoInitialElevation, torsoPan, 10000);
                                            cmdMan.HEAD_lookat(hdPan, hdTilt, 5000);
                                //            cmdMan.OBJ_FND_removeTable(30000);

                                            WorldObject newObj2;

                                            findObject(obj2.Name, out newObj2);
                                            obj2 = newObj2;
                                        }

                                        searchedObj2 = true;
                                        nextState = 0;
                                        break;
                                    }
                                    else
                                    {
                                        TBWriter.Warning1(" Already Searched Obj2 ");
                                        TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                        return true;
                                    }

                                    TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                    return true;
                                }
                                else
                                {
                                    if (!searchedObj2)
                                    {
                                        if (getClose(obj2.Position))
                                        {
                                            cmdMan.TRS_abspos(torsoInitialElevation, torsoPan, 10000);
                                            cmdMan.HEAD_lookat(hdPan, hdTilt, 5000);
                                            //cmdMan.OBJ_FND_removeTable(30000);

                                            WorldObject newObj2;

                                            findObject(obj2.Name, out newObj2);
                                            obj2 = newObj2;
                                        }

                                        searchedObj2 = true;
                                        nextState = 0;
                                        break;
                                    }
                                    else
                                    {
                                        TBWriter.Warning1(" Already Searched Obj2 ");
                                        TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                        return true;
                                    }

                                    if (!searchedObj1)
                                    {
                                        if (getClose(obj1.Position))
                                        {
                                            cmdMan.TRS_abspos(torsoInitialElevation, torsoPan, 10000);
                                            cmdMan.HEAD_lookat(hdPan, hdTilt, 5000);
                                //            cmdMan.OBJ_FND_removeTable(30000);

                                            WorldObject newObj1;

                                            findObject(obj1.Name, out newObj1);
                                            obj1 = newObj1;
                                        }

                                        searchedObj1 = true;
                                        nextState = 0;
                                        break;
                                    }
                                    else
                                    {
                                        TBWriter.Warning1(" Already Searched Obj1 ");
                                        TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                        return true;
                                    }

                                    TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                    return true;
                                }
                            }

                            TBWriter.Spaced("    ---  Finishing FindObject Command  ---");
                            return true;
                        }

                        #endregion

                        break;

                        #endregion

                    case 40:

                        #region State 40 : Checking which arm to use

                        TBWriter.Info1("State 40 : Checking which arm to use");

                        #region Taking obj1

                        if (obj1 != null)
                        {
                            TBWriter.Info1("obj1 " + obj1.Name + " is not null");

                            #region If BOTH arms are empty Or NONE is empty

                            if ((robot.RightArmIsEmpty && robot.LeftArmIsEmpty) || (!robot.RightArmIsEmpty && !robot.LeftArmIsEmpty))
                            {
                                if (obj1isBothReach)
                                {
                                    if (obj1isClosestToRight)
                                    {
                                        if (takeRightArm(obj1))
                                        {
                                            if (!verifyObjectsOnTable(obj1.Name))
                                            {
                                                objectsTaked += obj1.Name + " ";
                                                rightArmObject = obj1.Name;
                                                robot.RightArmIsEmpty = false;
                                            }
                                            else
                                                robot.RightArmIsEmpty = true;
                                        }

                                        return true;
                                    }
                                    else
                                    {
                                        if (takeLeftArm(obj1))
                                        {
                                            if (!verifyObjectsOnTable(obj1.Name))
                                            {
                                                objectsTaked += obj1.Name + " ";
                                                leftArmObject = obj1.Name;
                                                robot.LeftArmIsEmpty = false;
                                            }
                                            else
                                                robot.LeftArmIsEmpty = true;
                                        }

                                        return true;
                                    }
                                }
                                else if (obj1isRightReach)
                                {
                                    if (takeRightArm(obj1))
                                    {
                                        if (!verifyObjectsOnTable(obj1.Name))
                                        {
                                            objectsTaked += obj1.Name + " ";
                                            rightArmObject = obj1.Name;
                                            robot.RightArmIsEmpty = false;
                                        }
                                        else
                                            robot.RightArmIsEmpty = true;
                                    }

                                    return true;

                                }
                                else if (obj1isLeftReach)
                                {
                                    if (takeLeftArm(obj1))
                                    {
                                        if (!verifyObjectsOnTable(obj1.Name))
                                        {
                                            objectsTaked += obj1.Name + " ";
                                            leftArmObject = obj1.Name;
                                            robot.LeftArmIsEmpty = false;
                                        }
                                        else
                                            robot.LeftArmIsEmpty = true;
                                    }

                                    return true;
                                }
                                else
                                {
                                    if (!searchedObj1)
                                    {
                                        if (getClose(obj1.Position))
                                        {
                                            cmdMan.TRS_abspos(torsoInitialElevation, torsoPan, 10000);
                                            cmdMan.HEAD_lookat(hdPan, hdTilt, 5000);
                                            //cmdMan.OBJ_FND_removeTable(30000);

                                            WorldObject newObj1;

                                            findObject(obj1.Name, out newObj1);
                                            obj1 = newObj1;
                                        }

                                        searchedObj1 = true;
                                        nextState = 0;
                                        break;
                                    }
                                    else
                                    {
                                        TBWriter.Warning1(" Already Searched Obj1 ");
                                        TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                        return true;
                                    }
                                }
                            }

                            #endregion

                            #region If Right Arm is Empty

                            else if (robot.RightArmIsEmpty)
                            {
                                if (obj1isRightReach)
                                {
                                    if (takeRightArm(obj1))
                                    {
                                        if (!verifyObjectsOnTable(obj1.Name))
                                        {
                                            objectsTaked += obj1.Name + " ";
                                            rightArmObject = obj1.Name;
                                            robot.RightArmIsEmpty = false;
                                        }
                                        else
                                            robot.RightArmIsEmpty = true;
                                    }
                                }
                                else
                                {
                                    if (!getClose(obj1.Position))
                                    {
                                        TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                        return false;
                                    }

                                    cmdMan.TRS_abspos(torsoInitialElevation, torsoPan, 10000);
                                    cmdMan.HEAD_lookat(hdPan, hdTilt, 5000);
                                    //cmdMan.OBJ_FND_removeTable(30000);

                                    WorldObject objfounded;
                                    if (!findObject(obj1.Name, out objfounded))
                                    {
                                        TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                        return false;
                                    }

                                    if (takeRightArm(objfounded))
                                    {
                                        if (!verifyObjectsOnTable(obj1.Name))
                                        {
                                            objectsTaked += obj1.Name + " ";
                                            rightArmObject = obj1.Name;
                                            robot.RightArmIsEmpty = false;
                                        }
                                        else
                                            robot.RightArmIsEmpty = true;
                                    }
                                }

                                TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                return true;
                            }

                            #endregion

                            #region If Left arm is empty

                            else if (robot.LeftArmIsEmpty)
                            {


                                if (obj1isLeftReach)
                                {
                                    if (takeLeftArm(obj1))
                                    {
                                        if (!verifyObjectsOnTable(obj1.Name))
                                        {
                                            objectsTaked += obj1.Name + " ";
                                            leftArmObject = obj1.Name;
                                            robot.LeftArmIsEmpty = false;
                                        }
                                        else
                                            robot.LeftArmIsEmpty = true;
                                    }
                                }
                                else
                                {
                                    if (!getClose(obj1.Position))
                                    {
                                        TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                        return false;
                                    }

                                    cmdMan.TRS_abspos(torsoInitialElevation, torsoPan, 10000);
                                    cmdMan.HEAD_lookat(hdPan, hdTilt, 5000);
                                    //cmdMan.OBJ_FND_removeTable(30000);

                                    WorldObject objfounded;
                                    if (!findObject(obj1.Name, out objfounded))
                                    {
                                        TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                        return false;
                                    }

                                    if (takeLeftArm(objfounded))
                                    {
                                        if (!verifyObjectsOnTable(obj1.Name))
                                        {
                                            objectsTaked += obj1.Name + " ";
                                            leftArmObject = obj1.Name;
                                            robot.LeftArmIsEmpty = false;
                                        }
                                        else
                                            robot.LeftArmIsEmpty = true;
                                    }
                                }

                                TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                return true;
                            }
                            #endregion
                        }

                        #endregion

                        #region taking obj2

                        if (obj2 != null)
                        {

                            #region If BOTH Arms are empty or NONE is empty

                            if ((robot.RightArmIsEmpty && robot.LeftArmIsEmpty) || (!robot.RightArmIsEmpty && !robot.LeftArmIsEmpty))
                            {
                                if (obj2isBothReach)
                                {
                                    if (obj2isClosestToRight)
                                    {
                                        if (takeRightArm(obj2))
                                        {
                                            if (!verifyObjectsOnTable(obj2.Name))
                                            {
                                                objectsTaked += obj2.Name + " ";
                                                rightArmObject = obj2.Name;
                                                robot.RightArmIsEmpty = false;
                                            }
                                            else
                                                robot.RightArmIsEmpty = true;
                                        }

                                        return true;
                                    }
                                    else
                                    {
                                        if (takeLeftArm(obj2))
                                        {
                                            if (!verifyObjectsOnTable(obj2.Name))
                                            {
                                                objectsTaked += obj2.Name + " ";
                                                leftArmObject = obj2.Name;
                                                robot.LeftArmIsEmpty = false;
                                            }
                                            else
                                                robot.LeftArmIsEmpty = true;
                                        }

                                        return true;
                                    }
                                }
                                else if (obj2isRightReach)
                                {
                                    if (takeRightArm(obj2))
                                    {
                                        if (!verifyObjectsOnTable(obj2.Name))
                                        {
                                            objectsTaked += obj2.Name + " ";
                                            rightArmObject = obj2.Name;
                                            robot.RightArmIsEmpty = false;
                                        }
                                        else
                                            robot.RightArmIsEmpty = true;
                                    }

                                    return true;

                                }
                                else if (obj2isLeftReach)
                                {
                                    if (takeLeftArm(obj2))
                                    {
                                        if (!verifyObjectsOnTable(obj2.Name))
                                        {
                                            objectsTaked += obj2.Name + " ";
                                            leftArmObject = obj2.Name;
                                            robot.LeftArmIsEmpty = false;
                                        }
                                        else
                                            robot.LeftArmIsEmpty = true;
                                    }

                                    return true;
                                }
                                else
                                {
                                    if (!searchedObj2)
                                    {
                                        if (getClose(obj2.Position))
                                        {
                                            cmdMan.TRS_abspos(torsoInitialElevation, torsoPan, 10000);
                                            cmdMan.HEAD_lookat(hdPan, hdTilt, 5000);
                                            //cmdMan.OBJ_FND_removeTable(30000);

                                            WorldObject newObj2;

                                            findObject(obj2.Name, out newObj2);
                                            obj2 = newObj2;
                                        }

                                        searchedObj2 = true;
                                        nextState = 0;
                                        break;
                                    }
                                    else
                                    {
                                        TBWriter.Warning1(" Already Searched Obj2 ");
                                        TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                        return true;
                                    }
                                }
                            }

                            #endregion

                            #region If right arm is empty

                            else if (robot.RightArmIsEmpty)
                            {
                                if (obj2isRightReach)
                                {
                                    if (takeRightArm(obj2))
                                    {
                                        if (!verifyObjectsOnTable(obj2.Name))
                                        {
                                            objectsTaked += obj2.Name + " ";
                                            rightArmObject = obj2.Name;
                                            robot.RightArmIsEmpty = false;
                                        }
                                        else
                                            robot.RightArmIsEmpty = true;
                                    }
                                }
                                else
                                {
                                    if (!getClose(obj2.Position))
                                    {
                                        TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                        return false;
                                    }

                                    cmdMan.TRS_abspos(torsoInitialElevation, torsoPan, 10000);
                                    cmdMan.HEAD_lookat(hdPan, hdTilt, 5000);
                                  //  cmdMan.OBJ_FND_removeTable(30000);

                                    WorldObject objfounded;

                                    if (!findObject(obj2.Name, out objfounded))
                                    {
                                        TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                        return false;
                                    }

                                    if (takeRightArm(objfounded))
                                    {
                                        if (!verifyObjectsOnTable(obj2.Name))
                                        {
                                            objectsTaked += obj2.Name + " ";
                                            rightArmObject = obj2.Name;
                                            robot.RightArmIsEmpty = false;
                                        }
                                        else
                                            robot.RightArmIsEmpty = true;
                                    }
                                }

                                TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                return true;
                            }

                            #endregion

                            #region If left arm is empty

                            else if (robot.LeftArmIsEmpty)
                            {
                                if (obj2isLeftReach)
                                {
                                    if (takeLeftArm(obj2))
                                    {
                                        if (!verifyObjectsOnTable(obj2.Name))
                                        {
                                            objectsTaked += obj1.Name + " ";
                                            leftArmObject = obj2.Name;
                                            robot.LeftArmIsEmpty = false;
                                        }
                                        else
                                            robot.LeftArmIsEmpty = true;
                                    }
                                }
                                else
                                {
                                    if (!getClose(obj2.Position))
                                    {
                                        TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                        return false;
                                    }

                                    cmdMan.TRS_abspos(torsoInitialElevation, torsoPan, 10000);
                                    cmdMan.HEAD_lookat(hdPan, hdTilt, 5000);
                                    //                                    cmdMan.OBJ_FND_removeTable(30000);

                                    WorldObject objfounded;
                                    if (!findObject(obj2.Name, out objfounded))
                                    {
                                        TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                        return false;
                                    }

                                    if (takeLeftArm(objfounded))
                                    {
                                        if (!verifyObjectsOnTable(obj2.Name))
                                        {
                                            objectsTaked += obj2.Name + " ";
                                            leftArmObject = obj2.Name;
                                            robot.LeftArmIsEmpty = false;
                                        }
                                        else
                                            robot.LeftArmIsEmpty = true;
                                    }
                                }

                                TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                return true;
                            }

                            #endregion
                        }

                        #endregion

                        break;

                        #endregion

                    default:

                        #region State Default : Something unknown hapened

                        TBWriter.Info1("State Default : Something unknown hapened");

                        cmd_TakeObjectRunning = false;
                        TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                        return false;
                        break;

                        #endregion

                }
            }

            TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
            return false;
        }

        public bool Cmd_TakeObject(string firstObject, string secondObject, string armSelection, out string objectsTaked)
        {
            TBWriter.Info1("    ---  Starting TakeObject Command  ---");
            TBWriter.Write(" Params = Obj1=" + firstObject);
            TBWriter.Write("          Obj2=" + secondObject);
            TBWriter.Write("          Arm=" + armSelection);


            objectsTaked = "";
            string empty = "empty";
            string emptyAndEmpty = "empty empty";

            if (!robot.modules[Module.Arms].IsConnected)
            {
                TBWriter.Error("Can't execute TakeObject, [ Arms ] is not connected");
                TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                return false;
            }

            WorldObject obj1 = null;
            WorldObject obj2 = null;

            TBWriter.Info1("Searching objects in objectsList : ");

            if (robot.lastObjectsFounded.ContainsKey(firstObject))
            {
                obj1 = new WorldObject(robot.lastObjectsFounded[firstObject]);
                TBWriter.Info1("            > Obj1(" + obj1.Name + ") is in objList, preparing to take it.");
            }
            else
            {
                TBWriter.Info1("            > Obj1(" + obj1.Name + ") not found in objList, can't take it");
            }
            if (robot.lastObjectsFounded.ContainsKey(secondObject))
            {
                obj2 = new WorldObject(robot.lastObjectsFounded[secondObject]);
                TBWriter.Info1("            > Obj2(" + obj1.Name + ") in objList, preparing to take it.");
            }
            else
            {
                TBWriter.Info1("            > Obj2(" + secondObject + ") not found in objList, can't take it");
            }


            double[] torsoDefaultPos = new double[2];
            torsoDefaultPos[0] = 0.7;
            torsoDefaultPos[1] = 0.0;

            ActualizeHeadPos();
            if (!ActualizeTorsoPos())
                robot.torso.ActualizeTorsoStatus(torsoDefaultPos);

            double torsoInitialElevation = robot.torso.Elevation;

            double torsoPan = 0;
            double torsoElevation = robot.torso.Elevation;
            double hdPan = 0;
            double hdTilt = -1.1;

            Vector3 errorVec;
            double errorAngle;

            Vector3 armRightRobot;
            Vector3 armLeftRobot;

            Vector3 obj1RightArmPos;
            Vector3 obj1LeftArmPos;
            double obj1distToRight = double.MaxValue;
            double obj1distToLeft = double.MaxValue;
            bool obj1isRightReach = false;
            bool obj1isLeftReach = false;
            bool obj1isBothReach = false;
            bool obj1isClosestToRight = false;

            Vector3 obj2RightArmPos;
            Vector3 obj2LeftArmPos;
            double obj2disToRight = double.MaxValue;
            double obj2disToLeft = double.MaxValue;
            bool obj2isRightReach = false;
            bool obj2isLeftReach = false;
            bool obj2isBothReach = false;
            bool obj2isClosestToRight = false;

            bool searchedObj1 = false;
            bool searchedObj2 = false;

            int nextState = 0;
            bool cmd_TakeObjectRunning = true;

            //////////// para el video 
            //if (!CheckObjectReachableRightArm(obj1))
            //{
            //    TBWriter.Warning1( "Object " + obj1.Name + "is not reachable for right Arm  !!!");
            //    return false;
            //}
            //return takeRightArm(obj1);
            ///////////

            while (cmd_TakeObjectRunning)
            {
                switch (nextState)
                {
                    case 0:

                        #region State 0 : Verifying Parameters

                        TBWriter.Info1("State 0 : Verifying Parameters");

                        if ((obj1 == null) && (obj2 == null))
                        {
                            TBWriter.Info1(" Both objects are null: cant take neither");
                            TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                            return true;
                        }


                        #region Cheking Arms Reachability and closest

                        armRightRobot = robot.TransRightArm2Robot(Vector3.Zero);
                        armLeftRobot = robot.TransLeftArm2Robot(Vector3.Zero);

                        #region Checking Obj1

                        if (obj1 != null)
                        {
//                            TBWriter.Info2("Checking Obj1 : " + obj1.Name);

                            /////////////

//                            TBWriter.Info8("Checking Obj1 for RightArm");

                            errorVec = obj1.Position - armRightRobot;
                            errorAngle = Math.Atan2(errorVec.Y, errorVec.X);

                            obj1distToRight = errorVec.Magnitude;
                            obj1RightArmPos = robot.TransRobot2RightArm(obj1.Position);
                            obj1isRightReach = cmdMan.ARMS_ra_reachable(obj1RightArmPos.X, obj1RightArmPos.Y, obj1RightArmPos.Z, MathUtil.PiOver2, errorAngle, 0, -errorAngle, 5000);

                            if (obj1isRightReach)
                                TBWriter.Info3("Obj1 reachable for RightArm");
                            else
                                TBWriter.Info3("Obj1 is not reachable for RightArm");

                            /////////////

//                            TBWriter.Info1("Checking Obj1 for LeftArm");

                            errorVec = obj1.Position - armLeftRobot;
                            errorAngle = Math.Atan2(errorVec.Y, errorVec.X);

                            obj1distToLeft = errorVec.Magnitude;
                            obj1LeftArmPos = robot.TransRobot2LeftArm(obj1.Position);
                            obj1isLeftReach = cmdMan.ARMS_la_reachable(obj1LeftArmPos.X, obj1LeftArmPos.Y, obj1LeftArmPos.Z, MathUtil.PiOver2, errorAngle, 0, -errorAngle, 5000);

                            if (obj1isLeftReach)
                                TBWriter.Info3("Obj1 reachable for LeftArm");
                            else
                                TBWriter.Info3("Obj1 is not reachable for LeftArm");

                            /////////////

                            if (obj1isRightReach && obj1isLeftReach)
                            {
                                TBWriter.Info3("Obj1 is both Reachable");
                                obj1isBothReach = true;
                            }

                            if (obj1distToRight <= obj1distToLeft)
                            {
                                TBWriter.Info3("Obj1 is closest to RightArm");
                                obj1isClosestToRight = true;
                            }
                            else
                            {
                                TBWriter.Info3("Obj1 is closest to LeftArm");
                                obj1isClosestToRight = false;
                            }
                        }

                        #endregion

                        TBWriter.Write("");

                        #region Checking Obj2

                        if (obj2 != null)
                        {
                           // TBWriter.Info2("Checking Obj2 : " + obj2.Name);

                            /////////////

                          //  TBWriter.Info8("Checking Obj2 for RightArm");

                            errorVec = obj2.Position - armRightRobot;
                            errorAngle = Math.Atan2(errorVec.Y, errorVec.X);

                            obj2disToRight = errorVec.Magnitude;
                            obj2RightArmPos = robot.TransRobot2RightArm(obj2.Position);
                            obj2isRightReach = cmdMan.ARMS_ra_reachable(obj2RightArmPos.X, obj2RightArmPos.Y, obj2RightArmPos.Z, MathUtil.PiOver2, errorAngle, 0, -errorAngle, 5000);

                            if (obj2isRightReach)
                                TBWriter.Info3("Obj2 reachable for RightArm");
                            else
                                TBWriter.Info3("Obj2 is not reachable for RightArm");

                            /////////////

                          //  TBWriter.Info8("Checking Obj2 for LeftArm");

                            errorVec = obj2.Position - armLeftRobot;
                            errorAngle = Math.Atan2(errorVec.Y, errorVec.X);

                            obj2disToLeft = errorVec.Magnitude;
                            obj2LeftArmPos = robot.TransRobot2LeftArm(obj2.Position);
                            obj2isLeftReach = cmdMan.ARMS_la_reachable(obj2LeftArmPos.X, obj2LeftArmPos.Y, obj2LeftArmPos.Z, MathUtil.PiOver2, errorAngle, 0, -errorAngle, 5000);

                            if (obj2isLeftReach)
                                TBWriter.Info3("Obj2 reachable for LeftArm");
                            else
                                TBWriter.Info3("Obj2 is not reachable for LeftArm");

                            /////////////

                            if (obj2isRightReach && obj2isLeftReach)
                            {
                                TBWriter.Info3("Obj2 is both Reachable");
                                obj2isBothReach = true;
                            }

                            if (obj2disToRight <= obj2disToLeft)
                            {
                                TBWriter.Info3("Obj2 is closest to Right");
                                obj2isClosestToRight = true;
                            }
                            else
                            {
                                TBWriter.Info3("Obj2 is closest to Left");
                                obj2isClosestToRight = false;
                            }
                        }

                        #endregion

                        #endregion


						if (armSelection == "right")
						{
							TBWriter.Info1("Next State = 10");
							nextState = 10;
						}
						else if (armSelection == "left")
						{
							TBWriter.Info1("Next State = 20");
							nextState = 20;
						}
						else if ((obj1 != null) && (obj2 != null)) // take with both hands
						{
							TBWriter.Info1("Next State = 30");
							nextState = 30;
						}
						else
						{
							TBWriter.Info1("Next State = 40");
							nextState = 40;
						}
                        break;

                        #endregion

                    case 10:

                        #region State 10: Using Right Arm to take obj1

                        TBWriter.Info1("State 10: (Obj1 with right. (Obj1=" + obj1.Name + " , ArmSelection=Right)");

                        if (obj1isRightReach)
                        {
                            TBWriter.Info1(obj1.Name + " is RIGHT Arm reachable, trying to take it");
                            takeRightArm(obj1, out objectsTaked);
                        }
                        else
                        {   
                            TBWriter.Warning1("Cant take " + obj1.Name + " , is NOT RIGHT Arm reachable");
                            TBWriter.Info3("Taking NOTHING");
                            objectsTaked = emptyAndEmpty;
                        }

                        TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                        return true;

                        #endregion

                    case 20:

                        #region State 20: Using Left Arm to take obj1

                         TBWriter.Info1("State 20: Obj1 with Left. (Obj1="+ obj1.Name +" , ArmSelection=Left)");

                        if (obj1isLeftReach)
                        {
                            TBWriter.Info1(obj1.Name + " is LEFT arm reachable, trying to take it");
                            takeLeftArm(obj1, out objectsTaked);
                        }
                        else
                        {
                            TBWriter.Warning1("Cant take " + obj1.Name + " , is NOT LEFT Arm reachable");
                            TBWriter.Info3("Taking NOTHING");
                            objectsTaked = emptyAndEmpty;
                        }

                        TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                        return true;

                        #endregion

                    case 30:

                        #region State 30 : Taking Both objects

                        TBWriter.Info1("State 30 :  Taking 2 objects. (Obj1=" +obj1.Name+" , Obj2=" + obj2.Name +") AutoDecition");

                        #region Both objects are both reachable

                        if (obj1isBothReach && obj2isBothReach)
                        {
                            TBWriter.Info2("Both objects are both reachable");

                            if (obj1isClosestToRight && obj2isClosestToRight)
                            {
                                TBWriter.Info3("Both objects are closest to RIGHT");
                                if (obj1distToLeft <= obj2disToLeft)
                                {
                                    TBWriter.Info4("But Obj1 is closest to to LEFT than obj2");
                                    takeBothArms(obj2, obj1, out objectsTaked);
                                }
                                else
                                {
                                    TBWriter.Info4("But Obj2 is closest to to LEFT than obj1");
                                    takeBothArms(obj1, obj2, out objectsTaked);
                                }
                            }
                            else if (!obj1isClosestToRight && !obj2isClosestToRight) // >> Then both are closest to Left
                            {
                                TBWriter.Info3("Both objects are closest to LEFT");
                                if (obj1distToRight <= obj2disToRight)
                                {
                                    TBWriter.Info4("But Obj1 is closest to to RIGHT than obj2");
                                    takeBothArms(obj1, obj2, out objectsTaked);
                                }
                                else
                                {
                                    TBWriter.Info4("But Obj2 is closest to to RIGHT than obj1");
                                    takeBothArms(obj2, obj1, out objectsTaked);
                                }
                            }

                            else if (obj1isClosestToRight)
                            {
                                TBWriter.Info3("Obj1 is closest to right than obj1");
                                takeBothArms(obj1, obj2, out objectsTaked);
                            }

                            else if (obj2isClosestToRight)
                            {
                                TBWriter.Info3("Obj2 is closest to right than obj1");
                                takeBothArms(obj2, obj1, out objectsTaked);
                            }

                            TBWriter.Spaced("    ---  Finishing FindObject Command  ---");
                            return true;
                        }

                        #endregion

                        #region Only Obj1 is Both Reachable

                        else if (obj1isBothReach)
                        {
                            TBWriter.Info2("Only Obj1 is Both Reachable");

                            if (obj2isRightReach)
                            {
                                TBWriter.Info3("Obj2 is RIGHT reachable");
                                takeBothArms(obj2, obj1, out objectsTaked);
                            }
                            else if (obj2isLeftReach)
                            {
                                TBWriter.Info3("Obj2 is LEFT reachable");
                                takeBothArms(obj1, obj2, out objectsTaked);
                            }
                            else
                            {
                                TBWriter.Info3("Obj2 is NOT reachable");
                                if (obj1isClosestToRight)
                                {
                                    TBWriter.Info4("Obj1 is closest to to RIGHT");
                                    TBWriter.Info4("Taking ONLY Obj1 with RIGHT");
                                    takeRightArm(obj1, out objectsTaked);
                                }
                                else
                                {
                                    TBWriter.Info4("Obj1 is closest to to LEFT");
                                    TBWriter.Info4("Taking ONLY Obj1 with LEFT");
                                    takeLeftArm(obj1, out objectsTaked);
                                }
                            }

                            TBWriter.Spaced("    ---  Finishing FindObject Command  ---");
                            return true;
                        }

                        #endregion

                        #region Only Obj2 is Both Reachable

                        else if (obj2isBothReach)
                        {
                            TBWriter.Info2("Only Obj2 is Both Reachable");

                            if (obj1isRightReach)
                            {
                                TBWriter.Info3("Obj1 is RIGHT reachable");
                                takeBothArms(obj1, obj2, out objectsTaked);
                            }
                            else if (obj1isLeftReach)
                            {
                                TBWriter.Info3("Obj1 is LEFT reachable");
                                takeBothArms(obj2, obj1, out objectsTaked);
                            }
                            else
                            {
                                TBWriter.Info3("Obj1 is NOT reachable");
                                if (obj2isClosestToRight)
                                {
                                    TBWriter.Info4("Obj2 is closest to to RIGHT");
                                    TBWriter.Info4("Taking ONLY Obj2 with RIGHT");
                                    takeRightArm(obj2, out objectsTaked);
                                }
                                else
                                {
                                    TBWriter.Info4("Obj2 is closest to to LEFT");
                                    TBWriter.Info4("Taking ONLY Obj2 with LEFT");
                                    takeLeftArm(obj2, out objectsTaked);
                                }
                            }

                            TBWriter.Spaced("    ---  Finishing FindObject Command  ---");
                            return true;
                        }

                        #endregion

                        #region None is Both Reachable

                        else if ((!obj1isBothReach) && (!obj2isBothReach))
                        {
                            TBWriter.Info2("None is Both Reachable");

                            if (obj1isRightReach)
                            {
                                TBWriter.Info3("Obj1 is RIGHT reachable");
                                if (obj2isLeftReach)
                                {
                                    TBWriter.Info4("Obj2 is LEFT reachable");
                                    takeBothArms(obj1, obj2, out objectsTaked);
                                }
                                else
                                {
                                    TBWriter.Info4("Obj2 is NOT reachable");
                                    TBWriter.Info4("Taking ONLY Obj1 with RIGHT");
                                    takeRightArm(obj1, out objectsTaked);
                                }
                                TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                return true;
                            }

                            if (obj1isLeftReach)
                            {
                                TBWriter.Info3("Obj1 is LEFT reachable");
                                if (obj2isRightReach)
                                {
                                    TBWriter.Info4("Obj2 is RIGHT reachable");
                                    takeBothArms(obj2, obj1, out objectsTaked);
                                }
                                else
                                {
                                    TBWriter.Info4("Obj2 is NOT reachable");
                                    TBWriter.Info4("Taking ONLY Obj1 with LEFT");
                                    takeLeftArm(obj1, out objectsTaked);
                                }
                                TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                return true;
                            }

                            if (obj2isRightReach)
                            {
                                TBWriter.Info3("Obj2 is RIGHT reachable");
                                if (obj1isLeftReach)
                                {
                                    TBWriter.Info4("Obj1 is LEFT reachable");
                                    takeBothArms(obj2, obj1, out objectsTaked);
                                }
                                else
                                {
                                    TBWriter.Info4("Obj1 is NOT reachable");
                                    TBWriter.Info4("Taking ONLY Obj2 with RIGHT");
                                    takeRightArm(obj2, out objectsTaked);
                                }
                                TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                return true;
                            }

                            if (obj2isLeftReach)
                            {
                                TBWriter.Info3("Obj2 is LEFT reachable");
                                if (obj1isRightReach)
                                {
                                    TBWriter.Info4("Obj1 is RIGHT reachable");
                                    takeBothArms(obj1, obj2, out objectsTaked);
                                }
                                else
                                {
                                    TBWriter.Info4("Obj1 is NOT reachable");
                                    TBWriter.Info4("Taking ONLY Obj2 with LEFT");
                                    takeLeftArm(obj2, out objectsTaked);
                                }
                                TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                                return true;
                            }


                            objectsTaked = empty + " " + empty;
                            TBWriter.Spaced("    ---  Finishing FindObject Command  ---");
                            return true;
                        }

                        #endregion

                        break;

                        #endregion

                    case 40:

                        #region State 40 : Checking which arm to use

						TBWriter.Info1("State 40 : Taking Obj1 OR Obj2. ");

                        #region Taking obj1

                        if (obj1 != null)
                        {
                            TBWriter.Info2("ONLY Obj1="+obj1.Name+" is not null");
                            
                            if (obj1isBothReach)
                            {
                                TBWriter.Info3("Obj1 is BOTH reachable");
                                if (obj1isClosestToRight)
                                {
                                    TBWriter.Info4("But Obj1 is closest to RIGHT");
                                    TBWriter.Info4("Taking ONLY Obj1 with RIGHT");
                                    takeRightArm(obj1, out objectsTaked);
                                }
                                else
                                {
                                    TBWriter.Info4("But Obj1 is closest to LEFT");
                                    TBWriter.Info4("Taking ONLY Obj1 with LEFT");
                                    takeLeftArm(obj1, out objectsTaked);
                                }
                            }
                            else if (obj1isRightReach)
                            {
                                TBWriter.Info3("Obj1 is ONLY RIGTH reachable");
                                TBWriter.Info3("Taking ONLY Obj1 with RIGHT");
                                takeRightArm(obj1, out objectsTaked);
                            }
                            else if (obj1isLeftReach)
                            {
                                TBWriter.Info3("Obj1 is ONLY LEFT reachable");
                                TBWriter.Info3("Taking ONLY Obj1 with LEFT");
                                takeLeftArm(obj1, out objectsTaked);
                            }
                            else
                            {
                                TBWriter.Info3("Obj1 is NOT reachable");
                                TBWriter.Info3("Taking NOTHING");
                                objectsTaked = emptyAndEmpty;
                            }

                            TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                            return true;
                        }

                        #endregion

                        #region taking obj2

                        if (obj2 != null)
                        {
                            TBWriter.Info1("ONLY obj2="+obj2.Name+ "is not null");

                            if (obj2isBothReach)
                            {
                                TBWriter.Info3("Obj2 is BOTH reachable");
                                if (obj2isClosestToRight)
                                {
                                    TBWriter.Info4("But Obj2 is closest to RIGHT");
                                    TBWriter.Info4("Taking ONLY Obj2 with RIGHT");
                                    takeRightArm(obj2, out objectsTaked);
                                }
                                else
                                {
                                    TBWriter.Info4("But Obj2 is closest to LEFT");
                                    TBWriter.Info4("Taking ONLY Obj2 with LEFT");
                                    takeLeftArm(obj2, out objectsTaked);
                                }
                            }

                            else if (obj2isRightReach)
                            {
                                TBWriter.Info3("Obj2 is ONLY RIGTH reachable");
                                TBWriter.Info3("Taking ONLY Obj1 with RIGHT");
                                takeRightArm(obj2, out objectsTaked);
                            }
                            else if (obj2isLeftReach)
                            {
                                TBWriter.Info3("Obj2 is ONLY LEFT reachable");
                                TBWriter.Info3("Taking ONLY Obj2 with LEFT");
                                takeLeftArm(obj2, out objectsTaked);
                            }
                            else
                            {
                                TBWriter.Info3("Obj2 is NOT reachable");
                                TBWriter.Info3("Taking NOTHING");
                                objectsTaked = emptyAndEmpty;
                            }

                            TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                            return true;
                        }

                        #endregion

                        break;

                        #endregion

                    default:

                        #region State Default : Something unknown hapened

                        TBWriter.Info1("State Default : Something unknown hapened");
                        TBWriter.Info3("Taking NOTHING");
                        
                        cmd_TakeObjectRunning = false;
                        TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
                        return false;
                        break;

                        #endregion

                }
            }

            TBWriter.Spaced("    ---  Finishing TakeObject Command  ---");
            return false;
        }

		public bool Cmd_RememberScene(string sceneNodeName)
		{
			TBWriter.Info1("	--->  [Cmd_RememberScene]   STARTING  ---");

			if (!robot.modules[Module.ObjectFnd].IsConnected)
			{
				TBWriter.Error("[Cmd_RememberScene] cant be executed: " + Module.ObjectFnd + "is not conncted");
				TBWriter.Info1("    <--- [Cmd_RememberScene] FINISHED ---");
				return false;
			}

			ActualizeHeadPos();
			ActualizeTorsoPos();
			ActualizeMobilBasePos();

			if (cmdMan.OBJ_FIND_setNode(sceneNodeName))
				TBWriter.Info2("[Cmd_RememberScene] OBJ_FND successfully set Node{" + sceneNodeName + "}");
			else
			{
				TBWriter.Error("[Cmd_RememberScene] OBJ_FND can't setNode");
				TBWriter.Info1("    <--- [Cmd_RememberScene] FINISHED ---");
				return false;
			}

			if (cmdMan.OBJ_FND_saveNodeMap(sceneMap.FileSceneMap))
				TBWriter.Info2("[Cmd_RememberScene] OBJ_FND map saved as {" + sceneMap.FileSceneMap + "}");
			else
				TBWriter.Warning1("[Cmd_RememberScene] OBJ_FND cant save file as {" + sceneMap.FileSceneMap + "}");

			if (sceneMap.ContainsNode(sceneNodeName))
				TBWriter.Info2("[Cmd_RememberScene] Found SceneNode{" + sceneNodeName + "} , adding frame to it...");
			else
			{
				TBWriter.Info2("[Cmd_RememberScene] Creating new SceneNode{" + sceneNodeName + " in SceneMap, adding frame to it...");
				sceneMap.AddNode(sceneNodeName, robot.Position);
			}

			double orientation = robot.Orientation + robot.head.Pan + robot.torso.Pan;
			SceneFrame newframe = new SceneFrame(orientation, robot.head.Tilt, robot.torso.Elevation);
			sceneMap.AddSceneFrame(sceneNodeName, newframe);
			int frameCount = sceneMap.GetNodeFrames(sceneNodeName).Length;
			TBWriter.Info1("[Cmd_RememberScene] Added new SceneFrame (" + (frameCount - 1) + ")  to ScenenNode: " + sceneMap.GetNodeFrames(sceneNodeName)[frameCount - 1].ToString());

			if( sceneMap.SaveSceneMap() )
				TBWriter.Info2("[Cmd_RememberScene] SceneMap saved as {" + sceneMap.FileSceneMap + "}");
			else
				TBWriter.Warning1("[Cmd_RememberScene]  Cant save file as {" + sceneMap.FileSceneMap + "}");

			TBWriter.Info1("    <--- [Cmd_RememberScene] FINISHED ---");
			return true;
		}

		public bool Cmd_AproachToSceneNode(string sceneNodeName)
		{
			TBWriter.Info1("	--->  [Cmd_AproachToScene]   STARTING  ---");

			if (!sceneMap.ContainsNode(sceneNodeName))
			{
				TBWriter.Error("[Cmd_AproachToScene] can't be executed : unknown sceneName {" + sceneNodeName + "} ");
				TBWriter.Info1("    <--- [Cmd_RememberScene] FINISHED ---");
				return false;
			}

			
			if (sceneMap.GetNodeFrames( sceneNodeName).Length < 1)
			{
				TBWriter.Error("[Cmd_AproachToScene] can't be executed : no frames in Scene {" + sceneNodeName + "} ");
				TBWriter.Info1("    <--- [Cmd_AproachToScene] FINISHED ---");
				return false;
			}

			Vector3 nodeLocation = sceneMap.GetNodeLocation(sceneNodeName);
			SceneFrame[] nodeFrames = sceneMap.GetNodeFrames(sceneNodeName);
			double nodeOrientation = nodeFrames[0].Orientation;
			double headPan = 0;
			double torsoPan = 0;

			TBWriter.Info4("[Cmd_AproachToScene] navigating to SceneNode location");
			if( NavigateTo( nodeLocation, nodeOrientation))
			{
				TBWriter.Error("[Cmd_AproachToScene] can't be terminated : can't navigate to nodeLocation {" + nodeLocation + "} ");
				TBWriter.Info1("    <--- [Cmd_AproachToScene] FINISHED ---");
				return false;
			}

			Vector3 errorPos;
			double errorAngle;

			for (int i = 0; i < nodeFrames.Length; i++)
			{		
				TBWriter.Info4("[Cmd_AproachToScene] Verifing frame no."+i+ nodeFrames[i].ToString());
	
				cmdMan.HEAD_lookat( headPan,  nodeFrames[i].HeadTilt); 

				if( !MoveTorso( nodeFrames[i].TorsoElevation, torsoPan))
					TBWriter.Warning1("[Cmd_AproachToScene] finding frame{" +i+"} : can't MoveTorso");
				if( !cmdMan.WaitForResponse( JustinaCommands.HEAD_lookat, 3000))
					TBWriter.Warning1( "[Cmd_AproachToScene] finding frame{" +i+"} : can't MoveHead");



				//if(cmdMan.OBJ_FND_getNode( sceneNodeName, i, out errorPos,out  errorAngle)
				//{

				//}
			}
			


			return true;

		}

		public bool Cmd_AlignWithEdge(double desireAngle, double desiredDistance)
		{
			TBWriter.Info1("    --->  Starting Cmd_AlignWithEdge Command  ---");

			Vector3 point1;
			Vector3 point2;
			Vector3 point1InKinect;
			Vector3 point2InKinect;

			int[] tireSpeeds;

			int maxSpeed = 150;
			double distanceTolerance = 0.05;
			double angleTolerance = MathUtil.ToRadians(5);
			double errorDist;
			double errorAngle;

			if (!cmdMan.HEAD_lookat(0, robot.head.MinTilt, 5000)) TBWriter.Warning1("Cant execute HEAD_lookat(0," + robot.head.MinTilt + ")");
			if (!cmdMan.TRS_abspos(0.7, 0, 15000)) TBWriter.Warning1("Cant execute TRS_abspos");

			ActualizeHeadPos();
			ActualizeTorsoPos();

			int attemptsToFindLine = 0;
			int maxAttemptsToFindLine = 3;
			double distDecrement = -0.25;

			while (!this.cmdMan.OBJ_FIND_findEdge(this.robot.head.Tilt, out point1InKinect, out point2InKinect))
			{
				attemptsToFindLine++;
				TBWriter.Warning2("Can't findEdge in attempt #" + attemptsToFindLine + " . Moving away [" + distDecrement.ToString("0.00") + " m] ");
				this.cmdMan.MVN_PLN_move(distDecrement, 10000);

				if (attemptsToFindLine > maxAttemptsToFindLine)
				{
					TBWriter.Error("Max attempts to find line reached (" + maxAttemptsToFindLine + "). Can't find Edge");
					TBWriter.Info1("    <---  Finishing Cmd_AlignWithEdge Command  ---");
					return false;
				}
			}

			point1 = robot.TransHeadKinect2Robot(point1InKinect);
			point2 = robot.TransHeadKinect2Robot(point2InKinect);
			TBWriter.Info3("Found EdgePoints: P1[" + point1.ToString() + ", dist=" + point1.Magnitude.ToString("0.00") + "] ; P2[" + point2.ToString() + ", dist=" + point2.Magnitude.ToString("0.00") + "]");
			CalculateEdgeParams(point1, point2, out errorAngle, out errorDist);
			errorDist -= desiredDistance;

			int steps = 0;
			int maxSteps = 20;

			while ((Math.Abs(errorAngle) > angleTolerance) || (Math.Abs(errorDist) > distanceTolerance))
			{
				CalculateTireSpeeds(errorAngle, errorDist, maxSpeed, angleTolerance, distanceTolerance, out tireSpeeds);
				TBWriter.Info2("Sending Speeds: Left[" + tireSpeeds + "] ; Right[" + tireSpeeds + "]");

				if (!this.cmdMan.MVN_PLN_setSpeeds(tireSpeeds[0], tireSpeeds[1]))
				{
					TBWriter.Error("MVN_PLN_setSpeeds returned false. can't move MobileBase. The goal can't be reached");
					TBWriter.Info1("    <---  Finishing Cmd_AlignWithEdge Command  ---");
					return false;
				}

				if (steps >= maxSteps)
				{
					TBWriter.Error("Max Steps reached. The goal can't be reached");
					TBWriter.Info1("    <---  Finishing Cmd_AlignWithEdge Command  ---");
					return false;
				}

				if (!cmdMan.OBJ_FIND_findEdge(this.robot.head.Tilt, out point1InKinect, out point2InKinect))
				{
					TBWriter.Warning2("OBJ_FIND_findEdge returned false. Can't findEdge. The goal can't be reached");
					TBWriter.Info1("    <---  Finishing Cmd_AlignWithEdge Command  ---");
					return true;
				}

				point1 = robot.TransHeadKinect2Robot(point1InKinect);
				point2 = robot.TransHeadKinect2Robot(point2InKinect);
				TBWriter.Info3("Found EdgePoints: P1[" + point1.ToString() + ", dist=" + point1.Magnitude.ToString("0.00") + "] ; P2[" + point2.ToString() + ", dist=" + point2.Magnitude.ToString("0.00") + "]");

				CalculateEdgeParams(point1, point2, out errorAngle, out errorDist);
				TBWriter.Info1("Found Errors in step " + steps + ": DistError[" + errorDist.ToString("0.00") + "] , AngleError[" + errorAngle.ToString("0.000") + "]. Alignin with edge");

				errorAngle -= desiredDistance;

				steps++;
			}

			TBWriter.Info1("Reached");
			TBWriter.Info1("    <---  Finishing Cmd_AlignWithEdge Command  ---");

			return true;
		}

		public bool Cmd_TakeTable()
		{
			double x0, y0, z0, x1, y1, z1;
			double headPan = 0;
			double headTilt = -1.1;
			
			double xp0, yp0, zp0, xp1, yp1, zp1; 
			Vector3 laIni,raIni;
			Vector3 laEnd,  raEnd;

			this.cmdMan.MVN_PLN_move(-0.5, 0, 4000);

			this.cmdMan.HEAD_lookat(headPan, headTilt, 7000);

			while (!this.cmdMan.OBJ_FNDT_findedgereturns(headTilt, out x0, out y0, out z0, out x1, out y1, out z1, 5000)) ;
			
			laIni = new Vector3(x0, y0, z0);
			raIni = new Vector3(x1, y1, z1);

			laIni = TransHeadToRobot(laIni, headPan);
			raIni = TransHeadToRobot(raIni, headPan);

			double distanceToCorrect = (laIni.X+raIni.X)/2;
												
			headPan = 1.3;
			this.cmdMan.HEAD_lookat(headPan, headTilt, 7000);
			Thread.Sleep(3000);
			while (!this.cmdMan.OBJ_FNDT_findedgereturns(headTilt, out xp0, out yp0, out zp0, out xp1, out yp1, out zp1, 5000)) ;
				

			laEnd = new Vector3(xp0, yp0, zp0);
			raEnd = new Vector3(xp1, yp1, zp1);

			laEnd = TransHeadToRobot(laEnd, headPan);
			raEnd = TransHeadToRobot(raEnd, headPan);

			Vector3 line1 = raIni - laIni;
			Vector3 line2 = raEnd - laEnd;

			double angleToCorrect = Math.Acos((line1.X * line2.X + line1.Y * line2.Y + line1.Z * line2.Z)/(line1.Magnitude*line2.Magnitude));

			this.TakePoints(laIni,raIni,laEnd,raEnd,headTilt,0.3,out laIni,out raIni,out laEnd,out raEnd);

			laIni = this.TransRobotToLeftArm(laIni);
			raIni = this.TransRobotToRightArm(raIni);

			laEnd = this.TransRobotToLeftArm(laEnd);
			raEnd = this.TransRobotToRightArm(raEnd);
			
			this.cmdMan.ARMS_arms_goto("navigation",7000);
			

			this.cmdMan.ARMS_la_opengrip();
			this.cmdMan.ARMS_ra_opengrip();

			this.cmdMan.HEAD_lookat(0, headTilt, 7000);

			if (laIni.Y > 0.15 || raIni.Y > 0.15) 
			{
				double lrobotX, lrobotY, lorientation, robotX, robotY, orientation, distance;
				this.cmdMan.MVN_PLN_position(out lrobotX, out lrobotY, out lorientation, 1000);
				this.cmdMan.MVN_PLN_move(distanceToCorrect+0.1,5000);
				this.cmdMan.MVN_PLN_position(out robotX, out robotY, out orientation, 1000);
				distance = Math.Sqrt((lrobotX - robotX) * (lrobotX - robotX) + (lrobotY - robotY) * (lrobotY - robotY));
				laIni.Y -= distance;
				raIni.Y -= distance;
				laEnd.Y -= distance;
				raEnd.Y -= distance;
			}
			laIni.Z = 0;
			raIni.Z = 0;
			laEnd.Z = 0;
			raEnd.Z = 0;
			raEnd.Y = raIni.Y + 0.33 * Math.Sin(angleToCorrect);
			//this.cmdMan.ARMS_la_abspos(laIni.X-0.13,laIni.Y,laIni.Z,1.4168,0.2183,1.4358,0);
			//this.cmdMan.ARMS_ra_abspos(raIni.X - 0.13, raIni.Y , raIni.Z, 1.4168, 0.2183, 1.4358, 0);
			this.cmdMan.ARMS_la_abspos(laIni.X - 0.13, laIni.Y, laIni.Z, 1.5708, 0.0, 1.5708, 0, 20000);
			this.cmdMan.ARMS_ra_abspos(raIni.X - 0.13, raIni.Y, raIni.Z, 1.5708, 0.0, 1.5708, 0, 20000);

			
			
			//Thread.Sleep(5000);

			//this.cmdMan.ARMS_la_abspos(laEnd.X - 0.13, laEnd.Y + 0.11, laEnd.Z, 1.4168, 0.2183, 1.4358, 0);
			//this.cmdMan.ARMS_ra_abspos(raEnd.X - 0.13, raEnd.Y + 0.13, raEnd.Z, 1.4168, 0.2183, 1.4358, 0);
			//this.cmdMan.ARMS_la_abspos(laEnd.X - 0.13, laEnd.Y + 0.11, laEnd.Z, 1.5708, 0.0, 1.5708, 0);
			//this.cmdMan.ARMS_ra_abspos(raEnd.X - 0.13, raEnd.Y + 0.13, raEnd.Z, 1.5708, 0.0, 1.5708, 0);
			this.cmdMan.ARMS_la_abspos(laEnd.X - 0.10, laEnd.Y + 0.11, laEnd.Z, 1.5708, 0.0, 1.5708, 0, 20000);
			this.cmdMan.ARMS_ra_abspos(raEnd.X - 0.10, raEnd.Y + 0.13, raEnd.Z, 1.5708, 0.0, 1.5708, 0, 20000);
			return true;
		}

		public bool Cmd_TakeTray(double height)
		{
			double x0, y0, z0, x1, y1, z1;
			double lp0, lp1, lp2, lp3, lp4, lp5, lp6;
			double rp0, rp1, rp2, rp3, rp4, rp5, rp6;
			double headPan = 0;
			double headTilt = -1.1;
			double percent=70;

			this.cmdMan.HEAD_lookat(headPan, headTilt, 7000);
			if (!this.cmdMan.OBJ_FNDT_findedgefastandfurious(headTilt, height, out x0, out y0, out z0, out x1, out y1, out z1, 5000)) return false;
			
			Vector3 larmPoint = new Vector3(x0, y0, z0);
			Vector3 rarmPoint = new Vector3(x1, y1, z1);

			larmPoint = TransHeadToRobot(larmPoint, headPan);
			rarmPoint = TransHeadToRobot(rarmPoint, headPan);

			double distanceToCorrect = (larmPoint.X + rarmPoint.X) / 2;

			larmPoint.Y -= 0.01;
			//rarmPoint.Y += 0.01;

			larmPoint = TransRobotToLeftArm(larmPoint);
			rarmPoint = TransRobotToRightArm(rarmPoint);

			this.cmdMan.ARMS_arms_goto("navigation", 7000);

			//Poner Apertura
			this.cmdMan.ARMS_la_opengrip(percent,3000);
			this.cmdMan.ARMS_ra_opengrip(percent,3000);

			this.cmdMan.ARMS_la_abspos(larmPoint.X - 0.05, larmPoint.Y  , larmPoint.Z, 1.4168, 0.2183, 1.4358, 0,10000);
			this.cmdMan.ARMS_ra_abspos(rarmPoint.X - 0.05, rarmPoint.Y , rarmPoint.Z, 1.4168, 0.2183, 1.4358, 0, 10000);

			Thread.Sleep(2000);

			this.cmdMan.ARMS_la_closegrip(3000);

			this.cmdMan.ARMS_ra_closegrip(3000);

			this.cmdMan.ARMS_la_artpos(out lp0, out lp1, out lp2, out lp3, out lp4, out lp5, out lp6, 10000);
			Thread.Sleep(3000);
			this.cmdMan.ARMS_ra_artpos(out rp0, out rp1, out rp2, out rp3, out rp4, out rp5, out rp6, 10000);

			this.cmdMan.ARMS_la_artpos(lp0, lp1, lp2, lp3, lp4, lp5, lp6, 10000);
			this.cmdMan.ARMS_ra_artpos(rp0, rp1, rp2, rp3 + 0.2, rp4, rp5, rp6, 10000);
			
			return true;
		
		}

        #endregion


		public Vector3 TransRobotToLeftArm(Vector3 vectorRobot) 
		{
			//return new Vector3(vectorRobot.Y, 1.35 - vectorRobot.Z, vectorRobot.X - 0.15);
			return new Vector3(1.35 - vectorRobot.Z, vectorRobot.X, -vectorRobot.Y + 0.17);
		}

		public Vector3 TransRobotToRightArm(Vector3 vectorRobot)
		{
			//return new Vector3(vectorRobot.Y, 1.14 - vectorRobot.Z, vectorRobot.X + 0.15);
			return new Vector3(1.35 - vectorRobot.Z, vectorRobot.X, -vectorRobot.Y - 0.17);
		}

		public Vector3 TransHeadToRobot(Vector3 vectorInKinectCoords, double headPan)
		{
			//Transforms a coordinate in the kinect system to robot system considering only the head pan. 
			//Vision returns the coordinates considering the kinect in an horizontal position, i.e.
			//it corrects the head tilt using the kinect accelerometers.
			Vector3 temp = new Vector3(vectorInKinectCoords.Z, -vectorInKinectCoords.X, vectorInKinectCoords.Y);
			//The order Z,-X, Y is to transform from kinect system to head system

			//1.75 is the Jutina's height
			//0.1 is the horizontal distance between Justinas kinematic center (point between tire axis)
			// and the front of kinect
			HomogeneousM R = new HomogeneousM(headPan, 1.75, 0.1, 0);
			return R.Transform(temp);
		}

        #region SubTasks

		public bool CheckObjectReachableRightArm(WorldObject obj1)
		{
			TBWriter.Info1("Verifying obj1 for right Arm");

			if (obj1 == null)
			{
				TBWriter.Info1(" objects are null");
				return false;
			}

			Vector3 armRightRobot = robot.TransRightArm2Robot(Vector3.Zero);
			TBWriter.Info2("Checking Obj1 : " + obj1.Name);


			/////////////

			TBWriter.Info8("Checking Obj1 for RightArm");

			Vector3 errorVec = obj1.Position - armRightRobot;
			double errorAngle = Math.Atan2(errorVec.Y, errorVec.X);

			double obj1distToRight = errorVec.Magnitude;
			Vector3 obj1RightArmPos = robot.TransRobot2RightArm(obj1.Position);
			bool obj1isRightReach = cmdMan.ARMS_ra_reachable(obj1RightArmPos.X, obj1RightArmPos.Y, obj1RightArmPos.Z, MathUtil.PiOver2, errorAngle, 0, -errorAngle, 5000);

			if (obj1isRightReach)
				TBWriter.Info8("Obj1 reachable for RightArm");
			else
				TBWriter.Info8("Obj1 is not reachable for RightArm");

			return obj1isRightReach;

		}

        public void CalculateEdgeParams(Vector3 point1, Vector3 point2, out double angleError, out double distError)
        {
            /////Calculo del error de angulo
            double  deltaX = (point1.Y > point2.Y) ? point2.X - point1.X : point1.X - point2.X;
            double  deltaY = (point1.Y > point2.Y) ? point2.Y - point1.Y : point1.Y - point2.Y;

            angleError = (deltaY == 0) ? 90 : -Math.Atan(deltaX / deltaY);
             
            /////Parametros de la ec. de la linea
            double A = point1.Y - point2.Y;
            double B = point2.X - point1.X;
            double C = point1.X * point2.Y - point2.X * point1.Y;

            distError = Math.Abs(C / Math.Sqrt(A * A + B * B));
        }

        public void CalculateTireSpeeds(double angleError, double distanceError, int maxSpeed ,double AngleTolerance, double DistanceTolerance, out int[] tiresSpeed)
        {
            tiresSpeed = new int[2];

            int maxPosVel = maxSpeed;
            int minPosVel = 140;
            int minOffsetVel = minPosVel - 127;
            int maxOffsetVel = maxPosVel - 127;
            int maxNegVel = 127 - maxOffsetVel;
            int minNegVel = 127- minOffsetVel;

            double angleForMaxSpeed = MathUtil.ToRadians(30);
            double angleTolerance = AngleTolerance;
            double distanceTolerance = DistanceTolerance;;
            double distanceForMaxSpeed = 0.5;

            
            if (Math.Abs(angleError) >= angleTolerance)
            {
                tiresSpeed[0] = (int)(127 - (maxOffsetVel / angleForMaxSpeed) * angleError);
                tiresSpeed[1] = (int)(127 + (maxOffsetVel / angleForMaxSpeed) * angleError);

                if (angleError > 0)
                {
                    if (tiresSpeed[0] < maxNegVel) 
                        tiresSpeed[0] = maxNegVel;
                    if (tiresSpeed[0] > minNegVel) 
                        tiresSpeed[0] = minNegVel;
                    
                    if (tiresSpeed[1] > maxPosVel) 
                        tiresSpeed[1] = maxPosVel;
                    if (tiresSpeed[1] < minPosVel) 
                        tiresSpeed[1] = minPosVel;
                }
                else
                {
                    if (tiresSpeed[0] > maxPosVel) 
                        tiresSpeed[0] = maxPosVel;
                    if (tiresSpeed[0] < minPosVel) 
                        tiresSpeed[0] = minPosVel;

                    if (tiresSpeed[1] < maxNegVel) 
                        tiresSpeed[1] = maxNegVel;
                    if (tiresSpeed[1] > minNegVel) 
                        tiresSpeed[1] = minNegVel;
                }
            }
            else
            {
                if (distanceError > distanceTolerance)
                {
                    tiresSpeed[0] = (int)(((maxPosVel - minPosVel) / (distanceForMaxSpeed - distanceTolerance)) * (distanceError-distanceForMaxSpeed) + maxPosVel);

                    if (tiresSpeed[0] > maxPosVel) 
                        tiresSpeed[0] = maxPosVel;
                    if (tiresSpeed[0] < minPosVel)
                        tiresSpeed[0] = minPosVel;
                }

                if (distanceError < -distanceTolerance)
                {
                    tiresSpeed[0] = (int)(((minNegVel - maxNegVel) / (distanceForMaxSpeed - distanceTolerance)) * (distanceError  -distanceTolerance) + minNegVel);
                    
                    if (tiresSpeed[0] > minNegVel)
                        tiresSpeed[0] = minNegVel;
                    if (tiresSpeed[0] < maxNegVel)
                        tiresSpeed[0] = maxNegVel;
                }
                tiresSpeed[1] = tiresSpeed[0];
            }
        }

        public void TakePoints(Vector3 lPoint, Vector3 rPoint, Vector3 p1,  Vector3 p2, double giroAngle, double sDistance, out Vector3 laIni, out Vector3 raIni, out Vector3 laEnd, out Vector3 raEnd)
		{
            double sAngle;
            Vector3 np1, np2;
            Vector3 newLine1 ;
            Vector3 newLine2 ; 
            
            obtainGraspPoints(lPoint, rPoint, sDistance, out laIni, out raIni);
            rotateLine(p1, p2, giroAngle, out np1, out np2);

            sAngle = Math.PI - getSeparationAngle(lPoint,rPoint,np1,np2);
            rotateLine(new Vector3(lPoint.X - lPoint.X, lPoint.Y- lPoint.Y, lPoint.Z -lPoint.Z), new Vector3(rPoint.X - lPoint.X, rPoint.Y - lPoint.Y, rPoint.Z - lPoint.Z), sAngle, out newLine1, out newLine2);
            //regresar 
            newLine1.X = newLine1.X + lPoint.X;
            newLine1.Y = newLine1.Y + lPoint.Y;
            newLine1.Z = newLine1.Z + lPoint.Z;
            newLine2.X = newLine2.X + lPoint.X;
            newLine2.Y = newLine2.Y + lPoint.Y;
            newLine2.Z = newLine2.Z + lPoint.Z;

            obtainGraspPoints(newLine1, newLine2, sDistance, out laEnd, out raEnd);
		}
 
        public double getSeparationAngle(Vector3 gLine1, Vector3 gLine2, Vector3 bLine1, Vector3 bLine2)
        {
            double glAngle, blAngle;

            glAngle = Math.Asin((gLine2.Y - gLine1.Y) / computeEuclideanDistance(gLine1, gLine2));
            blAngle = Math.Asin((bLine2.Y - bLine1.Y) / computeEuclideanDistance(bLine1, bLine2));

            return Math.PI - blAngle + glAngle;
        }
   
        public void rotateLine(Vector3 p1, Vector3 p2, double giroAngle, out Vector3 np1, out Vector3 np2)
        {
            np1 = new Vector3();
            np2 = new Vector3();
            Matrix rotationMatrix = new Matrix(3, 3);
            Matrix p1Matrix = new Matrix(3, 1);
            Matrix p2Matrix = new Matrix(3, 1);

            /*Rotation matrix*/
            rotationMatrix[0,0] = Math.Cos(giroAngle);
            rotationMatrix[0,1] = -Math.Sin(giroAngle);
            rotationMatrix[0,2] = 0;
			rotationMatrix[1, 0] = Math.Sin(giroAngle);
			rotationMatrix[1, 1] = Math.Cos(giroAngle);
            rotationMatrix[1,2] = 0;
            rotationMatrix[2,0] = 0;
            rotationMatrix[2,1] = 0;
            rotationMatrix[2,2] = 1;

            /*coordinates*/
            p1Matrix[0, 0] = p1.X; p1Matrix[1, 0] = p1.Y; p1Matrix[2, 0] = p1.Z;
            p2Matrix[0, 0] = p2.X; p2Matrix[1, 0] = p2.Y; p2Matrix[2, 0] = p2.Z;

            /*multiply matrix*/
            p1Matrix = rotationMatrix * p1Matrix;
            p2Matrix = rotationMatrix*p2Matrix ;

            np1.X = p1Matrix[0, 0];
            np1.Y = p1Matrix[1, 0];
            np1.Z = p1Matrix[2, 0];

            np2.X = p2Matrix[0, 0];
            np2.Y = p2Matrix[1, 0];
            np2.Z = p2Matrix[2, 0];

        }

		public void obtainGraspPoints(Vector3 lPoint, Vector3 rPoint, double sdMPoint, out Vector3 lGP, out Vector3 rGP)
		{
			Vector3 Ulr = new Vector3();
			Vector3 Url = new Vector3();

            lGP = new Vector3();
            rGP = new Vector3();

			double sDistance, Xm, Ym, Zm;

			/*Obtain middle point*/
			//Get separation distance from point to middle point
			//sDistance = computeEuclideanDistance(lPoint, rPoint);
			sDistance = (lPoint - rPoint).Magnitude;
			//Get units vector U lp->rp
			Ulr.X = (rPoint.X - lPoint.X) / sDistance;
			Ulr.Y = (rPoint.Y - lPoint.Y) / sDistance;
			Ulr.Z = (rPoint.Z - lPoint.Z) / sDistance;
			//Get units vector U rp->lp
			Url.X = (lPoint.X - rPoint.X) / sDistance;
			Url.Y = (lPoint.Y - rPoint.Y) / sDistance;
			Url.Z = (lPoint.Z - rPoint.Z) / sDistance;
			//Obtain x,y, coordinates of middle point
			Xm = (Url.X * (sDistance / 2)) + rPoint.X;
			Ym = (Url.Y * (sDistance / 2)) + rPoint.Y;
			Zm = (Url.Z * (sDistance / 2)) + rPoint.Z;
			
			/*Obtain grasp points*/
			//Obtain left grasp point
            lGP.X = (Url.X * sdMPoint) + Xm;
            lGP.Y = (Url.Y * sdMPoint) + Ym;
            lGP.Z = (Url.Z * sdMPoint) + Zm;
            //Obtain rigth grasp point
            rGP.X = (Ulr.X * sdMPoint) + Xm;
            rGP.Y = (Ulr.Y * sdMPoint) + Ym;
            rGP.Z = (Ulr.Z * sdMPoint) + Zm;
		}

		public double computeEuclideanDistance(Vector3 p1, Vector3 p2)
		{
			return Math.Sqrt(Math.Pow(p1.X - p2.X,2) + Math.Pow(p1.Y - p2.Y,2) + Math.Pow(p1.Z - p2.Z,2));
		}

        public bool waitForDoor()
        {
            bool isDoorOpen=false;
           
            TBWriter.Info1("Waiting for the door is open");
            this.cmdMan.SPG_GEN_say("I am waiting for the door is open");
            while(!isDoorOpen)
            {
                if(!this.cmdMan.MVN_PLN_obstacles("door",2000))
                {
                    TBWriter.Info8("Door still closed");
                    Thread.Sleep(500);
                }
                else
                {
                    TBWriter.Info8("Door is now open");
                    this.cmdMan.SPG_GEN_say("I can see now that the door is open");
                    isDoorOpen=true;
                }
            }
            return true;

        }

        public void timerThreadTask()
        {
            Stopwatch timer = new Stopwatch();

            while (!technicalChallengeTimeOut)
            {
                if (timer.ElapsedMilliseconds >= 270000)
                {
                    TBWriter.Info1("Technical challenge time out, elapsed time = " + timer.ElapsedMilliseconds.ToString());
                    technicalChallengeTimeOut = true;
                }
                else
                {
                    TBWriter.Info8("Elapsed time = " + timer.ElapsedMilliseconds.ToString());
                    Thread.Sleep(500);
                }
            }
        }


        public bool Cmd_DoTechnicalChallenge()
        {
            timerThread.IsBackground = true;
            timerThread = new Thread(new ThreadStart(timerThreadTask));

            bool search = true;
            int location = 0;
            int maxLocationNumber = 2;


            this.cmdMan.ARMS_la_goto("navigation");
            this.cmdMan.ARMS_ra_goto("navigation", 7000);
            this.cmdMan.WaitForResponse(JustinaCommands.ARMS_la_goto, 1000);
            this.cmdMan.TRS_abspos(0.43, 0, 7000);
            this.cmdMan.HEAD_lookat(0, -0.9, 7000);


            waitForDoor();
            timerThread.Start();


            while ((search)&&(!technicalChallengeTimeOut))
            {
                TBWriter.Info1(" Getting close to location " + location.ToString());
                if (!this.cmdMan.MVN_PLN_getclose("dinnertable" + location.ToString(), 60000))
                {
                    TBWriter.Error("Can't get close to location" + location.ToString());
                    location++;

                    if (maxLocationNumber <= location) search = false;
                    continue;
                }

                ActualizeHeadPos();
                ActualizeTorsoPos();

                TBWriter.Info1("Sending Find on table with headAngle " + MathUtil.ToDegrees(robot.head.Tilt).ToString("0.00"));
                this.cmdMan.OBJ_FND_findontable(robot.head.Tilt );
                
                Thread.Sleep(10000);

                location++;

                if (maxLocationNumber <= location)
                    search = false;
            }

            this.cmdMan.SPG_GEN_say("I have finished the search. Please, look at my screen, I am showing the objects I found");
            this.cmdMan.MVN_PLN_move(0, MathUtil.PiOver2);

            return true;
        }

        public void learnArmMotionThreadTask()
        {
            TBWriter.Spaced("   === Starting learnArmMotionThreadTask");

            #region cheking Availability

            if (!robot.modules[Module.Head].IsConnected)
            {
                TBWriter.Warning1("Cant execute learnArmMotionThreadTask, " + Module.Head + " is not connected");
                TBWriter.Spaced("   === Finished learnArmMotionThreadTask");
                return;
            }
            if (!robot.modules[Module.ObjectFnd].IsConnected)
            {
                TBWriter.Error("learnArmMotionThreadTask can't be executed , " + Module.ObjectFnd + " is not connected");
                TBWriter.Spaced("   === Finished learnArmMotionThreadTask");
                return;
            }
            if (!robot.modules[Module.Arms].IsConnected)
            {
                TBWriter.Warning1("learnArmMotionThreadTask can't be executed , " + Module.Arms + " is not connected");
            }

            #endregion

            string armToUse;
            string armToSay;
            Vector3 currentArmPosInKnt;
            Vector3 currentArmPosInRobot;
            List<Vector3> positionListInRobot;


            cmdMan.TRS_abspos(0.7, 0, 7000);


            cmdMan.ARMS_ra_goto(Arm.ppHome);
            cmdMan.ARMS_la_goto(Arm.ppHome, 7000);
            cmdMan.WaitForResponse(JustinaCommands.ARMS_ra_goto, 5000);

            cmdMan.ARMS_ra_torque(false, 3000);
            cmdMan.ARMS_la_torque(false, 3000);
            cmdMan.WaitForResponse(JustinaCommands.ARMS_ra_torque, 1000);



            TBWriter.Info2("Moving Head to 0, -1.1");
            cmdMan.HEAD_lookat(0, -1.1, 3000);

            cmdMan.SPG_GEN_say("Please, Take my  hand. I will learn the movement " + learnMotion_name, 5000);

            cmdMan.SPG_GEN_say("Place my gripper in front of me, so i can see it");
            Thread.Sleep(learnMotion_cantFindHandWaits_ms);


            ArmMotion learningMotion;

            string armInsight;
            if (cmdMan.OBJ_FND_findArmInSight(out armInsight, 4000))
            {
                if (armInsight == "right")
                {
                    learningMotion = new ArmMotion(this.learnMotion_name, true);
                    armToSay = "right";
                    armToUse = "rhand";
                }
                else
                {
                    learningMotion = new ArmMotion(this.learnMotion_name, false);
                    armToSay = "left";
                    armToUse = "lhand";
                }
            }



            #region validating armfind

            if (!cmdMan.OBJ_FND_findArmInSight(out armInsight, 4000))
            {
                cmdMan.SPG_GEN_say("I can't see my  hand, please, move it in front of me", 2000);
                Thread.Sleep(learnMotion_cantFindHandWaits_ms);

                if (!cmdMan.OBJ_FND_findArmInSight(out armInsight, 4000))
                {
                    cmdMan.SPG_GEN_say("I don't see my hand, move it again", 2000);
                    Thread.Sleep(learnMotion_cantFindHandWaits_ms);

                    if (!cmdMan.OBJ_FND_findArmInSight(out armInsight, 4000))
                    {
                        cmdMan.SPG_GEN_say("Sorry, i cant see my hand. I cant learn the movements");
                        TBWriter.Spaced("   === Finished learnArmMotionThreadTask");
                        return;
                    }
                }
            }



            if (armInsight == "right")
            {
                learningMotion = new ArmMotion(this.learnMotion_name, true);
                armToSay = "right";
                armToUse = "rhand";
            }
            else
            {
                learningMotion = new ArmMotion(this.learnMotion_name, false);
                armToSay = "left";
                armToUse = "lhand";
            }


            #endregion

            TBWriter.Info1("Learning ArmMotion [ " + learningMotion.Name + " ] using " + armToUse);

            ActualizeHeadPos();
            ActualizeTorsoPos();

            positionListInRobot = new List<Vector3>();

            cmdMan.SPG_GEN_say("Start motion, now", 5000);


            TBWriter.Info2("Reading Arm Motion");
            int steps = 1;
            while (learnMotionRunning && (steps < this.learnMotion_maxSteps))
            {
                if (cmdMan.OBJ_FND_findArm(armToUse, out currentArmPosInKnt, 500))
                {
                    currentArmPosInRobot = robot.TransHeadKinect2Robot(currentArmPosInKnt);
                    positionListInRobot.Add(currentArmPosInRobot);
                }
                else
                {
                    Vector3 nullVec = null;
                    positionListInRobot.Add(nullVec);
                }

                Thread.Sleep(learnMotion_sleepInAdquisition);
                steps++;
            }

            if (steps >= this.learnMotion_maxSteps)
                TBWriter.Warning1("Reached maxSteps in armMotion, steps: " + steps.ToString());



            bool useSpin = false;

            double currentDistance;
            double prevDistance;
            double prevPrevDistance;

            bool currentIsMotion;
            bool prevIsMotion;
            bool prevPrevIsMotion;

            bool lastMotionDetected = false;
            bool startSpinDetected = false;
            bool stopSpinDtected = false;
            int indexStopSpin = 0;
            int indexStartSpin = 0;
            int lastPosIndex = 0;


            Vector3 mirrorPosition;
            List<Vector3> rightArmPositionsInArms = new List<Vector3>();
            List<Vector3> leftArmPositionsInArms = new List<Vector3>();


            TBWriter.Info2("Processing Motion : No. of steps = " + positionListInRobot.Count);

            for (int i = positionListInRobot.Count - 1; i > 2; i--)
            {
                if ((positionListInRobot[i] != null) && (positionListInRobot[i - 1] != null) && (positionListInRobot[i - 2] != null) && (positionListInRobot[i - 3] != null))
                {
                    currentDistance = (positionListInRobot[i] - positionListInRobot[i - 1]).Magnitude;
                    prevDistance = (positionListInRobot[i - 1] - positionListInRobot[i - 2]).Magnitude;
                    prevPrevDistance = (positionListInRobot[i - 2] - positionListInRobot[i - 3]).Magnitude;

                    currentIsMotion = currentDistance > maxDistanceForNoMotion ? true : false;
                    prevIsMotion = prevDistance > maxDistanceForNoMotion ? true : false;
                    prevPrevIsMotion = prevPrevDistance > maxDistanceForNoMotion ? true : false;


                    // Last motion
                    if ((!lastMotionDetected) && !currentIsMotion && !prevIsMotion && prevPrevIsMotion)
                    {
                        if (learningMotion.RightArm)
                        {
                            rightArmPositionsInArms.Add(robot.TransRobot2RightArm(positionListInRobot[i]));

                            mirrorPosition = new Vector3(positionListInRobot[i]);
                            mirrorPosition.Y = -mirrorPosition.Y;

                            leftArmPositionsInArms.Add(robot.TransRobot2LeftArm(mirrorPosition));
                        }
                        else
                        {
                            leftArmPositionsInArms.Add(robot.TransRobot2LeftArm(positionListInRobot[i]));

                            mirrorPosition = new Vector3(positionListInRobot[i]);
                            mirrorPosition.Y = -mirrorPosition.Y;

                            rightArmPositionsInArms.Add(robot.TransRobot2RightArm(mirrorPosition));
                        }

                        lastMotionDetected = true;
                    }

                    // intermediate positions
                    if (currentIsMotion && !prevIsMotion && !prevPrevIsMotion)
                    {
                        if (learningMotion.RightArm)
                        {
                            rightArmPositionsInArms.Add(robot.TransRobot2RightArm(positionListInRobot[i]));

                            mirrorPosition = new Vector3(positionListInRobot[i]);
                            mirrorPosition.Y = -mirrorPosition.Y;

                            leftArmPositionsInArms.Add(robot.TransRobot2LeftArm(mirrorPosition));
                        }
                        else
                        {
                            leftArmPositionsInArms.Add(robot.TransRobot2LeftArm(positionListInRobot[i]));

                            mirrorPosition = new Vector3(positionListInRobot[i]);
                            mirrorPosition.Y = -mirrorPosition.Y;

                            rightArmPositionsInArms.Add(robot.TransRobot2RightArm(mirrorPosition));
                        }
                    }
                }

                // Stop Spin
                if ((!startSpinDetected) && (positionListInRobot[i] != null) && (positionListInRobot[i - 1] == null) && (positionListInRobot[i - 2] == null) && (positionListInRobot[i - 3] == null))
                {
                    if (learningMotion.RightArm)
                    {
                        rightArmPositionsInArms.Add(robot.TransRobot2RightArm(positionListInRobot[i]));

                        mirrorPosition = new Vector3(positionListInRobot[i]);
                        mirrorPosition.Y = -mirrorPosition.Y;

                        leftArmPositionsInArms.Add(robot.TransRobot2LeftArm(mirrorPosition));
                    }
                    else
                    {
                        leftArmPositionsInArms.Add(robot.TransRobot2LeftArm(positionListInRobot[i]));

                        mirrorPosition = new Vector3(positionListInRobot[i]);
                        mirrorPosition.Y = -mirrorPosition.Y;

                        rightArmPositionsInArms.Add(robot.TransRobot2RightArm(mirrorPosition));
                    }

                    startSpinDetected = true;
                    learningMotion.UseSpin = true;
                    learningMotion.SpinStopIndex = rightArmPositionsInArms.Count - 1;

                    useSpin = true;
                }

                // Start Spin
                if ((!stopSpinDtected) && (positionListInRobot[i] == null) && (positionListInRobot[i - 1] == null) && (positionListInRobot[i - 2] == null) && (positionListInRobot[i - 3] != null))
                {
                    if (learningMotion.RightArm)
                    {
                        rightArmPositionsInArms.Add(robot.TransRobot2RightArm(positionListInRobot[i]));

                        mirrorPosition = new Vector3(positionListInRobot[i]);
                        mirrorPosition.Y = -mirrorPosition.Y;

                        leftArmPositionsInArms.Add(robot.TransRobot2LeftArm(mirrorPosition));
                    }
                    else
                    {
                        leftArmPositionsInArms.Add(robot.TransRobot2LeftArm(positionListInRobot[i]));

                        mirrorPosition = new Vector3(positionListInRobot[i]);
                        mirrorPosition.Y = -mirrorPosition.Y;

                        rightArmPositionsInArms.Add(robot.TransRobot2RightArm(mirrorPosition));
                    }

                    stopSpinDtected = true;
                    learningMotion.SpinStartIndex = rightArmPositionsInArms.Count - 1;
                }
            }

            learningMotion.LeftArmPositions = new Vector3[leftArmPositionsInArms.ToArray().Length];
            learningMotion.RightArmPositions = new Vector3[rightArmPositionsInArms.ToArray().Length];

            leftArmPositionsInArms.ToArray().CopyTo(learningMotion.LeftArmPositions, 0);
            rightArmPositionsInArms.ToArray().CopyTo(learningMotion.RightArmPositions, 0);

            if (learnedArmMotionsList.ContainsKey(learningMotion.Name))
                learnedArmMotionsList.Remove(learningMotion.Name);

            learnedArmMotionsList.Add(learningMotion.Name, learningMotion);

            TBWriter.Info1(" Learned ArmMotion [ " + learningMotion.Name + " ] , No. of steps = " + rightArmPositionsInArms.Count + " , useSpin = " + useSpin.ToString());
            TBWriter.Spaced("   === Finished learnArmMotionThreadTask");
        }

        public bool takeBothArmsOnShelf(WorldObject objToRight, WorldObject objToLeft, out string objectsTaked)
        {
            TBWriter.Info1("	--->	Starting TakeBothArms");

            objectsTaked = "";

            TBWriter.Info1("Trying to take : with leftArm  " + objToLeft + " ; with rghtArm " + objToRight); 

            takeRightObject = new WorldObject(objToRight);
            takeLeftObject = new WorldObject(objToLeft);

            takeRightArmThread = new Thread(new ThreadStart(takeRightArmThreadTask));
            takeLeftArmThread = new Thread(new ThreadStart(takeLeftArtThreadTask));
            takeRightArmThread.IsBackground = true;
            takeLeftArmThread.IsBackground = true;

            takeRightArmRunning = true;
            takeLeftArmRunning = true;

            takeRightArmThread.Start();
            takeLeftArmThread.Start();

            while ((takeRightArmRunning) || (takeLeftArmRunning))
            {
                Thread.Sleep(100);
            }

            if (takeLeftArmThread.IsAlive)
                takeLeftArmThread.Abort();

            if (takeRightArmThread.IsAlive)
                takeRightArmThread.Abort();


            bool verify;

            ActualizeHeadPos();

            verifyObjOnShelf(out objectsTaked);
            
            if (!objectsTaked.Contains(objToRight.Name))
            {
                objectsTaked = objToRight.Name + " ";
                robot.RightArmIsEmpty = false;
                rightArmObject = objToRight.Name;
                TBWriter.Info1("Successfully taked " + objToRight.Name + " with rightArm,");
            }

            if (!objectsTaked.Contains(objToLeft.Name))
            {
                objectsTaked = objToLeft.Name + " ";
                robot.LeftArmIsEmpty = false;
                leftArmObject = objToLeft.Name;
                TBWriter.Info1("successfully taked " + objToLeft.Name + " with lefttArm, now is not empty");
            }

            TBWriter.Info1("	Finishing TakeBothArms	<---");
            return true;
        }


        public bool takeBothArms2(WorldObject objToRight, WorldObject objToLeft, out string objectsTaked)
        {
            TBWriter.Info1("	--->	Starting TakeBothArms: [Left " + objToLeft.Name + "]" + " [Right " + objToRight.Name + "]");

            objectsTaked = "";

            double distBetweenObjs;
            double minDistBetweenObjs = 0.30;

            bool takeSimultaneous=true;

            takeRightObject = new WorldObject(objToRight);
            takeLeftObject = new WorldObject(objToLeft);

            takeRightArmThread = new Thread(new ThreadStart(takeRightArmThreadTask));
            takeLeftArmThread = new Thread(new ThreadStart(takeLeftArtThreadTask));
            takeRightArmThread.IsBackground = true;
            takeLeftArmThread.IsBackground = true;

            takeRightArmRunning = true;
            takeLeftArmRunning = true;


            distBetweenObjs = (objToLeft.Position - objToRight.Position).Magnitude;

            if (takeSimultaneous && distBetweenObjs > minDistBetweenObjs)
            {
                takeRightArmThread.Start();
                takeLeftArmThread.Start();

                while (takeLeftArmRunning || takeLeftArmRunning)
                    Thread.Sleep(1000);

                if (takeLeftArmThread.IsAlive)
                    takeLeftArmThread.Abort();

                if (takeRightArmThread.IsAlive)
                    takeRightArmThread.Abort();

                objectsTaked = objToLeft.Name + " " + objToRight.Name;
            }
            else
            {
                takeRightArmThread.Start();

                while ((takeRightArmRunning))
                    Thread.Sleep(1000);

                takeLeftArmThread.Start();

                while ((takeLeftArmRunning))
                    Thread.Sleep(1000);

                if (takeLeftArmThread.IsAlive)
                    takeLeftArmThread.Abort();

                if (takeRightArmThread.IsAlive)
                    takeRightArmThread.Abort();

                objectsTaked = objToLeft.Name + " " + objToRight.Name;

            }

            TBWriter.Info1("	Finishing TakeBothArms	<---");
            return true;
        }

        public bool takeBothArms(WorldObject objToRight, WorldObject objToLeft, out string objectsTaked)
        {
            TBWriter.Info1("	===>    Starting TakeBothArms: [LeftArm:" + objToLeft.Name + "]" + " [RightArm:" + objToRight.Name + "]");

            objectsTaked = "";

            double distBetweenObjs;
            double minDistBetweenObjs = 0.30;

            bool takeSimultaneous; 

            // if cant see both arms at same time
            //takeRightArm(objToRight);
            //takeLeftArm(objToLeft);        

            takeRightObject = new WorldObject(objToRight);
            takeLeftObject = new WorldObject(objToLeft);

            takeRightArmThread = new Thread(new ThreadStart(takeRightArmThreadTask));
            takeLeftArmThread = new Thread(new ThreadStart(takeLeftArtThreadTask));
            takeRightArmThread.IsBackground = true;
            takeLeftArmThread.IsBackground = true;

            takeRightArmRunning = true;
            takeLeftArmRunning = true;


            distBetweenObjs = (objToLeft.Position - objToRight.Position).Magnitude;

            TBWriter.Info3("Starting takeRightArm (" + takeRightObject.Name + ")"); 
            takeRightArmThread.Start();
            while ((takeRightArmRunning))
                Thread.Sleep(1000);
            TBWriter.Info4("Finished takeRightArm");

            //if (distBetweenObjs < minDistBetweenObjs)
            //{
            //    while ((takeRightArmRunning))
            //        Thread.Sleep(1000);
            //}
            
            TBWriter.Info3("Starting takeLeftArm (" + takeLeftObject.Name + ")"); 
            takeLeftArmThread.Start();
            while ((takeLeftArmRunning))
                Thread.Sleep(1000);
            TBWriter.Info4("Finished takeLeftArm");
            
            //while ((takeRightArmRunning) && (takeLeftArmRunning))
            //{
            //    Thread.Sleep(100);
            //}

            if (takeLeftArmThread.IsAlive)
                takeLeftArmThread.Abort();

            if (takeRightArmThread.IsAlive)
                takeRightArmThread.Abort();

            objectsTaked = objToLeft.Name + " " + objToRight.Name;

            //verifyObjectsOnTable(out objectsTaked);

            //if (!objectsTaked.Contains(objToRight.Name))
            //{
            //    objectsTaked = objToRight.Name + " ";
            //    robot.RightArmIsEmpty = false;
            //    rightArmObject = objToRight.Name;
            //    TBWriter.Info1("Taked " + objToRight.Name + " with rightArm, now is not empty");
            //}

            //if (!objectsTaked.Contains(objToLeft.Name))
            //{
            //    objectsTaked = objToLeft.Name + " ";
            //    robot.LeftArmIsEmpty = false;
            //    leftArmObject = objToLeft.Name;
            //    TBWriter.Info1("Taked " + objToLeft.Name + " with lefttArm, now is not empty");
            //}

            TBWriter.Info1("	Finishing TakeBothArms	<---");
            return true;
        }

        private void takeRightArmThreadTask()
        {
            string objTaken = "";

            takeRightArmRunning = true;

            takeRightArm(takeRightObject, out objTaken);

            takeRightArmRunning = false;
        }

        private void takeLeftArtThreadTask()
        {
            string objTaken = "";

            takeLeftArmRunning = true;

            takeLeftArm(takeLeftObject, out objTaken);

            takeLeftArmRunning = false;
        }

        public bool takeRightArm(WorldObject objectToTake, out string takedObj)
        {
            WorldObject obj = new WorldObject(objectToTake.Name, objectToTake.Position, objectToTake.DistanceFromTable);
            TBWriter.Info1("    === Starting takeRightArm , trying to take " + obj.Name);

            takedObj = "empty empty";

            #region Validating Connected Modules

            if (!robot.modules[Module.Arms].IsConnected)
            {
                TBWriter.Error("Can't execute takeRightArm, Module [ Arms ] is not connected");
                return false;
            }
            if (obj == null)
            {
                TBWriter.Error("Can't execute takeRightArm, object is null");
                return false;
            }

            #endregion

            takedObj = "empty " + obj.Name;

            bool useVisionFeedBack = true;
            bool useCloseStart = true;

            double distIncrement = takeDistanceIncrement;
            double distanceToTake = 0.04;

            double distanceFromTableToTake = 0.06;
            double distanceToOpen = 0.20;
            int maxSteps = 40;
            int steps = 0;

            Vector3 armPos;
            Vector3 errorVec;
            Vector3 nextPos;
            Vector3 nextPosInArms;

            Vector3 findArmPos;
            Vector3 findArmPosInRobot;

            double currentPitch;
            double nextPitch;
            double errorDis;
            double errorAng;

            //ActualizeTorsoPos();
            ActualizeHeadPos();


            TBWriter.Info2("takeRightArm : Sending RightArm to navigation");
            cmdMan.ARMS_ra_goto("navigation", 7000);
            currentPitch = 0;

            #region Cheking if is empty

            //if (!robot.RightArmIsEmpty)
            //{
            //    TBWriter.Warning1("RightArm is not empty, droping " + rightArmObject);
            //    cmdMan.ARMS_ra_opengrip(100, 3000);
            //    robot.RightArmIsEmpty = true;
            //    rightArmObject = "";
            //    cmdMan.ARMS_ra_opengrip(20, 3000);
            //}

            #endregion

            // Always take objects at 8cm from table
            obj.Position.Z += -obj.DistanceFromTable + distanceFromTableToTake;

            if (useCloseStart)
            {
                ActualizeRightArmPosition();
                armPos = new Vector3(robot.RightArmPosition);

                errorVec = obj.Position - armPos;
                errorAng = Math.Atan2(errorVec.Y, errorVec.X);
                errorDis = errorVec.Magnitude;

                nextPos = obj.Position - errorVec.Unitary * distanceToOpen;
                nextPitch = errorAng;

				// FIXING HIGH TABLES FOR NEOJUSTINA
				nextPos.Z += 0.1;

                nextPosInArms = robot.TransRobot2RightArm(nextPos);
                TBWriter.Write(5,"takeRightArm : CloseStart, sending RightArm to (" + nextPosInArms.ToString() 
                                                                                  + "(r=" + MathUtil.PiOver2.ToString("0.00") + " , " 
                                                                                  +  "p=" + nextPitch.ToString("0.00") +" , " 
                                                                                  +  "y=" + "0.00" + " , " 
                                                                                  +  "e=" + (-1*nextPitch).ToString("0.00") + ")"
                                                                                  + " [ArmsCoor]");
                
                if( !cmdMan.ARMS_ra_abspos(nextPosInArms, MathUtil.PiOver2, nextPitch, 0, -nextPitch, 9000) )
                    TBWriter.Error("Cant use closeStart, ARMS_ra_abspos returned false");
                
                currentPitch = nextPitch;
                cmdMan.ARMS_ra_opengrip(60, 3000);
            }

            ActualizeRightArmPosition();
            armPos = new Vector3(robot.RightArmPosition);
            errorVec = obj.Position - armPos;
            errorAng = Math.Atan2(errorVec.Y, errorVec.X);
            errorDis = errorVec.Magnitude;
 
            TBWriter.Write(5,"takeRightArm : (arm feedback)");
			TBWriter.Write(5,"   armPosit=(" + armPos.ToString() + ") [robotCoor]");
            TBWriter.Write(7,"   errorVec=(" + errorVec.X.ToString("0.00") + errorVec.Y.ToString("0.00") + errorVec.Z.ToString("0.00") + ")");
            TBWriter.Write(7,"   errorAng=" + errorAng.ToString("0.00") );
            TBWriter.Write(7,"   errorDis=" + errorDis.ToString("0.00") );

            nextPos = armPos + errorVec.Unitary * distIncrement;
            nextPitch = errorAng;

            while ((errorDis > distanceToTake) && (steps < maxSteps))
            {
                nextPosInArms = robot.TransRobot2RightArm(nextPos);
                TBWriter.Write(5,"takeRightArm : (Step="+steps.ToString() +")");
                TBWriter.Write(5,"takeRightArm : Sending RightArm to (" + nextPosInArms.ToString()
                                                                      + "(r=" +MathUtil.PiOver2.ToString("0.00") + " , " 
                                                                      + "p=" + nextPitch.ToString("0.00") +" , " 
                                                                      + "y=" + "0.00" + " , " 
                                                                      + "e=" + (-1*nextPitch).ToString("0.00") + ")"
                                                                      + " [ArmsCoor]");

               if( !cmdMan.ARMS_ra_abspos(nextPosInArms, MathUtil.PiOver2, nextPitch, 0, -nextPitch, 7000) )
                   TBWriter.Error("takeRightArm : Cant get close in Step=" + steps + " , ARMS_ra_abspos returned false");

                ActualizeRightArmPosition();

                armPos = new Vector3(robot.RightArmPosition);
                errorVec = obj.Position - armPos;
                errorAng = Math.Atan2(errorVec.Y, errorVec.X);
                errorDis = errorVec.Magnitude;

                TBWriter.Write(5, "takeRightArm : (arm feedback)");
				TBWriter.Write(5, "   armPosit=(" + armPos.ToString() + ") [robotCoor]");
                TBWriter.Write(7, "   errorVec=(" + errorVec.X.ToString("0.00") + errorVec.Y.ToString("0.00") + errorVec.Z.ToString("0.00") + ")");
                TBWriter.Write(7, "   errorAng=" + errorAng.ToString("0.00") );
				TBWriter.Write(7, "   errorDis=" + errorDis.ToString("0.00"));

                if ((useVisionFeedBack) && (cmdMan.OBJ_FND_findRightArm(1000, out findArmPos)))
                {
                    findArmPosInRobot = robot.TransHeadKinect2Robot(findArmPos);
                    double visionError = (obj.Position - findArmPosInRobot).Magnitude;
                   
                    if (visionError < distanceToOpen)
                    {
                        errorVec = obj.Position - findArmPosInRobot;
                        errorAng = Math.Atan2(errorVec.Y, errorVec.X);
                        errorDis = errorVec.Magnitude;

						TBWriter.Write(5, "takeRightArm : (vision feedback)");
						TBWriter.Write(5, "   armPosit=(" + findArmPosInRobot.ToString() + ") [robotCoor]");
						TBWriter.Write(7, "   errorVec=(" + errorVec.X.ToString("0.00") + errorVec.Y.ToString("0.00") + errorVec.Z.ToString("0.00") + ")");
						TBWriter.Write(7, "   errorAng=" + errorAng.ToString("0.00"));
						TBWriter.Write(7, "   errorDis=" + errorDis.ToString("0.00"));
                    }
                }

                nextPos = armPos + errorVec.Unitary * distIncrement;
                nextPitch = errorAng;
                steps++;
            }

            if (steps >= maxSteps)
                TBWriter.Warning1(" >>> takeRightArm : Max Steeps Reached");

            TBWriter.Info3("takeRightArm : Closing gripper");
            cmdMan.ARMS_ra_closegrip(4000);

            ActualizeRightArmPosition();
            armPos = new Vector3(robot.RightArmPosition);
            nextPos = armPos + new Vector3(0, 0, 0.15);

            Vector3 finalRightPos = robot.TransRobot2RightArm(nextPos);
            finalRightPos.Z = 0.0;

            TBWriter.Info3("takeRightArm : FINALPOS: Lift Obj. Sending RightArm to (" + finalRightPos.ToString()
                                                                                      + "(r=" +MathUtil.PiOver2.ToString("00.00") + " , " 
                                                                                      + "p=" + "0.00" +" , " 
                                                                                      + "y=" + "0.00" + " , " 
                                                                                      + "e=" + "0.00" + ")"
                                                                                      + " [ArmsCoor]");
           
            cmdMan.ARMS_ra_abspos(finalRightPos, MathUtil.PiOver2, 0.0, 0, 0.0, 7000);
            cmdMan.ARMS_ra_goto("navigation", 10000);

            TBWriter.Info1("    === takeLeftArm : Finishing take object with RightArm");
            return true;
        }

        public bool takeLeftArm(WorldObject objectToTake, out string takedObj)
        {
            WorldObject obj = new WorldObject(objectToTake.Name, objectToTake.Position, objectToTake.DistanceFromTable);
            TBWriter.Info1("    === Starting takeLeftArm , trying to take " + obj.Name);

            takedObj = "empty empty";

            #region Validating Connected Modules

            if (!robot.modules[Module.Arms].IsConnected)
            {
                TBWriter.Error("Can't execute takeObj_LeftArm, Module [ Arms ] is not connected");
                return false;
            }
            if (obj == null)
            {
                TBWriter.Error("Can't execute takeObj_LeftArm, object is null");
                return false;
            }

            #endregion

            takedObj = obj.Name + " empty";

            bool useVisionFeedBack = true;
            bool useCloseStart = true;

            double distIncrement = takeDistanceIncrement;
            double distanceToTake = 0.04;

            double distanceFromTableToTake = 0.06;
            double distanceToOpen = 0.20;
            int maxSteps = 40;
            int steps = 0;

            Vector3 armPos;
            Vector3 errorVec;
            Vector3 nextPos;
            Vector3 nextPosInArms;

            Vector3 findArmPos;
            Vector3 findArmPosInRobot;

            double currentPitch;
            double nextPitch;
            double errorDis;
            double errorAng;

            // ActualizeTorsoPos();
            ActualizeHeadPos();

            TBWriter.Info2("takeLeftArm : Sending LeftArm to navigation");
            cmdMan.ARMS_la_goto("navigation", 7000);
            currentPitch = 0;

            #region Cheking if is empty

            //if (!robot.RightArmIsEmpty)
            //{
            //    TBWriter.Warning1("LeftArm is not empty, droping " + rightArmObject);
            //    cmdMan.ARMS_la_opengrip(100, 3000);
            //    robot.LeftArmIsEmpty = true;
            //    leftArmObject = "";
            //    cmdMan.ARMS_la_opengrip(20, 3000);
            //}

            #endregion

            // Always take objects at 8cm from table
            obj.Position.Z += -obj.DistanceFromTable + distanceFromTableToTake;

            if (useCloseStart)
            {
                ActualizeLeftArmPosition();
                armPos = new Vector3(robot.LeftArmPosition);

                errorVec = obj.Position - armPos;
                errorAng = Math.Atan2(errorVec.Y, errorVec.X);
                errorDis = errorVec.Magnitude;

                nextPos = obj.Position - errorVec.Unitary * distanceToOpen;
                nextPitch = errorAng;

				// FIXING HIGH TABLES FOR NEOJUSTINA
				nextPos.Z += 0.1;

                nextPosInArms = robot.TransRobot2LeftArm(nextPos);
                TBWriter.Write(5,"takeLeftArm : CloseStart, sending LeftArm to (" + nextPosInArms.ToString()
                                                                                + "(r=" + MathUtil.PiOver2.ToString("0.00") + " , "
                                                                                + "p=" + nextPitch.ToString("0.00") + " , "
                                                                                + "y=" + "0.00" + " , "
                                                                                + "e=" + (-1 * nextPitch).ToString("0.00") + ")"
                                                                                + " [ArmsCoor]");

				

               if(! cmdMan.ARMS_la_abspos(nextPosInArms, MathUtil.PiOver2, nextPitch, 0, -nextPitch, 9000) )
                   TBWriter.Error("Cant use closeStart, ARMS_la_abspos returned false");

                currentPitch = nextPitch;
                cmdMan.ARMS_la_opengrip(60, 3000);
            }

            ActualizeLeftArmPosition();
            armPos = new Vector3(robot.LeftArmPosition);
            errorVec = obj.Position - armPos;
            errorAng = Math.Atan2(errorVec.Y, errorVec.X);
            errorDis = errorVec.Magnitude;

            TBWriter.Write(5,"takeLeftArm : (arm feedback)");
			TBWriter.Write(5,"    armPosit=(" + armPos.ToString() + ") [robotCoor]");
            TBWriter.Write(7,"    errorVec=(" + errorVec.X.ToString("0.00") + errorVec.Y.ToString("0.00") + errorVec.Z.ToString("0.00") + ")");
            TBWriter.Write(7,"    errorAng=" + errorAng.ToString("0.00"));
            TBWriter.Write(7,"    errorDis=" + errorDis.ToString("0.00"));

            nextPos = armPos + errorVec.Unitary * distIncrement;
			nextPitch = errorAng;

            while ((errorDis > distanceToTake) && (steps < maxSteps))
            {
                nextPosInArms = robot.TransRobot2LeftArm(nextPos);
                TBWriter.Write(5,"takeLeftArm : (Step=" + steps.ToString() + ")");
                TBWriter.Write(5,"takeLeftArm : Sending LeftArm to (" + nextPosInArms.ToString()
                                                                    + "(r=" + MathUtil.PiOver2.ToString("0.00") + " , "
                                                                    + "p=" + nextPitch.ToString("0.00") + " , "
                                                                    + "y=" + "0.00" + " , "
                                                                    + "e=" + (-1 * nextPitch).ToString("0.00") + ")"
                                                                    + " [ArmsCoor]");

                if(cmdMan.ARMS_la_abspos(nextPosInArms, MathUtil.PiOver2, nextPitch, 0, -nextPitch, 7000))
                    TBWriter.Error("takeLeftArm : Cant get close in Step=" + steps + " , ARMS_la_abspos returned false");

                ActualizeLeftArmPosition();
                
                armPos = new Vector3(robot.LeftArmPosition);
                errorVec = obj.Position - armPos;
                errorAng = Math.Atan2(errorVec.Y, errorVec.X);
                errorDis = errorVec.Magnitude;

                TBWriter.Write(5,"takeLeftArm : (arm feedback)");
				TBWriter.Write(5,"   armPosit=(" + armPos.ToString() + ") [robotCoor]");
                TBWriter.Write(7,"   errorVec=(" + errorVec.X.ToString("0.00") + errorVec.Y.ToString("0.00") + errorVec.Z.ToString("0.00") + ")");
                TBWriter.Write(7,"   errorAng=" + errorAng.ToString("0.00"));
                TBWriter.Write(7,"   errorDis=" + errorDis.ToString("0.00"));

                if ((useVisionFeedBack) && (cmdMan.OBJ_FND_findLeftArm(1000, out findArmPos)))
                {
                    findArmPosInRobot = robot.TransHeadKinect2Robot(findArmPos);
                    double visionError = (obj.Position - findArmPosInRobot).Magnitude;

                    if (visionError < distanceToOpen)
                    {
                        errorVec = obj.Position - findArmPosInRobot;
                        errorAng = Math.Atan2(errorVec.Y, errorVec.X);
                        errorDis = errorVec.Magnitude;

						TBWriter.Write(5, "takeLefttArm : (vision feedback)");
						TBWriter.Write(5, "   armPosit=(" + findArmPosInRobot.ToString() + ") [robotCoor]");
                        TBWriter.Write(7, "   errorVec=(" + errorVec.X.ToString("0.00") + errorVec.Y.ToString("0.00") + errorVec.Z.ToString("0.00") + ")");
                        TBWriter.Write(7, "   errorAng=" + errorAng.ToString("0.00"));
                        TBWriter.Write(7, "   errorDis=" + errorDis.ToString("0.00"));
                    }
                }

                nextPos = armPos + errorVec.Unitary * distIncrement;
				nextPitch = errorAng;
                steps++;
            }

            if (steps >= maxSteps)
                TBWriter.Warning1(" >>> takeLeftArm : Max Steeps Reached");

            TBWriter.Info3("takeLeftArm : Closing gripper");
            cmdMan.ARMS_la_closegrip(4000);

            ActualizeLeftArmPosition();
            armPos = new Vector3(robot.LeftArmPosition);
            nextPos = armPos + new Vector3(0, 0, 0.15);
            Vector3 finalLeftPos = robot.TransRobot2LeftArm(nextPos);
            finalLeftPos.Z = 0.0;

            TBWriter.Info3("takeLeftArm : FINALPOS: Lift Obj. Sending RightArm to (" + finalLeftPos.ToString()
                                                                                     + "(r=" + MathUtil.PiOver2.ToString("00.00") + " , "
                                                                                     + "p=" + "0.00" + " , "
                                                                                     + "y=" + "0.00" + " , "
                                                                                     + "e=" + "0.00" + ")"
                                                                                     + " [ArmsCoor]");
           
            cmdMan.ARMS_la_abspos(finalLeftPos, MathUtil.PiOver2, 0.0, 0, 0.0, 7000);
            cmdMan.ARMS_la_goto("navigation", 10000);

            TBWriter.Info1("    === takeLeftArm : Finishing take object with LeftArm");
            return true;
        }

		public bool _OLD_takeBothArms(WorldObject objToRight, WorldObject objToLeft, out string objectsTaked)
		{
			TBWriter.Info1("	--->	Starting TakeBothArms");

			objectsTaked = "";


            // if cant see both arms at same time
            //takeRightArm(objToRight);
            //takeLeftArm(objToLeft);


            takeRightObject = new WorldObject(objToRight);
            takeLeftObject = new WorldObject(objToLeft);

            takeRightArmThread = new Thread(new ThreadStart(takeRightArmThreadTask));
            takeLeftArmThread = new Thread(new ThreadStart(takeLeftArtThreadTask));
            takeRightArmThread.IsBackground = true;
            takeLeftArmThread.IsBackground = true;

            takeRightArmRunning = true;
            takeLeftArmRunning = true;

            takeRightArmThread.Start();
            takeLeftArmThread.Start();

            while ((takeRightArmRunning) || (takeLeftArmRunning))
            {
                Thread.Sleep(100);
            }

            if (takeLeftArmThread.IsAlive)
                takeLeftArmThread.Abort();

            if (takeRightArmThread.IsAlive)
                takeRightArmThread.Abort();

            
            ActualizeHeadPos();

            verifyObjectsOnTable(out objectsTaked);

			if (!objectsTaked.Contains(objToRight.Name))
			{
				objectsTaked = objToRight.Name + " ";
				robot.RightArmIsEmpty = false;
				rightArmObject = objToRight.Name;
				TBWriter.Info1("Taked " + objToRight.Name + " with rightArm, now is not empty");
			}

			if (!objectsTaked.Contains(objToLeft.Name))
			{
				objectsTaked = objToLeft.Name + " ";
				robot.LeftArmIsEmpty = false;
				leftArmObject = objToLeft.Name;
                TBWriter.Info1("Taked " + objToLeft.Name + " with lefttArm, now is not empty");
			}

			TBWriter.Info1("	Finishing TakeBothArms	<---");
			return true;
		}

        private void _OLD_takeRightArmThreadTask()
		{
			takeRightArmRunning = true;

			takeRightArm(takeRightObject);

			takeLeftArmRunning = false;
		}

        private void _OLD_takeLeftArtThreadTask()
		{
			takeRightArmRunning = true;

			takeLeftArm(takeLeftObject);

			takeRightArmRunning = false;
		}

        public bool takeRightArm(WorldObject obj)
        {
            TBWriter.Info1("    === Starting takeRightArm , trying to take " + obj.Name);

            #region Validating

            if (!robot.modules[Module.Arms].IsConnected)
            {
                TBWriter.Error("Can't execute takeObj_RightArm, Module [ Arms ] is not connected");
                return false;
            }
            if (obj == null)
            {
				TBWriter.Error("Can't execute takeObj_RightArm, object is null");
                return false;
            }

            #endregion

            bool useVisionFeedBack = true;
            bool useCloseStart = true;

            double distIncrement = 0.02;
            double angleIncrement = 0.2;

            double distanceToTake = 0.08;
            double distanceToOpen = 0.20;
            double distanceFromTableToTake = 0.08;
            int maxSteps = 40;
            int steps = 0;

            Vector3 armPos;
            Vector3 errorVec;
            Vector3 incrementPos;
            Vector3 nextPos;
            Vector3 nextError;
            Vector3 currentError;
            Vector3 nextPosInArms;

            Vector3 findArmPos;
            Vector3 findArmPosInRobot;

            Vector3 closeStartPos;
            Vector3 closeStartPosArm;

            double lastPitch;
            double currentPitch;
            double nextPitch;
            double incrementPitch;
            double errorDis;
            double errorAng;


            //ActualizeTorsoPos();
            ActualizeHeadPos();


            TBWriter.Info2("Sending RightArm to navigation");
            cmdMan.ARMS_ra_goto("navigation", 7000);
            currentPitch = 0;

            #region Cheking if is empty

            if (!robot.RightArmIsEmpty)
            {
                TBWriter.Warning1("RightArm is not empty, droping " + rightArmObject);
                cmdMan.ARMS_ra_opengrip(100, 3000);
                robot.RightArmIsEmpty = true;
                rightArmObject = "";
                cmdMan.ARMS_ra_opengrip(20, 3000);
            }

            #endregion

            // Always take objects at 8cm from table
            obj.Position.Z += -obj.DistanceFromTable + distanceFromTableToTake;

            if (useCloseStart)
            {
                ActualizeRightArmPosition();
                armPos = new Vector3(robot.RightArmPosition);

                errorVec = obj.Position - armPos;
                errorAng = Math.Atan2(errorVec.Y, errorVec.X);
                errorDis = errorVec.Magnitude;

                nextPos = obj.Position - errorVec.Unitary * distanceToOpen;
                nextPitch = errorAng;

                TBWriter.Info3("Using CloseStart, sending RightArm to " + nextPos.ToString());

                nextPosInArms = robot.TransRobot2RightArm(nextPos);
                cmdMan.ARMS_ra_abspos(nextPosInArms, MathUtil.PiOver2, nextPitch, 0, -nextPitch, 9000);

                currentPitch = nextPitch;

                cmdMan.ARMS_ra_opengrip(80, 3000);
            }

            ActualizeRightArmPosition();
            armPos = new Vector3(robot.RightArmPosition);
            errorVec = obj.Position - armPos;
            errorAng = Math.Atan2(errorVec.Y, errorVec.X);

            errorDis = errorVec.Magnitude;

            nextPos = armPos + errorVec.Unitary * distIncrement;
            nextPitch = errorAng;



            while ((errorDis > distanceToTake) && (steps < maxSteps))
            {

                TBWriter.Info4("Distance error " + errorDis.ToString("0.00") + " in step " + steps.ToString());

                nextPosInArms = robot.TransRobot2RightArm(nextPos);

                cmdMan.ARMS_ra_abspos(nextPosInArms, MathUtil.PiOver2, nextPitch, 0, -nextPitch, 7000);


                ActualizeRightArmPosition();
                armPos = new Vector3(robot.RightArmPosition);

                TBWriter.Info4("ARMS RightArm Pos = " + armPos.X.ToString("0.00") + " " + armPos.Y.ToString("0.00") + " " + armPos.Z.ToString("0.00"));
                errorVec = obj.Position - armPos;
                errorAng = Math.Atan2(errorVec.Y, errorVec.X);
                errorDis = errorVec.Magnitude;

                if ((useVisionFeedBack) && (cmdMan.OBJ_FND_findRightArm(1000, out findArmPos)))
                {
                    findArmPosInRobot = robot.TransHeadKinect2Robot(findArmPos);

                    double visionError = (obj.Position - findArmPosInRobot).Magnitude;

                    if (visionError < distanceToOpen)
                    {
                        errorVec = obj.Position - findArmPosInRobot;
                        errorAng = Math.Atan2(errorVec.Y, errorVec.X);
                        errorDis = errorVec.Magnitude;
                    }
                }


                nextPos = armPos + errorVec.Unitary * distIncrement;
                nextPitch = errorAng;

                steps++;
            }

            if (steps >= maxSteps)
                TBWriter.Warning1(" >>> Max Steeps Reached");

            //cmdMan.ARMS_ra_opengrip(10, 4000);
            cmdMan.ARMS_ra_closegrip(4000);

            ActualizeRightArmPosition();
            armPos = new Vector3(robot.RightArmPosition);
            nextPos = armPos + new Vector3(0, 0, 0.10);

            // pa que lo levante mas fheeeee (mas aca) 
            Vector3 newArmPOs = armPos + new Vector3(-0.15, 0, 0.15);

            //if (!cmdMan.ARMS_ra_abspos(robot.TransRobot2RightArm(newArmPOs), MathUtil.PiOver2, nextPitch, 0, -nextPitch, 7000))
            cmdMan.ARMS_ra_abspos(robot.TransRobot2RightArm(nextPos), MathUtil.PiOver2, nextPitch, 0, -nextPitch, 7000);

            cmdMan.ARMS_ra_goto("navigation", 7000);

            TBWriter.Info1("    === Finishing take object with RightArm");
            return true;
        }

        public bool takeLeftArm(WorldObject obj)
        {
            TBWriter.Info1("    === Starting takeRightArm , trying to take " + obj.Name);

            #region Validating

            if (!robot.modules[Module.Arms].IsConnected)
            {
                TBWriter.Error("Can't execute takeObj_RightArm, Module [ Arms ] is not connected");
                return false;
            }
            if (obj == null)
            {
                TBWriter.Error("Can't execute takeObj_LeftArm, object is null");
                return false;
            }

            #endregion

            bool useVisionFeedBack = false;
            bool useCloseStart = true;

            double distIncrement = 0.02;
            double distanceToTake = 0.08;
            double distanceFromTableToTake = 0.8;
            double distanceToOpen = 0.2;
            int maxSteps = 35;
            int steps = 0;

            Vector3 armPos;
            Vector3 errorVec;
            Vector3 nextPos;
            Vector3 nextPosInArms;

            Vector3 findArmPos;
            Vector3 findArmPosInRobot;

            double currentPitch;
            double nextPitch;
            double errorDis;
            double errorAng;

            ActualizeTorsoPos();
            ActualizeHeadPos();

            TBWriter.Info2("Sending LeftArm to navigation");
            cmdMan.ARMS_la_goto("navigation", 7000);
            currentPitch = 0;

            #region Cheking if is empty

            if (!robot.RightArmIsEmpty)
            {
                TBWriter.Warning1("LeftArm is not empty, droping " + rightArmObject);
                cmdMan.ARMS_la_opengrip(100, 3000);
                robot.LeftArmIsEmpty = true;
                leftArmObject = "";
                cmdMan.ARMS_la_opengrip(20, 3000);
            }

            #endregion


            // Always take objects at 8cm from table
            obj.Position.Z += -obj.DistanceFromTable + distanceFromTableToTake;

            if (useCloseStart)
            {
                ActualizeLeftArmPosition();
                armPos = new Vector3(robot.LeftArmPosition);

                errorVec = obj.Position - armPos;
                errorAng = Math.Atan2(errorVec.Y, errorVec.X);
                errorDis = errorVec.Magnitude;

                nextPos = obj.Position - errorVec.Unitary * distanceToOpen;
                nextPitch = errorAng;

                TBWriter.Info3("LeftArm : using CloseStart, sending LeftArm to " + nextPos.ToString());

                nextPosInArms = robot.TransRobot2LeftArm(nextPos);
                cmdMan.ARMS_la_abspos(nextPosInArms, MathUtil.PiOver2, nextPitch, 0, -nextPitch, 9000);

                currentPitch = nextPitch;

                cmdMan.ARMS_la_opengrip(80, 3000);
            }

            ActualizeLeftArmPosition();
            armPos = new Vector3(robot.LeftArmPosition);
            errorVec = obj.Position - armPos;
            errorAng = Math.Atan2(errorVec.Y, errorVec.X);

            errorDis = errorVec.Magnitude;

            nextPos = armPos + errorVec.Unitary * distIncrement;
            nextPitch = errorAng;


            while ((errorDis > distanceToTake) && (steps < maxSteps))
            {
                TBWriter.Info4("LeftArm : errorDist " + errorDis.ToString("0.00") + " in step " + steps.ToString());

                nextPosInArms = robot.TransRobot2LeftArm(nextPos);

                cmdMan.ARMS_la_abspos(nextPosInArms, MathUtil.PiOver2, nextPitch, 0, -nextPitch, 7000);

                ActualizeLeftArmPosition();
                armPos = new Vector3(robot.LeftArmPosition);

                TBWriter.Info4("LeftArm : ARMS Pos = " + armPos.X.ToString("0.00") + " " + armPos.Y.ToString("0.00") + " " + armPos.Z.ToString("0.00"));
                errorVec = obj.Position - armPos;
                errorAng = Math.Atan2(errorVec.Y, errorVec.X);
                errorDis = errorVec.Magnitude;


                if ((useVisionFeedBack) && (cmdMan.OBJ_FND_findLeftArm(500, out findArmPos)))
                {
                    findArmPosInRobot = robot.TransHeadKinect2Robot(findArmPos);

                    double visionError = (obj.Position - findArmPosInRobot).Magnitude;

                    if (visionError < distanceToOpen)
                    {
                        errorVec = obj.Position - findArmPosInRobot;
                        errorAng = Math.Atan2(errorVec.Y, errorVec.X);
                        errorDis = errorVec.Magnitude;
                    }
                }


                nextPos = armPos + errorVec.Unitary * distIncrement;
                nextPitch = errorAng;

                steps++;
            }

            if (steps >= maxSteps)
                TBWriter.Warning1(" >>> LeftArm : Max Steeps Reached");


            cmdMan.ARMS_la_closegrip(4000);

            ActualizeLeftArmPosition();
            armPos = new Vector3(robot.LeftArmPosition);
            nextPos = armPos + new Vector3(0, 0, 0.10);

            cmdMan.ARMS_la_abspos(robot.TransRobot2LeftArm(nextPos), MathUtil.PiOver2, nextPitch, 0, -nextPitch, 7000);
            cmdMan.ARMS_la_goto("navigation", 7000);

            TBWriter.Info1("    === LeftArm : Finishing take object with LeftArm");
            return true;
        }

		private bool findObject(string objToFind, out WorldObject objectFounded)
		{
			TBWriter.Info2("Starting Find object");

			ActualizeHeadPos();
			ActualizeTorsoPos();

			objectFounded = null;

			if (!cmdMan.OBJ_FND_findontable(robot.head.Pan, 120000))
				return false;

			string objectsInfo = cmdMan.JustinaCmdAndResps[(int)JustinaCommands.OBJ_FND_findOnTable].Response.Parameters;

			WorldObject[] objectArray;

			if (!Parse.FindObjectOnTableInfo(objectsInfo, out objectArray))
				return false;

			if ((objectArray == null) && (objectArray.Length == 0))
				return false;

			foreach (WorldObject obj in objectArray)
			{
				if (objToFind == obj.Name)
				{
					 objectFounded= new WorldObject(obj.Name, robot.TransHeadKinect2Robot(obj.Position), obj.DistanceFromTable);
					return true;
				}
			}

			return false;
		}

        private bool verifyObjOnShelf(string objectToLookFor)
        {
            TBWriter.Info2("Starting verifyObjOnShelf");

            ActualizeHeadPos();

            return cmdMan.OBJ_FND_findObjectsOnshelf(objectToLookFor, robot.head.Pan, 35000);
        }

        private bool verifyObjOnShelf(out string objectsOnTable)
        {
            TBWriter.Info2("Starting verifyObjectsOnTable");

            objectsOnTable = "";

            ActualizeHeadPos();

            if (!cmdMan.OBJ_FND_findObjectsOnshelf(robot.head.Pan, 35000))
                return false;

            string objectsInfo = cmdMan.JustinaCmdAndResps[(int)JustinaCommands.OBJ_FND_findObjectsOnshelf].Response.Parameters;

            WorldObject[] objectArray;

            if (!Parse.FindObjectOnTableInfo(objectsInfo, out objectArray))
                return false;

            if (objectArray == null)
                return false;

            foreach (WorldObject obj in objectArray)
            {
                objectsOnTable = obj.Name + " ";
            }

            TBWriter.Info1("Founded on Table " + objectsOnTable);
            return true;
        }

		private bool verifyObjectsOnTable(out string objectsOnTable)
		{
			TBWriter.Info2("Starting verifyObjectsOnTable");

			 objectsOnTable = "";

             ActualizeHeadPos();

			if (!cmdMan.OBJ_FND_findontable(robot.head.Pan, 35000))
				return false;

			string objectsInfo = cmdMan.JustinaCmdAndResps[(int)JustinaCommands.OBJ_FND_findOnTable].Response.Parameters;

			WorldObject[] objectArray;

			if (!Parse.FindObjectOnTableInfo(objectsInfo, out objectArray))
				return false;

			if (objectArray == null)
				return false;

			foreach (WorldObject obj in objectArray)
			{
				objectsOnTable = obj.Name + " ";
			}

			TBWriter.Info1("Founded on Table " + objectsOnTable);
			return true;
		}

        private bool verifyObjectsOnTable(string objectToLookFor)
        {
            TBWriter.Info2("Starting verifyObjectsOnTable");

            ActualizeHeadPos();

            return cmdMan.OBJ_FND_findontable(objectToLookFor, robot.head.Pan, 35000);

            //if (!cmdMan.OBJ_FND_findontable( objectToLookFor, robot.head.Pan, 35000))
            //{
            //    TBWriter.Warning1("Can't find " + objectToLookFor + "on Table");
            //    return false;
            //}

            //string objectsInfo = cmdMan.JustinaCmdAndResps[(int)JustinaCommands.OBJ_FND_findOnTable].Response.Parameters;

            //WorldObject[] objectArray;

            //if (!Parse.FindObjectOnTableInfo(objectsInfo, out objectArray))
            //    return false;

            //if (objectArray == null)
            //    return false;

            //foreach (WorldObject obj in objectArray)
            //{
            //    if (objectToLookFor == obj.Name)
            //    {
            //        TBWriter.Info1(" Founded " + obj.Name + " on Table");
            //        return true;
            //    }
            //}

            //return false;
        }

		private bool getClose(Vector3 position)
		{
            TBWriter.Info3(" Starting getClose to but im not going to do that " + position.ToString());

            return false;

            if (!ActualizeMobilBasePos())
            {
                TBWriter.Warning1("Cant getClose, can't actualize MobilBasePosition" + position.ToString());
                return false;
            }
			Vector3 worldPos = new Vector3(Vector3.Zero);

			worldPos.X =position.X*MathUtil.Cos( robot.Orientation) - position.Y*MathUtil.Sin( robot.Orientation) + robot.Position.X;
			worldPos.Y =position.X*MathUtil.Sin( robot.Orientation) + position.Y*MathUtil.Cos( robot.Orientation) + robot.Position.Y;

			cmdMan.ARMS_la_goto(Arm.ppNavigation);

            if (!cmdMan.ARMS_ra_goto(Arm.ppNavigation, 7000))
            {
                TBWriter.Warning1("Cant getClose, RightArm can't goto Navigation " + position.ToString());
                return false;
            }
            if (!this.cmdMan.WaitForResponse(JustinaCommands.ARMS_la_goto, 5000))
            {
                TBWriter.Warning1("Cant getClose, LeftArm can't goto Navigation " + position.ToString());
                return false;
            }
            if (!cmdMan.TRS_abspos(TorsoHW.navigationElevation, TorsoHW.navigationPan, 10000))
            {
                TBWriter.Warning1("Cant getClose, torso can't goto Navigation " + position.ToString());
                return false;
            }
			if (!cmdMan.MVN_PLN_getclose(worldPos.X, worldPos.Y, 90000))
			{
				TBWriter.Warning2("Can't getClose, MotionPlanner to " + position.ToString());
				return false;
			}

			return true;
		}

        //private void autoLocalizationThreadTask()
        //{
        //    Vector3 lastPosition = new Vector3(double.MaxValue, double.MaxValue, double.MaxValue);

        //    double headTilt = 0;
        //    double torsoElevation = 0.7;

        //    while (autoLocalizationRunning)
        //    {
        //        if ((robot.Position == null) && (!ActualizeMobilBasePos()))
        //        {
        //            Thread.Sleep(3000);
        //        }
        //        else if (robot.Region == null)
        //        {
        //            Thread.Sleep(3000);
        //        }
        //        else if (!imageMap.ContainsRegion(robot.Region))
        //        {
        //            Thread.Sleep(3000);
        //        }
        //        else if (lastPosition == robot.Position)
        //        {
        //            Thread.Sleep(2000);
        //        }
        //        else
        //        {

        //            TBWriter.Info3("Robot position changed, searching for near nodes for AutoLocalization in Region [ " + robot.Region + " ]");
        //            lastPosition = robot.Position;

        //            string closestNode = null;
        //            double closestDistance = distanceThreashold;

        //            foreach (ImageMapNode node in imageMap[robot.Region].NodeList.Values)
        //            {
        //                double distanceToNode = (node.Position - robot.Position).Magnitude;

        //                if ((distanceToNode < closestDistance) && (!node.Verified))
        //                {
        //                    closestDistance = distanceToNode;
        //                    closestNode = node.NodeName;

        //                    TBWriter.Info4("Closest node , until now " + closestNode);
        //                }

        //                if ((node.Verified) && (distanceToNode > distanceThreashold)) node.Verified = false;
        //            }



        //            if (string.IsNullOrEmpty(closestNode))
        //                continue;
        //            TBWriter.Info3("Checking for closest heading in node [ " + closestNode + " ]");


        //            if (!cmdMan.MVN_PLN_pause(true, 300))
        //                continue;


        //            double closestHeadingID = double.NaN;
        //            double closestAngleError = robot.head.MaxPan + robot.torso.MaxPan;

        //            foreach (ImageMapHeading heading in imageMap[robot.Region].NodeList[closestNode].Headings)
        //            {
        //                double angleError = Math.Abs(heading.Value - robot.Orientation);

        //                if (angleError > closestAngleError) continue;

        //                closestAngleError = angleError;
        //                closestHeadingID = heading.ID;
        //            }



        //            if (double.IsNaN(closestHeadingID))
        //                continue;
        //            TBWriter.Info3("Founded closest heading, heading id [ " + ((int)closestHeadingID).ToString() + " ]");



        //            double[] panAngles;

        //            double angle = imageMap[robot.Region].NodeList[closestNode].Headings[(int)closestHeadingID].Value;

        //            if (!SetHeadOrientation(angle, out panAngles)) continue;

        //            cmdMan.HEAD_lookat(panAngles[0], headTilt);

        //            if (!cmdMan.MVN_PLN_pause(true, 300))
        //                continue;

        //            bool torsoSuccess = cmdMan.TRS_abspos(torsoElevation, panAngles[1], 10000);


        //            if (!cmdMan.MVN_PLN_pause(true, 300))
        //                continue;

        //            bool headSuccess = cmdMan.WaitForResponse(JustinaCommands.HEAD_lookat, 10000);


        //            if ((!headSuccess) || (!torsoSuccess)) continue;


        //            Vector3 errorPos;
        //            double errorOri;

        //            if (!cmdMan.OBJ_FND_getNode(closestNode, closestHeadingID.ToString(), 10000, out errorPos, out errorOri)) continue;


        //            if (!cmdMan.MVN_PLN_pause(true, 300))
        //                continue;

        //            if (errorPos == null)
        //            {
        //                TBWriter.Warning1("Vector errorPosition is null");
        //                continue;
        //            }

        //            TBWriter.Info1("Succesfully calculated localization error [ positionError=" + errorPos.ToString() + " , orientationError" + errorOri.ToString("0.00") + " ]");
        //        }
        //    }
        //}

		public bool NavigateTo(Vector3 position, double orientation)
		{
			TBWriter.Info3("Navigating to position[ " + position.ToString() + "] , orientation[" + orientation.ToString("0.00") + "]");

			if (!SetNavigationConfig())
			{
				TBWriter.Error("Can't navigateTo position : navigationConfig not set");
				return false;
			}

			TBWriter.Info3("Sending MVN_PLN_goto position");
			if (!cmdMan.MVN_PLN_goto(position, orientation, 120000))
			{
				TBWriter.Error("Can't navigateTo position : MVN_PLN_goto didnt reach the position");
				return false;
			}

			if (!SetStandbyConfig())
				TBWriter.Warning1("standbyConfig not set");


			TBWriter.Info3("Navigating to position : reached NavigateTo ...");
			return true;
		}

		public bool MoveTorso(double elevation, double pan)
		{

			if (elevation > 0.65)
				return cmdMan.TRS_abspos(elevation, pan, 10000);

			if (!ArmsGoTo(Arm.ppNavigation))
			{
				TBWriter.Error("Can't MoveTorso : Arms didn't reach secure position");
				return false;
			}

			return cmdMan.TRS_abspos(elevation, pan, 10000);
		}

		public bool ArmsGoTo( string leftArmPos, string rightArmPos)
		{
			bool success =true;

			cmdMan.ARMS_la_goto( leftArmPos);
			cmdMan.ARMS_ra_goto(rightArmPos);

			if (!cmdMan.WaitForResponse(JustinaCommands.ARMS_la_goto, 10000))
			{
				TBWriter.Error("LeftArm didn't reach PredefinePosition{" + leftArmPos + "}");
				success = false;
			}
			if( !cmdMan.WaitForResponse(JustinaCommands.ARMS_ra_goto, 3000))
			{
				TBWriter.Error("RightArm didn't reach PredefinePosition{" + leftArmPos + "}");
				success = false;
			}

			return success;
		}

		public bool ArmsGoTo(string armsPos)
		{
			return ArmsGoTo(armsPos, armsPos);
		}

        public bool SetNavigationConfig()
        {
            TBWriter.Info4("setting NavigationConfig...");

			string armsPos = Arm.ppNavigation;
			double torsoStepElevation = 0.5;
			double torsoFinalElevation = torsoElevationForNavigation;

			cmdMan.ARMS_la_goto(armsPos);
            cmdMan.ARMS_ra_goto(armsPos);

			if (!cmdMan.WaitForResponse(JustinaCommands.ARMS_la_goto, 10000))
            {
				TBWriter.Error("Can't set navigationConfig, LEFT arm didn't reach to NAVIGATION");
                return false;
            }
            if (!cmdMan.WaitForResponse(JustinaCommands.ARMS_ra_goto, 5000))
            {
				TBWriter.Error("Can't set navigationConfig, RIGHT arm didn't reach to NAVIGATION");
                return false;
            }

			if (!cmdMan.TRS_abspos(torsoStepElevation, 0.0, 10000))
            {
				TBWriter.Error("Can't set navigationConfig, TORSO didn't reach to STEP elevation");
                return false;
            }

            if (!cmdMan.TRS_abspos(torsoFinalElevation, 0.0, 10000))
            {
				TBWriter.Error("Can't set navigationConfig, TORSO didn't reach to FINAL elevation");
                return false;
            }

			TBWriter.Info4(" ... setted navigationConfig ");
			return true;
        }

		public bool SetStandbyConfig()
		{
			TBWriter.Info4("setting standbyConfig ...");

			string armsPos = Arm.ppStandBy;
			double torsoStepElevation = 0.5;
			double torsoFinalElevation = 0.75;

			if (!cmdMan.TRS_abspos(torsoStepElevation, 0.0, 10000))
			{
				TBWriter.Error("Can't set StandbyConfig: TORSO didn't reach to STEP elevation");
				return false;
			}
			if (!cmdMan.TRS_abspos(torsoFinalElevation, 0.0, 10000))
			{
				TBWriter.Error("Can't set standbyConfig: TORSO didn't reach to FINAL elevation");
				return false;
			}

			cmdMan.ARMS_la_goto(armsPos);
			cmdMan.ARMS_ra_goto(armsPos);

			if (!cmdMan.WaitForResponse(JustinaCommands.ARMS_la_goto, 10000))
			{
				TBWriter.Error("Can't set standbyConfig: LEFT arm didn't reach to {" + armsPos + "}");
				return false;
			}
			if (!cmdMan.WaitForResponse(JustinaCommands.ARMS_ra_goto, 5000))
			{
				TBWriter.Error("Can't set standbyConfig: RIGHT arm didn't reach to {" + armsPos + "}");
				return false;
			}

			TBWriter.Info4(" ... setted standbyConfig");
			return true;
		}

		public bool SetNormalConfig()
		{
			TBWriter.Info4("setting normalConfig ...");

			string armsPos = Arm.ppHome;
			double torsoStepElevation = 0.5;
			double torsoFinalElevation = 0.85;

			if (!cmdMan.TRS_abspos(torsoStepElevation, 0.0, 10000))
			{
				TBWriter.Error("Can't set normalConfig: TORSO didn't reach to STEP elevation");
				return false;
			}
			if (!cmdMan.TRS_abspos(torsoFinalElevation, 0.0, 10000))
			{
				TBWriter.Error("Can't set normalConfig: TORSO didn't reach to FINAL elevation");
				return false;
			}

			cmdMan.ARMS_la_goto(armsPos);
			cmdMan.ARMS_ra_goto(armsPos);

			if (!cmdMan.WaitForResponse(JustinaCommands.ARMS_la_goto, 10000))
			{
				TBWriter.Error("Can't set normalConfig: LEFT arm didn't reach to {" + armsPos + "}");
				return false;
			}
			if (!cmdMan.WaitForResponse(JustinaCommands.ARMS_ra_goto, 5000))
			{
				TBWriter.Error("Can't set normalConfig: RIGHT arm didn't reach to {" + armsPos + "}");
				return false;
			}

			TBWriter.Info4(" ... setted normalConfig");
			return true;
		}

		public bool findHumanWithKinect(string humanName, out Human[] humans)
		{
			TBWriter.Info2( "Looking for Human [ " + humanName + " ]  with Kinect");
			
			if (robot.Skeletons == null)
			{
				TBWriter.Warning1("Robot.skeletons is null or there's no skeletons to look for");
				humans = null;
				return false;		
			}

			if (robot.Skeletons.Length == 0)
			{
				TBWriter.Info1("There's no skeletons to verify");
				humans = new Human[0];
				return false;
			}
			
			TBWriter.Info3( "Verifiyng [ " + robot.Skeletons.Length.ToString() + " ] skeletons" );
				
			string name = humanName;

			if (humanName == "any") name = "human";
				
			humans = new Human[robot.Skeletons.Length];

			for (int i = 0; i < robot.Skeletons.Length; i++)
			{
				Human personFounded;
				Vector3	skeletonPos;
				
				// looking with head at skeleton head.

				Vector3 skelNeck =robot.TransRobot2Neck( robot.Skeletons[i].Head);

				cmdMan.HEAD_lookatobject(skelNeck, 5000);

				TBWriter.Info3("Verifying skeleton ID[ " + robot.Skeletons[i].ID.ToString() + " at [ " + robot.Skeletons[i].Head.ToString() + " ]");

				double humanPan;
				double humanTilt;

				if (LookForHuman( name, false , out personFounded, out humanPan, out humanTilt))
				{
					TBWriter.Info2("Founded " + name + " in Skeleton ID:" + robot.Skeletons[i].ID.ToString());
					humans[i] = new Human(personFounded.Name, personFounded.Head);

					cmdMan.HEAD_lookatobject( robot.TransRobot2Neck( humans[i].Head), 2000);

                    if (humanName != "any") break;
				}
			}

			return true;
		}

		public bool SetHeadOrientation(double absoluteAngle, out double[] panAngles)
		{
			double headTilt = 0;
			double torsoElevation = 0.7;

			panAngles = null;

			TBWriter.Info3("Begining SetHeadOrientation routine for angle: " + absoluteAngle.ToString());

			if ((robot.Orientation == null) && (!ActualizeMobilBasePos()))
				TBWriter.Warning1("SetHeadOrientation, robotOrientation is unknown");

			if ((robot.head.Pan == null) && (!ActualizeHeadPos()))
				TBWriter.Warning1("SetHeadOrientation, headPan is unknown");

			if ((robot.torso.Pan == null) && (!ActualizeTorsoPos()))
				TBWriter.Warning1("SetHeadOrientation, torsoPan is unknown");

			double angError = absoluteAngle - robot.Orientation - robot.torso.Pan - robot.head.Pan;

			double headPosRange = robot.Orientation + robot.torso.Pan + robot.head.MaxPan;
			double headNegRange = robot.Orientation + robot.torso.Pan + robot.head.MinPan;

			double robotPosRange = robot.Orientation + robot.torso.MaxPan + robot.head.MaxPan;
			double robotNegRange = robot.Orientation + robot.torso.MinPan + robot.head.MinPan;

			double headNewPan;
			double torsoNewPan;

			if (angError == 0)
			{
				TBWriter.Info2("Angle error is zero");

				headNewPan = 0;
				torsoNewPan = 0;

				panAngles = new double[2];

				panAngles[0] = headNewPan;
				panAngles[1] = torsoNewPan;

				return true;
			}

			if ((headNegRange < angError) && (angError < headPosRange))
			{
				TBWriter.Info2("Angle in range of head");

				headNewPan = angError;
				torsoNewPan = 0;

				panAngles = new double[2];

				panAngles[0] = headNewPan;
				panAngles[1] = torsoNewPan;

				return true;

				//bool success = cmdMan.HEAD_lookat(angError, 0, 3000);
			}

			if ((robotNegRange < angError) && (angError < robotPosRange))
			{
				TBWriter.Info2("Angle in range using head AND torso");

				headNewPan = (angError > 0) ? robot.head.MaxPan : robot.head.MinPan;
				torsoNewPan = angError - headNewPan;

				panAngles = new double[2];

				panAngles[0] = headNewPan;
				panAngles[1] = torsoNewPan;

				return true;

				//cmdMan.HEAD_lookat(headNewPan, headTilt);
				//torsoSuccess = cmdMan.TRS_abspos(torsoElevation, torsoNewPan, 8000);
				//headSuccess = cmdMan.WaitForResponse(JustinaCommands.HEAD_lookat, 5000);
				//return torsoSuccess && headSuccess;
			}

			TBWriter.Warning1("Angle out of range");

			return false;
		}

		private bool LookForHuman(string personName, bool usePFAuto, out Human personFounded, out double humanPan, out double humanTilt)
		{
			ActualizeHeadPos();

			humanPan = double.MaxValue;
			humanTilt = double.MaxValue;

			TBWriter.Info2("Starting LookForHuman " + personName);

			personFounded = null;

			if (personName == "any")
				personName = "human";

			if( usePFAuto)
			{
				TBWriter.Info3("Enable usePFauto, checking for not null person");

				if (this.personPFAutoDetected != null)
				{
					TBWriter.Info2("Person founded usng PRS_FND_find");

					personFounded = new Human(this.personPFAutoDetected.Name, this.personPFAutoDetected.Head); 

					TBWriter.Info1("Founded " + personFounded.Name.ToUpper() + " in " + personFounded.Head.ToString());
					this.personPFAutoDetected = null;
				}
				else
				{
					TBWriter.Info2("No Person founded usng PFauto");
				}
			}
			else
			{
				TBWriter.Info2("Disabled usePFAuto, looking for person with PRS_FND_find");

				if (this.cmdMan.PRS_FND_find(personName, 2000))
				{
					TBWriter.Info3("Person founded usng PRS_FND_find");

					string humanInfo;
					string humanName;
					Vector3 humanPosition;

					humanInfo = cmdMan.JustinaCmdAndResps[(int)JustinaCommands.PRS_FND_find].Response.Parameters;

					if (Parse.humanInfo(humanInfo, out humanName, out humanPosition, out humanPan, out humanTilt))
					{

						ActualizeHeadPos();

						personFounded = new Human(humanName, robot.TransMinoru2Robot(humanPosition));
						TBWriter.Info1("Founded " + personFounded.Name.ToUpper() + " in " + personFounded.ToString());
	
					}
					else
						return false;
				}
				else
				{
					TBWriter.Info2("No Person founded usng PRS_FND_find");
					return false;
				}
			}

			TBWriter.Info3("Finished LookForHuman " + personName);

			if (personFounded != null)
				return true;
			else
				return false;

		}

		private void sleep(int time_ms)
		{
			TBWriter.Info8("Sleeping " + time_ms.ToString() + " ms");
			Thread.Sleep(time_ms);
			TBWriter.Info8(" Sleeping finish");
		}		

		public bool ActualizeHeadPos()
		{
			double[] headAngles;

			if (cmdMan.HEAD_getAngles( 1000, out headAngles))
			{
				robot.head.ActualizeHeadPosition(headAngles);
				TBWriter.Write(9, "Actualized Head Status");
				return true;
			}
			else
			{
				TBWriter.Warning1("Can't actualize Head Status");
				return false;
			}
		}

		public bool ActualizeTorsoPos()
		{
			double[] torsoPosition;

			if (cmdMan.TRS_getPosition(1000, out torsoPosition))
			{
				robot.torso.ActualizeTorsoStatus(torsoPosition);
				TBWriter.Write(9, "Actualized Torso Status");
				return true;
			}
			else
			{
				TBWriter.Warning1("Can't actualize Torso Status");
				return false;
			}
		}

		public bool ActualizeMobilBasePos()
		{
			double[] basePosition;

			if (cmdMan.MVN_PLN_getPosition(500, out basePosition))
			{
				robot.hardwareMan.MobileBase.ActualizeOdometryPosition(basePosition);
				TBWriter.Write(9, "Actualized MobileBase Status");
				return true;
			}
			else
			{
				TBWriter.Warning1("Can't actualize MobilBase Status");
				return false;
			}
		}

        public bool ActualizeRightArmPosition()
        {
            string armInfo;

            if (!cmdMan.ARMS_ra_getabspos(1000, out armInfo))
                return false;

            if (!robot.hardwareMan.RightArm.ActualizePositionStatus(armInfo))
              return false;

            robot.RightArmPosition = robot.TransRightArm2Robot(robot.hardwareMan.RightArm.Position);
            return true;
        }

		public bool ActualizeLeftArmPosition()
		{
			string armInfo;

			if (!cmdMan.ARMS_la_getabspos(1000, out armInfo))
				return false;

			if (!robot.hardwareMan.LeftArm.ActualizePositionStatus(armInfo))
				return false;

			robot.LeftArmPosition = robot.TransLeftArm2Robot(robot.hardwareMan.LeftArm.Position);

			return true;
		}

		
		#endregion


		#region drop and take XYZ

        /*
		public bool dropXYZ_RIGHT(WorldObject objectToDrop)
		{
			Vector3 armPosInRobotCoor;
			Vector3 armPosInArmsCoor;

			Vector3 errorVec;
			double errorAng;

			double nextRoll = MathUtil.PiOver2;
			double nextPitch = 0.0;
			double nextYaw = 0.0;
			double nextElbow = 0.0;


			Vector3 aboveOffset = new Vector3(-0.10, 0.0, 0.1);
			double aboveRoll = MathUtil.PiOver2 + MathUtil.PiOver4;

			double gripperOpening = 45.0;

			Vector3 toDropOffset = new Vector3(-0.05, 0.0, 0.05);
			double toTakeRoll = MathUtil.PiOver2;

			WorldObject wObj = new WorldObject(objectToDrop.Name, objectToDrop.Position, objectToDrop.DistanceFromTable);

			Vector3 objInRobotCoor = wObj.Position;
			Vector3 objInArmsCoor = robot.TransRobot2RightArm(wObj.Position);

			TBWriter.Info1("=== Starting dropXYZ_RIGHT ===");
			TBWriter.Info1(" trying to drop {" + wObj.Name + "} in :");
			TBWriter.Info1("	(robotCoor) { " + objInRobotCoor.X.ToString("0.000") + " , " +
													objInRobotCoor.Y.ToString("0.000") + " , " +
													objInRobotCoor.Z.ToString("0.000") + " } ");
			TBWriter.Info1("	(armsCoor) { " + objInArmsCoor.X.ToString("0.000") + " , " +
													objInArmsCoor.Y.ToString("0.000") + " , " +
													objInArmsCoor.Z.ToString("0.000") + " } ");

			#region CALCULATING FINAL POSITION

			armPosInRobotCoor = robot.TransRightArm2Robot(Vector3.Zero);
			errorVec = wObj.Position - armPosInRobotCoor;
			errorAng = Math.Atan2(errorVec.Y, errorVec.X);

			//nextPitch = errorAng;

			// if torso colide ( Change for left and right )
			//if (errorAng < 0)
			//    nextElbow = -nextPitch;
			//else
			//    nextElbow = 0;

			bool isReachable = this.cmdMan.ARMS_ra_reachable(objInArmsCoor.X,
																objInArmsCoor.Y,
																objInArmsCoor.Z,
																MathUtil.PiOver2,
																nextPitch,
																0,
																nextElbow, 3000);

			TBWriter.Info1("dropXYZ_RIGHT : final position calculation (ArmsCoor) { X=" + objInArmsCoor.X.ToString("0.000") +
																				 " Y=" + objInArmsCoor.Y.ToString("0.000") +
																				 " Z=" + objInArmsCoor.Z.ToString("0.000") +
																				 " R=" + nextRoll.ToString("0.000") +
																				 " P=" + nextPitch.ToString("0.000") +
																				 " Y=" + nextYaw.ToString("0.000") +
																				 " E=" + nextElbow.ToString("0.000") + " }");

			if (!isReachable)
			{
				TBWriter.Warning1("dropXYZ_RIGHT : final position NOT REACHABLE. Finishing action ...");
				return false;
			}

			#endregion

			#region SENDING ARM TO hightake

			TBWriter.Info2(" dropXYZ_RIGHT : Sending RightArm to { hightake }");
			if (!cmdMan.ARMS_ra_goto("hightake", 7000))
				return false;

			#endregion

			#region SENDING ARM ABOVE THE OBJECT

			armPosInRobotCoor = wObj.Position + aboveOffset;
			armPosInArmsCoor = robot.TransRobot2RightArm(armPosInRobotCoor);

			nextRoll = aboveRoll;
			nextPitch = nextPitch;
			nextYaw = 0.0;
			nextElbow = nextElbow;

			TBWriter.Info1(" dropXYZ_RIGHT : Sending RightArm {above the object} :");
			TBWriter.Info1("	(worldCoor) { " + armPosInRobotCoor.X.ToString("0.000") + " , " +
													armPosInRobotCoor.Y.ToString("0.000") + " , " +
													armPosInRobotCoor.Z.ToString("0.000") + " } ");

			TBWriter.Info1("	(armsCoor)  { " + armPosInRobotCoor.X.ToString("0.000") + " , " +
													armPosInRobotCoor.Y.ToString("0.000") + " , " +
													armPosInRobotCoor.Z.ToString("0.000") + " } ");

			if (!this.cmdMan.ARMS_ra_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
			{
				TBWriter.Error("  dropXYZ_RIGHT : Cant send arm to {above the object}, trying again...");
				if (!this.cmdMan.ARMS_ra_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
				{
					TBWriter.Error("    dropXYZ_RIGHT : Cant send arm to {above the object}, trying again...");
					if (!this.cmdMan.ARMS_ra_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
					{
						TBWriter.Error("    dropXYZ_RIGHT : Cant send arm to {above the object}, RETURNING FALSE");
						return false;
					}
				}
			}

			#endregion

			#region SENDING ARMS TO TAKE OBJECT

			armPosInRobotCoor = wObj.Position + toDropOffset;
			armPosInArmsCoor = robot.TransRobot2RightArm(armPosInRobotCoor);

			nextRoll = toTakeRoll;
			nextPitch = nextPitch;
			nextYaw = 0.0;
			nextElbow = nextElbow;

			TBWriter.Info1(" dropXYZ_RIGHT : Sending RightArm {to drop object} :");
			TBWriter.Info1("	(worldCoor) { " + armPosInRobotCoor.X.ToString("0.000") + " , " +
													armPosInRobotCoor.Y.ToString("0.000") + " , " +
													armPosInRobotCoor.Z.ToString("0.000") + " } ");

			TBWriter.Info1("	(armsCoor)  { " + armPosInRobotCoor.X.ToString("0.000") + " , " +
													armPosInRobotCoor.Y.ToString("0.000") + " , " +
													armPosInRobotCoor.Z.ToString("0.000") + " } ");

			if (!this.cmdMan.ARMS_ra_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
			{
				TBWriter.Error("  dropXYZ_RIGHT : Cant send arm to {above the object}, trying again...");
				if (!this.cmdMan.ARMS_ra_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
				{
					TBWriter.Error("    dropXYZ_RIGHT : Cant send arm to {above the object}, trying again...");
					if (!this.cmdMan.ARMS_ra_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
					{
						TBWriter.Error("      dropXYZ_RIGHT : Cant send arm to {above the object}, RETURNING FALSE");
						return false;
					}
				}
			}

			#endregion

			#region OPENING GRIPPER

			TBWriter.Info1(" dropXYZ_RIGHT : openning RightArm {gripper} :");

			if (!this.cmdMan.ARMS_ra_opengrip(gripperOpening, 4000))
			{
				TBWriter.Error("  dropXYZ_RIGHT :  Can´t open gripper. Trying again...");
				if (!this.cmdMan.ARMS_ra_opengrip(gripperOpening, 4000))
				{
					TBWriter.Error("    dropXYZ_RIGHT :  Can´t open gripper. Trying again...");
					if (!this.cmdMan.ARMS_ra_opengrip(gripperOpening, 4000))
					{
						TBWriter.Error("      dropXYZ_RIGHT :  Can´t open gripper. RETURNING FALSE");
						return false;
					}
				}
			}

			#endregion

			#region SENDING ARM ABOVE THE OBJECT

			armPosInRobotCoor = wObj.Position + aboveOffset;
			armPosInArmsCoor = robot.TransRobot2RightArm(armPosInRobotCoor);

			nextRoll = aboveRoll;
			nextPitch = nextPitch;
			nextYaw = 0.0;
			nextElbow = nextElbow;

			TBWriter.Info1(" dropXYZ_RIGHT : Sending RightArm {above the object} :");
			TBWriter.Info1("	(worldCoor) { " + armPosInRobotCoor.X.ToString("0.000") + " , " +
													armPosInRobotCoor.Y.ToString("0.000") + " , " +
													armPosInRobotCoor.Z.ToString("0.000") + " } ");

			TBWriter.Info1("	(armsCoor)  { " + armPosInRobotCoor.X.ToString("0.000") + " , " +
													armPosInRobotCoor.Y.ToString("0.000") + " , " +
													armPosInRobotCoor.Z.ToString("0.000") + " } ");

			if (!this.cmdMan.ARMS_ra_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
			{
				TBWriter.Error("  dropXYZ_RIGHT : Cant send arm to {above the object}, trying again...");
				if (!this.cmdMan.ARMS_ra_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
				{
					TBWriter.Error("    dropXYZ_RIGHT : Cant send arm to {above the object}, trying again...");
					if (!this.cmdMan.ARMS_ra_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
					{
						TBWriter.Error("    dropXYZ_RIGHT : Cant send arm to {above the object}, RETURNING FALSE");
						return false;
					}
				}
			}

			#endregion

			#region CLOSING GRIPPER

			TBWriter.Info1(" dropXYZ_RIGHT : closing RightArm {gripper} :");

			if (!this.cmdMan.ARMS_ra_closegrip(4000))
			{
				TBWriter.Error("  dropXYZ_RIGHT :  Can´t close gripper. Trying again...");
				if (!this.cmdMan.ARMS_ra_closegrip(4000))
				{
					TBWriter.Error("    dropXYZ_RIGHT :  Can´t close gripper. Trying again...");
					if (!this.cmdMan.ARMS_ra_closegrip(4000))
					{
						TBWriter.Error("      dropXYZ_RIGHT :  Can´t close gripper. RETURNING FALSE");
						return false;
					}
				}
			}

			#endregion

			#region SENDING ARM TO hightake

			TBWriter.Info2(" dropXYZ_RIGHT : Sending RightArm to { hightake }");
			if (!cmdMan.ARMS_ra_goto("hightake", 7000))
				return false;

			#endregion

			return true;
		}*/

        public bool dropXYZ_RIGHT(WorldObject objectToDrop)
        {
            Vector3 armPosInRobotCoor;
            Vector3 armPosInArmsCoor;

            Vector3 errorVec;
            double errorAng;

            double nextRoll = MathUtil.PiOver2;
            double nextPitch = 0.0;
            double nextYaw = 0.0;
            double nextElbow = 0.0;


            double above_XY_Offset = 0.10;
            double above_Z_Offset = 0.1;
            double aboveRoll = MathUtil.PiOver2 + MathUtil.PiOver4;

            double gripperOpening = 50.0;

            double toDrop_XY_Offset = 0.05;
            double toDrop_Z_Offset = 0.05;
            double toDropRoll = MathUtil.PiOver2;

            WorldObject wObj = new WorldObject(objectToDrop.Name, objectToDrop.Position, objectToDrop.DistanceFromTable);

            Vector3 objInRobotCoor = wObj.Position;
            Vector3 objInArmsCoor = robot.TransRobot2RightArm(wObj.Position);

            TBWriter.Info1("=== Starting dropXYZ_RIGHT ===");
            TBWriter.Info1(" trying to drop {" + wObj.Name + "} in :");
            TBWriter.Info1("	(robotCoor) { " + objInRobotCoor.X.ToString("0.000") + " , " +
                                                    objInRobotCoor.Y.ToString("0.000") + " , " +
                                                    objInRobotCoor.Z.ToString("0.000") + " } ");
            TBWriter.Info1("	(armsCoor) { " + objInArmsCoor.X.ToString("0.000") + " , " +
                                                    objInArmsCoor.Y.ToString("0.000") + " , " +
                                                    objInArmsCoor.Z.ToString("0.000") + " } ");

            #region CALCULATING FINAL POSITION

            armPosInRobotCoor = robot.TransRightArm2Robot(Vector3.Zero);
            errorVec = wObj.Position - armPosInRobotCoor;
            errorVec.Z = 0;
            errorAng = Math.Atan2(errorVec.Y, errorVec.X);

            nextPitch = errorAng;
            nextElbow = -errorAng;

            bool isReachable = this.cmdMan.ARMS_ra_reachable(objInArmsCoor.X,
                                                                objInArmsCoor.Y,
                                                                objInArmsCoor.Z,
                                                                MathUtil.PiOver2,
                                                                nextPitch,
                                                                0,
                                                                nextElbow, 3000);

            TBWriter.Info1("dropXYZ_RIGHT : final position calculation (ArmsCoor) { X=" + objInArmsCoor.X.ToString("0.000") +
                                                                                 " Y=" + objInArmsCoor.Y.ToString("0.000") +
                                                                                 " Z=" + objInArmsCoor.Z.ToString("0.000") +
                                                                                 " R=" + nextRoll.ToString("0.000") +
                                                                                 " P=" + nextPitch.ToString("0.000") +
                                                                                 " Y=" + nextYaw.ToString("0.000") +
                                                                                 " E=" + nextElbow.ToString("0.000") + " }");

            if (!isReachable)
            {
                TBWriter.Warning1("dropXYZ_RIGHT : final position NOT REACHABLE. Finishing action ...");
                return false;
            }

            #endregion

            #region SENDING ARM TO hightake

            TBWriter.Info2(" dropXYZ_RIGHT : Sending RightArm to { hightake }");
            if (!cmdMan.ARMS_ra_goto("hightake", 7000))
                return false;

            #endregion

            #region SENDING ARM ABOVE THE OBJECT

            armPosInRobotCoor = wObj.Position - errorVec.Unitary * above_XY_Offset + new Vector3(0, 0, above_Z_Offset);
            armPosInArmsCoor = robot.TransRobot2RightArm(armPosInRobotCoor);

            nextRoll = aboveRoll;
            nextPitch = nextPitch;
            nextYaw = 0.0;
            nextElbow = nextElbow;

            TBWriter.Info1(" dropXYZ_RIGHT : Sending RightArm {above the object} :");
            TBWriter.Info1("	(worldCoor) { " + armPosInRobotCoor.X.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Y.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Z.ToString("0.000") + " } ");

            TBWriter.Info1("	(armsCoor)  { " + armPosInRobotCoor.X.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Y.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Z.ToString("0.000") + " } ");

            if (!this.cmdMan.ARMS_ra_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
            {
                TBWriter.Error("  dropXYZ_RIGHT : Cant send arm to {above the object}, trying again...");
                if (!this.cmdMan.ARMS_ra_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
                {
                    TBWriter.Error("    dropXYZ_RIGHT : Cant send arm to {above the object}, trying again...");
                    if (!this.cmdMan.ARMS_ra_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
                    {
                        TBWriter.Error("    dropXYZ_RIGHT : Cant send arm to {above the object}, RETURNING FALSE");
                        return false;
                    }
                }
            }

            #endregion

            #region SENDING ARMS TO DROP OBJECT

            armPosInRobotCoor = wObj.Position - errorVec.Unitary * toDrop_XY_Offset + new Vector3(0, 0, toDrop_Z_Offset);
            armPosInArmsCoor = robot.TransRobot2RightArm(armPosInRobotCoor);

            nextRoll = toDropRoll;
            nextYaw = 0.0;

            TBWriter.Info1(" dropXYZ_RIGHT : Sending RightArm {to drop object} :");
            TBWriter.Info1("	(worldCoor) { " + armPosInRobotCoor.X.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Y.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Z.ToString("0.000") + " } ");

            TBWriter.Info1("	(armsCoor)  { " + armPosInRobotCoor.X.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Y.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Z.ToString("0.000") + " } ");

            if (!this.cmdMan.ARMS_ra_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
            {
                TBWriter.Error("  dropXYZ_RIGHT : Cant send arm to {above the object}, trying again...");
                if (!this.cmdMan.ARMS_ra_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
                {
                    TBWriter.Error("    dropXYZ_RIGHT : Cant send arm to {above the object}, trying again...");
                    if (!this.cmdMan.ARMS_ra_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
                    {
                        TBWriter.Error("      dropXYZ_RIGHT : Cant send arm to {above the object}, RETURNING FALSE");
                        return false;
                    }
                }
            }

            #endregion

            #region OPENING GRIPPER

            TBWriter.Info1(" dropXYZ_RIGHT : openning RightArm {gripper} :");

            if (!this.cmdMan.ARMS_ra_opengrip(gripperOpening, 4000))
            {
                TBWriter.Error("  dropXYZ_RIGHT :  Can´t open gripper. Trying again...");
                if (!this.cmdMan.ARMS_ra_opengrip(gripperOpening, 4000))
                {
                    TBWriter.Error("    dropXYZ_RIGHT :  Can´t open gripper. Trying again...");
                    if (!this.cmdMan.ARMS_ra_opengrip(gripperOpening, 4000))
                    {
                        TBWriter.Error("      dropXYZ_RIGHT :  Can´t open gripper. RETURNING FALSE");
                        return false;
                    }
                }
            }

            #endregion

            #region SENDING ARM ABOVE THE OBJECT

            armPosInRobotCoor = wObj.Position - errorVec.Unitary * above_XY_Offset + new Vector3(0, 0, above_Z_Offset);
            armPosInArmsCoor = robot.TransRobot2RightArm(armPosInRobotCoor);

            nextRoll = aboveRoll;
            nextPitch = nextPitch;
            nextYaw = 0.0;
            nextElbow = nextElbow;

            TBWriter.Info1(" dropXYZ_RIGHT : Sending RightArm {above the object} :");
            TBWriter.Info1("	(worldCoor) { " + armPosInRobotCoor.X.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Y.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Z.ToString("0.000") + " } ");

            TBWriter.Info1("	(armsCoor)  { " + armPosInRobotCoor.X.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Y.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Z.ToString("0.000") + " } ");

            if (!this.cmdMan.ARMS_ra_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
            {
                TBWriter.Error("  dropXYZ_RIGHT : Cant send arm to {above the object}, trying again...");
                if (!this.cmdMan.ARMS_ra_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
                {
                    TBWriter.Error("    dropXYZ_RIGHT : Cant send arm to {above the object}, trying again...");
                    if (!this.cmdMan.ARMS_ra_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
                    {
                        TBWriter.Error("    dropXYZ_RIGHT : Cant send arm to {above the object}, RETURNING FALSE");
                        return false;
                    }
                }
            }

            #endregion

            #region CLOSING GRIPPER

            TBWriter.Info1(" dropXYZ_RIGHT : closing RightArm {gripper} :");

            if (!this.cmdMan.ARMS_ra_closegrip(4000))
            {
                TBWriter.Error("  dropXYZ_RIGHT :  Can´t close gripper. Trying again...");
                if (!this.cmdMan.ARMS_ra_closegrip(4000))
                {
                    TBWriter.Error("    dropXYZ_RIGHT :  Can´t close gripper. Trying again...");
                    if (!this.cmdMan.ARMS_ra_closegrip(4000))
                    {
                        TBWriter.Error("      dropXYZ_RIGHT :  Can´t close gripper. RETURNING FALSE");
                        return false;
                    }
                }
            }

            #endregion

            #region SENDING ARM TO hightake

            TBWriter.Info2(" dropXYZ_RIGHT : Sending RightArm to { hightake }");
            if (!cmdMan.ARMS_ra_goto("hightake", 7000))
                return false;

            #endregion

            return true;
        }

        public bool dropXYZ_LEFT(WorldObject objectToDrop)
        {
            Vector3 armPosInRobotCoor;
            Vector3 armPosInArmsCoor;

            Vector3 errorVec;
            double errorAng;

            double nextRoll = MathUtil.PiOver2;
            double nextPitch = 0.0;
            double nextYaw = 0.0;
            double nextElbow = 0.0;


            double above_XY_Offset = 0.10;
            double above_Z_Offset = 0.1;
            double aboveRoll = MathUtil.PiOver2 + MathUtil.PiOver4;

            double gripperOpening = 50.0;

            double toDrop_XY_Offset = 0.05;
            double toDrop_Z_Offset = 0.05;
            double toDropRoll = MathUtil.PiOver2;

            WorldObject wObj = new WorldObject(objectToDrop.Name, objectToDrop.Position, objectToDrop.DistanceFromTable);

            Vector3 objInRobotCoor = wObj.Position;
            Vector3 objInArmsCoor = robot.TransRobot2LeftArm(wObj.Position);

            TBWriter.Info1("=== Starting dropXYZ_LEFT ===");
            TBWriter.Info1(" trying to drop {" + wObj.Name + "} in :");
            TBWriter.Info1("    (robotCoor) { " + objInRobotCoor.X.ToString("0.000") + " , " +
                                                    objInRobotCoor.Y.ToString("0.000") + " , " +
                                                    objInRobotCoor.Z.ToString("0.000") + " } ");
            TBWriter.Info1("    (armsCoor) { " + objInArmsCoor.X.ToString("0.000") + " , " +
                                                    objInArmsCoor.Y.ToString("0.000") + " , " +
                                                    objInArmsCoor.Z.ToString("0.000") + " } ");

            #region CALCULATING FINAL POSITION

            armPosInRobotCoor = robot.TransLeftArm2Robot(Vector3.Zero);
            errorVec = wObj.Position - armPosInRobotCoor;
            errorVec.Z = 0;
            errorAng = Math.Atan2(errorVec.Y, errorVec.X);

            nextPitch = errorAng;
            nextElbow = -errorAng;

            bool isReachable = this.cmdMan.ARMS_la_reachable(objInArmsCoor.X,
                                                                objInArmsCoor.Y,
                                                                objInArmsCoor.Z,
                                                                MathUtil.PiOver2,
                                                                nextPitch,
                                                                0,
                                                                nextElbow, 3000);

            TBWriter.Info1("dropXYZ_LEFT : final position calculation (ArmsCoor) { X=" + objInArmsCoor.X.ToString("0.000") +
                                                                                 " Y=" + objInArmsCoor.Y.ToString("0.000") +
                                                                                 " Z=" + objInArmsCoor.Z.ToString("0.000") +
                                                                                 " R=" + nextRoll.ToString("0.000") +
                                                                                 " P=" + nextPitch.ToString("0.000") +
                                                                                 " Y=" + nextYaw.ToString("0.000") +
                                                                                 " E=" + nextElbow.ToString("0.000") + " }");

            if (!isReachable)
            {
                TBWriter.Warning1("dropXYZ_LEFT : final position NOT REACHABLE. Finishing action ...");
                return false;
            }

            #endregion

            #region SENDING ARM TO hightake

            TBWriter.Info2(" dropXYZ_LEFT : Sending LeftArm to { hightake }");
            if (!cmdMan.ARMS_la_goto("hightake", 7000))
                return false;

            #endregion

            #region SENDING ARM ABOVE THE OBJECT

            armPosInRobotCoor = wObj.Position - errorVec.Unitary * above_XY_Offset + new Vector3(0, 0, above_Z_Offset);
            armPosInArmsCoor = robot.TransRobot2LeftArm(armPosInRobotCoor);

            nextRoll = aboveRoll;
            nextPitch = nextPitch;
            nextYaw = 0.0;
            nextElbow = nextElbow;

            TBWriter.Info1(" dropXYZ_LEFT : Sending LeftArm {above the object} :");
            TBWriter.Info1("    (worldCoor) { " + armPosInRobotCoor.X.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Y.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Z.ToString("0.000") + " } ");

            TBWriter.Info1("    (armsCoor)  { " + armPosInRobotCoor.X.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Y.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Z.ToString("0.000") + " } ");

            if (!this.cmdMan.ARMS_la_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
            {
                TBWriter.Error("  dropXYZ_LEFT : Cant send arm to {above the object}, trying again...");
                if (!this.cmdMan.ARMS_la_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
                {
                    TBWriter.Error("    dropXYZ_LEFT : Cant send arm to {above the object}, trying again...");
                    if (!this.cmdMan.ARMS_la_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
                    {
                        TBWriter.Error("    dropXYZ_LEFT : Cant send arm to {above the object}, RETURNING FALSE");
                        return false;
                    }
                }
            }

            #endregion

            #region SENDING ARMS TO DROP OBJECT

            armPosInRobotCoor = wObj.Position - errorVec.Unitary * toDrop_XY_Offset + new Vector3(0, 0, toDrop_Z_Offset);
            armPosInArmsCoor = robot.TransRobot2LeftArm(armPosInRobotCoor);

            nextRoll = toDropRoll;
            nextYaw = 0.0;

            TBWriter.Info1(" dropXYZ_LEFT : Sending LeftArm {to drop object} :");
            TBWriter.Info1("    (worldCoor) { " + armPosInRobotCoor.X.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Y.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Z.ToString("0.000") + " } ");

            TBWriter.Info1("    (armsCoor)  { " + armPosInRobotCoor.X.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Y.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Z.ToString("0.000") + " } ");

            if (!this.cmdMan.ARMS_la_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
            {
                TBWriter.Error("  dropXYZ_LEFT : Cant send arm to {above the object}, trying again...");
                if (!this.cmdMan.ARMS_la_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
                {
                    TBWriter.Error("    dropXYZ_LEFT : Cant send arm to {above the object}, trying again...");
                    if (!this.cmdMan.ARMS_la_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
                    {
                        TBWriter.Error("      dropXYZ_LEFT : Cant send arm to {above the object}, RETURNING FALSE");
                        return false;
                    }
                }
            }

            #endregion

            #region OPENING GRIPPER

            TBWriter.Info1(" dropXYZ_LEFT : openning LeftArm {gripper} :");

            if (!this.cmdMan.ARMS_la_opengrip(gripperOpening, 4000))
            {
                TBWriter.Error("  dropXYZ_LEFT :  Can´t open gripper. Trying again...");
                if (!this.cmdMan.ARMS_la_opengrip(gripperOpening, 4000))
                {
                    TBWriter.Error("    dropXYZ_LEFT :  Can´t open gripper. Trying again...");
                    if (!this.cmdMan.ARMS_la_opengrip(gripperOpening, 4000))
                    {
                        TBWriter.Error("      dropXYZ_LEFT :  Can´t open gripper. RETURNING FALSE");
                        return false;
                    }
                }
            }

            #endregion

            #region SENDING ARM ABOVE THE OBJECT

            armPosInRobotCoor = wObj.Position - errorVec.Unitary * above_XY_Offset + new Vector3(0, 0, above_Z_Offset);
            armPosInArmsCoor = robot.TransRobot2LeftArm(armPosInRobotCoor);

            nextRoll = aboveRoll;
            nextPitch = nextPitch;
            nextYaw = 0.0;
            nextElbow = nextElbow;

            TBWriter.Info1(" dropXYZ_LEFT : Sending LeftArm {above the object} :");
            TBWriter.Info1("    (worldCoor) { " + armPosInRobotCoor.X.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Y.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Z.ToString("0.000") + " } ");

            TBWriter.Info1("    (armsCoor)  { " + armPosInRobotCoor.X.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Y.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Z.ToString("0.000") + " } ");

            if (!this.cmdMan.ARMS_la_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
            {
                TBWriter.Error("  dropXYZ_LEFT : Cant send arm to {above the object}, trying again...");
                if (!this.cmdMan.ARMS_la_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
                {
                    TBWriter.Error("    dropXYZ_LEFT : Cant send arm to {above the object}, trying again...");
                    if (!this.cmdMan.ARMS_la_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
                    {
                        TBWriter.Error("    dropXYZ_LEFT : Cant send arm to {above the object}, RETURNING FALSE");
                        return false;
                    }
                }
            }

            #endregion

            #region CLOSING GRIPPER

            TBWriter.Info1(" dropXYZ_LEFT : closing LeftArm {gripper} :");

            if (!this.cmdMan.ARMS_la_closegrip(4000))
            {
                TBWriter.Error("  dropXYZ_LEFT :  Can´t close gripper. Trying again...");
                if (!this.cmdMan.ARMS_la_closegrip(4000))
                {
                    TBWriter.Error("    dropXYZ_LEFT :  Can´t close gripper. Trying again...");
                    if (!this.cmdMan.ARMS_la_closegrip(4000))
                    {
                        TBWriter.Error("      dropXYZ_LEFT :  Can´t close gripper. RETURNING FALSE");
                        return false;
                    }
                }
            }

            #endregion

            #region SENDING ARM TO hightake

            TBWriter.Info2(" dropXYZ_LEFT : Sending LeftArm to { hightake }");
            if (!cmdMan.ARMS_la_goto("hightake", 7000))
                return false;

            #endregion

            return true;
        }


        public bool takeXYZ_LEFT(WorldObject objectToTake)
        {
            Vector3 armPosInRobotCoor;
            Vector3 armPosInArmsCoor;

            Vector3 errorVec;
            double errorAng;

            double nextRoll = MathUtil.PiOver2;
            double nextPitch;
            double nextYaw = 0.0;
            double nextElbow = 0.0;

            double above_XY_Offset = 0.20;
            double above_Z_Offset = 0.05;
            double aboveRoll = MathUtil.PiOver2 + MathUtil.PiOver4;

            double gripperOpening = 50.0;

            double toTake_XY_Offset = 0.05;
            double toTake_Z_Offset = 0.03;
            double toTakeRoll = MathUtil.PiOver2;

            double retrieve_XY_offset = 0.20;
            double retrieve_Z_offset = 0.15;
            double retrieveRoll = MathUtil.PiOver2 + MathUtil.PiOver4;

            WorldObject wObj = new WorldObject(objectToTake.Name, objectToTake.Position, objectToTake.DistanceFromTable);
            Vector3 objInRobotCoor = wObj.Position;
            Vector3 objInArmsCoor = robot.TransRobot2LeftArm(wObj.Position);

            TBWriter.Info1("=== Starting  takeXYZ_LEFT ===");
            TBWriter.Info1(" trying to take {" + wObj.Name + "} in :");
            TBWriter.Info1("	(robotCoor) { " + objInRobotCoor.X.ToString("0.000") + " , " +
                                                    objInRobotCoor.Y.ToString("0.000") + " , " +
                                                    objInRobotCoor.Z.ToString("0.000") + " } ");
            TBWriter.Info1("	(armsCoor) { " + objInArmsCoor.X.ToString("0.000") + " , " +
                                                    objInArmsCoor.Y.ToString("0.000") + " , " +
                                                    objInArmsCoor.Z.ToString("0.000") + " } ");

            #region CALCULATING FINAL POSITION

            armPosInRobotCoor = robot.TransLeftArm2Robot(Vector3.Zero);
            errorVec = wObj.Position - armPosInRobotCoor;
            errorVec.Z = 0;
            errorAng = Math.Atan2(errorVec.Y, errorVec.X);


            // if torso colide ( Change for left and right )
            nextPitch = errorAng;
            nextElbow = -errorAng;

            bool isReachable = this.cmdMan.ARMS_la_reachable(objInArmsCoor.X,
                                                                objInArmsCoor.Y,
                                                                objInArmsCoor.Z,
                                                                MathUtil.PiOver2,
                                                                nextPitch,
                                                                0,
                                                                nextElbow, 3000);

            TBWriter.Info1(" takeXYZ_LEFT : final position calculation (ArmsCoor) { X=" + objInArmsCoor.X.ToString("0.000") +
                                                                                 " Y=" + objInArmsCoor.Y.ToString("0.000") +
                                                                                 " Z=" + objInArmsCoor.Z.ToString("0.000") +
                                                                                 " R=" + nextRoll.ToString("0.000") +
                                                                                 " P=" + nextPitch.ToString("0.000") +
                                                                                 " Y=" + nextYaw.ToString("0.000") +
                                                                                 " E=" + nextElbow.ToString("0.000") + " }");

            if (!isReachable)
            {
                TBWriter.Warning1(" takeXYZ_LEFT : final position NOT REACHABLE. Finishing action ...");
                return false;
            }

            #endregion

            #region SENDING ARM TO navigation AND hightake

            //TBWriter.Info2("  takeXYZ_LEFT : Sending LeftArm to { navigation }");
            //if (!cmdMan.ARMS_la_goto("navigation", 7000))
            //    return false;

            TBWriter.Info2("  takeXYZ_LEFT : Sending LeftArm to { hightake }");
            if (!cmdMan.ARMS_la_goto("hightake", 7000))
            {
                TBWriter.Error(" takeXYZ_LEFT : LeftArm can't go to { hightake }");
                return false;
            }

            #endregion

            #region SENDING ARM ABOVE THE OBJECT

            armPosInRobotCoor = wObj.Position - errorVec.Unitary * above_XY_Offset + new Vector3(0, 0, above_Z_Offset);
            armPosInArmsCoor = robot.TransRobot2LeftArm(armPosInRobotCoor);

            nextRoll = aboveRoll;
            //nextPitch = nextPitch;
            nextYaw = 0.0;
            //nextElbow = nextElbow;

            TBWriter.Info1("  takeXYZ_LEFT : Sending LeftArm {above the object} :");
            TBWriter.Info1("	(worldCoor) { " + armPosInRobotCoor.X.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Y.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Z.ToString("0.000") + " } ");

            TBWriter.Info1("	(armsCoor)  { " + armPosInRobotCoor.X.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Y.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Z.ToString("0.000") + " } ");

            if (!this.cmdMan.ARMS_la_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
            {
                TBWriter.Error("   takeXYZ_LEFT : Cant send arm to {above the object}, trying again...");
                if (!this.cmdMan.ARMS_la_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
                {
                    TBWriter.Error("     takeXYZ_LEFT : Cant send arm to {above the object}, trying again...");
                    if (!this.cmdMan.ARMS_la_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
                    {
                        TBWriter.Error("     takeXYZ_LEFT : Cant send arm to {above the object}, RETURNING FALSE");
                        return false;
                    }
                }
            }

            #endregion

            #region OPENING GRIPPER

            TBWriter.Info1("  takeXYZ_LEFT : openning LeftArm {gripper} :");

            if (!this.cmdMan.ARMS_la_opengrip(gripperOpening, 4000))
            {
                TBWriter.Error("   takeXYZ_LEFT :  Can´t open gripper. Trying again...");
                if (!this.cmdMan.ARMS_la_opengrip(gripperOpening, 4000))
                {
                    TBWriter.Error("     takeXYZ_LEFT :  Can´t open gripper. Trying again...");
                    if (!this.cmdMan.ARMS_la_opengrip(gripperOpening, 4000))
                    {
                        TBWriter.Error("       takeXYZ_LEFT :  Can´t open gripper. RETURNING FALSE");
                        return false;
                    }
                }
            }

            #endregion

            #region SENDING ARMS TO TAKE OBJECT

            armPosInRobotCoor = wObj.Position - errorVec.Unitary * toTake_XY_Offset + new Vector3(0, 0, toTake_Z_Offset);
            armPosInArmsCoor = robot.TransRobot2LeftArm(armPosInRobotCoor);

            nextRoll = toTakeRoll;
            //nextPitch = nextPitch;
            nextYaw = 0.0;
            //nextElbow = nextElbow;

            TBWriter.Info1("  takeXYZ_LEFT : Sending LeftArm {to take object} :");
            TBWriter.Info1("	(worldCoor) { " + armPosInRobotCoor.X.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Y.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Z.ToString("0.000") + " } ");

            TBWriter.Info1("	(armsCoor)  { " + armPosInRobotCoor.X.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Y.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Z.ToString("0.000") + " } ");

            if (!this.cmdMan.ARMS_la_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
            {
                TBWriter.Error("   takeXYZ_LEFT : Cant send arm to {above the object}, trying again...");
                if (!this.cmdMan.ARMS_la_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
                {
                    TBWriter.Error("     takeXYZ_LEFT : Cant send arm to {above the object}, trying again...");
                    if (!this.cmdMan.ARMS_la_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
                    {
                        TBWriter.Error("       takeXYZ_LEFT : Cant send arm to {above the object}, RETURNING FALSE");
                        return false;
                    }
                }
            }

            #endregion

            #region CLOSING GRIPPER

            TBWriter.Info1("  takeXYZ_LEFT : closing LeftArm {gripper} :");

            if (!this.cmdMan.ARMS_la_closegrip(4000))
            {
                TBWriter.Error("   takeXYZ_LEFT :  Can´t close gripper. Trying again...");
                if (!this.cmdMan.ARMS_la_closegrip(4000))
                {
                    TBWriter.Error("     takeXYZ_LEFT :  Can´t close gripper. Trying again...");
                    if (!this.cmdMan.ARMS_la_closegrip(4000))
                    {
                        TBWriter.Error("       takeXYZ_LEFT :  Can´t close gripper. RETURNING FALSE");
                        return false;
                    }
                }
            }

            #endregion

            #region SENDING ARM ABOVE THE OBJECT

            armPosInRobotCoor = wObj.Position - errorVec.Unitary * retrieve_XY_offset + new Vector3(0.0, 0.0, retrieve_Z_offset);
            armPosInArmsCoor = robot.TransRobot2LeftArm(armPosInRobotCoor);

            nextRoll = retrieveRoll;
            //nextPitch = nextPitch;
            nextYaw = 0.0;
            //nextElbow = nextElbow;

            TBWriter.Info1("  takeXYZ_LEFT : Sending LeftArm {above the object} :");
            TBWriter.Info1("	(worldCoor) { " + armPosInRobotCoor.X.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Y.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Z.ToString("0.000") + " } ");

            TBWriter.Info1("	(armsCoor)  { " + armPosInRobotCoor.X.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Y.ToString("0.000") + " , " +
                                                    armPosInRobotCoor.Z.ToString("0.000") + " } ");

            if (!this.cmdMan.ARMS_la_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
            {
                TBWriter.Error("   takeXYZ_LEFT : Cant send arm to {above the object}, trying again...");
                if (!this.cmdMan.ARMS_la_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
                {
                    TBWriter.Error("     takeXYZ_LEFT : Cant send arm to {above the object}, trying again...");
                    if (!this.cmdMan.ARMS_la_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
                    {
                        TBWriter.Error("     takeXYZ_LEFT : Cant send arm to {above the object}, RETURNING FALSE");
                        return false;
                    }
                }
            }

            #endregion

            #region SENDING ARM TO hightake

            TBWriter.Info2("  takeXYZ_LEFT : Sending LeftArm to { hightake }");
            if (!cmdMan.ARMS_la_goto("hightake", 7000))
            {
                TBWriter.Error(" takeXYZ_LEFT : LeftArm can't go to { hightake }");
                return false;
            }
            #endregion

            return true;
        }

		public bool takeXYZ_RIGHT(WorldObject objectToTake)
		{
			Vector3 armPosInRobotCoor;
			Vector3 armPosInArmsCoor;

			Vector3 errorVec;
			double errorAng;

			double nextRoll = MathUtil.PiOver2;
			double nextPitch;
			double nextYaw = 0.0;
			double nextElbow = 0.0;

            double above_XY_Offset = 0.20;
            double above_Z_Offset = 0.05;
			double aboveRoll = MathUtil.PiOver2 + MathUtil.PiOver4;

			double gripperOpening = 50.0;

            double toTake_XY_Offset = 0.05;
            double toTake_Z_Offset = 0.03;        
			double toTakeRoll = MathUtil.PiOver2;

            double retrieve_XY_offset = 0.20;
            double retrieve_Z_offset = 0.15;
			double retrieveRoll = MathUtil.PiOver2 + MathUtil.PiOver4;

			WorldObject wObj = new WorldObject(objectToTake.Name, objectToTake.Position, objectToTake.DistanceFromTable);
			Vector3 objInRobotCoor = wObj.Position;
			Vector3 objInArmsCoor = robot.TransRobot2RightArm(wObj.Position);

			TBWriter.Info1("=== Starting  takeXYZ_RIGHT ===");
			TBWriter.Info1(" trying to take {" + wObj.Name + "} in :");
			TBWriter.Info1("	(robotCoor) { " +   objInRobotCoor.X.ToString("0.000") + " , " +
												    objInRobotCoor.Y.ToString("0.000") + " , " +
													objInRobotCoor.Z.ToString("0.000") + " } ");
			TBWriter.Info1("	(armsCoor) { " +    objInArmsCoor.X.ToString("0.000") + " , " +
													objInArmsCoor.Y.ToString("0.000") + " , " +
													objInArmsCoor.Z.ToString("0.000") + " } ");

			#region CALCULATING FINAL POSITION

			armPosInRobotCoor = robot.TransRightArm2Robot(Vector3.Zero);            
			errorVec = wObj.Position - armPosInRobotCoor;
            errorVec.Z = 0;
			errorAng = Math.Atan2(errorVec.Y, errorVec.X);
         

			// if torso colide ( Change for left and right )
			nextPitch = errorAng;
            nextElbow = -errorAng;

			bool isReachable = this.cmdMan.ARMS_ra_reachable(objInArmsCoor.X,
																objInArmsCoor.Y,
																objInArmsCoor.Z,
																MathUtil.PiOver2,
																nextPitch,
																0,
																nextElbow, 3000);

			TBWriter.Info1(" takeXYZ_RIGHT : final position calculation (ArmsCoor) { X=" + objInArmsCoor.X.ToString("0.000") +
																				 " Y=" + objInArmsCoor.Y.ToString("0.000") +
																				 " Z=" + objInArmsCoor.Z.ToString("0.000") +
																				 " R=" + nextRoll.ToString("0.000") +
																				 " P=" + nextPitch.ToString("0.000") +
																				 " Y=" + nextYaw.ToString("0.000") +
																				 " E=" + nextElbow.ToString("0.000") + " }");

			if (!isReachable)
			{
				TBWriter.Warning1(" takeXYZ_RIGHT : final position NOT REACHABLE. Finishing action ...");
				return false;
			}

			#endregion

			#region SENDING ARM TO navigation AND hightake

			//TBWriter.Info2("  takeXYZ_RIGHT : Sending RightArm to { navigation }");
			//if (!cmdMan.ARMS_ra_goto("navigation", 7000))
			//    return false;

			TBWriter.Info2("  takeXYZ_RIGHT : Sending RightArm to { hightake }");
			if (!cmdMan.ARMS_ra_goto("hightake", 7000))
			{
				TBWriter.Error(" takeXYZ_RIGHT : RightArm can't go to { hightake }");
				return false;
			}

			#endregion

			#region SENDING ARM ABOVE THE OBJECT

            armPosInRobotCoor = wObj.Position - errorVec.Unitary * above_XY_Offset + new Vector3(0, 0, above_Z_Offset);
			armPosInArmsCoor = robot.TransRobot2RightArm(armPosInRobotCoor);

			nextRoll = aboveRoll;
			//nextPitch = nextPitch;
			nextYaw = 0.0;
			//nextElbow = nextElbow;

			TBWriter.Info1("  takeXYZ_RIGHT : Sending RightArm {above the object} :");
			TBWriter.Info1("	(worldCoor) { " + armPosInRobotCoor.X.ToString("0.000") + " , " +
													armPosInRobotCoor.Y.ToString("0.000") + " , " +
													armPosInRobotCoor.Z.ToString("0.000") + " } ");

			TBWriter.Info1("	(armsCoor)  { " + armPosInRobotCoor.X.ToString("0.000") + " , " +
													armPosInRobotCoor.Y.ToString("0.000") + " , " +
													armPosInRobotCoor.Z.ToString("0.000") + " } ");

			if (!this.cmdMan.ARMS_ra_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
			{
				TBWriter.Error("   takeXYZ_RIGHT : Cant send arm to {above the object}, trying again...");
				if (!this.cmdMan.ARMS_ra_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
				{
					TBWriter.Error("     takeXYZ_RIGHT : Cant send arm to {above the object}, trying again...");
					if (!this.cmdMan.ARMS_ra_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
					{
						TBWriter.Error("     takeXYZ_RIGHT : Cant send arm to {above the object}, RETURNING FALSE");
						return false;
					}
				}
			}

			#endregion

			#region OPENING GRIPPER

			TBWriter.Info1("  takeXYZ_RIGHT : openning RightArm {gripper} :");

			if (!this.cmdMan.ARMS_ra_opengrip(gripperOpening, 4000))
			{
				TBWriter.Error("   takeXYZ_RIGHT :  Can´t open gripper. Trying again...");
				if (!this.cmdMan.ARMS_ra_opengrip(gripperOpening, 4000))
				{
					TBWriter.Error("     takeXYZ_RIGHT :  Can´t open gripper. Trying again...");
					if (!this.cmdMan.ARMS_ra_opengrip(gripperOpening, 4000))
					{
						TBWriter.Error("       takeXYZ_RIGHT :  Can´t open gripper. RETURNING FALSE");
						return false;
					}
				}
			}

			#endregion

			#region SENDING ARMS TO TAKE OBJECT

            armPosInRobotCoor = wObj.Position - errorVec.Unitary * toTake_XY_Offset + new Vector3(0, 0, toTake_Z_Offset);
			armPosInArmsCoor = robot.TransRobot2RightArm(armPosInRobotCoor);

			nextRoll = toTakeRoll;
			//nextPitch = nextPitch;
			nextYaw = 0.0;
			//nextElbow = nextElbow;

			TBWriter.Info1("  takeXYZ_RIGHT : Sending RightArm {to take object} :");
			TBWriter.Info1("	(worldCoor) { " + armPosInRobotCoor.X.ToString("0.000") + " , " +
													armPosInRobotCoor.Y.ToString("0.000") + " , " +
													armPosInRobotCoor.Z.ToString("0.000") + " } ");

			TBWriter.Info1("	(armsCoor)  { " + armPosInRobotCoor.X.ToString("0.000") + " , " +
													armPosInRobotCoor.Y.ToString("0.000") + " , " +
													armPosInRobotCoor.Z.ToString("0.000") + " } ");

			if (!this.cmdMan.ARMS_ra_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
			{
				TBWriter.Error("   takeXYZ_RIGHT : Cant send arm to {above the object}, trying again...");
				if (!this.cmdMan.ARMS_ra_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
				{
					TBWriter.Error("     takeXYZ_RIGHT : Cant send arm to {above the object}, trying again...");
					if (!this.cmdMan.ARMS_ra_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
					{
						TBWriter.Error("       takeXYZ_RIGHT : Cant send arm to {above the object}, RETURNING FALSE");
						return false;
					}
				}
			}

			#endregion

			#region CLOSING GRIPPER

			TBWriter.Info1("  takeXYZ_RIGHT : closing RightArm {gripper} :");

			if (!this.cmdMan.ARMS_ra_closegrip(4000))
			{
				TBWriter.Error("   takeXYZ_RIGHT :  Can´t close gripper. Trying again...");
				if (!this.cmdMan.ARMS_ra_closegrip(4000))
				{
					TBWriter.Error("     takeXYZ_RIGHT :  Can´t close gripper. Trying again...");
					if (!this.cmdMan.ARMS_ra_closegrip(4000))
					{
						TBWriter.Error("       takeXYZ_RIGHT :  Can´t close gripper. RETURNING FALSE");
						return false;
					}
				}
			}

			#endregion

			#region SENDING ARM ABOVE THE OBJECT

            armPosInRobotCoor = wObj.Position - errorVec.Unitary * retrieve_XY_offset  + new Vector3(0.0, 0.0, retrieve_Z_offset);
			armPosInArmsCoor = robot.TransRobot2RightArm(armPosInRobotCoor);

			nextRoll = retrieveRoll;
			//nextPitch = nextPitch;
			nextYaw = 0.0;
			//nextElbow = nextElbow;

			TBWriter.Info1("  takeXYZ_RIGHT : Sending RightArm {above the object} :");
			TBWriter.Info1("	(worldCoor) { " + armPosInRobotCoor.X.ToString("0.000") + " , " +
													armPosInRobotCoor.Y.ToString("0.000") + " , " +
													armPosInRobotCoor.Z.ToString("0.000") + " } ");

			TBWriter.Info1("	(armsCoor)  { " + armPosInRobotCoor.X.ToString("0.000") + " , " +
													armPosInRobotCoor.Y.ToString("0.000") + " , " +
													armPosInRobotCoor.Z.ToString("0.000") + " } ");

			if (!this.cmdMan.ARMS_ra_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
			{
				TBWriter.Error("   takeXYZ_RIGHT : Cant send arm to {above the object}, trying again...");
				if (!this.cmdMan.ARMS_ra_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
				{
					TBWriter.Error("     takeXYZ_RIGHT : Cant send arm to {above the object}, trying again...");
					if (!this.cmdMan.ARMS_ra_abspos(armPosInArmsCoor, nextRoll, nextPitch, nextYaw, nextElbow, 10000))
					{
						TBWriter.Error("     takeXYZ_RIGHT : Cant send arm to {above the object}, RETURNING FALSE");
						return false;
					}
				}
			}

			#endregion

			#region SENDING ARM TO hightake

			TBWriter.Info2("  takeXYZ_RIGHT : Sending RightArm to { hightake }");
			if (!cmdMan.ARMS_ra_goto("hightake", 7000))
			{
				TBWriter.Error(" takeXYZ_RIGHT : RightArm can't go to { hightake }");
				return false;
			}
			#endregion

			return true;
		}



		#endregion 
	}
}
