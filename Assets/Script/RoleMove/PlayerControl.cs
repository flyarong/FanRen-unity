using UnityEngine;


public class PlayerControl : MonoBehaviour
{

    public const bool IS_DEBUG = true;

    public Camera playerCamera;
    private Animator animator;

    TalkButtonController talkButtonController = null;

    private CharacterController cc;

    Rigidbody mRigidbody;
    Vector3 moveDir;

    public float moveSpeed = 5f;

    private GameObject lastHitGameObject;
    private IColliderWithCC colliderWithCCScript;

    //非正常状态：这是为了解决返回或者读档时候，人物的位置不能还原，人物创建的时候设置position和cc的move有冲突，所以设置了前面1秒不让运行cc.move
    //但这样做其实会有隐患，待解决。 这个问题应该是unity内部多线程问题导致的。
    //可以加个过场动画掩饰这1秒
    bool normalState = false;

    private void SetNormalState()
    {
        normalState = true;
    }

    private void Awake()
    {
        //debug
        //PlayerPrefs.DeleteAll();

        HandleSaveOrLoad();
    }

    private void OnApplicationQuit()
    {
        Debug.LogWarning("OnApplicationQuit()");
        PlayerPrefs.DeleteAll();
    }


    void Start()
    {

        animator = GetComponent<Animator>();

        talkButtonController = GameObject.FindGameObjectWithTag("DoTalkButton")?.GetComponent<TalkButtonController>();

        //mRigidbody = GetComponent<Rigidbody>();

        cc = GetComponent<CharacterController>();

        //cc.detectCollisions = false;

        if (playerCamera == null) playerCamera = Camera.main;

        Invoke("SetNormalState", 1);
    }

    private void HandleSaveOrLoad()
    {
        int lastSceneIndex = SaveUtil.GetLastSceneBuildIndex();
        if (lastSceneIndex >= 0 && lastSceneIndex == this.gameObject.scene.buildIndex) //有保存记录,且保存场景和当前一样，说明是返回或者读档
        {
            Debug.LogWarning("=================正在返回前面的场景 或者 读档最后保存的场景");
            Vector3 lastPositon = SaveUtil.GetLastPosition();
            if (lastPositon != Vector3.zero)
            {
                Debug.LogWarning("设置 role hanli positon " + lastPositon);
                //this.transform.position = lastPositon;
                this.transform.position = lastPositon;
            }
            else
            {
                Debug.LogError("数据错误 position is 0");
            }
        }
        else
        {
            //每次创建主角(加载新场景)保存或者进入战斗前保存
            Debug.LogWarning("=================正在保存角色数据");
            SaveUtil.SaveGameObjLastState(this.gameObject);
        }
    }

    private void DoTurn(Vector3 dir)
    {
        this.moveDir = dir;
        this.moveDir.y = 0f;
        transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, this.moveDir, 0.7f, 0f));
    }

    private void DoMove()
    {
        if (!cc.isGrounded)
        {
            this.moveDir += new Vector3(0f, -1f, 0f);
        }
        if (this.moveDir.x != 0f || this.moveDir.z != 0f || this.moveDir.y == -1f)
        {
            if (normalState)
            {
                CollisionFlags cf = cc.Move(this.moveDir / 8);
                if (colliderWithCCScript != null && ((MonoBehaviour)colliderWithCCScript).gameObject.activeInHierarchy && cf == CollisionFlags.None && (this.moveDir.x != 0f || this.moveDir.y != 0f))
                {
                    colliderWithCCScript.OnPlayerCollisionExit(this.gameObject);
                    lastHitGameObject = null;
                    colliderWithCCScript = null;
                }
            }
        }
    }

    float horizontal;
    float vertical;

    void Update()
    {
        if(talkButtonController != null && talkButtonController.IsTalkUIShowing())
        {
            return;
        }
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
    }

    private void FixedUpdate()
    {
        if (horizontal != 0 && vertical != 0) //斜
        {
            Vector3 camForword = playerCamera.transform.forward;
            Vector3 dir = (horizontal > 0 ? playerCamera.transform.right : -playerCamera.transform.right) + (vertical > 0 ? camForword : -camForword);
            DoTurn(dir.normalized);
        }
        else if (horizontal != 0 && vertical == 0) //左右
        {
            DoTurn(horizontal > 0 ? playerCamera.transform.right : -playerCamera.transform.right);
        }
        else if (horizontal == 0 && vertical != 0) //前后
        {
            Vector3 camForword = playerCamera.transform.forward;
            DoTurn(vertical > 0 ? camForword : -camForword);

        }
        if (horizontal != 0f || vertical != 0f)
        {
            animator?.SetBool("isRun", true);
        }
        else
        {
            animator?.SetBool("isRun", false);
            this.moveDir = Vector3.zero;
        }
        DoMove();
    }


    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("PlayerControl OnCollisionEnter");
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        IColliderWithCC tmp = hit.collider.gameObject.GetComponent<IColliderWithCC>();
        if(tmp != null) //是一个需要和CC碰撞的物体
        {
            if (lastHitGameObject == hit.collider.gameObject) return; //重复碰撞一个物体忽略
            colliderWithCCScript = tmp;
            colliderWithCCScript.OnPlayerCollisionEnter(this.gameObject);
            lastHitGameObject = hit.collider.gameObject;
        }
    }

}
