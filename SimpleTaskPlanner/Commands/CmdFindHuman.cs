using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.API;
using Robotics.Controls;

namespace SimpleTaskPlanner
{
    public class CmdFindHuman : AsyncCommandExecuter
    {
        STPCommandManager cmdMan;
        TaskPlanner taskPlanner;

        public CmdFindHuman(STPCommandManager cmdMan, TaskPlanner taskPlanner)
            : base("find_human")
        {
            this.cmdMan = cmdMan;
            this.taskPlanner = taskPlanner;
        }

		protected override Response AsyncTask(Command command)
		{
            TBWriter.Spaced(" >>>  Received  " + command.CommandName + " , received = [" + command.Parameters + "]");

			Human personFounded;

			string[] param = command.Parameters.Split(null as char[], StringSplitOptions.RemoveEmptyEntries);

			string humanName;
			string devices;

			if (param == null)
			{
				humanName = "human";
				devices = "";
			}
			else if (param.Length == 1)
			{
				humanName = param[0];
				devices = "";
			}
			else
			{
				humanName = param[0];
				devices = command.Parameters;
			}

			bool success = taskPlanner.Cmd_FindHuman(humanName, devices, out personFounded);

			if (success)
				command.Parameters = personFounded.Name;


            TBWriter.Spaced("      Terminated  " + command.CommandName + " , sending = [" + command.Parameters + "]  <<<");
			
            return Response.CreateFromCommand(command, success);
		}

        public override void DefaultParameterParser(string[] parameters)
        {
            return;
        }
    }
}
