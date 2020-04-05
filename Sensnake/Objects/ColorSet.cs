using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Sensnake.Objects
{
    class ColorSet
    {
        public SolidColorBrush snakeHead { get; set; } = Brushes.Red;
        public SolidColorBrush snakeBody1 { get; set; } = Brushes.Black;
        public SolidColorBrush snakeBody2 { get; set; } = Brushes.Orange;
        public SolidColorBrush food { get; set; } = Brushes.DarkRed;
    }
}
