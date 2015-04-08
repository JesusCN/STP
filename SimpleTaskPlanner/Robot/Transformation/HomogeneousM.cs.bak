using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.Mathematics;
using Robotics.Controls;

namespace SimpleTaskPlanner
{
    public class HomogeneousM
    {
        Matrix homogenMat;
        Matrix inverseMat;
        Matrix rotationMat;
        Matrix translationMat;

        double roll;
        double pitch;
        double yaw;
        double xTrans;
        double yTrans;
        double zTrans;

        double theta;
        double d;
        double alpha;
        double a;

        public HomogeneousM(Matrix Homogeneous)
        {
            this.rotationMat = getRotMatrixFromHomogen(Homogeneous);
            this.translationMat = getTransMatrixFromHomogen(Homogeneous);

            this.homogenMat = setHomogeneousMatrix(this.rotationMat, this.translationMat);
            this.inverseMat = CalculateInverseHomogeneous(this.homogenMat);
        }

        public HomogeneousM(double rollZ, double pitchY, double yawX, double xTranslation, double yTranslation, double zTranslation)
        {
            this.roll = rollZ;
            this.pitch = pitchY;
            this.yaw = yawX;

            this.xTrans = xTranslation;
            this.yTrans = yTranslation;
            this.zTrans = zTranslation;

            this.rotationMat = new Matrix(rotationMatrix(this.roll, this.pitch, this.yaw));
            this.translationMat = new Matrix(translationMatrix(this.xTrans, this.yTrans, this.zTrans));

            this.homogenMat = setHomogeneousMatrix(this.rotationMat, this.translationMat);
            this.inverseMat = CalculateInverseHomogeneous(this.homogenMat);
        }

        public HomogeneousM(double theta, double d, double a, double alpha)
        {
            this.theta = theta;
            this.d = d;
            this.a = a;
            this.alpha = alpha;

            this.homogenMat = CalculateHomogeneousMatrix(theta, d, a, alpha);
            this.inverseMat = CalculateInverseHomogeneous(this.homogenMat);

            this.rotationMat = getRotMatrixFromHomogen(this.homogenMat);
            this.translationMat = getRotMatrixFromHomogen(this.homogenMat);
        }


        public Matrix Matrix
        { get { return this.homogenMat; } }

        public Matrix Inverse
        { get { return this.inverseMat; } }


        public Vector3 Transform(Vector3 position)
        {
            if (position == null)
            {
                TBWriter.Warning2("Cant Transform, Vector3 is Null");
                return null;
            }

            Matrix positionMat = new Matrix(4, 1);

            positionMat[0, 0] = position.X;
            positionMat[1, 0] = position.Y;
            positionMat[2, 0] = position.Z;
            positionMat[3, 0] = 1;

            Matrix resultMat = new Matrix(this.Matrix * positionMat);

            Vector3 resultVec = new Vector3(resultMat[0, 0], resultMat[1, 0], resultMat[2, 0]);

            return resultVec;
        }


        private Matrix CalculateHomogeneousMatrix(double theta, double d, double a, double alpha)
        {
            Matrix HM = new Matrix(4, 4);

            HM[0, 0] = MathUtil.Cos(theta);
            HM[1, 0] = MathUtil.Sin(theta);
            HM[2, 0] = 0;
            HM[3, 0] = 0;

            HM[0, 1] = -(1)*MathUtil.Sin(theta) * MathUtil.Cos(alpha);
            HM[1, 1] = MathUtil.Cos(theta) * MathUtil.Cos(alpha);
            HM[2, 1] = MathUtil.Sin(alpha);
            HM[3, 1] = 0;

            HM[0, 2] = MathUtil.Sin(theta) * MathUtil.Sin(alpha);
            HM[1, 2] = -(1)*MathUtil.Cos(theta) * MathUtil.Sin(alpha);
            HM[2, 2] = MathUtil.Cos(alpha); ;
            HM[3, 2] = 0;

            HM[0, 3] = a * MathUtil.Cos(theta);
            HM[1, 3] = a * MathUtil.Sin(theta);
            HM[2, 3] = d;
            HM[3, 3] = 1;

            return HM;
        }

