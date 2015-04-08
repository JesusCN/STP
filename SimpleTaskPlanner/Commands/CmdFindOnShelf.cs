using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Robotics.API;
using System.Threading;

namespace SimpleTaskPlanner
{
    class CmdFindOnShelf:AsyncCommandExecuter
    {
		STPCommandManager cmdMan;
		TaskPlanner taskPlanner;

        public CmdFindOnShelf(STPCommandManager cmdMan, TaskPlanner taskPlanner)
            : base("findonshelf")
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

			bool succes;
			string[] objectsFounded;

			if (!taskPlanner.Cmd_FindObjOnShelf(out objectsFounded))
				return Response.CreateFromCommand(command, false);

			if (objectsFounded == null)
				return Response.CreateFromCommand(command, false);

			string objectsList = "";

			foreach (string obj in objectsFounded)
			{
				objectsList += obj + " ";
			}

			command.Parameters = objectsList;

            TBWriter.Spaced("      Terminated  " + command.CommandName + " , sending = [" + command.Parameters + "]  <<<");

			return Response.CreateFromCommand(command, true);
		}

		public override void DefaultParameterParser(string[] parameters)
		{
			return;
		}
	}
}
