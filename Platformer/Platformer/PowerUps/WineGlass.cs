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
            Texture = Level.ScreenManager.Content.Load<Texture2D>("Sprites/PowerUps/Goblet filled stem");
            //origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);
            CollectedSound = Level.ScreenManager.Content.Load<SoundEffect>("Sounds/WINE SFX");
            base.LoadContent();
        }

        public override void OnCollected(Player collectedBy)
        {
            Level.TimeRemaining += new TimeSpan(0, 0, 15);
            Level.Player.Fattyfatness -= 0.25f;
            base.OnCollected(collectedBy);
        }
    }
}
