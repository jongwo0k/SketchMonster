using UnityEngine;

public static class GameConfig
{
    private static BalanceData _data;

    public static BalanceData Data
    {
        get
        {
            if (_data == null)
            {
                _data = Resources.Load<BalanceData>("BalanceData");
            }
            return _data;
        }
    }
}