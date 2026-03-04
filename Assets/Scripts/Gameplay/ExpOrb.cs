using UnityEngine;

public class ExpOrb : MonoBehaviour
{
    // 1개당 경험치 증가량
    public float expValue;

    private void Awake()
    {
        expValue = GameConfig.Data.expValue;
    }
}