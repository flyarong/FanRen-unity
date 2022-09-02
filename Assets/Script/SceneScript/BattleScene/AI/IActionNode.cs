using System.Collections.Generic;
using UnityEngine;

public abstract class IActionNode
{
    /// <summary>
    /// 唯一，子策略优先级，后续重复不会被添加
    /// </summary>
    public float priority;
    //给此子策略添加名字，非必填，只用来调试
    public string name;
    public IActionNode(float priority, string name = null)
    {
        this.priority = priority;
        this.name = name;
    }

    protected HanLiScriptInBattle GetHanLi(List<GameObject> allRoleGO)
    {
        foreach(GameObject roleGO in allRoleGO)
        {
            if (roleGO == null || !roleGO.activeInHierarchy || !roleGO.activeSelf) continue;
            if (roleGO.tag.Equals("Player"))
            {
                return roleGO.GetComponent<HanLiScriptInBattle>();
            }
        }
        return null;
    }

    protected int ManHaDunDistanceTrim((int, int) p1, (int, int) p2)
    {
        return Mathf.Abs(p1.Item1-p2.Item1) + Mathf.Abs(p1.Item2 - p2.Item2) - 1;
    }

    /// <summary>
    /// 获取障碍物，目前障碍物暂时只有主角，以后可能会添加其它石头树木之类
    /// </summary>
    /// <param name="allRoleGO"></param>
    /// <param name="activitingRoleGO"></param>
    /// <param name="targetRoleGO"></param>
    /// <returns></returns>
    protected List<(int, int)> GetObstacles(List<GameObject> allRoleGO, GameObject activitingRoleGO, GameObject targetRoleGO)
    {
        List<(int, int)> obstacles = new List<(int, int)>();
        foreach (GameObject roleGO in allRoleGO)
        {
            if (roleGO == null || !roleGO.activeInHierarchy || !roleGO.activeSelf) continue;
            if (roleGO == activitingRoleGO || roleGO == targetRoleGO)
            {
                continue;
            }
            BaseRole role = roleGO.GetComponent<BaseRole>();
            obstacles.Add((role.battleOriginPosX, role.battleOriginPosZ));
        }
        return obstacles;
    }

    public abstract bool Run(GameObject activingRoleGO, List<GameObject> allRoleGO, GameObject[,] mapGridItems, ActionStrategySmart actionStrategySmart);
}
