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
                string mainPath = game.InstallDirectory ?? string.Empty;
                string actionType = "Unknown";
                // Prüfen, ob das Spiel ROMs hat
                if (game.Roms != null && game.Roms.Count > 0)
                {
                    mainPath = game.Roms[0].Path;

                    // {InstallDir} im ROM-Pfad ersetzen
                    if (!string.IsNullOrEmpty(game.InstallDirectory))
                    {
                        var relativePath = mainPath.Replace("{InstallDir}", "").TrimStart('\\', '/');
                        mainPath = Path.Combine(game.InstallDirectory, relativePath);
                    }

                    actionType = "Emulator";
                }
                else if (game.GameActions != null && game.GameActions.Count > 0)
                {
                    foreach (var action in game.GameActions)
                    {
                        // Aktionstyp bestimmen
                        switch (action.Type)
                        {
                            case GameActionType.Emulator:
                                actionType = "Emulator";
                                break;
                            case GameActionType.File:
                                actionType = "File";
                                break;
                            case GameActionType.Script:
                                actionType = "Script";
                                break;
                            case GameActionType.URL:
                                actionType = "URL";
                                break;
                        }

                        // Pfad übernehmen
                        if (!string.IsNullOrEmpty(action.Path))
                        {
                            mainPath = action.Path;

                            // {InstallDir} ersetzen
                            if (!string.IsNullOrEmpty(game.InstallDirectory) && mainPath.Contains("{InstallDir}"))
                            {
                                var relativePath = mainPath.Replace("{InstallDir}", "").TrimStart('\\', '/');
                                mainPath = Path.Combine(game.InstallDirectory, relativePath);
                            }
                            break;
                        }
                    }
                }

                var data = new
                {
                    Event = eventType,
                    Name = game.Name,
                    Source = game.Source?.Name,
                    Type = actionType,
                    Path = mainPath
                };

                var json = JsonConvert.SerializeObject(data, Formatting.Indented);
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
