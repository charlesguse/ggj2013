using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unicorn.PowerUps;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;
using System.IO;

namespace Unicorn.PowerUps
{
    public class Journal : Gem
    {
        public static int journalPiece = 0;
        public static int journalMax = 0;
        public static List<string> JournalDoc = null;

        public Journal(Level level, Vector2 position) :
            base(level, position)
        {

        }

        public override void LoadContent()
        {
            if (JournalDoc == null)
            {
                LoadStories();
            }

            //Texture = Level.Content.Load<Texture2D>("Sprites/PowerUps/Goblet filled stem");
            Texture = Level.ScreenManager.Content.Load<Texture2D>("Sprites/PowerUps/Journal");
            //origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);
            CollectedSound = Level.ScreenManager.Content.Load<SoundEffect>("Sounds/PAPER SFX");
            base.LoadContent();
        }

        public static void LoadStories()
        {
            int i = 0;

            try
            {
                JournalDoc = new List<string>();
                while (true)
                {
                    using (StreamReader reader = new StreamReader(string.Format("Content/Story/{0}.txt", i++)))
                    {
                        string line = reader.ReadToEnd();
                        if (line != null)
                        {
                            JournalDoc.Add(line);
                        }
                    }
                }
            }
            catch
            {
                journalMax = i - 1;
            }
        }

        public override void OnCollected(Player collectedBy)
        {
            MessageBoxScreen confirmExitMessageBox;
            //Level.TimeRemaining += new TimeSpan(0, 0, 15);
            if (journalPiece < journalMax)
            {
                confirmExitMessageBox = new MessageBoxScreen(JournalDoc[journalPiece++]);
            }
            else
            {
                confirmExitMessageBox = new MessageBoxScreen("You have found all of the journal entries");
            }
            Level.ScreenManager.AddScreen(confirmExitMessageBox, null);

            base.OnCollected(collectedBy);
        }

        public static bool AllJournalPiecesFound()
        {
            if (JournalDoc == null)
            {
                LoadStories();
            }
            return journalPiece >= journalMax;
        }
    }
}
