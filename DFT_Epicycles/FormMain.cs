using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;

namespace DFT_Epicycles
{
    public partial class FormMain : Form
    {
        Rectangle DrawingCanvas;
        Rectangle SimulationCanvas;
        Rectangle SettingCanvas;

        Pen PenUserDrawing = new Pen(Color.PapayaWhip, 5);


        public FormMain()
        {
            InitializeComponent();
            CustomInitialization();
        }

        private void CustomInitialization()
        {
            this.DoubleBuffered = true;

            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormMain_KeyDown_HandleHotKey);

            this.Paint += new System.Windows.Forms.PaintEventHandler(this.FormMain_Paint_CanvasSeparating);

            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.FormMain_MouseMove_UserDrawing);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FormMain_MouseUp_UserDrawing);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FormMain_MouseDown_UserDrawing);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.FormMain_MouseMove_UserDeleting);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FormMain_MouseUp_UserDeleting);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FormMain_MouseDown_UserDeleting);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.FormMain_Paint_UserDrawing);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.FormMain_Paint_UserDeleting);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.FormMain_Paint_Simulation);

            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.FormMain_MouseMove_CoefLimitSetting);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FormMain_MouseUp_CoefLimitSetting);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FormMain_MouseDown_CoefLimitSetting);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.FormMain_Paint_CoefLimitSetting);

            this.Paint += new System.Windows.Forms.PaintEventHandler(this.FormMain_Paint_UserLog);

            this.SimulationTimer.Tick += SimulationTimer_Tick_UpdateFrame;
            this.mspf = 10;

            SetCanvas();
            EpicycleRenderer = new EpicycleRenderer( SimulationCanvas );
        }

        private void SetCanvas()
        {
            const int SettingHeight = 30;

            DrawingCanvas.Location = new Point(0, 0);
            DrawingCanvas.Size = new Size(this.Width/2, this.Height - SettingHeight);

            SimulationCanvas.Location = new Point(this.Width/2, 0);
            SimulationCanvas.Size = new Size(this.Width/2, this.Height - SettingHeight);
            if( this.EpicycleRenderer != null )
                this.EpicycleRenderer.Canvas = SimulationCanvas;

            SettingCanvas.Location = new Point(0, this.Height-SettingHeight);

            // if size has changed, reset SettingButtonPos
            if(this.Width != SettingCanvas.Width)
                SettingButtonPos = this.Width;

            SettingCanvas.Size = new Size(this.Width, SettingHeight);
        }

        private void PaintCanvas(Graphics g)
        {
            SetCanvas(); 

            Pen p = new Pen(Color.LightGreen, 5);

            g.DrawRectangle( p, DrawingCanvas );
            g.DrawRectangle( p, SimulationCanvas );
            g.DrawRectangle( p, SettingCanvas );
        }

        private void FormMain_Paint_CanvasSeparating(object sender, PaintEventArgs e)
        {
            PaintCanvas(e.Graphics);
        }

        // USER LOG ////////////////////////////////////////////////////////////////////////////////////////////////// 
        string LogMsg = "";
        int LogFrameLeft = 0;

        private void UserLog(string msg)
        {
            LogMsg = msg;
            LogFrameLeft = 100;
        }

        private void FormMain_Paint_UserLog(object sender, PaintEventArgs e)
        {
            if( LogFrameLeft == 0 )
                return;

            Graphics g = e.Graphics;
            Pen p = new Pen( Color.FromArgb(200, Color.White), 3);
            Brush b = new SolidBrush( Color.FromArgb(100, Color.White ) );
            Rectangle rect = new Rectangle(0, 0, this.Width, 30);

            g.DrawRectangle(p, rect);
            g.FillRectangle(b, rect);

            Font font = new Font("Arial", 15);
            b = new SolidBrush( Color.Black );
            g.DrawString(LogMsg, font, b, new PointF(0, 5) );
            LogFrameLeft--;
        }
        // HOTKEY HANDLING ////////////////////////////////////////////////////////////////////////////////////////////////// 
        private void HandleHotKey(KeyEventArgs e)
        {
            switch(e.KeyCode)
            {
                case Keys.Escape:
                {
                    Application.Exit();
                    break;
                }
                case Keys.Back:
                {
                    Stroke.Clear();
                    break;
                }
                case Keys.Left:
                {
                    AdjustCirclesLimit(-1);
                    UserLog("Circle Limit is set to: " + (CalculateCircleLimit()-1) );
                    break;
                }
                case Keys.Right:
                {
                    AdjustCirclesLimit(1);
                    UserLog("Circle Limit is set to: " + (CalculateCircleLimit()-1) );
                    break;
                }
                case Keys.Add:
                {
                    if(EraserRadius < 500)
                        EraserRadius += 5;
                    break;
                }
                case Keys.Subtract:
                {
                    if(EraserRadius > 0)
                        EraserRadius -= 5;
                    break;
                }
                case Keys.Down:
                {
                    if( e.Shift )
                        mspf += 5;
                    else
                        mspf += 1;
                    UserLog("Time per frame: " + mspf + "ms");
                    break;
                }
                case Keys.Up:
                {
                    if( e.Shift )
                        mspf -= 5;
                    else
                        mspf -= 1;
                    UserLog("Time per frame: " + mspf + "ms");
                    break;
                }
                case Keys.R:
                {
                    EnableDrawingRadius = ! EnableDrawingRadius;
                    UserLog("Draw radius: turned " + (EnableDrawingRadius? "ON":"OFF") );
                    break;
                }
                case Keys.C:
                {
                    EnableDrawingCircles = ! EnableDrawingCircles;
                    UserLog("Draw circles: turned " + (EnableDrawingCircles? "ON":"OFF") );
                    break;
                }
                case Keys.P:
                {
                    SimulationIsPlaying = ! SimulationIsPlaying;
                    break;
                }
            }
        }

        private void FormMain_KeyDown_HandleHotKey(object sender, KeyEventArgs e)
        {
            HandleHotKey(e);
        }

        // USER DRAWING AND DELETING /////////////////////////////////////////////////////////////////////////////////////////////  

        bool UserIsDrawing = false;
        bool UserIsDeleting = false;

        List<PointF> Stroke = new List<PointF>();
        public float MinPointsDistance = 5;

        double EraserRadius = 20;

        private bool AddPoint(PointF point)
        {
            if( Stroke.Count >= 1)
            {
                double d = Utility.Distance(point, Stroke.Last());
                // if 2 points are too close, do not add
                if( d <= MinPointsDistance )
                    return false;
            }

            if( Stroke.Count >= 300000 )
            {
                // if the stroke is too loooong, do not add
                return false;
            }

            Stroke.Add(point);
            return true;
        }

        private void FormMain_MouseDown_UserDrawing(object sender, MouseEventArgs e)
        {
            if( UserIsDeleting )
                return;

            if( e.Button == MouseButtons.Left && DrawingCanvas.Contains(Cursor.Position) )
            {
                UserIsDrawing = true;
                AddPoint(Cursor.Position);
                this.Refresh();
            }
        }

        private void FormMain_MouseMove_UserDrawing(object sender, MouseEventArgs e)
        {
            if( UserIsDrawing && DrawingCanvas.Contains(Cursor.Position) )
            {
                AddPoint(Cursor.Position);
                this.Refresh();
            }
        }

        private void FormMain_MouseUp_UserDrawing(object sender, MouseEventArgs e)
        {
            if( e.Button != MouseButtons.Left )
                return;
            
            UserIsDrawing = false;

            this.UpdateChange();
            this.Refresh();
        }

        private void FormMain_MouseDown_UserDeleting(object sender, MouseEventArgs e)
        {
            if( UserIsDrawing )
                return;

            if( e.Button == MouseButtons.Right && DrawingCanvas.Contains(Cursor.Position) )
            {
                UserIsDeleting = true;
                this.Refresh();
            }
        }

        private void FormMain_MouseMove_UserDeleting(object sender, MouseEventArgs e)
        {
            if( UserIsDeleting && DrawingCanvas.Contains(Cursor.Position) )
            {
                for(int i = Stroke.Count-1; i>=0 ; i--)
                {
                    if( Utility.Distance(Cursor.Position, Stroke[i]) <= 10 + EraserRadius )
                        Stroke.RemoveAt(i);
                }
                this.Refresh();
            }
        }

        private void FormMain_MouseUp_UserDeleting(object sender, MouseEventArgs e)
        {
            if( e.Button != MouseButtons.Right )
                return;
            
            UserIsDeleting = false;

            this.UpdateChange();
            this.Refresh();
        }

        private void FormMain_Paint_UserDrawing(object sender, PaintEventArgs e)
        {
            if(Stroke == null )
                return;
            if(Stroke.Count <= 1)
                return;

            Graphics g = e.Graphics;
            g.DrawLines(PenUserDrawing, Stroke.ToArray());
            g.DrawLine(PenUserDrawing, Stroke.First(), Stroke.Last());
        }

        private void FormMain_Paint_UserDeleting(object sender, PaintEventArgs e)
        {
            if(! UserIsDeleting )
                return;

            Graphics g = e.Graphics;
            Pen p = new Pen(PenUserDrawing.Color, 1);

            g.DrawEllipse( p, Utility.BoundingRectangle(Cursor.Position, EraserRadius) );
            foreach(var Point in Stroke)
                g.DrawEllipse( p, Utility.BoundingRectangle(Point, 10) );
        }

        // USER COEF LIMIT SETTING //////////////////////////////////////////////////////////////////////////////////////////////////  
        bool UserIsSettingCoefLimit = false;
        
        int SettingButtonPos;


        private void FormMain_MouseDown_CoefLimitSetting(object sender, MouseEventArgs e)
        {
            if( e.Button == MouseButtons.Left && SettingCanvas.Contains(Cursor.Position) )
            {
                UserIsSettingCoefLimit = true;
                SettingButtonPos = Cursor.Position.X;
                this.Refresh();
            }
        }

        private void FormMain_MouseMove_CoefLimitSetting(object sender, MouseEventArgs e)
        {
            if( UserIsSettingCoefLimit )
            {
                SettingButtonPos = Cursor.Position.X;
                this.Refresh();
            }
        }

        private void FormMain_MouseUp_CoefLimitSetting(object sender, MouseEventArgs e)
        {
            if( e.Button != MouseButtons.Left )
                return;
            
            UserIsSettingCoefLimit = false;
            this.Refresh();
        }

        private void FormMain_Paint_CoefLimitSetting(object sender, PaintEventArgs e)
        {
            Pen p = new Pen(Color.LightSkyBlue, 10);
            PointF Center = new PointF(SettingButtonPos, SettingCanvas.Location.Y + SettingCanvas.Height/2);
            e.Graphics.DrawEllipse(p, Utility.BoundingRectangle(Center, 10) );
        }


        // SIMULATION //////////////////////////////////////////////////////////////////////////////////////////////////  
        List<Complex> FourierCoefs = new List<Complex>();

        EpicycleRenderer EpicycleRenderer; 

        Timer SimulationTimer = new Timer();
        /// milisecond per frame
        public int mspf
        {
            get
            {
                return this.SimulationTimer.Interval;
            }
            set
            {
                if(value > 0)
                    this.SimulationTimer.Interval = value;
                else
                    this.SimulationTimer.Interval = 1;
            }
        }
        private int Frame = 0;

        private bool simulationIsPlaying = true;
        public bool SimulationIsPlaying
        {
            get { return simulationIsPlaying; }
            set
            {
                simulationIsPlaying = value;
                if(value)
                    SimulationTimer.Start();
                else
                    SimulationTimer.Stop();
            }
        }


        private void UpdateChange()
        {
            FourierCoefs = DiscreteFourierTransform.Coefficients(Stroke);
            EpicycleRenderer.UpdateFourier(FourierCoefs);
            Frame = 0;
            SimulationTimer.Start();
        }
        
        private void FormMain_Paint_Simulation(object sender, PaintEventArgs e)
        {
            if( UserIsDeleting || UserIsDrawing )
                return;
            if(! SimulationIsPlaying )
                return;

            Graphics g = e.Graphics;
            float Time = CalculateTime();
            int CircleLimit = CalculateCircleLimit();
            EpicycleRenderer.Render(g, Time, CircleLimit );
        }

        private void SimulationTimer_Tick_UpdateFrame(object sender, EventArgs e)
        {
            Frame++;
            this.Refresh();
        }

        private void AdjustCirclesLimit(int delta)
        {
            SettingButtonPos += (int)( (double)delta*SettingCanvas.Width/FourierCoefs.Count );

            if( SettingButtonPos > SettingCanvas.Width )
                SettingButtonPos = SettingCanvas.Width;

            if( SettingButtonPos < 0 )
                SettingButtonPos = 0;
        }

        private int CalculateCircleLimit()
        {
            return (int)Math.Ceiling( (double)SettingButtonPos*( FourierCoefs.Count - 2 )/SettingCanvas.Width ) + 2;
        }

        private float CalculateTime()
        {
            return (float)(2*Frame*Math.PI/FourierCoefs.Count);
        }

        bool EnableDrawingCircles
        {
            get
            {
                return EpicycleRenderer.EnableDrawingCircles;
            }
            set
            {
                EpicycleRenderer.EnableDrawingCircles = value;
            }
        }

        bool EnableDrawingRadius
        {
            get
            {
                return EpicycleRenderer.EnableDrawingRadius;
            }
            set
            {
                EpicycleRenderer.EnableDrawingRadius = value;
            }
        }


    

    }
}
