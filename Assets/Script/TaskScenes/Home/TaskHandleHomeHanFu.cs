using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskHandleHomeHanFu : ITaskHandle
{

    public const int ROLE_ID = 3;

    public override Queue<TalkContentItemModel> TriggerTaskTalkData(int taskId)
    {
        Queue<TalkContentItemModel> allTalkContent = new Queue<TalkContentItemModel>();
        TalkContentItemModel talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "hanFu",
            dfName = "韩父",
            dfTalkContent = "韩立，去后山捡些干柴回来"
        };
        allTalkContent.Enqueue(talkContentItemModel);

        talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "hanLi",
            dfName = "韩立",
            dfTalkContent = "好，我这就去"
        };
        allTalkContent.Enqueue(talkContentItemModel);

        talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "hanFu",
            dfName = "韩父",
            dfTalkContent = "早点回来，别乱跑。"
        };
        allTalkContent.Enqueue(talkContentItemModel);
        return allTalkContent;
    }

    public override Queue<TalkContentItemModel> InProgressTaskTalkData(int taskId)  //A NPC触发，B NPC提交的情况
    {
        if(taskId == 1) //干柴任务，触发、提交同人
        {
            return TriggerTaskTalkData(taskId);
        }
        else if (taskId == 5) //告别任务，触发、提交不同人
        {
            Queue<TalkContentItemModel> allTalkContent = new Queue<TalkContentItemModel>();
            TalkContentItemModel talkContentItemModel = new TalkContentItemModel
            {
                dfAvatar = "hanFu",
                dfName = "韩父",
                dfTalkContent = "韩立，你三叔来看你了，快叫人"
            };
            allTalkContent.Enqueue(talkContentItemModel);
            return allTalkContent;
        }
        Debug.LogError("逻辑错误 TaskHandleHomeHanFu InProgressTaskTalkData taskId " + taskId);
        return null;
        //return TriggerTaskTalkData(taskId);
    }

    public override Queue<TalkContentItemModel> SubmitTaskTalkData(int taskId)
    {
        Queue<TalkContentItemModel> allTalkContent = new Queue<TalkContentItemModel>();
        if (taskId == 1) //提交收集干柴任务
        {
            TalkContentItemModel talkContentItemModel = new TalkContentItemModel
            {
                dfAvatar = "hanFu",
                dfName = "韩父",
                dfTalkContent = "韩立，你三叔来看你了，快叫人"
            };
            allTalkContent.Enqueue(talkContentItemModel);
        }
        else if(taskId == 5) //提交告别任务
        {
            TalkContentItemModel talkContentItemModel = new TalkContentItemModel
            {
                dfAvatar = "hanFu",
                dfName = "韩父",
                dfTalkContent = "韩立，在外面要老实，遇事多忍让，别和其他人起争执"
            };
            allTalkContent.Enqueue(talkContentItemModel);
        }
        return allTalkContent;
    }

    public override Queue<TalkContentItemModel> GeneralTalkData()
    {
        MyDBManager.GetInstance().ConnDB();
        RoleTask roleTask = MyDBManager.GetInstance().GetRoleTask(1);
        RoleTask roleTask2 = MyDBManager.GetInstance().GetRoleTask(5);

        Queue<TalkContentItemModel> allTalkContent = new Queue<TalkContentItemModel>();
        if (roleTask.taskState == (int)FRTaskState.Untrigger) //未接任务
        {
            Debug.LogError("逻辑错误 TaskHandleHomeHanFu GeneralTalkData");
        }
        else if (roleTask.taskState == (int)FRTaskState.Finished && roleTask2.taskState == (int)FRTaskState.Untrigger)
        {
            TalkContentItemModel talkContentItemModel = new TalkContentItemModel
            {
                dfAvatar = "hanFu",
                dfName = "韩父",
                dfTalkContent = "韩立，你三叔来看你了，快叫人"
            };
            allTalkContent.Enqueue(talkContentItemModel);
        }
        else if (roleTask2.taskState == (int)FRTaskState.Finished)
        {
            TalkContentItemModel talkContentItemModel = new TalkContentItemModel
            {
                dfAvatar = "hanFu",
                dfName = "韩父",
                dfTalkContent = "韩立，在外面要老实，遇事多忍让，别和其他人起争执"
            };
            allTalkContent.Enqueue(talkContentItemModel);
        }

        return allTalkContent;
    }

    public override bool IsTriggerable(int taskId)
    {
        MyDBManager.GetInstance().ConnDB();
        return MyDBManager.GetInstance().GetRoleTask(taskId).taskState == (int)FRTaskState.Untrigger;
    }

    public override bool IsSubmitable(int taskId)
    {
        if(taskId == 1)
        {
            MyDBManager.GetInstance().ConnDB();
            RoleItem roleItem = MyDBManager.GetInstance().GetRoleItemInBag(1); //1是干柴
            return roleItem.itemCount >= 5;
        }
        else if(taskId == 5) //告别
        {
            return true;
        }
        Debug.LogError("逻辑错误TaskHandleHomeHanFu IsSubmitable taskId " + taskId);
        return false;
    }

    public override void OnSubmitTaskComplete(int taskId)
    {
        if(taskId == 1)
        {
            Debug.Log("干柴>=5，-5干柴");
            MyDBManager.GetInstance().ConnDB();
            RoleItem roleItem = MyDBManager.GetInstance().GetRoleItemInBag(1);
            MyDBManager.GetInstance().DeleteItemInBag(1, 5, roleItem.itemCount);
        }else if (taskId == 5)
        {
            Debug.Log("告别，心境+1");
        }
    }

    public override void OnTriggerTask(int taskId)
    {
        Debug.Log("OnTriggerTask taskId " + taskId);
    }

}
