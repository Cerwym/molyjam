using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pidgeon
{
    internal class PickupObject
    {
        public CModel pickUp;
        public CModel collision;
        public int NumOF;
        public int _id;

        public PickupObject(int id)
        {
            _id = id;
        }
    }
}