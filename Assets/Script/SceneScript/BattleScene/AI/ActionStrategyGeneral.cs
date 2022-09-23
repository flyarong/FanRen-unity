using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 此策略每次只会走到离主角最近的位置使用普通攻击
/// </summary>
public class ActionStrategyGeneral : ActionStrategy
{

    //本回合走到哪一格
    private GameObject moveTargetGridItem;
    //攻击哪个格子
    private GameObject attackMapGrid;
    //本回合是否待机, 被冰冻等，不可行动
    private bool isPass = false;
    //选择了使用道具
    //private RoleItem selectRoleItem;

    /// <summary>
    /// 继承本类，重写这个方法让NPC实现新的战斗策略，战场初始化的时候赋给角色即可
    /// 
    /// </summary>
    /// <param name="activingRoleGO">角色的回合</param>
    /// <param name="allRole">全部角色战场信息</param>
    /// <param name="mapGrids">全部地图格子信息</param>
    public override void GenerateStrategy(GameObject activingRoleGO, List<GameObject> allRoleGO, GameObject[,] mapGridItems)
    {
        GameObject hanLiGO = null;
        List<(int, int)> obstacles = new List<(int, int)>();
        BaseRole activingRole = activingRoleGO.GetComponent<BaseRole>();
        foreach (GameObject item in allRoleGO)
        {
            if (item == null || !item.activeInHierarchy || !item.activeSelf) continue;
            if (item.tag.Equals("Player"))
            {
                hanLiGO = item;
                continue; //目标不是障碍物
            }
            if (item == activingRoleGO) continue; //自己不是障碍物
            BaseRole itemRole = item.GetComponent<BaseRole>();
            obstacles.Add((itemRole.battleOriginPosX, itemRole.battleOriginPosZ));
        }
        BaseRole hanLiRole = hanLiGO.GetComponent<BaseRole>();
        //BaseRole activingRole = activingRoleGO.GetComponent<BaseRole>();
        AStarPathUtil aStarPathUtil = new AStarPathUtil();
        aStarPathUtil.Reset(mapGridItems.GetLength(0), mapGridItems.GetLength(1), (activingRole.battleOriginPosX, activingRole.battleOriginPosZ), (hanLiRole.battleOriginPosX, hanLiRole.battleOriginPosZ), obstacles);
        List<AStarPathUtil.Node> nodes = aStarPathUtil.GetShortestPath(true);
        
        if(nodes != null)
        {
            //测试，直奔主角身边，用普通攻击，攻击距离为1

            //两者之间的格数 + 1
            int targetDistance = nodes.Count + 1;
            //可以攻击的最远距离
            int attackDistance = activingRole.speed + activingRole.shentongInBattle[0].unitDistance;
            if (targetDistance > attackDistance) //目标距离超过(移动+攻击)距离，选择最大的可移动距离
            {
                AStarPathUtil.Node maxSpeedCanReachNode = nodes[activingRole.speed-1];
                this.moveTargetGridItem = mapGridItems[maxSpeedCanReachNode.x, maxSpeedCanReachNode.y];
                this.isPass = true;
            }
            else //目标距离在攻击范围内
            {
                if (nodes.Count > 0)
                {
                    AStarPathUtil.Node targetNode = nodes[nodes.Count - 1]; //韩立身边的格子，如果可以使用其它神通，那么应该是一个可选的范围
                    this.moveTargetGridItem = mapGridItems[targetNode.x, targetNode.y];
                    this.isPass = false;
                }
                else if (nodes.Count == 0)
                {
                    this.moveTargetGridItem = mapGridItems[activingRole.battleOriginPosX, activingRole.battleOriginPosZ];
                    this.isPass = false;
                }
            }
        }
        else
        {
            //todo AI一般策略：无路可走，待在原地，尝试其它攻击方式
            Debug.LogWarning("AI一般策略：无路可走，待在原地，尝试其它攻击方式");
            this.moveTargetGridItem = mapGridItems[activingRole.battleOriginPosX, activingRole.battleOriginPosZ];
            this.isPass = true;
        }
        
        activingRole.selectedShentong = activingRole.shentongInBattle[0];
        this.attackMapGrid = mapGridItems[hanLiRole.battleOriginPosX, hanLiRole.battleOriginPosZ];
    }


    public override GameObject GetMoveTargetGridItem()
    {
        return this.moveTargetGridItem;
    }

    public void SetMoveTargetGridItem(GameObject moveTargetGridItem)
    {
        this.moveTargetGridItem = moveTargetGridItem;
    }

    public override GameObject GetAttackMapGridItem()
    {
        return this.attackMapGrid;
    }

    public void SetAttackMapGridItem(GameObject attackMapGrid)
    {
        this.attackMapGrid = attackMapGrid;
    }

    public override bool IsPass()
    {
        return this.isPass;
    }

    public void SetIsPass(bool isPass)
    {
        this.isPass = isPass;
    }

}