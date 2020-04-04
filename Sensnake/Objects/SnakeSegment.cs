using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Sensnake.Objects
{
    class SnakeSegment
    {
        public UIElement shape { get; set; }
        public Point position { get; set; }
        public bool isHead { get; set; }
    }
}
