﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using Robotics.API;
using Robotics.Controls;
using Robotics.Mathematics;

namespace SimpleTaskPlanner
{
	public enum JustinaCommands
	{
		BLK_alive, BLK_ready, BLK_busy,

		ARMS_arms_goto,
		ARMS_ra_goto, ARMS_ra_abspos, ARMS_ra_articularPos, ARMS_ra_orientation, ARMS_ra_opengrip, ARMS_ra_closegrip,
		ARMS_ra_getgripstatus, ARMS_ra_torque, ARMS_ra_servotorqueon, ARMS_ra_relpos, ARMS_ra_move, ARMS_ra_reachable,
		ARM_ra_torque, ARMS_ra_hand, ARMS_ra_handGoto,

		ARMS_la_goto, ARMS_la_abspos, ARMS_la_articularPos, ARMS_la_orientation, ARMS_la_opengrip, ARMS_la_closegrip,
		ARMS_la_getgripstatus, ARMS_la_torque, ARMS_la_servotorqueon, ARMS_la_relpos, ARMS_la_move, ARMS_la_reachable,
		ARM_la_torque,

		HEAD_lookat, HEAD_show, HEAD_search, HEAD_stop, HEAD_lookatloop, HEAD_lookatrel, HEAD_lookatobject, HEAD_getPosition,

		MVN_PLN_pause, MVN_PLN_move, MVN_PLN_goto, MVN_PLN_addobject, MVN_PLN_obstacle, MVN_PLN_position,
		MVN_PLN_enablelaser, MVN_PLN_disablelaser, MVN_PLN_getclose, MVN_PLN_go_to_room, MVN_PLN_go_to_region,
		MVN_PLN_mv, MVN_PLN_ic, MVN_PLN_stop, MVN_PLN_setspeeds, MVN_PLN_report, MVN_PLN_getpos, MVN_PLN_lectures,
		MVN_PLN_startfollowhuman, MVN_PLN_stopfollowhuman, MVN_PLN_selfterminate,

		SENSORS_start, SENSORS_stop,

		TRS_relpos, TRS_abspos, TRS_getPosition,

		SP_GEN_say, SP_GEN_asay, SP_GEN_play, SP_GEN_aplay, SP_GEN_read, SP_GEN_aread, SP_GEN_playloop, SP_GEN_shutup,
		SP_GEN_voice,

		SP_REC_na, SP_REC_status, SP_REC_grammar, SP_REC_words,

		PRS_FND_find, PRS_FND_remember, PRS_FND_auto, PRS_FND_sleep, PRS_FND_shutdown,

		OBJ_FND_findOnTable, OBJ_FND_removeTable, OBJ_FND_find, OBJ_FND_findShelfPlanes, OBJ_FND_findObjectsOnshelf,
		OBJ_FND_findHumanHands, OBJ_FND_findEdge, OBJ_FND_setnode, OBJ_FND_getnode, OBJ_FND_saveMap, OBJ_FND_loadMap,
		OBJ_FND_remember, OBJ_FNDT_findedgetuned, OBJ_FNDT_findedgefastandfurious, OBJ_FNDT_findedgereturns, OBJ_FND_findCubes,

		ST_PLN_alignhuman, ST_PLN_cleanplane, ST_PLN_comewith, ST_PLN_dopresentation, ST_PLN_drop, ST_PLN_findhuman,
		ST_PLN_findhumanobject, ST_PLN_findobject, ST_PLN_findtable, ST_PLN_grab, ST_PLN_greethuman, ST_PLN_release,
		ST_PLN_rememberhuman, ST_PLN_searchobject, ST_PLN_seehand, ST_PLN_seeobject, ST_PLN_shutdown, ST_PLN_take,
		ST_PLN_deliverObject, ST_PLN_findonshelf, ST_PLN_takefromShelf, ST_PLN_holding, ST_PLN_executelearned, ST_PLN_startlearn,
		ST_PLN_stoplearn, ST_PLN_takehandover, ST_PLN_takehumanhands, alignTable, ST_PLN_aligneEdge
	}

	public enum RegionType { Room, Region, Location }

	public class STPCommandManager : CommandManager
	{
		private const int TotalExistingCommands = 200;
		private JustinaCmdAndResp[] justinaCmdAndResp;
		private SortedList<string, JustinaCmdAndResp> sortedCmdAndResp;

		Random wait;

		public STPCommandManager()
			: base()
		{
			//this.status = status;
			this.ResponseReceived += new ResponseReceivedEventHandler(HAL9000CmdMan_ResponseReceived);
			this.justinaCmdAndResp = new JustinaCmdAndResp[TotalExistingCommands];
			this.sortedCmdAndResp = new SortedList<string, JustinaCmdAndResp>();
			this.LoadJustinaCommands();

			//int i = 0;

			//this.

			wait = new Random();
		}

