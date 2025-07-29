using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace Com.LuisPedroFonseca.ProCamera2D
{
    [CustomEditor(typeof(ProCamera2DCinematics))]
    [CanEditMultipleObjects]
    public class ProCamera2DCinematicsEditor : Editor
    {
        GUIContent _tooltip;

        MonoScript _script;

        ReorderableList _cinematicTargetsList;

        void OnEnable()
        {
            if (target == null)
                return;
            
            _script = MonoScript.FromMonoBehaviour((ProCamera2DCinematics)target);

            // Cinematic targets list
            _cinematicTargetsList = new ReorderableList(serializedObject, serializedObject.FindProperty("CinematicTargets"), false, true, false, true);

            _cinematicTargetsList.drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    rect.y += 2;
                    var element = _cinematicTargetsList.serializedProperty.GetArrayElementAtIndex(index);

                    EditorGUI.PrefixLabel(new Rect(
                        rect.x, 
                        rect.y, 
                        90, 
                        10), 
                        new GUIContent("타겟", "시네마틱 시퀀스에서 카메라가 바라볼 타겟 Transform 입니다."));
                    EditorGUI.PropertyField(new Rect(
                        rect.x + 90,
                        rect.y,
                        90,
                        EditorGUIUtility.singleLineHeight * 1.1f),
                        element.FindPropertyRelative("TargetTransform"), GUIContent.none);

                    EditorGUI.PrefixLabel(new Rect(
                        rect.x + 200, 
                        rect.y, 
                        90, 
                        10), 
                        new GUIContent("이동 방식", "카메라가 이 타겟으로 이동할 때의 애니메이션 커브입니다."));
                    EditorGUI.PropertyField(new Rect(
                        rect.x + 260,
                        rect.y,
                        rect.width - 260,
                        EditorGUIUtility.singleLineHeight * 1.1f),
                        element.FindPropertyRelative("EaseType"), GUIContent.none);

                    rect.y += 25;
                    EditorGUI.PrefixLabel(new Rect(
                        rect.x, 
                        rect.y, 
                        90, 
                        10), 
                        new GUIContent("이동 시간", "카메라가 이 타겟에 도달하는 데 걸리는 시간(초)입니다."));
                    EditorGUI.PropertyField(new Rect(
                        rect.x + 90,
                        rect.y,
                        30,
                        EditorGUIUtility.singleLineHeight * 1.1f),
                        element.FindPropertyRelative("EaseInDuration"), GUIContent.none);
                    
                    EditorGUI.PrefixLabel(new Rect(
                        rect.x + 135, 
                        rect.y, 
                        90, 
                        10), 
                        new GUIContent("유지 시간", "카메라가 이 타겟에 머무르는 시간(초)입니다. 0 미만으로 설정 시 다음 타겟으로 자동으로 넘어가지 않습니다."));
                    EditorGUI.PropertyField(new Rect(
                        rect.x + 215,
                        rect.y,
                        30,
                        EditorGUIUtility.singleLineHeight * 1.1f),
                        element.FindPropertyRelative("HoldDuration"), GUIContent.none);

                    EditorGUI.PrefixLabel(new Rect(
                        rect.x + 260, 
                        rect.y, 
                        90, 
                        10), 
                        new GUIContent("줌 배율", "카메라가 이 타겟을 비출 때의 줌 배율입니다. 1은 줌 없음, 2는 2배 줌인입니다."));
                    EditorGUI.PropertyField(new Rect(
                        rect.x + 300,
                        rect.y,
                        rect.width - 300,
                        EditorGUIUtility.singleLineHeight * 1.1f),
                        element.FindPropertyRelative("Zoom"), GUIContent.none);

                    rect.y += 25;
                    EditorGUI.PrefixLabel(new Rect(
                        rect.x, 
                        rect.y, 
                        90, 
                        10), 
                        new GUIContent("메시지 이름", "타겟에 도달했을 때 해당 타겟의 게임오브젝트로 보낼 메시지(메서드) 이름입니다."));
                    EditorGUI.PropertyField(new Rect(
                        rect.x + 115,
                        rect.y,
                        70,
                        EditorGUIUtility.singleLineHeight * 1.1f),
                        element.FindPropertyRelative("SendMessageName"), GUIContent.none);

                    EditorGUI.PrefixLabel(new Rect(
                        rect.x + 195, 
                        rect.y, 
                        90, 
                        10), 
                        new GUIContent("메시지 파라미터", "SendMessage 호출 시 전달할 (선택적) 문자열 파라미터입니다."));
                    EditorGUI.PropertyField(new Rect(
                        rect.x + 310,
                        rect.y,
                        rect.width - 310,
                        EditorGUIUtility.singleLineHeight * 1.1f),
                        element.FindPropertyRelative("SendMessageParam"), GUIContent.none);
                };

            _cinematicTargetsList.drawHeaderCallback = (Rect rect) =>
                {  
                    EditorGUI.LabelField(rect, "시네마틱 타겟 목록");
                };

            _cinematicTargetsList.elementHeight = 90;
            _cinematicTargetsList.draggable = true;
        }

        public override void OnInspectorGUI()
        {
            if (target == null)
                return;
            
            var proCamera2DCinematics = (ProCamera2DCinematics)target;
            if (proCamera2DCinematics.ProCamera2D == null)
            {
                EditorGUILayout.HelpBox("ProCamera2D is not set.", MessageType.Error, true);
                return;
            }

            serializedObject.Update();

            // Show script link
            GUI.enabled = false;
            _script = EditorGUILayout.ObjectField("Script", _script, typeof(MonoScript), false) as MonoScript;
            GUI.enabled = true;

            // ProCamera2D
            _tooltip = new GUIContent("Pro Camera 2D", "");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_pc2D"), _tooltip);

            // Targets Drop Area
            EditorGUILayout.Space();
            Event evt = Event.current;
            Rect drop_area = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
            var style = new GUIStyle("box");
            if (EditorGUIUtility.isProSkin)
                style.normal.textColor = Color.white;
            GUI.Box(drop_area, "\n이곳에 시네마틱 타겟(게임 오브젝트)을 드래그하세요", style);

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (drop_area.Contains(evt.mousePosition))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                        if (evt.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();

                            foreach (Object dragged_object in DragAndDrop.objectReferences)
                            {
                                var newCinematicTarget = new CinematicTarget
                                {
                                    TargetTransform = ((GameObject)dragged_object).transform,
                                    EaseInDuration = 1f,
                                    HoldDuration = 1f,
                                    EaseType = EaseType.EaseOut
                                };

                                proCamera2DCinematics.CinematicTargets.Add(newCinematicTarget);
                                EditorUtility.SetDirty(proCamera2DCinematics);
                            }
                        }
                    }
                    break;
            }

            EditorGUILayout.Space();

            // Remove empty targets
            for (int i = 0; i < proCamera2DCinematics.CinematicTargets.Count; i++)
            {
                if (proCamera2DCinematics.CinematicTargets[i].TargetTransform == null)
                {
                    proCamera2DCinematics.CinematicTargets.RemoveAt(i);
                }
            }

            // Camera targets list
            _cinematicTargetsList.DoLayoutList();
            EditorGUILayout.Space();

            // End duration
            _tooltip = new GUIContent("종료 시간", "시네마틱이 끝난 후, 카메라가 원래의 타겟으로 돌아가는 데 걸리는 시간입니다.");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("EndDuration"), _tooltip);

            // End ease type
            _tooltip = new GUIContent("종료 이동 방식", "시네마틱이 끝날 때의 카메라 복귀 애니메이션 커브입니다.");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("EndEaseType"), _tooltip);
            
            // Use numeric boundaries
            _tooltip = new GUIContent("경계(Boundaries) 사용", "활성화하면, 시네마틱 카메라의 움직임이 Numeric Boundaries의 영향을 받습니다.");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("UseNumericBoundaries"), _tooltip);

            // Letterbox
            _tooltip = new GUIContent("레터박스 사용", "활성화하면, 시네마틱 재생 중에 화면 위아래에 검은색 레터박스가 나타납니다.");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("UseLetterbox"), _tooltip);

            if (proCamera2DCinematics.UseLetterbox)
            {
                // Letterbox amount
                _tooltip = new GUIContent("레터박스 두께", "화면 높이 대비 레터박스의 두께 비율입니다. (0~0.5)");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("LetterboxAmount"), _tooltip);

                // Letterbox animation duration
                _tooltip = new GUIContent("레터박스 애니메이션 시간", "레터박스가 나타나고 사라지는 데 걸리는 시간(초)입니다.");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("LetterboxAnimDuration"), _tooltip);

                // Letterbox color
                _tooltip = new GUIContent("레터박스 색상", "레터박스의 색상입니다.");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("LetterboxColor"), _tooltip);
            }

            EditorGUILayout.Space();

            // Events
            // Cinematic started event
            _tooltip = new GUIContent("시네마틱 시작 시", "시네마틱 시퀀스가 시작될 때 호출될 이벤트입니다.");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnCinematicStarted"), _tooltip);

            // Cinematic target reached event
            _tooltip = new GUIContent("타겟 도달 시", "각 시네마틱 타겟에 도달했을 때 호출될 이벤트입니다. 도달한 타겟의 인덱스(int)를 전달합니다.");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnCinematicTargetReached"), _tooltip);

            // Cinematic finished event
            _tooltip = new GUIContent("시네마틱 종료 시", "시네마틱 시퀀스가 모두 끝났을 때 호출될 이벤트입니다.");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnCinematicFinished"), _tooltip);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // Test buttons
            GUI.enabled = Application.isPlaying && proCamera2DCinematics.CinematicTargets.Count > 0;
            if (GUILayout.Button((proCamera2DCinematics.IsPlaying ? "시네마틱 중지" : "시네마틱 시작")))
            {
                if (proCamera2DCinematics.IsPlaying)
                    proCamera2DCinematics.Stop();
                else
                    proCamera2DCinematics.Play();
            }
            GUI.enabled = true;

            serializedObject.ApplyModifiedProperties();
        }
    }
}