using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.API.PrimitiveSharedVariables;
using Robotics.Mathematics;
using Robotics.Controls;

namespace SimpleTaskPlanner
{
    public class SensorsManager
    {
        private Skeleton[] skeletonsList;
        private string skeletonsString;

        private Human[] persons;

        public SensorsManager()
        {

        }

        public Skeleton[] Skeletons
        {
            get
            {
				skeletonsList = new Skeleton[0];

                if (Parse.Skeletons( skeletonsString, out this.skeletonsList))
                    TextBoxStreamWriter.DefaultLog.Write(9, "Skeletons Shared Variable Successfully Parsed");
                else
                    TextBoxStreamWriter.DefaultLog.Write(1, "ERROR : Skeletons string is null:");

                return this.skeletonsList;
            }
        }

        public void ActualizeSkeletonsSharedVar(string sharedVar)
        {
            if (sharedVar == null)
            {
                TBWriter.Write("ERROR !!! Can't actualize SKELETONS SharedVar : String is null");
                return;
            }

            this.skeletonsString = sharedVar;
            TBWriter.Write(9, "        Successfully actualized SKELETONS sharedVar");
            return;
        }

        public Human[] Persons
        {
            get { return this.persons; }
            set { this.persons = value; }
        }
    }
}
