﻿#region File Description
//-----------------------------------------------------------------------------
// Level.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Unicorn.PowerUps;
using Unicorn.ScreenArchitecture;

namespace Unicorn
{
    /// <summary>
    /// A uniform grid of tiles with collections of gems and enemies.
    /// The level owns the player and controls the game's win and lose
    /// conditions as well as scoring.
    /// </summary>
    public class Level : IDisposable
    {
        // Physical structure of the level.
        private List<Tile[]> levelTiles;
        private Layer[] layers;

        private string[] enemieSpriteSets = { "Slug", "CakeMonster", "Scorpion" };

        // The layer which entities are drawn on top of.
        private const int EntityLayer = 1;

        public static int NumberOfLevels = -1;
        private int levelHeight = 0;

        // Entities in the level.
        public Player Player
        {
            get { return player; }
        }
        Player player;

        private List<Gem> gems = new List<Gem>();
        private List<Enemy> enemies = new List<Enemy>();

        private SoundEffect heartBeat;

        // Key locations in the level.        
        private Vector2 start;
        private Point exit = InvalidPosition;
        private static readonly Point InvalidPosition = new Point(-1, -1);

        // Level game state.
        private float cameraPosition;

        public int Score
        {
            get { return score; }
        }
        int score;

        public bool ReachedExit
        {
            get { return reachedExit; }
        }
        bool reachedExit;

        public TimeSpan TimeRemaining
        {
            get { return timeRemaining; }
            set { timeRemaining = value; }
        }
        TimeSpan timeRemaining;

        private const int PointsPerSecond = 5;
        private SoundEffectInstance heartInstance;

        // Level content.        
        //public ContentManager Content { get; set;}
        public ScreenManager ScreenManager { get; set; }

        //private SoundEffect exitReachedSound;



        #region Loading

        /// <summary>
        /// Constructs a new level.
        /// </summary>
        /// <param name="serviceProvider">
        /// The service provider that will be used to construct a ContentManager.
        /// </param>
        /// <param name="fileStream">
        /// A stream containing the tile data.
        /// </param>
        public Level(ScreenManager screenManager)
        {
            // Create a new content manager to load content used just by this level.
            ScreenManager = screenManager;

            timeRemaining = TimeSpan.FromMinutes(2);

            levelTiles = new List<Tile[]>();
            //LoadTiles(fileStream);

            layers = new Layer[2];
            layers[0] = new Layer(ScreenManager.Content, "Backgrounds/background0", 0.2f);
            layers[1] = new Layer(ScreenManager.Content, "Backgrounds/clouds0", 0.8f);
            //layers[2] = new Layer(ScreenManager.Content, "Backgrounds/Layer2", 0.8f);

            // Load sounds.
            heartBeat = ScreenManager.Content.Load<SoundEffect>("Sounds/heart 3");
            //exitReachedSound = ScreenManager.Content.Load<SoundEffect>("Sounds/ExitReached");
            LoadLevelAmount();
            LoadNextLevel();
        }

        public static void LoadLevelAmount()
        {
            if (NumberOfLevels == -1)
            {
                int i = 0;
                try
                {

                    while (true)
                    {
                        using (StreamReader reader = new StreamReader(string.Format("Content/Levels/{0}.txt", i)))
                        {
                            string line = reader.ReadToEnd();
                            foreach (char c in line)
                            {
                                if (c != '\r' && c != '\n')
                                {
                                    //LoadTile(c, 0, 0);
                                }
                            }
                        }
                        i++;
                    }
                }
                catch (FileNotFoundException) { }
                NumberOfLevels = i;
            }
        }

