using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Core.GameSave.IO
{
    /// <summary>
    /// 슬롯 데이터 저장 및 로드 시의 비동기 작업을 담당하는 정적 클래스
    /// </summary>
    public static class SlotIO
    {
        /// <summary>
        /// 정돈해주고, 추상타입 받으며, 국제표준형식 시간표현
        /// </summary>
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto,
            DateFormatHandling = DateFormatHandling.IsoDateFormat
        };
        
        /// <summary>
        /// 파일명만으로 userData에 저장 가능하도록 함.
        /// </summary>
        /// <param name="pathOrFileName">파일 이름만 넣어도 되도록 한다.</param>
        /// <returns>userdata 이하의 경로 반환</returns>
        private static string GetFullPath(string pathOrFileName)
        {
            if (Path.IsPathRooted(pathOrFileName)) return pathOrFileName;
            string file = Path.HasExtension(pathOrFileName) ? pathOrFileName : pathOrFileName + ".json";
            return Path.Combine(Application.persistentDataPath, "userdata", file);
        }
        
        public static async Task<GameSlotData> LoadAsync(string path, CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                string json = File.ReadAllText(GetFullPath(path), Encoding.UTF8);
                return JsonConvert.DeserializeObject<GameSlotData>(json, _jsonSettings);
            }, ct);
        }

        public static async Task SaveAsync(string path, GameSlotData slot, CancellationToken ct)
        {
            string dir = Path.GetDirectoryName(GetFullPath(path));
            Directory.CreateDirectory(dir!);
            string tmp = Path.Combine(dir!, "slot.tmp");
            string json = JsonConvert.SerializeObject(slot, _jsonSettings);
            
            await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                using FileStream fs = new (tmp, FileMode.Create, FileAccess.Write, FileShare.None);
                using StreamWriter sw = new (fs, Encoding.UTF8);
                sw.Write(json);
                sw.Flush();
                fs.Flush(true);
            }, ct);
        }
    }
}