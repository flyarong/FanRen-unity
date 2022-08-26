using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyTriggerToBattleScript : BaseMono, IColliderWithCC
{

    //数据库中角色id
    public int[] roleId;
    //希望该角色在战场的数量
    public int[] countOfRoleId;

    //public string[] rolePrefabPath;

    private void Start()
    {
        ShowOrHideGameObjByUniquePrefenceKey();
    }

    public void OnPlayerCollisionEnter(GameObject player)
    {
        if (player.tag.Equals("Player"))
        {
            Debug.Log("韩立过来了，开始战斗");

            RootBattleInit.enemyRoleIds = roleId;
            RootBattleInit.countOfEnemyRole = countOfRoleId;
            //RootBattleInit.enemyRolePrefabPath = rolePrefabPath;
            RootBattleInit.triggerToBattleGameObjUnionPreKey = this.uniquePrefenceKey;

            SaveUtil.SaveGameObjLastState(player);

            SceneManager.LoadScene(2);
        }
    }

    public void OnPlayerCollisionExit(GameObject player)
    {
        if (player.tag.Equals("Player"))
        {
            Debug.Log("韩立离开了");
        }
    }

}
