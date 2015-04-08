using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.Mathematics;
using Robotics.Controls;

namespace SimpleTaskPlanner
{
    public static class Parse
    {
        public static bool Skeletons(string sharedSkeletons, out Skeleton[] skeletons)
        {
            /* The serialized data hast the following form (only hex-characters (0-F), spaces
                 * and dots are only to ilustrate the format):
                 * 0x 0 ... 0 0 ... 0 0 ... 0 0 ... 0 0 ... 0 . . . 0 ... 0 0 ... 0 0 ... 0 0 ... 0
                 * |__________| |_________| |_________| |_________| |_________| |_________| |_________| |_________| |_________|
                 * int32 count int32 ID1 short x short y1 short z1 int32 IDn short xn short yn short zn
                 * 4 bytes 4 bytes 8 bytes 8 bytes 8 bytes 4 bytes 8 bytes 8 bytes
                 * 8 chars 8 chars 16 chars 16 chars 16 chars 8 chars 16 chars 16 chars 16 chars
                 * The length of the string is given by length = 8 + (8+20*12)*count
                 * where count is the number of found persons*/

            if ((sharedSkeletons == null) || (sharedSkeletons == "") || (sharedSkeletons.Length < 256) || ((sharedSkeletons.Length % 2) != 0))
            {
                TBWriter.Error("Can't Parse Skeleton string, string null or invalid length");
                skeletons = null;
                return false;
            }
            
            int count;

            int id; 
            double x;
            double y;
            double z;

            byte[] bytes;

            int ix = 0;

            ix = sharedSkeletons.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase) ? 2 : 0;
            bytes = new byte[(sharedSkeletons.Length  - ix)/2];

            for (int i = 0; i < bytes.Length; ++i, ix += 2)
                bytes[i] = Byte.Parse(sharedSkeletons.Substring(ix, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);

            ix = 0;
            count = BitConverter.ToInt32(bytes, ix);
            ix += 4;

            if ((count < 0) || (((sharedSkeletons.Length - 8) / 248) != count))
            {
                TBWriter.Error("Can't Parse Skeleton string, skeleton lenght error");
                skeletons = null;
                return false;
            }
    
            skeletons = new Skeleton[count];

            for (int i = 0; i < count; i++)
            {
                //The first four bytes are the ID
                id = BitConverter.ToInt32(bytes, ix);
                ix += 4;

                x = BitConverter.ToInt16(bytes, ix);
                ix += 2;

                y = BitConverter.ToInt16(bytes, ix);
                ix += 2;

                z = BitConverter.ToInt16(bytes, ix);
                ix += 2 + 6 * 19;

				

                skeletons[i] = new Skeleton(id, new Vector3(x/1000, y/1000, z/1000));

				TBWriter.Write(9,"Successfully parsed skeleton [ " + skeletons[i].ToString() + " ]");
			}
            return true;
        }

        public static bool ArmPosition(string armPosition, out Vector3 position, out Vector3 orientation, out double elbow)
        {
            string[] armValues;
            char[] separator = { ' ' };

            if (armPosition == null)
            {
                TBWriter.Warning1("Can't parse armPosition, null value");
                position = null;
                orientation = null;
                elbow = 0;
                return false;
            }

            armValues = armPosition.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            if (armValues.Length != 7)
            {
                TBWriter.Warning1("Can't parse armPosition, invalid lenght");

                position = null;
                orientation = null;
                elbow = 0;

                return false;
            }

            double tempX;
            double tempY;
            double tempZ;
            double tempRoll;
            double tempPitch;
            double tempYaw;
            double tempElbow;

            if (!double.TryParse(armValues[0], out tempX))
            {
                TBWriter.Warning1("Can't parse armPosition X");

                position = null;
                orientation = null;
                elbow = 0;

                return false;
            }

            if (!double.TryParse(armValues[1], out tempY))
            {
                TBWriter.Warning1("Can't parse armPosition Y");

                position = null;
                orientation = null;
                elbow = 0;
                
                return false;
            }
            if (!double.TryParse(armValues[2], out tempZ))
            {
				TBWriter.Warning1("Can't parse armPosition Z");

                position = null;
                orientation = null;
                elbow = 0;
                return false;
            }
            if (!double.TryParse(armValues[3], out tempRoll))
            {
				TBWriter.Warning1("Can't parse armPosition Roll");
                position = null;
                orientation = null;
                elbow = 0;
                return false;
            }
            if (!double.TryParse(armValues[4], out tempPitch))
            {
                TBWriter.Warning1("Can't parse armPosition Pitch");

                position = null;
                orientation = null;
                elbow = 0;

                return false;
            }
            if (!double.TryParse(armValues[5], out tempYaw))
            {
                TBWriter.Warning1("Can't parse armPosition Yaw");
                position = null;
                orientation = null;
                elbow = 0;

                return false;
            }
            if (!double.TryParse(armValues[6], out tempElbow))
            {
				TBWriter.Warning1("Can't parse armPosition Elbow");
                position = null;
                orientation = null;
                elbow = 0;

                return false;
            }

            position = new Vector3(tempX, tempY, tempZ);
            orientation = new Vector3(tempRoll, tempPitch, tempYaw);
            elbow = tempElbow;

            TBWriter.Write(9,"Succesfully parsed armPosition [ " + tempX.ToString()     + " " + tempY.ToString()     + " " + tempZ.ToString()   + " " +
																		tempRoll.ToString()  + " " + tempPitch.ToString() + " " + tempYaw.ToString() + " " +
																		tempElbow.ToString() + " ]");
            return true;
        }

