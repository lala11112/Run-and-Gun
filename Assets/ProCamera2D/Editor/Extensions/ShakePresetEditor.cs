using UnityEngine;
using UnityEditor;

namespace Com.LuisPedroFonseca.ProCamera2D
{
    [CustomEditor(typeof(ShakePreset))]
    public class ShakePresetEditor : Editor
    {
        GUIContent _tooltip;

        ShakePreset _preset;

        void OnEnable()
        {
            _preset = (ShakePreset)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Strength
            _tooltip = new GUIContent("강도 (Strength)", "각 축(X, Y) 방향으로의 흔들림 강도입니다.");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Strength"), _tooltip);

            // Duration
            _tooltip = new GUIContent("지속 시간 (Duration)", "흔들림의 전체 지속 시간(초)입니다.");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Duration"), _tooltip);

            // Vibrato
            _tooltip = new GUIContent("진동 (Vibrato)", "흔들림이 얼마나 잘게 떨리는지를 결정합니다. 높을수록 더 많이 진동합니다.");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Vibrato"), _tooltip);

            // Smoothness
            _tooltip = new GUIContent("부드러움 (Smoothness)", "카메라의 움직임과 회전이 얼마나 부드럽게 적용될지 결정합니다. 0에 가까울수록 즉각적입니다.");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Smoothness"), _tooltip);

            // Randomness
            _tooltip = new GUIContent("불규칙성 (Randomness)", "흔들림의 불규칙성입니다. 0이면 완전히 규칙적이고, 1에 가까울수록 예측 불가능해집니다.");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Randomness"), _tooltip);

            // Random initial direction
            EditorGUILayout.BeginHorizontal();
            _tooltip = new GUIContent("랜덤 시작 각도 사용", "활성화하면, InitialAngle 값을 무시하고 항상 랜덤 각도로 흔들림을 시작합니다.");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("UseRandomInitialAngle"), _tooltip);

            if (!_preset.UseRandomInitialAngle)
            {
                _tooltip = new GUIContent("시작 각도 (Initial Angle)", "흔들림이 시작될 때의 초기 각도입니다.");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("InitialAngle"), _tooltip);
            }
            EditorGUILayout.EndHorizontal();

            // Rotation
            _tooltip = new GUIContent("회전 (Rotation)", "흔들림 중 카메라가 회전할 최대 각도(x, y, z)입니다.");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Rotation"), _tooltip);

            // Ignore time scale
            _tooltip = new GUIContent("TimeScale 무시", "활성화하면, Time.timeScale 값에 영향을 받지 않고 흔들림이 발생합니다.");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("IgnoreTimeScale"), _tooltip);

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // Shake test buttons
            GUI.enabled = Application.isPlaying;
            if (GUILayout.Button("흔들기 테스트 (Shake!)"))
            {
                if (ProCamera2DShake.Exists)
                    ProCamera2DShake.Instance.Shake(_preset);
            }

            if (GUILayout.Button("중지 (Stop!)"))
            {
                if (ProCamera2DShake.Exists)
                    ProCamera2DShake.Instance.StopShaking();
            }
            GUI.enabled = true;

            if (GUILayout.Button("ProCamera2D 오브젝트로 이동"))
            {
                Selection.activeGameObject = ProCamera2D.Instance.gameObject;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}