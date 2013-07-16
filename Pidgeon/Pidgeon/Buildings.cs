using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Pidgeon
{
    class Buildings : CModel
    {
        private bool _istheGoal;

        public Buildings(Model model, GraphicsDevice graphicsDevice, Matrix world, BasicEffect effect)
            : base(model, graphicsDevice, world, effect)
        {
            _istheGoal = false;
        }
    }
}
