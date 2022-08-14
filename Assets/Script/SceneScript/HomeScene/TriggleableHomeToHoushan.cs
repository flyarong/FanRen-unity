using UnityEngine;

public class TriggleableHomeToHoushan : IExitCondition
{
    public override bool IsTriggerable()
    {
        MyDBManager.GetInstance().ConnDB();
        if (MyDBManager.GetInstance().GetTriggedRoleTask(1) != null && MyDBManager.GetInstance().GetTriggedRoleTask(2) != null)
        {
            return true;
        }
        return false;
    }
}
