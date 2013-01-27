using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unicorn.ScreenArchitecture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Unicorn.Screens;
using Unicorn;

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

        public GameOverScreen(Ending ending)
        {
            Ending = ending;
        }

        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            if (input.IsPauseGame(ControllingPlayer))
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
            ScreenManager.GraphicsDevice.Clear(Color.Black);

            ScreenManager.SpriteBatch.Begin();
            switch (Ending)
            {
                case Ending.Win:
                    ScreenManager.SpriteBatch.DrawString(ScreenManager.Font, "Win!", new Vector2(1280 / 2, 720 / 2), Color.White);
                    break;
                case Ending.Diabetes:
                    ScreenManager.SpriteBatch.DrawString(ScreenManager.Font, "Diabetes!", new Vector2(1280 / 2, 720 / 2), Color.White);
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
    }
}
