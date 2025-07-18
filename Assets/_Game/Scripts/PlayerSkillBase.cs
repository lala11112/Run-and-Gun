using System.Collections;
using UnityEngine;

/// <summary>
/// 각 스킬(Z,X,C,V)의 공통 베이스.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public abstract class PlayerSkillBase : MonoBehaviour
{
    [Tooltip("해당 스킬 타입")] public SkillType skillType;

    protected PlayerController pc;

    protected virtual void Awake()
    {
        pc = GetComponent<PlayerController>();
    }

    /// <summary>
    /// SkillManager 가 스킬을 발동할 때 호출
    /// weakened: 연속 사용 패널티 여부
    /// </summary>
    public void RequestActivate(bool weakened)
    {
        if (!CanActivate()) return;
        StartCoroutine(Activate(weakened));
    }

    protected virtual bool CanActivate() => true;

    protected abstract IEnumerator Activate(bool weakened);
} 