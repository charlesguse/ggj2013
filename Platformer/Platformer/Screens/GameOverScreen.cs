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
        private Vector2 diabeetusSpawnLocation;
        private Vector2 wilfordLocation;
        private Vector2 wilfordVelocity;
        private SoundEffect diabeetusSound;

        public GameOverScreen(Ending ending)
        {
            Ending = ending;

            wilfordLocation = new Vector2(1280 / 2, 720 / 2);
            wilfordVelocity = new Vector2(200, 200);
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
                    background = ScreenManager.Content.Load<Texture2D>("Backgrounds/endGameAllother");
                    break;
                case Ending.Death:
                    background = ScreenManager.Content.Load<Texture2D>("Backgrounds/endGameAllother");
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
        }

        public override void  Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(Color.Black);

            ScreenManager.SpriteBatch.Begin();
            ScreenManager.SpriteBatch.Draw(background, Vector2.Zero, Color.White);

            switch (Ending)
            {
                case Ending.Diabetes:
                    DrawDiabeetusStuff(gameTime);
                    break;
                default:
                    break;
            }
            ScreenManager.SpriteBatch.End();
        }

        private void DrawDiabeetusStuff(GameTime gameTime)
        {
            float wilfordNewX = wilfordLocation.X + wilfordVelocity.X;
            float wilfordNewY = wilfordLocation.Y + wilfordVelocity.Y;
            if (wilfordNewX < 0 || wilfordNewX > 1280)
            {
                wilfordVelocity.X *= -1;
                PlayDiabeetus();
            }
            if (wilfordNewY < 0 || wilfordNewY > 720)
            {
                wilfordVelocity.Y *= -1;
                PlayDiabeetus();
            }

            wilfordLocation += wilfordVelocity*(float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 origin = new Vector2(wilford.Width / 2, wilford.Height / 2);
            ScreenManager.SpriteBatch.Draw(wilford, wilfordLocation, null, Color.White, (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds), origin, 1f, SpriteEffects.None, 0f);
            ScreenManager.SpriteBatch.Draw(diabeetus, diabeetusSpawnLocation, Color.Black);
        }

        private void PlayDiabeetus()
        {
            diabeetusSound.Play();
            diabeetusSpawnLocation = new Vector2((float)(ScreenManager.Random.NextDouble() * 1200), (float)(ScreenManager.Random.NextDouble() * 700));
        }
    }
}
