using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.API;
using Robotics.Controls;

namespace SimpleTaskPlanner
{
	class CmdTakeTray: AsyncCommandExecuter
	{
		STPCommandManager cmdMan;
        TaskPlanner taskPlanner;

        public CmdTakeTray(STPCommandManager cmdMan, TaskPlanner taskPlanner)
            :base ("taketray")
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
            bool result;
            TBWriter.Spaced(TBWriter.time() + "    ===>>>   COMMAND RECEIVED : " + command.CommandName + " ; Params{" + command.Parameters + "}");

			           
            result = taskPlanner.Cmd_TakeTray(double.Parse(command.Parameters));

            TBWriter.Spaced(TBWriter.time() + "    <<<===   COMMAND TERMINATED : " + command.CommandName + " ; Result{" + result + "} Params{" + command.Parameters + "}");
            return Response.CreateFromCommand(command, result);
        }

        public override void DefaultParameterParser(string[] parameters)
        {
            return;
        }
	}
}
