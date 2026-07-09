using System;

public static class EventManager
{
    // Game State
    public static event Action OnGameOver;
    public static event Action OnStageClear;
    public static event Action OnNextStage;
    public static event Action OnBossDefeated;

    // Player
    public static event Action<int> OnLevelUp;

    // Stage UI
    public static event Action<int> OnStagePanel;
    public static event Action<float> OnStageSlider;

    public static void GameOver() => OnGameOver?.Invoke();
    public static void StageClear() => OnStageClear?.Invoke();
    public static void NextStage() => OnNextStage?.Invoke();
    public static void BossDefeated() => OnBossDefeated?.Invoke();
    public static void LevelUp(int level) => OnLevelUp?.Invoke(level);
    public static void StagePanel(int stageLevel) => OnStagePanel?.Invoke(stageLevel);
    public static void StageSlider(float value) => OnStageSlider?.Invoke(value);

    // 해제
    public static void ClearAllEvents()
    {
        OnGameOver = null;
        OnStageClear = null;
        OnNextStage = null;
        OnBossDefeated = null;
        OnLevelUp = null;
        OnStagePanel = null;
        OnStageSlider = null;
    }
}