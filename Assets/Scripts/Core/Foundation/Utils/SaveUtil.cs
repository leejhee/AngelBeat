using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using UnityEngine;

namespace Core.Foundation.Utils
{
    public static class AsyncSaveLoadUtil
    {
        private static readonly Channel<Func<Task>> _saveTaskChannel;
        private static readonly CancellationTokenSource _cts = new();

        static AsyncSaveLoadUtil()
        {
            _saveTaskChannel = Channel.CreateUnbounded<Func<Task>>(
                new UnboundedChannelOptions
                {
                    SingleReader = true,
                    SingleWriter = false
                });

            // 백그라운드에서 계속 작업 소비
            Task.Factory.StartNew(async () =>
            {
                await foreach (var work in _saveTaskChannel.Reader.ReadAllAsync(_cts.Token))
                {
                    try
                    {
                        await work();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[AsyncSaveLoadUtil] 작업 실행 중 오류: {ex}");
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        public static void SaveJsonNewtonsoftAsync<TSave>(TSave dataClass, string fileName = null) where TSave : class
        {
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = typeof(TSave).Name;
                int index = fileName.IndexOf("DataClass", StringComparison.Ordinal);
                if (index != -1)
                {
                    fileName = string.Concat(fileName.Substring(0, index), 's');
                }
            }

            string savePath = GetSavePath();
            string fullPath = Path.Combine(savePath, "userdata", $"{fileName}.json");

            // 채널에 저장 작업 enqueue
            _saveTaskChannel.Writer.TryWrite(async () =>
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? string.Empty);

                    var settings = new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        DateFormatHandling = DateFormatHandling.IsoDateFormat,
                        TypeNameHandling = TypeNameHandling.Auto
                    };

                    string jsonText = JsonConvert.SerializeObject(dataClass, settings);
                    await File.WriteAllTextAsync(fullPath, jsonText, Encoding.UTF8);

                    Debug.Log($"[Async Save Complete]: {fullPath}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Async Save Error] {fullPath} : {ex}");
                }
            });
        }

        public static async Task<TSave> LoadSaveDataNewtonsoftAsync<TSave>(string fileName = null) where TSave : class
        {
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = typeof(TSave).Name;
                int index = fileName.IndexOf("DataClass", StringComparison.Ordinal);
                if (index != -1)
                {
                    fileName = string.Concat(fileName.Substring(0, index), 's');
                }
            }

            string loadPath = GetSavePath();
            string fullPath = Path.Combine(loadPath, "userdata", $"{fileName}.json");

            if (!File.Exists(fullPath))
            {
                Debug.Log($"[Async Load] No save file found: {fullPath}");
                return default;
            }

            try
            {
                string jsonData = await File.ReadAllTextAsync(fullPath, Encoding.UTF8);
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                };

                return JsonConvert.DeserializeObject<TSave>(jsonData, settings);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Async Load Error] {fullPath} : {e}");
                return default;
            }
        }

        private static string GetSavePath()
        {
            return Application.persistentDataPath;
        }

        public static void Shutdown()
        {
            _cts.Cancel();
            _saveTaskChannel.Writer.Complete();
        }
    }
}
