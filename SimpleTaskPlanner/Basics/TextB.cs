using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.Controls;

namespace SimpleTaskPlanner
{
    public static class TBWriter
    {
		public static void Write(string Text)
		{ TextBoxStreamWriter.DefaultLog.WriteLine(0, Text); }

        public static void Write(int priority, string Text)
        { TextBoxStreamWriter.DefaultLog.WriteLine(priority, Text); }

		public static void Error(string Text)
		{ TextBoxStreamWriter.DefaultLog.WriteLine(0, time() + ">>> ERROR :" + Text); }

        public static void Warning1(string Text)
		{ TextBoxStreamWriter.DefaultLog.WriteLine(1, time() + " > Warning lv1 : " + Text); }

        public static void Warning2(string Text)
		{ TextBoxStreamWriter.DefaultLog.WriteLine(2, time() + "   Warning lv2: " + Text); }

		public static void State(int state)
		{ TextBoxStreamWriter.DefaultLog.WriteLine(3, time() + "	Starting State : " + state.ToString()); }

        public static void Info1(string Info)
		{ TextBoxStreamWriter.DefaultLog.WriteLine(3, time() + " > " + Info.ToString()); }

        public static void Info2(string Info)
		{ TextBoxStreamWriter.DefaultLog.WriteLine(4, time() + "	> " + Info.ToString()); }

        public static void Info3(string Info)
		{ TextBoxStreamWriter.DefaultLog.WriteLine(5, time() + "		> " + Info.ToString()); }

		public static void Info4(string Info)
		{ TextBoxStreamWriter.DefaultLog.WriteLine(6, time() + "			>" + Info.ToString()); }

        public static void Info8(string Info)
        { TextBoxStreamWriter.DefaultLog.WriteLine(8, time() + " -" + Info.ToString()); }

        public static void Info9(string Info)
        { TextBoxStreamWriter.DefaultLog.WriteLine(9, time() + " -" + Info.ToString()); }

        public static void SharedVarAct(string text)
		{ TextBoxStreamWriter.DefaultLog.WriteLine(8, time() + "           Actualized Shared Var: " + text.ToUpper()); }


		public static void Spaced(string text)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine(0, "");
			TextBoxStreamWriter.DefaultLog.WriteLine(0, "  " + text);
            TextBoxStreamWriter.DefaultLog.WriteLine(0, ""); 
        }

		public static string time()
		{
			return "[ " + DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString() + ":" + DateTime.Now.Second.ToString() + " ]   ";
		}
	}
}
