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
using Sensnake.Objects;

namespace Sensnake
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int SnakeSquareSize = 20;

        private SolidColorBrush headColor = Brushes.Red;
        private List<SnakeSegment> segments = new List<SnakeSegment>();

        public enum SnakeDirection { Up, Down, Left, Right };
        private SnakeDirection snakeDirection = SnakeDirection.Right;
        private int snakeLength;

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

        // Draws snake on the visible game map based on the stored positions
        private void DrawSnake()
        {
            foreach(SnakeSegment segment in segments)
            {
                if(segment.shape == null)
                {
                    segment.shape = new Rectangle
                    {
                        Width = SnakeSquareSize,
                        Height = SnakeSquareSize,
                        Fill = segment.isHead ? headColor : bodyColor(0)
                    };

                    Playground.Children.Add(segment.shape);
                    Canvas.SetTop(segment.shape, segment.position.Y);
                    Canvas.SetLeft(segment.shape, segment.position.X);
                }
            }
        }

        private void MoveSnake()
        {
            // Snake movement explained - First we make the snake one segment shorter
            // than it actually is, after that we can add new segment to the front,
            // which will serve as the head (pointed at desired direction)

            while(segments.Count > snakeLength) //
            {
                Playground.Children.Remove(segments[0].shape); //Remove the snake's very tail from the map
                segments.RemoveAt(0); // Remove the snake's very tail from the actual game
            }

            // Before we can add the head segment to the snake, we make sure
            // that none of the existing segments are considered head

            int blocksFromHead = 1;
            foreach(SnakeSegment segment in segments)
            {
                (segment.shape as Rectangle).Fill = bodyColor(blocksFromHead);
                segment.isHead = false;
                blocksFromHead++;
            }

            // When all is ready, the head segment can now be prepared to be added based on the direction
            // given by the player, as well as on the position of the previous head segment  

            SnakeSegment prevHead = segments[segments.Count - 1]; // this is the position of the former head
            double headX = prevHead.position.X; // New snake head coordinates are temporarily inherited from the previous one
            double headY = prevHead.position.Y; // only to be edited in the next step of the algorithm

            switch(snakeDirection)
            {
                case SnakeDirection.Up:
                    headY -= SnakeSquareSize;
                    break;
                case SnakeDirection.Down:
                    headY += SnakeSquareSize;
                    break;
                case SnakeDirection.Left:
                    headX -= SnakeSquareSize;
                    break;
                case SnakeDirection.Right:
                    headX += SnakeSquareSize;
                    break;
            }

            // Final step of the algorithm is to add the head to the list of segments
            // and add it to the visible map
            
            segments.Add(new SnakeSegment
            {
                position = new Point(headX, headY),
                isHead = true
            });

            DrawSnake();
        }

        // This method will return the body color based on the segment's blocks distance from the head
        // If it's odd, return Black
        // If it's even, return Orange
        private SolidColorBrush bodyColor(int blocksFromHead) => blocksFromHead % 2 == 0 ? Brushes.Orange : Brushes.Black;

    }
}
