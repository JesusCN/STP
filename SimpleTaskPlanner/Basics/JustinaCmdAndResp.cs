using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.API;

namespace SimpleTaskPlanner
{
	public class JustinaCmdAndResp
	{
		private Command command;
		private Response response;
		private bool isResposeReceived;

		public JustinaCmdAndResp(string cmdName)
		{   
			this.command = new Command(cmdName, "");
			this.response = Response.CreateFromCommand(this.command, false);
			this.isResposeReceived = false;
		}

		public Command Command
		{
            get { return this.command; }
			set { this.command = value; }
		}

		public Response Response
		{
			get { return this.response; }
			set { this.response = value; }
		}

		public bool IsResponseReceived
		{
			get { return this.isResposeReceived; }
			set { this.isResposeReceived = value; }
		}
	}
}
