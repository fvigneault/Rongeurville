﻿using System;

namespace Rongeurville.Communication
{
    [Serializable]
    public class MoveRequest : Request
    {
        public Tile DesiredTile;
    }
}