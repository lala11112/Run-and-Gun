using UnityEngine;

namespace Com.LuisPedroFonseca.ProCamera2D
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "ProCamera2D/Shake Preset")]
    public class ShakePreset : ScriptableObject
    {
        [Tooltip("흔들림의 전체 지속 시간(초)입니다.")]
        public float Duration = .5f;

        [Tooltip("각 축(X, Y) 방향으로의 흔들림 강도입니다.")]
        public Vector2 Strength = new Vector2(1, 1);

        [Tooltip("흔들림이 얼마나 잘게 떨리는지를 결정합니다. 높을수록 더 많이 진동합니다.")]
        public int Vibrato = 10;

        [Tooltip("흔들림의 불규칙성입니다. 0이면 완전히 규칙적이고, 1에 가까울수록 예측 불가능해집니다.")]
        [Range(0, 1)]
        public float Randomness = .1f;

        [Tooltip("흔들림이 시작될 때의 초기 각도입니다. -1로 설정하면 랜덤 각도로 시작합니다.")]
        public float InitialAngle = -1f;

        [Tooltip("활성화하면, InitialAngle 값을 무시하고 항상 랜덤 각도로 흔들림을 시작합니다.")]
        public bool UseRandomInitialAngle = true;

        [Tooltip("흔들림 중 카메라가 회전할 최대 각도(x, y, z)입니다.")]
        public Vector3 Rotation;

        [Tooltip("카메라의 움직임과 회전이 얼마나 부드럽게 적용될지 결정합니다. 0에 가까울수록 즉각적입니다.")]
        public float Smoothness = .1f;

        [Tooltip("활성화하면, Time.timeScale 값에 영향을 받지 않고 흔들림이 발생합니다.")]
        public bool IgnoreTimeScale;
    }
}