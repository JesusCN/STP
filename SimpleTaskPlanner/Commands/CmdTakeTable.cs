using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.API;
using Robotics.Controls;

namespace SimpleTaskPlanner
{
	class CmdTakeTable : AsyncCommandExecuter
	{
		STPCommandManager cmdMan;
		TaskPlanner taskPlanner;

		public CmdTakeTable(STPCommandManager cmdMan, TaskPlanner taskPlanner)
			: base("taketable")
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
			bool result=false;
			
			TBWriter.Spaced(TBWriter.time() + "    ===>>>   COMMAND RECEIVED : " + command.CommandName);
	

			 result = taskPlanner.Cmd_TakeTable();

			TBWriter.Spaced(TBWriter.time() + "    <<<===   COMMAND TERMINATED : " + command.CommandName + " ; Result{" + result + "}" );
			return Response.CreateFromCommand(command, result);
		}

		public override void DefaultParameterParser(string[] parameters)
		{
			return;
		}
	}
}

