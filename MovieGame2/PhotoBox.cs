using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ShapeGame
{
    class PhotoBox
    {
        private int m_X;
        private int m_Y;
        private BitmapImage m_Photo = new BitmapImage();

        public PhotoBox(int x, int y, String imagePath)
        {
            this.m_X = x;
            this.m_Y = y;

            this.m_Photo.BeginInit();
            this.m_Photo.UriSource = new Uri(imagePath);
            this.m_Photo.CacheOption = BitmapCacheOption.OnLoad;
            this.m_Photo.EndInit();

        }

        public int GetDistance(int x, int y)
        {
            int disX = Math.Abs(m_X - x);
            int disY = Math.Abs(m_Y - y);

            return (disX + disY);
        }

        public BitmapImage GetPhoto()
        {
            return m_Photo;
        }
    }
}
