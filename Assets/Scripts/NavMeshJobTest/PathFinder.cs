using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

public class PathRequest
{
    public Vector3 Start;
    public Vector3 Destination;
    public float MaxRayDistance;
    public List<Vector3> OutPoints;
    public List<Vector3> OutDebugSearchList;
    public bool OutResult;
    public Action Callback;
}

public class PathFinder : MonoBehaviour
{
    static public PathFinder Get { get; private set; }

    [SerializeField] private NavmeshView _navmesh;

    public int MaxThreads = 8;
    public int BufferCountWarning = 200;

    private AStarPool _aStarPool = new AStarPool();
    private Queue<PathRequest> _finishedRequests = new Queue<PathRequest>(100);
    private List<PathRequest> _finishedRequestsBuffer = new List<PathRequest>(100);

    private List<Thread> _threads = new List<Thread>(20);
    private Queue<PathRequest> _newRequests = new Queue<PathRequest>(100);

    private void Awake()
    {
        //SINGLETON
        if (Get == null)
        {
            Get = this;
        }
        else
        {
            DestroyImmediate(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        //ASTAR
        for (int i = 0; i < MaxThreads; ++i)
        {
            _aStarPool.AddItem(new AStar(NeighborStrategy, NeighbourDistanceStrategy, HeuristicStrategy, _navmesh.NavmeshSO.Navmesh.CenterPoints.Length));
        }
    }

    private void Update()
    {
        ExecuteCallbacksOnPathFound();
        UpdateThreads();
    }

    public void RequestPath(PathRequest pr)
    {
        _newRequests.Enqueue(pr);

        //WaitCallback threadwork = (s) => ThreadWork(pr);
        //ThreadPool.QueueUserWorkItem(threadwork);
    }

    public int GetNumNodes()
    {
        return _navmesh.NavmeshSO.Navmesh.CenterPoints.Length;
    }

    private void StartThread(PathRequest pr)
    {
        ThreadStart threadStart = () => ThreadWork(pr);

        //threadStart.Invoke();

        Thread t = new Thread(threadStart);
        t.Start();
        _threads.Add(t);
    }

    private void ThreadWork(PathRequest pr)
    {
        AStar instance = null;
        lock (_aStarPool)
        {
            instance = _aStarPool.GetItem();
        }

        Stopwatch sw = new Stopwatch();
        sw.Start();
        FindPath(pr, instance);
        sw.Stop();

        lock (_aStarPool)
        {
            _aStarPool.AddItem(instance);
        }

        float distance = 0f;
        if (pr.OutResult)
        {
            for (int i = 1; i < pr.OutPoints.Count; ++i)
                distance += (pr.OutPoints[i] - pr.OutPoints[i - 1]).magnitude;
        }
        UnityEngine.Debug.Log($"Path found: {pr.OutResult}, distance: {distance}, with elapsed ms {sw.Elapsed.TotalMilliseconds}");

        lock (_finishedRequests)
        {
            _finishedRequests.Enqueue(pr);
        }
    }

    private void UpdateThreads()
    {
        for (int i = _threads.Count - 1; i >= 0; --i)
        {
            if (!_threads[i].IsAlive)
            {
                _threads.RemoveAt(i);
            }
        }

        while (_threads.Count < MaxThreads)
        {
            if (_newRequests.Count > 0)
            {
                StartThread(_newRequests.Dequeue());
            }
            else
            {
                break;
            }
        }

        if (_newRequests.Count > BufferCountWarning)
        {
            UnityEngine.Debug.Log($"Warning: over {BufferCountWarning} path requests in queue");
        }
    }

    private void ExecuteCallbacksOnPathFound()
    {
        if (_finishedRequests.Count > 0)
        {
            _finishedRequestsBuffer.Clear();
            lock (_finishedRequests)
            {
                for (int i = 0; i < _finishedRequests.Count; ++i)
                {
                    _finishedRequestsBuffer.Add(_finishedRequests.Dequeue());
                }
            }

            foreach (var request in _finishedRequestsBuffer)
            {
                request.Callback();
            }
        }
    }

    private void NeighborStrategy(int nodeIn, int[] nArr)
    {
        nArr[0] = _navmesh.NavmeshSO.Navmesh.TriangleConnections[nodeIn * 3];
        nArr[1] = _navmesh.NavmeshSO.Navmesh.TriangleConnections[nodeIn * 3 + 1];
        nArr[2] = _navmesh.NavmeshSO.Navmesh.TriangleConnections[nodeIn * 3 + 2];
    }

    private float NeighbourDistanceStrategy(int nodeFrom, int toNeighbour)
    {
        return (float)_navmesh.NavmeshSO.Navmesh.TriangleConnectionsDistances[nodeFrom * 3 + toNeighbour];
    }

    private float HeuristicStrategy(int nodeFrom, int nodeTo)
    {
        Vector3 posFrom = _navmesh.NavmeshSO.Navmesh.CenterPoints[nodeFrom];
        Vector3 posTo = _navmesh.NavmeshSO.Navmesh.CenterPoints[nodeTo];
        return (posTo - posFrom).magnitude;
    }

    private void FindPath(PathRequest pr, AStar aStar)
    {
        Vector3 navmeshStartPos = default(Vector3);
        Vector3 navmeshEndPos = default(Vector3);
        pr.OutResult = false;

        int startNode = _navmesh.GetClosestPointOnNavmesh_Partitioning(new Ray(pr.Start, Vector3.down), pr.MaxRayDistance, ref navmeshStartPos);
        if (startNode == -1) return;
        int endNode = _navmesh.GetClosestPointOnNavmesh_Partitioning(new Ray(pr.Destination, Vector3.down), pr.MaxRayDistance, ref navmeshEndPos);
        if (endNode == -1) return;

        List<int> nodes = null;

        bool success = false;
        Stopwatch sw = null;

        //sw = new Stopwatch();
        //sw.Start();
        //success = _aStar.OldFindPathNodes(startNode, endNode, ref nodes);
        //sw.Stop();
        //UnityEngine.Debug.Log($"Old AStar ran with {sw.Elapsed.TotalMilliseconds} ms");

        sw = new Stopwatch();
        sw.Start();
        success = aStar.FindPathNodes(startNode, endNode, ref nodes);
        sw.Stop();
        UnityEngine.Debug.Log($"AStar ran with {sw.Elapsed.TotalMilliseconds} ms");

        if (!success) return;

        if (pr.OutDebugSearchList != null)
        {
            pr.OutDebugSearchList.Clear();
            foreach (int node in nodes)
            {
                pr.OutDebugSearchList.Add(_navmesh.NavmeshSO.Navmesh.CenterPoints[node]);
            }
        }

        pr.OutPoints.Clear();
        foreach (int node in nodes)
        {
            pr.OutPoints.Add(_navmesh.NavmeshSO.Navmesh.CenterPoints[node]);
        }
        pr.OutPoints.Add(pr.Destination);

        pr.OutResult = true;
    }
}
