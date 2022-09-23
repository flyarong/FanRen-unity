using System.Collections.Generic;
using UnityEngine;

public class ActionNodeManager
{
    private SortedList<float, IActionNode> actionNodes;

    public GameObject activingRoleGO;
    public List<GameObject> allRoleGO;
    public GameObject[,] mapGridItems;

    public ActionStrategyGeneral actionStrategyGeneral;

    public ActionNodeManager(GameObject activingRoleGO, List<GameObject> allRoleGO, GameObject[,] mapGridItems, ActionStrategyGeneral actionStrategyGeneral)
    {
        this.actionNodes = new SortedList<float, IActionNode>(new MyActionNodeSort());
        this.activingRoleGO = activingRoleGO;
        this.allRoleGO = allRoleGO;
        this.mapGridItems = mapGridItems;
        this.actionStrategyGeneral = actionStrategyGeneral;
    }

    public ActionNodeManager AddActionNode(IActionNode actionNode)
    {
        if (!actionNodes.ContainsKey(actionNode.priority))
        {
            actionNodes.Add(actionNode.priority, actionNode);
        }
        else
        {
            Debug.LogError("重复的actionNode优先级! 忽略添加此actionNode priority : " + actionNode.priority + ", actionNode name : " + actionNode.name);
        }
        return this;
    }

    public bool Execute()
    {
        //for (int i = 0; i < actionNodes.Count; i++)
        //{
        //    if (actionNodes[i].Run(activingRoleGO, allRoleGO, mapGridItems, actionStrategyGeneral))
        //    {
        //        return true;
        //    }
        //}

        foreach(IActionNode an in actionNodes.Values)
        {
            if (an.Run(activingRoleGO, allRoleGO, mapGridItems, actionStrategyGeneral))
            {
                return true;
            }
        }

        Debug.LogWarning("没有子策略被执行");
        return false;
    }

    private class MyActionNodeSort : IComparer<float>
    {
        int IComparer<float>.Compare(float x, float y)
        {
            if (x > y) return 1;
            if (x == y) return 0;
            if (x < y) return -1;
            return 0;
        }
    }

}
