using HandHero.Widgets;
using Microsoft.Kinect;
using Microsoft.Samples.Kinect.DepthBasics.Widgets;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace HandHero
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        /// <summary>
        /// Width of output drawing
        /// </summary>
        private const float RenderWidth = 640.0f;

        /// <summary>
        /// Height of our output drawing
        /// </summary>
        private const float RenderHeight = 480.0f;

        /// <summary>
        /// Thickness of drawn joint lines
        /// </summary>
        private const double JointThickness = 50;

        /// <summary>
        /// Thickness of body center ellipse
        /// </summary>
        private const double BodyCenterThickness = 10;

        /// <summary>
        /// Thickness of clip edge rectangles
        /// </summary>
        private const double ClipBoundsThickness = 10;

        /// <summary>
        /// Brush used to draw skeleton center point
        /// </summary>
        private readonly Brush centerPointBrush = Brushes.Blue;

        /// <summary>
        /// Brush used for drawing joints that are currently tracked
        /// </summary>
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

        /// <summary>
        /// Brush used for drawing joints that are currently inferred
        /// </summary>        
        private readonly Brush inferredJointBrush = Brushes.Yellow;

        /// <summary>
        /// Pen used for drawing bones that are currently tracked
        /// </summary>
        private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);

        /// <summary>
        /// Pen used for drawing bones that are currently inferred
        /// </summary>        
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;















        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool Beep(uint dwFreq, uint dwDuration);
        private readonly Border[] tagToBox;
        private Point lastPoint = new Point(0, 0);
        private const int TableRows = 20, TableCols = 20;
        private const int NumberOfColors = 5;
        private readonly Brush[] colors = { Brushes.Red, Brushes.Blue, Brushes.Green, Brushes.Yellow, Brushes.Pink };
        private readonly Border[,] TableArray = new Border[TableRows, TableCols];
        private Border lastBorder = null;
        uint currentBeamIndex1 = 0 , currentBeamIndex2 = 0 ;
        private readonly Random rndGen = new Random();
        List<Shape> enemies = new List<Shape>();
        int shots = 1, misses = 0;
        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;

        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        private WriteableBitmap colorBitmapDepth;

        /// <summary>
        /// Intermediate storage for the depth data received from the camera
        /// </summary>
        private DepthImagePixel[] depthPixels;

        /// <summary>
        /// Intermediate storage for the depth data converted to color
        /// </summary>
        private byte[] colorBitmapDepthImage;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        /// </summary>
        private WriteableBitmap colorBitmap;

        /// <summary>
        /// Intermediate storage for the color data received from the camera
        /// </summary>
        private byte[] colorPixels;

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {


            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            // Display the drawing using our image control
            MainGameView.Source = this.imageSource;
        

            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }
            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                // Turn on the depth stream to receive depth frames
                this.sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);

                // Allocate space to put the depth pixels we'll receive
                this.depthPixels = new DepthImagePixel[this.sensor.DepthStream.FramePixelDataLength];

                // Allocate space to put the color pixels we'll create
                this.colorBitmapDepthImage = new byte[this.sensor.DepthStream.FramePixelDataLength * sizeof(int)];

                // This is the bitmap we'll display on-screen
                this.colorBitmapDepth = new WriteableBitmap(this.sensor.DepthStream.FrameWidth, this.sensor.DepthStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                // Set the image we display to point to the bitmap where we'll put the image data
                this.DepthImage.Source = this.colorBitmapDepth;

                // Add an event handler to be called whenever there is new depth frame data
                this.sensor.DepthFrameReady += this.SensorDepthFrameReady;



                // Turn on the color stream to receive color frames
                this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                // Allocate space to put the pixels we'll receive
                this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];

                // This is the bitmap we'll display on-screen
                this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                // Set the image we display to point to the bitmap where we'll put the image data
                this.ColorImage.Source = this.colorBitmap;

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.ColorFrameReady += this.SensorColorFrameReady;




                // Turn on the skeleton stream to receive skeleton frames
                this.sensor.SkeletonStream.Enable();

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;






                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                this.Data.Content = "Kinect is not ready ";
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
        }

        /// <summary>
        /// Event handler for Kinect sensor's DepthFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    // Copy the pixel data from the image to a temporary array
                    depthFrame.CopyDepthImagePixelDataTo(this.depthPixels);

                    // Get the min and max reliable depth for the current frame
                    int minDepth = depthFrame.MinDepth;
                    int maxDepth = depthFrame.MaxDepth;

                    // Convert the depth to RGB
                    int colorPixelIndex = 0;
                    for (int i = 0; i < this.depthPixels.Length; ++i)
                    {
                        // Get the depth for this pixel
                        short depth = depthPixels[i].Depth;

                        // To convert to a byte, we're discarding the most-significant
                        // rather than least-significant bits.
                        // We're preserving detail, although the intensity will "wrap."
                        // Values outside the reliable depth range are mapped to 0 (black).

                        // Note: Using conditionals in this loop could degrade performance.
                        // Consider using a lookup table instead when writing production code.
                        // See the KinectDepthViewer class used by the KinectExplorer sample
                        // for a lookup table example.

                        byte intensity = (byte)(depth >= minDepth && depth <= maxDepth ? depth : 0);
                        if (intensity < 122)
                        {
                            // Write out blue byte
                            this.colorBitmapDepthImage[colorPixelIndex++] = (byte)(intensity);


                            // Write out green byte
                            this.colorBitmapDepthImage[colorPixelIndex++] = (byte)(intensity * 2);

                            // Write out red byte                        
                            this.colorBitmapDepthImage[colorPixelIndex++] = (byte)(intensity * 8);

                            // We're outputting BGR, the last byte in the 32 bits is unused so skip it
                            // If we were outputting BGRA, we would write alpha here.

                        }
                        else
                        {

                            this.colorBitmapDepthImage[colorPixelIndex++] = 55;


                            // Write out green byte
                            this.colorBitmapDepthImage[colorPixelIndex++] = 0;

                            // Write out red byte                        
                            this.colorBitmapDepthImage[colorPixelIndex++] = 0;

                        }
                        ++colorPixelIndex;
                    }


                    // Write the pixel data into our bitmap
                    this.colorBitmapDepth.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmapDepth.PixelWidth, this.colorBitmapDepth.PixelHeight),
                        this.colorBitmapDepthImage,
                        this.colorBitmapDepth.PixelWidth * sizeof(int),
                        0);
                }
            }
        }


        public MainWindow()
        {
            InitializeComponent();
            tagToBox = new Border[5] { RedBox, BlueBox, GreenBox, YellowBox, PinkBox };
            #region TableReset 
            for (int i = 0; i < TableRows; i++)
            {

                for (int k = 0; k < NumberOfColors; k++)
                {
                    for (int j = 0; j < TableCols / NumberOfColors; j++)
                    {

                        Border brd = new Border();
                        brd.BorderThickness = new Thickness(1);
                        brd.BorderBrush = colors[k];
                        brd.Tag = k;
                        brd.MouseMove += (sender, e) => {
                            Border BSender = (Border)sender;
                            BSender.Background = Brushes.Orange;
                            if (lastBorder is UIElement)
                            {
                                lastBorder.Background = null;
                                tagToBox[(int)lastBorder.Tag].Background = colors[(int)lastBorder.Tag];
                            }
                            this.lastBorder = BSender;
                            tagToBox[(int)BSender.Tag].Background = Brushes.GhostWhite;
                            this.currentBeamIndex1 = this.currentBeamIndex2 = (uint)((int)BSender.Tag);

                        };
                        TableArray[i, k * (TableCols / NumberOfColors) + j] = brd;



                    }

                }

            }
            #endregion

            #region EnemyTimer 
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.03);
            timer.Tick += EnemyCreation;
            timer.Start();
            #endregion

            Table.Children.Add(new CostumTable(TableRows, TableCols, TableArray));



        }

        private void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    // Copy the pixel data from the image to a temporary array
                    colorFrame.CopyPixelDataTo(this.colorPixels);

                    // Write the pixel data into our bitmap
                    this.colorBitmap.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                        this.colorPixels,
                        this.colorBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }
        }


        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];
            

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }

            using (DrawingContext dc = this.drawingGroup.Open())
            {
                // Draw a transparent background to set the render size
                dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

                if (skeletons.Length != 0)
                {
                    foreach (Skeleton skel in skeletons)
                    {
                        RenderClippedEdges(skel, dc);

                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            this.DrawBonesAndJoints(skel, dc);
                        }
                        else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                        {
                            dc.DrawEllipse(
                            this.centerPointBrush,
                            null,
                            this.SkeletonPointToScreen(skel.Position),
                            BodyCenterThickness,
                            BodyCenterThickness);
                        }
                    }
                }

                // prevent drawing outside of our render area
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
            }
        }


        private static void RenderClippedEdges(Skeleton skeleton, DrawingContext drawingContext)
        {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, RenderHeight - ClipBoundsThickness, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, RenderHeight));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(RenderWidth - ClipBoundsThickness, 0, ClipBoundsThickness, RenderHeight));
            }
        }


        /// <summary>
        /// Draws a skeleton's bones and joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext)
        {

            Point posRightHand = this.SkeletonPointToScreen(skeleton.Joints[JointType.HandRight].Position) , 
                posLeftHand = this.SkeletonPointToScreen(skeleton.Joints[JointType.HandLeft].Position);
            
            drawingContext.DrawEllipse(handColRowDetect(this.SkeletonPointToScreen(skeleton.Joints[JointType.HandRight].Position), ref currentBeamIndex2), null, posRightHand, JointThickness, JointThickness);

            drawingContext.DrawEllipse(handColRowDetect(this.SkeletonPointToScreen(skeleton.Joints[JointType.HandLeft].Position), ref currentBeamIndex1), null, posLeftHand, JointThickness, JointThickness);
        

           
          
               
        
        }




        Brush handColRowDetect(Point pos , ref uint prm ) {

           
            Point startingPoint = MainGameView.TranslatePoint(new Point(0, 0), this);
            double Width = MainGameView.ActualWidth, Height = MainGameView.ActualHeight;

            #region DataDisplay 
            Data.Content = pos;
            Data.Content += "\n" + MainGameView.TranslatePoint(new Point(0, 0), MainGameView);
            Data.Content += "\n" + MainGameView.Width + "\n" + MainGameView.Height;
            #endregion

            double colSize = Width / MainWindow.TableCols,
                rowSize = Height / MainWindow.TableRows;

            int col = (int)((pos.X) / colSize),
                row = (int)((pos.Y) / rowSize);

            Data.Content += "\ncol: " + col + "\nrow : " + row;
           if (col >= 0 && row >= 0 && col < MainWindow.TableCols && row < MainWindow.TableRows && TableArray[row, col].Background == null)
            {
              
                prm = (uint)(col / (MainWindow.TableCols/ MainWindow.NumberOfColors) );
                tagToBox[prm].Background = Brushes.GhostWhite;
                return  colors[prm]; 
            }
            return Brushes.OrangeRed; 
           
        

        }

        /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }

        /// <summary>
        /// Draws a bone line between two joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw bones from</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="jointType0">joint to start drawing from</param>
        /// <param name="jointType1">joint to end drawing at</param>
        private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
        {
            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == JointTrackingState.NotTracked ||
                joint1.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }

            // Don't draw if both points are inferred
            if (joint0.TrackingState == JointTrackingState.Inferred &&
                joint1.TrackingState == JointTrackingState.Inferred)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
            {
                drawPen = this.trackedBonePen;
            }

            drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));

        }

        /// <summary>
        /// Handles the checking or unchecking of the seated mode combo box
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void CheckBoxSeatedModeChanged(object sender, RoutedEventArgs e)
        {
            if (null != this.sensor)
            {
                if (this.checkBoxSeatedMode.IsChecked.GetValueOrDefault())
                {
                    this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                }
                else
                {
                    this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
                }
            }
        }

        void EnemyCreation(object sender, EventArgs e)
        {
            int chance = rndGen.Next(10);
            double width = MainGameView.ActualWidth, height = MainGameView.ActualHeight;
            if (chance % 10 == 0)
            {
                for (int i = 0; i < tagToBox.Length; i++)
                {
                    tagToBox[i].Background = colors[i];
                }
                int beamOfEnemy = 0;
                do
                {
                    beamOfEnemy = rndGen.Next(5);
                } while (false);
                
                Data.Content = beamOfEnemy;

               Rectangle enemy = new Rectangle();
               enemy.Width = width / MainWindow.NumberOfColors;
                enemy.Height = height / MainWindow.TableRows * NumberOfColors / 2;
                enemy.Fill = colors[beamOfEnemy];
                enemy.Tag = beamOfEnemy;
                enemy.RadiusX = 30;
                enemy.RadiusY = 30;
                enemy.Stroke = Brushes.DarkGoldenrod; 
                Canvas.SetLeft(enemy, beamOfEnemy * width / MainWindow.NumberOfColors);
                Canvas.SetTop(enemy, -height / MainWindow.TableRows / 8);

                MainRail.Children.Add(enemy);
                enemies.Add(enemy);
            }

            for (int i = 0; i < enemies.Count; i++)
            {
                var enemyToMove = enemies[i];
                double posY = Canvas.GetTop(enemyToMove) + height / MainWindow.TableRows / 8;
                Canvas.SetTop(enemyToMove, posY);
                if (posY > height)
                {

                    enemies[i] = null;
                    MainRail.Children.Remove(enemyToMove);
                    if (currentBeamIndex1 == (int)enemyToMove.Tag || currentBeamIndex2 == (int)enemyToMove.Tag)
                    {
                        tagToBox[(int)enemyToMove.Tag].Background = Brushes.LawnGreen;
                        Mouse2.Fill = Brushes.LawnGreen; 
                        shots++;

                    }
                    else
                    {
                        tagToBox[(int)enemyToMove.Tag].Background = Brushes.Black;
                        Mouse2.Fill = Brushes.IndianRed;
                        misses++;
                    }

                }
            }
            enemies = enemies.FindAll(z => !(z is null));

           // Data.Content = shots + @"\" + (misses + shots);
            Rate.Value = (int)(((float)shots) / (misses + shots) * 100);
        }


    }
}
