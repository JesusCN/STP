using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.API;

namespace SimpleTaskPlanner
{
	public class CmdDeliverObject: AsyncCommandExecuter
	{
		STPCommandManager cmdMan;
		TaskPlanner taskPlanner;

		public CmdDeliverObject(STPCommandManager cmdMan, TaskPlanner taskPlanner)
			:base("deliverobject")
		{
			this.cmdMan = cmdMan;
			this.taskPlanner = taskPlanner;
		}

        protected override Response AsyncTask(Command command)
        {
            TBWriter.Spaced(" >>>  Received  " + command.CommandName + " , received = [" + command.Parameters + "]");

            if (string.IsNullOrEmpty(command.Parameters))
            {
                TBWriter.Error("Command Parameters is null or empty");
                return Response.CreateFromCommand(command, false);
            }

            bool success = taskPlanner.Cmd_DeliverObject(command.Parameters);
        
            TBWriter.Spaced("      Terminated  " + command.CommandName + " , sending = [" + command.Parameters + "]  <<<");

            return Response.CreateFromCommand(command, success);
        }

		public override void DefaultParameterParser(string[] parameters)
		{
			return;
		}
	}
}
