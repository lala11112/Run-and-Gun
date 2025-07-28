/// <summary>
/// 한 번의 게임 플레이(Run) 결과를 담는 데이터 클래스입니다.
/// GameplayState에서 생성하여 ResultState로 전달됩니다.
/// </summary>
public class RunResultData
{
    public bool wasVictory;
    public float timePlayed;
    public int goldEarned;
    // 필요한 통계(처치한 적 수 등)를 자유롭게 추가할 수 있습니다.
} 