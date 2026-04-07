using UnityEngine;
using System.Collections.Generic;

public class PathManager : MonoBehaviour
{
    public static PathManager instance;

    [Header("Prefab")]
    public UIPathFollower prefab;

    [Header("Route Assets")]
    public UIRoute[] routes;

    [Header("Pooling")]
    public int poolSize = 20;

    [Header("List Respawn Settings")]
    public float minDelay = 2f;
    public float maxDelay = 4f;

    [Header("Editor Gizmos")]
    public float gizmoPointSize = 25f;

    Queue<UIPathFollower> pool = new Queue<UIPathFollower>();

    public List<string> seenFollowers = new List<string>();
    Queue<string> pendingManualSpawns = new Queue<string>();

    // NEW: track how many active followers exist per name
    Dictionary<string, int> activeFollowers = new Dictionary<string, int>();

    bool spawnedThisFrame = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        BuildPool();

        foreach (var r in routes)
        {
            r.active = 0;
            r.currentSpawnInterval = Random.Range(r.minSpawnInterval, r.maxSpawnInterval);
            r.timer = r.currentSpawnInterval;
            r.nextListSpawnTime = Time.time + Random.Range(minDelay, maxDelay);
        }
    }

    void Update()
    {
        spawnedThisFrame = false;

        foreach (var r in routes)
        {
            if (r.timer < r.currentSpawnInterval)
                r.timer += Time.deltaTime;
        }

        ProcessPendingManualSpawns();

        foreach (var r in routes)
        {
            if (spawnedThisFrame)
                break;

            ProcessRouteListSpawn(r);
        }
    }

    // =====================================
    // PUBLIC SPAWN (PRIORITY)
    // =====================================

    public void Spawn(string s)
    {
        if (pool.Count == 0)
        {
            pendingManualSpawns.Enqueue(s);
            return;
        }

        UIRoute route = GetAvailableRoute();
        if (route == null)
        {
            pendingManualSpawns.Enqueue(s);
            return;
        }

        TrySpawn(route, s);
        spawnedThisFrame = true;
    }

    void ProcessPendingManualSpawns()
    {
        if (pendingManualSpawns.Count == 0)
            return;

        if (pool.Count == 0)
            return;

        UIRoute route = GetAvailableRoute();
        if (route == null)
            return;

        string name = pendingManualSpawns.Dequeue();

        TrySpawn(route, name);
        spawnedThisFrame = true;
    }

    // =====================================
    // LIST PER ROUTE SPAWNING
    // =====================================

    void ProcessRouteListSpawn(UIRoute route)
    {
        if (seenFollowers.Count == 0)
            return;

        if (Time.time < route.nextListSpawnTime)
            return;

        if (route.active >= route.maxActive)
            return;

        if (route.timer < route.currentSpawnInterval)
            return;

        if (pool.Count == 0)
            return;

        for (int i = 0; i < seenFollowers.Count; i++)
        {
            string name = seenFollowers[i];

            // IMPORTANT: skip if there are still active followers with this name
            if (activeFollowers.ContainsKey(name))
                continue;

            seenFollowers.RemoveAt(i);

            TrySpawn(route, name);

            route.nextListSpawnTime = Time.time + Random.Range(minDelay, maxDelay);
            spawnedThisFrame = true;

            return;
        }
    }

    // =====================================
    // INTERNAL
    // =====================================

    void BuildPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var obj = Instantiate(prefab, transform);
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    UIPathFollower GetFromPool()
    {
        if (pool.Count == 0)
            return null;

        return pool.Dequeue();
    }

    void ReturnToPool(UIPathFollower obj)
    {
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }

    UIRoute GetAvailableRoute()
    {
        foreach (var route in routes)
        {
            if (route.active >= route.maxActive)
                continue;

            if (route.timer < route.currentSpawnInterval)
                continue;

            return route;
        }

        return null;
    }

    void TrySpawn(UIRoute route, string name)
    {
        UIPathFollower obj = GetFromPool();
        if (obj == null)
            return;

        RectTransform[] points = ResolveRoute(route);
        if (points.Length < 2)
        {
            ReturnToPool(obj);
            return;
        }

        obj.transform.SetParent(points[0].parent);
        obj.gameObject.SetActive(true);
        obj.followerName.text = name;

        // register active follower
        if (!activeFollowers.ContainsKey(name))
            activeFollowers[name] = 0;

        activeFollowers[name]++;

        route.active++;
        route.currentSpawnInterval =
            Random.Range(route.minSpawnInterval, route.maxSpawnInterval);

        route.timer = 0f;

        obj.Initialize(
            points,
            route.speed,
            route.easeType,
            route.easePower,
            route.customCurve,
            route.gizmoColor,
            route.specialRotation,
            route.fixedRotation,
            route.textFlip180,
            (o) =>
            {
                route.active--;

                string finishedName = o.followerName.text;

                if (activeFollowers.ContainsKey(finishedName))
                {
                    activeFollowers[finishedName]--;

                    if (activeFollowers[finishedName] <= 0)
                        activeFollowers.Remove(finishedName);
                }

                if (!string.IsNullOrEmpty(finishedName) && !seenFollowers.Contains(finishedName))
                {
                    seenFollowers.Add(finishedName);
                }

                ReturnToPool(o);
            });
    }

    RectTransform[] ResolveRoute(UIRoute route)
    {
        List<RectTransform> list = new List<RectTransform>();

        GameObject parent = GameObject.Find(route.name + "_Points");
        if (!parent) return list.ToArray();

        foreach (string n in route.pointNames)
        {
            Transform t = parent.transform.Find(n);
            if (t)
                list.Add(t.GetComponent<RectTransform>());
        }

        return list.ToArray();
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (routes == null) return;

        foreach (var r in routes)
        {
            if (r == null) continue;

            Gizmos.color = r.gizmoColor;

            RectTransform[] pts = ResolveRoute(r);

            for (int i = 0; i < pts.Length - 1; i++)
            {
                if (!pts[i] || !pts[i + 1]) continue;

                Gizmos.DrawLine(pts[i].position, pts[i + 1].position);
                Gizmos.DrawSphere(pts[i].position, gizmoPointSize);
            }
        }
    }
#endif
}