using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.API;


namespace SimpleTaskPlanner
{
	class CmdAutolocalization: SyncCommandExecuter
	{
		STPCommandManager cmdMan;
        TaskPlanner taskPlanner;

		public CmdAutolocalization(STPCommandManager cmdMan, TaskPlanner taskPlanner)
            : base("autolocalization")
        {
            this.cmdMan = cmdMan;
            this.taskPlanner = taskPlanner;
        }

        public override bool ParametersRequired
        { get { return true; } }


        protected override Response SyncTask(Command command)
        {
            TBWriter.Spaced(" >>>  Received  " + command.CommandName + " , received = [" + command.Parameters + "]");


            bool autoLocalization;

            //if (command.Parameters == "enable")
            //{
            //    taskPlanner.AutoLocalization = true;
            //    return Response.CreateFromCommand(command, taskPlanner.AutoLocalization);
            //}
            //if (command.Parameters == "disable")
            //{
            //    taskPlanner.AutoLocalization = false;
            //    return Response.CreateFromCommand(command, taskPlanner.AutoLocalization);
            //}
         

            TBWriter.Spaced("      Terminated  " + command.CommandName + " , sending = [" + command.Parameters + "]  <<<");
            
            return Response.CreateFromCommand(command, false);
        }
	}
}
