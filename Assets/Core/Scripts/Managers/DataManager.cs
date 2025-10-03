using Core.Scripts.Data;
using Core.Scripts.Foundation.Singleton;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Core.Scripts.Managers
{
    /// <summary> 
/// 데이터 매니저 (Sheet 데이터 관리)
/// </summary>
public partial class DataManager : SingletonObject<DataManager>
{
    /// 로드한 적 있는 DataTable (Table 명을  Key1 데이터 ID를 Key2로 사용)
    Dictionary<string, Dictionary<long, SheetData>> _cache = new();

    #region 생성자
    DataManager() { }
    #endregion

    private UniTaskCompletionSource _readyTCS = new();
    public bool Ready {get; private set;}
    public UniTask WhenReady() => _readyTCS.Task;
    
    
    public async UniTask InitAsync()
    {
        ClearCache();
        await DataLoadAsync();
        Ready = true;
        _readyTCS.TrySetResult();
    }
    
    // TODO : SemaphoreSlim(4)는 현재 magic number이므로 근거를 찾아 재설정할 것
    public async UniTask DataLoadAsync(CancellationToken ct = default)
    {
        await ResourceManager.Instance.InitAsync();
        
        var sheetAsm = typeof(SheetData).Assembly;
        var sheetDataTypes = sheetAsm.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(SheetData))).ToArray();
        var sem = new System.Threading.SemaphoreSlim(4);
        var parallelTasks = new List<UniTask<(string name, Dictionary<long, SheetData> sheet)>>();
        
        foreach (var type in sheetDataTypes)
        {
            var instance = (SheetData)Activator.CreateInstance(type);
            
            parallelTasks.Add(UniTask.Create(async () =>
            {
                await sem.WaitAsync(ct);
                try
                {
                    // 의존을 줄이기 위한 상위에서의 로드
                    var key = $"CSV/MEMCSV/{type.Name}";
                    var textAsset = await ResourceManager.Instance.LoadAsync<TextAsset>(key);
                    var text = textAsset.text;
                    ResourceManager.Instance.Release(key);
                    
                    var dict = await instance.ParseAsync(text, ct);
                    return (type.Name, dict);
                }
                finally { sem.Release(); }
            }));
        }
        
        var results = await UniTask.WhenAll(parallelTasks);
        foreach (var (name, dict) in results)
        {
            _cache.Add(name, dict);
            await SetTypeData(name);
        }
    }

    private async UniTask SetTypeData(string typeName)
    {
        switch (typeName)
        {
            case nameof(KeywordData): 
                SetKeywordDataMap(); 
                return;
            case nameof(DokkaebiData):
                await InitCharacterSpriteMapWithYeon();
                return;
            case nameof(CompanionData):
                await SetCompanionSpriteMap(); 
                return;
            case nameof(MonsterData):
                await SetMonsterSpriteMap();
                return;
            case nameof(DokkaebiSkillData):
                await SetDokkaebiSkillIconSpriteMap();
                return;
            case nameof(SkillData):
                SetCharacterSkillMap();
                //await SetNormalSkillIconSpriteMap();
                return;
            case nameof(SkillRangeData):
                SetSkillRangeMap();
                return;
        }
        
        
    }

    public T GetData<T>(long Index) where T : SheetData
    {
        string key = typeof(T).ToString();
        key = key.Replace("Core.Scripts.Data.", "");
        if (!_cache.ContainsKey(key))
        {
            Debug.LogError($"{key} 데이터 테이블은 존재하지 않습니다.");
            return null;
        }
        if (!_cache[key].ContainsKey(Index))
        {
            Debug.LogError($"{key} 데이터에 ID {Index}는 존재하지 않습니다.");
            return null;
        }
        T returnData = _cache[key][Index] as T;
        if (returnData == null)
        {
            Debug.LogError($"{key} 데이터에 ID {Index}는 존재하지만 {key}타입으로 변환 실패했습니다.");
            return null;

        }

        return returnData;
    }

    public void SetData<T>(int id, T data) where T : SheetData
    {
        string key = typeof(T).ToString();
        key = key.Replace("Core.Data.", "");

        if (_cache.ContainsKey(key))
        {
            Debug.LogWarning($"{key} 데이터 테이블은 이미 존재합니다.");
        }
        else
        {
            _cache.Add(key, new Dictionary<long, SheetData>());
        }

        if (_cache[key].ContainsKey(id))
        {
            Debug.LogWarning($"{key} 타입 ID: {id} 칼럼은 이미 존재합니다. !(주의) 게임 중 데이터 칼럼을 변경할 수 없습니다!");
        }
        else
        {
            _cache[key].Add(id, data);
        }
    }
    
    public List<SheetData> GetDataList<T>() where T : SheetData
    {
        string typeName = typeof(T).Name;
        if (_cache.ContainsKey(typeName) == false)
        {
            Debug.LogWarning($"DataManager : {typeName} 타입 데이터가 존재하지 않습니다.");

            return null;
        }

        return _cache[typeName].Values.ToList();
    }
    
    public void ClearCache()
    {
        _cache.Clear();
    }
    
#if UNITY_EDITOR
    // 데이터 검증용(에디터에서만 사용)
    public Dictionary<long, SheetData> GetDictionary(string typeName)
    {
        if (_cache.ContainsKey(typeName) == false)
        {
            Debug.LogWarning($"DataManager : {typeName} 타입 데이터가 존재하지 않습니다.");
            return null;
        }
        return _cache[typeName];
    }
#endif


}

}
