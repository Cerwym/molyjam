using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pidgeon
{
    class BuisnessMan : CModel
    {
        public CModel collision;

        // Wanted Objects.
        // Maybe stuff that they want
        // Might delete this later
        // Fuck it, we'll see.
        public BuisnessMan(Model model, GraphicsDevice graphicsDevice, Matrix world, BasicEffect effect) : base(model, graphicsDevice, world, effect)
        {
        }
    }
}
