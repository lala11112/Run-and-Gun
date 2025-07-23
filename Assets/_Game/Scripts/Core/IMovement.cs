using UnityEngine;

public interface IMovement
{
    float MoveSpeed { get; }
    float KeepDistance { get; }
    void Move(Transform target);
    void Stop();
}

// NavMeshMovement 클래스는 이제 NavMeshMovement.cs 로 이동했습니다. 