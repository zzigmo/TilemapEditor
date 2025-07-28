using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Xml.Linq;

namespace TilemapEditor
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D tileset;

        const int tileSize = 16;

        int mapWidth = 30;
        int mapHeight = 18;
        int[,] mapData;

        int selectedTile = 0;
        MouseState curMouse;

        int tilesetColumns;
        int tileCount;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            graphics.PreferredBackBufferWidth = tileSize * mapWidth;
            graphics.PreferredBackBufferHeight = tileSize * (mapHeight + 5); // +5 строк под палитру
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            mapData = new int[mapWidth, mapHeight];
            ClearMap();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            tileset = Content.Load<Texture2D>("tiles");

            tilesetColumns = tileset.Width / tileSize;
            tileCount = (tileset.Height / tileSize) * tilesetColumns;
        }

        protected override void Update(GameTime gameTime)
        {
            curMouse = Mouse.GetState();
            int mx = curMouse.X;
            int my = curMouse.Y;

            if (curMouse.LeftButton == ButtonState.Pressed)
            {
                if (my < mapHeight * tileSize)
                {
                    int x = mx / tileSize;
                    int y = my / tileSize;
                    if (InBounds(x, y)) mapData[x, y] = selectedTile;
                }
                else
                {
                    int px = mx / tileSize;
                    int py = (my - mapHeight * tileSize) / tileSize;
                    int index = py * mapWidth + px;
                    if (index >= 0 && index < tileCount)
                        selectedTile = index;
                }
            }

            if (curMouse.RightButton == ButtonState.Pressed)
            {
                int x = mx / tileSize;
                int y = my / tileSize;
                if (InBounds(x, y)) mapData[x, y] = -1;
            }

            var k = Keyboard.GetState();
            if (k.IsKeyDown(Keys.S))
                SaveMapToXml("map.xml");
            if (k.IsKeyDown(Keys.L))
                LoadMapFromXml("map.xml");

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Gray);
            spriteBatch.Begin();

            // Рисуем карту
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    int idx = mapData[x, y];
                    if (idx < 0) continue;

                    Rectangle src = GetTileSourceRect(idx);
                    Vector2 pos = new Vector2(x * tileSize, y * tileSize);
                    spriteBatch.Draw(tileset, pos, src, Color.White);
                }
            }

            // Рисуем палитру
            for (int i = 0; i < tileCount; i++)
            {
                int px = i % mapWidth;
                int py = i / mapWidth;
                if ((py * tileSize + mapHeight * tileSize) > graphics.PreferredBackBufferHeight - tileSize)
                    continue;

                Rectangle src = GetTileSourceRect(i);
                Vector2 pos = new Vector2(px * tileSize, mapHeight * tileSize + py * tileSize);

                spriteBatch.Draw(tileset, pos, src, Color.White);

                if (i == selectedTile)
                    spriteBatch.Draw(CreateOutline(tileSize, tileSize, Color.Yellow), pos, Color.White);
            }
            spriteBatch.End();
            base.Draw(gameTime);
        }

        bool InBounds(int x, int y) => x >= 0 && x < mapWidth && y >= 0 && y < mapHeight;

        Rectangle GetTileSourceRect(int index)
        {
            int tx = index % tilesetColumns;
            int ty = index / tilesetColumns;
            return new Rectangle(tx * tileSize, ty * tileSize, tileSize, tileSize);
        }

        void ClearMap()
        {
            for (int x = 0; x < mapWidth; x++)
                for (int y = 0; y < mapHeight; y++)
                    mapData[x, y] = -1;
        }

        Texture2D CreateOutline(int width, int height, Color color)
        {
            Texture2D tex = new Texture2D(GraphicsDevice, width, height);
            Color[] data = new Color[width * height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                { bool edge = (x == 0 || y == 0 || x == width - 1 || y == height - 1);
                 data[y * width + x] = edge ? color : Color.Transparent;
                }
            }
            tex.SetData(data);
            return tex;
        }

        void SaveMapToXml(string filePath)
        {
            XElement root = new XElement("Map",
                new XAttribute("Width", mapWidth),
                new XAttribute("Height", mapHeight)
            );
        
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    int tile = mapData[x, y];
                    if (tile >= 0)
                    {
                        root.Add(new XElement("Tile",
                            new XAttribute("X", x),
                            new XAttribute("Y", y),
                            new XAttribute("ID", tile)));
                    }
                }
            }
        
            root.Save(filePath);
            Console.WriteLine("Карта сохранена в " + Path.GetFullPath(filePath));
        }
        
        void LoadMapFromXml(string filePath)
        {
            if (!File.Exists(filePath)) return;
        
            XElement root = XElement.Load(filePath);
            int width = int.Parse(root.Attribute("Width").Value);
            int height = int.Parse(root.Attribute("Height").Value);
        
            if (width != mapWidth || height != mapHeight)
            {
                Console.WriteLine("Размер карты не совпадает с текущими настройками.");
                return;
            }
        
            ClearMap();
        
            foreach (XElement tileElem in root.Elements("Tile"))
            {
                int x = int.Parse(tileElem.Attribute("X").Value);
                int y = int.Parse(tileElem.Attribute("Y").Value);
                int id = int.Parse(tileElem.Attribute("ID").Value);
        
                if (InBounds(x, y))
                    mapData[x, y] = id;
            }
        
            Console.WriteLine("Карта загружена из " + Path.GetFullPath(filePath));
        }
    }
}