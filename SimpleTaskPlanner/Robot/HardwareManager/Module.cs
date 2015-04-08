using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleTaskPlanner
{
    public class Module
    {
        private string name;
        private bool isConnected;
        public bool IsAlive;
        public bool IsReady;
        public bool IsBusy;

        public Module(string Name)
        {
            this.name = Name;
            this.IsAlive = false;
            this.IsReady = false;
            this.IsBusy = false;
            this.IsConnected = false;
        }

        public string Name
        {
            get { return this.name; }
        }

        public bool IsConnected
        {
            get { return this.isConnected; }
            set { this.isConnected = value; }
        }

        public override string ToString()
        {
            return " Name:" + Name + " IsConnected:" + this.isConnected.ToString();
        }

        #region Modules XML Names
        public const string ActionPln = "ACT-PLN";
        public const string MovingPln = "MVN-PLN";
        public const string Torso = "TORSO";
        public const string Arms = "ARMS";
        public const string Sensors = "SENSORS";
        public const string SpGenerator = "SP-GEN";
        public const string SpRecorder = "SP-REC";
        public const string PersonFnd = "PRS-FND";
        public const string HumanFnd = "HMN-FND";
        public const string ObjectFnd = "OBJ-FNDT";
        public const string Head = "HEAD";
        public const string Cartographer = "CARTOGRAPHER";
        #endregion
    }
}
