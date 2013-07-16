using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pidgeon
{
    public class HudText
    {
        private readonly Texture2D _texture;
        private TimeSpan _fadeTime;
        private Vector2 _position;
        public bool DoneDrawing;
        private float _opacityAlpha;

        public HudText(Texture2D texture, TimeSpan fadeOutTime, Vector2 pos)
        {
            _texture = texture;
            _fadeTime = fadeOutTime;
            _position = pos;
            _opacityAlpha = 255f;

            DoneDrawing = false;
        }

        public void Update(GameTime gameTime)
        {
            _fadeTime -= gameTime.ElapsedGameTime;
            _position.Y += 1.5f;
            _opacityAlpha -= 1.5f;
            Console.WriteLine("Alpha " + _opacityAlpha);

            if (_fadeTime.TotalMilliseconds <= 0)
                DoneDrawing = true;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

            spriteBatch.Draw(_texture, new Vector2(_position.X - _texture.Width /2, _position.Y - _texture.Height /2), null, new Color(255,255,255, _opacityAlpha));

            spriteBatch.End();
        }
    }
}
