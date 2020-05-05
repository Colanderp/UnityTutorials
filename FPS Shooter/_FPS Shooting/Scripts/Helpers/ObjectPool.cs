using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObjectPool
{
    public PooledObject pooledObj;
    List<PooledObject> pool;
    Transform PoolParent;

    public ObjectPool(PooledObject o)
    {
        pooledObj = o;
        pool = new List<PooledObject>();

        PoolParent = new GameObject().transform;
        PoolParent.transform.name = pooledObj.transform.name + "-Pool";
    }

    public ObjectPool getPool()
    {
        return this;
    }

    public PooledObject get()
    {
        PooledObject obj = null;
        if (pool == null) pool = new List<PooledObject>();
        if((obj = getInPool()) == null)
        {
            GameObject clone = GameObject.Instantiate(pooledObj.gameObject, PoolParent);
            clone.transform.name = pooledObj.transform.name;
            obj = clone.GetComponent<PooledObject>();
            obj.SetParent(PoolParent);
            pool.Add(obj);
        }

        obj.Initialize();
        return obj;
    }

    PooledObject getInPool()
    {
        for(int i = 0; i < pool.Count; i++)
        {
            if (pool[i].isInPool)
                return pool[i];
        }
        return null;
    }
}
