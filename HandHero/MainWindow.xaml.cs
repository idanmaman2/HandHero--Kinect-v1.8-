using HandHero.Widgets;
using Microsoft.Kinect;
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
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool Beep(uint dwFreq, uint dwDuration);
        private readonly Border []  tagToBox ; 
        private Point lastPoint = new Point(0, 0); 
        private const int TableRows = 20, TableCols = 20;
        private const int NumberOfColors = 5;
        private readonly Brush[] colors = { Brushes.Red, Brushes.Blue, Brushes.Green, Brushes.Yellow , Brushes.Pink };
        private readonly Border[,] TableArray = new Border[TableRows, TableCols];
        private Border lastBorder = null;
        uint currentBeamIndex = 0;
        private readonly Random rndGen = new Random(); 
        List<Rectangle> enemies =new List<Rectangle>();
        int shots = 1, misses = 0;
        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;

        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        private WriteableBitmap colorBitmap;

        /// <summary>
        /// Intermediate storage for the depth data received from the camera
        /// </summary>
        private DepthImagePixel[] depthPixels;

        /// <summary>
        /// Intermediate storage for the depth data converted to color
        /// </summary>
        private byte[] colorPixels;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
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
                this.colorPixels = new byte[this.sensor.DepthStream.FramePixelDataLength * sizeof(int)];

                // This is the bitmap we'll display on-screen
                this.colorBitmap = new WriteableBitmap(this.sensor.DepthStream.FrameWidth, this.sensor.DepthStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                // Set the image we display to point to the bitmap where we'll put the image data
                this.DepthImage.Source = this.colorBitmap;

                // Add an event handler to be called whenever there is new depth frame data
                this.sensor.DepthFrameReady += this.SensorDepthFrameReady;

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
                this.Data.Content ="Kinect is not ready "; 
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
                            this.colorPixels[colorPixelIndex++] = (byte)(intensity);


                            // Write out green byte
                            this.colorPixels[colorPixelIndex++] = (byte)(intensity * 2);

                            // Write out red byte                        
                            this.colorPixels[colorPixelIndex++] = (byte)(intensity * 8);

                            // We're outputting BGR, the last byte in the 32 bits is unused so skip it
                            // If we were outputting BGRA, we would write alpha here.

                        }
                        else
                        {

                            this.colorPixels[colorPixelIndex++] = 55;


                            // Write out green byte
                            this.colorPixels[colorPixelIndex++] = 0;

                            // Write out red byte                        
                            this.colorPixels[colorPixelIndex++] = 0;

                        }
                        ++colorPixelIndex;
                    }


                    // Write the pixel data into our bitmap
                    this.colorBitmap.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                        this.colorPixels,
                        this.colorBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }
        }


        public MainWindow()
        {
            InitializeComponent();
            tagToBox =new Border[5] { RedBox , BlueBox ,GreenBox , YellowBox ,PinkBox} ;
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
                            this.currentBeamIndex = (uint)((int) BSender.Tag);

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
        void EnemyCreation(object sender, EventArgs e)
        {
            int chance = rndGen.Next(10);
            double width = MainGameView.ActualWidth, height = MainGameView.ActualHeight;
            if (chance % 10 == 0) {
                for (int i = 0; i < tagToBox.Length; i++)
                {
                    tagToBox[i].Background = colors[i];
                }
                int beamOfEnemy = 0;
                do
                {
                    beamOfEnemy = rndGen.Next(5);
                } while (enemies.Any(e => Canvas.GetLeft(e) == beamOfEnemy * width / MainWindow.NumberOfColors &&  Canvas.GetTop(e) <= (height / MainWindow.TableRows * 2 )));

                Data.Content = beamOfEnemy;

                Rectangle enemy = new Rectangle();
                enemy.Width = width / MainWindow.NumberOfColors;
                enemy.Height = height / MainWindow.TableRows * NumberOfColors / 2;
                enemy.Fill = colors[beamOfEnemy];
                enemy.Tag = beamOfEnemy;
                Canvas.SetLeft(enemy, beamOfEnemy * width / MainWindow.NumberOfColors);
                Canvas.SetTop(enemy, -height / MainWindow.TableRows / 8 );

                MainRail.Children.Add(enemy);
                enemies.Add(enemy);
            }
       
            for(int i = 0;  i<enemies.Count; i++)
            {
                var enemyToMove = enemies[i];
                double posY = Canvas.GetTop(enemyToMove) + height / MainWindow.TableRows /8  ;
                Canvas.SetTop(enemyToMove, posY );
                if (posY > height)
                {
                  
                    enemies[i] = null; 
                    MainRail.Children.Remove(enemyToMove);
                    if (currentBeamIndex == (int)enemyToMove.Tag)
                    {
                        tagToBox[(int)enemyToMove.Tag].Background = Brushes.LawnGreen;
                        shots++; 

                    }
                    else {
                        tagToBox[(int)enemyToMove.Tag].Background = Brushes.Black;
                        misses++; 
                    }
                      
                }                
            }
            enemies = enemies.FindAll(e => !(e is null)); 
          
            Data.Content = shots + @"\" + (misses + shots);
            Rate.Value = (int)(((float)shots) / (misses + shots) * 100); 
        }


    }
} 
