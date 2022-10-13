using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 主角Hp低于15%
/// 要考虑的情况实在太多了，估计还是需要使用贪心算法穷举
/// </summary>
public class ActionNodePlayerHPLower15Percent : IActionNode
{
    public ActionNodePlayerHPLower15Percent(float priority, string name = null) : base(priority, name)
    {
    }

    public override bool Run(GameObject activingRoleGO, List<GameObject> allRoleGO, GameObject[,] mapGridItems, ActionStrategyGeneral actionStrategyGeneral)
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
            HanLiScriptInBattle hanLi = hanLiGO.GetComponent<HanLiScriptInBattle>();
            if (hanLi.hp / hanLi.maxHp <= 0.15f) //主角血量低于15%，紧贴主角，全力攻击主角
            {
                Debug.LogWarning("主角hp低于15%，进入贴身全力攻击模型");
                bool isNeedMove;
                AStarPathUtil aStarPathUtil = new AStarPathUtil();
                (int, int) start = (activingRole.battleOriginPosX, activingRole.battleOriginPosZ);
                (int, int) end = (hanLi.battleOriginPosX, hanLi.battleOriginPosZ);
                aStarPathUtil.Reset(mapGridItems.GetLength(0), mapGridItems.GetLength(1), start, end, obstacles);
                List<AStarPathUtil.Node> nodes = aStarPathUtil.GetShortestPath(true);
                if (nodes.Count > activingRole.speed) //距离太远，选择最大移动距离
                {
                    AStarPathUtil.Node maxSpeedNode = nodes[activingRole.speed - 1];
                    actionStrategyGeneral.SetMoveTargetGridItem(mapGridItems[maxSpeedNode.x, maxSpeedNode.y]);
                    isNeedMove = true;
                }
                else
                {
                    if (nodes.Count > 0) //寻路的最后一格
                    {
                        AStarPathUtil.Node lastPathNode = nodes[nodes.Count - 1];
                        actionStrategyGeneral.SetMoveTargetGridItem(mapGridItems[lastPathNode.x, lastPathNode.y]);
                        isNeedMove = true;
                    }
                    else //原地
                    {
                        actionStrategyGeneral.SetMoveTargetGridItem(mapGridItems[activingRole.battleOriginPosX, activingRole.battleOriginPosZ]);
                        isNeedMove = false;
                    }
                }

                //选择伤害最高的神通（todo 其实也不一定要选伤害最高的，mp不足的情况下，也可以选择其它mp消耗较小的，以后再改智能一点吧）
                SortedSet<Shentong> sortST = new SortedSet<Shentong>(new ShentongDamageSort());
                foreach (Shentong shentong in activingRole.shentongInBattle)
                {
                    if(shentong.damage > 0 && shentong.effType == ShentongEffType.Gong_Ji) sortST.Add(shentong);
                }

                //先检测灵力是否足够施法
                activingRole.selectedShentong = null;
                foreach (Shentong st in sortST)
                {
                    //选择一个足够mp施展的神通且伤害最大的且主角在攻击范围内的 todo ...
                    //if (st.needMp <= activingRole.mp && (攻击范围内)) 
                    //{
                    //    actionStrategySmart.SetSelectShentong(st);
                    //    break;
                    //}
                }

                //没有合适的神通，则做最大输出策略或者攻击可以秒杀的敌人或者如果mp太低则优先补充mp(mp低于30%优先补充)，hp低于20%，优先补hp
                if (activingRole.selectedShentong == null)
                {
                    if(activingRole.hp / activingRole.maxHp < 0.2f)
                    {
                        //todo ...
                    }
                    else if (activingRole.mp / activingRole.maxMp < 0.3f)
                    {
                        //补充灵力,可以从物品补充，也可以待机自然补充
                        foreach (RoleItem item in activingRole.roleItems)
                        {
                            //如果是恢复mp道具，则使用
                            if (item.recoverMp > 0)
                            {
                                activingRole.selectRoleItem = item;
                                //actionStrategySmart.SetSelectRoleItem(item); //todo 使用完道具需要set null
                                activingRole.roleItems.Remove(item);
                                break;
                            }
                        }
                        if (activingRole.selectRoleItem == null) //没有恢复mp的道具了，只等靠待机调息恢复mp
                        {
                            if (isNeedMove) //向主角移动后调息
                            {
                                actionStrategyGeneral.SetIsPass(true);
                            }
                            else //已经在主角身边，直接调息
                            {
                                actionStrategyGeneral.SetIsPass(true);
                            }
                        }
                    }
                    else
                    {
                        //todo ... 做最大输出策略或者攻击可以秒杀的敌人
                    }
                }
                else //有合适的攻击神通
                {
                    //todo ...
                    //actionStrategySmart.SetAttackMapGridItem();
                }

                return true;
            }
        }

        return false;
    }

    private class ShentongDamageSort : IComparer<Shentong>
    {
        public int Compare(Shentong x, Shentong y)
        {
            return x.damage - y.damage;
        }
    }

}

