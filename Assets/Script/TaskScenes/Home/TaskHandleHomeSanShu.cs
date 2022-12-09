using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskHandleHomeSanShu : ITaskHandle
{

    public const int ROLE_ID = 6;

    /// <summary>
    /// todo ...... 对话应该存在数据库或者excel，待重构
    /// </summary>
    /// <param name="taskId"></param>
    /// <returns></returns>
    public override Queue<TalkContentItemModel> TriggerTaskTalkData(int taskId)
    {

        Queue<TalkContentItemModel> allTalkContent = new Queue<TalkContentItemModel>();

        TalkContentItemModel talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "hanLi",
            dfName = "韩立",
            dfTalkContent = "三叔好"
        };
        allTalkContent.Enqueue(talkContentItemModel);

        talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "sanShu",
            dfName = "三叔",
            dfTalkContent = "韩立，长大了啊。大哥，嫂子，我这次来啊，其实是要跟你们商量个事儿。我工作的酒楼不是普通的酒楼，他其实属于江湖中一个很大的门派，七玄门。七玄门有外门和内门之分，前不久，我正式成为了七玄门的外门弟子，能够推举七到十二岁的孩童去参加七玄门招收内门弟子的考验。这个测试五年一次，下个月就要开始了。大哥大嫂，你们也知道，我尚没有儿女，想来想去，也就你们家的二愣子年龄合适，机会难得，不知道你们意下如何呀？"
        };
        allTalkContent.Enqueue(talkContentItemModel);

        talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "hanFu",
            dfName = "韩父",
            dfTalkContent = "江湖门派？"
        };
        allTalkContent.Enqueue(talkContentItemModel);

        talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "sanShu",
            dfName = "三叔",
            dfTalkContent = "大哥，这可是个好机会呀，七玄门是这方圆数百里内数一数二的大门派，只要小立成了内门弟子，不但以后可以免费习武，吃喝不愁，每月还能有一两多的散银子零花呢。"
        };
        allTalkContent.Enqueue(talkContentItemModel);

        talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "hanFu",
            dfName = "韩父",
            dfTalkContent = "每个月一两多银子！？"
        };
        allTalkContent.Enqueue(talkContentItemModel);

        talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "sanShu",
            dfName = "三叔",
            dfTalkContent = "对呀，而且参加考验的人，即使未能入选，也有机会成为像我一样的外门人员，专门替七玄门打理门外的生意。这机会难得，大哥你可得好好想想，错过可惜。"
        };
        allTalkContent.Enqueue(talkContentItemModel);

        talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "hanFu",
            dfName = "韩父",
            dfTalkContent = "................................"
        };
        allTalkContent.Enqueue(talkContentItemModel);

        talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "hanFu",
            dfName = "韩父",
            dfTalkContent = "................."
        };
        allTalkContent.Enqueue(talkContentItemModel);

        talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "hanFu",
            dfName = "韩父",
            dfTalkContent = "......"
        };
        allTalkContent.Enqueue(talkContentItemModel);

        talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "hanFu",
            dfName = "韩父",
            dfTalkContent = "行，那就让二愣子跟你去吧。"
        };
        allTalkContent.Enqueue(talkContentItemModel);

        talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "sanShu",
            dfName = "三叔",
            dfTalkContent = "好，那可太好了，我这还有些银子，大哥你拿着。考验有难度，体力也很重要，这段时间你们多给小立做点好吃的，好应付考验，我一个月后就来带小立走，那我就先告辞了，大哥大嫂留步。"
        };
        allTalkContent.Enqueue(talkContentItemModel);

        talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "hanLi",
            dfName = "韩立",
            dfTalkContent = "虽然三叔刚刚说的话不全明白，但我好像可以进城能挣大钱了。",
            isInner = true
        };
        allTalkContent.Enqueue(talkContentItemModel);

        return allTalkContent;
    }

    public override Queue<TalkContentItemModel> InProgressTaskTalkData(int taskId)
    {
        return TriggerTaskTalkData(taskId);
    }

    public override Queue<TalkContentItemModel> SubmitTaskTalkData(int taskId)
    {
        throw new System.NotImplementedException();
    }

    public override Queue<TalkContentItemModel> GeneralTalkData()
    {
        Queue<TalkContentItemModel> allTalkContent = new Queue<TalkContentItemModel>();
        TalkContentItemModel talkContentItemModel = new TalkContentItemModel
        {
            dfAvatar = "sanShu",
            dfName = "三叔",
            dfTalkContent = "韩立，长大了啊"
        };
        allTalkContent.Enqueue(talkContentItemModel);
        return allTalkContent;
    }

    public override bool IsTriggerable(int taskId)
    {
        MyDBManager.GetInstance().ConnDB();
        if(taskId == 3) //和三叔到青牛镇任务
        {
            return MyDBManager.GetInstance().GetRoleTask(1).taskState == (int)FRTaskState.Finished; //收集干柴任务完成可触发
        }
        else
        {
            Debug.LogError("逻辑错误 TaskHandleHomeSanShu IsTriggerable taskId " + taskId);
            return false;
        }
    }

    public override bool IsSubmitable(int taskId)
    {
        throw new System.NotImplementedException();
    }

    public override void OnSubmitTaskComplete(int taskId)
    {

    }

    public override void OnTriggerTask(int taskId)
    {
        MyDBManager.GetInstance().ConnDB();
        MyDBManager.GetInstance().AddRoleTask(4);
        MyDBManager.GetInstance().AddRoleTask(5);
        UIUtil.NotifyTaskUIDatasetChanged();
    }

}
