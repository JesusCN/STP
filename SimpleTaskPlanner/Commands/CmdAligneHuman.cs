using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.API;
using Robotics.Controls;

namespace SimpleTaskPlanner
{
	class CmdAligneHuman : AsyncCommandExecuter
	{
		STPCommandManager cmdMan;
		TaskPlanner taskPlanner;

		public CmdAligneHuman(STPCommandManager cmdMan, TaskPlanner taskPlanner)
			: base("align_human")
		{
			this.cmdMan = cmdMan;
			this.taskPlanner = taskPlanner;
		}

		public override bool ParametersRequired
		{
            get { return false; }
		}

        protected override Response AsyncTask(Command command)
        {
            TBWriter.Spaced( TBWriter.time() + "   >>>   Received  " + command.CommandName + " , received = [" + command.Parameters + "]");


            bool success = taskPlanner.Cmd_AligneHuman();
          
            
            TBWriter.Spaced(TBWriter.time() + "         Terminated  " + command.CommandName + " , sending = [" + command.Parameters + "]  <<<");
            
            return Response.CreateFromCommand(command, success);
        }

		public override void DefaultParameterParser(string[] parameters)
		{
			return;
		}
	}
}