/// <summary>
/// 贪婪 穷举 攻击
/// </summary>
public class ActionNodeGreedyAlgorithm : IActionNode
{
    public ActionNodeGreedyAlgorithm(float priority, string name = null) : base(priority, name)
    {
    }

    public override bool Run(GameObject activingRoleGO, List<GameObject> allRoleGO, GameObject[,] mapGridItems, ActionStrategyGeneral actionStrategyGeneral)
    {
        Debug.Log("Run " + this.name);
        BaseRole currentRole = activingRoleGO.GetComponent<BaseRole>();

        //敌人位置
        Dictionary<(int, int), GameObject> position_roleGO = new Dictionary<(int, int), GameObject>();
        foreach (GameObject roleGO in allRoleGO)
        {
            if (roleGO == null || roleGO.IsDestroyed()) continue;
            BaseRole role = roleGO.GetComponent<BaseRole>();
            if (role.teamNum != currentRole.teamNum)
            {
                position_roleGO.Add((role.battleOriginPosX, role.battleOriginPosZ), roleGO);
            }
        }

        Shentong[] roleST = currentRole.shentongInBattle;

        List<GameObject> canMoveMapGrids = currentRole.lastAllCanMoveGrids;

        int maxDamage = 0;

        foreach (GameObject canMoveMapGrid in canMoveMapGrids)
        {
            (int, int) rolePosition = MapUtil.GetPositionFromGridItemGO(canMoveMapGrid);
            foreach(Shentong st in roleST)
            {
                if (st == null) continue;
                if (st.rangeType == ShentongRangeType.Point)
                {
                    int minX = rolePosition.Item1 - st.unitDistance;
                    if (minX < 0) minX = 0;

                    int maxX = rolePosition.Item1 + st.unitDistance;
                    if (maxX > mapGridItems.GetLength(0)-1) maxX = mapGridItems.GetLength(0)-1;

                    int minY = rolePosition.Item2 - st.unitDistance;
                    if (minY < 0) minY = 0;

                    int maxY = rolePosition.Item2 + st.unitDistance;
                    if (maxY > mapGridItems.GetLength(1)-1) maxY = mapGridItems.GetLength(1) - 1;

                    for(int i=minX; i<= maxX; i++)
                    {
                        for (int j = minY; j <= maxY; j++)
                        {
                            if((ManHaDunDistanceTrim(rolePosition, (i, j))+1) <= st.unitDistance) //攻击范围
                            {
                                GameObject enemyGO = position_roleGO.GetValueOrDefault((i, j), null);
                                if (enemyGO != null)//格子上有行动人的敌人
                                {
                                    int damage = currentRole.CalculateDamage(enemyGO.GetComponent<BaseRole>(), st);
                                    if (damage > maxDamage)
                                    {
                                        maxDamage = damage;
                                        actionStrategyGeneral.SetMoveTargetGridItem(canMoveMapGrid); //移动到哪
                                        currentRole.selectedShentong = st;//使用什么神通
                                        actionStrategyGeneral.SetAttackMapGridItem(mapGridItems[i, j]);//攻击哪一格
                                    }
                                }
                            }
                        }
                    }

                }
                else if (st.rangeType == ShentongRangeType.Line)
                {
                    int minX = rolePosition.Item1 - st.unitDistance;
                    if (minX < 0) minX = 0;

                    int maxX = rolePosition.Item1 + st.unitDistance;
                    if (maxX > mapGridItems.GetLength(0) - 1) maxX = mapGridItems.GetLength(0) - 1;

                    int minY = rolePosition.Item2 - st.unitDistance;
                    if (minY < 0) minY = 0;

                    int maxY = rolePosition.Item2 + st.unitDistance;
                    if (maxY > mapGridItems.GetLength(1) - 1) maxY = mapGridItems.GetLength(1) - 1;

                    int thisGroupTotalDamage = 0;
                    for(int i= minX; i< rolePosition.Item1; i++)
                    {
                        GameObject enemyGO = position_roleGO.GetValueOrDefault((i, rolePosition.Item2), null);
                        if(enemyGO != null)
                        {
                            thisGroupTotalDamage += currentRole.CalculateDamage(enemyGO.GetComponent<BaseRole>(), st);
                        }
                    }
                    if(thisGroupTotalDamage > maxDamage)
                    {
                        maxDamage = thisGroupTotalDamage;
                        actionStrategyGeneral.SetMoveTargetGridItem(canMoveMapGrid); //移动到哪
                        currentRole.selectedShentong = st;//使用什么神通
                        actionStrategyGeneral.SetAttackMapGridItem(mapGridItems[rolePosition.Item1-1, rolePosition.Item2]);//攻击哪一格
                    }

                    thisGroupTotalDamage = 0;
                    for (int i = rolePosition.Item1+1; i <= maxX; i++)
                    {
                        GameObject enemyGO = position_roleGO.GetValueOrDefault((i, rolePosition.Item2), null);
                        if (enemyGO != null)
                        {
                            thisGroupTotalDamage += currentRole.CalculateDamage(enemyGO.GetComponent<BaseRole>(), st);
                        }
                    }
                    if (thisGroupTotalDamage > maxDamage)
                    {
                        maxDamage = thisGroupTotalDamage;
                        actionStrategyGeneral.SetMoveTargetGridItem(canMoveMapGrid); //移动到哪
                        currentRole.selectedShentong = st;//使用什么神通
                        actionStrategyGeneral.SetAttackMapGridItem(mapGridItems[rolePosition.Item1 + 1, rolePosition.Item2]);//攻击哪一格
                    }

                    thisGroupTotalDamage = 0;
                    for (int i = minY; i < rolePosition.Item2; i++)
                    {
                        GameObject enemyGO = position_roleGO.GetValueOrDefault((rolePosition.Item1, i), null);
                        if (enemyGO != null)
                        {
                            thisGroupTotalDamage += currentRole.CalculateDamage(enemyGO.GetComponent<BaseRole>(), st);
                        }
                    }
                    if (thisGroupTotalDamage > maxDamage)
                    {
                        maxDamage = thisGroupTotalDamage;
                        actionStrategyGeneral.SetMoveTargetGridItem(canMoveMapGrid); //移动到哪
                        currentRole.selectedShentong = st;//使用什么神通
                        actionStrategyGeneral.SetAttackMapGridItem(mapGridItems[rolePosition.Item1, rolePosition.Item2 - 1]);//攻击哪一格
                    }

                    thisGroupTotalDamage = 0;
                    for (int i = rolePosition.Item2 + 1; i <= maxY; i++)
                    {
                        GameObject enemyGO = position_roleGO.GetValueOrDefault((rolePosition.Item1, i), null);
                        if (enemyGO != null)
                        {
                            thisGroupTotalDamage += currentRole.CalculateDamage(enemyGO.GetComponent<BaseRole>(), st);
                        }
                    }
                    if (thisGroupTotalDamage > maxDamage)
                    {
                        maxDamage = thisGroupTotalDamage;
                        actionStrategyGeneral.SetMoveTargetGridItem(canMoveMapGrid); //移动到哪
                        currentRole.selectedShentong = st;//使用什么神通
                        actionStrategyGeneral.SetAttackMapGridItem(mapGridItems[rolePosition.Item1, rolePosition.Item2 + 1]);//攻击哪一格
                    }

                }
                else if (st.rangeType == ShentongRangeType.Plane)
                {
                    int minX = rolePosition.Item1 - st.unitDistance;
                    if (minX < 0) minX = 0;

                    int maxX = rolePosition.Item1 + st.unitDistance;
                    if (maxX > mapGridItems.GetLength(0) - 1) maxX = mapGridItems.GetLength(0) - 1;

                    int minY = rolePosition.Item2 - st.unitDistance;
                    if (minY < 0) minY = 0;

                    int maxY = rolePosition.Item2 + st.unitDistance;
                    if (maxY > mapGridItems.GetLength(1) - 1) maxY = mapGridItems.GetLength(1) - 1;

                    int thisGroupTotalDamage = 0;

                    for (int i = minX; i <= maxX; i++)
                    {
                        for (int j = minY; j <= maxY; j++)
                        {
                            if ((ManHaDunDistanceTrim(rolePosition, (i, j)) + 1) <= st.unitDistance) //鼠标点击范围
                            {
                                (int, int) attackCore = (i, j);

                                int minX2 = attackCore.Item1 - st.planeRadius;
                                if (minX2 < 0) minX2 = 0;

                                int maxX2 = attackCore.Item1 + st.planeRadius;
                                if (maxX2 > mapGridItems.GetLength(0) - 1) maxX2 = mapGridItems.GetLength(0) - 1;

                                int minY2 = attackCore.Item2 - st.planeRadius;
                                if (minY2 < 0) minY2 = 0;

                                int maxY2 = attackCore.Item2 + st.planeRadius;
                                if (maxY2 > mapGridItems.GetLength(1) - 1) maxY2 = mapGridItems.GetLength(1) - 1;

                                for (int m = minX2; m <= maxX2; m++)
                                {
                                    for (int n = minY2; n <= maxY2; n++)
                                    {
                                        if ((ManHaDunDistanceTrim(attackCore, (m, n)) + 1) <= st.planeRadius) //伤害范围
                                        {
                                            GameObject enemyGO = position_roleGO.GetValueOrDefault((m, n), null);
                                            if (enemyGO != null)//格子上有行动人的敌人
                                            {
                                                thisGroupTotalDamage += currentRole.CalculateDamage(enemyGO.GetComponent<BaseRole>(), st);
                                            }
                                        }
                                    }
                                }

                                if(thisGroupTotalDamage > maxDamage)
                                {
                                    maxDamage = thisGroupTotalDamage;
                                    actionStrategyGeneral.SetMoveTargetGridItem(canMoveMapGrid); //移动到哪
                                    currentRole.selectedShentong = st;//使用什么神通
                                    actionStrategyGeneral.SetAttackMapGridItem(mapGridItems[i, j]);//攻击哪一格
                                }
                                thisGroupTotalDamage = 0;

                            }
                        }
                    }
                }
            }
        }

        if(maxDamage > 0)
        {
            actionStrategyGeneral.SetIsPass(false);
            return true;
        }
        else
        {
            return false;
        }
        
    }
}

