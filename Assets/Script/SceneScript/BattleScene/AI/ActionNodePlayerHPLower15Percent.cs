using System.Collections.Generic;
using UnityEngine;

public class ActionNodePlayerHPLower15Percent : IActionNode
{
    public ActionNodePlayerHPLower15Percent(float priority, string name = null) : base(priority, name)
    {
    }

    public override bool Run(GameObject activingRoleGO, List<GameObject> allRoleGO, GameObject[,] mapGridItems, List<GameObject> allCanMoveGridItems, ActionStrategySmart actionStrategySmart)
    {
        BaseRole activingRole = activingRoleGO.GetComponent<BaseRole>();
        if (activingRole.teamNum == TeamNum.TEAM_TWO) //敌人，目标击败主角
        {
            List<(int, int)> obstacles = new List<(int, int)>();
            GameObject hanLiGO = null;
            foreach (GameObject roleGO in allRoleGO)
            {
                if (roleGO.tag.Equals("Player"))
                {
                    hanLiGO = roleGO;
                    continue;
                }
                if (roleGO == activingRoleGO) continue;
                BaseRole role = roleGO.GetComponent<BaseRole>();
                obstacles.Add((role.battleOriginPosX, role.battleOriginPosZ));
            }
            HanLi hanLi = hanLiGO.GetComponent<HanLi>();
            if (hanLi.hp / hanLi.maxHp <= 0.15f) //主角血量低于15%，紧贴主角，全力攻击主角
            {
                Debug.LogWarning("主角hp低于15%，进入贴身全力攻击模型");
                AStarPathUtil aStarPathUtil = new AStarPathUtil();
                (int, int) start = (activingRole.battleOriginPosX, activingRole.battleOriginPosZ);
                (int, int) end = (hanLi.battleOriginPosX, hanLi.battleOriginPosZ);
                aStarPathUtil.Reset(mapGridItems.GetLength(0), mapGridItems.GetLength(1), start, end, obstacles);
                List<AStarPathUtil.Node> nodes = aStarPathUtil.GetShortestPath(true);
                if (nodes.Count > activingRole.speed) //距离太远，选择最大移动距离
                {
                    AStarPathUtil.Node maxSpeedNode = nodes[activingRole.speed - 1];
                    actionStrategySmart.SetMoveTargetGridItem(mapGridItems[maxSpeedNode.x, maxSpeedNode.y]);
                }
                else
                {
                    if (nodes.Count > 0) //寻路的最后一格
                    {
                        AStarPathUtil.Node lastPathNode = nodes[nodes.Count - 1];
                        actionStrategySmart.SetMoveTargetGridItem(mapGridItems[lastPathNode.x, lastPathNode.y]);
                    }
                    else //原地
                    {
                        actionStrategySmart.SetMoveTargetGridItem(mapGridItems[activingRole.battleOriginPosX, activingRole.battleOriginPosZ]);
                    }
                }

                //选择伤害最高的神通
                Shentong maxDamageShentong = activingRole.shentongInBattle[0];
                foreach (Shentong shentong in activingRole.shentongInBattle)
                {
                    if (shentong.effType == ShentongEffType.Gong_Ji)
                    {
                        if (shentong.damage > maxDamageShentong.damage)
                        {
                            maxDamageShentong = shentong;
                        }
                    }
                }

                //先检测灵力是否足够施法
                if (activingRole.mp >= maxDamageShentong.needMp)
                {
                    actionStrategySmart.SetSelectShentong(maxDamageShentong);
                }
                else //todo 补充灵力,可以从物品补充，也可以待机自然补充
                {
                    actionStrategySmart.SetSelectShentong(null);

                }

                return true;
            }
        }

        return false;
    }
}

public class ActionNodeMaxTotalDamage : IActionNode
{
    public ActionNodeMaxTotalDamage(float priority, string name = null) : base(priority, name)
    {
    }

    public override bool Run(GameObject activingRoleGO, List<GameObject> allRoleGO, GameObject[,] mapGridItems, List<GameObject> allCanMoveGridItems, ActionStrategySmart actionStrategySmart)
    {
        throw new System.NotImplementedException();
    }
}

public class ActionNodeAttackShortestDistance: IActionNode
{
    public ActionNodeAttackShortestDistance(float priority, string name = null) : base(priority, name)
    {
    }

    public override bool Run(GameObject activingRoleGO, List<GameObject> allRoleGO, GameObject[,] mapGridItems, List<GameObject> allCanMoveGridItems, ActionStrategySmart actionStrategySmart)
    {
        throw new System.NotImplementedException();
    }
}