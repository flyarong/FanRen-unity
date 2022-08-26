using System.Collections.Generic;
using UnityEngine;

public class RootBattleInit : BaseMono
{

    public List<GameObject> roles;

    public static int[] enemyRoleIds; //从数据库查询角色属性

    public static int[] countOfEnemyRole; //对应数量

    //public static string[] enemyRolePrefabPath; //人物预制体路径

    public static string triggerToBattleGameObjUnionPreKey; //触发战斗的触发器标记，用来保存记录这个触发器以后不再显示了

    private void OnDestroy()
    {
        enemyRoleIds = null;
        countOfEnemyRole = null;
        //enemyRolePrefabPath = null;
        triggerToBattleGameObjUnionPreKey = null;

        //可能还在播放成功或者失败音乐
        MyAudioManager.GetInstance().StopSE();
    }

    // Start is called before the first frame update
    void Start()
    {
        //roles = new GameObject[enemyRoleIds.Length + 1 + (队友?傀儡？灵兽？灵虫？)];

        

        if (enemyRoleIds == null) //for test
        {
            roles[0].SetActive(true);
            roles[1].SetActive(true);

            GameObject hanLiGameObj = roles[0];
            HanLi hanLiCS = hanLiGameObj.GetComponent<HanLi>();
            hanLiCS.Init();
            hanLiCS.InitRoleBattelePos(2, 7); //todo
            
            Enemy enemyCS = roles[1].GetComponent<Enemy>();
            RoleInfo enemyRoleInfo = MyDBManager.GetInstance().GetRoleInfo(7);
            enemyCS.Init(enemyRoleInfo, 1);
            enemyCS.InitRoleBattelePos(2, 22);
            enemyCS.SetActionStrategy(new ActionStrategyGeneral());

            GameObject.FindGameObjectWithTag("UI_Canvas").GetComponent<BattleUIControl>().Init(roles);
            GameObject.FindGameObjectWithTag("Terrain").GetComponent<BattleController>().Init(roles);

        }
        else
        {
            List<GameObject> roleList = new List<GameObject>();
            //roles = new GameObject[enemyRoleIds.Length + 1]; //todo
            GameObject hanLiPrefab = Resources.Load<GameObject>("Prefab/RolePrefab/HanLiBattle");
            GameObject hanLiGameObj = Instantiate(hanLiPrefab);
            HanLi hanLiCS = hanLiGameObj.GetComponent<HanLi>();
            hanLiCS.Init();
            hanLiCS.InitRoleBattelePos(5, 5); //todo
            //roles[0] = hanLiGameObj;
            roleList.Add(hanLiGameObj);

            MyDBManager.GetInstance().ConnDB();
            for (int i = 0; i < enemyRoleIds.Length; i++)
            {
                RoleInfo enemyRoleInfo = MyDBManager.GetInstance().GetRoleInfo(enemyRoleIds[i]);
                for (int j = 0; j < countOfEnemyRole[i]; j++)
                {
                    GameObject enemyRolePrefab = Resources.Load<GameObject>(enemyRoleInfo.battleModelPath);
                    GameObject enemyRoleGameObj = Instantiate(enemyRolePrefab);
                    Enemy enemyCS = enemyRoleGameObj.AddComponent<Enemy>();
                    enemyCS.Init(enemyRoleInfo, j+1);
                    enemyCS.InitRoleBattelePos(7 + j*2, 7 + j*2); //todo
                    enemyCS.SetActionStrategy(new ActionStrategyGeneral());
                    roleList.Add(enemyRoleGameObj);

                    if(enemyCS.roleId == 7) //幼犬
                    {
                        enemyRoleGameObj.transform.localScale = new Vector3(0.36f, 0.36f, 0.36f);
                    }
                }
            }

            roles = roleList;
            GameObject.FindGameObjectWithTag("UI_Canvas").GetComponent<BattleUIControl>().Init(roles);
            GameObject.FindGameObjectWithTag("Terrain").GetComponent<BattleController>().Init(roles);
        }

    }    

}
