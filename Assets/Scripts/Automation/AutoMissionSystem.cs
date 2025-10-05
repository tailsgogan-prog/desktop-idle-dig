using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DesktopIdleDig.Automation
{
    /// <summary>
    /// Runtime controller for Progress Quest 스타일 자동 임무 루프의 기본 동작을 담당합니다.
    /// - 미션 풀에서 임무를 선택하고, 타이머를 진행하며, 결과 로그를 축적합니다.
    /// - 오프라인 경과 시간을 일괄 계산하는 기본 API를 제공합니다.
    /// - UI와 상호작용하기 위한 로그 이벤트(UnityEvent)를 노출합니다.
    /// </summary>
    public class AutoMissionSystem : MonoBehaviour
    {
        [Serializable]
        public class MissionReward
        {
            [Tooltip("보상을 식별하기 위한 리소스 키(예: currency.soft, item.core 등).")]
            public string rewardId = string.Empty;

            [Min(0)]
            [Tooltip("지급 수량. 단순 정수로 관리하고, 실제 보상 지급 로직은 외부에서 처리합니다.")]
            public int amount = 1;
        }

        [Serializable]
        public class MissionDefinition
        {
            [Tooltip("임무 식별자. 저장/불러오기, 오프라인 처리 시 참조됩니다.")]
            public string missionId = string.Empty;

            [Tooltip("UI에서 사용할 표시 이름.")]
            public string displayName = "새 임무";

            [Min(1f)]
            [Tooltip("임무 수행에 필요한 시간(초).")]
            public float durationSeconds = 30f;

            [Range(0f, 1f)]
            [Tooltip("성공 확률. 부분 성공 및 실패 확률과 합이 1을 넘지 않도록 구성합니다.")]
            public float successChance = 0.6f;

            [Range(0f, 1f)]
            [Tooltip("부분 성공 확률. 남은 확률은 실패로 간주합니다.")]
            public float partialSuccessChance = 0.3f;

            [Tooltip("기본 난이도나 필요 자원 같은 메타 정보를 문자열로 저장합니다.")]
            public string flavorTag = "NORMAL";

            [Tooltip("성공 시 지급되는 보상 목록.")]
            public List<MissionReward> rewards = new();
        }

        public enum MissionOutcome
        {
            Success,
            PartialSuccess,
            Failure
        }

        public enum MissionLogType
        {
            Info,
            Outcome,
            Warning
        }

        [Serializable]
        public class MissionLogEntry
        {
            public string missionId = string.Empty;
            public string message = string.Empty;
            public MissionLogType type = MissionLogType.Info;
            public MissionOutcome? outcome;
            public double totalElapsedSeconds;
            public bool generatedOffline;
        }

        [Serializable]
        public class MissionLogEvent : UnityEvent<MissionLogEntry> { }

        private class MissionRuntimeState
        {
            public MissionDefinition Definition { get; }
            public double ElapsedSeconds { get; set; }

            public MissionRuntimeState(MissionDefinition definition)
            {
                Definition = definition;
                ElapsedSeconds = 0d;
            }
        }

        [Header("임무 풀 구성")]
        [SerializeField]
        private List<MissionDefinition> missionPool = new();

        [Header("랜덤 시드 설정")]
        [SerializeField]
        private int randomSeed = 0;

        [Header("진행 로그")]
        public MissionLogEvent onLogEntry = new();

        private readonly List<MissionLogEntry> _logEntries = new();
        private System.Random _random;
        private MissionRuntimeState _currentMission;
        private double _totalElapsedSeconds;

        public IReadOnlyList<MissionLogEntry> LogEntries => _logEntries;
        public MissionDefinition ActiveMission => _currentMission?.Definition;
        public double ActiveMissionElapsedSeconds => _currentMission?.ElapsedSeconds ?? 0d;

        private void Awake()
        {
            _random = randomSeed != 0 ? new System.Random(randomSeed) : new System.Random();
        }

        private void Start()
        {
            TryStartNextMission();
        }

        private void Update()
        {
            if (_currentMission == null)
            {
                return;
            }

            AdvanceMission(Time.deltaTime, generatedOffline: false);
        }

        /// <summary>
        /// 오프라인에서 경과한 시간을 시뮬레이션합니다.
        /// 예: 앱 재접속 시 마지막 저장 이후의 경과 초를 입력합니다.
        /// </summary>
        public void SimulateOfflineProgress(double elapsedSeconds)
        {
            if (elapsedSeconds <= 0d)
            {
                return;
            }

            double remaining = elapsedSeconds;
            while (remaining > 0d)
            {
                if (_currentMission == null)
                {
                    if (!TryStartNextMission(generatedOffline: true))
                    {
                        break;
                    }
                }

                if (_currentMission == null)
                {
                    break;
                }

                double timeNeeded = _currentMission.Definition.durationSeconds - _currentMission.ElapsedSeconds;
                double step = Math.Min(timeNeeded, remaining);
                AdvanceMission(step, generatedOffline: true);
                remaining -= step;
            }
        }

        /// <summary>
        /// 현재 진행 중인 임무를 취소하고 다음 임무를 강제로 시작합니다.
        /// </summary>
        public void SkipToNextMission()
        {
            if (_currentMission != null)
            {
                PushLog(new MissionLogEntry
                {
                    missionId = _currentMission.Definition.missionId,
                    message = $"임무 '{_currentMission.Definition.displayName}'이(가) 조기 종료되었습니다.",
                    type = MissionLogType.Warning,
                    totalElapsedSeconds = _totalElapsedSeconds,
                    generatedOffline = false
                });
            }

            _currentMission = null;
            TryStartNextMission();
        }

        /// <summary>
        /// 외부에서 특정 임무를 시작해야 할 때 호출합니다.
        /// missionId가 풀에 존재하지 않으면 false를 반환합니다.
        /// </summary>
        public bool ForceStartMission(string missionId)
        {
            MissionDefinition definition = missionPool.Find(m => m.missionId == missionId);
            if (definition == null)
            {
                return false;
            }

            EnsureMissionId(definition);
            _currentMission = new MissionRuntimeState(definition);
            PushLog(new MissionLogEntry
            {
                missionId = definition.missionId,
                message = $"임무 '{definition.displayName}' 시작",
                type = MissionLogType.Info,
                totalElapsedSeconds = _totalElapsedSeconds,
                generatedOffline = false
            });

            return true;
        }

        private void AdvanceMission(double deltaSeconds, bool generatedOffline)
        {
            if (_currentMission == null)
            {
                return;
            }

            _currentMission.ElapsedSeconds += deltaSeconds;
            _totalElapsedSeconds += deltaSeconds;

            if (_currentMission.ElapsedSeconds + 0.0001d < _currentMission.Definition.durationSeconds)
            {
                return;
            }

            ResolveCurrentMission(generatedOffline);
        }

        private bool TryStartNextMission(bool generatedOffline = false)
        {
            if (missionPool.Count == 0)
            {
                return false;
            }

            MissionDefinition selected = missionPool[_random.Next(missionPool.Count)];
            EnsureMissionId(selected);
            _currentMission = new MissionRuntimeState(selected);
            PushLog(new MissionLogEntry
            {
                missionId = selected.missionId,
                message = $"임무 '{selected.displayName}' 시작",
                type = MissionLogType.Info,
                totalElapsedSeconds = _totalElapsedSeconds,
                generatedOffline = generatedOffline
            });

            return true;
        }

        private void ResolveCurrentMission(bool generatedOffline)
        {
            if (_currentMission == null)
            {
                return;
            }

            MissionDefinition definition = _currentMission.Definition;
            MissionOutcome outcome = RollOutcome(definition);

            string message = outcome switch
            {
                MissionOutcome.Success => $"임무 '{definition.displayName}' 성공",
                MissionOutcome.PartialSuccess => $"임무 '{definition.displayName}' 부분 성공",
                _ => $"임무 '{definition.displayName}' 실패"
            };

            PushLog(new MissionLogEntry
            {
                missionId = definition.missionId,
                message = message,
                type = MissionLogType.Outcome,
                outcome = outcome,
                totalElapsedSeconds = _totalElapsedSeconds,
                generatedOffline = generatedOffline
            });

            _currentMission = null;
            TryStartNextMission(generatedOffline);
        }

        private MissionOutcome RollOutcome(MissionDefinition definition)
        {
            float roll = (float)_random.NextDouble();
            if (roll <= definition.successChance)
            {
                return MissionOutcome.Success;
            }

            if (roll <= definition.successChance + definition.partialSuccessChance)
            {
                return MissionOutcome.PartialSuccess;
            }

            return MissionOutcome.Failure;
        }

        private void PushLog(MissionLogEntry entry)
        {
            _logEntries.Add(entry);
            onLogEntry?.Invoke(entry);
        }

        private static void EnsureMissionId(MissionDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(definition.missionId))
            {
                definition.missionId = Guid.NewGuid().ToString();
            }
        }
    }
}
