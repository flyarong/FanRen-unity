using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZhujueAttackBehaviour : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("OnStateEnter");
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("OnStateExit");
    }

    public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        Debug.Log("OnStateMachineEnter");
    }

    public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    {
        Debug.Log("OnStateMachineExit");
    }

    public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Debug.Log("OnStateMove");
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Debug.Log("OnStateUpdate");
    }

}
