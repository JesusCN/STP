using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.API;
using Robotics.Controls;


namespace SimpleTaskPlanner
{
    class CmdDoTechnicalChallenge : AsyncCommandExecuter
    {
        STPCommandManager cmdMan;
        TaskPlanner taskPlanner;

        public CmdDoTechnicalChallenge(STPCommandManager cmdMan, TaskPlanner taskPlanner)
            : base("dotechnicalchallenge")
        {
            this.cmdMan = cmdMan;
            this.taskPlanner = taskPlanner;
        }

        public override bool ParametersRequired
        {
            get
            {
                return false;
            }
        }
        protected override Response AsyncTask(Command command)
        {

            TBWriter.Spaced(" >>>  Received  " + command.CommandName + " , received = [" + command.Parameters + "]");


            bool success = taskPlanner.Cmd_DoTechnicalChallenge();


            TBWriter.Spaced("      Terminated  " + command.CommandName + " , sending = [" + command.Parameters + "]  <<<");

            return Response.CreateFromCommand(command, success);
        }

        public override void DefaultParameterParser(string[] parameters)
        {
            return;
        }
    }
}
