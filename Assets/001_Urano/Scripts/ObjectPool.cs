using System.Collections.Generic;
using UnityEngine;

// 複数種類のプレハブに対応したGameObjectプール
// 破片などを使い回して、Instantiate/Destroyのコストを減らす
public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private int initialSizePerPrefab = 2;

    // プレハブごとの待機プール
    private readonly Dictionary<GameObject, Queue<GameObject>> pools =
        new Dictionary<GameObject, Queue<GameObject>>();

    // 生成したインスタンスがどのプレハブ由来かを記録（Releaseで戻す先を判定するため）
    private readonly Dictionary<GameObject, GameObject> instanceToPrefab =
        new Dictionary<GameObject, GameObject>();

    private void Awake()
    {
        if (prefabs == null)
            return;

        foreach (GameObject prefab in prefabs)
        {
            if (prefab == null)
                continue;

            Queue<GameObject> queue = new Queue<GameObject>();
            pools[prefab] = queue;

            for (int i = 0; i < initialSizePerPrefab; i++)
            {
                queue.Enqueue(CreateInstance(prefab));
            }
        }
    }

    private GameObject CreateInstance(GameObject prefab)
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.SetActive(false);
        instanceToPrefab[obj] = prefab;
        return obj;
    }

    // プレハブの中からランダムに1種類選び、プールから取り出して有効化する
    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogWarning("ObjectPoolにprefabsが設定されていません。");
            return null;
        }

        GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
        Queue<GameObject> queue = pools[prefab];

        GameObject obj = queue.Count > 0 ? queue.Dequeue() : CreateInstance(prefab);

        obj.transform.SetParent(null);
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);

        return obj;
    }

    // 使い終わったオブジェクトを、由来したプレハブのプールへ戻す
    public void Release(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(transform);

        if (instanceToPrefab.TryGetValue(obj, out GameObject prefab) &&
            pools.TryGetValue(prefab, out Queue<GameObject> queue))
        {
            queue.Enqueue(obj);
        }
    }
}
