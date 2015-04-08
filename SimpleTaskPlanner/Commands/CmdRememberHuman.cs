using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.API;

namespace SimpleTaskPlanner
{
public	class CmdRememberHuman: AsyncCommandExecuter
	{
		CommandManager cmdMan;
		TaskPlanner taskPlanner;

		public CmdRememberHuman(STPCommandManager cmdMan, TaskPlanner taskPlanner)
			: base("remember_human")
		{
			this.cmdMan = cmdMan;
			this.taskPlanner = taskPlanner;
		}

        protected override Response AsyncTask(Command command)
        {
            TBWriter.Spaced(" >>>  Received  " + command.CommandName + " , received = [" + command.Parameters + "]");


            bool success = taskPlanner.Cmd_RememberHuman(command.Parameters);


            TBWriter.Spaced("      Terminated  " + command.CommandName + " , sending = [" + command.Parameters + "]  <<<");

            return Response.CreateFromCommand(command, success);
        }

		public override void DefaultParameterParser(string[] parameters)
		{
			return;
		}
	}
}
