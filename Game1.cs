using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;

namespace TilemapEditor
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D tileset;

        const int tileSize = 16; // размер тайла

        int mapWidth = 30;
        int mapHeight = 18; // размер карты
        int[,] mapData;

        int selectedTile = 0;
        MouseState curMouse, prevMouse;

        int tilesetColumns;
        int tilesetRows;
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

        protected override void Initialize() // инициализация размеров
        {
            mapData = new int[mapWidth, mapHeight];
            for (int x = 0; x < mapWidth; x++)
                for (int y = 0; y < mapHeight; y++)
                    mapData[x, y] = -1;

            base.Initialize();
        }

        protected override void LoadContent() // грузит тайлы
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            tileset = Content.Load<Texture2D>("tiles"); // название ехе файла с тайлами

            tilesetColumns = tileset.Width / tileSize;//
            tilesetRows = tileset.Height / tileSize;  //
            tileCount = tilesetColumns * tilesetRows; // типо автоматически режет 
        }

        protected override void Update(GameTime gameTime)
        {
            curMouse = Mouse.GetState(); // корды мыш собирает
            int mx = curMouse.X;
            int my = curMouse.Y;

            if (curMouse.LeftButton == ButtonState.Pressed) // рисует
            {
                if (my < mapHeight * tileSize)
                {
                    int x = mx / tileSize;
                    int y = my / tileSize;
                    if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
                        mapData[x, y] = selectedTile;
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

            if (curMouse.RightButton == ButtonState.Pressed) // стирает
            {
                int x = mx / tileSize;
                int y = my / tileSize;
                if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
                    mapData[x, y] = -1;
            }

            var k = Keyboard.GetState(); // псевдо сохранение
            if (k.IsKeyDown(Keys.S))
                SaveMap("map.txt");
            if (k.IsKeyDown(Keys.L)) // псевдо загрузка
                LoadMap("map.txt");

            prevMouse = curMouse;
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

                    int tx = idx % tilesetColumns;
                    int ty = idx / tilesetColumns;
                    Rectangle src = new Rectangle(tx * tileSize, ty * tileSize, tileSize, tileSize);
                    Vector2 pos = new Vector2(x * tileSize, y * tileSize);
                    spriteBatch.Draw(tileset, pos, src, Color.White);
                }
            }

            // Рисуем палитру
            for (int i = 0; i < tileCount; i++)
            {
                int px = i % mapWidth;
                int py = i / mapWidth;
                if (py * tileSize + mapHeight * tileSize > graphics.PreferredBackBufferHeight - tileSize)
                    continue;

                int tx = i % tilesetColumns;
                int ty = i / tilesetColumns;
                Rectangle src = new Rectangle(tx * tileSize, ty * tileSize, tileSize, tileSize);
                Vector2 pos = new Vector2(px * tileSize, mapHeight * tileSize + py * tileSize);

                spriteBatch.Draw(tileset, pos, src, Color.White);

                if (i == selectedTile)
                    spriteBatch.Draw(CreateOutline(tileSize, tileSize, Color.Yellow), pos, Color.White); // обводка
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }

        Texture2D CreateOutline(int width, int height, Color color)
        {
            Texture2D tex = new Texture2D(GraphicsDevice, width, height);
            Color[] data = new Color[width * height];

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    bool edge = (x == 0 ||  y == 0 || x == width - 1 || y == height - 1);
                    data[y * width + x] = edge ? color : Color.Transparent;
                }

            tex.SetData(data);
            return tex;
        }

        void SaveMap(string path)
        {
            using StreamWriter sw = new StreamWriter(path);
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    sw.Write(mapData[x, y]);
                    if (x < mapWidth - 1) sw.Write(",");
                }
                sw.WriteLine();
            }
        }

        void LoadMap(string path)
        {
            if (!File.Exists(path)) return;
            string[] lines = File.ReadAllLines(path);
            for (int y = 0; y < Math.Min(lines.Length, mapHeight); y++)
            {
                string[] parts = lines[y].Split(',');
                for (int x = 0; x < Math.Min(parts.Length, mapWidth); x++)
                    if (int.TryParse(parts[x], out int val)) mapData[x, y] = val;
            }
        }
    }
}