using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PoolConfig
{
    public GameObject prefab;
    public int spawnCount = 100;
    public Transform spawnPoint;

    public bool isUI = false;
}

public class ObjectPoolingManager : MonoBehaviour
{
    public static ObjectPoolingManager Instance { get; private set; }

    [SerializeField] private PoolConfig[] pools;

    private class Pool
    {
        public PoolConfig config;
        public Queue<GameObject> inactive = new Queue<GameObject>();
        public HashSet<GameObject> allObjects = new HashSet<GameObject>();
        public Transform parent;
    }

    private Dictionary<string, Pool> poolDict = new Dictionary<string, Pool>();
    private Transform rootFolder;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        rootFolder = new GameObject("ObjectPool").transform;
        rootFolder.SetParent(transform, false);

        InitializePools();
    }

    private void InitializePools()
    {
        poolDict.Clear();

        foreach (PoolConfig config in pools)
        {
            if (config == null || config.prefab == null)
                continue;

            string key = config.prefab.name;

            if (poolDict.ContainsKey(key))
            {
                continue;
            }

            Transform parent = config.spawnPoint != null ? config.spawnPoint : new GameObject(key + "_Pool").transform;

            if (!config.isUI && parent.parent == null)
            {
                parent.SetParent(rootFolder, false);
            }

            Pool pool = new Pool
            {
                config = config,
                parent = parent
            };

            poolDict[key] = pool;

            for (int i = 0; i < Mathf.Max(0, config.spawnCount); i++)
            {
                GameObject go = Instantiate(config.prefab, parent);
                go.name = config.prefab.name;
                go.SetActive(false);
                pool.inactive.Enqueue(go);
                pool.allObjects.Add(go);
            }
        }
    }

    public GameObject GetFromPool(string prefabName, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (!poolDict.TryGetValue(prefabName, out Pool pool))
        {
            return null;
        }

        if (pool.inactive.Count == 0)
        {
            return null;
        }

        GameObject obj = pool.inactive.Dequeue();
        if (obj == null)
        {
            pool.allObjects.Remove(obj);
            return GetFromPool(prefabName, position, rotation, parent);
        }

        if (parent != null)
        {
            obj.transform.SetParent(parent, false);
        }
        else
        {
            obj.transform.SetParent(null, true);
        }

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);

        return obj;
    }

    public void ReturnToPool(GameObject obj)
    {
        if (obj == null) return;

        string prefabName = obj.name;

        if (!poolDict.TryGetValue(prefabName, out Pool pool))
        {
            Destroy(obj);
            return;
        }

        obj.SetActive(false);
        obj.transform.SetParent(pool.parent, false);
        pool.inactive.Enqueue(obj);
        pool.allObjects.Add(obj);
    }
}