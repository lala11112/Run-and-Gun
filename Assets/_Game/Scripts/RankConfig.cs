using UnityEngine;

/// <summary>
/// 스타일 랭크에 대한 임계값과 버프/패널티 설정을 묶은 ScriptableObject.
/// StyleManager가 로드하여 사용한다.
/// </summary>
[CreateAssetMenu(fileName = "RankConfig", menuName = "Game/Rank Config")]
public class RankConfig : ScriptableObject
{
    [Header("랭크 임계값")] public int dThreshold = 50;
    public int cThreshold = 100;
    public int bThreshold = 200;
    public int aThreshold = 350;
    public int sThreshold = 600;

    [Header("쿨타임 배수")] public float dCooldownMultiplier = 1.2f;
    public float bCooldownMultiplier = 0.9f;
    public float aCooldownMultiplier = 0.7f;
    public float sCooldownMultiplier = 0.5f;

    [Header("이동 속도 배수")] public float dMoveSpeedMultiplier = 0.8f;
    public float bMoveSpeedMultiplier = 1.1f;
    public float aMoveSpeedMultiplier = 1.25f;
    public float sMoveSpeedMultiplier = 1.4f;

    [Header("S 랭크 설정")] public float sDuration = 8f;
    public int sExitScore = 400;
} 