        /// <summary>
        /// Iterates over every tile in the structure file and loads its
        /// appearance and behavior. This method also validates that the
        /// file is well-formed with a player start point, exit, etc.
        /// </summary>
        /// <param name="fileStream">
        /// A stream containing the tile data.
        /// </param>
        private void LoadTiles(Stream fileStream)
        {
            List<string> newLevelLines = ReadAndValidateLevelFile(fileStream);
            int newLevelWidth = newLevelLines[0].Length;


            // Loop over every tile position
            int initialWidth = this.Width;
            for (int x = 0; x < newLevelWidth; ++x)
            {
                Tile[] column = new Tile[levelHeight];
                levelTiles.Add(column);
                for (int y = 0; y < levelHeight; ++y)
                {
                    // to load each tile.
                    char tileType = newLevelLines[y][x];
                    levelTiles[initialWidth + x][y] = LoadTile(tileType, initialWidth + x, y);
                }
            }

            // Verify that the level has a beginning and an end.
            if (Player == null)
                throw new NotSupportedException("A level must have a starting point.");
            //if (exit == InvalidPosition)
            //    throw new NotSupportedException("A level must have an exit.");
        }

        private List<string> ReadAndValidateLevelFile(Stream fileStream)
        {
            // Load the level and ensure all of the lines are the same length.
            List<string> lines = new List<string>();
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line = reader.ReadLine();
                int width = line.Length;
                while (line != null)
                {
                    lines.Add(line);
                    if (line.Length != width)
                        throw new Exception(String.Format("The length of line {0} is different from all preceeding lines.", lines.Count));
                    line = reader.ReadLine();
                }
            }

            if (levelHeight == 0)
                levelHeight = lines.Count;
            else if (levelHeight != lines.Count)
                throw new Exception(String.Format("The height of the level does not match the height of the initial level."));

            return lines;
        }

        /// <summary>
        /// Loads an individual tile's appearance and behavior.
        /// </summary>
        /// <param name="tileType">
        /// The character loaded from the structure file which
        /// indicates what should be loaded.
        /// </param>
        /// <param name="x">
        /// The X location of this tile in tile space.
        /// </param>
        /// <param name="y">
        /// The Y location of this tile in tile space.
        /// </param>
        /// <returns>The loaded tile.</returns>
        private Tile LoadTile(char tileType, int x, int y)
        {
            switch (tileType)
            {
                // Blank space
                case '.':
                    return new Tile(null, TileCollision.Passable);

                // Passable block
                case ':':
                    return LoadTile("Dirt48", TileCollision.Passable);

                // Random powerup in front of passable
                case 'P':
                    LoadRandomPowerUp(x, y);
                    return LoadTile("Dirt48", TileCollision.Passable);

                // Random Powerup
                case 'p':
                    return LoadRandomPowerUp(x, y);

                // Random powerup in front of passable
                case 'E':
                    LoadEnemyTile(x, y, GetRandomMonster());
                    return LoadTile("Dirt48", TileCollision.Passable);

                // Random Enemy
                case 'e':
                    return LoadEnemyTile(x, y, GetRandomMonster());

                // Journal in front of passable
                case 'J':
                    LoadJournalTile(x, y);
                    return LoadTile("Dirt48", TileCollision.Passable);

                // Journal
                case 'j':
                    return LoadJournalTile(x, y);

                // Platform block
                case '~':
                    return LoadTile("DirtandGrass", TileCollision.Platform);

                // Ground block
                case '#':
                    return LoadTile("DirtandGrass", TileCollision.Impassable);

                // Impassable block
                case 'X':
                case 'x':
                    return LoadTile("Impassable", TileCollision.Impassable);

                // Top Double Impassable block
                case 'n':
                    return LoadTile("DoubleImpassableUpper", TileCollision.Impassable);

                // Lower Double Impassable block
                case 'U':
                    return LoadTile("DoubleImpassableLower", TileCollision.Impassable);

                // Flying Platform - Middle
                case '%':
                    return LoadTile("FlyingMiddle", TileCollision.Platform);

                // Flying Platform - Right
                case '>':
                    return LoadTile("FlyingRight", TileCollision.Platform);

                // Flying Platform - Left
                case '<':
                    return LoadTile("FlyingLeft", TileCollision.Platform);

                // Player 1 start point
                case '1':
                    return LoadStartTile(x, y);

                // Unknown tile type character
                default:
                    throw new NotSupportedException(String.Format("Unsupported tile type character '{0}' at position {1}, {2}.", tileType, x, y));
            }
        }

