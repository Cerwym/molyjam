using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pidgeon.Models;

namespace Pidgeon
{
    class Building : Renderable3D
    {
        private bool _isDestination;

        public Building(Model Model, Vector3 Position, Vector3 Rotation, Vector3 Scale) : base(Model, Position, Rotation, Scale)
        {
            _isDestination = false;
        }
    }
}
