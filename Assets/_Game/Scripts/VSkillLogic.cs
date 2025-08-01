using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening; // DOTween을 사용하므로 네임스페이스 추가

/// <summary>
/// V 스킬(점프-슬램 & 다방향 투사체)의 행동을 실제로 구현하는 전용 로직 클래스입니다.
/// </summary>
public class VSkillLogic : SkillBase
{
    private bool _isRunning;

    public override void Activate(GameObject caster, SkillDataSO skillData, StyleRank currentRank)
    {
        if (_isRunning) return;
        StartCoroutine(SkillRoutine(caster, skillData, currentRank));
    }

    private IEnumerator SkillRoutine(GameObject caster, SkillDataSO skillData, StyleRank currentRank)
    {
        _isRunning = true;
        
        var playerController = caster.GetComponent<PlayerController>();

        // 1. 점프-슬램 연출
        if (skillData.jumpSlamData != null)
        {
            var jumpData = skillData.jumpSlamData;
            // TODO: PlayerController를 통해 무적 처리 필요
            
            Sequence jumpSeq = DOTween.Sequence();
            jumpSeq.Append(caster.transform.DOBlendableMoveBy(Vector3.up * jumpData.jumpHeight, jumpData.airTime * 0.5f).SetEase(Ease.OutQuad));
            jumpSeq.Append(caster.transform.DOBlendableMoveBy(Vector3.down * jumpData.jumpHeight, jumpData.airTime * 0.5f).SetEase(Ease.InQuad));
            yield return jumpSeq.WaitForCompletion();

            // TODO: 카메라 흔들림 및 착지 이펙트 로직 추가
            // CameraManager.Instance?.ShakeWithPreset(jumpData.slamShakePreset);
            // if(jumpData.landingEffectPrefab != null) Instantiate(jumpData.landingEffectPrefab, caster.transform.position, Quaternion.identity);
        }

        // 2. 다방향 투사체 발사
        if (skillData.multiShotData != null)
        {
            var shotData = skillData.multiShotData;
            var rankBonus = skillData.rankBonuses.FirstOrDefault(b => b.rank == currentRank) ?? new RankBonusData();
            
            float finalDamage = skillData.baseDamage * rankBonus.damageMultiplier;
            int directions = shotData.directions; // TODO: 랭크별 방향 개수 보너스 적용 필요
            
            Vector2[] dirVectors = GetDirections(directions, caster.transform);

            foreach (var dir in dirVectors)
            {
                Quaternion rot = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f);
                GameObject proj = Instantiate(shotData.projectilePrefab, caster.transform.position, rot);
                
                if(proj.TryGetComponent<Projectile>(out var p))
                {
                    p.damage = (int)finalDamage;
                    // p.speed = ...
                    p.Init(dir);
                    // TODO: 랭크별 스턴/커브/넉백 등 특수효과 적용 로직 필요
                }
            }
        }

        _isRunning = false;
    }

    private Vector2[] GetDirections(int count, Transform casterTransform)
    {
        List<Vector2> dirs = new List<Vector2>();
        float angleStep = 360f / count;
        for(int i = 0; i < count; i++)
        {
            float angle = i * angleStep;
            // caster의 up 벡터를 기준으로 회전
            Vector2 dir = Quaternion.Euler(0, 0, angle) * casterTransform.up;
            dirs.Add(dir.normalized);
        }
        return dirs.ToArray();
    }
}
