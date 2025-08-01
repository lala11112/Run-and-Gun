using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// V 스킬(점프-슬램 & 다방향 투사체)의 행동을 실제로 구현하는 전용 로직 클래스입니다.
/// </summary>
public class VSkillLogic : SkillBase
{
    /// <summary>
    /// VSkill의 랭크별 보너스 데이터만 담는 전용 클래스입니다.
    /// </summary>
    [System.Serializable]
    public class VRankBonus
    {
        public StyleRank rank;
        [Tooltip("투사체 데미지에 곱해질 배율입니다.")]
        public float damageMultiplier = 1f;
        [Tooltip("발사 방향 개수에 곱해질 배율입니다.")]
        public float countMultiplier = 1f;
    }


    [Header("V-Skill 고유 데이터")]
    [Tooltip("점프 후 내리찍는 행동에 대한 상세 데이터입니다.")]
    public JumpSlamData jumpSlamData;
    [Tooltip("여러 방향으로 동시에 투사체를 발사하는 행동에 대한 상세 데이터입니다.")]
    public MultiShotData multiShotData;
    [Tooltip("투사체의 기본 데미지입니다.")]
    public float projectileDamage = 10f;
    
    [Header("랭크별 성장 정보")]
    [Tooltip("V스킬의 랭크별 성능 변화 목록입니다.")]
    public List<VRankBonus> rankBonuses = new List<VRankBonus>();

    private bool _isRunning;

    public override void Activate(GameObject caster, StyleRank currentRank)
    {
        if (_isRunning) return;
        StartCoroutine(SkillRoutine(caster, currentRank));
    }

    private IEnumerator SkillRoutine(GameObject caster, StyleRank currentRank)
    {
        _isRunning = true;
        
        // 1. 점프-슬램 연출
        if (jumpSlamData != null)
        {
            // TODO: PlayerController를 통해 무적 처리 필요
            
            Sequence jumpSeq = DOTween.Sequence();
            jumpSeq.Append(caster.transform.DOBlendableMoveBy(Vector3.up * jumpSlamData.jumpHeight, jumpSlamData.airTime * 0.5f).SetEase(Ease.OutQuad));
            jumpSeq.Append(caster.transform.DOBlendableMoveBy(Vector3.down * jumpSlamData.jumpHeight, jumpSlamData.airTime * 0.5f).SetEase(Ease.InQuad));
            yield return jumpSeq.WaitForCompletion();

            if(!string.IsNullOrEmpty(jumpSlamData.slamShakePreset)) CameraManager.Instance?.ShakeWithPreset(jumpSlamData.slamShakePreset);
            if(jumpSlamData.landingEffectPrefab != null) Instantiate(jumpSlamData.landingEffectPrefab, caster.transform.position, Quaternion.identity);
        }

        // 2. 다방향 투사체 발사
        if (multiShotData != null)
        {
            var rankBonus = rankBonuses.FirstOrDefault(b => b.rank == currentRank) ?? new VRankBonus();
            
            float finalDamage = projectileDamage * rankBonus.damageMultiplier;
            int finalDirections = Mathf.RoundToInt(multiShotData.directions * rankBonus.countMultiplier);
            
            Vector2[] dirVectors = GetDirections(finalDirections, caster.transform);

            foreach (var dir in dirVectors)
            {
                Quaternion rot = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f);
                GameObject proj = Instantiate(multiShotData.projectilePrefab, caster.transform.position, rot);
                
                if(proj.TryGetComponent<Projectile>(out var p))
                {
                    p.damage = (int)finalDamage;
                    // p.speed = ... // 투사체 속도 설정이 필요하다면 MultiShotData에 speed 필드 추가 필요
                    p.Init(dir);
                }
            }
        }

        _isRunning = false;
    }

    private Vector2[] GetDirections(int count, Transform casterTransform)
    {
        List<Vector2> dirs = new List<Vector2>();
        float angleStep = 360f / count;
        float baseAngle = Mathf.Atan2(casterTransform.up.y, casterTransform.up.x) * Mathf.Rad2Deg;
        for(int i = 0; i < count; i++)
        {
            float angle = baseAngle + i * angleStep;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            dirs.Add(dir);
        }
        return dirs.ToArray();
    }
}
