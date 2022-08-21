using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleCameraController : BaseMono
{
    private Transform player;
    private Vector3 dir;

    private GameObject selectedRole;

    //镜头切换人物的时候，相机不要跟随更新
    private bool isCameraMoving = false;

    //Vector3 roleHeadPointToCamera = Vector3.zero;

    //GameObject terrainGO;
    private TerrainCollider terrainCollider;

    private BattleController mBattleController;

    void Start()
    {
        GameObject terrainGO = GameObject.FindGameObjectWithTag("Terrain");
        terrainCollider = terrainGO.GetComponent<TerrainCollider>();
        mBattleController = terrainGO.GetComponent<BattleController>();
    }

    private Vector3 CalcScreenCenterPosOnTerrain()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3((float)Screen.width / 2, (float)Screen.height / 2, 0));
        //public bool Raycast(Ray ray, out RaycastHit hitInfo, float maxDistance);
        RaycastHit hitInfo;
        if (terrainCollider.Raycast(ray, out hitInfo, 1000))
        {
            return ray.GetPoint(hitInfo.distance);
        }
        else
        {
            return Vector3.zero;
        }
    }

    Transform lastTarget;

    private void CameraFocusAt(Transform target)
    {
        Vector3 cp = CalcScreenCenterPosOnTerrain();
        Vector3 tp = target.position;

        Hashtable args = new Hashtable();
        //lookahead
        //args.Add("lookahead", 0.9f);
        //args.Add("path", path.ToArray());
        //args.Add("looktarget", selectedRole.transform);
        args.Add("looptype", iTween.LoopType.none);
        args.Add("easeType", iTween.EaseType.easeOutQuint);
        if(lastTarget == target)
        {
            args.Add("time", 0.01f);
        }
        else
        {
            args.Add("time", 0.6f);
        }
        //args.Add("speed", 7);
        //args.Add("orienttopath", true);
        //Debug.Log("roleHeadPointToCamera y " + roleHeadPointToCamera);
        args.Add("position", Camera.main.transform.position + (tp - cp));
        args.Add("oncomplete", "OnComplete");
        args.Add("oncompletetarget", this.gameObject);
        //looktarget
        iTween.MoveTo(this.gameObject, args);
        this.lastTarget = target;
    }

    public void SetSelectedRole(GameObject selectedRole)
    {
        isCameraMoving = true;
        mBattleController.OnChangeRoleCameraMove(CameraState.Moving, selectedRole);
        this.selectedRole = selectedRole;
        CameraFocusAt(selectedRole.transform);
    }

    private void OnComplete()
    {
        player = this.selectedRole.transform;
        dir = player.transform.position - transform.position;
        isCameraMoving = false;
        mBattleController.OnChangeRoleCameraMove(CameraState.Stopped, this.selectedRole);
    }

    void LateUpdate()
    {

        //if (Input.GetKeyUp(KeyCode.LeftAlt))
        //{
        //    Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
        //}

        if (player == null || dir == null || isCameraMoving) return;

        transform.position = player.transform.position - dir;

        if (Input.GetMouseButton(1))
        {
            if (Input.GetAxis("Mouse X") != 0f)
            {
                float mouseX = Input.GetAxis("Mouse X");
                transform.RotateAround(player.position, player.up, mouseX * 600 * Time.deltaTime);
                dir = player.transform.position - transform.position;
            }

            if (Input.GetAxis("Mouse Y") != 0f)
            {
                float mouseY = Input.GetAxis("Mouse Y");
                //transform.RotateAround(player.position, transform.right, -mouseY * 600 * Time.deltaTime);
                //dir = player.transform.position - transform.position;

                if (mouseY > 0)
                {
                    if (transform.rotation.eulerAngles.x > 20)
                    {
                        transform.RotateAround(player.position, transform.right, -mouseY * 600 * Time.deltaTime);
                        dir = player.transform.position - transform.position;
                    }
                }
                else if (mouseY < 0)
                {
                    if (transform.rotation.eulerAngles.x < 70)
                    {
                        transform.RotateAround(player.position, transform.right, -mouseY * 600 * Time.deltaTime);
                        dir = player.transform.position - transform.position;
                    }
                }

            }
        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0) //靠近
        {
            if(Vector3.Distance(this.transform.position, player.transform.position) > 10)
            {
                transform.Translate(dir / 10f, Space.World);
                dir = player.transform.position - transform.position;
            }
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0) //远离
        {
            if (Vector3.Distance(this.transform.position, player.transform.position) < 15) {
                transform.Translate(-dir / 10f, Space.World);
                dir = player.transform.position - transform.position;
            }
        }

    }

}

public enum CameraState
{
    Moving = 1, Stopped = 2
}
