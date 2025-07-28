using System;
using UnityEngine;

/// <summary>
/// 게임 모드별 전용 로직을 캡슐화하기 위한 전략 인터페이스.
/// </summary>
public interface IGameMode
{
    /// <summary>모드 초기화(씬 로드 직후)</summary>
    void Initialize();
    /// <summary>러닝(스테이지/던전) 시작</summary>
    void StartRun();
    /// <summary>러닝 종료</summary>
    void EndRun(bool victory);
    /// <summary>씬 전환·모드 교체 전에 정리</summary>
    void Cleanup();

    /// <summary>런 종료 이벤트: bool victory</summary>
    event Action<bool> OnRunEnded;

    /// <summary>이 모드에서 사용할 일시정지 메뉴 프리팹</summary>
    GameObject PauseMenuPrefab { get; }

    /// <summary>일시정지 메뉴에 표시할 컨텍스트 데이터를 생성합니다.</summary>
    PauseMenuContext GetPauseMenuContext();
} 