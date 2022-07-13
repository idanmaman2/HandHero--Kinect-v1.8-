using HandHero.Widgets;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
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
        private MediaPlayer mediaPlayer = new MediaPlayer();

        private const int TableRows = 20, TableCols = 20;
        private const int NumberOfColors = 5;
        private readonly Brush[] colors = { Brushes.Red, Brushes.Blue, Brushes.Green, Brushes.Yellow , Brushes.Pink };
        private readonly Border[,] TableArray = new Border[TableRows, TableCols];
        private int ? lastCol =null ,  lastRow =null; 
        private void MouseMove(object sender, MouseEventArgs e)
        {
            
            Grid senderG = ((Grid)sender); 
            Point pos = e.GetPosition(this);
            Point startingPoint = senderG.TranslatePoint(new Point(0, 0), this); 
            double Width = senderG.Width , Height = senderG.Height;

            #region DataDisplay 
            Data.Content = pos;
            Data.Content += "\n" + senderG.TranslatePoint(new Point(0, 0), this);
            Data.Content += "\n" + senderG.Width + "\n" + senderG.Height;
            #endregion

            double colSize = Width / MainWindow.TableCols , rowSize = Height / MainWindow.TableRows;

            uint col = Math.Min((uint)((pos.X - startingPoint.X) / colSize -1 ), TableCols-1), 
                row = Math.Min((uint)((pos.Y - startingPoint.Y) / rowSize - 1), TableRows-1);

            Data.Content += "\ncol: " + col + "\nrow : " + row;
            if (TableArray[row, col].Background == null) {
                TableArray[row, col].Background = TableArray[row, col].BorderBrush;
            }
             


       
        
        
        
        }


  


        public MainWindow()
        {
            InitializeComponent();

           
            for (int i = 0; i < TableRows; i++ )
            { 

                for (int k = 0; k < NumberOfColors; k++)
                {
                    for (int j = 0; j < TableCols / NumberOfColors; j++)
                {
                     
                            Border brd = new Border();
                            brd.BorderThickness = new Thickness(1);
                            brd.BorderBrush = colors[k];
                            TableArray[i,k * (TableCols / NumberOfColors) + j ] = brd;


                        
                    }
                  
                }
               
            }
            
            Table.Children.Add(new CostumTable(TableRows, TableCols, TableArray));

       
              mediaPlayer.Open(new Uri(@"https://file-examples.com/storage/fef3ae9ac162ce030988192/2017/11/file_example_MP3_2MG.mp3"));

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
             mediaPlayer.Play();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (mediaPlayer.Source != null)
                Data.Content = mediaPlayer.Volume; 
            else
                Data.Content = "No file selected...";
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Play();
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
        }
    }
} 
