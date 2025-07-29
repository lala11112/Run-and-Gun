using UnityEngine;
using UnityEditor;

namespace Com.LuisPedroFonseca.ProCamera2D
{
    [CustomEditor(typeof(ConstantShakePreset))]
    public class ConstantShakePresetEditor : Editor
    {
        GUIContent _tooltip;

        ConstantShakePreset _preset;

        void OnEnable()
        {
            _preset = (ConstantShakePreset)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Intensity
            _tooltip = new GUIContent("강도 (Intensity)", "카메라가 새로운 흔들림 위치로 얼마나 빠르게 움직일지 결정합니다.");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Intensity"), _tooltip);

            // Layers
            _tooltip = new GUIContent("레이어 (Layers)", "서로 다른 주파수와 진폭을 가진 여러 흔들림 레이어를 중첩시켜 복잡한 효과를 만듭니다.");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Layers"), _tooltip, true);

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // Shake test buttons
            GUI.enabled = Application.isPlaying && ProCamera2DShake.Exists;
            if (GUILayout.Button("지속 흔들기 테스트 (Constant Shake!)"))
            {
                ProCamera2DShake.Instance.ConstantShake(_preset);
            }

            if (GUILayout.Button("중지 (Stop!)"))
            {
                ProCamera2DShake.Instance.StopConstantShaking();
            }
            GUI.enabled = true;

            if (GUILayout.Button("ProCamera2D 오브젝트로 이동"))
            {
                if (ProCamera2D.Instance != null)
                {
                    Selection.activeGameObject = ProCamera2D.Instance.gameObject;
                }
            }

            if (_preset.Intensity < .01f)
                _preset.Intensity = .01f;

            serializedObject.ApplyModifiedProperties();
        }
    }
}