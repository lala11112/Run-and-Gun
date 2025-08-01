using UnityEngine;

/// <summary>
/// 모든 구체적인 스킬 로직 클래스들의 부모가 될 추상 클래스입니다.
/// </summary>
public abstract class SkillBase : MonoBehaviour
{
    /// <summary>
    /// 스킬을 발동시키는 핵심 로직입니다.
    /// 이제 각 스킬 로직은 자기 자신의 데이터를 직접 참조합니다.
    /// </summary>
    /// <param name="caster">스킬을 시전한 게임 오브젝트 (플레이어)</param>
    /// <param name="currentRank">스킬 발동 시점의 현재 스타일 랭크</param>
    public abstract void Activate(GameObject caster, StyleRank currentRank);
}

// 참고: 기존에 있던 다른 데이터 컨테이너들은 SkillDataSO.cs 파일로 이동하여 관리합니다.
