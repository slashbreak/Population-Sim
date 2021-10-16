using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game_2d_terrain
{
    public class FrameRateCounter : Microsoft.Xna.Framework.DrawableGameComponent
    {
        const long ONE_SECOND = 1000; //1 second
 
        private SpriteFont m_font;
        private SpriteBatch m_sb;
        private long m_timeStamp;
        private long m_frames;
        private float m_fps;
        private float m_threshold = 0.80f; //What percent below the target fps
                                           //should the display text change color
        private long m_targetFPS = 30;     //The rate are we shooting for
 
        public FrameRateCounter(Game game, SpriteFont sf)
            : base(game)
        {
            // TODO: Construct any child components here
            m_font = sf;
            m_timeStamp = 0;
            m_frames = 0;
            m_sb = new SpriteBatch(Game.GraphicsDevice);
        }
 
        ///
 
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// 
 
        public override void Initialize()
        {
            // TODO: Add your initialization code here
            base.Initialize();
        }
 
        protected override void LoadContent()
        {
            base.LoadContent();
        }
 
        ///
 
        /// Allows the game component to update itself.
        /// 
 
        ///

        public override void Update(GameTime gameTime)
        {
            long time = (long)gameTime.TotalGameTime.TotalMilliseconds;
            calcFPS(time);
            base.Update(gameTime);
        }
 
        public override void Draw(GameTime gameTime)
        {
            m_sb.Begin();
            renderFPS();
            m_sb.End();
            m_frames++;
            base.Draw(gameTime);
        }
 
        private void calcFPS(long time)
        {
            long diff = time - m_timeStamp;
            if (diff >= ONE_SECOND)
            {
                m_fps = m_frames;
                m_frames = 0;
                m_timeStamp = time;
            }
        }
 
        private void renderFPS()
        {
            string fps = "FPS: " + m_fps;
            float fpsRot = 0;
            Vector2 fpsPos = new Vector2(0, 80);
            Color color;
            if (m_fps < (m_threshold * m_targetFPS))
                color = Color.IndianRed;
            else
                color = Color.DarkSeaGreen;
 
            m_sb.DrawString(m_font, fps, fpsPos, color,
                fpsRot, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0.5f);
        }
    }
}