using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.API;
using Robotics.Controls;

namespace SimpleTaskPlanner
{
    class CmdAligneEdge : AsyncCommandExecuter
    {
        STPCommandManager cmdMan;
        TaskPlanner taskPlanner;

        public CmdAligneEdge(STPCommandManager cmdMan, TaskPlanner taskPlanner)
            :base ("aligneedge")
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
            double[] desireParams;

            TBWriter.Spaced(TBWriter.time() + "    ===>>>   COMMAND RECEIVED : " + command.CommandName + " ; Params{" + command.Parameters + "}");

            if (!Parse.string2doubleArray(command.Parameters, out desireParams))
            {
                desireParams = new double[2];
                desireParams[0] = 0.0;
                desireParams[1] = 0.15;
            }

            result = taskPlanner.Cmd_AlignWithEdge(desireParams[0], desireParams[1]);

            TBWriter.Spaced(TBWriter.time() + "    <<<===   COMMAND TERMINATED : " + command.CommandName + " ; Result{" + result + "} Params{" + command.Parameters + "}");
            return Response.CreateFromCommand(command, result);
        }

        public override void DefaultParameterParser(string[] parameters)
        {
            return;
        }
    }
}
