using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.IO;
using Newtonsoft.Json;

namespace RunningGameToJson
{
    public class RunningGameToJson : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        public override Guid Id { get; } = Guid.Parse("7ad03f78-50f9-40d5-a7a3-cac5776e5482");

        private readonly string jsonPath;

        public RunningGameToJson(IPlayniteAPI api) : base(api)
        {
            jsonPath = Path.Combine(api.Paths.ConfigurationPath, "RunningGame.json");
            Properties = new GenericPluginProperties { HasSettings = false };
        }

        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            var game = args.Game;
            WriteGameToJson(game, "GameStarted");
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            WriteGameToJson(args.Game, "GameStopped");
        }

        private void WriteGameToJson(Game game, string eventType)
        {
            try
            {
                var data = new
                {
                    Event = eventType,
                    Name = game.Name,
                    Source = game.Source?.Name

                };

                var json = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(jsonPath, json);

                logger.Info($"RunningGameToJson: {eventType} - {game.Name} -> {jsonPath}");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Fehler beim Schreiben der JSON-Datei.");
            }
        }
    }
}
