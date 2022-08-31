using System.Collections.Generic;
using UnityEngine;

public class ActionStrategySmart : ActionStrategyGeneral
{
    //攻击一般原则，总输出最大
    //主色血量少例如10%以下，可以一招干掉， 优先级最高，9999 (向主角移动，一路上有可以攻击的目标会顺手攻击，如果灵力不足，会优先补充灵力)
    //敌人血量都健康的状态下，向主角移动
    //一般情况下优先攻击最近可攻击目标，如果有主角在内，优先攻击主角，
    //优先选择伤害最高且主角在射程范围内的神通
    //攻击范围内有机会抹杀目标的情况下，会更加优先，在前面的前提下，会尽可能同时攻击多个目标
    //法修没有灵力会优先补给，体修则不需要
    public override void GenerateStrategy(GameObject activingRoleGO, List<GameObject> allRoleGO, GameObject[,] mapGridItems, List<GameObject> allCanMoveGridItems)
    {
        new ActionNodeManager(activingRoleGO, allRoleGO, mapGridItems, allCanMoveGridItems, this)
           // .AddActionNode(new ActionNodePlayerHPLower15Percent(99.0f, "执行策略：主角hp_lower_15%"))
           // .AddActionNode(new ActionNodeMaxTotalDamage(98.0f, "执行策略：总伤害最大化"))
           // .AddActionNode(new ActionNodeAttackShortestDistance(97.0f, "执行策略：攻击最近的敌人"))
           .AddActionNode(new ActionNodeMpNotEnough(99.0f, "判断mp"))
           .AddActionNode(new ActionNodeGreedyAlgorithm(98.0f, "执行策略：贪心算法，穷举"))
           .Execute();
    }

}
