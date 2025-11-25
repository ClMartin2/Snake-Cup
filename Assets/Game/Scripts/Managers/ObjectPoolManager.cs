using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    [System.Serializable]
    public class PoolEntry
    {
        public string Key;
        public GameObject Prefab;
        public int Count = 10;
    }

    public List<PoolEntry> Pools = new List<PoolEntry>();

    private Dictionary<string, Queue<GameObject>> poolQueues = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<GameObject, string> instanceToKey = new Dictionary<GameObject, string>();

    private void Awake()
    {
        Instance = this;
        InitializePools();
    }

    private void InitializePools()
    {
        poolQueues.Clear();
        instanceToKey.Clear();

        foreach (var entry in Pools)
        {
            if (entry.Prefab == null || string.IsNullOrEmpty(entry.Key))
                continue;

            var q = new Queue<GameObject>();
            poolQueues[entry.Key] = q;

            var parent = new GameObject(entry.Key + "_Pool").transform;
            parent.SetParent(transform);

            for (int i = 0; i < entry.Count; i++)
            {
                var instance = CreateInstance(entry.Key, parent);
                instance.SetActive(false);
                q.Enqueue(instance);
            }
        }
    }

    private GameObject CreateInstance(string key, Transform parent)
    {
        var prefab = Pools.Find(p => p.Key == key)?.Prefab;
        if (prefab == null)
        {
            Debug.LogWarning($"SimplePoolManager: prefab introuvable pour '{key}'.");
            return null;
        }

        var obj = Instantiate(prefab, parent);
        instanceToKey[obj] = key;
        return obj;
    }

    public GameObject Get(string key)
    {
        if (!poolQueues.TryGetValue(key, out var q))
        {
            Debug.LogWarning($"SimplePoolManager: pool '{key}' introuvable.");
            return null;
        }

        // Si vide → on crée automatiquement une nouvelle instance
        if (q.Count == 0)
        {
            var parent = transform.Find(key + "_Pool");
            return CreateInstance(key, parent);
        }

        var go = q.Dequeue();
        go.SetActive(true);
        return go;
    }

    public void Release(GameObject go)
    {
        if (go == null) return;

        if (instanceToKey.TryGetValue(go, out var key))
        {
            go.SetActive(false);
            var parent = transform.Find(key + "_Pool");
            go.transform.SetParent(parent);
            poolQueues[key].Enqueue(go);
        }
        else
        {
            Destroy(go);
        }
    }
}
