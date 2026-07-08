using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

// 오브젝트 종류
public enum PoolType
{
    Projectile,
    Enemy,
    ExpOrb,
    HitParticle,
    EnemyHitParticle,
    DieParticle,
    BarkEffect,
    BubbleSkill,
    BossProjectile,
    Boss,
}

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    [System.Serializable]
    public class PoolInfo
    {
        public PoolType type;
        public GameObject prefab;
        public int initialCount = 20;
        public Transform container; // 정리용
    }

    [SerializeField] private List<PoolInfo> poolInfoList;

    private Dictionary<PoolType, IObjectPool<GameObject>> poolDictionary; // 재사용 대기
    private Dictionary<PoolType, HashSet<GameObject>> activeObjects;      // 활성화, 전체

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        InitializePool();
    }

    private void InitializePool()
    {
        poolDictionary = new Dictionary<PoolType, IObjectPool<GameObject>>();
        activeObjects = new Dictionary<PoolType, HashSet<GameObject>>();

        foreach (var info in poolInfoList)
        {
            activeObjects.Add(info.type, new HashSet<GameObject>());

            IObjectPool<GameObject> pool = new ObjectPool<GameObject>(
                createFunc: () => CreateNewObject(info),
                actionOnGet: (obj) =>
                {
                    activeObjects[info.type].Add(obj);
                },
                actionOnRelease: (obj) =>
                {
                    obj.SetActive(false);
                    activeObjects[info.type].Remove(obj);
                },
                actionOnDestroy: (obj) => Destroy(obj),
                defaultCapacity: info.initialCount
            );

            poolDictionary.Add(info.type, pool);

            // 미리 생성
            var prewarmList = new List<GameObject>(info.initialCount);
            for (int i = 0; i < info.initialCount; i++)
            {
                prewarmList.Add(pool.Get());
            }
            foreach (var obj in prewarmList)
            {
                pool.Release(obj);
            }
        }
    }

    private GameObject CreateNewObject(PoolInfo info)
    {
        GameObject obj = Instantiate(info.prefab, info.container);
        obj.SetActive(false);
        return obj;
    }

    // 풀에서 꺼내 사용
    public GameObject Spawn(PoolType type, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.TryGetValue(type, out var pool)) return null;

        GameObject obj = pool.Get();
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);

        return obj;
    }

    // 비활성화 (재사용 대기)
    public void Despawn(GameObject obj, PoolType type)
    {
        if (!poolDictionary.TryGetValue(type, out var pool)) return;
        if (!obj.activeSelf) return;

        pool.Release(obj);
    }

    // 전체 정리
    public void ClearObjects()
    {
        foreach (var key in activeObjects.Keys)
        {
            var listToClear = new List<GameObject>(activeObjects[key]);

            foreach (var obj in listToClear)
            {
                if (obj.activeSelf)
                {
                    Despawn(obj, key);
                }
            }
        }
    }
}