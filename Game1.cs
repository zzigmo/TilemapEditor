using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace TilemapEditor
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D tileset;

        const int tileSize = 32;

        int mapWidth = 20;
        int mapHeight = 15;
        int[,] mapData;

        int selectedTile = 0;
        MouseState curMouse, prevMouse;

        // Палитра
        Rectangle paletteArea;
        int tilesetColumns;
        int tilesetRows;
        int tileCount;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // Размер окна: под карту + палитру
            graphics.PreferredBackBufferWidth = tileSize * 20;
            graphics.PreferredBackBufferHeight = tileSize * 15 + tileSize * 3;
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            // Создаём карту и заполняем пустыми (-1)
            mapData = new int[mapWidth, mapHeight];
            for (int y = 0; y < mapHeight; y++)
                for (int x = 0; x < mapWidth; x++)
                    mapData[x, y] = -1; // пустая клетка
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Загружаем tileset
            tileset = Content.Load<Texture2D>("tiles");

            // Считаем количество тайлов по ширине и высоте изображения
            tilesetColumns = tileset.Width / tileSize;
            tilesetRows = tileset.Height / tileSize;
            tileCount = tilesetColumns * tilesetRows;

            // Палитра будет внизу, 3 строки максимум
            paletteArea = new Rectangle(0, mapHeight * tileSize, graphics.PreferredBackBufferWidth, tileSize * 3);
        }

        protected override void Update(GameTime gameTime)
        {
            curMouse = Mouse.GetState();
            int mouseX = curMouse.X;
            int mouseY = curMouse.Y;

            // Рисуем на карте при удержании ЛКМ
            if (curMouse.LeftButton == ButtonState.Pressed)
            {
                if (mouseY < mapHeight * tileSize)
                {
                    int x = mouseX / tileSize;
                    int y = mouseY / tileSize;

                    if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
                        mapData[x, y] = selectedTile;
                }
            }

            // Стираем при удержании ПКМ
            if (curMouse.RightButton == ButtonState.Pressed)
            {
                if (mouseY < mapHeight * tileSize)
                {
                    int x = mouseX / tileSize;
                    int y = mouseY / tileSize;

                    if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
                        mapData[x, y] = -1;
                }
            }

            // Если клик по палитре — выбираем тайл
            if (curMouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released)
            {
                if (paletteArea.Contains(mouseX, mouseY))
                {
                    int px = mouseX / tileSize;
                    int py = (mouseY - paletteArea.Y) / tileSize;
                    int index = py * tilesetColumns + px;

                    if (index >= 0 && index < tileCount)
                        selectedTile = index;
                }
            }

            prevMouse = curMouse;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Gray); // Серая пустая карта
            spriteBatch.Begin();
            // --- 1. Рисуем карту ---
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    int idx = mapData[x, y];
                    if (idx < 0) continue; // не рисуем пустое

                    int tx = idx % tilesetColumns;
                    int ty = idx / tilesetColumns;
                    Rectangle src = new Rectangle(tx * tileSize, ty * tileSize, tileSize, tileSize);
                    Vector2 pos = new Vector2(x * tileSize, y * tileSize);
                    spriteBatch.Draw(tileset, pos, src, Color.White);
                }
            }

            // --- 2. Рисуем палитру внизу ---
            for (int i = 0; i < tileCount; i++)
            {
                int tx = i % tilesetColumns;
                int ty = i / tilesetColumns;
                Vector2 pos = new Vector2(tx * tileSize, mapHeight * tileSize + ty * tileSize);
                Rectangle src = new Rectangle(tx * tileSize, ty * tileSize, tileSize, tileSize);
                spriteBatch.Draw(tileset, pos, src, Color.White);

                // Обводка у выбранного тайла
                if (i == selectedTile)
                    spriteBatch.Draw(CreateOutline(tileSize, tileSize, Color.Yellow), pos, Color.White);
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }

        // Вспомогательная функция: создаёт рамку (обводку)
        Texture2D CreateOutline(int width, int height, Color color)
        {
            Texture2D texture = new Texture2D(GraphicsDevice, width, height);
            Color[] data = new Color[width * height];

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    bool isEdge = x == 0 || y == 0 || x == width - 1 || y == height - 1;
                    data[y * width + x] = isEdge ? color : Color.Transparent;
                }

            texture.SetData(data);
            return texture;
        }
    }
}