using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.API;

namespace SimpleTaskPlanner
{
    public class CmdTakeHandOver : AsyncCommandExecuter
    {
        STPCommandManager cmdMan;
        TaskPlanner taskPlanner;

        public CmdTakeHandOver(STPCommandManager cmdMan, TaskPlanner taskPlanner)
            : base("takehandover")
        {
            this.cmdMan = cmdMan;
            this.taskPlanner = taskPlanner;
        }

        protected override Response AsyncTask(Command command)
        {
            TBWriter.Spaced(" >>>  Received  " + command.CommandName + " , received = [" + command.Parameters + "]");

            string obj1 = "";
            string obj2 = "";

            string[] parameters = command.Parameters.Split(null as char[], StringSplitOptions.RemoveEmptyEntries);

            if (parameters.Length < 1)
            {
                TBWriter.Error("Invalid parmeters");
                return Response.CreateFromCommand(command, false);
            }

            if (parameters.Length == 1)
            {
                obj1 = parameters[0];
            }

            if (parameters.Length == 2)
            {
                obj1 = parameters[0];
                obj2 = parameters[1];
            }

            bool success = taskPlanner.Cmd_TakeHandOver(obj1, obj2);

            TBWriter.Spaced("      Terminated  " + command.CommandName + " , sending = [" + command.Parameters + "]  <<<");

            return Response.CreateFromCommand(command, success);
        }

        public override void DefaultParameterParser(string[] parameters)
        {
            return;
        }
    }
}
