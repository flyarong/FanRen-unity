using System.Collections.Generic;
using UnityEngine;

public class ActionStrategyGeneral : ActionStrategy
{

    protected GameObject moveTargetGridItem;
    protected Shentong selectShentong;
    protected GameObject attackMapGrid;
    protected bool isPassAfterMove = false;

    public override void GenerateStrategy(GameObject activingRoleGO, List<GameObject> allRole, GameObject[,] mapGrids)
    {
        GameObject hanLiGO = null;
        List<(int, int)> obstacles = new List<(int, int)>();
        BaseRole activingRole = activingRoleGO.GetComponent<BaseRole>();
        foreach (GameObject item in allRole)
        {
            if (item.tag.Equals("Player"))
            {
                hanLiGO = item;
                continue; //目标不是障碍物
            }
            if (item == activingRoleGO) continue; //自己不是障碍物
            BaseRole itemRole = item.GetComponent<BaseRole>();
            //if (itemRole.teamNum == activingRole.teamNum) //todo 队友是障碍物，非目标敌人也是障碍物
            //{
            //    if (item == activingRoleGO) continue;
            //    obstacles.Add((itemRole.battleOriginPosX, itemRole.battleOriginPosZ));
            //}
            obstacles.Add((itemRole.battleOriginPosX, itemRole.battleOriginPosZ));
        }
        BaseRole hanLiRole = hanLiGO.GetComponent<BaseRole>();
        //BaseRole activingRole = activingRoleGO.GetComponent<BaseRole>();
        AStarPathUtil aStarPathUtil = new AStarPathUtil();
        aStarPathUtil.Reset(mapGrids.GetLength(0), mapGrids.GetLength(1), (activingRole.battleOriginPosX, activingRole.battleOriginPosZ), (hanLiRole.battleOriginPosX, hanLiRole.battleOriginPosZ), obstacles);
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
                this.moveTargetGridItem = mapGrids[maxSpeedCanReachNode.x, maxSpeedCanReachNode.y];
                this.isPassAfterMove = true;
            }
            else //目标距离在攻击范围内
            {
                if (nodes.Count > 0)
                {
                    AStarPathUtil.Node targetNode = nodes[nodes.Count - 1]; //韩立身边的格子，如果可以使用其它神通，那么应该是一个可选的范围
                    this.moveTargetGridItem = mapGrids[targetNode.x, targetNode.y];
                    this.isPassAfterMove = false;
                }
                else if (nodes.Count == 0)
                {
                    this.moveTargetGridItem = mapGrids[activingRole.battleOriginPosX, activingRole.battleOriginPosZ];
                    this.isPassAfterMove = false;
                }
            }
        }
        else
        {
            //todo AI一般策略：无路可走，待在原地，尝试其它攻击方式
            Debug.LogWarning("AI一般策略：无路可走，待在原地，尝试其它攻击方式");
            this.moveTargetGridItem = mapGrids[activingRole.battleOriginPosX, activingRole.battleOriginPosZ];
            this.isPassAfterMove = true;
        }
        
        this.selectShentong = activingRole.shentongInBattle[0];
        this.attackMapGrid = mapGrids[hanLiRole.battleOriginPosX, hanLiRole.battleOriginPosZ];
    }


    public override GameObject GetMoveTargetGridItem()
    {
        return this.moveTargetGridItem;
    }

    public override Shentong GetSelectShentong()
    {
        return this.selectShentong;
    }

    public override GameObject GetAttackMapGridItem()
    {
        return this.attackMapGrid;
    }

    public override bool IsPassAfterMove()
    {
        return this.isPassAfterMove;
    }
}
