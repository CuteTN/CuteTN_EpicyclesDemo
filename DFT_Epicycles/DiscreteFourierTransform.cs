using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;

namespace DFT_Epicycles
{
    /// Provides functions to transform discrete points into Fourier coefficients
    static class DiscreteFourierTransform
    {
        static public bool UsesFFT = false;
        static public int NumberOfCoefsLimit = 1000000;


        /// transform discrete points into Fourier coefficients
        /// NumberOfCoef : number of coefficients to calculate, equal to the number of points by default
        static public List<Complex> Coefficients(List<PointF> Points, int NumberOfCoefs = -1)
        {
            if(Points == null)
                return new List<Complex>();

            List<PointF> AdjustedPoints;
            if( EnableFillInFarPoints )
                AdjustedPoints = FillGaps(Points);
            else
                AdjustedPoints = new List<PointF>(Points);

            return NormalTransform(AdjustedPoints, NumberOfCoefs);
        }

        static private List<Complex> NormalTransform(List<PointF> Points, int NumberOfCoefs = -1)
        {
            List<Complex> Result = new List<Complex>();

            if (NumberOfCoefs <= 0)
                NumberOfCoefs = Math.Min(NumberOfCoefsLimit, Points.Count);

            for(int k = 0; k < NumberOfCoefs; k++)
            {
                Complex Xk = new Complex(0,0);
                
                for( int i = 0; i < Points.Count; i++ )
                {
                    float phi = (float)( 2*Math.PI*k*i/Points.Count );
                    Complex c1 = new Complex( Points[i].X, Points[i].Y );
                    Complex c2 = new Complex( Math.Cos(phi), -Math.Sin(phi) );
                    Xk += c1*c2;
                }

                Xk /= Points.Count;

                Result.Add(Xk);
            }

            return Result;
        }


        // FILLING GAPS //////////////////////////////////////////////////////////////////////////////////////////////////// 
        static public float GapsFillingDistance = 15;
        static bool EnableFillInFarPoints = true;

        static private List<PointF> FillGaps(List<PointF> Stroke)
        {
            // immutable data
            List<PointF> Result = new List<PointF>(Stroke);

            if(Result.Count <= 1)
                return Result;
            Result.Insert( 0, Result.Last() );

            for(int i = Result.Count - 1; i >= 1; i--)
            {
                if( Utility.Distance( Result[i], Result[i-1] ) > 2*GapsFillingDistance )
                {
                    int interval = (int)( Utility.Distance( Result[i], Result[i-1] ) / GapsFillingDistance );
                    float dx = (Result[i].X - Result[i-1].X) / interval;
                    float dy = (Result[i].Y - Result[i-1].Y) / interval;

                    for(int j=interval-1; j>0; j--)
                    {
                        Result.Insert(i, new PointF(Result[i-1].X + j*dx, Result[i-1].Y + j*dy) );
                    }
                }

            }

            Result.RemoveAt(0);
            return Result;
        }

        

    }
}
