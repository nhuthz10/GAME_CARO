using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caro
{
    class PlayInfor
    {
        private Point point;

        public Point Point { get => point; set => point = value; }
        public int Current { get => current; set => current = value; }

        private int current;

        public PlayInfor(Point point, int current)
        {
            this.Point = point;
            this.Current = current;
        }

    }
}
