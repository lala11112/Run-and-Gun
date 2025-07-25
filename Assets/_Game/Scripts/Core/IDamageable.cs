using UnityEngine;

/// <summary>
/// 객체가 피해를 받을 수 있음을 나타내는 최소 인터페이스.
/// </summary>
public interface IDamageable
{
    void TakeDamage(int dmg);
} 