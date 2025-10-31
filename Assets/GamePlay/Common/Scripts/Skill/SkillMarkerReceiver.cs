using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Timeline.Marker;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Playables;

namespace GamePlay.Common.Scripts.Skill
{
    /// <summary>
    /// Timeline의 Marker들이 발생시키는 비동기 작업을 추적하고 완료를 보장하는 클래스
    /// SkillBase에 필수로 붙어야 하는 컴포넌트
    /// </summary>
    [RequireComponent(typeof(PlayableDirector))]
    public class SkillMarkerReceiver : MonoBehaviour, INotificationReceiver
    {
        private SkillParameter _input;
        private List<UniTask> _pending = new();
        private UniTaskCompletionSource _allDone;
        private CancellationToken _ct;
        private int _markerCount = 0;
        
        /// <summary>
        /// 현재 실행 중인 스킬의 파라미터
        /// </summary>
        public SkillParameter Input => _input;
        
        /// <summary>
        /// 모든 Marker 작업이 완료될 때까지 대기. SkillBase.PlaySkillAsync에서 이 속성을 await함
        /// </summary>
        public UniTask Completion => _allDone?.Task ?? UniTask.CompletedTask;

        /// <summary>
        /// 스킬 재생 시작 시 호출 (SkillBase.PlaySkillAsync에서)
        /// </summary>
        public void Begin(SkillParameter input, CancellationToken ct)
        {
            _input = input;
            _ct = ct;
            _pending.Clear();
            _allDone = new UniTaskCompletionSource();
            _markerCount = 0;
            
            Debug.Log($"[SkillMarkerReceiver] Begin - Skill: {input.Model.SkillIndex}");
        }

        /// <summary>
        /// Timeline에서 Marker가 발동될 때 Unity가 자동으로 호출 (INotificationReceiver 인터페이스)
        /// </summary>
        public void OnNotify(Playable origin, INotification notification, object context)
        {
            // SkillTimeLineMarker가 아니면 무시
            if (notification is not SkillTimeLineMarker skillMarker) return;
            
            _markerCount++;
            string markerName = skillMarker.GetType().Name;
            
            try
            {
                // 1. Marker에 파라미터 전달
                skillMarker.InitInput(Input);
                
                // 2. Marker의 비동기 작업 시작
                UniTask task = skillMarker.BuildTaskAsync(_ct);
                
                // 3. 작업이 즉시 완료된 것이 아니라면 추적 리스트에 추가
                if (task.Status == UniTaskStatus.Pending)
                {
                    _pending.Add(WrapTask(task, markerName));
                    Debug.Log($"[SkillMarkerReceiver] Marker Task Added [{_pending.Count}]: {markerName}");
                }
                else
                {
                    // 즉시 완료된 경우 (동기 작업)
                    Debug.Log($"[SkillMarkerReceiver] Marker Completed Immediately: {markerName}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SkillMarkerReceiver] Marker Init Failed: {markerName}\n{e}");
            }
        }

        /// <summary>
        /// Marker 작업을 래핑하여 예외 처리 및 로깅 추가
        /// </summary>
        private async UniTask WrapTask(UniTask task, string markerName)
        {
            try
            {
                await task;
                Debug.Log($"[SkillMarkerReceiver] Marker Completed: {markerName}");
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"[SkillMarkerReceiver] Marker Cancelled: {markerName}");
                throw; // 취소는 정상 흐름이므로 재throw
            }
            catch (Exception e)
            {
                Debug.LogError($"[SkillMarkerReceiver] Marker Error: {markerName}\n{e}");
                throw; // 예외 재throw하여 WhenAll에서 잡히도록
            }
        }

        /// <summary>
        /// Timeline 재생 완료 시 호출 (SkillBase.PlaySkillAsync에서)
        /// 모든 Marker 작업이 완료될 때까지 대기 시작
        /// </summary>
        public void End()
        {
            Debug.Log($"[SkillMarkerReceiver] End - Total Markers: {_markerCount}, Pending: {_pending.Count}");
            CompleteAsync().Forget();
        }

        /// <summary>
        /// 모든 Marker 작업이 완료될 때까지 대기하는 내부 메서드
        /// </summary>
        private async UniTaskVoid CompleteAsync()
        {
            try
            {
                if (_pending.Count > 0)
                {
                    Debug.Log($"[SkillMarkerReceiver] Waiting for {_pending.Count} tasks...");
                    await UniTask.WhenAll(_pending);
                }
                
                _allDone?.TrySetResult();
                Debug.Log("[SkillMarkerReceiver] All tasks completed successfully");
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[SkillMarkerReceiver] All tasks cancelled");
                _allDone?.TrySetCanceled(_ct);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SkillMarkerReceiver] Task execution failed:\n{e}");
                _allDone?.TrySetException(e);
            }
            finally
            {
                // 정리
                _pending.Clear();
                _input = null;
                _markerCount = 0;
            }
        }

        /// <summary>
        /// 컴포넌트 파괴 시 정리
        /// </summary>
        private void OnDestroy()
        {
            _allDone?.TrySetCanceled();
            _pending.Clear();
        }

        /// <summary>
        /// 디버깅용: 현재 상태 정보
        /// </summary>
        public string GetDebugInfo()
        {
            return $"Markers: {_markerCount}, Pending: {_pending.Count}, Input: {_input != null}";
        }
    }
}