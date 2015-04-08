using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.API;


namespace SimpleTaskPlanner
{
    class CmdStartLearn : SyncCommandExecuter
    {
        STPCommandManager cmdMan;
        TaskPlanner taskPlanner;

        public CmdStartLearn(STPCommandManager cmdMan, TaskPlanner taskPlanner)
            : base("startlearn")
        {
            this.cmdMan = cmdMan;
            this.taskPlanner = taskPlanner;
        }

        protected override Response SyncTask(Command command)
        {
            TBWriter.Spaced(" >>>  Received  " + command.CommandName + " , received = [" + command.Parameters + "]");

            string[] cmdinfo = command.Parameters.Split(null as char[], StringSplitOptions.RemoveEmptyEntries);

            if (cmdinfo.Length < 1)
            {
                TBWriter.Error("Invalid parameters ");
                TBWriter.Spaced("      Terminated  " + command.CommandName + " , sending = [" + command.Parameters + "]  <<<");
                return Response.CreateFromCommand(command, false);
            }
            
            string motionLearningName = cmdinfo[0];

            this.taskPlanner.Cmd_StartLearn(motionLearningName);

            
            TBWriter.Spaced("      Terminated  " + command.CommandName + " , sending = [" + command.Parameters + "]  <<<");

            return Response.CreateFromCommand(command, true);
        }
    }
}