        private string GetRandomMonster()
        {
            return enemieSpriteSets[ScreenManager.Random.Next(enemieSpriteSets.Length)];
        }

        /// <summary>
        /// Creates a new tile. The other tile loading methods typically chain to this
        /// method after performing their special logic.
        /// </summary>
        /// <param name="name">
        /// Path to a tile texture relative to the Content/Tiles directory.
        /// </param>
        /// <param name="collision">
        /// The tile collision type for the new tile.
        /// </param>
        /// <returns>The new tile.</returns>
        private Tile LoadTile(string name, TileCollision collision)
        {
            return new Tile(ScreenManager.Content.Load<Texture2D>("Tiles/" + name), collision);
        }


        /// <summary>
        /// Loads a tile with a random appearance.
        /// </summary>
        /// <param name="baseName">
        /// The content name prefix for this group of tile variations. Tile groups are
        /// name LikeThis0.png and LikeThis1.png and LikeThis2.png.
        /// </param>
        /// <param name="variationCount">
        /// The number of variations in this group.
        /// </param>
        private Tile LoadVarietyTile(string baseName, int variationCount, TileCollision collision)
        {
            int index = ScreenManager.Random.Next(variationCount);
            return LoadTile(baseName + index, collision);
        }


        /// <summary>
        /// Instantiates a player, puts him in the level, and remembers where to put him when he is resurrected.
        /// </summary>
        private Tile LoadStartTile(int x, int y)
        {
            if (Player == null)
            {
                start = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
                start.Y -= 100;
                player = new Player(this, start);

                return new Tile(null, TileCollision.Passable);
            }
            else
                return new Tile(null, TileCollision.Passable);
        }

        /// <summary>
        /// Remembers the location of the level's exit.
        /// </summary>
        private Tile LoadExitTile(int x, int y)
        {
            if (exit == InvalidPosition)
            {

                exit = GetBounds(x, y).Center;

                return LoadTile("Exit", TileCollision.Passable);
            }
            else
                return new Tile(null, TileCollision.Passable);
        }

        /// <summary>
        /// Instantiates an enemy and puts him in the level.
        /// </summary>
        private Tile LoadEnemyTile(int x, int y, string spriteSet)
        {
            Vector2 position = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            enemies.Add(new Enemy(this, position, spriteSet));

            return new Tile(null, TileCollision.Passable);
        }

        private Tile LoadJournalTile(int x, int y)
        {
            Point position = GetBounds(x, y).Center;
            gems.Add(new Journal(this, new Vector2(position.X, position.Y)));
            return new Tile(null, TileCollision.Passable);
        }

        private Tile LoadRandomPowerUp(int x, int y)
        {
            Point position = GetBounds(x, y).Center;

            const int numberOfPowerups = 4;
            const float probabilityOfPowerup = 0.75f;

            if (ScreenManager.Random.NextDouble() > probabilityOfPowerup)
            {
                int powerupToLoad = ScreenManager.Random.Next(numberOfPowerups);
                switch (powerupToLoad)
                {
                    case 0:
                        gems.Add(new Aspirin(this, new Vector2(position.X, position.Y)));
                        break;
                    case 1:
                        gems.Add(new WineGlass(this, new Vector2(position.X, position.Y)));
                        break;
                    case 2:
                        gems.Add(new EnergyDrink(this, new Vector2(position.X, position.Y)));
                        break;
                    case 3:
                        gems.Add(new Salmon(this, new Vector2(position.X, position.Y)));
                        break;
                    default:
                        throw new IndexOutOfRangeException(String.Format("Random number generator selected an invalid powerup. Check constant value for number of powerups."));
                }
            }

            return new Tile(null, TileCollision.Passable);
        }

        /// <summary>
        /// Unloads the level content.
        /// </summary>
        public void Dispose()
        {
            ScreenManager.Content.Unload();
        }

        #endregion

        #region Bounds and collision

