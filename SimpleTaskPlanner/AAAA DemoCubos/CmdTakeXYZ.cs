using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.API;
using Robotics.Mathematics;

namespace SimpleTaskPlanner
{
    class CmdTakeXYZ : AsyncCommandExecuter
    {
        STPCommandManager cmdMan;
        TaskPlanner taskplanner;

        public CmdTakeXYZ(STPCommandManager cmdMan, TaskPlanner taskplanner)
            : base("takexyz")
        {
            this.cmdMan = cmdMan;
            this.taskplanner = taskplanner;
        }

        protected override Response AsyncTask(Command command)
        {
            TBWriter.Spaced(" >>>  Received  " + command.CommandName + " , received = [" + command.Parameters + "]");

			string[] splitParams = command.Parameters.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

			if( splitParams.Length != 4)
			{
                TBWriter.Error("Not enough parameters found.");
				TBWriter.Spaced("      Terminated  " + command.CommandName + " , sending = [" + command.Parameters + "]  <<<");
				this.cmdMan.Busy = false;
                return Response.CreateFromCommand(command, false);
			}

			string armToTake = splitParams[0];

			double x;
			double y;
			double z;
			if (!double.TryParse(splitParams[1], out x))
			{
				TBWriter.Error("Cant parse X= " + splitParams[1]);
				TBWriter.Spaced("      Terminated  " + command.CommandName + " , sending = [" + command.Parameters + "]  <<<");
				this.cmdMan.Busy = false;
				return Response.CreateFromCommand(command, false);
			}
			if (!double.TryParse(splitParams[2], out y))
			{
				TBWriter.Error("Cant parse Y= " + splitParams[2]);
				TBWriter.Spaced("      Terminated  " + command.CommandName + " , sending = [" + command.Parameters + "]  <<<");
				this.cmdMan.Busy = false;
				return Response.CreateFromCommand(command, false);
			}
			if (!double.TryParse(splitParams[3], out z))
			{
				TBWriter.Error("Cant parse Z= " + splitParams[3]);
				TBWriter.Spaced("      Terminated  " + command.CommandName + " , sending = [" + command.Parameters + "]  <<<");
				this.cmdMan.Busy = false;
				return Response.CreateFromCommand(command, false);
			}

			bool success;
			if (armToTake.Contains("left"))
				success = this.taskplanner.takeXYZ_LEFT(new WorldObject("unknown", new Vector3(x, y, z)));
			else if(armToTake.Contains("right"))
				success = this.taskplanner.takeXYZ_RIGHT(new WorldObject("unknown", new Vector3(x, y, z)));
			else
			{
				TBWriter.Error("No arm to take defined = " + armToTake);
				TBWriter.Spaced("      Terminated  " + command.CommandName + " , sending = [" + command.Parameters + "]  <<<");
				this.cmdMan.Busy = false;
				return Response.CreateFromCommand(command, false);
			}
			
			TBWriter.Spaced("      Terminated  " + command.CommandName + ", success{"+success+ "} , sending = [" + command.Parameters + "]  <<<");
			this.cmdMan.Busy = false;
			return Response.CreateFromCommand(command, success);
        }

		public override bool ParametersRequired
		{
			get
			{
				return true;
			}
		}


        public override void DefaultParameterParser(string[] parameters)
        {
            return;
        }
    }
}