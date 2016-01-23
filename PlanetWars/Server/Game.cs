﻿using PlanetWars.Commands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PlanetWars.Server
{
    public interface IGame
    {
        LogonResult LogonPlayer(string playerName);
        void Update(long delta);
        void Start();
        void Stop();
    }

    public class Game : IGame
    {
        private static int _MAXID = 0;
        private static readonly long START_DELAY = 5000; // 5 seconds
        private static readonly long TURN_LENGTH = 200; // 200 ms
        private static readonly long PROCESSING = 200; // 200 ms
        private static readonly int MAX_TURN = 200; // default 200 turns

        public static bool IsRunningLocally = HttpContext.Current.Request.IsLocal;
        public bool Running { get; private set; }
        public int Id { get; private set; }
        public Random Random { get; set; }
        public int Turn { get; private set; }

        private HighFrequencyTimer _gameLoop = null;
        public ConcurrentDictionary<string, Player> Players = new ConcurrentDictionary<string, Player>();
        private ConcurrentDictionary<string, Player> _authTokens = new ConcurrentDictionary<string, Player>();

        private long gameStartCountdown = START_DELAY;
        private bool _started;

        public Game(int? seed, int? id) : base()
        {
            if (seed != null && seed.HasValue)
            {
                Random = new Random(seed.Value);
            }

            if (id != null && id.HasValue)
            {
                Id = id.Value;
            }
        }

        public Game()
        {
            if (Random == null)
            {
                Random = new Random();
            }

            Id = _MAXID++;
            
            Turn = 0;
            Running = false;
            _started = false;
            _gameLoop = new HighFrequencyTimer(60, this.Update);
        }

        public LogonResult LogonPlayer(string playerName)
        {
            var result = new LogonResult();
            if (!Players.ContainsKey(playerName))
            {
                var newPlayer = new Player()
                {
                    AuthToken = System.Guid.NewGuid().ToString(),
                    PlayerName = playerName
                };

                var success = Players.TryAdd(playerName, newPlayer);
                var success2 = _authTokens.TryAdd(newPlayer.AuthToken, newPlayer);

                if (success && success2)
                {
                    System.Diagnostics.Debug.WriteLine("Player logon [{0}]:[{1}]", newPlayer.PlayerName,
                        newPlayer.AuthToken);
                }

                
                result.AuthToken = newPlayer.AuthToken;
                result.GameId = Id;
                result.GameStart = (int)this.gameStartCountdown;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Player {0} already logged on!", playerName);
            }
            result.GameId = Id;

            return result;
        }

        public void StartDemoAgent(LogonResult demoResult, string playerName)
        {
            var agentTask = Task.Factory.StartNew(() =>
            {
                string endpoint = "";
                if (IsRunningLocally)
                {
                    endpoint = "http://localhost:3193";
                }
                else {
                    endpoint = "http://elevators.azurewebsites.net";
                }
                AgentBase sweetDemoAgent = new AgentBase(playerName, endpoint);
                sweetDemoAgent.Start(demoResult).Wait();
            });
        }

        public void Start()
        {
            Running = true;
            _gameLoop.Start();
        }

        public void Stop()
        {
            Running = false;
            _gameLoop.Stop();
        }

        public void Update(long delta)
        {
            throw new NotImplementedException();
        }
    }
}
