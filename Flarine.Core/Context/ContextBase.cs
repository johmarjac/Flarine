﻿using System;
using System.IO;
using Flarine.Core.Config;
using Newtonsoft.Json;

namespace Flarine.Core.Context
{
    public abstract class ContextBase : IDisposable
    {
        private static ContextBase instance;
        public static T GetInstance<T>() where T : ContextBase, new()
        {
            if (instance == null)
                instance = new T();
            return (T)instance;
        }

        public ContextBase()
        {
            LoadConfigurations();
        }
        
        public abstract void LoadConfigurations();
        public abstract void SaveConfigurations();

        protected TConfig LoadConfiguration<TConfig>(string file) where TConfig : ContextConfiguration, new()
        {
            var directory = Path.GetDirectoryName(file);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            if (!File.Exists(file))
                return new TConfig();

            return JsonConvert.DeserializeObject<TConfig>(File.ReadAllText(file));
        }

        protected void SaveConfiguration<TConfig>(TConfig config, string file) where TConfig : ContextConfiguration, new()
        {
            var directory = Path.GetDirectoryName(file);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(file, JsonConvert.SerializeObject(config, Formatting.Indented));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                SaveConfigurations();
            }
        }
    }
}