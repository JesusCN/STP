using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.API;


namespace SimpleTaskPlanner
{
    class CmdDrop: AsyncCommandExecuter
    {
        STPCommandManager cmdMan;
        TaskPlanner taskPlanner;

        public CmdDrop(STPCommandManager cmdMan, TaskPlanner taskPlanner)
           : base("drop")
        {
            this.cmdMan = cmdMan;
            this.taskPlanner = taskPlanner;
        }

        public override bool ParametersRequired
        {
            get { return true; }
        }

		protected override Response AsyncTask(Command command)
		{
			TBWriter.Spaced(" >>>  Received  " + command.CommandName + " , received = [" + command.Parameters + "]");

			bool success = taskPlanner.Cmd_Drop(command.Parameters);

			TBWriter.Spaced("      Terminated  " + command.CommandName + " , sending = [" + command.Parameters + "]  <<<");

			return Response.CreateFromCommand(command, success);
		}
        public override void DefaultParameterParser(string[] parameters)
        {
            return;
        }
    }
}
