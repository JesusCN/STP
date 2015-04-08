using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.API;

namespace SimpleTaskPlanner
{
  public  class CmdExecuteLearn: AsyncCommandExecuter
    {
        TaskPlanner taskPlanner;
        STPCommandManager cmdMan;

        public CmdExecuteLearn(STPCommandManager cmdMan, TaskPlanner taskPlanner)
            : base("executelearned")
        {
            this.taskPlanner = taskPlanner;
            this.cmdMan = cmdMan;
        }

        protected override Response AsyncTask(Command command)
        {
            TBWriter.Info1("----------------------------------------------");
            TBWriter.Spaced(" >>>>>>  Received  " + command.CommandName + " , received = [" + command.Parameters + "]");


            string[] info = command.Parameters.Split(null as char[], StringSplitOptions.RemoveEmptyEntries);

            if( info.Length < 2) 
            { 
                TBWriter.Error("Cant execute CmdExecuteLearn, invalid parameters");
                
                TBWriter.Spaced("      Terminated  " + command.CommandName + " , sending = [" + command.Parameters + "]  <<<");
                TBWriter.Info1("----------------------------------------------");
                
                return Response.CreateFromCommand(command, false);

            }

            bool success = this.taskPlanner.Cmd_ExecuteLearn(info[0], info[1]);

            
            TBWriter.Spaced("      Terminated  " + command.CommandName + " , sending = [" + command.Parameters + "]  <<<");
            TBWriter.Info1("----------------------------------------------");
            return Response.CreateFromCommand(command, success);
        }

        public override void DefaultParameterParser(string[] parameters)
        {
         return;
        }
    }
}
