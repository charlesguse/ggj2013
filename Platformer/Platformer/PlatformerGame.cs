#region File Description
//-----------------------------------------------------------------------------
// PlatformerGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input.Touch;
using Unicorn.ScreenArchitecture;
using Microsoft.Xna.Framework.Content;
using Unicorn.Screens;
using Platformer.Screens;


namespace Unicorn
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class PlatformerGame : GameScreen
    {
        // Resources for drawing.
        //private GraphicsDeviceManager graphics;
        //private SpriteBatch spriteBatch;
        public ContentManager Content;
        // Global content.
        private SpriteFont hudFont;

        //private Texture2D winOverlay;
        //private Texture2D loseOverlay;
        //private Texture2D diedOverlay;

        // Meta-level game state.
        private int levelIndex = -1;
        private Level level;
        private bool wasContinuePressed;

        // When the time remaining is less than the warning time, it blinks on the hud
        private static readonly TimeSpan WarningTime = TimeSpan.FromSeconds(30);

        // We store our input states so that we only poll once per frame, 
        // then we use the same input state wherever needed
        private GamePadState gamePadState;
        private KeyboardState keyboardState;
        //private TouchCollection touchState;
        //private AccelerometerState accelerometerState;

        // The number of levels in the Levels directory of our content. We assume that
        // levels in our content are 0-based and that all numbers under this constant
        // have a level file present. This allows us to not need to check for the file
        // or handle exceptions, both of which can add unnecessary time to level loading.
        private const int numberOfLevels = 7;

        public PlatformerGame()
        {
            //            graphics = new GraphicsDeviceManager(this);
            //            Content.RootDirectory = "Content";

            //#if WINDOWS_PHONE
            //            graphics.IsFullScreen = true;
            //            TargetElapsedTime = TimeSpan.FromTicks(333333);
            //#endif

            //            Accelerometer.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        public override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            //spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load fonts
            hudFont = Content.Load<SpriteFont>("Fonts/Hud");

            // Load overlay textures
            //winOverlay = Content.Load<Texture2D>("Overlays/you_win");
            //loseOverlay = Content.Load<Texture2D>("Overlays/you_lose");
            //diedOverlay = Content.Load<Texture2D>("Overlays/you_died");

            //Known issue that you get exceptions if you use Media PLayer while connected to your PC
            //See http://social.msdn.microsoft.com/Forums/en/windowsphone7series/thread/c8a243d2-d360-46b1-96bd-62b1ef268c66
            //Which means its impossible to test this from VS.
            //So we have to catch the exception and throw it away
            try
            {
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(Content.Load<Song>("Sounds/FINAL LEVEL LOOP 2"));
            }
            catch { }

            LoadNextLevel();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            // Handle polling for our input and handling high-level input
            HandleInput();

            // update our level, passing down the GameTime along with all of our input states
            level.Update(gameTime, keyboardState, gamePadState);

            if (level.TimeRemaining == TimeSpan.Zero)
            {
                ScreenManager.AddScreen(new GameOverScreen(Ending.TimeOut), null);
                LoadingScreen.Load(ScreenManager, true, null,
                              new GameOverScreen(Ending.TimeOut));
            }
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        private void HandleInput()
        {
            // get all of our input states
            keyboardState = Keyboard.GetState();
            gamePadState = GamePad.GetState(PlayerIndex.One);

            // Exit the game when back is pressed.
            //if (gamePadState.Buttons.Back == ButtonState.Pressed)
            //    Exit();

            bool continuePressed =
                keyboardState.IsKeyDown(Keys.Space) ||
                gamePadState.IsButtonDown(Buttons.A);

            // Perform the appropriate action to advance the game and
            // to get the player back to playing.
            if (!wasContinuePressed && continuePressed)
            {
                if (!level.Player.IsAlive)
                {
                    level.StartNewLife();
                }
                else if (level.TimeRemaining == TimeSpan.Zero)
                {
                    if (level.ReachedExit)
                        LoadNextLevel();
                    else
                        ReloadCurrentLevel();
                }
            }

            wasContinuePressed = continuePressed;
        }

        private void LoadNextLevel()
        {
            level = new Level(ScreenManager);
        }

        private void ReloadCurrentLevel()
        {
            --levelIndex;
            LoadNextLevel();
        }

        /// <summary>
        /// Draws the game from background to foreground.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            //graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            level.Draw(gameTime, ScreenManager.SpriteBatch);

            DrawHud();

            base.Draw(gameTime);
        }

        private void DrawHud()
        {
            ScreenManager.SpriteBatch.Begin();
            Rectangle titleSafeArea = ScreenManager.GraphicsDevice.Viewport.TitleSafeArea;
            Vector2 hudLocation = new Vector2(titleSafeArea.X, titleSafeArea.Y);
            Vector2 center = new Vector2(titleSafeArea.X + titleSafeArea.Width / 2.0f,
                                         titleSafeArea.Y + titleSafeArea.Height / 2.0f);

            // Draw time remaining. Uses modulo division to cause blinking when the
            // player is running out of time.
            string timeString = "TIME: " + level.TimeRemaining.Minutes.ToString("00") + ":" + level.TimeRemaining.Seconds.ToString("00");
            Color timeColor;
            if (level.TimeRemaining > WarningTime ||
                level.ReachedExit ||
                (int)level.TimeRemaining.TotalSeconds % 2 == 0)
            {
                timeColor = Color.Yellow;
            }
            else
            {
                timeColor = Color.Red;
            }
            DrawShadowedString(hudFont, timeString, hudLocation, timeColor);

            // Draw score
            float timeHeight = hudFont.MeasureString(timeString).Y;
            //DrawShadowedString(hudFont, "SCORE: " + level.Score.ToString(), hudLocation + new Vector2(0.0f, timeHeight * 1.2f), Color.Yellow);

            string fatString = "FATTY FATNESS: " + (int)(level.Player.Fattyfatness * 100);
            Color fatColor;
            fatColor = (level.Player.FatIsTooHigh() || level.Player.FatIsTooLow()) ? Color.Red : Color.Yellow;
            DrawShadowedString(hudFont, fatString, hudLocation + new Vector2(0.0f, timeHeight * 1.2f), fatColor);

            ScreenManager.SpriteBatch.End();
        }

        private void DrawShadowedString(SpriteFont font, string value, Vector2 position, Color color)
        {
            ScreenManager.SpriteBatch.DrawString(font, value, position + new Vector2(1.0f, 1.0f), Color.Black);
            ScreenManager.SpriteBatch.DrawString(font, value, position, color);
        }
    }
}
