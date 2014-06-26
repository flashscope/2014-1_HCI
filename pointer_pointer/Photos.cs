using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.IO;
using System.Xml;

namespace ShapeGame
{
    class Photos
    {
        private const int IMAGES_NUM = 900;

        private PhotoBox[] photoBoxList = new PhotoBox[IMAGES_NUM];
        public Photos()
        {
            loadPhotos();
        }

        private void loadPhotos()
        {

            String curDir = Directory.GetCurrentDirectory();
            curDir = curDir + "\\";

            XmlDocument xml = new XmlDocument();
            xml.Load( curDir + "photos.xml" );

            XmlElement element = xml.DocumentElement;
            XmlNodeList childList = element.ChildNodes;

            for (int i = 0; i < childList.Count; ++i)
            {
                XmlElement childNode = (XmlElement)childList[i];
                String x = childNode.SelectSingleNode("x").InnerText;
                String y = childNode.SelectSingleNode("y").InnerText;
                String src = childNode.SelectSingleNode("src").InnerText;
                photoBoxList[i] = new PhotoBox( Convert.ToInt32(x), Convert.ToInt32(y), curDir + "images\\" + src );
            }

            Console.WriteLine("Loaded Image Num:" + childList.Count);
        }

        public PhotoBox GetNearestPhotoBox(int x, int y)
        {
            int closestDistance = 9999;
            PhotoBox resultPhoto = photoBoxList[0];
            for (int i = 0; i < IMAGES_NUM; ++i)
            {
                int distance = photoBoxList[i].GetDistance(x,y);
                if( closestDistance > distance)
                {
                    resultPhoto = photoBoxList[i];
                    closestDistance = distance;
                }
            }
            return resultPhoto;
        }

    }
}
