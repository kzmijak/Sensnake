﻿using System;
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
        const int SnakeSquareSize = 20; // This is the pre-set square size of the playground tiles
        const int SnakeStartLength = 2; // Starting snake length
        const int MinSpeed = 400;   // Starting speed of the game, the game will refresh once in 400 ticks (slowest)
        const int MaxSpeed = 100;   // Maximum speed of the game, the game will refresh once in 100 ticks (fastest)
        private int CurrentSpeed = MinSpeed;
        private int score = 0;      // Current player score (food eaten)

        private SolidColorBrush headColor = Brushes.Red; // This is the color of the snake's head
        private List<SnakeSegment> segments = new List<SnakeSegment>(); // this is the list of visible segments
                                                                        // !!! Note: the lenghth of is not the same 
                                                                        // as snakeLength, which is more of a score 
                                                                        // the player reached

        public enum SnakeDirection { Up, Down, Left, Right };   // enum storing the values for snakeDirection variable
        private SnakeDirection snakeDirection = SnakeDirection.Right; // represents the direction the snake is moving in real time
        private int snakeLength; // Actual snake's length, also player's current score

        private System.Windows.Threading.DispatcherTimer tickTimer  // To keep the snake moving, we need to perpetually re-call
            = new System.Windows.Threading.DispatcherTimer();       // the MoveSnake() method using threads, which will do it for us

        private UIElement food = null;  // Circles on the map representing food
        private SolidColorBrush foodColor = Brushes.OrangeRed; // Color of the food circle

        private Grid gameOverBlock = null;
        bool freeze = false;

        public MainWindow()
        {
            InitializeComponent(); // Initialize the main windows component
            tickTimer.Tick += TickTimer_Tick; // Subscribe to the threading handler
        }

        /// <summary>
        /// This is the handler that will recursively call the MoveSnake() function, effectively simulating
        /// the snake's movement on the visible game window
        /// <summary>
        private void TickTimer_Tick(object sender, EventArgs e)
        {
            MoveSnake();
        }

        /// <summary>
        /// This event handler fills the Playground with squares on start-up
        /// <summary>
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            int y = 0; // Y position of the first (and consecutive) rectangle(s) on the canvas
            int x = 0; // X position of the first (and consecutive) rectangle(s) on the canvas
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
            StartGame(); // start the game after loading the playground
        }

        /// <summary>
        /// Returns rectangle fill color based on the number of rectangles
        /// <summary>
        private SolidColorBrush GetNextColor(int rects) => rects % 2 == 0 ? Brushes.ForestGreen : Brushes.Green;

        /// <summary>
        /// Draws snake on the visible game map based on the stored positions
        /// <summary>
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

        /// <summary>
        /// 
        /// </summary>
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
            CheckPosition();

        }

        /// <summary>
        /// This method will return the body color based on the segment's blocks distance from the head
        /// If it's odd, return Black
        /// If it's even, return Orange
        /// <summary>
        private SolidColorBrush bodyColor(int blocksFromHead) => blocksFromHead % 2 == 0 ? Brushes.Orange : Brushes.Black;

        /// <summary>
        /// Start a new game with default starting values, clean the playground if restarted
        /// </summary>
        private void StartGame()
        {
            // Clean the playground from the old core elements in case the game has been restarted
            Playground.Children.Remove(food);   //Remove the food element
            foreach(SnakeSegment segment in segments)   //Remove all snake segments from the map
            {
                Playground.Children.Remove(segment.shape);
            }
            score = 0;  // reset score
            Playground.Children.Remove(gameOverBlock);  // remove Game Over box if lost once
            segments.Clear();   // clear all the segments from the game

            // Set default values and place the snake
            snakeLength = SnakeStartLength;
            snakeDirection = SnakeDirection.Right;
            segments.Add(new SnakeSegment
            {
                position = new Point(0, SnakeSquareSize * 5),   // Starting position (in blocks) is (X,Y)=(0,5)
            });
            tickTimer.Interval = TimeSpan.FromMilliseconds(MinSpeed);   // Set the tick rate
                                   

            // Place the new core elements on the map and enable movement
            DrawSnake();
            DrawFood();
            freeze = false;

            // Start the tick timer
            tickTimer.IsEnabled = true;
        }

        /// <summary>
        /// Dicates where the next food chunk should be placed after starting the game
        /// or consuming the previous chunk
        /// </summary>
        private Point PlantFood(int sofdefender = 0)
        {
            Random rnd = new Random();  // initialize the random value
            int maxX = (int)Playground.ActualWidth / SnakeSquareSize;   // Maximum X value a food can be generated on (in squares)
            int maxY = (int)Playground.ActualHeight / SnakeSquareSize;  // Maximum Y value a food can be generated on (in squares)
            int plantX = rnd.Next(0, maxX) * SnakeSquareSize;   // Randomize the next X position the chunk is to be planted on
            int plantY = rnd.Next(0, maxY) * SnakeSquareSize;   // Randomize the next Y position the chunk is to be planted on

            Point randPlantPoint = new Point(plantX, plantY);

            if (sofdefender == 10)   // if the sofdefender (StackOverflowDefender) reaches 10 (due to the size of the snake),
            {                        // the new algorithm will attempt to find a new free position faster
                randPlantPoint = FindFirstPosition(maxX, maxY);
            }
            if (sofdefender < 10)
            {
                foreach (SnakeSegment segment in segments) // Checks if the potential food position is occupied by one of the
                                                           // snake's segments...
                {
                    if (segment.position.Equals(randPlantPoint))
                    {
                        sofdefender++;
                        PlantFood(sofdefender); // ... if so, then restart
                    }
                }
            }
            return randPlantPoint; 
        }

        /// <summary>
        /// Returns the position of the next free space
        /// Called when there is a risk of stack overflow
        /// </summary>
        /// <param name="maxX">Maximum canvas width</param>
        /// <param name="maxY">Maximum canvas height</param>
        /// <returns>Position of the next free space</returns>
        private Point FindFirstPosition(int maxX, int maxY)
        {
            for(int y = 0; y <= maxY; y += SnakeSquareSize)
            {
                for(int x = 0; x <= maxX; x += SnakeSquareSize)
                {
                    foreach(SnakeSegment segment in segments)
                    {
                        if(!(new Point(x,y).Equals(segment.position)))
                        {
                            return new Point(x, y); // return the first free position on the playground
                        }
                    }
                }
            }
            throw new Exception("No free space found"); // throw the exception when the snake occupies all the map
        }

        /// <summary>
        /// Put the food element on the visible game map
        /// </summary>
        private void DrawFood()
        {
            Point position = PlantFood(); // get the adequate position from the algorithm
            food = new Ellipse  // create a new shape representing food
            {
                Width = SnakeSquareSize,
                Height = SnakeSquareSize,
                Fill = foodColor
            };

            Playground.Children.Add(food);  // add the circle to the Playground elements
            Canvas.SetLeft(food, position.X);   // position it to the correcnt X coordinate
            Canvas.SetTop(food, position.Y);    // position it to the correcnt Y coordinate
        }

        /// <summary>
        /// Handles the keyboard input
        /// Use key arrows to change snake direction
        /// Use space to start a new game
        /// </summary>
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            SnakeDirection previousDirection = snakeDirection;

            switch(e.Key)
            {
                case Key.Up:
                    if(snakeDirection != SnakeDirection.Down && !freeze)
                        snakeDirection = SnakeDirection.Up;
                    break;
                case Key.Down:
                    if (snakeDirection != SnakeDirection.Up && !freeze)
                        snakeDirection = SnakeDirection.Down;
                    break;
                case Key.Left:
                    if (snakeDirection != SnakeDirection.Right && !freeze)
                        snakeDirection = SnakeDirection.Left;
                    break;
                case Key.Right:
                    if (snakeDirection != SnakeDirection.Left && !freeze)
                        snakeDirection = SnakeDirection.Right;
                    break;
                case Key.Space: // spacebar stars a new game
                    StartGame();
                    break;
            }
            if (snakeDirection != previousDirection)
                MoveSnake();
        }

        private void CheckPosition()
        {
            Point headPosition = segments[segments.Count - 1].position;

            CheckIfTail(headPosition);
            CheckIfFood(headPosition);
            CheckIfWall(headPosition);
        }

        private void CheckIfTail(Point headPosition)
        {
            foreach(SnakeSegment segment in segments.Take(segments.Count -1))
            {
                if (segment.position.X == headPosition.X && segment.position.Y == headPosition.Y)
                    GameOver();
            }
        }

        private void CheckIfWall(Point headPosition)
        {
            if (headPosition.X < 0
                || headPosition.X >= Playground.ActualWidth
                || headPosition.Y >= Playground.ActualHeight
                || headPosition.Y < 0)
            {
                GameOver();
            }
        }

        private void CheckIfFood(Point headPosition)
        {
            if(headPosition.X == Canvas.GetLeft(food) && headPosition.Y==Canvas.GetTop(food))
            {
                snakeLength++;
                score += 402 - CurrentSpeed;
                CurrentSpeed = Math.Max(MaxSpeed, CurrentSpeed - score);
                tickTimer.Interval = TimeSpan.FromMilliseconds(CurrentSpeed);
                Playground.Children.Remove(food);
                DrawFood();
            }
        }

        /// <summary>
        /// Stops the tick timer, displays the Game Over box (containing score and the bitmap)
        /// </summary>
        private void GameOver()
        {
            tickTimer.IsEnabled = false; // Prevents the snake from moving autonomously
            freeze = true;  // Prevents the snake from moving using arrows

            // Creating a new Grid that will pop-up upon GameOver
            gameOverBlock = new Grid
            {
                Width = 300,
                Height = 200
            };
            // Add black transparent background with soft edges
            gameOverBlock.Children.Add(
                    new Rectangle
                    {
                        Fill = Brushes.Black,
                        Opacity = .5,
                        RadiusX = 10,
                        RadiusY = 10
                    });

            // Add info text
            gameOverBlock.Children.Add(
                    new TextBlock { 
                        Text = "Game over! Press space to retry. \nScore: " + score, 
                        FontSize = 15,
                        TextAlignment = TextAlignment.Center,
                        Foreground = Brushes.White
                    });

            // Add bitmap
            var image = new Image();
            var fullFilePath = @"https://cdn.pixabay.com/photo/2016/03/31/19/42/circle-icons-1295218_960_720.png";

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(fullFilePath, UriKind.Absolute);
            bitmap.EndInit();

            image.Source = bitmap;
            image.Width = 80;
            image.Height = 80;
            gameOverBlock.Children.Add(image);

            // Set the box position
            Playground.Children.Add(gameOverBlock);
            Canvas.SetLeft(gameOverBlock, Playground.Width / 2 - 150);
            Canvas.SetTop(gameOverBlock, Playground.Height / 2 - 100);
        }
    }
}
