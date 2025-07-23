using System;
using UnityEngine;

/// <summary>
/// 체력을 보유하는 모든 생명체가 공통으로 사용할 추상 클래스입니다.
/// 최대 체력, 피해 처리, 사망 처리 등 기본 로직을 제공합니다.
/// 파생 클래스는 <see cref="Die"/> 를 구현해 개별 연출을 처리하세요.
/// </summary>
public abstract class LivingEntity : MonoBehaviour
{
    [Header("공통 체력 설정")]
    [Tooltip("최대 체력")] public int maxHealth = 5;

    /// <summary>현재 체력. 인스펙터에서 확인만 가능합니다.</summary>
    [Tooltip("현재 체력(읽기 전용)")] public int currentHealth;

    /// <summary>현재 체력 접근용 프로퍼티</summary>
    public int CurrentHealth => currentHealth;

    /// <summary>체력 변화 이벤트 (현재, 최대)</summary>
    public Action<int, int> OnHealthChanged;

    /// <summary>
    /// 파생 클래스에서 base.Awake() 호출 필수! (currentHealth 초기화)
    /// </summary>
    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    /// <summary>
    /// 피해 처리 메서드. 파생 클래스에서 확장하려면 base.TakeDamage 호출 후 추가 로직을 작성하세요.
    /// </summary>
    public virtual void TakeDamage(int dmg)
    {
        currentHealth -= dmg;
        if (currentHealth < 0) currentHealth = 0;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 사망 처리. 구체적인 연출은 파생 클래스에서 구현합니다.
    /// </summary>
    protected abstract void Die();
}

public class GenericLivingEntity : LivingEntity
{
    [Header("사망 시 Destroy 여부")]
    public bool destroyOnDeath = true;

    protected override void Die()
    {
        // 추가 연출이 필요하면 여기서 호출
        if (destroyOnDeath) Destroy(gameObject);
    }
} 