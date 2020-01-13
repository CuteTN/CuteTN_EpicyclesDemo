using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace DFT_Epicycles
{
    static class Utility
    {

        /// Euclidean norm of a vector
        static public double VNorm(PointF vector)
        {
            double x = vector.X;
            double y = vector.Y;

            return Math.Sqrt(x*x + y*y);
        }

        /// getting the bounding rectangle of a circle
        static public RectangleF BoundingRectangle(PointF Center, double Radius)
        {
            RectangleF Result = new RectangleF();
            double SideLength = Radius*2;

            Result.Location = new PointF( (float)(Center.X - SideLength/2), (float)(Center.Y - SideLength/2) );
            Result.Size = new SizeF( (float)SideLength, (float)SideLength );
            return Result;
        }

        /// Distance from PointF A to PointF B
        static public double Distance(PointF A, PointF B)
        {
            PointF vector = new PointF(B.X - A.X, B.Y - A.Y);
            return VNorm(vector);
        }

        /// addition of 2 vectors
        static public PointF Plus(PointF A, PointF B)
        {
            return new PointF( A.X+B.X, A.Y+B.Y );
        }

    }
}
