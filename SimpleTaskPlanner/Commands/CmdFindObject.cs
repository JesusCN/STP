using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.API;
using System.Threading;

namespace SimpleTaskPlanner
{
	public class CmdFindObject : AsyncCommandExecuter
	{
		STPCommandManager cmdMan;
		TaskPlanner taskPlanner;

		public CmdFindObject(STPCommandManager cmdMan, TaskPlanner taskPlanner)
			: base("find_object")
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
            TBWriter.Info1("----------------------------------------------");
            TBWriter.Spaced(" >>>  Received  " + command.CommandName + " , received = [" + command.Parameters + "]");

			bool succes;
			WorldObject[] objectsFounded;

            if (!taskPlanner.Cmd_FindObject(out objectsFounded))
            {
                TBWriter.Spaced("      Terminated  " + command.CommandName + " , sending = [" + command.Parameters + "]  <<<");
                TBWriter.Info1("----------------------------------------------");
                return Response.CreateFromCommand(command, false);
            }
            if (objectsFounded == null)
            {
                TBWriter.Spaced("      Terminated  " + command.CommandName + " , sending = [" + command.Parameters + "]  <<<");
                TBWriter.Info1("----------------------------------------------");
                return Response.CreateFromCommand(command, false);
            }
			string objectsList = "";

			foreach (WorldObject obj in objectsFounded)
			{
				objectsList += obj.Name + " ";
			}

			command.Parameters = objectsList;

            TBWriter.Spaced("      Terminated  " + command.CommandName + " , sending = [" + command.Parameters + "]  <<<");
            TBWriter.Info1("----------------------------------------------");
			
            return Response.CreateFromCommand(command, true);
		}

		public override void DefaultParameterParser(string[] parameters)
		{
			return;
		}
	}
}
