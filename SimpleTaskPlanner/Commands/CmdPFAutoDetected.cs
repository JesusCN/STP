using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.API;
using Robotics.Mathematics;

namespace SimpleTaskPlanner
{
    class CmdPFAutoDetected:SyncCommandExecuter
    {
        STPCommandManager cmdMan;
        TaskPlanner taskPlanner;

        public CmdPFAutoDetected(STPCommandManager cmdMan, TaskPlanner taskPlanner)
            : base("detected")
        {
            this.cmdMan = cmdMan;
            this.taskPlanner = taskPlanner;
        }

        public override bool ParametersRequired
        { get { return false; } }

        protected override Response SyncTask(Command command)
        {
            TBWriter.Spaced(" >>>  Received  " + command.CommandName + " , received = [" + command.Parameters + "]");
            
            string humanName;
            Vector3 posiblePosition;

			double humanPan;
			double humanTilt;

            if (Parse.humanInfo(command.Parameters, out humanName, out posiblePosition, out humanPan, out humanTilt))
            {
                posiblePosition = taskPlanner.robot.TransMinoru2Robot(posiblePosition);
                taskPlanner.personPFAutoDetected = new Human(humanName, posiblePosition);

                TBWriter.Info2("PFAutoDetected : founded " + humanName + " located in " + posiblePosition.ToString());
				this.cmdMan.PRS_FND_auto(false, "");
			}       
            else
                taskPlanner.personPFAutoDetected = null;
            
            TBWriter.Spaced("      Terminated  " + command.CommandName + " , sending = [" + command.Parameters + "]  <<<");

            return null;
        }
    }
}