public class ActionNodeMpNotEnough : IActionNode
{
    public ActionNodeMpNotEnough(float priority, string name = null) : base(priority, name)
    {
    }

    public override bool Run(GameObject activingRoleGO, 
        List<GameObject> allRoleGO, 
        GameObject[,] mapGridItems,
        ActionStrategyGeneral actionStrategyGeneral)
    {
        Debug.Log("Run " + this.name);
        BaseRole activingRole = activingRoleGO.GetComponent<BaseRole>();

        //这个策略是给非主角方NPC使用的
        if (activingRole.teamNum != TeamNum.TEAM_TWO) {
            Debug.LogError("这个节点策略只能用在TeamNum.TEAM_TWO身上");
            return false;
        }

        HanLiScriptInBattle hanLiScriptInBattle = GetHanLi(allRoleGO);

        foreach (Shentong st in activingRole.shentongInBattle)
        {
            if (st == null) continue;
            //if ((((float)hanLiScriptInBattle.hp) / ((float)hanLiScriptInBattle.maxHp)) > 0.1f && st.shenTongId == 1) //主角血量在10%以上时候，不要用普通攻击(只有普通攻击呢？)
            //{
            //    continue;
            //}
            if (st.needMp <= activingRole.mp && st.effType == ShentongEffType.Gong_Ji)
            {
                //有足够的mp使用攻击神通
                return false;
            }
        }

        //无攻击神通可用，使用道具或者调息补充mp
        //如果主角血量在10%以下，向主角移动，否则远离主角
        if(((float)hanLiScriptInBattle.hp / (float)hanLiScriptInBattle.maxHp) < 0.1f) //向主角移动
        {
            Debug.LogWarning("主角hp低于10%，会贴身主角");
            AStarPathUtil aStarPathUtil = new AStarPathUtil();
            (int, int) start = (activingRole.battleOriginPosX, activingRole.battleOriginPosZ);
            (int, int) end = (hanLiScriptInBattle.battleOriginPosX, hanLiScriptInBattle.battleOriginPosZ);
            aStarPathUtil.Reset(mapGridItems.GetLength(0), mapGridItems.GetLength(1), start, end, GetObstacles(allRoleGO, activingRoleGO, hanLiScriptInBattle.gameObject));
            List<AStarPathUtil.Node> nodes = aStarPathUtil.GetShortestPath(true);
            if (nodes.Count > activingRole.speed) //距离太远，选择最大移动距离
            {
                AStarPathUtil.Node maxSpeedNode = nodes[activingRole.speed - 1];
                actionStrategyGeneral.SetMoveTargetGridItem(mapGridItems[maxSpeedNode.x, maxSpeedNode.y]);
            }
            else
            {
                if (nodes.Count > 0) //寻路的最后一格
                {
                    AStarPathUtil.Node lastPathNode = nodes[nodes.Count - 1];
                    actionStrategyGeneral.SetMoveTargetGridItem(mapGridItems[lastPathNode.x, lastPathNode.y]);
                }
                else //留在原地
                {
                    actionStrategyGeneral.SetMoveTargetGridItem(mapGridItems[activingRole.battleOriginPosX, activingRole.battleOriginPosZ]);
                }
            }
        }
        else //远离主角
        {
            Debug.LogWarning("远离主角行动");
            int maxDistance = 0;
            GameObject targetMapGridItem = null;
            foreach(GameObject canMoveMapGridItem in activingRole.lastAllCanMoveGrids)
            {
                int distance = ManHaDunDistanceTrim(MapUtil.GetPositionFromGridItemGO(canMoveMapGridItem), hanLiScriptInBattle.battleOriginPosition);
                if (distance > maxDistance) 
                {
                    maxDistance = distance;
                    targetMapGridItem = canMoveMapGridItem;
                }
            }
            actionStrategyGeneral.SetMoveTargetGridItem(targetMapGridItem);
        }

        foreach(RoleItem item in activingRole.roleItems) //挑选道具恢复mp
        {
            if(item.recoverMp > 0)
            {
                activingRole.selectRoleItem = item;
                break;
            }
        }

        if(activingRole.selectRoleItem == null) //无道具可用->调息
        {
            actionStrategyGeneral.SetIsPass(true);
        }

        Debug.Log("执行了ActionNodeMpNotEnough");
        return true;
    }
}