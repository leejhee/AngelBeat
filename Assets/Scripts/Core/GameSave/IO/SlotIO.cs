using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Core.GameSave.IO
{
    public static class SlotIO
    {
        public static async Task<GameSlotData> LoadAsync(string path, CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                var json = System.IO.File.ReadAllText(path);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<GameSlotData>(json);
            }, ct);
        }

        public static async Task SaveAsync(string path, GameSlotData slot, CancellationToken ct)
        {
            var json = JsonConvert.SerializeObject(slot, Formatting.Indented);
            var dir = System.IO.Path.GetDirectoryName(path);
            var tmp = System.IO.Path.Combine(dir, "slot.tmp");

            await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                System.IO.File.WriteAllText(tmp, json);
                // 원자적 교체(플랫폼별로 Replace가 안되면 Move로 대체)
                if (System.IO.File.Exists(path)) System.IO.File.Replace(tmp, path, null);
                else System.IO.File.Move(tmp, path);
            }, ct);
        }
    }
}