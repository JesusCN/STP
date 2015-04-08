using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.API;

namespace SimpleTaskPlanner
{
    class CmdTakeHumandHands: AsyncCommandExecuter
    {
        STPCommandManager cmdMan;
        TaskPlanner taskplanner;

        public CmdTakeHumandHands(STPCommandManager cmdMan, TaskPlanner taskplanner)
            :base("takehumanhands")
        {
            this.cmdMan = cmdMan;
            this.taskplanner = taskplanner;
        }

        public override bool ParametersRequired
        {
            get { return false; }
        }

        protected override Response AsyncTask(Command command)
        {
            TBWriter.Spaced(" >>>  Received  " + command.CommandName + " , received = [" + command.Parameters + "]");


            bool success = taskplanner.Cmd_TakeHumanHands();


            TBWriter.Spaced("      Terminated  " + command.CommandName + " , sending = [" + command.Parameters + "]  <<<");

            return Response.CreateFromCommand(command, success);
        }

        public override void DefaultParameterParser(string[] parameters)
        {
            return;
        }
    }
}