        /// <summary>
        /// Gets the collision mode of the tile at a particular location.
        /// This method handles tiles outside of the levels boundries by making it
        /// impossible to escape past the left or right edges, but allowing things
        /// to jump beyond the top of the level and fall off the bottom.
        /// </summary>
        public TileCollision GetCollision(int x, int y)
        {
            // Prevent escaping past the level ends.
            if (x < 0 || x >= Width)
                return TileCollision.Impassable;
            // Allow jumping past the level top and falling through the bottom.
            if (y < 0 || y >= Height)
                return TileCollision.Passable;

            return levelTiles[x][y].Collision;
        }

        /// <summary>
        /// Gets the bounding rectangle of a tile in world space.
        /// </summary>        
        public Rectangle GetBounds(int x, int y)
        {
            return new Rectangle(x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
        }

        /// <summary>
        /// Width of level measured in tiles.
        /// </summary>
        public int Width
        {
            get { return levelTiles.Count; }
        }

        /// <summary>
        /// Height of the level measured in tiles.
        /// </summary>
        public int Height
        {
            get { return levelHeight; }
        }

        #endregion

        #region Update

        /// <summary>
        /// Updates all objects in the world, performs collision between them,
        /// and handles the time limit with scoring.
        /// </summary>
        public void Update(
            GameTime gameTime,
            KeyboardState keyboardState,
            GamePadState gamePadState
            )
        {
            TimeSpan previousTime = timeRemaining;

            if (player.Position.X + 1280 > Width * Tile.Width)
                LoadNextLevel();

            //if (player.Position.X / Tile.Width > Width * 0.66f)
            //    LoadNextLevel();

            // Pause while the player is dead or time is expired.
            if (!Player.IsAlive || TimeRemaining == TimeSpan.Zero)
            {
                // Still want to perform physics on the player.
                Player.ApplyPhysics(gameTime);
            }
            else if (ReachedExit)
            {
                // Animate the time being converted into points.
                int seconds = (int)Math.Round(gameTime.ElapsedGameTime.TotalSeconds * 100.0f);
                seconds = Math.Min(seconds, (int)Math.Ceiling(TimeRemaining.TotalSeconds));
                timeRemaining -= TimeSpan.FromSeconds(seconds);
                score += seconds * PointsPerSecond;
            }
            else
            {
                timeRemaining -= new TimeSpan((long)(gameTime.ElapsedGameTime.Ticks * player.TimeModifier));
                Player.Update(gameTime, keyboardState, gamePadState);
                UpdateGems(gameTime);

                // Falling off the bottom of the level kills the player.
                if (Player.BoundingRectangle.Top >= Height * Tile.Height)
                    OnPlayerKilled(null);

                UpdateEnemies(gameTime);

                // The player has reached the exit if they are standing on the ground and
                // his bounding rectangle contains the center of the exit tile. They can only
                // exit when they have collected all of the gems.
                if (Player.IsAlive &&
                    Player.IsOnGround &&
                    Player.BoundingRectangle.Contains(exit))
                {
                    OnExitReached();
                }
            }

            // Clamp the time remaining at zero.
            if (heartInstance == null && timeRemaining < new TimeSpan(0, 0, 6))
            {
                heartInstance = heartBeat.CreateInstance();
                heartInstance.Play();
            }
            if (heartInstance != null && timeRemaining > new TimeSpan(0, 0, 6))
            {
                heartInstance.Stop();
                heartInstance = null;
            }

            if (timeRemaining < TimeSpan.Zero)
                timeRemaining = TimeSpan.Zero;
        }

        private void LoadNextLevel()
        {
            int levelIndex = levelIndex = ScreenManager.Random.Next(NumberOfLevels);
            //levelIndex = 20;
            string levelPath = string.Format("Content/Levels/{0}.txt", levelIndex);
            using (Stream fileStream = TitleContainer.OpenStream(levelPath))
                LoadTiles(fileStream);
        }

        /// <summary>
        /// Animates each gem and checks to allows the player to collect them.
        /// </summary>
        private void UpdateGems(GameTime gameTime)
        {
            for (int i = 0; i < gems.Count; ++i)
            {
                Gem gem = gems[i];

                gem.Update(gameTime);

                if (gem.BoundingCircle.Intersects(Player.BoundingRectangle))
                {
                    gems.RemoveAt(i--);
                    OnGemCollected(gem, Player);
                }
            }
        }

        /// <summary>
        /// Animates each enemy and allow them to kill the player.
        /// </summary>
        private void UpdateEnemies(GameTime gameTime)
        {
            foreach (Enemy enemy in enemies)
            {
                enemy.Update(gameTime);

                // Touching an enemy instantly kills the player
                if (enemy.BoundingRectangle.Intersects(Player.BoundingRectangle))
                {
                    OnPlayerKilled(enemy);
                }
            }
        }

        /// <summary>
        /// Called when a gem is collected.
        /// </summary>
        /// <param name="gem">The gem that was collected.</param>
        /// <param name="collectedBy">The player who collected this gem.</param>
        private void OnGemCollected(Gem gem, Player collectedBy)
        {
            score += Gem.PointValue;

            gem.OnCollected(collectedBy);
        }

        /// <summary>
        /// Called when the player is killed.
        /// </summary>
        /// <param name="killedBy">
        /// The enemy who killed the player. This is null if the player was not killed by an
        /// enemy, such as when a player falls into a hole.
        /// </param>
        private void OnPlayerKilled(Enemy killedBy)
        {
            Player.OnKilled(killedBy);
        }

        /// <summary>
        /// Called when the player reaches the level's exit.
        /// </summary>
        private void OnExitReached()
        {
            Player.OnReachedExit();
            //exitReachedSound.Play();
            reachedExit = true;
        }

        /// <summary>
        /// Restores the player to the starting point to try the level again.
        /// </summary>
        public void StartNewLife()
        {
            Player.Reset(start);
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draw everything in the level from background to foreground.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            for (int i = 0; i <= EntityLayer; ++i)
                layers[i].Draw(spriteBatch, cameraPosition);
            spriteBatch.End();

            ScrollCamera(spriteBatch.GraphicsDevice.Viewport);
            Matrix cameraTransform = Matrix.CreateTranslation(-cameraPosition, 0.0f, 0.0f);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, cameraTransform);

            DrawTiles(spriteBatch);

            foreach (Gem gem in gems)
                gem.Draw(gameTime, spriteBatch);

            Player.Draw(gameTime, spriteBatch);

            foreach (Enemy enemy in enemies)
                enemy.Draw(gameTime, spriteBatch);

            spriteBatch.End();

            spriteBatch.Begin();
            for (int i = EntityLayer + 1; i < layers.Length; ++i)
                layers[i].Draw(spriteBatch, cameraPosition);
            spriteBatch.End();
        }

