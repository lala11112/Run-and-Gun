using System.Collections.Generic;
using UnityEngine;

namespace Com.LuisPedroFonseca.ProCamera2D
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "ProCamera2D/Constant Shake Preset")]
    public class ConstantShakePreset : ScriptableObject
    {
        [Tooltip("지속적인 흔들림의 전체 강도입니다.")]
        public float Intensity = .1f;

        [Tooltip("서로 다른 주파수와 진폭을 가진 여러 흔들림 레이어를 중첩시켜 복잡한 효과를 만듭니다.")]
        public List<ConstantShakeLayer> Layers = new List<ConstantShakeLayer>();
    }

    [System.Serializable]
    public class ConstantShakeLayer
    {
        [Tooltip("이 레이어의 흔들림이 얼마나 자주 발생할지를 결정하는 주파수 범위(최소/최대)입니다.")]
        public Vector2 Frequency = new Vector2(.02f, .06f);

        [Tooltip("수평(X) 방향으로의 최대 흔들림 폭입니다.")]
        public float AmplitudeHorizontal;

        [Tooltip("수직(Y) 방향으로의 최대 흔들림 폭입니다.")]
        public float AmplitudeVertical;

        [Tooltip("깊이(Z) 방향으로의 최대 흔들림 폭입니다.")]
        public float AmplitudeDepth;
    }
}