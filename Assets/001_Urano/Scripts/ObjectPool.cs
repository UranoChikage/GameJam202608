using System.Collections.Generic;
using UnityEngine;

// 汎用のGameObjectプール
// 破片などを使い回して、Instantiate/Destroyのコストを減らす
public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int initialSize = 6;

    private readonly Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        for (int i = 0; i < initialSize; i++)
        {
            pool.Enqueue(CreateInstance());
        }
    }

    private GameObject CreateInstance()
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.SetActive(false);
        return obj;
    }

    // プールから取り出して有効化する
    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        GameObject obj = pool.Count > 0 ? pool.Dequeue() : CreateInstance();

        obj.transform.SetParent(null);
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);

        return obj;
    }

    // 使い終わったオブジェクトをプールへ戻す
    public void Release(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(transform);
        pool.Enqueue(obj);
    }
}
