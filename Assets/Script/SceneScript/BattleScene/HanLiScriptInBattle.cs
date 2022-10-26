using System.Collections.Generic;
using UnityEngine;

public class HanLiScriptInBattle : BaseRole
{

    //todo 要从数据库查询出装备了哪些神通
    public void Init()
    {
        MyDBManager.GetInstance().ConnDB();


        if (PlayerControl.IS_DEBUG)
        {
            Debug.LogWarning("初始化游戏数据，测试需要放在这里");
            MyDBManager.GetInstance().ConnDB();
            RoleInfo roleInfoHanLi = MyDBManager.GetInstance().GetRoleInfo(1);
            if (roleInfoHanLi == null) MyDBManager.GetInstance().InsertRoleInfo(1);
        }

        List<Shentong> shenTongList = MyDBManager.GetInstance().GetRoleShentong(1, 1, true);

        if (PlayerControl.IS_DEBUG && shenTongList.Count == 0)
        {
            Debug.LogWarning("HanLi Init() debug模式，主角默认学会三个神通");
            MyDBManager.GetInstance().ZhujueLearnShentong(1, 1);
            MyDBManager.GetInstance().ZhujueLearnShentong(2, 1);
            MyDBManager.GetInstance().ZhujueLearnShentong(3, 1);
            shenTongList = MyDBManager.GetInstance().GetRoleShentong(1, 1, true);
        }

        for (int i = 1; i <= 33; i++)
        {
            //Debug.Log("i " + i);
            MyDBManager.GetInstance().AddItemToBag(i, 1);
        }

        Shentong[] tmp = new Shentong[12];
        for(int i=0; i< shenTongList.Count; i++)
        {
            tmp[i] = shenTongList[i];
        }

        RoleInfo roleInfo = MyDBManager.GetInstance().GetRoleInfo(1);

        //InitRoleData(roleInfo.currentHp, roleInfo.maxHp, roleInfo.currentMp, roleInfo.maxMp, roleInfo.gongJiLi, roleInfo.fangYuLi, tmp, roleInfo.speed, roleInfo.roleId, TeamNum.TEAM_ONE, roleInfo.roleName, roleInfo.roleName);
        InitRoleData(roleInfo.currentHp, roleInfo.maxHp, roleInfo.currentMp, roleInfo.maxMp, roleInfo.gongJiLi, roleInfo.fangYuLi, tmp, roleInfo.speed, roleInfo.roleId, TeamNum.TEAM_ONE, roleInfo.roleName, roleInfo.roleName, roleInfo.roleAvatar);

    }

    //状态机攻击动画事件结束回调 
    //public void Hit()
    //{
    //    base.EndRoleHitAnim();
    //}

    //hit anim end
    //public void Hit()
    //{
    //    Debug.Log("Hit");
    //    GetComponent<Animator>().SetBool("isAttack", false);
    //}

}
