using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace novel
{
    public class NovelResources
    {
        public NovelAudioData audio { get; private set; }
        public NovelVariableData variable { get; private set; }
        public NovelBackgroundData background { get; private set; }
        public NovelScriptData script { get; private set; }
        public NovelCharacterData character { get; private set; }

        private const string SettingsLabel = "NovelSettingData";
        private AsyncOperationHandle<IList<ScriptableObject>> _handleAll;

        public async Task InitByLabelAsync()
        {
            if (_handleAll.IsValid()) return;


            _handleAll = Addressables.LoadAssetsAsync<ScriptableObject>(SettingsLabel, null);
            var list = await _handleAll.Task;

            foreach (var so in list)
            {
                switch (so)
                {
                    case NovelAudioData a: audio = a; break;
                    case NovelVariableData v: variable = v; break;
                    case NovelBackgroundData b: background = b; break;
                    case NovelScriptData s: script = s; break;
                    case NovelCharacterData c: character = c; break;
                }
            }

        }
        public void OnNovelEnd()
        {
            if (_handleAll.IsValid())
            {
                Addressables.Release(_handleAll);
                _handleAll = default;
            }

            audio = null;
            variable = null;
            background = null;
            script = null;
            character = null;
        }

    }
}