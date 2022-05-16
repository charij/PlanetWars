﻿namespace PlanetWars.Client
{
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using PlanetWars.Common.Data;
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;

    public class Program
    {
        public static void Main()
        {
            try
            {
                Run().Wait();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }

        public static async Task Run()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var endpoint = config.GetValue<string>("Client:ServerUri");
            var gameId = config.GetValue<string>("Client:LobbyId");
            var gameSettings = config.GetSection("LobbySettings").Get<Settings>();
            var profile = config.GetSection("Profile").Get<Player>();
            var isRunning = true;

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                isRunning = false;
            };

            using var httpClient = new HttpClient()
            { 
                BaseAddress = new Uri(endpoint), 
                Timeout = new TimeSpan(0, 0, 30)
            };

            Console.Write($"Logging onto game server {endpoint}...");

            var logonRequest = new HttpRequestMessage { Method = HttpMethod.Post, RequestUri = new Uri(@"\logon"), Content = JsonContent.Create(profile) };
            var logonResponse = await httpClient.SendAsync(logonRequest);
            if (logonResponse .StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine($"...logged on as {profile.Id} : {profile.Name}");

                while (isRunning)
                {
                    Console.Write("Requesting game...");

                    var gameRequest = new HttpRequestMessage { Method = HttpMethod.Post, RequestUri = new Uri(@$"\join?gameId={gameId}"), Content = JsonContent.Create(gameSettings) };
                    var gameResponse = await httpClient.SendAsync(gameRequest);
                    if (gameResponse.StatusCode == HttpStatusCode.OK)
                    {
                        Console.WriteLine($"...game Found!");

                        var rawGame = await gameResponse.Content.ReadAsStringAsync();
                        var game = JsonConvert.DeserializeObject<GameDetails>(rawGame);
                        var bot = new Bot(profile, game);

                        while (!game.IsGameOver)
                        {
                            Console.WriteLine($"\n\tGame {game.Id} Turn {game.History.Count - 1}\n");

                            var commands = bot.CalculateMovesForTurn();
                            var updateRequest = new HttpRequestMessage { Method = HttpMethod.Post, RequestUri = new Uri(@$"\submit\{game.Id}"), Content = JsonContent.Create(commands) };
                            var updateResponse = await httpClient.SendAsync(updateRequest);
                            if (updateResponse.StatusCode != HttpStatusCode.OK)
                            {
                                Console.WriteLine($"\tError(s) occurred with submitted commands:\n{updateResponse.ReasonPhrase}");
                            }

                            var rawUpdate = await updateResponse.Content.ReadAsStringAsync();
                            var update = JsonConvert.DeserializeObject<State>(rawUpdate);
                            game.History.Add(update);
                        }
                    }
                    else
                    {
                        Console.WriteLine("...timed out, no match found!");
                    }
                }
            }
            else
            {
                Console.Error.WriteLine($"{logonResponse.StatusCode} Failed to Logon: {logonResponse.ReasonPhrase}");
            }
        }
    }
}