        public static bool OdometryPosition(double[] sharedVar, out Vector3 position, out double orientation)
        {
            if ((sharedVar == null) || (sharedVar.Length != 3))
            {
                TBWriter.Warning1("Can't parse odometryPos, null value or invalid lenght");

                position = null;
                orientation = double.NaN;
                return false;
            }

            position = new Vector3(sharedVar[0], sharedVar[1], 0);
            orientation = sharedVar[2];

			TBWriter.Write(9,"Successfully parsed odometryPos [ x= " + position.X.ToString("0.00") + " , y= " + position.Y.ToString("0.00") + " , orientation= " + orientation.ToString("0.00") + " ]" );
            return true;
        }

		public static bool OdometryPosition(string parameters, out Vector3 position, out double orientation)
		{
			position = null;
			orientation = double.NaN;

			string[] odometry;
			char[] separator = { ' ' };

			odometry = parameters.Split(separator, StringSplitOptions.RemoveEmptyEntries);

			if (odometry.Length == 0)
			{
				TBWriter.Warning1("Cant parse odometryPos, info lenght is 0");
				return false;
			}

			double tempX;
			double tempY;
			double tempOrientation;

			if (!double.TryParse(odometry[0], out tempX))
			{
				TBWriter.Warning1("Cant Parse Position X");
				position = null;
				return false;
			}
			if (!double.TryParse(odometry[1], out tempY))
			{
				TBWriter.Warning1("Cant Parse Position T");
				return false;
			}
			if (!double.TryParse(odometry[2], out tempOrientation))
			{
				TBWriter.Warning1("Cant Parse Orientation");
				position = null;
				return false;
			}

			position = new Vector3(tempX, tempY, 0);
			orientation = tempOrientation;

			TBWriter.Write(9, "Successfully parsed odometryPos [ x= " + position.X.ToString("0.00") + " , y= " + position.Y.ToString("0.00") + " , orientation= " + orientation.ToString("0.00") + " ]");
			return true;
		}

        public static bool HeadPosition(double[] sharedVar, out double pan, out double tilt)
        {
            if ((sharedVar == null) || (sharedVar.Length != 2))
            {
                TBWriter.Error("Can't parse headPos, null value or invalid lenght");

                pan = double.NaN;
                tilt = double.NaN;

                return false;
            }

            pan = sharedVar[0];
            tilt = sharedVar[1];

			TBWriter.Write(9, "Successfully parsed headPos [ pan=" + pan.ToString("0.00") + " , tilt=" + tilt.ToString("0.00") + " ]");
            return true;
        }

        public static bool TorsoPosition(double[] sharedVar, out double elevation, out double pan)
        {
            if ((sharedVar == null) || (sharedVar.Length != 2))
            {
                TBWriter.Warning1("Can't parse torsoPos, null value or invalid lenght");

                elevation = 0.7;
                pan = 0.0;

                return false;
            }

            elevation = sharedVar[0];
            pan = sharedVar[1];

			TBWriter.Write(9, "Successfully parsed torsoPos [ elevation=" + elevation.ToString("0.00") + " , pan=" + pan.ToString("0.00") + " ]");
            return true;
        }

