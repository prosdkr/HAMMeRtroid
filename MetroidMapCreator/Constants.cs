using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroidMapCreator
{
    public class Constants
    {
        public class ToolTypes
        {
            public const int DRAW_TILE = 1;
            public const int ERASE_TILE = 2;
            public const int SELECT_TILE = 3;
        }

        public const int TILE_HEIGHT = 32;
        public const int TILE_WIDTH = 32;


        public static int CameraX = 0;
        public static int CameraY = 0;


        public static int SelectedTool = -1;


        public static string ASSETS_DIRECTORY = Environment.CurrentDirectory + @"..\..\..\Assets\";



    }
}
