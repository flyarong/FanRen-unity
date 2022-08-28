using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

public class BaseRole : BaseMono
{
    //todo 攻击完毕后，需要更新 battleOriginPosX = battleToPosX
    public int battleOriginPosX;
    //todo 攻击完毕后，需要更新 battleOriginPosZ = battleToPosZ
    public int battleOriginPosZ;

    public int battleToPosX;
    public int battleToPosZ;

    //敌人或者友军身上携带的物品
    public List<RoleItem> roleItems = new List<RoleItem>();

    public int BattleToPosXWillOriginPosXIfNone
    {
        set {
            battleToPosX = value;
        }
        get {
            if (battleToPosX == -1) return battleOriginPosX;
            return battleToPosX;
        }
    }

    public int BattleToPosZWillOriginPosZIfNone
    {
        set
        {
            battleToPosZ = value;
        }
        get
        {
            if (battleToPosZ == -1) return battleOriginPosZ;
            return battleToPosZ;
        }
    }

    //角色唯一id,数据库id
    public int roleId;

    //哪个团队 1我方， 2敌方
    public TeamNum teamNum;

    public int maxHp;
    public int maxMp;
    public int hp;
    public int mp;
    public int gongJiLi;
    public int fangYuLi;
    public int speed;
    public string roleName;
    public string roleAvatar;
    
    public Shentong[] shentongInBattle;

    public Shentong selectedShentong;

    //private Animator animator;




    public delegate void RoleHitAnimDelegate();

    private RoleHitAnimDelegate mRoleHitAnimDelegate;




    private RoleHitAnimListener mRoleHitAnimListener;

    public virtual void StartRoleHitAnim(RoleHitAnimDelegate roleHitAnimDelegate)
    {
        Debug.Log("StartHitAnim");
        mRoleHitAnimDelegate = roleHitAnimDelegate;
        GetComponent<Animator>().SetBool("isAttack", true);
    }

    public virtual void StartRoleHitAnim(RoleHitAnimListener roleHitAnimListener)
    {
        Debug.Log("StartHitAnim");
        mRoleHitAnimListener = roleHitAnimListener;
        GetComponent<Animator>().SetBool("isAttack", true);
    }

    //hit anim end call back
    public virtual void EndRoleHitAnim()
    {
        Debug.Log("EndHitAnim");
        GetComponent<Animator>().SetBool("isAttack", false);
        if (mRoleHitAnimListener != null) {
            mRoleHitAnimListener.OnEndRoleHitAnim();
            mRoleHitAnimListener = null;
        }
        if (mRoleHitAnimDelegate != null)
        {
            mRoleHitAnimDelegate();
            mRoleHitAnimDelegate = null;
        }
    }

    

    //状态机攻击动画事件结束回调 
    public void Hit()
    {
        EndRoleHitAnim();
    }

    private void Awake()
    {
        //animator = this.GetComponent<Animator>();
    }

    //public GameObject sliderPrefab;

    public RoleInBattleStatus roleInBattleStatus = RoleInBattleStatus.Waiting;

    //GameObject uiParent;
    //GameObject damageTextPrefab;

    public GameObject hpGO = null;
    public GameObject sliderAvatarGO = null;

    private void OnDestroy()
    {
        Destroy(hpGO);
        Destroy(sliderAvatarGO);
    }

    public void InitRoleBattelePos(int startX, int startZ)
    {
        this.battleOriginPosX = startX;
        this.battleOriginPosZ = startZ;
        this.battleToPosX = -1;
        this.battleToPosZ = -1;
        this.gameObject.transform.position = new Vector3(startX + 0.5f, 0, startZ + 0.5f);
    }

    public void InitRoleData(int hp, int maxHp, int mp, int maxMp, int gongJiLi, int fangYuLi, Shentong[] shentongInBattle, int speed, int roleId, TeamNum teamNum, string roleName, string gameObjName, string roleAvatar)
    {
        this.hp = hp;
        this.maxHp = maxHp;
        this.mp = mp;
        this.maxMp = maxMp;
        this.gongJiLi = gongJiLi;
        this.fangYuLi = fangYuLi;
        this.shentongInBattle = shentongInBattle;        
        this.speed = speed;
        this.roleId = roleId;
        this.teamNum = teamNum;
        this.roleName = roleName;
        this.name = gameObjName;
        this.roleAvatar = roleAvatar;

        gameObjectType = GameObjectType.Role;

        //uiParent = GameObject.FindGameObjectWithTag("UI_Canvas");
        //damageTextPrefab = Resources.Load<GameObject>("Prefab/TextDamage");
    }

    public void UpdateHP(int damage)
    {
        this.hp -= damage;
        Debug.Log(this.name + "更新血条 enemy.maxHp " + this.maxHp + ", enemy.hp " + this.hp);
        Slider enemySlide = this.hpGO.GetComponent<Slider>();
        enemySlide.maxValue = this.maxHp;
        enemySlide.minValue = 0;
        enemySlide.value = this.hp;
    }

    public int GetMoveDistanceInBattle()
    {
        //todo 速度转换成移动格子数，公式待定
        return this.speed;
    }

    public void OnSelectShentong(int index)
    {
        
        if (this.shentongInBattle[index].needMp <= this.mp)
        {
            this.selectedShentong = this.shentongInBattle[index];
            
            BattleController battleController = GameObject.FindGameObjectWithTag("Terrain").GetComponent<BattleController>();
            battleController.OnRoleSelectedShentong(this.selectedShentong);
        }
        else
        {
            //todo UI提示灵力不足
            Debug.LogError("灵力不足");
        }
    }

    public void DoCancelShentong()
    {
        this.selectedShentong = null;
    }

    //返回是否死了
    public bool DoAck(BaseRole enemy)
    {
        Debug.Log("DoAck");
        //todo 公式待定
        int damage = this.gongJiLi + this.selectedShentong.damage - enemy.fangYuLi;

        if (damage <= 0) damage = 1;

        GameObject uiParent = GameObject.FindGameObjectWithTag("UI_Canvas");
        uiParent.GetComponent<BattleUIControl>().ShowDamageTextUI(damage, enemy.gameObject);

        if (enemy.hp > damage)
        {
            this.mp -= this.selectedShentong.needMp;
            enemy.UpdateHP(damage);
            return false;
        }
        else
        {
            GameObject.Destroy(enemy.gameObject);
            //die
            return true;
        }
    }

    private ActionStrategy mActionStrategy;

    /// <summary>
    /// 设置战场敌人的行为策略
    /// </summary>
    public void SetActionStrategy(ActionStrategy actionStrategy)
    {
        this.mActionStrategy = actionStrategy;
    }

    public ActionStrategy GetActionStrategy()
    {
        return this.mActionStrategy;
    }

}

public enum RoleInBattleStatus
{
    Activing = 1,
    Waiting = 2
}

public enum TeamNum
{
    //Us = 1,
    //Enemy = 2
    TEAM_ONE = 1,
    TEAM_TWO = 2
}

public interface RoleHitAnimListener
{
    public void OnEndRoleHitAnim();
}

public abstract class ActionStrategy
{

    //protected GameObject moveTargetGridItem;
    //protected Shentong selectShentong;
    //protected GameObject attackMapGrid;

    public abstract void GenerateStrategy(GameObject activingRoleGO, List<GameObject> allRoleGO, GameObject[,] mapGridItems, List<GameObject> allCanMoveGridItems);

    public abstract GameObject GetMoveTargetGridItem();

    public abstract Shentong GetSelectShentong();

    public abstract GameObject GetAttackMapGridItem();

    public abstract bool IsPassAfterMove();

    public abstract bool IsPass();
}