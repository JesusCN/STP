using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.API;

namespace SimpleTaskPlanner
{
    class CmdTakeFromShelf : AsyncCommandExecuter
    {
        STPCommandManager cmdMan;
        TaskPlanner taskPlanner;

        public CmdTakeFromShelf(STPCommandManager cmdMan, TaskPlanner taskPlanner)
            : base("takefromshelf")
        {
            this.cmdMan = cmdMan;
            this.taskPlanner = taskPlanner;
        }

        protected override Response AsyncTask(Command command)
        {
            TBWriter.Spaced(" >>>  Received  " + command.CommandName + " , received = [" + command.Parameters + "]");
            

            string[] info = command.Parameters.Split(null as char[], StringSplitOptions.RemoveEmptyEntries);


            if ((info == null) || (info.Length == 0))
                return Response.CreateFromCommand(command, false);


            string obj1 = "";
            string obj2 = "";
            string armSelection = "";
            string objectsTaked = "";


            obj1 = info[0];

            if ((info.Length > 1) && (info[1] != "left") && (info[1] != "right"))
                obj2 = info[1];
            else
                obj2 = "";

            if (command.Parameters.Contains("right"))
                armSelection = "right";
            else if (command.Parameters.Contains("left"))
                armSelection = "left";
            else
                armSelection = "";

            if (taskPlanner.Cmd_TakeObjFromShelf(obj1, obj2, armSelection, out objectsTaked))
            {
                command.Parameters = objectsTaked;
                return Response.CreateFromCommand(command, true);

                TBWriter.Spaced("      Terminated  " + command.CommandName + " , sending = [" + command.Parameters + "]  <<<");
            }
            else
            {
                command.Parameters = objectsTaked;
                return Response.CreateFromCommand(command, false);

                TBWriter.Spaced("      Terminated  " + command.CommandName + " , sending = [" + command.Parameters + "]  <<<");
            }
        }

        public override void DefaultParameterParser(string[] parameters)
        {
            return;
        }
    }
}