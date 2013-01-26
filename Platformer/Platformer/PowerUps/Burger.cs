using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace Unicorn.PowerUps
{
    public class EnergyDrink : Gem
    {
        public EnergyDrink(Level level, Vector2 position) :
            base(level, position)
        {

        }

        public override void LoadContent()
        {
            Texture = Level.Content.Load<Texture2D>("Sprites/Powerups/EnergyDrink");
            //origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);
            CollectedSound = Level.Content.Load<SoundEffect>("Sounds/GemCollected");
            base.LoadContent();
        }

        public override void OnCollected(Player collectedBy)
        {
            Level.TimeRemaining += new TimeSpan(0, 0, 15);
            Level.Player.Fattyfatness += 0.02f;
            base.OnCollected(collectedBy);
        }
    }
}
