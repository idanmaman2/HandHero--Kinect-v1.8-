using HandHero.Widgets;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
        int shots = 0, misses = 0; 
        



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
            double width = MainGameView.ActualWidth, height = MainGameView.ActualHeight;
            for (int i = 0; i < tagToBox.Length; i++) {
                tagToBox[i].Background = colors[i];
            }
            int beamOfEnemy = 0;
            do {
                beamOfEnemy = rndGen.Next(5);
            } while (enemies.Any(e => Canvas.GetLeft(e) == beamOfEnemy * width / MainWindow.NumberOfColors && Canvas.GetTop(e) <= (height / MainWindow.TableRows ) ));
           
            Data.Content = beamOfEnemy;

            Rectangle enemy = new Rectangle() ;
            enemy.Width = width / MainWindow.NumberOfColors; 
            enemy.Height = height / MainWindow.TableRows * NumberOfColors /2 ;
            enemy.Fill = colors[beamOfEnemy];
            enemy.Tag = beamOfEnemy; 
            Canvas.SetLeft(enemy, beamOfEnemy * width / MainWindow.NumberOfColors);
            Canvas.SetTop(enemy,0);

            MainRail.Children.Add(enemy);
            for(int i = 0;  i<enemies.Count; i++)
            {
                var enemyToMove = enemies[i];
                double posY = Canvas.GetTop(enemyToMove) + height / MainWindow.TableRows ;
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
            enemies.Add(enemy);
            Data.Content = shots + @"\" + (misses + shots); 

        }


    }
} 