		public static bool NodeInfo( string nodeInfo, out Vector3 errorPosition, out double errorOrientation)
		{
			errorPosition = null;
			errorOrientation = double.NaN;

			string[] nodeError;
			char[] separator = { ' ' };

			nodeError = nodeInfo.Split(separator, StringSplitOptions.RemoveEmptyEntries);

			if (nodeError.Length == 0)
			{
				TBWriter.Warning1("Cant parse nodeInfo, lenght is 0");
				return false;
			}

			double tempX;
			double tempY;
			double tempOrientation;

			if (!double.TryParse(nodeError[0], out tempX))
			{
				TBWriter.Warning1("Can't parse  errorX");
				return false;
			}
			if (!double.TryParse(nodeError[1], out tempY))
			{
				TBWriter.Warning1("Cant parse errorY");
				return false;
			}
			if (!double.TryParse(nodeError[2], out tempOrientation))
			{
				TBWriter.Warning1("Can't parse errorOrientation");
				return false;
			}

			errorPosition = new Vector3(tempX, tempY, 0);
			errorOrientation = tempOrientation;

			TBWriter.Write(9,"Successfully parsed NodeInfo [ x=" + tempX.ToString() + ", y=" + tempY.ToString() + ", orientation=" + tempOrientation.ToString() + " ]" );
			return true;
		}

		public static bool string2doubleArray(string stringInfo, out double[] doubleArray)
		{
			if (string.IsNullOrEmpty(stringInfo))
			{
				doubleArray = null;
				return false;
			}

			string[] values = stringInfo.Split(null as char[], StringSplitOptions.RemoveEmptyEntries);

			if ((values == null) || (values.Length == 0))
			{
				doubleArray = null;
				return false;
			}

			doubleArray = new double[values.Length];

			for (int i = 0; i < values.Length; i++)
			{
				if (!double.TryParse(values[i], out doubleArray[i]))
				{
					doubleArray = null;
					return false;
				}
			}

			return true;
		}
      

		/// <summary>
		/// Try to Parse human info from minoru, 
		/// </summary>
		/// <param name="humanInfo">string with information</param>
		/// <param name="humanName"></param>
		/// <param name="facePosition"></param>
		/// <returns></returns>
		public static bool humanInfo(string humanInfo, out string humanName, out Vector3 facePosition,out double humanPan,out double humanTilt)
		{
			humanPan = double.MaxValue;
			humanTilt = double.MaxValue;

			double distanceToPerson = 1;

			string[] parts = humanInfo.Split(null as char[], StringSplitOptions.RemoveEmptyEntries);

			if (parts.Length == 1)
			{
				humanName = parts[0];

				facePosition = new Vector3(0, 0, distanceToPerson);
				TBWriter.Write(9, "Successfully parsed humanInfo [ Name:" + humanName + " , FacePos:" + facePosition.ToString() + " ]");

				humanPan = 0;
				humanTilt = 0;

				return true;
			}

			if (parts.Length == 3)
			{
				double pan;
				double tilt;

				if (!Double.TryParse(parts[1], out pan))
				{
					TBWriter.Warning1("humanInfo can't parse pan");

					humanName = "";
					facePosition = null;

					return false;
				}
				if (!Double.TryParse(parts[2], out tilt))
				{
					TBWriter.Warning1("humanInfo can't parse tilt");

					humanName = "";
					facePosition = null;

					return false;
				}


				humanPan = pan;
				humanTilt = tilt;

				humanName = parts[0];

				facePosition = new Vector3( Vector3.SphericalToCartesian( new Vector3( distanceToPerson,  pan, tilt)));
				
				TBWriter.Write(9, "Successfully parsed humanInfo [ Name:" + humanName + " , FacePos:" + facePosition.ToString() + " ]");

				return true;
			}
			

			if (parts.Length == 4)
			{
				double pan;
				double tilt;

				if (!Double.TryParse(parts[2], out pan))
				{
					TBWriter.Warning1("humanInfo can't parse pan");

					humanName = "";
					facePosition = null;

					return false;
				}
				if (!Double.TryParse(parts[3], out tilt))
				{
					TBWriter.Warning1("humanInfo can't parse tilt");

					humanName = "";
					facePosition = null;

					return false;
				}


				humanPan = pan;
				humanTilt = tilt;

				humanName = parts[1];

				facePosition = new Vector3(Vector3.SphericalToCartesian(new Vector3(distanceToPerson, pan, tilt)));

				TBWriter.Write(9, "Successfully parsed humanInfo [ Name:" + humanName + " , FacePos:" + facePosition.ToString() + " ]");

				return true;
			}

			TBWriter.Warning1("humanInfo can't parse, parts.lenght < 0 ");

			humanName = "";
			facePosition = null;

			return false;
		}

