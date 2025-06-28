using System;
using UnityEngine;

/// <summary>
/// 스킬 타입을 정의하는 열거형
/// Q, W, E, R 네 가지 스킬을 구분
/// </summary>
public enum SkillType { Z, X, C, V }

/// <summary>
/// 개별 스킬의 기본 설정 데이터를 담는 직렬화 가능한 클래스
/// Inspector에서 스킬별 설정을 편집할 수 있음
/// </summary>
[Serializable]
public class SkillData
{
    [Header("Basic Settings")]
    [Tooltip("스킬 타입 (Z, X, C, V 중 하나)")]
    public SkillType type;
    
    [Tooltip("스킬의 기본 쿨타임 (초) - 연속 사용 시 패널티로 증가함")]
    public float baseCooldown = 3f;
    
    [Header("Visual Effects")]
    [Tooltip("스킬 사용 시 생성할 프리팹 또는 이펙트 (선택사항)")]
    public GameObject skillPrefab;
} 