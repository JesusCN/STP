using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.API;

namespace SimpleTaskPlanner
{
    class CmdStopLearn: SyncCommandExecuter
    {
        STPCommandManager cmdMan;
        TaskPlanner taskPlanner;

        
        public CmdStopLearn(STPCommandManager cmdMan, TaskPlanner taskPlanner)
            : base("stoplearn")
        {
            this.cmdMan = cmdMan;
            this.taskPlanner = taskPlanner;
        }

        public override bool ParametersRequired
        {
            get { return false; }
        }

        protected override Response SyncTask(Command command)
        {
            TBWriter.Spaced(" >>>  Received  " + command.CommandName + " , received = [" + command.Parameters + "]");

            this.taskPlanner.Cmd_StopLearn();
            return Response.CreateFromCommand(command, true);

            TBWriter.Spaced("      Terminated  " + command.CommandName + " , sending = [" + command.Parameters + "]  <<<");
        }

      
    }
}
