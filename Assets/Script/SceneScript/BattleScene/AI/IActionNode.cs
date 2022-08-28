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
    public abstract bool Run(GameObject activingRoleGO, List<GameObject> allRoleGO, GameObject[,] mapGridItems, List<GameObject> allCanMoveGridItems, ActionStrategySmart actionStrategySmart);
}
