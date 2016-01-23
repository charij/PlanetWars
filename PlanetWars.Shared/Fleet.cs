﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetWars.Shared
{
    public class Fleet
    {
        public int Id { get; set; }
        public int Owner { get; set; }
        public int NumberOfShips { get; set; }
        public int SourcePlanetId { get; set; }
        public int DestinationPlanetId { get; set; }
    }
}
