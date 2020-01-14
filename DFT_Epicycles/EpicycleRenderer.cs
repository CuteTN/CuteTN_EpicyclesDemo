using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Numerics;

namespace DFT_Epicycles
{
    /// holds the collection of Fourier coefficients 
    /// and renders a frame of simulation to UI
    class EpicycleRenderer
    {
        List<Complex> Coefficients = new List<Complex>();
        List<int> Frequency = new List<int>();
        List<PointF> Stroke = new List<PointF>();

        public Pen PenRadius = new Pen( Color.FromArgb(200, Color.MistyRose), 1);
        public Pen PenCircles = new Pen( Color.FromArgb(100, Color.MistyRose), 2);
        public Pen PenStroke = new Pen(Color.HotPink, 5);

        public bool EnableDrawingCircles = true;
        public bool EnableDrawingRadius = true;
        public bool EnableSmoothStroke = true;

        public bool EnalbeSkippingSmallCircles = true;
        public double SmallRadius = 0.1d;

        public Rectangle Canvas = new Rectangle();


        public EpicycleRenderer( Rectangle Canvas )
        {
            this.Canvas = Canvas;
        }

        bool EnableSortingByMagnitude = true;

        /// quicksort
        private void SortByMagnitude(int L, int R)
        {
            if( L >= R )
                return;

            int i = L;
            int j = R;
            double pivot = this.Coefficients[ (L+R)/2 ].Magnitude;
            
            while(true)
            {
                while( Coefficients[i].Magnitude > pivot )
                    i++;

                while( Coefficients[j].Magnitude < pivot )
                    j--;

                if(i <= j)
                {
                    Complex temp = Coefficients[i];
                    Coefficients[i] = Coefficients[j];
                    Coefficients[j] = temp;

                    int tempf = Frequency[i];
                    Frequency[i] = Frequency[j];
                    Frequency[j] = tempf;

                    i++;
                    j--;
                }

                if(i > j)
                    break;
            }

            SortByMagnitude(L, j);
            SortByMagnitude(i, R);
        }

        public void UpdateFourier(List<Complex> Coefficients)
        {
            this.Coefficients = Coefficients;
            this.Frequency.Clear();
            for(int i = 0; i<Coefficients.Count; i++)
                Frequency.Add(i);
            if( EnableSortingByMagnitude )
                SortByMagnitude(1, Coefficients.Count-1);
            this.Stroke.Clear();
        }

        public void Render(Graphics g, float Time, int CoefsLimit = -1)
        {
            // adjusts coefficients limit
            CoefsLimit = Math.Min(Coefficients.Count, CoefsLimit);
            if( CoefsLimit <= 0 )
                CoefsLimit = Coefficients.Count;
            if( CoefsLimit == 0 )
                return;

            // MessageBox.Show(CoefsLimit.ToString());

            PointF Center = new PointF(0,0);
            PointF ArmPoint = new PointF(0,0);

            for( int i = 0; i < CoefsLimit; i++ )
            {
                if( (Coefficients[i].Magnitude <= SmallRadius) && EnableSortingByMagnitude )
                    break;

                ArmPoint = CalculateCircle(i, Center, Time);
                RenderCircle(g, i, Center, ArmPoint);

                Center = ArmPoint;
            }

            // remember the last armpoint
            Stroke.Add(ArmPoint);
            if(EnableSmoothStroke && Stroke.Count >= 2)
                for(int i = 1; i<Stroke.Count; i++)
                {
                    Pen p = PenStroke.Clone() as Pen;
                    p.Color = Color.FromArgb( (int)(i*200f/Stroke.Count) + 50, p.Color);
                    g.DrawLine(p, Utility.Plus(Stroke[i], Canvas.Location), Utility.Plus(Stroke[i-1], Canvas.Location));
                    g.DrawLine(p, Stroke[i], Stroke[i-1] );
                }        

            if(Stroke.Count >= Coefficients.Count)
                Stroke.RemoveAt(0);
        }

        private PointF CalculateCircle(int index, PointF Center, float Time)
        {
            float Radius = (float)Coefficients[index].Magnitude;
            float Phase0 = (float)Coefficients[index].Phase;
            float Frequency = (float)this.Frequency[index];

            float Phase = Frequency*Time + Phase0;
            PointF ArmPoint = new PointF( );
            ArmPoint.X = Radius * (float)Math.Cos(Phase) + Center.X;
            ArmPoint.Y = Radius * (float)Math.Sin(Phase) + Center.Y;

            return ArmPoint;
        }


        /// draw the i-th circle
        private void RenderCircle(Graphics g, int index, PointF Center, PointF ArmPoint)
        {
            /// R: relatively :)
            PointF CenterR = Utility.Plus(Center, Canvas.Location);
            PointF ArmPointR = Utility.Plus(ArmPoint, Canvas.Location);

            if(EnableDrawingCircles && index > 0)
            {
                try
                {
                    g.DrawEllipse(PenCircles, Utility.BoundingRectangle(CenterR, Utility.Distance(CenterR, ArmPointR) ));
                }
                catch
                {

                }
            }

            if(EnableDrawingRadius && index > 0)
                g.DrawLine(PenRadius, CenterR, ArmPointR);
        }


    }
}