		private void LoadJustinaCommands()
		{
			#region Commands for BLK
			this.justinaCmdAndResp[(int)JustinaCommands.BLK_ready] = new JustinaCmdAndResp("ready");
			this.justinaCmdAndResp[(int)JustinaCommands.BLK_alive] = new JustinaCmdAndResp("alive");
			this.justinaCmdAndResp[(int)JustinaCommands.BLK_busy] = new JustinaCmdAndResp("busy");
			#endregion
			#region Commands for ARMS
			this.justinaCmdAndResp[(int)JustinaCommands.ARMS_ra_abspos] = new JustinaCmdAndResp("ra_abspos");
			this.justinaCmdAndResp[(int)JustinaCommands.ARMS_ra_articularPos] = new JustinaCmdAndResp("ra_artpos");
			this.justinaCmdAndResp[(int)JustinaCommands.ARMS_ra_closegrip] = new JustinaCmdAndResp("ra_closegrip");
			this.justinaCmdAndResp[(int)JustinaCommands.ARMS_ra_goto] = new JustinaCmdAndResp("ra_goto");
			this.justinaCmdAndResp[(int)JustinaCommands.ARMS_ra_move] = new JustinaCmdAndResp("ra_move");
			this.justinaCmdAndResp[(int)JustinaCommands.ARMS_ra_opengrip] = new JustinaCmdAndResp("ra_opengrip");
			this.justinaCmdAndResp[(int)JustinaCommands.ARMS_ra_relpos] = new JustinaCmdAndResp("ra_relpos");
			this.justinaCmdAndResp[(int)JustinaCommands.ARMS_ra_reachable] = new JustinaCmdAndResp("ra_reachable");
			this.justinaCmdAndResp[(int)JustinaCommands.ARMS_ra_torque] = new JustinaCmdAndResp("ra_torque");

			this.justinaCmdAndResp[(int)JustinaCommands.ARMS_la_abspos] = new JustinaCmdAndResp("la_abspos");
			this.justinaCmdAndResp[(int)JustinaCommands.ARMS_la_articularPos] = new JustinaCmdAndResp("la_artpos");
			this.justinaCmdAndResp[(int)JustinaCommands.ARMS_la_closegrip] = new JustinaCmdAndResp("la_closegrip");
			this.justinaCmdAndResp[(int)JustinaCommands.ARMS_la_goto] = new JustinaCmdAndResp("la_goto");
			this.justinaCmdAndResp[(int)JustinaCommands.ARMS_la_move] = new JustinaCmdAndResp("la_move");
			this.justinaCmdAndResp[(int)JustinaCommands.ARMS_la_opengrip] = new JustinaCmdAndResp("la_opengrip");
			this.justinaCmdAndResp[(int)JustinaCommands.ARMS_la_relpos] = new JustinaCmdAndResp("la_relpos");
			this.justinaCmdAndResp[(int)JustinaCommands.ARMS_la_reachable] = new JustinaCmdAndResp("la_reachable");
			this.justinaCmdAndResp[(int)JustinaCommands.ARMS_la_torque] = new JustinaCmdAndResp("la_torque");
			this.justinaCmdAndResp[(int)JustinaCommands.ARMS_arms_goto] = new JustinaCmdAndResp("arms_goto");
            this.justinaCmdAndResp[(int)JustinaCommands.ARMS_ra_hand] = new JustinaCmdAndResp("ra_hand");
            this.justinaCmdAndResp[(int)JustinaCommands.ARMS_ra_handGoto] = new JustinaCmdAndResp("ra_hand_move");
			//this.justinaCmdAndResp[(int)JustinaCommands.ARMS_la_goto] = new JustinaCmdAndResp("la_goto");
			//this.justinaCmdAndResp[(int)JustinaCommands.ARMS_la_abspos] = new JustinaCmdAndResp("la_abspos");
			//this.justinaCmdAndResp[(int)JustinaCommands.ARMS_la_articularPos] = new JustinaCmdAndResp("la_artpos");

			#endregion
			#region Commands for HEAD
			this.justinaCmdAndResp[(int)JustinaCommands.HEAD_lookat] = new JustinaCmdAndResp("hd_lookat");
			this.justinaCmdAndResp[(int)JustinaCommands.HEAD_search] = new JustinaCmdAndResp("hd_search");
			this.justinaCmdAndResp[(int)JustinaCommands.HEAD_show] = new JustinaCmdAndResp("hd_show");
			this.justinaCmdAndResp[(int)JustinaCommands.HEAD_lookatobject] = new JustinaCmdAndResp("hd_lookatobject");
			this.justinaCmdAndResp[(int)JustinaCommands.HEAD_lookatrel] = new JustinaCmdAndResp("hd_lookatrel");
			#endregion
			#region Commands for MVN-PLN
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_getclose] = new JustinaCmdAndResp("mp_getclose");
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_goto] = new JustinaCmdAndResp("mp_goto");
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_move] = new JustinaCmdAndResp("mp_move");
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_position] = new JustinaCmdAndResp("mp_position");
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_report] = new JustinaCmdAndResp("mp_report");
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_startfollowhuman] = new JustinaCmdAndResp("mp_startfollowhuman");
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_stopfollowhuman] = new JustinaCmdAndResp("mp_stopfollowhuman");
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_pause] = new JustinaCmdAndResp("mp_pause");
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_obstacle] = new JustinaCmdAndResp("mp_obstacle");
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_setspeeds] = new JustinaCmdAndResp("mp_setspeeds");
			#endregion
			#region Commands for TRS
			this.justinaCmdAndResp[(int)JustinaCommands.TRS_relpos] = new JustinaCmdAndResp("trs_relpos");
			this.justinaCmdAndResp[(int)JustinaCommands.TRS_abspos] = new JustinaCmdAndResp("trs_abspos");
			#endregion
			#region Commands for PRS-FND
			this.justinaCmdAndResp[(int)JustinaCommands.PRS_FND_find] = new JustinaCmdAndResp("pf_find");
			this.justinaCmdAndResp[(int)JustinaCommands.PRS_FND_remember] = new JustinaCmdAndResp("pf_remember");
			this.justinaCmdAndResp[(int)JustinaCommands.PRS_FND_auto] = new JustinaCmdAndResp("pf_auto");
			this.justinaCmdAndResp[(int)JustinaCommands.PRS_FND_sleep] = new JustinaCmdAndResp("pf_sleep");
			this.justinaCmdAndResp[(int)JustinaCommands.PRS_FND_shutdown] = new JustinaCmdAndResp("pf_shutdown");
			#endregion
			#region Commands for SPG-GEN
			//this.justinaCmdAndResp[(int)JustinaCommands.SP_GEN_aread] = new JustinaCmdAndResp("spg_aread");
			//this.justinaCmdAndResp[(int)JustinaCommands.SP_GEN_asay] = new JustinaCmdAndResp("spg_asay");
			//this.justinaCmdAndResp[(int)JustinaCommands.SP_GEN_read] = new JustinaCmdAndResp("spg_read");
			this.justinaCmdAndResp[(int)JustinaCommands.SP_GEN_say] = new JustinaCmdAndResp("spg_asay");
			this.justinaCmdAndResp[(int)JustinaCommands.SP_GEN_play] = new JustinaCmdAndResp("spg_aplay");
			this.justinaCmdAndResp[(int)JustinaCommands.SP_GEN_shutup] = new JustinaCmdAndResp("spg_shutup");
			this.justinaCmdAndResp[(int)JustinaCommands.SP_GEN_playloop] = new JustinaCmdAndResp("spg_playloop");
			#endregion
			#region Commands for ST-PLN
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_alignhuman] = new JustinaCmdAndResp("align_human");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_cleanplane] = new JustinaCmdAndResp("clean_plane");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_comewith] = new JustinaCmdAndResp("come_with");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_dopresentation] = new JustinaCmdAndResp("dopresentation");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_drop] = new JustinaCmdAndResp("drop");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_findhuman] = new JustinaCmdAndResp("find_human");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_findhumanobject] = new JustinaCmdAndResp("find_human_object");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_findobject] = new JustinaCmdAndResp("find_object");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_findtable] = new JustinaCmdAndResp("find_table");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_grab] = new JustinaCmdAndResp("grab");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_greethuman] = new JustinaCmdAndResp("greethuman");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_release] = new JustinaCmdAndResp("release");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_rememberhuman] = new JustinaCmdAndResp("remember_human");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_searchobject] = new JustinaCmdAndResp("search_object");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_seehand] = new JustinaCmdAndResp("see_hand");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_seeobject] = new JustinaCmdAndResp("see_object");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_shutdown] = new JustinaCmdAndResp("shutdown");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_take] = new JustinaCmdAndResp("take");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_deliverObject] = new JustinaCmdAndResp("deliverobject");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_findonshelf] = new JustinaCmdAndResp("findonshelf");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_takefromShelf] = new JustinaCmdAndResp("takefromshelf");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_holding] = new JustinaCmdAndResp("holding");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_executelearned] = new JustinaCmdAndResp("executelearned");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_startlearn] = new JustinaCmdAndResp("startlearn");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_stoplearn] = new JustinaCmdAndResp("stoplearn");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_takehandover] = new JustinaCmdAndResp("takehandover");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_takehumanhands] = new JustinaCmdAndResp("takehumanhands");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_aligneEdge] = new JustinaCmdAndResp("aligneedge");

			#endregion
			#region Commands for OBJ-FND

			this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FND_find] = new JustinaCmdAndResp("oft_find");
			this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FND_findOnTable] = new JustinaCmdAndResp("oft_findontabledos");
			this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FND_removeTable] = new JustinaCmdAndResp("oft_removetable");
			this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_findedgetuned] = new JustinaCmdAndResp("oft_findedgetuned");
			this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FND_findShelfPlanes] = new JustinaCmdAndResp("oft_findshelf");
			this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FND_findObjectsOnshelf] = new JustinaCmdAndResp("oft_findonshelf");
			this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FND_findHumanHands] = new JustinaCmdAndResp("oft_findhumanhands");
			this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FND_findEdge] = new JustinaCmdAndResp("oft_findedge");
			this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FND_getnode] = new JustinaCmdAndResp("oft_getnode");
			this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FND_setnode] = new JustinaCmdAndResp("oft_setnode");
			this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FND_saveMap] = new JustinaCmdAndResp("oft_savemap");
			this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FND_loadMap] = new JustinaCmdAndResp("oft_readmap");
            this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FND_remember] = new JustinaCmdAndResp("oft_remember");
			this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_findedgereturns] = new JustinaCmdAndResp("oft_findedgereturns");
			this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_findedgefastandfurious] = new JustinaCmdAndResp("oft_findedgefastandfurious");
			this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FND_findCubes] = new JustinaCmdAndResp("find_cubes");

			#endregion

			foreach (JustinaCmdAndResp jcar in this.justinaCmdAndResp)
				if (jcar != null)
					this.sortedCmdAndResp.Add(jcar.Command.CommandName, jcar);

			#region Old Code

			#endregion
		}

		#region Methods

		private void HAL9000CmdMan_ResponseReceived(Response response)
		{
			this.ParseResponse(response);
		}

		protected override void ParseResponse(Response response)
		{
			base.ParseResponse(response);

			if (response.CommandName == "read_var" || response.CommandName == "list_vars" ||
				response.CommandName == "read_vars" || response.CommandName == "subscribe_var" ||
				response.CommandName == "create_var" || response.CommandName == "write_var" ||
				response.CommandName == "var" || response.CommandName == "vars" ||
                // To use with newer version of Robotics.dll :
                response.CommandName == "prototypes" || response.CommandName == "stat_var"
				) return;

			this.sortedCmdAndResp[response.CommandName].Response = response;
			this.sortedCmdAndResp[response.CommandName].IsResponseReceived = true;
		}

		private void TerminateProcess(string processName, int waitTime_ms)
		{
			Process[] processesToTerminate = Process.GetProcessesByName(processName);
			foreach (Process p in processesToTerminate)
			{
				try
				{
					if (!p.HasExited && p.Responding)
						p.Close();
				}
				catch { }
			}
			Thread.Sleep(waitTime_ms);
			foreach (Process p in processesToTerminate)
			{
				try
				{
					p.Kill();
				}
				catch { }
			}
		}

		private void RunProcess(string path, string args)
		{
			ProcessStartInfo psi = new ProcessStartInfo();
			psi.FileName = path;
			psi.Arguments = args;
			psi.WindowStyle = ProcessWindowStyle.Minimized;
			Process.Start(psi);
		}

		private void SetupAndSendCommand(JustinaCommands justinaCmd, string parameters)
		{
			this.justinaCmdAndResp[(int)justinaCmd].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)justinaCmd].Command.Parameters = parameters;
			this.SendCommand(this.justinaCmdAndResp[(int)justinaCmd].Command);
			TextBoxStreamWriter.DefaultLog.WriteLine(7, TBWriter.time() + justinaCmd.ToString() + "  command sended");
		}

		public bool WaitForResponse(JustinaCommands expectedCmdResp, int timeOut_ms)
		{
			TBWriter.Write(2, TBWriter.time() + "Waiting response from command : [" + expectedCmdResp.ToString() + " ]");

			int sleepsNumber = (int)(((double)timeOut_ms) / (10));

			bool resp = this.IsResponseReceived(expectedCmdResp);

			while (!this.IsResponseReceived(expectedCmdResp) && sleepsNumber-- > 0)
				Thread.Sleep(10);

			if (this.IsResponseReceived(expectedCmdResp))
			{
				TBWriter.Write(2, TBWriter.time() + "Response recieved from command [ " + expectedCmdResp.ToString() + " ]");
				return this.justinaCmdAndResp[(int)expectedCmdResp].Response.Success;
			}
			else
			{
				TBWriter.Error("CMD TIMEOUT: " + expectedCmdResp.ToString() + " > waited time " + timeOut_ms + "ms");
				return false;
			}
		}

		public bool IsResponseReceived(JustinaCommands expectedCmdResp)
		{
			if (this.justinaCmdAndResp[(int)expectedCmdResp].IsResponseReceived)
				return true;
			else
				return false;
		}

		public bool IsResponseReceived(JustinaCommands expectedCmdResp, out Response receivedResp)
		{
			receivedResp = null;
			if (this.IsResponseReceived(expectedCmdResp))
			{
				receivedResp = this.justinaCmdAndResp[(int)expectedCmdResp].Response;
				return true;
			}
			else return false;

		}

		public JustinaCmdAndResp[] JustinaCmdAndResps
		{
			get { return this.justinaCmdAndResp; }
		}

		#endregion

		#region Commands BLK

		public bool BLK_alive(string module, int timeOut_ms)
		{
			this.justinaCmdAndResp[(int)JustinaCommands.BLK_alive].IsResponseReceived = false;
			this.SetupAndSendCommand(JustinaCommands.BLK_alive, module);
			return this.WaitForResponse(JustinaCommands.BLK_alive, timeOut_ms);
		}

		public bool BLK_ready(string module, int timeOut_ms)
		{
			this.justinaCmdAndResp[(int)JustinaCommands.BLK_ready].IsResponseReceived = false;
			this.SetupAndSendCommand(JustinaCommands.BLK_ready, module);
			return this.WaitForResponse(JustinaCommands.BLK_ready, timeOut_ms);
		}

		public bool BLK_busy(string module, int timeOut_ms)
		{
			this.justinaCmdAndResp[(int)JustinaCommands.BLK_busy].IsResponseReceived = false;
			this.SetupAndSendCommand(JustinaCommands.BLK_busy, module);
			return this.WaitForResponse(JustinaCommands.BLK_busy, timeOut_ms);
		}

		#endregion

		#region Commands ARMS

		public bool ARMS_arms_goto(string predefinedPosition, int timeOut_ms)
		{
			SetupAndSendCommand(JustinaCommands.ARMS_arms_goto,predefinedPosition);
			return this.WaitForResponse(JustinaCommands.ARMS_arms_goto, timeOut_ms);
		}

		// --- RIGHT ARM

		public void ARMS_ra_relpos(double x, double y, double z, double roll, double pitch, double yaw, double elbow)
		{
			string parameters = x.ToString("0.00") + " " + y.ToString("0.00") + " " + z.ToString("0.00") + " " +
								 roll.ToString("0.00") + " " + pitch.ToString("0.00") + " " + yaw.ToString("0.00") + " " +
								elbow.ToString("0.00");

			TBWriter.Info1("RIGHT ARM : RelPos sending " + parameters);

			this.SetupAndSendCommand(JustinaCommands.ARMS_ra_relpos, parameters);
		}

		public bool ARMS_ra_relpos(double x, double y, double z, double roll, double pitch, double yaw, double elbow, int timeOut_ms)
		{
			string parameters = x.ToString("0.00") + " " + y.ToString("0.00") + " " + z.ToString("0.00") + " " +
								 roll.ToString("0.00") + " " + pitch.ToString("0.00") + " " + yaw.ToString("0.00") + " " +
								elbow.ToString("0.00");

			TBWriter.Info1("RIGHT ARM : RelPos sending " + parameters);

			this.SetupAndSendCommand(JustinaCommands.ARMS_ra_relpos, parameters);
			return this.WaitForResponse(JustinaCommands.ARMS_ra_relpos, timeOut_ms);
		}

		public bool ARMS_ra_getabspos(int timeOut_ms, out string armInfo)
		{
			SetupAndSendCommand(JustinaCommands.ARMS_ra_abspos, "");

			if (WaitForResponse(JustinaCommands.ARMS_ra_abspos, timeOut_ms))
			{
				armInfo = justinaCmdAndResp[(int)JustinaCommands.ARMS_ra_abspos].Response.Parameters;

				if (string.IsNullOrEmpty(armInfo))
					return false;
				else
					return true;
			}
			else
			{
				armInfo = "";
				return false;
			}
		}

		public void ARMS_ra_abspos(Vector3 position, double roll, double pitch, double yaw, double elbow)
		{
			ARMS_ra_abspos(position.X, position.Y, position.Z, roll, pitch, yaw, elbow);
		}

		public bool ARMS_ra_abspos(Vector3 position, double roll, double pitch, double yaw, double elbow, int timeOut_ms)
		{
			if (position == null)
				return false;

			return ARMS_ra_abspos(position.X, position.Y, position.Z, roll, pitch, yaw, elbow, timeOut_ms);
		}

		public void ARMS_ra_artpos(double x, double y, double z, double roll, double pitch, double yaw, double elbow)
		{
			string parameters = x.ToString("0.00") + " " + y.ToString("0.00") + " " + z.ToString("0.00") + " " +
								 roll.ToString("0.00") + " " + pitch.ToString("0.00") + " " + yaw.ToString("0.00") + " " +
								elbow.ToString("0.00");

			this.SetupAndSendCommand(JustinaCommands.ARMS_ra_articularPos, parameters);
		}

		public bool ARMS_ra_artpos(out double p0, out double p1, out double p2, out double p3, out double p4, out double p5,out double p6, int timeOut_ms)
		{
			p0 = 0;
			p1 = 0;
			p2 = 0;
			p3 = 0;
			p4 = 0;
			p5 = 0;
			p6 = 0;
			
			this.SetupAndSendCommand(JustinaCommands.ARMS_ra_articularPos," ");
			if (!this.WaitForResponse(JustinaCommands.ARMS_ra_articularPos, timeOut_ms)) return false;
			char[] delimiters = { ' ', '\t' };
			string[] parts = this.justinaCmdAndResp[(int)JustinaCommands.ARMS_ra_articularPos].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length != 7) return false;
			try
			{
				p0 = double.Parse(parts[0]);
				p1 = double.Parse(parts[1]);
				p2 = double.Parse(parts[2]);
				p3 = double.Parse(parts[3]);
				p4 = double.Parse(parts[4]);
				p5 = double.Parse(parts[5]);
				p6 = double.Parse(parts[6]);
				return true;
			}
			catch
			{
				return false;
			}
			
		}

		public bool ARMS_ra_artpos(double x, double y, double z, double roll, double pitch, double yaw, double elbow, int timeOut_ms)
		{
			this.ARMS_ra_artpos(x, y, z, roll, pitch, yaw, elbow);
			return this.WaitForResponse(JustinaCommands.ARMS_ra_articularPos, timeOut_ms);
		}

		public void ARMS_ra_abspos(double x, double y, double z, double roll, double pitch, double yaw, double elbow)
		{
			string parameters = x.ToString("0.00") + " " + y.ToString("0.00") + " " + z.ToString("0.00") + " " +
								 roll.ToString("0.00") + " " + pitch.ToString("0.00") + " " + yaw.ToString("0.00") + " " +
								elbow.ToString("0.00");

			this.SetupAndSendCommand(JustinaCommands.ARMS_ra_abspos, parameters);
		}

		public bool ARMS_ra_abspos(double x, double y, double z, double roll, double pitch, double yaw, double elbow, int timeOut_ms)
		{
			this.ARMS_ra_abspos(x, y, z, roll, pitch, yaw, elbow);
			return this.WaitForResponse(JustinaCommands.ARMS_ra_abspos, timeOut_ms);
		}

		public void ARMS_ra_closegrip()
		{
			this.SetupAndSendCommand(JustinaCommands.ARMS_ra_closegrip, "");
		}

		public bool ARMS_ra_closegrip(int timeOut_ms)
		{
			this.ARMS_ra_closegrip();
			return this.WaitForResponse(JustinaCommands.ARMS_ra_closegrip, timeOut_ms);
		}

		public void ARMS_ra_goto(string predefinedPosition)
		{
			this.SetupAndSendCommand(JustinaCommands.ARMS_ra_goto, predefinedPosition);
			//this.justinaCmdAndResp[(int)JustinaCommands.ARMS_ra_goto].IsResponseReceived = false;
			//this.justinaCmdAndResp[(int)JustinaCommands.ARMS_ra_goto].Command.Parameters = predefinedPosition;
			//this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.ARMS_ra_goto].Command);
		}

		public bool ARMS_ra_goto(string predefinedPosition, int timeOut_ms)
		{
			this.ARMS_ra_goto(predefinedPosition);
			return this.WaitForResponse(JustinaCommands.ARMS_ra_goto, timeOut_ms);
		}

		public void ARMS_ra_move(string predefinedMovement)
		{
			this.SetupAndSendCommand(JustinaCommands.ARMS_ra_move, predefinedMovement);
			//this.justinaCmdAndResp[(int)JustinaCommands.ARMS_ra_move].IsResponseReceived = false;
			//this.justinaCmdAndResp[(int)JustinaCommands.ARMS_ra_move].Command.Parameters = predefinedMovement;
			//this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.ARMS_ra_move].Command);
		}

		public bool ARMS_ra_move(string predefinedMovement, int timeOut_ms)
		{
			this.ARMS_ra_move(predefinedMovement);
			return this.WaitForResponse(JustinaCommands.ARMS_ra_move, timeOut_ms);
		}

		public void ARMS_ra_opengrip()
		{
			this.SetupAndSendCommand(JustinaCommands.ARMS_ra_opengrip, "");
		}

		public bool ARMS_ra_opengrip(int timeOut_ms)
		{
			this.ARMS_ra_opengrip();
			return this.WaitForResponse(JustinaCommands.ARMS_ra_opengrip, timeOut_ms);
		}

		public void ARMS_ra_opengrip(double aperture)
		{
			this.SetupAndSendCommand(JustinaCommands.ARMS_ra_opengrip, ((int)aperture).ToString());
		}

		public bool ARMS_ra_opengrip(double aperture, int timeOut_ms)
		{
			this.ARMS_ra_opengrip(aperture);
			return this.WaitForResponse(JustinaCommands.ARMS_ra_opengrip, timeOut_ms);
		}

		public bool ARMS_ra_reachable(double x, double y, double z, double roll, double pitch, double yaw, double elbow, int timeOut_ms)
		{
			string parameters = x.ToString("0.00") + " " + y.ToString("0.00") + " " + z.ToString("0.00") + " " +
								 roll.ToString("0.00") + " " + pitch.ToString("0.00") + " " + yaw.ToString("0.00") + " " +
								elbow.ToString("0.00");

			this.SetupAndSendCommand(JustinaCommands.ARMS_ra_reachable, parameters);
			return this.WaitForResponse(JustinaCommands.ARMS_ra_reachable, timeOut_ms);
		}

		public bool ARMS_ra_torque(bool enable, int timeOut_ms)
		{
			if (enable)
				SetupAndSendCommand(JustinaCommands.ARMS_ra_torque, "on");
			else
				SetupAndSendCommand(JustinaCommands.ARMS_ra_torque, "off");

			return this.WaitForResponse(JustinaCommands.ARMS_ra_torque, timeOut_ms);
		}

        public bool ARMS_ra_hand(int thumb, int baseThumb, int index, int others, int timeOut_ms)
        {
            string parameters = thumb.ToString() + " " + baseThumb.ToString() + " " + index.ToString() + " " + others.ToString();
            this.SetupAndSendCommand(JustinaCommands.ARMS_ra_hand, parameters);
            return this.WaitForResponse(JustinaCommands.ARMS_ra_hand, timeOut_ms);
        }

        public bool ARMS_ra_handGoTo(string position, int timeOut_ms)
        {
            this.SetupAndSendCommand(JustinaCommands.ARMS_ra_handGoto, position);
            return this.WaitForResponse(JustinaCommands.ARMS_ra_handGoto, timeOut_ms);
        }

		// --- LEFT ARM

		public void ARMS_la_relpos(double x, double y, double z, double roll, double pitch, double yaw, double elbow)
		{
			string parameters = x.ToString("0.000") + " " + y.ToString("0.000") + " " + z.ToString("0.000") + " " +
								 roll.ToString("0.000") + " " + pitch.ToString("0.000") + " " + yaw.ToString("0.000") + " " +
								elbow.ToString("0.000");

			TBWriter.Info1("LEFT ARM : RelPos sending " + parameters);

			this.SetupAndSendCommand(JustinaCommands.ARMS_la_relpos, parameters);
		}

		public void ARMS_la_relpos(double x, double y, double z, double roll, double pitch, double yaw, double elbow, int timeOut_ms)
		{
			string parameters = x.ToString("0.00") + " " + y.ToString("0.00") + " " + z.ToString("0.00") + " " +
								 roll.ToString("0.00") + " " + pitch.ToString("0.00") + " " + yaw.ToString("0.00") + " " +
								elbow.ToString("0.00");

			TBWriter.Info1("LEFT ARM : RelPos sending " + parameters);

			this.SetupAndSendCommand(JustinaCommands.ARMS_la_relpos, parameters);
			this.WaitForResponse(JustinaCommands.ARMS_la_relpos, timeOut_ms);
		}

		public bool ARMS_la_getabspos(int timeOut_ms, out string armInfo)
		{
			SetupAndSendCommand(JustinaCommands.ARMS_la_abspos, "");

			if (WaitForResponse(JustinaCommands.ARMS_la_abspos, timeOut_ms))
			{
				armInfo = justinaCmdAndResp[(int)JustinaCommands.ARMS_la_abspos].Response.Parameters;

				if (string.IsNullOrEmpty(armInfo))
					return false;
				else
					return true;
			}
			else
			{
				armInfo = "";
				return false;
			}
		}

		public void ARMS_la_abspos(Vector3 position, double roll, double pitch, double yaw, double elbow)
		{
			ARMS_la_abspos(position.X, position.Y, position.Z, roll, pitch, yaw, elbow);
		}

		public bool ARMS_la_abspos(Vector3 position, double roll, double pitch, double yaw, double elbow, int timeOut_ms)
		{
			if (position == null)
				return false;

			return ARMS_la_abspos(position.X, position.Y, position.Z, roll, pitch, yaw, elbow, timeOut_ms);
		}

		public void ARMS_la_artpos(double x, double y, double z, double roll, double pitch, double yaw, double elbow)
		{
			string parameters = x.ToString("0.00") + " " + y.ToString("0.00") + " " + z.ToString("0.00") + " " +
								 roll.ToString("0.00") + " " + pitch.ToString("0.00") + " " + yaw.ToString("0.00") + " " +
								elbow.ToString("0.00");

			this.SetupAndSendCommand(JustinaCommands.ARMS_la_articularPos, parameters);
		}

		public bool ARMS_la_artpos(double x, double y, double z, double roll, double pitch, double yaw, double elbow, int timeOut_ms)
		{
			this.ARMS_la_artpos(x, y, z, roll, pitch, yaw, elbow);
			return this.WaitForResponse(JustinaCommands.ARMS_la_articularPos, timeOut_ms);
		}

		public bool ARMS_la_artpos(out double p0, out double p1, out double p2, out double p3, out double p4, out double p5,out double p6, int timeOut_ms)
		{
			p0 = 0;
			p1 = 0;
			p2 = 0;
			p3 = 0;
			p4 = 0;
			p5 = 0;
			p6 = 0;

			this.SetupAndSendCommand(JustinaCommands.ARMS_la_articularPos, "");
			
			if (!this.WaitForResponse(JustinaCommands.ARMS_la_articularPos, timeOut_ms)) return false;
			char[] delimiters = { ' ', '\t' };
			string[] parts = this.justinaCmdAndResp[(int)JustinaCommands.ARMS_la_articularPos].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length != 7) return false;
			try
			{
				p0 = double.Parse(parts[0]);
				p1 = double.Parse(parts[1]);
				p2 = double.Parse(parts[2]);
				p3 = double.Parse(parts[3]);
				p4 = double.Parse(parts[4]);
				p5 = double.Parse(parts[5]);
				p6 = double.Parse(parts[6]);
				return true;
			}
			catch
			{
				return false;
			}

		}

		public void ARMS_la_abspos(double x, double y, double z, double roll, double pitch, double yaw, double elbow)
		{
			string parameters = x.ToString("0.00") + " " + y.ToString("0.00") + " " + z.ToString("0.00") + " " +
								 roll.ToString("0.00") + " " + pitch.ToString("0.00") + " " + yaw.ToString("0.00") + " " +
								elbow.ToString("0.00");

			this.SetupAndSendCommand(JustinaCommands.ARMS_la_abspos, parameters);
		}

		public bool ARMS_la_abspos(double x, double y, double z, double roll, double pitch, double yaw, double elbow, int timeOut_ms)
		{
			this.ARMS_la_abspos(x, y, z, roll, pitch, yaw, elbow);
			return this.WaitForResponse(JustinaCommands.ARMS_la_abspos, timeOut_ms);
		}

		public void ARMS_la_closegrip()
		{
			this.SetupAndSendCommand(JustinaCommands.ARMS_la_closegrip, "100");
		}

		public bool ARMS_la_closegrip(int timeOut_ms)
		{
			this.ARMS_la_closegrip();
			return this.WaitForResponse(JustinaCommands.ARMS_la_closegrip, timeOut_ms);
		}

		public void ARMS_la_goto(string predefinedPosition)
		{
			this.SetupAndSendCommand(JustinaCommands.ARMS_la_goto, predefinedPosition);
		}

		public bool ARMS_la_goto(string predefinedPosition, int timeOut_ms)
		{
			this.ARMS_la_goto(predefinedPosition);
			return this.WaitForResponse(JustinaCommands.ARMS_la_goto, timeOut_ms);
		}

		public void ARMS_la_move(string predefinedMovement)
		{
			this.SetupAndSendCommand(JustinaCommands.ARMS_la_move, predefinedMovement);
		}

		public bool ARMS_la_move(string predefinedMovement, int timeOut_ms)
		{
			this.ARMS_la_move(predefinedMovement);
			return this.WaitForResponse(JustinaCommands.ARMS_la_move, timeOut_ms);
		}

		public void ARMS_la_opengrip()
		{
			this.SetupAndSendCommand(JustinaCommands.ARMS_la_opengrip, "");
		}

		public bool ARMS_la_opengrip(int timeOut_ms)
		{
			this.ARMS_la_opengrip();
			return this.WaitForResponse(JustinaCommands.ARMS_la_opengrip, timeOut_ms);
		}

		public void ARMS_la_opengrip(double aperture)
		{
			this.SetupAndSendCommand(JustinaCommands.ARMS_la_opengrip, ((int)aperture).ToString());
		}

		public bool ARMS_la_opengrip(double aperture, int timeOut_ms)
		{
			this.ARMS_la_opengrip(aperture);
			return this.WaitForResponse(JustinaCommands.ARMS_la_opengrip, timeOut_ms);
		}

		public bool ARMS_la_reachable(double x, double y, double z, double roll, double pitch, double yaw, double elbow, int timeOut_ms)
		{
			string parameters = x.ToString("0.00") + " " + y.ToString("0.00") + " " + z.ToString("0.00") + " " +
								 roll.ToString("0.00") + " " + pitch.ToString("0.00") + " " + yaw.ToString("0.00") + " " +
								elbow.ToString("0.00");

			this.SetupAndSendCommand(JustinaCommands.ARMS_la_reachable, parameters);
			bool success = this.WaitForResponse(JustinaCommands.ARMS_la_reachable, timeOut_ms);
			return success;
		}

		public bool ARMS_la_torque(bool enable, int timeOut_ms)
		{
			if (enable)
				SetupAndSendCommand(JustinaCommands.ARMS_la_torque, "on");
			else
				SetupAndSendCommand(JustinaCommands.ARMS_la_torque, "off");

			return this.WaitForResponse(JustinaCommands.ARMS_la_torque, timeOut_ms);
		}

		#endregion

		#region Commands HEAD

		public bool HEAD_getAngles(int timeOut_ms, out double[] angles)
		{
			angles = null;

			//this.justinaCmdAndResp[(int)JustinaCommands.HEAD_lookat].IsResponseReceived = false;
			//this.justinaCmdAndResp[(int)JustinaCommands.HEAD_lookat].Command.Parameters = "";
			//this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.HEAD_lookat].Command);

			this.SetupAndSendCommand(JustinaCommands.HEAD_lookat, " ");

			if (!this.WaitForResponse(JustinaCommands.HEAD_lookat, timeOut_ms+1500))
				return false;

			return SimpleTaskPlanner.Parse.string2doubleArray(justinaCmdAndResp[(int)JustinaCommands.HEAD_lookat].Response.Parameters, out angles);
		}

		public void HEAD_lookat(double pan, double tilt)
		{
			this.justinaCmdAndResp[(int)JustinaCommands.HEAD_lookat].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.HEAD_lookat].Command.Parameters = pan.ToString("0.0000") + " " + tilt.ToString("0.0000");
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.HEAD_lookat].Command);
		}

		public bool HEAD_lookat(double pan, double tilt, int timeOut_ms)
		{
			this.HEAD_lookat(pan, tilt);
			bool success = this.WaitForResponse(JustinaCommands.HEAD_lookat, timeOut_ms+5000);
			//Thread.Sleep(1000);
			return success;
		}

		public void HEAD_search(string whatToSearch, int timeInSeconds)
		{
			this.justinaCmdAndResp[(int)JustinaCommands.HEAD_search].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.HEAD_search].Command.Parameters = whatToSearch + " " + timeInSeconds.ToString();
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.HEAD_search].Command);
		}

		public void HEAD_show(string emotion)
		{
			this.justinaCmdAndResp[(int)JustinaCommands.HEAD_show].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.HEAD_show].Command.Parameters = emotion;
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.HEAD_show].Command);
		}

		public bool HEAD_show(string emotion, int timeOut_ms)
		{
			this.HEAD_show(emotion);
			return this.WaitForResponse(JustinaCommands.HEAD_show, timeOut_ms);
		}

		public void HEAD_lookatobject(Vector3 position)
		{
			this.justinaCmdAndResp[(int)JustinaCommands.HEAD_lookatobject].IsResponseReceived = false;

			string parameters = position.X.ToString("0.000") + " " + position.Y.ToString("0.000") + " " + position.Z.ToString("0.000");
			this.SetupAndSendCommand(JustinaCommands.HEAD_lookatobject, parameters);
		}

		public bool HEAD_lookatobject(Vector3 position, int timeOut_ms)
		{
			this.HEAD_lookatobject(position);
			return this.WaitForResponse(JustinaCommands.HEAD_lookatobject, timeOut_ms);
		}

		public bool HEAD_lookatREL(double hdPan, double hdTilt, int timeOut_ms)
		{
			string parameters = hdPan.ToString("0.000")+ " " + hdTilt.ToString("0.000");
			this.SetupAndSendCommand(JustinaCommands.HEAD_lookatrel, parameters);
			return this.WaitForResponse(JustinaCommands.HEAD_lookatrel, timeOut_ms);
		}

		#endregion

		#region Commands MVN-PLN

		public bool MVN_PLN_pause(bool enable, int timeOut_ms)
		{
			string parameters = (enable) ? "on" : "off";

			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_pause].IsResponseReceived = false;
			this.SetupAndSendCommand(JustinaCommands.MVN_PLN_pause, parameters);

			return this.WaitForResponse(JustinaCommands.MVN_PLN_pause, timeOut_ms);
		}

		public void MVN_PLN_getclose()
		{
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_getclose].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_getclose].Command.Parameters = "";
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_getclose].Command);
		}

		public bool MVN_PLN_getclose(int timeOut_ms)
		{
			this.MVN_PLN_getclose();
			return this.WaitForResponse(JustinaCommands.MVN_PLN_getclose, timeOut_ms);
		}

		public bool MVN_PLN_getclose(double x, double y, int timeOut_ms)
		{
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_getclose].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_getclose].Command.Parameters = x.ToString("0.00") + " " + y.ToString("0.00");

			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_getclose].Command);

			return this.WaitForResponse(JustinaCommands.MVN_PLN_getclose, timeOut_ms);
		}

		public bool MVN_PLN_getclose(string location, int timeOut_ms)
		{
			this.SetupAndSendCommand(JustinaCommands.MVN_PLN_getclose, location);
			return this.WaitForResponse(JustinaCommands.MVN_PLN_getclose, timeOut_ms);

		}

		public void MVN_PLN_goto(string goalRoom)
		{
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_goto].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_goto].Command.Parameters = goalRoom;
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_goto].Command);
		}

		public bool MVN_PLN_goto(string goalRoom, int timeOut_ms)
		{
			this.MVN_PLN_goto(goalRoom);
			return this.WaitForResponse(JustinaCommands.MVN_PLN_goto, timeOut_ms);
		}

		public void MVN_PLN_goto(RegionType regionType, string goalRegion)
		{
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_goto].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_goto].Command.Parameters = regionType.ToString().ToLower() + " " + goalRegion;
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_goto].Command);
		}

		public bool MVN_PLN_goto(RegionType regionType, string goalRegion, int timeOut_ms)
		{
			this.MVN_PLN_goto(regionType, goalRegion);
			return this.WaitForResponse(JustinaCommands.MVN_PLN_goto, timeOut_ms);
		}

		public void MVN_PLN_goto(double x, double y, double angle)
		{
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_goto].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_goto].Command.Parameters = "xy" + " " +
				x.ToString("0.0000") + " " + y.ToString("0.0000") + " " + angle.ToString("0.0000");
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_goto].Command);
		}

		public bool MVN_PLN_goto(Vector3 position, double angle, int timeOut_ms)
		{
			this.MVN_PLN_goto(position.X, position.Y, angle);

			return this.WaitForResponse(JustinaCommands.MVN_PLN_goto, timeOut_ms);
		}

		public void MVN_PLN_goto(double x, double y)
		{
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_goto].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_goto].Command.Parameters = "xy" + " " +
				x.ToString("0.0000") + " " + y.ToString("0.0000");
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_goto].Command);
		}

		public void MVN_PLN_move(double distance, double angle, double time)
		{
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_move].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_move].Command.Parameters = distance.ToString("0.0000") +
				" " + angle.ToString("0.0000") + " " + time.ToString("0.0000");
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_move].Command);
		}

		public bool MVN_PLN_move(double distance, double angle, double time, int timeOut_ms)
		{
			this.MVN_PLN_move(distance, angle, time);
			return this.WaitForResponse(JustinaCommands.MVN_PLN_move, timeOut_ms);
		}

		public void MVN_PLN_move(double distance, double angle)
		{
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_move].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_move].Command.Parameters = distance.ToString("0.0000") +
				" " + angle.ToString("0.0000");
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_move].Command);
		}

		public bool MVN_PLN_move(double distance, double angle, int timeOut_ms)
		{
			this.MVN_PLN_move(distance, angle);
			return this.WaitForResponse(JustinaCommands.MVN_PLN_move, timeOut_ms);
		}

		public void MVN_PLN_move(double distance)
		{
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_move].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_move].Command.Parameters = distance.ToString("0.0000");
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_move].Command);
		}

		public bool MVN_PLN_move(double distance, int timeOut_ms)
		{
			this.MVN_PLN_move(distance);
			return this.WaitForResponse(JustinaCommands.MVN_PLN_move, timeOut_ms);
		}

		public bool MVN_PLN_getPosition(int timeOut_ms, out double[] positionAndOrientation)
		{
			positionAndOrientation = null;

			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_position].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_position].Command.Parameters = "";
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_position].Command);

			if (!this.WaitForResponse(JustinaCommands.MVN_PLN_position, timeOut_ms)) return false;

			return SimpleTaskPlanner.Parse.string2doubleArray(justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_position].Response.Parameters, out positionAndOrientation);
		}

		public void MVN_PLN_position(double x, double y, double angle)
		{
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_position].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_position].Command.Parameters =
				x.ToString("0.0000") + " " + y.ToString("0.0000") + " " + angle.ToString("0.0000");
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_position].Command);
		}

		public bool MVN_PLN_position(out double x, out double y, out double orientation, int timeOut_ms)
		{
			x = 0;
			y = 0;
			orientation = 0;
			this.SetupAndSendCommand(JustinaCommands.MVN_PLN_position, "");
			if (!this.WaitForResponse(JustinaCommands.MVN_PLN_position, timeOut_ms)) return false;
			char[] delimiters = { ' ', '\t' };
			string[] parts = this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_position].Response.Parameters.Split(delimiters,StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length != 3) return false;
			try {
				x=double.Parse(parts[0]);
				y = double.Parse(parts[1]);
				orientation = double.Parse(parts[2]);
				return true;
			}
			catch {
				return false;
			}
		}

		public void MVN_PLN_position(double x, double y)
		{
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_position].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_position].Command.Parameters =
				x.ToString("0.0000") + " " + y.ToString("0.0000");
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_position].Command);
		}

		public void MVN_PLN_report()
		{
			this.SetupAndSendCommand(JustinaCommands.MVN_PLN_report, "");
		}

		public bool MVN_PLN_report(int timeOut_ms)
		{
			this.MVN_PLN_report();
			return this.WaitForResponse(JustinaCommands.MVN_PLN_report, timeOut_ms);
		}

		public void MVN_PLN_selfterminate()
		{
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_selfterminate].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_selfterminate].Command.Parameters = "";
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_selfterminate].Command);
			this.TerminateProcess("MotionPlannerRevolutions.exe", 1000);
		}

		public void MVN_PLN_startfollowhuman()
		{
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_startfollowhuman].IsResponseReceived = false;
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_startfollowhuman].Command);
		}

		public void MVN_PLN_stopfollowhuman()
		{
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_stopfollowhuman].IsResponseReceived = false;
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_stopfollowhuman].Command);
		}

		public bool MVN_PLN_obstacles(string options, int timeOut_ms)
		{
			bool success;

			SetupAndSendCommand(JustinaCommands.MVN_PLN_obstacle, options);
			success = WaitForResponse(JustinaCommands.MVN_PLN_obstacle, timeOut_ms);

			return success;
		}

		public bool MVN_PLN_setSpeeds(int leftSpeed, int rightSpeed)
		{
			SetupAndSendCommand(JustinaCommands.MVN_PLN_setspeeds, leftSpeed.ToString() + " " + rightSpeed.ToString());
			return WaitForResponse(JustinaCommands.MVN_PLN_setspeeds, 1000);
		}

		#endregion

		#region Commands OBJ-FND

		public bool OBJ_FND_findcubes(out List<WorldObject> cubes)
		{
			cubes = new List<WorldObject>();

			this.SetupAndSendCommand(JustinaCommands.OBJ_FND_findCubes, "all");
			if (!this.WaitForResponse(JustinaCommands.OBJ_FND_findCubes, 5000))
				return false;

			string resp = justinaCmdAndResp[(int)JustinaCommands.OBJ_FND_findCubes].Response.Parameters;
			string[] splitResp = resp.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

			if( splitResp.Length % 4 != 0)
				TBWriter.Error( "FindCubes : Parameters Length Error (must be multiple of 4). Params={" + resp+ "} , Lenght={" + splitResp.Length+"}" );

			int cnt = 0;
			while (cnt < splitResp.Length)
			{
				string color = splitResp[cnt];
				cnt++;

				double x;
				if (!double.TryParse(splitResp[cnt], out x))
				{
					TBWriter.Error("Cant Parse X coor in findCubes:" + splitResp[cnt]);
					return false;
				}
				cnt++;

				double y;
				if( !double.TryParse(splitResp[cnt], out y))
				{
					TBWriter.Error("Cant Parse Y coor in findCubes:" + splitResp[cnt]);
					return false;
				}
				cnt++;

				double z;
				if (!double.TryParse(splitResp[cnt], out z))
				{
					TBWriter.Error("Cant Parse Z coor in findCubes:" + splitResp[cnt]);
					return false;
				}
				cnt++;

				WorldObject cube = new WorldObject(color, new Vector3(x, y, z));
				cubes.Add(cube);
			}
			return true;
		}

		public bool OBJ_FND_findcubes(string searchColor, out WorldObject cube)
		{
			cube = null;

			this.SetupAndSendCommand(JustinaCommands.OBJ_FND_findCubes, searchColor);
			if (!this.WaitForResponse(JustinaCommands.OBJ_FND_findCubes, 5000))
				return false;

			string resp = justinaCmdAndResp[(int)JustinaCommands.OBJ_FND_findCubes].Response.Parameters;
			string[] splitResp = resp.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

			if (splitResp.Length % 4 != 0)
				TBWriter.Error("FindCubes : Parameters Length Error (must be multiple of 4). Params={" + resp + "} , Lenght={" + splitResp.Length + "}");

			int cnt = 0;

			string color = splitResp[cnt];
			cnt++;

			double x;
			if (!double.TryParse(splitResp[cnt], out x))
			{
				TBWriter.Error("Cant Parse X coor in findCubes:" + splitResp[cnt]);
				return false;
			}
			cnt++;

			double y;
			if (!double.TryParse(splitResp[cnt], out y))
			{
				TBWriter.Error("Cant Parse Y coor in findCubes:" + splitResp[cnt]);
				return false;
			}
			cnt++;

			double z;
			if (!double.TryParse(splitResp[cnt], out z))
			{
				TBWriter.Error("Cant Parse Z coor in findCubes:" + splitResp[cnt]);
				return false;
			}
			cnt++;

			cube = new WorldObject(color, new Vector3(x, y, z));
			return true;
		}

		public bool OBj_FND_findcubesEmptyPoint(string direction, out WorldObject emptyPoint)
		{
			emptyPoint = null;

			this.SetupAndSendCommand(JustinaCommands.OBJ_FND_findCubes, "cubestable " + direction);
			if (!this.WaitForResponse(JustinaCommands.OBJ_FND_findCubes, 5000))
				return false;

			string resp = justinaCmdAndResp[(int)JustinaCommands.OBJ_FND_findCubes].Response.Parameters;
			string[] splitResp = resp.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			
			if (splitResp.Length % 4 != 0)
				TBWriter.Error("FindCubes : Parameters Length Error (must be multiple of 4). Params={" + resp + "} , Lenght={" + splitResp.Length + "}");

			int cnt = 0;

			string color = splitResp[cnt];
			cnt++;

			double x;
			if (!double.TryParse(splitResp[cnt], out x))
			{
				TBWriter.Error("Cant Parse X coor in findCubes:" + splitResp[cnt]);
				return false;
			}
			cnt++;

			double y;
			if (!double.TryParse(splitResp[cnt], out y))
			{
				TBWriter.Error("Cant Parse Y coor in findCubes:" + splitResp[cnt]);
				return false;
			}
			cnt++;

			double z;
			if (!double.TryParse(splitResp[cnt], out z))
			{
				TBWriter.Error("Cant Parse Z coor in findCubes:" + splitResp[cnt]);
				return false;
			}
			cnt++;

			emptyPoint = new WorldObject(color, new Vector3(x, y, z));
			return true;
		}

		public bool OBJ_FND_findShelfPlanes(double headAngle, out Vector3[] plane1, out Vector3[] plane2, int timeOut_ms)
		{
			plane1 = null;
			plane2 = null;

			string parameters = MathUtil.ToDegrees(headAngle).ToString("0.00");
			this.SetupAndSendCommand(JustinaCommands.OBJ_FND_findShelfPlanes, "");

			if (!this.WaitForResponse(JustinaCommands.OBJ_FND_findShelfPlanes, timeOut_ms))
				return false;

			string planesInfo = this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FND_findShelfPlanes].Response.Parameters;

			return SimpleTaskPlanner.Parse.FindShelfPlanes(planesInfo, out plane1, out plane2);
		}

		public bool OBJ_FND_findObjectsOnshelf(double headAngle, int timeOut_ms)
		{
			string parameters = "objects" + " " + MathUtil.ToDegrees(headAngle).ToString("0.00");
			this.SetupAndSendCommand(JustinaCommands.OBJ_FND_findObjectsOnshelf, parameters);
			return WaitForResponse(JustinaCommands.OBJ_FND_findObjectsOnshelf, timeOut_ms);
		}

		public bool OBJ_FND_findObjectsOnshelf(string objectToFind, double headAngle, int timeOut_ms)
		{
			string parameters = objectToFind + " " + MathUtil.ToDegrees(headAngle).ToString("0.00");
			this.SetupAndSendCommand(JustinaCommands.OBJ_FND_findObjectsOnshelf, parameters);
			return WaitForResponse(JustinaCommands.OBJ_FND_findObjectsOnshelf, timeOut_ms);
		}

		public bool OBJ_FNDT_findedgetuned(double angleRads, out double x0, out double y0, out double z0, out double x1,
			out double y1, out double z1, int timeOut_ms)
		{
			x0 = 0;
			y0 = 0;
			z0 = 0;
			x1 = 1;
			y1 = 0;
			z1 = 0;
			//El -3 es correción dada por los de visión para ver bien el plano de la mesa
			this.SetupAndSendCommand(JustinaCommands.OBJ_FNDT_findedgetuned, (angleRads * 180 / Math.PI - 3).ToString("0.00"));
			//Thread.Sleep(1000);
			if (!this.WaitForResponse(JustinaCommands.OBJ_FNDT_findedgetuned, timeOut_ms)) return false;

			char[] delimiters = { ' ' };
			string[] parts = this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_findedgetuned].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

			try
			{
				x0 = double.Parse(parts[0]) / 1000;
				y0 = double.Parse(parts[1]) / 1000;
				z0 = double.Parse(parts[2]) / 1000;
				x1 = double.Parse(parts[3]) / 1000;
				y1 = double.Parse(parts[4]) / 1000;
				z1 = double.Parse(parts[5]) / 1000;
			}
			catch
			{
				TextBoxStreamWriter.DefaultLog.WriteLine("CmdMan: Cannot parse response from oft_findedgetuned");
				return false;
			}
			return true;
		}

		public bool OBJ_FNDT_findedgereturns(double angleRads, out double x0, out double y0, out double z0, out double x1, out double y1, out double z1, int timeOut_ms)
		{
			x0 = 0;
			y0 = 0;
			z0 = 0;
			x1 = 1;
			y1 = 0;
			z1 = 0;
			//El -3 es correción dada por los de visión para ver bien el plano de la mesa
			this.SetupAndSendCommand(JustinaCommands.OBJ_FNDT_findedgereturns, (angleRads * 180 / Math.PI - 3).ToString("0.00"));
			//Thread.Sleep(1000);
			if (!this.WaitForResponse(JustinaCommands.OBJ_FNDT_findedgereturns, timeOut_ms)) return false;

			char[] delimiters = { ' ' };
			string[] parts = this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_findedgereturns].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

			try
			{
				x0 = double.Parse(parts[0]) / 1000;
				y0 = double.Parse(parts[1]) / 1000;
				z0 = double.Parse(parts[2]) / 1000;
				x1 = double.Parse(parts[3]) / 1000;
				y1 = double.Parse(parts[4]) / 1000;
				z1 = double.Parse(parts[5]) / 1000;
			}
			catch
			{
				TextBoxStreamWriter.DefaultLog.WriteLine("CmdMan: Cannot parse response from oft_findedgetuned");
				return false;
			}
			return true;
		}

		public bool OBJ_FNDT_findedgefastandfurious(double angleRads, double height, out double x0, out double y0, out double z0, out double x1, out double y1, out double z1, int timeOut_ms)
		{
			x0 = 0;
			y0 = 0;
			z0 = 0;
			x1 = 1;
			y1 = 0;
			z1 = 0;
			//El -3 es correción dada por los de visión para ver bien el plano de la mesa
			this.SetupAndSendCommand(JustinaCommands.OBJ_FNDT_findedgefastandfurious, (angleRads * 180 / Math.PI - 3).ToString("0.00")+" "+(height).ToString("0.00"));
			//Thread.Sleep(1000);
			if (!this.WaitForResponse(JustinaCommands.OBJ_FNDT_findedgefastandfurious, timeOut_ms)) return false;

			char[] delimiters = { ' ' };
			string[] parts = this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_findedgefastandfurious].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

			try
			{
				x0 = double.Parse(parts[0]) / 1000;
				y0 = double.Parse(parts[1]) / 1000;
				z0 = double.Parse(parts[2]) / 1000;
				x1 = double.Parse(parts[3]) / 1000;
				y1 = double.Parse(parts[4]) / 1000;
				z1 = double.Parse(parts[5]) / 1000;
			}
			catch
			{
				TextBoxStreamWriter.DefaultLog.WriteLine("CmdMan: Cannot parse response from oft_findedgetuned");
				return false;
			}
			return true;
		}


		//public bool OBJ_FND_removeTable(int timeOut_ms)
		//{
		//    this.SetupAndSendCommand(JustinaCommands.OBJ_FND_removeTable, "");
		//    return this.WaitForResponse(JustinaCommands.OBJ_FND_removeTable, timeOut_ms);
		//}

		public bool OBJ_FND_findOnTable(int timeOut_ms)
        {
            string parameters = "objects" + " 0 0 640 480";
			this.SetupAndSendCommand(JustinaCommands.OBJ_FND_findOnTable, parameters);
			return WaitForResponse(JustinaCommands.OBJ_FND_findOnTable, timeOut_ms);
        }

		public void OBJ_FND_findontable(double headAngle)
		{
			string parameters = "objects" + " " + MathUtil.ToDegrees(headAngle).ToString("0.00");
			this.SetupAndSendCommand(JustinaCommands.OBJ_FND_findOnTable, parameters);
		}

		public bool OBJ_FND_findontable(double headAngle, int timeOut_ms)
		{
			string parameters = "objects" + " " + MathUtil.ToDegrees(headAngle).ToString("0.00");
			this.SetupAndSendCommand(JustinaCommands.OBJ_FND_findOnTable, parameters);
			return WaitForResponse(JustinaCommands.OBJ_FND_findOnTable, timeOut_ms);
		}

		public bool OBJ_FND_findontable(string objectTofind, double headAngle, int timeOut_ms)
		{
			string parameters = objectTofind + " " + MathUtil.ToDegrees(headAngle).ToString("0.00");
			this.SetupAndSendCommand(JustinaCommands.OBJ_FND_findOnTable, parameters);
			return WaitForResponse(JustinaCommands.OBJ_FND_findOnTable, timeOut_ms);
		}


		public bool OBJ_FND_findArmInSight(out string armFounded, int timeOut_ms)
		{
			this.SetupAndSendCommand(JustinaCommands.OBJ_FND_find, "lhand");
			if (WaitForResponse(JustinaCommands.OBJ_FND_find, 2000))
			{
				armFounded = "left";
				return true;
			}


			this.SetupAndSendCommand(JustinaCommands.OBJ_FND_find, "rhand");
			if (WaitForResponse(JustinaCommands.OBJ_FND_find, 2000))
			{
				armFounded = "right";
				return true;
			}

			armFounded = "";
			return false;
		}

		public bool OBJ_FND_findArm(string armToFind, out Vector3 armPos, int timeOut_ms)
		{
			armPos = new Vector3(Vector3.Zero);

			if ((armToFind != "lhand") && (armToFind != "rhand"))
			{
				TBWriter.Error("OBJ_FND_findArm, invalid parameter ( must be [lhand] or [rhand] )");
				return false;
			}

			armPos = null;
			string armInfo;

			string parameters = armToFind;
			this.SetupAndSendCommand(JustinaCommands.OBJ_FND_find, parameters);

			if (this.WaitForResponse(JustinaCommands.OBJ_FND_find, timeOut_ms))
			{
				armInfo = justinaCmdAndResp[(int)JustinaCommands.OBJ_FND_find].Response.Parameters;
				if (SimpleTaskPlanner.Parse.findArmInfo(armInfo, out armPos))
				{
					return true;
				}
				else
					return false;
			}
			else
				return false;
		}

		public bool OBJ_FND_findLeftArm(int timeOut_ms, out Vector3 armPos)
		{
			armPos = null;
			string armInfo;

			string parameters = "lhand";
			this.SetupAndSendCommand(JustinaCommands.OBJ_FND_find, parameters);

			if (this.WaitForResponse(JustinaCommands.OBJ_FND_find, timeOut_ms))
			{
				armInfo = justinaCmdAndResp[(int)JustinaCommands.OBJ_FND_find].Response.Parameters;
				if (SimpleTaskPlanner.Parse.findArmInfo(armInfo, out armPos))
				{
					return true;
				}
				else
					return false;
			}
			else
				return false;
		}

		public bool OBJ_FND_findRightArm(int timeOut_ms, out Vector3 armPos)
		{
			armPos = null;
			string armInfo;



			string parameters = "rhand";
			this.SetupAndSendCommand(JustinaCommands.OBJ_FND_find, parameters);

			if (this.WaitForResponse(JustinaCommands.OBJ_FND_find, timeOut_ms))
			{
				armInfo = justinaCmdAndResp[(int)JustinaCommands.OBJ_FND_find].Response.Parameters;
				if (SimpleTaskPlanner.Parse.findArmInfo(armInfo, out armPos))
				{
					return true;
				}
				else
					return false;
			}
			else
				return false;
		}

		public bool OBJ_FND_findHumanHands(double HeadAngle_inRad, out Vector3 leftHand, out Vector3 righthand, int timeOut_ms)
		{
			leftHand = Vector3.Zero;
			righthand = Vector3.Zero;

			this.SetupAndSendCommand(JustinaCommands.OBJ_FND_findHumanHands, MathUtil.ToDegrees(HeadAngle_inRad).ToString("0.00"));

			if (!WaitForResponse(JustinaCommands.OBJ_FND_findHumanHands, timeOut_ms))
				return false;

			string handsString = justinaCmdAndResp[(int)JustinaCommands.OBJ_FND_findHumanHands].Response.Parameters;
			double[] handsInfo;

			if (!SimpleTaskPlanner.Parse.string2doubleArray(handsString, out handsInfo))
			{
				TBWriter.Error(" OBJ_FND_findHumanHands, can't Parse info");
				return false;
			}

			if (handsInfo.Length != 6)
			{
				TBWriter.Error(" OBJ_FND_findHumanHands, Invalid lenght in params");
				return false;
			}

			leftHand = new Vector3(handsInfo[0], handsInfo[1], handsInfo[2]);
			righthand = new Vector3(handsInfo[3], handsInfo[4], handsInfo[5]);

			return true;
		}

		public bool OBJ_FIND_findEdge(double headAngleInRadians, out Vector3 point1, out Vector3 point2)
		{
			point1 = null;
			point2 = null;
			double[] pointsInfo;
			string parameters = MathUtil.ToDegrees(headAngleInRadians).ToString("0.000");

			this.SetupAndSendCommand(JustinaCommands.OBJ_FND_findEdge, parameters);
			if (!this.WaitForResponse(JustinaCommands.OBJ_FND_findEdge, 3000))
			{
				return false;
			}

			if (!SimpleTaskPlanner.Parse.string2doubleArray(this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FND_findEdge].Response.Parameters, out pointsInfo))
			{
				TBWriter.Error("OBJ_FND_findEdge response recieved but can't parse response value=[" + this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FND_findEdge].Response.Parameters + "]");
				return false;
			}

			point1 = new Vector3(pointsInfo[0], pointsInfo[1], pointsInfo[2]);
			point2 = new Vector3(pointsInfo[3], pointsInfo[4], pointsInfo[5]);

			/// el sistema de este comando esta en mm y no es el standard

			point1.Y *= -1;
			point1 /= 1000;
			point2.Y *= -1;
			point2 /= 1000;

			double mag1 = point1.Magnitude;
			double mag2 = point2.Magnitude;


			return true;
		}

		public bool OBJ_FIND_setNode(string nodeName)
		{
			bool succes;

			SetupAndSendCommand(JustinaCommands.OBJ_FND_setnode, nodeName);
			succes = WaitForResponse(JustinaCommands.OBJ_FND_setnode, 1000);

			return succes;
		}

		public bool OBJ_FND_getNode(string nodeName, int frameNumber, out Vector3 errorPos, out double errorOr)
		{
			this.SetupAndSendCommand(JustinaCommands.OBJ_FND_getnode, nodeName + " " + frameNumber);

			if (this.WaitForResponse(JustinaCommands.OBJ_FND_getnode, 1000))
			{
				string nodeInfo = this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FND_getnode].Response.Parameters;

				if (SimpleTaskPlanner.Parse.NodeInfo(nodeInfo, out errorPos, out errorOr))
				{
					TBWriter.Error("Can't Parse getNode information");
					return false;
				}

				return true;
			}
			else
			{
				errorPos = null;
				errorOr = double.NaN;
				return false;
			}
		}


		public bool OBJ_FND_saveNodeMap(string nodeMapName)
		{
			SetupAndSendCommand(JustinaCommands.OBJ_FND_saveMap, nodeMapName);
			return WaitForResponse(JustinaCommands.OBJ_FND_saveMap, 5000);
		}

		public bool OBJ_FND_loadNodeMap(string nodeMapName)
		{
			SetupAndSendCommand(JustinaCommands.OBJ_FND_loadMap, nodeMapName);
			return WaitForResponse(JustinaCommands.OBJ_FND_loadMap, 5000);
		}

        public bool OBJ_FND_rememberForObjects(int timeOut_ms)
        {
            int numScenes = 7;

            SetupAndSendCommand(JustinaCommands.OBJ_FND_remember, numScenes.ToString());
            return this.WaitForResponse(JustinaCommands.OBJ_FND_remember, timeOut_ms);
        }


        public void OBJ_FND_findRememberedObjects( string objects, double headAngle_rad)  
        {   
            string parameters = objects + " " + MathUtil.ToDegrees(headAngle_rad).ToString("0.00") + " " + "remembered";
            this.SetupAndSendCommand(JustinaCommands.OBJ_FND_findOnTable, parameters);
        }

        public void OBJ_FND_findRememberedObjects(double headAngle_Rad)
        {
            OBJ_FND_findRememberedObjects("objects", headAngle_Rad);
        }

        public bool OBJ_FND_findRememberedObjects(string objects, double headAngle_rad, int timeOut_ms)
        {
            string parameters = objects+ " " + MathUtil.ToDegrees( headAngle_rad).ToString("0.00") + " "+ "remembered";

            this.SetupAndSendCommand(JustinaCommands.OBJ_FND_findOnTable, parameters);

            return this.WaitForResponse(JustinaCommands.OBJ_FND_findOnTable, timeOut_ms);
        }

		#endregion

		#region Commands TORSO

		public void TRS_relpos(double elevation, double pan)
		{
			string parameters = elevation.ToString("0.00") + " " + pan.ToString("0.00");
			this.SetupAndSendCommand(JustinaCommands.TRS_relpos, parameters);
		}

		public bool TRS_relpos(double elevation, double pan, int timeOut_ms)
		{
			TRS_relpos(elevation, pan);
			return WaitForResponse(JustinaCommands.TRS_relpos, timeOut_ms);
		}

		public void TRS_abspos(double elevation, double pan)
		{
			string parameters = elevation.ToString("0.00") + " " + pan.ToString("0.00");
			this.SetupAndSendCommand(JustinaCommands.TRS_abspos, parameters);
		}

		public bool TRS_abspos(double elevation, double pan, int timeOut_ms)
		{
			TRS_abspos(elevation, pan);
			return WaitForResponse(JustinaCommands.TRS_abspos, timeOut_ms);
		}

		public bool TRS_getPosition(int timeOut_ms, out double[] positions)
		{
			this.SetupAndSendCommand(JustinaCommands.TRS_abspos, "");

			positions = null;

			if (!WaitForResponse(JustinaCommands.TRS_abspos, timeOut_ms))
			{
				return false;
			}

			bool success;

			success = SimpleTaskPlanner.Parse.string2doubleArray(justinaCmdAndResp[(int)JustinaCommands.TRS_abspos].Response.Parameters, out positions);

			if (success)
			{
				TBWriter.Info9("TRS_getPosition successfull Parse");
				TBWriter.Info9("TRS_getPosition elev =" + positions[0].ToString() + " ang = " + positions[1].ToString());
				return true;
			}
			else
			{
				TBWriter.Error("TRS_getPosition CAN'T Parse");
				return false;
			}
		}

		#endregion

		#region Commands PRS-FND

		public void PRS_FND_find(string humanName)
		{
			JustinaCmdAndResps[(int)JustinaCommands.PRS_FND_find].IsResponseReceived = false;
			SetupAndSendCommand(JustinaCommands.PRS_FND_find, humanName);
		}

		public bool PRS_FND_find(string humanName, int timeOut_ms)
		{
			PRS_FND_find(humanName);
			return WaitForResponse(JustinaCommands.PRS_FND_find, timeOut_ms);
		}

		public void PRS_FND_rememeber(string humanName)
		{
			justinaCmdAndResp[(int)JustinaCommands.PRS_FND_remember].IsResponseReceived = false;
			SetupAndSendCommand(JustinaCommands.PRS_FND_remember, humanName);
		}

		public bool PRS_FND_rememeber(string humanName, int timeOut_ms)
		{
			bool success;

			PRS_FND_rememeber(humanName);
			success = WaitForResponse(JustinaCommands.PRS_FND_remember, timeOut_ms);
			return success;
		}

		public void PRS_FND_sleep(bool enable)
		{
			string parameters;
			if (enable) parameters = "enable";
			else parameters = "disable";
			this.SetupAndSendCommand(JustinaCommands.PRS_FND_sleep, parameters);
		}

		public bool PRS_FND_sleep(bool enable, int timeOut_ms)
		{
			this.PRS_FND_sleep(enable);
			return this.WaitForResponse(JustinaCommands.PRS_FND_sleep, timeOut_ms);
		}

		public void PRS_FND_auto(bool enable, string humanName)
		{
			string parameters;

			if (enable)
				parameters = "enable " + humanName;
			else
				parameters = "disable";

			this.justinaCmdAndResp[(int)JustinaCommands.PRS_FND_auto].IsResponseReceived = false;
			this.SetupAndSendCommand(JustinaCommands.PRS_FND_auto, parameters);
		}

		public bool PRS_FND_auto(bool enable, string humanName, int timeOut_ms)
		{
			PRS_FND_auto(enable, humanName);
			return this.WaitForResponse(JustinaCommands.PRS_FND_auto, timeOut_ms);
		}

		#endregion

		#region Commands SP GEN

		public void SPG_GEN_playloop(string fileToPlay)
		{
			SetupAndSendCommand(JustinaCommands.SP_GEN_playloop, fileToPlay);
		}

		public void SPG_GEN_shutup()
		{
			this.SetupAndSendCommand(JustinaCommands.SP_GEN_shutup, "");
		}

		public void SPG_GEN_say(string strToSay)
		{
			this.justinaCmdAndResp[(int)JustinaCommands.SP_GEN_say].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.SP_GEN_say].Command.Parameters = strToSay;
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.SP_GEN_say].Command);
		}

		public bool SPG_GEN_say(string strToSay, int timeOut_ms)
		{
			this.SPG_GEN_say(strToSay);
			return (this.WaitForResponse(JustinaCommands.SP_GEN_say, timeOut_ms));
		}

		public void SPG_GEN_play(string fileToPlay)
        {
            return;
			this.justinaCmdAndResp[(int)JustinaCommands.SP_GEN_play].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.SP_GEN_play].Command.Parameters = fileToPlay;
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.SP_GEN_play].Command);
		}

		public void SPG_GEN_playWait()
		{
			this.justinaCmdAndResp[(int)JustinaCommands.SP_GEN_play].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.SP_GEN_play].Command.Parameters = "wait1.mp3";
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.SP_GEN_play].Command);
            return;
        }

		public void SP_GEN_shutdown()
		{
			SetupAndSendCommand(JustinaCommands.SP_GEN_shutup, "");
		}

		#endregion

		#region Command ST PLN

		public void ST_PLN_alignhuman(string parameters)
		{
			this.SetupAndSendCommand(JustinaCommands.ST_PLN_alignhuman, parameters);
		}

		public bool ST_PLN_alignhuman(string parameters, int timeOut_ms)
		{
			this.ST_PLN_alignhuman(parameters);
			return this.WaitForResponse(JustinaCommands.ST_PLN_alignhuman, timeOut_ms);
		}

		public void ST_PLN_cleantable()
		{
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_cleanplane].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_cleanplane].Command.Parameters = "";
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_cleanplane].Command);
		}

		public bool ST_PLN_cleantable(int timeOut_ms)
		{
			this.ST_PLN_cleantable();
			return this.WaitForResponse(JustinaCommands.ST_PLN_cleanplane, timeOut_ms);
		}

		public void ST_PLN_dopresentation()
		{
			this.SetupAndSendCommand(JustinaCommands.ST_PLN_dopresentation, "");
		}

		public bool ST_PLN_dopresentation(int timeOut_ms)
		{
			this.ST_PLN_dopresentation();
			return this.WaitForResponse(JustinaCommands.ST_PLN_dopresentation, timeOut_ms);
		}

		public void ST_PLN_drop()
		{
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_drop].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_drop].Command.Parameters = "";
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_drop].Command);
		}

		public bool ST_PLN_drop(int timetOut_ms)
		{
			this.ST_PLN_drop();
			return this.WaitForResponse(JustinaCommands.ST_PLN_drop, timetOut_ms);
		}

		public void ST_PLN_drop(string whereToDrop)
		{
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_drop].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_drop].Command.Parameters = whereToDrop;
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_drop].Command);
		}

		public bool ST_PLN_drop(string whereToDrop, int timetOut_ms)
		{
			this.ST_PLN_drop(whereToDrop);
			return this.WaitForResponse(JustinaCommands.ST_PLN_drop, timetOut_ms);
		}

		public void ST_PLN_findhuman(string humanName, string devices)
		{
			this.SetupAndSendCommand(JustinaCommands.ST_PLN_findhuman, devices + " " + humanName);
		}

		public bool ST_PLN_findhuman(string humanName, string devices, int timeOut_ms, out string foundHumanName)
		{
			foundHumanName = "";
			this.ST_PLN_findhuman(humanName, devices);
			if (!this.WaitForResponse(JustinaCommands.ST_PLN_findhuman, timeOut_ms)) return false;
			foundHumanName = this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_findhuman].Response.Parameters;
			return true;
		}

		public void ST_PLN_findobject(string objectName, string devices)
		{
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_findobject].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_findobject].Command.Parameters = devices + " " + objectName;
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_findobject].Command);
		}

		public bool ST_PLN_findobject(string objectName, string devices, int timeOut_ms)
		{
			this.ST_PLN_findobject(objectName, devices);
			return this.WaitForResponse(JustinaCommands.ST_PLN_findobject, timeOut_ms);
		}

		public bool ST_PLN_findobject(string objectName, string devices, int timeOut_ms, out string foundObject)
		{
			foundObject = "";
			this.ST_PLN_findobject(objectName, devices);
			if (!this.WaitForResponse(JustinaCommands.ST_PLN_findobject, timeOut_ms)) return false;
			foundObject = this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_findobject].Response.Parameters;
			return true;
		}

		public void ST_PLN_greethuman(string devices)
		{
			this.SetupAndSendCommand(JustinaCommands.ST_PLN_greethuman, devices);
		}

		public bool ST_PLN_greethuman(string devices, int timeOut_ms)
		{
			this.ST_PLN_greethuman(devices);
			return this.WaitForResponse(JustinaCommands.ST_PLN_greethuman, timeOut_ms);
		}

		public void ST_PLN_rememberhuman(string humanName, string devices)
		{
			this.SetupAndSendCommand(JustinaCommands.ST_PLN_rememberhuman, devices + " " + humanName);
		}

		public bool ST_PLN_rememberhuman(string humanName, string devices, int timeOut_ms)
		{
			this.ST_PLN_rememberhuman(humanName, devices);
			return this.WaitForResponse(JustinaCommands.ST_PLN_rememberhuman, timeOut_ms);
		}

		public void ST_PLN_seehand(string objectToBeRecognized)
		{
			this.SetupAndSendCommand(JustinaCommands.ST_PLN_seehand, objectToBeRecognized);
		}

		public bool ST_PLN_seehand(string objectToBeRecognized, int timeOut_ms)
		{
			this.ST_PLN_seehand(objectToBeRecognized);
			return this.WaitForResponse(JustinaCommands.ST_PLN_seehand, timeOut_ms);
		}

		public void ST_PLN_takeobject()
		{
			this.SetupAndSendCommand(JustinaCommands.ST_PLN_take, "");
		}

		public bool ST_PLN_takeobject(int timeOut_ms)
		{
			this.ST_PLN_takeobject();
			return this.WaitForResponse(JustinaCommands.ST_PLN_take, timeOut_ms);
		}

		public void ST_PLN_takeobject(string objectToTake)
		{
			this.SetupAndSendCommand(JustinaCommands.ST_PLN_take, objectToTake);
		}

		public bool ST_PLN_takeobject(string objectToTake, int timeOut_ms)
		{
			this.ST_PLN_takeobject(objectToTake);
			return this.WaitForResponse(JustinaCommands.ST_PLN_take, timeOut_ms);
		}

		#endregion		
	}

}