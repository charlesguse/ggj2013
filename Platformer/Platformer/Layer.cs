using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Unicorn
{
    public class Layer
    {
        public Texture2D[] Textures { get; private set; }
        public float ScrollRate { get; private set; }

        public Layer(ContentManager content, string basePath, float scrollRate) 
        {
            Textures = new Texture2D[3];

            for (int i = 0; i < 3; ++i)
                Textures[i] = content.Load<Texture2D>(basePath + "_" + i);

            ScrollRate = scrollRate;
        }

        public void Draw(SpriteBatch spriteBatch, float cameraPosition)
        {
            int segmentWidth = Textures[0].Width;

            float x = cameraPosition * ScrollRate;
            int leftSegment = (int)Math.Floor(x / segmentWidth);
            x = (x / segmentWidth - leftSegment) * -segmentWidth;


            spriteBatch.Draw(Textures[leftSegment % Textures.Length], new Vector2(x, 0.0f), Color.White);
            spriteBatch.Draw(Textures[(leftSegment + 1) % Textures.Length], new Vector2(x + segmentWidth, 0.0f), Color.White);
            spriteBatch.Draw(Textures[(leftSegment + 2) % Textures.Length], new Vector2(x + segmentWidth + segmentWidth, 0.0f), Color.White);
        }

    }
}
