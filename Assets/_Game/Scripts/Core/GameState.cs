using UnityEngine;

/// <summary>
/// 게임 전체 상태를 나타내는 열거형입니다.
/// </summary>
public enum GameState
{
    Boot,       // 게임 실행 직후 초기화 단계
    Title,      // 타이틀/메뉴 화면
    InGame,     // 실제 플레이 중
    Paused,     // 일시정지
    GameOver    // 게임 오버 또는 클리어 결과 화면
} 