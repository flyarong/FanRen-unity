using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinUIScript : MonoBehaviour
{

    public GameObject winTextGO;
    public GameObject winTextGO2;
    public GameObject gridLayout;

    public List<GameObject> gridGameObj;

    private void Start()
    {
        if(RootBattleInit.enemyRoleIds != null)
        {
            for (int i = 0; i < RootBattleInit.enemyRoleIds.Length; i++)
            {
                int roleId = RootBattleInit.enemyRoleIds[i];

            }
        }
        else
        {
            Debug.LogError("RootBattleInit.enemyRoleIds is null, 如果是从战斗场景启动的请忽略本日志");
        }
    }

}
