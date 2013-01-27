using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace Unicorn.PowerUps
{
    public class Sandwhich : Gem
    {
        public Sandwhich(Level level, Vector2 position) :
            base(level, position)
        {

        }

        public override void LoadContent()
        {
            Texture = Level.ScreenManager.Content.Load<Texture2D>("Sprites/Powerups/Sub Dead-on");
            //origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);
            CollectedSound = Level.ScreenManager.Content.Load<SoundEffect>("Sounds/SANDWICH SFX");
            base.LoadContent();
        }

        public override void OnCollected(Player collectedBy)
        {
            Level.TimeRemaining += new TimeSpan(0, 0, 3);
            Level.Player.Fattyfatness += 0.70f;
            base.OnCollected(collectedBy);
        }
    }
}
