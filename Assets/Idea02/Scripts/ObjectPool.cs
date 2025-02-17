using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Object = UnityEngine.Object;

[System.Serializable]
public class Pool
{
    public string name;
    public GameObject go;
    public int count;
    public List<GameObject> actives;
    public Queue<GameObject> deactives;

    public Pool(string name, GameObject go, int count)
    {
        this.name = name;
        this.go = go;
        this.count = count;
    }

    public void CreateAPool(Transform container, Dictionary<int, int> dicClones)
    {
        actives = new List<GameObject>();
        deactives = new Queue<GameObject>();
        for (int i = 0; i < count; i++)
        {
            spawnAClone(container, dicClones);
        }
    }

    void spawnAClone(Transform container, Dictionary<int, int> dicClones)
    {
        var clone = Object.Instantiate(go, container);
        clone.transform.localScale = Vector3.one;
        clone.name += (actives.Count + deactives.Count);
        deactives.Enqueue(clone);
        dicClones.Add(clone.GetHashCode(), GetHashCode());
    }

    public GameObject Get(Transform container, Dictionary<int, int> dicClones)
    {
        if (deactives.Count == 0)
            spawnAClone(container, dicClones);
        var clone = deactives.Dequeue();
        actives.Add(clone);
        return clone;
    }

    public void Return(GameObject go, bool deactive)
    {
        if (deactive)
        {
            go.SetActive(false);
        }
        if (actives.Contains(go))
            actives.Remove(go);
        if (!deactives.Contains(go))
            deactives.Enqueue(go);
    }

    public void OnDestroy()
    {
        foreach (var active in actives)
        {
            if (active != null)
            {
                active.transform.DOKill();
            }

        }

        foreach (var deactive in deactives)
        {
            if (deactive != null)
            {
                deactive.transform.DOKill();
            }
        }
    }
}

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool instance;

    public Dictionary<int, int> dicClones = new Dictionary<int, int>();
    public List<Pool> pools = new List<Pool>();
    public List<Pool> mapPlat = new List<Pool>();

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        foreach (var pool in pools)
        {
            pool.CreateAPool(transform, dicClones);
        }
        foreach (var pool in mapPlat)
        {
            pool.CreateAPool(transform, dicClones);
        }
    }

    public void ReturnAllPool()
    {
        foreach (var p in pools)
        {
            while (p.actives.Count > 0)
            {
                p.Return(p.actives[p.actives.Count - 1], true);
            }
        }
        foreach (var p in mapPlat)
        {
            while (p.actives.Count > 0)
            {
                p.Return(p.actives[p.actives.Count - 1], true);
            }
        }
    }

    public GameObject Get(Pool p)
    {
        return p.Get(transform, dicClones);
    }

    public void Return(GameObject clone, bool deactive)
    {
        clone.transform.DOKill();
        var hash = clone.GetHashCode();
        if (dicClones.ContainsKey(hash))
        {
            var p = getPool(dicClones[hash]);
            p.Return(clone, deactive);
        }
        else
        {
            Debug.LogError(clone.transform.name, clone.transform);
        }
    }
    Pool getPool(int hash)
    {
        foreach (var pool in pools)
        {
            if (pool.GetHashCode() == hash)
                return pool;
        }
        foreach (var pool in mapPlat)
        {
            if (pool.GetHashCode() == hash)
                return pool;
        }
        return null;
    }

    private void OnDestroy()
    {
        foreach (var pool in pools)
        {
            pool.OnDestroy();
        }
        foreach (var pool in mapPlat)
        {
            pool.OnDestroy();
        }
    }
}