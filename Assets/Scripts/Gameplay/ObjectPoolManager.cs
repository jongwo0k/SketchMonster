using System.Collections.Generic;
using UnityEngine;

// 오브젝트 종류
public enum PoolType
{
    Projectile,
    Enemy,
    ExpOrb
    // Effect, Particle
    // Sound
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

    private Dictionary<PoolType, Queue<GameObject>> poolDictionary; // 재사용 대기
    private Dictionary<PoolType, List<GameObject>> activeObjects;   // 활성화, 전체

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        InitializePool();
    }

    private void InitializePool()
    {
        poolDictionary = new Dictionary<PoolType, Queue<GameObject>>();
        activeObjects = new Dictionary<PoolType, List<GameObject>>();

        foreach (var info in poolInfoList)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            List<GameObject> activeList = new List<GameObject>();

            // 미리 생성
            for (int i = 0; i < info.initialCount; i++)
            {
                GameObject obj = CreateNewObject(info);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(info.type, objectPool);
            activeObjects.Add(info.type, activeList);
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
        if (!poolDictionary.ContainsKey(type)) return null;

        GameObject obj;

        // 대기 풀에 있을 때
        if (poolDictionary[type].Count > 0)
        {
            obj = poolDictionary[type].Dequeue();
        }
        // 없으면 생성
        else
        {
            var info = poolInfoList.Find(x => x.type == type);
            obj = CreateNewObject(info);
        }

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);

        // 활성화 목록에 등록
        activeObjects[type].Add(obj);

        return obj;
    }

    // 비활성화 (Destroy)
    public void Despawn(GameObject obj, PoolType type)
    {
        if (!activeObjects.ContainsKey(type)) return;
        if (!obj.activeSelf) return;

        obj.SetActive(false);

        activeObjects[type].Remove(obj);
        poolDictionary[type].Enqueue(obj);
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