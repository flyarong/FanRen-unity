using UnityEditor;
using UnityEngine;

public class MyMenu : MonoBehaviour
{
    [MenuItem("凡人菜单/开始游戏")]
    public static void CustomMenu()
    {
        EdtorUtil.StartScene("Assets/Scenes/FirstScene_GameDesc.unity");
    }

}
