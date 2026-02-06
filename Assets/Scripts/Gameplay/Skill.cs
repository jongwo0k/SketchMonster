using UnityEngine;

public abstract class Skill : MonoBehaviour
{
    protected PlayerController player;

    public float cooldown = 5f; // 임시
    protected float lastUseTime = float.NegativeInfinity;

    private bool isReady => Time.time >= lastUseTime + cooldown; // 지났으면 true

    public virtual void Initialize(PlayerController _player)
    {
        player = _player;
    }

    public void UseSkill()
    {
        if (isReady)
        {
            lastUseTime = Time.time;
            Execute();
        }
    }

    // 쿨타임 UI 표시
    public float GetCooldownRatio()
    {
        if (isReady) return 0f; // overlay X

        float elapsed = Time.time - lastUseTime;
        return 1f - Mathf.Clamp01(elapsed / cooldown);
    }

    protected abstract void Execute(); // 개별 스킬
}