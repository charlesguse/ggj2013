using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unicorn.ScreenArchitecture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Unicorn.Screens;
using Unicorn;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace Platformer.Screens
{
    public enum Ending
    {
        Win,
        Diabetes,
        TimeOut,
        Death
    }
    public class GameOverScreen : GameScreen
    {
        public Ending Ending { get; set; }
        private Texture2D background { get; set; }
        private Texture2D wilford { get; set; }
        private Texture2D diabeetus { get; set; }
        private float diabeetusTime;
        private Vector2 diabeetusSpawnLocation;
        private SoundEffect diabeetusSound;

        public GameOverScreen(Ending ending)
        {
            Ending = ending;
        }

        public override void LoadContent()
        {
            diabeetusSound = ScreenManager.Content.Load<SoundEffect>("Sounds/Wilford");
            background = ScreenManager.Content.Load<Texture2D>("Backgrounds/diabeetusEndGame");
            switch (Ending)
            {
                case Ending.Win:
                    background = ScreenManager.Content.Load<Texture2D>("Backgrounds/journalEndGame");
                    break;
                case Ending.Diabetes:
                    background = ScreenManager.Content.Load<Texture2D>("Backgrounds/diabeetusEndGame");
                    break;
                case Ending.TimeOut:
                    break;
                case Ending.Death:
                    break;
                default:
                    break;
            }
            wilford = ScreenManager.Content.Load<Texture2D>("Sprites/wilfordHead");
            diabeetus = ScreenManager.Content.Load<Texture2D>("Sprites/diabeetus");

            base.LoadContent();
        }

        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            PlayerIndex junk;
            if (input.IsPauseGame(ControllingPlayer) || input.IsMenuSelect(ControllingPlayer, out junk))
            {
                LoadingScreen.Load(ScreenManager, true, null,
                    new BackgroundScreen(),
                    new MainMenuScreen());
            }
            //LoadingScreen.Load(Level.ScreenManager, true, null,
            //                   new GameOverScreen(Ending.Death));
        }

        public override void  Draw(GameTime gameTime)
        {
            diabeetusTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            ScreenManager.GraphicsDevice.Clear(Color.Black);

            ScreenManager.SpriteBatch.Begin();
            ScreenManager.SpriteBatch.Draw(background, Vector2.Zero, Color.White);

            switch (Ending)
            {
                case Ending.Diabetes:
                    DrawDiabeetusStuff(gameTime);
                    break;
                case Ending.TimeOut:
                    ScreenManager.SpriteBatch.DrawString(ScreenManager.Font, "Time Out!", new Vector2(1280 / 2, 720 / 2), Color.White);
                    break;
                case Ending.Death:
                    ScreenManager.SpriteBatch.DrawString(ScreenManager.Font, "Death!", new Vector2(1280 / 2, 720 / 2), Color.White);
                    break;
                default:
                    break;
            }
            ScreenManager.SpriteBatch.End();
        }

        private void DrawDiabeetusStuff(GameTime gameTime)
        {
            if (diabeetusTime <= 0)
            {
                diabeetusSound.Play();
                diabeetusTime = (float)(ScreenManager.Random.NextDouble() * 3);
                diabeetusSpawnLocation = new Vector2((float)(ScreenManager.Random.NextDouble() * 1280), (float)(ScreenManager.Random.NextDouble() * 720));
            }
            ScreenManager.SpriteBatch.Draw(diabeetus, diabeetusSpawnLocation, Color.Black);
            var middle = new Vector2(1280 / 2 - wilford.Width / 2, 720 / 2 - wilford.Height / 2);
            ScreenManager.SpriteBatch.Draw(wilford, middle, null, Color.White, (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds), Vector2.Zero, 0f, SpriteEffects.None, 0f);
        }
    }
}
