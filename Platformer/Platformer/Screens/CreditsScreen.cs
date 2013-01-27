using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Unicorn;
using Unicorn.ScreenArchitecture;
using Microsoft.Xna.Framework.Graphics;
using Unicorn.Screens;

namespace Platformer.Screens
{
    public class CreditsScreen : GameScreen
    {
        private Texture2D background { get; set; }

        public override void LoadContent()
        {
            background = ScreenManager.Content.Load<Texture2D>("Backgrounds/CreditsScreen");
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

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(Color.Black);
            ScreenManager.SpriteBatch.Begin();
            ScreenManager.SpriteBatch.Draw(background, Vector2.Zero, Color.White);
            ScreenManager.SpriteBatch.End();
        }
    }
}