        private Matrix CalculateInverseHomogeneous(Matrix homogeneousMat)
        {
            if ((homogeneousMat.Rows != 4) || (homogeneousMat.Columns != 4))
            {
                TBWriter.Write("Error : Cant get Inverse Homogeneous Matrix, invalid order");
                return null;
            }

            Matrix rotM = getRotMatrixFromHomogen(homogeneousMat);
            rotM = rotM.Transpose;

            Matrix transM = getTransMatrixFromHomogen(homogeneousMat);
            transM = (-1) * rotM * transM;

            Matrix InversM = setHomogeneousMatrix(rotM, transM);

            return InversM;
        }

        private Matrix setHomogeneousMatrix(Matrix rotMat, Matrix transMat)
        {
            Matrix HM = new Matrix(4, 4);

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    HM[i, j] = rotMat[i, j];
                }
            }

            for (int k = 0; k < 3; k++)
            {
                HM[k, 3] = transMat[k, 0];
            }

            HM[3, 0] = 0;
            HM[3, 1] = 0;
            HM[3, 2] = 0;
            HM[3, 3] = 1;

            return HM;
        }


        private Matrix getTransMatrixFromHomogen(Matrix homogenMat)
        {
            Matrix transMatrix = new Matrix(3, 1);
            
            for (int i = 0; i < 3; i++)
            {
                transMatrix[i, 0] = homogenMat[i, 3];
            }

            return transMatrix;
        }

        private Matrix getRotMatrixFromHomogen(Matrix homogenMat)
        {
            Matrix rotMatrix = new Matrix(3, 3);
    
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    rotMatrix[i, j] = homogenMat[i, j];
                }
            }

            return rotMatrix;
        }


        public static Matrix translationMatrix(double xTranslation, double yTranslation, double zTranslation)
        {
            Matrix transMatrix = new Matrix(3, 1);

            transMatrix[0, 0] = xTranslation;
            transMatrix[1, 0] = yTranslation;
            transMatrix[2, 0] = zTranslation;

            return transMatrix;
        }

        public static Matrix rotationMatrix(double rollAngle, double pitchAngle, double yawAngle)
        {
            Matrix rotMatrix = new Matrix(3, 3);
            rotMatrix = zRollMatrix(rollAngle) * yPitchMatrix(pitchAngle) * xYawMatrix(yawAngle);

            return rotMatrix;
        }

        public static Matrix zRollMatrix(double rollAngle)
        {
            Matrix rollM = new Matrix(3, 3);

            rollM[0, 0] = MathUtil.Cos(rollAngle);
            rollM[0, 1] = -MathUtil.Sin(rollAngle);
            rollM[0, 2] = 0;

            rollM[1, 0] = MathUtil.Sin(rollAngle);
            rollM[1, 1] = MathUtil.Cos(rollAngle);
            rollM[1, 2] = 0;

            rollM[2, 0] = 0;
            rollM[2, 1] = 0;
            rollM[2, 2] = 1;

            return rollM;
        }

        public static Matrix yPitchMatrix(double pitchAngle)
        {
            Matrix pitchM = new Matrix(3, 3);

            pitchM[0, 0] = MathUtil.Cos(pitchAngle);
            pitchM[0, 1] = 0;
            pitchM[0, 2] = MathUtil.Sin(pitchAngle);

            pitchM[1, 0] = 0;
            pitchM[1, 1] = 1;
            pitchM[1, 2] = 0;

            pitchM[2, 0] = -MathUtil.Sin(pitchAngle);
            pitchM[2, 1] = 0;
            pitchM[2, 2] = MathUtil.Cos(pitchAngle);

            return pitchM;
        }

        public static Matrix xYawMatrix(double YawAngle)
        {
            Matrix yawM = new Matrix(3, 3);

            yawM[0, 0] = 1;
            yawM[0, 1] = 0;
            yawM[0, 2] = 0;

            yawM[1, 0] = 0;
            yawM[1, 1] = MathUtil.Cos(YawAngle);
            yawM[1, 2] = -MathUtil.Sin(YawAngle);

            yawM[2, 0] = 0;
            yawM[2, 1] = MathUtil.Sin(YawAngle);
            yawM[2, 2] = MathUtil.Cos(YawAngle);

            return yawM;
        }

    }
}