using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.API;
using Robotics.Mathematics;

namespace SimpleTaskPlanner
{
	// find_cubes ( Y para abajo , en mts todo ) 
	// Parametros:
	//		"all"	- color X Y Z color2 X Y Z 
	//		"color" - color X Y Z
	//		"cubestable right/left" - cubestable X Y Z

	class CmdDetectCubes:AsyncCommandExecuter
	{
		STPCommandManager cmdMan;
		TaskPlanner taskPln;

		public CmdDetectCubes(STPCommandManager cmdMan, TaskPlanner taskPln)
			: base("detectcubes")
		{
			this.cmdMan = cmdMan;
			this.taskPln = taskPln;
		}

		protected override Response AsyncTask(Command command)
		{
			string parameters = command.Parameters;
			bool success;

			TBWriter.Info1("New Command {detectcubes} , parameters{" + parameters + "}. Parsing parameters...");

			if (parameters.Contains("all"))
			{
				TBWriter.Info1("Received detectcubes(all)...");

				List<WorldObject> cubesInVisionCoor;
				List<WorldObject> cubesInRobotCoor = new List<WorldObject>();

				if(! this.cmdMan.HEAD_lookat(0.00, -1.1, 5000) )
				{
					this.cmdMan.HEAD_lookat(0.00, 0.00, 5000);
					if (!this.cmdMan.HEAD_lookat(0.00, -1.1, 5000))
					{
						TBWriter.Error("CANT MOVE HEAD... RETURNING FALSE");
                        return Response.CreateFromCommand(command, false);
					}
				}

				if (!this.cmdMan.OBJ_FND_findcubes(out cubesInVisionCoor))
					success = false;
				else
				{
					success = true;

					this.taskPln.ActualizeHeadPos();
					foreach (WorldObject o in cubesInVisionCoor)
					{
						Vector3 robCoor = this.taskPln.robot.TransHeadKinect2Robot(o.Position);
						cubesInRobotCoor.Add(new WorldObject(o.Name, robCoor));
					}
				}

				string resp = "";
				foreach (WorldObject o in cubesInRobotCoor)
					resp += o.Name + " " + o.Position.X + " " + o.Position.Y + " " + o.Position.Z + " ";

				TBWriter.Info1("Sending detectcubes(all) => Success={" + success + "} , params={" + resp + "}");

				command.Parameters = resp;
				return Response.CreateFromCommand(command, success);
			}
			else if (parameters.Contains("cubestable"))
			{
				string direction;
				if (parameters.Contains("left"))
					direction = "left";
				else
					direction = "right";

				TBWriter.Info1("Received detectcubes(cubestable , " + direction + ") ...");

				WorldObject emptyPoint;
				Vector3 emptyInRobotCoor;
				if (!this.cmdMan.OBj_FND_findcubesEmptyPoint(direction, out emptyPoint))
					success = false;
				else
				{
					this.taskPln.ActualizeHeadPos();
					emptyInRobotCoor = this.taskPln.robot.TransHeadKinect2Robot(emptyPoint.Position);
					command.Parameters = "cubestable " + emptyInRobotCoor.X + " " + emptyInRobotCoor.Y + " " + emptyInRobotCoor.Z;
					success = true;
				}

                TBWriter.Info1("Sending detectcubes(cubestable , " + direction + ") => Success={" + success + "} , params={" + command.Parameters + "}");

				return Response.CreateFromCommand(command, success);
			}
			else
			{
				TBWriter.Info1("Received detectcubes(color=" + parameters + ")...");

				WorldObject cube;
				if (!this.cmdMan.OBJ_FND_findcubes(parameters, out cube))
					success = false;
				else
				{
					this.taskPln.ActualizeHeadPos();
					Vector3 cubeInRobotCoor = this.taskPln.robot.TransHeadKinect2Robot(cube.Position);
					command.Parameters = cube.Name + " " + cubeInRobotCoor.X + " " + cubeInRobotCoor.Y + " " + cubeInRobotCoor.Z;
					success = true;
				}

                TBWriter.Info1("Sending detectcubes(color) => Success={" + success + "} , params={" + command.Parameters + "}");
				return Response.CreateFromCommand(command, success);
			}
		}

		public override void DefaultParameterParser(string[] parameters)
		{
			throw new NotImplementedException();
		}

		public override bool ParametersRequired
		{
			get
			{
				return false;
			}
		}
	}
}
