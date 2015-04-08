using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.API;

namespace SimpleTaskPlanner
{
    class CmdHolding: AsyncCommandExecuter
    {
        TaskPlanner taskPlanner;
        STPCommandManager cmdMan;

        public CmdHolding( STPCommandManager cmdMan, TaskPlanner taskPLanner)
           : base("holding")
        {
            this.taskPlanner = taskPLanner;
            this.cmdMan = cmdMan;
        }

        public override bool ParametersRequired
        {
            get { return false; }
        }

        protected override Response AsyncTask(Command command)
        {
            TBWriter.Spaced(" >>>  Received  " + command.CommandName + " , received = [" + command.Parameters + "]");

            string holdinginfo = "";

            if (!this.taskPlanner.Cmd_Holding(out holdinginfo))
                return Response.CreateFromCommand(command, false);

            command.Parameters = holdinginfo;

            TBWriter.Spaced("      Terminated  " + command.CommandName + " , sending = [" + command.Parameters + "]  <<<");
            
            return Response.CreateFromCommand(command, true);
        }

        public override void DefaultParameterParser(string[] parameters)
        {
            throw new NotImplementedException();
        }
    }
}
