﻿namespace PlanetWars.Server.Data
{
    using System.Collections.Generic;

    public class Lobby
    {
        public Lobby()
        {
            Games = new HashSet<Game>();
            Players = new HashSet<Player>();
        }

        public string Id { get; set; }

        public string OwnerId { get; set; }

        public Player Owner { get; set; }

        public string Game { get; set; }

        public int MaxPlayers { get; set; }

        public int Seed { get; set; }

        public bool IsActive { get; set; }

        public HashSet<Game> Games { get; set; }

        public HashSet<Player> Players { get; set; }
    }
}