using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unicorn.PowerUps;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;

namespace Unicorn.PowerUps
{
    public class WineGlass : Gem
    {
        public WineGlass(Level level, Vector2 position) :
            base(level, position)
        {

        }

        public override void LoadContent()
        {
            Texture = Level.Content.Load<Texture2D>("Sprites/PowerUps/Goblet filled stem");
            //origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);
            CollectedSound = Level.Content.Load<SoundEffect>("Sounds/GemCollected");
            base.LoadContent();
        }
    }
}
