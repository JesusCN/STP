using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.API;

namespace SimpleTaskPlanner
{
    public class CmdPointAtObject : AsyncCommandExecuter
    {
        STPCommandManager cmdMan;
        TaskPlanner taskPlanner;

        public CmdPointAtObject(STPCommandManager cmdMan, TaskPlanner taskPlanner)
            : base("pointatobject")
        {
            this.cmdMan = cmdMan;
            this.taskPlanner = taskPlanner;
        }

        protected override Response AsyncTask(Command command)
        {
            bool success;
            string objToPoint;

            TBWriter.Spaced(" >>>  Received  " + command.CommandName + " , received = [" + command.Parameters + "]");

            objToPoint = command.Parameters;

            success = taskPlanner.Cmd_PointAtObject(objToPoint);

            if (success)
                TBWriter.Info1(command.CommandName + " succesfully executed");
            else
                TBWriter.Info1(command.CommandName + " execution failed");

            TBWriter.Spaced("      Terminated  " + command.CommandName + " , sending = [" + command.Parameters + "]  <<<");

            return Response.CreateFromCommand(command, success);

        }

        public override void DefaultParameterParser(string[] parameters)
        {
            throw new NotImplementedException();
        }
    }
}