        /// <summary>
        /// Draws each tile in the level.
        /// </summary>
        private void DrawTiles(SpriteBatch spriteBatch)
        {
            int left = (int)Math.Floor(cameraPosition / Tile.Width);
            int right = left + spriteBatch.GraphicsDevice.Viewport.Width;// / Tile.Width;
            right = Math.Min(right, Width - 1);

            // For each tile position
            for (int x = left; x <= right; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    // If there is a visible tile in that position
                    Texture2D texture = levelTiles[x][y].Texture;
                    if (texture != null)
                    {
                        // Draw it in screen space.
                        Vector2 position = new Vector2(x, y) * Tile.Size;
                        spriteBatch.Draw(texture, position, Color.White);
                    }
                }
            }
        }

        private void ScrollCamera(Viewport viewport)
        {
            const float ViewMargin = 0.35f;

            float marginWidth = viewport.Width * ViewMargin;
            float marginLeft = cameraPosition + marginWidth;
            float marginRight = cameraPosition + viewport.Width - marginWidth;

            float cameraMovement = 0.0f;
            if (Player.Position.X < marginLeft)
                cameraMovement = Player.Position.X - marginLeft;
            else if (Player.Position.X > marginRight)
                cameraMovement = Player.Position.X - marginRight;

            float maxCameraPosition = Tile.Width * Width - viewport.Width;
            cameraPosition = MathHelper.Clamp(cameraPosition + cameraMovement, 0.0f, maxCameraPosition);
        }

        #endregion
    }
}
