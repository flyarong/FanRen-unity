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
            }
            BaseRole itemRole = item.GetComponent<BaseRole>();
            if (itemRole.teamNum == activingRole.teamNum) //队友是障碍物
            {
                if (item == activingRoleGO) continue;
                obstacles.Add((itemRole.battleOriginPosX, itemRole.battleOriginPosZ));
            }
        }
        BaseRole hanLiRole = hanLiGO.GetComponent<BaseRole>();
        //BaseRole activingRole = activingRoleGO.GetComponent<BaseRole>();
        AStarPathUtil aStarPathUtil = new AStarPathUtil();
        aStarPathUtil.Reset(mapGrids.GetLength(0), mapGrids.GetLength(1), (activingRole.battleOriginPosX, activingRole.battleOriginPosZ), (hanLiRole.battleOriginPosX, hanLiRole.battleOriginPosZ), obstacles);
        List<AStarPathUtil.Node> nodes = aStarPathUtil.GetShortestPath();
        AStarPathUtil.Node targetNode = nodes[nodes.Count-1];

        //测试，直奔主角身边，用神通0攻击
        int targetDistance = Mathf.Abs(targetNode.x - activingRole.battleOriginPosX) + Mathf.Abs(targetNode.y - activingRole.battleOriginPosZ);
        if(targetDistance > activingRole.speed) //目标距离超过移动距离，选择最大的可移动距离
        {
            foreach(AStarPathUtil.Node node in nodes)
            {
                if((Mathf.Abs(node.x - activingRole.battleOriginPosX) + Mathf.Abs(node.y - activingRole.battleOriginPosZ)) == activingRole.speed)
                {
                    this.moveTargetGridItem = mapGrids[node.x, node.y];
                    break;
                }
            }
            this.isPassAfterMove = true;
        }
        else //目标距离在移动距离范围内
        {
            this.moveTargetGridItem = mapGrids[targetNode.x, targetNode.y];
            this.isPassAfterMove = false;
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
