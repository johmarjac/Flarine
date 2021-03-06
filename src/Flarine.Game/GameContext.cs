﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Flarine.Core.Context;
using Flarine.Core.Context.Model;
using Flarine.Core.Logging;
using Flarine.Core.Network.Web;
using Flarine.Database;
using Flarine.Game.Config.Model;
using Flarine.Game.Context.Model;
using Flarine.Network.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebCommon;

namespace Flarine.Game
{
    internal sealed class GameContext : ContextBase
    {
        private const string CONFIG_PATH = "Config/GameConfig.json";
        private const string GAMEDATA_PATH = "Config/GameData";

        public GameContext() : base("GameServer")
        {
            DatabaseService.SetEngine(GameConfig.DatabaseEngine);

            try
            {
                DatabaseService.GetContext().Database.Migrate();
            }
            catch (Exception ex)
            {
                Logger.Get<GameContext>().LogTrace(ex.ToString());
            }

            LoginSessions = new List<LoginSession>();
            GameSessions = new ObservableCollection<GameSession>();
        }

        public override void LoadConfigurations()
        {
            GameConfig = LoadConfiguration<GameConfig>(CONFIG_PATH);
        }

        public override void SaveConfigurations()
        {
            SaveConfiguration(GameConfig, CONFIG_PATH);
        }

        public override void LoadAssets()
        {
            Task.Factory.StartNew(() =>
            {
                if (!Directory.Exists(GAMEDATA_PATH))
                    Logger.Get<GameContext>().LogCritical($"GameData not found, please provide GameData in directory {GAMEDATA_PATH} first.");
                else
                {
                    var startTime = Environment.TickCount;
                    GameDatas = GameDatas.FromPath(GAMEDATA_PATH);
                    CompressedGameDatas = WPDUtil.ZipToBase64(GameDatas.SerializeBase64String());
                    var timeDiff = Environment.TickCount - startTime;

                    Logger.Get<GameContext>().LogInformation($"GameData has been loaded in {timeDiff} ms.");
                }
            });
        }

        internal GameConfig GameConfig { get; private set; }
        internal WPDGameDatas GameDatas { get; private set; }
        internal string CompressedGameDatas { get; private set; }
        internal List<LoginSession> LoginSessions { get; private set; }
        internal ObservableCollection<GameSession> GameSessions { get; private set; }
    }
}