        public static bool findArmInfo(string information, out Vector3 position)
        {
            position = null;

            string[] armInfo;
            char[] separator = { ' ' };

            armInfo = information.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            if (armInfo.Length == 0)
            {
				TBWriter.Warning1("findArmInfo can't parse, lenght is 0");
                return false;
            }

            double tempX;
            double tempY;
            double tempZ;

            if (!double.TryParse(armInfo[1], out tempX))
            {
				TBWriter.Warning1("findArmInfo can't parse position X");
                position = null;
                return false;
            }


            if (!double.TryParse(armInfo[2], out tempY))
            {
				TBWriter.Warning1("findArmInfo can;t parse position Y");
                position = null;
                return false;
            }

            if (!double.TryParse(armInfo[3], out tempZ))
            {
				TBWriter.Warning1("findArmInfo can't parse position Z");
                position = null;
                return false;
            }

            position = new Vector3(tempX, tempY, tempZ);

			TBWriter.Write(9,"Succesfully parsed findArmInfo [ " + position.ToString() + " ]" );
            return true;
        }

        public static bool FindObjectOnTableInfo(string objectsString, out WorldObject[] objects )
        {
            string name;
            double tempX;
            double tempY;
            double tempZ;
            double distanceFromTable;
            int numberOfObjectsFounded;
           
            string[] objInfo;
            char[] separator = { ' ' };

            objInfo = objectsString.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            if( objInfo.Length < 1 )
            {
                TBWriter.Warning1("FindObjectsOnTableInfo can't parse, string lenght is 0 ");
                objects = null;
                return false;
            }
            if (objInfo.Length % 5 != 0)
            {
                TBWriter.Warning1("FindObjectsOnTableInfo can't parse, objects.lenght%5 != 0");
                objects = null; 
                return false;
            }

            numberOfObjectsFounded = objInfo.Length / 5;
            objects = new WorldObject[numberOfObjectsFounded];
            
            int i=0;
            for (int j = 0; j < objects.Length; j++)
            {
                name = objInfo[i];
                
                i++;
                if (!double.TryParse(objInfo[i], out tempX))
                {
                    TBWriter.Error("FindObjectsOnTableInfo, can't parse position X");
                    objects = null;
                    return false;
                }
                
                i++;
                if (!double.TryParse(objInfo[i], out tempY))
                {
                    TBWriter.Error("FindObjectsOnTableInfo, can't parse position Y");
                    objects = null;
                    return false;
                }
                
                i++;
                if (!double.TryParse(objInfo[i], out tempZ))
                {
                    TBWriter.Error("FindObjectsOnTableInfo, can't parse position Z");
                    objects = null;
                    return false;
                }
                
                i++;
                if (!double.TryParse(objInfo[i], out distanceFromTable))
                {
                    TBWriter.Error("FindObjectsOnTableInfo, can't parse distance from Table");
                    objects = null;
                    return false;
                }

                i++;

                WorldObject newObject = new WorldObject(name, new Vector3(tempX, tempY, tempZ), distanceFromTable);
				TBWriter.Write(9,"Successfully parsed FindObjectsOnTableInfo [ " + newObject.ToString() + " ]");
                objects[j] = newObject;
            }
            return true;
        }


        public static bool FindShelfPlanes(string planeInfo, out Vector3[] plane1, out Vector3[] plane2)
        {
            plane1 = null;
            plane2 = null;

            double[] doubleArray;


            if (!string2doubleArray(planeInfo, out doubleArray))
                return false;

            if (doubleArray.Length != 24)
                return false;

            plane1 = new Vector3[4];
            plane2 = new Vector3[4];

            Vector3 corner;

            double tempx;
            double tempy;
            double tempz;

            int j;
            int i = 0;

            j = 0;
            for ( i = 0; i < 12; i++)
            {
                tempx = doubleArray[i];
                i++;
                tempy = doubleArray[i];
                i++;
                tempz = doubleArray[i];
                

                plane1[j] = new Vector3(tempx, tempy, tempz);
                j++;
            }


            j = 0;
            for ( i = 12; i < 24; i++)
            {
                tempx = doubleArray[i];
                i++;
                tempy = doubleArray[i];
                i++;
                tempz = doubleArray[i];
                

                plane2[j] = new Vector3(tempx, tempy, tempz);
                j++;
            }

            return true;
        }

    }
}