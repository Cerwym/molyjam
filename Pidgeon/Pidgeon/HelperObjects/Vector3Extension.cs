using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Pidgeon.HelperObjects
{
    public static class Vector3Extension
    {

        public static float NextFloat(this Random rand)
        {
            return (float)rand.NextDouble();
        }
        
        public static float NextFloat(this Random rand, float min, float max)
        {
            if (max < min)
                throw new ArgumentException("max cannot be less than min");
            return (float)rand.NextDouble() * (max - min) + min;
        } 

        public static Vector3 NextVector3(this Random random, Vector3 min, Vector3 max)
        {
            if (max.X < min.X)
            {
                throw new ArgumentException("Max (x) cannot be less than min");
            }
            if (max.Y < min.Y)
            {
                throw new ArgumentException("Max (y) cannot be less than min");
            }
            if (max.Z < min.Z)
            {
                throw new ArgumentException("Max (z) cannot be less than min");
            }

            return new Vector3(random.NextFloat(min.X, max.X), random.NextFloat(min.Y, max.Y), random.NextFloat(min.Z, max.Z));
        }  
    }
}
