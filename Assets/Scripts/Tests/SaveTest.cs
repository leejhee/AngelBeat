using Core.Foundation.Define;
using Core.Foundation.Utils;
using Core.Managers;
using NUnit.Framework;
using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayMode
{
    public class SaveTest
    {
        int _createdIdx = -1;

        [SetUp]
        public void Setup()
        {
            // 테스트 중에는 SlotLoaded 이벤트를 끈다(맵 리빌드 등 부작용 차단)
            SaveLoadManager.suppressSlotLoadEvent = true;
            SaveLoadManager.Instance.Init();
            Assert.IsNotNull(SaveLoadManager.Instance.GlobalSave);
        }

        [TearDown]
        public void Teardown()
        {
            if (_createdIdx >= 0)
            {
                SaveLoadManager.Instance.DeleteSlot(_createdIdx);
                _createdIdx = -1;
            }
            SaveLoadManager.suppressSlotLoadEvent = false;
        }

        [Test, Timeout(8000)]
        public async Task Sync_Create_Load_Succeed()
        {
            Assert.IsTrue(SaveLoadManager.Instance.CreateNewSlot("UT_S_CreateLoad", out _createdIdx));
            Assert.IsTrue(SaveLoadManager.Instance.LoadSlot(_createdIdx));

            var slot = SaveLoadManager.Instance.CurrentSlot;
            Assert.IsNotNull(slot);
            Assert.AreEqual("UT_S_CreateLoad", slot.slotName);

            await Task.Yield();
        }

        [Test, Timeout(8000)]
        public async Task Sync_SaveByState_Reload_Succeed()
        {
            Assert.IsTrue(SaveLoadManager.Instance.CreateNewSlot("UT_S_SaveByState_Reload", out _createdIdx));
            Assert.IsTrue(SaveLoadManager.Instance.LoadSlot(_createdIdx));
            
            SaveLoadManager.Instance.SaveSlotByState(SystemEnum.GameState.Village);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await SaveLoadManager.Instance.SaveCurrentSlotAsync(SystemEnum.GameState.Village);
            
            Assert.IsTrue(SaveLoadManager.Instance.LoadSlot(_createdIdx));
            Assert.AreEqual(SystemEnum.GameState.Village, SaveLoadManager.Instance.CurrentSlot.lastGameState);
            
        }
        
        [Test, Timeout(8000)]
        public async Task Async_SaveCurrentSlotAsync_Then_LoadAsync_Succeeds()
        {
            Assert.IsTrue(SaveLoadManager.Instance.CreateNewSlot("UT_Async_Save", out _createdIdx));

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            // 비동기 로드/세이브 모두 'await' (UTF가 메인 스레드에서 매 프레임 폴링)
            Assert.IsTrue(await SaveLoadManager.Instance.LoadSlotAsync(_createdIdx, cts.Token));
            await SaveLoadManager.Instance.SaveCurrentSlotAsync(SystemEnum.GameState.Battle);

            Assert.IsTrue(await SaveLoadManager.Instance.LoadSlotAsync(_createdIdx, cts.Token));
            Assert.AreEqual(SystemEnum.GameState.Battle, SaveLoadManager.Instance.CurrentSlot.lastGameState);
        }
        
        [Test, Timeout(8000)]
        public async Task Concurrent_Saves_Are_Serialized_And_Loadable()
        {
            Assert.IsTrue(SaveLoadManager.Instance.CreateNewSlot("UT_Concurrent", out _createdIdx));
            Assert.IsTrue(SaveLoadManager.Instance.LoadSlot(_createdIdx));

            // 같은 경로에 연속 저장 (Keyed 직렬화 확인)
            for (int i = 0; i < 5; i++)
                SaveLoadManager.Instance.SaveSlotByState(SystemEnum.GameState.Explore);

            // 큐가 빌 때까지 대기 (파일 교체 완료 보장)
            await Core.Foundation.Utils.AsyncJobQueue.WaitIdleAsync();

            // 다시 로드해서 마지막 상태가 반영됐는지 확인
            Assert.IsTrue(SaveLoadManager.Instance.LoadSlot(_createdIdx));
            Assert.AreEqual(SystemEnum.GameState.Explore, SaveLoadManager.Instance.CurrentSlot.lastGameState);
        }

        [UnityTest, Timeout(8000)]
        public IEnumerator Load_While_Saving_Does_Not_Block_Or_Throw()
        {
            Assert.IsTrue(SaveLoadManager.Instance.CreateNewSlot("UT_LoadWhileSaving", out _createdIdx));
            Assert.IsTrue(SaveLoadManager.Instance.LoadSlot(_createdIdx));

            // 저장을 몇 번 큐에 넣고
            for (int i = 0; i < 3; i++)
                SaveLoadManager.Instance.SaveSlotByState(SystemEnum.GameState.Village);

            // 저장 중에도 로드가 가능해야 함 (FileShare.Delete로 인해)
            Assert.IsTrue(SaveLoadManager.Instance.LoadSlot(_createdIdx));

            // Idle까지 기다려 마무리
            var waitTask = Core.Foundation.Utils.AsyncJobQueue.WaitIdleAsync();
            while (!waitTask.IsCompleted) { yield return null; }
        }
    }
}
