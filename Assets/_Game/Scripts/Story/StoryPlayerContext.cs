using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 스토리 재생 중에 여러 스텝에 걸쳐 공유되어야 하는 데이터를 담는 정적 클래스입니다.
/// 동적 변수 시스템과 오브젝트 참조 관리를 지원합니다.
/// </summary>
public static class StoryPlayerContext
{
    /// <summary>
    /// 현재 씬에 생성되어 있는 맵 프리팹의 인스턴스입니다.
    /// </summary>
    public static GameObject CurrentMap { get; set; }
    
    /// <summary>
    /// 플레이어 오브젝트 참조
    /// </summary>
    public static GameObject Player { get; set; }
    
    /// <summary>
    /// 현재 활성화된 적들의 목록
    /// </summary>
    public static List<GameObject> ActiveEnemies { get; private set; } = new List<GameObject>();
    
    /// <summary>
    /// 스토리 진행 중 사용할 수 있는 동적 변수들 (문자열 기반)
    /// </summary>
    private static Dictionary<string, object> _variables = new Dictionary<string, object>();
    
    /// <summary>
    /// 스토리 진행 중 참조해야 할 오브젝트들 (이름 기반)
    /// </summary>
    private static Dictionary<string, GameObject> _namedObjects = new Dictionary<string, GameObject>();
    
    /// <summary>
    /// 컨텍스트 변경 이벤트 (변수명, 이전값, 새값)
    /// </summary>
    public static event Action<string, object, object> OnVariableChanged;
    
    /// <summary>
    /// 오브젝트 등록/해제 이벤트 (이름, 오브젝트, 등록여부)
    /// </summary>
    public static event Action<string, GameObject, bool> OnObjectChanged;

    #region 변수 관리
    
    /// <summary>
    /// 변수를 설정합니다.
    /// </summary>
    public static void SetVariable<T>(string name, T value)
    {
        object oldValue = _variables.ContainsKey(name) ? _variables[name] : null;
        _variables[name] = value;
        OnVariableChanged?.Invoke(name, oldValue, value);
    }
    
    /// <summary>
    /// 변수를 가져옵니다.
    /// </summary>
    public static T GetVariable<T>(string name, T defaultValue = default(T))
    {
        if (_variables.ContainsKey(name) && _variables[name] is T)
        {
            return (T)_variables[name];
        }
        return defaultValue;
    }
    
    /// <summary>
    /// 변수가 존재하는지 확인합니다.
    /// </summary>
    public static bool HasVariable(string name)
    {
        return _variables.ContainsKey(name);
    }
    
    /// <summary>
    /// 변수를 제거합니다.
    /// </summary>
    public static bool RemoveVariable(string name)
    {
        if (_variables.ContainsKey(name))
        {
            object oldValue = _variables[name];
            _variables.Remove(name);
            OnVariableChanged?.Invoke(name, oldValue, null);
            return true;
        }
        return false;
    }
    
    #endregion
    
    #region 오브젝트 관리
    
    /// <summary>
    /// 이름으로 오브젝트를 등록합니다.
    /// </summary>
    public static void RegisterObject(string name, GameObject obj)
    {
        _namedObjects[name] = obj;
        OnObjectChanged?.Invoke(name, obj, true);
    }
    
    /// <summary>
    /// 이름으로 오브젝트를 가져옵니다.
    /// </summary>
    public static GameObject GetObject(string name)
    {
        return _namedObjects.ContainsKey(name) ? _namedObjects[name] : null;
    }
    
    /// <summary>
    /// 오브젝트 등록을 해제합니다.
    /// </summary>
    public static bool UnregisterObject(string name)
    {
        if (_namedObjects.ContainsKey(name))
        {
            GameObject obj = _namedObjects[name];
            _namedObjects.Remove(name);
            OnObjectChanged?.Invoke(name, obj, false);
            return true;
        }
        return false;
    }
    
    #endregion
    
    #region 적 관리
    
    /// <summary>
    /// 적을 활성 목록에 추가합니다.
    /// </summary>
    public static void AddEnemy(GameObject enemy)
    {
        if (enemy != null && !ActiveEnemies.Contains(enemy))
        {
            ActiveEnemies.Add(enemy);
        }
    }
    
    /// <summary>
    /// 적을 활성 목록에서 제거합니다.
    /// </summary>
    public static void RemoveEnemy(GameObject enemy)
    {
        ActiveEnemies.Remove(enemy);
    }
    
    /// <summary>
    /// 모든 활성 적을 제거합니다.
    /// </summary>
    public static void ClearAllEnemies()
    {
        foreach (var enemy in ActiveEnemies)
        {
            if (enemy != null)
            {
                UnityEngine.Object.Destroy(enemy);
            }
        }
        ActiveEnemies.Clear();
    }
    
    #endregion

    /// <summary>
    /// 컨텍스트를 초기 상태로 리셋합니다. 챕터 시작 시 호출될 수 있습니다.
    /// </summary>
    public static void Reset()
    {
        // 맵 정리
        if (CurrentMap != null)
        {
            UnityEngine.Object.Destroy(CurrentMap);
        }
        CurrentMap = null;
        
        // 적들 정리
        ClearAllEnemies();
        
        // 변수들 정리
        _variables.Clear();
        
        // 오브젝트 참조 정리
        _namedObjects.Clear();
        
        // 플레이어 참조 재설정
        var playerObj = GameObject.FindWithTag("Player");
        Player = playerObj;
        
        Debug.Log("[StoryPlayerContext] 컨텍스트가 리셋되었습니다.");
    }
    
    /// <summary>
    /// 현재 컨텍스트 상태를 로그로 출력합니다. (디버그용)
    /// </summary>
    public static void LogCurrentState()
    {
        Debug.Log($"[StoryPlayerContext] 상태:");
        Debug.Log($"  - 맵: {(CurrentMap != null ? CurrentMap.name : "없음")}");
        Debug.Log($"  - 플레이어: {(Player != null ? Player.name : "없음")}");
        Debug.Log($"  - 활성 적: {ActiveEnemies.Count}마리");
        Debug.Log($"  - 변수: {_variables.Count}개");
        Debug.Log($"  - 등록된 오브젝트: {_namedObjects.Count}개");
    }
}
