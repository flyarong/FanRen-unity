using System.Collections.Generic;
using UnityEngine;

public class AStarPathUtil
{

    public class Node
    {
        public int x;
        public int y;
        public int g;
        public int h;
        public int f;
        public Node preNode;
        public Node(int x, int y, Node preNode, (int, int) target)
        {
            this.x = x;
            this.y = y;
            this.preNode = preNode;
            this.g = preNode != null ? (preNode.g + 1) : 0; //不允许斜着行走，所以全部是固定1
            this.h = Mathf.Abs(this.x - target.Item1) + Mathf.Abs(this.y - target.Item2);
            this.f = this.g + this.h;
        }

    }

    public int[][] map;
    public (int, int) start;
    public (int, int) target;
    public List<(int, int)> obstacles;

    private List<Node> openList = new List<Node>();
    private List<Node> closeList = new List<Node>();


    public List<Node> GetShortestPath()
    {
        Node startNode = new Node(start.Item1, start.Item2, null, target);
        closeList.Add(startNode);

        List<Node> _openList = GetNeighbors(startNode, map.GetLength(0), map.GetLength(1));
        openList.AddRange(_openList);

        //检查openlist中有没终点，有则结束
        foreach (Node item in openList)
        {
            if(target.Item1 == item.x && target.Item2 == item.y)
            {
                return closeList;
            }
        }
        //================初始化完成

        Node smallestF;
        while ((smallestF = GetSmallestFValueFromOpenList()) != null)
        {
            //没有到终点，获取f最小的node
            //smallestF = GetSmallestFValueFromOpenList();
            if (smallestF == null)
            {
                //不存在路径
                return new List<Node>();
            }
            closeList.Add(smallestF);
            openList.Remove(smallestF);

            List<Node> _openList2 = GetNeighbors(smallestF, map.GetLength(0), map.GetLength(1));
            openList.AddRange(_openList2);

            //检查openlist中有没终点，有则结束
            foreach (Node item in openList)
            {
                if (target.Item1 == item.x && target.Item2 == item.y)
                {
                    List<Node> path = new List<Node>();
                    path.Add(item);
                    Node node = item;
                    while (node.preNode != null)
                    {
                        node = node.preNode;
                        path.Add(node);
                    }
                    return path;
                }
            }
        }

        //不存在路径
        return new List<Node>();

    }

    private Node GetSmallestFValueFromOpenList()
    {
        if (openList.Count == 0) return null;
        int f = openList[0].f;
        int index = 0;
        for(int i=1; i< openList.Count; i++)
        {
            if (openList[i].f < f)
            {
                f = openList[i].f;
                index = i;
            }
        }
        return openList[index];
    }

    private List<Node> GetNeighbors(Node node, int mapWidth, int mapHeight)
    {
        List<Node> nodes = new List<Node>();
        if (IsValidityPosition(node.x, node.y + 1, mapWidth, mapHeight))
        {
            nodes.Add(new Node(node.x, node.y + 1, node, target));
        }
        if (IsValidityPosition(node.x, node.y - 1, mapWidth, mapHeight))
        {
            nodes.Add(new Node(node.x, node.y - 1, node, target));
        }
        if (IsValidityPosition(node.x+1, node.y, mapWidth, mapHeight))
        {
            nodes.Add(new Node(node.x+1, node.y, node, target));
        }
        if (IsValidityPosition(node.x-1, node.y, mapWidth, mapHeight))
        {
            nodes.Add(new Node(node.x-1, node.y, node, target));
        }
        return nodes;
    }

    private bool IsValidityPosition(int x, int y, int mapWidth, int mapHeight)
    {

        //超出界限的位置
        if (x > mapWidth - 1) return false;
        if (x < 0) return false;
        if (y > mapHeight - 1) return false;
        if (y < 0) return false;

        //障碍物的位置
        if(obstacles != null && obstacles.Count > 0)
        {
            foreach ((int, int) wrongItem in obstacles)
            {
                if (wrongItem.Item1 == x && wrongItem.Item2 == y)
                {
                    return false;
                }
            }
        }

        foreach (Node item in openList)
        {
            if (x == item.x && y == item.y)
            {
                return false;
            }
        }

        foreach (Node item in closeList)
        {
            if (x == item.x && y == item.y)
            {
                return false;
            }
        }

        return true;
    }

}
