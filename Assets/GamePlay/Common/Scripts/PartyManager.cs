using Core.Scripts.Foundation.Singleton;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

// Common 내의 싱글톤들은, 다른 싱글톤을 제외하고 다른 객체들에서도 참조할 가능성을 가진다.
namespace GamePlay.Features.Scripts.Common
{
    public class PartyManager : SingletonMono<PartyManager>
    {
        public override async UniTask InitAsync()
        {
            await UniTask.Yield();
        }
    }
}