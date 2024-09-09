using System;
using System.Collections.Generic;

internal class AStar
{
    private Action<int, int[]> neighborStrategy;
    private Func<int, int, float> neighbourDistanceStrategy;
    private Func<int, int, float> heuristicStrategy;
    private int length;

    public AStar(Action<int, int[]> neighborStrategy, Func<int, int, float> neighbourDistanceStrategy, Func<int, int, float> heuristicStrategy, int length)
    {
        this.neighborStrategy = neighborStrategy;
        this.neighbourDistanceStrategy = neighbourDistanceStrategy;
        this.heuristicStrategy = heuristicStrategy;
        this.length = length;
    }

    internal bool FindPathNodes(int startNode, int endNode, ref List<int> nodes)
    {
        throw new NotImplementedException();
    }
}