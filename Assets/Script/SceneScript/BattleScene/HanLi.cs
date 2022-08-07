using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HanLi : BaseRole
{

    //todo 要从数据库查询出装备了哪些神通
    public void Init()
    {
        MyDBManager.GetInstance().ConnDB();
        List<Shentong> shenTongList = MyDBManager.GetInstance().GetRoleShentong(1, 1);

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
