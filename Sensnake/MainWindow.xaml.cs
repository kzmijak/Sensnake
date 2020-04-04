using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Sensnake
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int SnakeSquareSize = 20;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            int x = 0;
            int y = 0;
            int rects = 0;

            bool stop = false;  
            while(!stop)    //Starts at startup, ends when all the space has been filled with the rectangles
            {
                Rectangle rect = new Rectangle
                {
                    Width = SnakeSquareSize,
                    Height = SnakeSquareSize
                };

                rect.Fill = GetNextColor(rects);
                Playground.Children.Add(rect);
                Canvas.SetTop(rect, y);
                Canvas.SetLeft(rect, x);

                x += SnakeSquareSize;
                if(Playground.ActualWidth <= x)
                {
                    x = 0;
                    y += SnakeSquareSize;
                }
                if(Playground.ActualHeight <= y)
                {
                    stop = true;
                }
                rects++;
            }
        }
        // Returns rectangle fill color based on the number of rectangles
        private SolidColorBrush GetNextColor(int rects) => rects % 2 == 0 ? Brushes.ForestGreen : Brushes.Green;
    }
}
