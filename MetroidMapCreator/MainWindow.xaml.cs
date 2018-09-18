using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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

namespace MetroidMapCreator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        public class TileMap
        {
            public int X { get; set; }
            public int Y { get; set; }
            public Tile tile { get; set; }
        }


        public List<TileMap> tileMap = new List<TileMap>();

        public Point SelectedTile = new Point(1,1);

       
        List<Tile> Tiles = new List<Tile>();
        public MainWindow()
        {
            InitializeComponent();
            this.ResizeMode = ResizeMode.NoResize ^ ResizeMode.CanMinimize;
            WorldGrid.Background = Brushes.LightGray;
            DrawGrid();



            PopulateTileSelectionGrid();
            
           

            WorldGrid.Focusable = true;
            

            LinkButtonCallbacks();


            WorldGrid.MouseLeftButtonDown += WorldGrid_MouseLeftButtonDown;
        }



        #region Rendering
        private void WorldGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Constants.SelectedTool == Constants.ToolTypes.DRAW_TILE)
            {
                Point nearestTileCoords = FindNearestTile();

                TileMap item = new TileMap();
                item.X = (int)nearestTileCoords.X;
                item.Y = (int)nearestTileCoords.Y;
                item.tile = (Tile)(TilesGrid.SelectedItem);

                if (tileMap.Where<TileMap>(t => t.X == item.X && t.Y == item.Y).Count() > 0)
                {
                    int index = tileMap.FindIndex(t => t.X == item.X && t.Y == item.Y);
                    tileMap[index] = item;
                }
                else
                    tileMap.Add(item);
            }
            else if (Constants.SelectedTool == Constants.ToolTypes.ERASE_TILE)
            {
                Point nearestTileCoords = FindNearestTile();
                if (tileMap.Where<TileMap>(t => t.X == nearestTileCoords.X && t.Y == nearestTileCoords.Y).Count() > 0)
                {
                    int index = tileMap.FindIndex(t => t.X == nearestTileCoords.X && t.Y == nearestTileCoords.Y);
                    tileMap.RemoveAt(index);
                }
            }
            else if (Constants.SelectedTool == Constants.ToolTypes.SELECT_TILE)
            {
                Point nearestTileCoords = FindNearestTile();
                SelectedTile = nearestTileCoords;
            }

            WorldGrid.Children.Clear();
            DrawGrid();
            DrawWorld();


            //throw new NotImplementedException();
        }
        #endregion

        #region Button Callbacks
        private void LinkButtonCallbacks()
        {
            this.PreviewKeyDown += MainWindow_KeyDown;

            TileToolButton.Click += TileToolButton_Click;
            EraseTilesButton.Click += EraseTilesButton_Click;
            SaveButton.Click += SaveButton_Click;
            OpenButton.Click += OpenButton_Click;
            SelectTileButton.Click += SelectTileButton_Click;
        }

        private void TileToolButton_Click(object sender, RoutedEventArgs e)
        {
            ResetSelectedPoint();
            Constants.SelectedTool = Constants.ToolTypes.DRAW_TILE;
            this.Cursor = Cursors.Pen;
            //throw new NotImplementedException();
        }
        private void EraseTilesButton_Click(object sender, RoutedEventArgs e)
        {
            ResetSelectedPoint();
            Constants.SelectedTool = Constants.ToolTypes.ERASE_TILE;
            //FileStream x = new FileStream(, FileMode.Open);
            Cursor c = new Cursor(Constants.ASSETS_DIRECTORY + @"\eraser.cur");
            this.Cursor = c;
            // throw new NotImplementedException();
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {

            WorldGrid.Focus();
            if (e.Key == Key.Right)
                Constants.CameraX += Constants.TILE_WIDTH;
            if (e.Key == Key.Left)
                Constants.CameraX -= Constants.TILE_WIDTH;
            if (e.Key == Key.Up)
                Constants.CameraY -= Constants.TILE_HEIGHT;
            if (e.Key == Key.Down)
                Constants.CameraY += Constants.TILE_HEIGHT;

            WorldGrid.Children.Clear();
            DrawGrid();
            DrawWorld();
            
            //throw new NotImplementedException();
        }
        private void SelectTileButton_Click(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
            this.Cursor = Cursors.Hand;
            Constants.SelectedTool = Constants.ToolTypes.SELECT_TILE;

        }
        #endregion

        #region Processing
        public void DrawWorld()
        {
            foreach (TileMap t in tileMap)
            {
                if (t.X - Constants.CameraX >= 0 && t.Y - Constants.CameraY >= 0)
                {
                    ImageSource bitmapSource = new BitmapImage(new Uri(t.tile.Image));

                    Rectangle tile = new Rectangle();
                    tile.Width = Constants.TILE_WIDTH;
                    tile.Height = Constants.TILE_HEIGHT;
                    tile.Fill = new ImageBrush { ImageSource = bitmapSource };
                    Canvas.SetLeft(tile, t.X - Constants.CameraX);
                    Canvas.SetTop(tile, t.Y - Constants.CameraY);

                    WorldGrid.Children.Add(tile);
                }
            }
            if (SelectedTile.X % Constants.TILE_WIDTH == 0 && SelectedTile.Y % Constants.TILE_HEIGHT == 0)
            {
                Rectangle fill = new Rectangle();
                fill.Width = Constants.TILE_WIDTH;
                fill.Height = Constants.TILE_HEIGHT;
                fill.Fill = Brushes.LightYellow;
                fill.Opacity = .6;
                Canvas.SetTop(fill, SelectedTile.Y);
                Canvas.SetLeft(fill, SelectedTile.X);
                WorldGrid.Children.Add(fill);
            }
        }

        public Point FindNearestTile()
        {
            Point gridPositionClicked = Mouse.GetPosition(WorldGrid);
            Point nearestTileCoords = new Point(Constants.CameraX + gridPositionClicked.X - (gridPositionClicked.X % Constants.TILE_WIDTH),
                                                Constants.CameraY + gridPositionClicked.Y - (gridPositionClicked.Y % Constants.TILE_HEIGHT));
            return nearestTileCoords;
        }

        public void DrawGrid()
        {
            for(int i = 0; i < WorldGrid.Height; i+= Constants.TILE_HEIGHT)
            {
                Line line = new Line();
                line.Stroke = Brushes.Black;

                line.X1 = 0;
                line.X2 = WorldGrid.Width;
                line.Y1 = i;
                line.Y2 = i;

                line.StrokeThickness = 1;
                WorldGrid.Children.Add(line);

            }
            for (int i = 0; i < WorldGrid.Width; i += Constants.TILE_WIDTH)
            {

                Line line = new Line();
                line.Stroke = Brushes.Black;

                line.X1 = i;
                line.X2 = i;
                line.Y1 = 0;
                line.Y2 = WorldGrid.Height;

                line.StrokeThickness = 1;
                WorldGrid.Children.Add(line);

            }
        }

        public void PopulateTileSelectionGrid()
        {
            List<string> tileFiles = Directory.EnumerateFiles(Constants.ASSETS_DIRECTORY + @"\Tiles").ToList<string>();

            foreach (string s in tileFiles)
            {
                Tile x = new Tile();
                x.Image = s;

                Regex regex = new Regex(@"\d+");
                Match m = regex.Match(s);
                x.id = int.Parse(m.ToString());
                Tiles.Add(x);
            }
            TilesGrid.ItemsSource = Tiles;
            TilesGrid.CanUserAddRows = false;
            TilesGrid.SelectedIndex = 0;
        }
        public void ResetSelectedPoint()
        {
            SelectedTile = new Point(1, 1);
        }
        #endregion

        #region Functionality
        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {

            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();

            // Launch OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = openFileDlg.ShowDialog();
            // Get the selected file name and display in a TextBox.
            // Load content of file in a TextBlock
            if (result == true)
            {
                tileMap = JsonConvert.DeserializeObject<List<TileMap>>(System.IO.File.ReadAllText(openFileDlg.FileName));
                WorldGrid.Children.Clear();
                DrawGrid();
                DrawWorld();
            }

            //throw new NotImplementedException();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Configure save file dialog box
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Map1"; // Default file name
            dlg.DefaultExt = ".map"; // Default file extension
            dlg.Filter = "Map Files (.map)|*.map"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
                //File.Create(filename);
                FileStream fs = File.Open(filename, FileMode.Create);
                string TileMapString = JsonConvert.SerializeObject(tileMap).ToString();

                fs.Write(Encoding.ASCII.GetBytes(TileMapString), 0, TileMapString.Length);
            }
            //throw new NotImplementedException();
        }
        #endregion

    }
}
