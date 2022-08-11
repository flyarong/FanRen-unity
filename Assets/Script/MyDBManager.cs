using UnityEngine;
using Mono.Data.Sqlite;
using System.IO;
using System;
using System.Collections.Generic;

public class MyDBManager
{

    private string dbFilePath = Application.dataPath + "/../FanRenData/originData.db";
    private SqliteConnection mSqliteConnection;
    private static MyDBManager mMyDBManager = new MyDBManager();

    private bool mIsConnected = false;

    private MyDBManager()
    {
    }

    
    public static MyDBManager GetInstance()
    {
        return mMyDBManager;
    }

    //有新增RW表需要在这里添加
    public void DeleteAllRWGameData()
    {
        string[] rwTalbeName = { "role_active_gongfa_rw", "role_active_shentong_rw", "role_bag_rw", "role_tasks_rw"};

        //sqlite不支持truncate
        //sqliteCommand.CommandText = $"truncate table role_active_gongfa_rw,role_active_shentong_rw,role_bag_rw,role_tasks_rw";
        foreach(string tableName in rwTalbeName)
        {
            SqliteCommand sqliteCommand = this.mSqliteConnection.CreateCommand();
            sqliteCommand.CommandText = $"delete from {tableName}";
            sqliteCommand.ExecuteNonQuery();
            sqliteCommand.Dispose();
        }
    }

    public bool ConnDB()
    {
        if (this.mIsConnected) return true;
        try
        {
            if (!Directory.Exists(new FileInfo(dbFilePath).Directory.FullName))
            {
                Directory.CreateDirectory(new FileInfo(dbFilePath).Directory.FullName);
            }
            if (!File.Exists(dbFilePath))
            {
                SqliteConnection.CreateFile(dbFilePath);
            }
            if (mSqliteConnection == null)
            {
                mSqliteConnection = new SqliteConnection(new SqliteConnectionStringBuilder() { DataSource = dbFilePath }.ToString());
            }
            mSqliteConnection.Open();
            this.mIsConnected = true;
            return this.mIsConnected;
        }
        catch (Exception e)
        {
            Debug.LogError("ConnDB error : " + e.ToString());
            this.mIsConnected = false;
            return this.mIsConnected;
        }
    }

    public bool IsConnected()
    {
        return this.mIsConnected;
    }

    private string TryGetStringValue(SqliteDataReader sdr, string key)
    {
        return sdr[key].Equals(DBNull.Value) ? "" : (string)sdr[key];
    }

    public RoleInfo GetRoleInfo(int roleId)
    {
        RoleInfo roleInfo = null;
        SqliteCommand sqliteCommand = this.mSqliteConnection.CreateCommand();
        sqliteCommand.CommandText = $"select * from role_info_r where roleId={roleId}";
        SqliteDataReader sdr = sqliteCommand.ExecuteReader();
        if (sdr.Read())
        {
            roleInfo = new RoleInfo();
            //public int roleId;
            string roleName = (string)sdr["roleName"];
            string roleAvatar = (string)sdr["roleAvatar"];
            int hp = (int)((Int64)sdr["hp"]);
            int maxHp = (int)((Int64)sdr["maxHp"]);
            int mp = (int)((Int64)sdr["mp"]);
            int maxMp = (int)((Int64)sdr["maxMp"]);
            int speed = (int)((Int64)sdr["speed"]);
            int gongJiLi = (int)((Int64)sdr["attack"]);
            int fangYuLi = (int)((Int64)sdr["defense"]);

            string canGetItemId = TryGetStringValue(sdr, "canGetItemId");
            string canGetItemPercent = TryGetStringValue(sdr, "canGetItemPercent");

            roleInfo.roleId = roleId;
            roleInfo.roleName = roleName;
            roleInfo.roleAvatar = roleAvatar;
            roleInfo.currentHp = hp;
            roleInfo.maxHp = maxHp;
            roleInfo.currentMp = mp;
            roleInfo.maxMp = maxMp;
            roleInfo.speed = speed;
            roleInfo.gongJiLi = gongJiLi;
            roleInfo.fangYuLi = fangYuLi;

            roleInfo.canGetItemId = canGetItemId;
            roleInfo.canGetItemPercent = canGetItemPercent;

        }
        sdr.Close();
        sdr.Dispose();
        sqliteCommand.Dispose();
        return roleInfo;
    }

    //activeState 0查询全部 1只查询激活的
    //isZhuJue 是否是查询主角的数据， 否则查询NPC的数据，两者在不同的表
    public List<Shentong> GetRoleShentong(int roleId, int activeState, bool isZhuJue)
    {
        List<Shentong> roleShentong = new List<Shentong>();
        SqliteCommand sqliteCommand = this.mSqliteConnection.CreateCommand();
        string tableName = isZhuJue ? "role_active_shentong_rw" : "role_active_shentong_r";
        sqliteCommand.CommandText = $"select * from {tableName} a left join shen_tong_r b on a.shenTongId=b.id where a.roleId={roleId}";
        SqliteDataReader sdr = sqliteCommand.ExecuteReader();
        while (sdr.Read())
        {
            int isActive = (int)((Int64)sdr["isActive"]);
            if(activeState == 1) //只查询激活
            {
                if (isActive != 1) continue;
            }

            int shenTongId = (int)((Int64)sdr["shenTongId"]);
            
            //int roleId = (int)((Int64)sdr["roleId"]);

            string shenTongName = (string)sdr["name"];
            int damage = (int)((Int64)sdr["damage"]);
            int defence = (int)((Int64)sdr["defense"]);
            string desc = (string)sdr["desc"];
            int studyRequireLevel = (int)((Int64)sdr["studyRequireLevel"]);
            int effType = (int)((Int64)sdr["effType"]); //神通类型，攻击、防御、变身 等等
            int rangeType = (int)((Int64)sdr["rangeType"]); //攻击范围类型，一条、一个面、一个点 等等
            int planeRadius = (int)((Int64)sdr["planeRadius"]); //面类型的攻击范围“半径”
            string effPath = (string)sdr["effPath"];
            string soundEffPath = (string)sdr["soundEffPath"];
            int unitDistance = (int)((Int64)sdr["unitDistance"]); //神通攻击距离
            int needMp = (int)((Int64)sdr["needMp"]);

            Shentong shenTong = new Shentong();
            shenTong.shenTongId = shenTongId;
            shenTong.isActive = isActive;
            shenTong.roleId = roleId;

            shenTong.shenTongName = shenTongName;
            shenTong.damage = damage;
            shenTong.defence = defence;
            shenTong.desc = desc;
            shenTong.studyRequireLevel = studyRequireLevel;
            shenTong.effType = (ShentongEffType)effType;
            shenTong.rangeType = (ShentongRangeType)rangeType;
            shenTong.planeRadius = planeRadius;
            shenTong.effPath = effPath;
            shenTong.soundEffPath = soundEffPath;
            shenTong.unitDistance = unitDistance;
            shenTong.needMp = needMp;

            roleShentong.Add(shenTong);
        }
        sdr.Close();
        sdr.Dispose();
        sqliteCommand.Dispose();
        return roleShentong;
    }

    

    public RoleItem GetRoleItemInBag(int itemId)
    {
        RoleItem roleItem = new RoleItem();
        SqliteCommand sqliteCommand = this.mSqliteConnection.CreateCommand();
        sqliteCommand.CommandText = $"select * from role_bag_rw a left join items_r b on a.itemId=b.itemId where a.itemId={itemId}";
        SqliteDataReader sdr = sqliteCommand.ExecuteReader();
        if (sdr.Read())
        {
            roleItem.itemId = itemId;
            roleItem.itemCount = (int)((Int64)sdr["itemCount"]); //比GetItemDetailInfo仅多出这一项

            roleItem.itemType = (int)((Int64)sdr["itemType"]);
            roleItem.itemName = (string)(sdr["itemName"]);

            roleItem.addPhyAck = (int)((Int64)sdr["addPhyAck"]);
            roleItem.addPhyDef = (int)((Int64)sdr["addPhyDef"]);
            roleItem.price = (int)((Int64)sdr["price"]);

            roleItem.imageName = (string)(sdr["imageName"]);
            roleItem.itemDesc = (string)(sdr["itemDesc"]);

            roleItem.scarceLevel = (int)((Int64)sdr["scarceLevel"]);
        }
        sdr.Close();
        sdr.Dispose();
        sqliteCommand.Dispose();
        return roleItem;
    }

    public RoleItem GetItemDetailInfo(int itemId)
    {
        RoleItem roleItem = new RoleItem();
        SqliteCommand sqliteCommand = this.mSqliteConnection.CreateCommand();
        sqliteCommand.CommandText = $"select * from items_r where itemId={itemId}";
        SqliteDataReader sdr = sqliteCommand.ExecuteReader();
        if (sdr.Read())
        {
            roleItem.itemId = itemId;
            //roleItem.itemCount = (int)((Int64)sdr["itemCount"]);
            roleItem.itemType = (int)((Int64)sdr["itemType"]);
            roleItem.itemName = (string)(sdr["itemName"]);

            roleItem.addPhyAck = (int)((Int64)sdr["addPhyAck"]);
            roleItem.addPhyDef = (int)((Int64)sdr["addPhyDef"]);
            roleItem.price = (int)((Int64)sdr["price"]);

            roleItem.imageName = (string)(sdr["imageName"]);
            roleItem.itemDesc = (string)(sdr["itemDesc"]);

            roleItem.scarceLevel = (int)((Int64)sdr["scarceLevel"]);
        }
        sdr.Close();
        sdr.Dispose();
        sqliteCommand.Dispose();
        return roleItem;
    }


    public bool AddRoleTask(int taskId)
    {
        SqliteCommand sqliteCommand = this.mSqliteConnection.CreateCommand();
        sqliteCommand.CommandText = $"select * from role_tasks_rw where taskId={taskId}";
        SqliteDataReader sdr = sqliteCommand.ExecuteReader();
        if (sdr.Read())
        {
            //任务已经存在
            Debug.Log("任务已经存在 taskId " + taskId);
            sdr.Close();
            sdr.Dispose();
            sqliteCommand.Dispose();

            return true;
        }
        else
        {
            sdr.Close();
            sdr.Dispose();
            sqliteCommand.Dispose();

            SqliteCommand sqliteCommand2 = this.mSqliteConnection.CreateCommand();
            sqliteCommand2.CommandText = $"insert into role_tasks_rw (taskId, taskState) values ({taskId}, {((int)FRTaskState.InProgress)})";
            bool result = sqliteCommand2.ExecuteNonQuery() == 1;
            sqliteCommand2.Dispose();
            return result;
        }
    }

    //查询某个任务
    public RoleTask GetRoleTask(int taskId)
    {
        Debug.Log("GetRoleTask taskId : " + taskId);
        SqliteCommand sqliteCommand = this.mSqliteConnection.CreateCommand(); 
        sqliteCommand.CommandText = $"select * from tasks_r a left join role_tasks_rw b on a.taskId=b.taskId where a.taskId={taskId}";
        SqliteDataReader sdr = sqliteCommand.ExecuteReader();
        if (sdr.Read())
        {
            RoleTask roleTask = new RoleTask();
            roleTask.taskId = taskId;
            roleTask.taskState = sdr["taskState"].Equals(DBNull.Value) ? (int)FRTaskState.Untrigger : (int)((Int64)sdr["taskState"]);
            roleTask.remark = (string)sdr["remark"];
            roleTask.isMainTask = (int)((Int64)sdr["isMainTask"]);
            roleTask.storyLineIndex = (int)((Int64)sdr["storyLineIndex"]);
            roleTask.triggerRoleId = (int)((Int64)sdr["triggerRoleId"]);
            roleTask.submitRoleId = (int)((Int64)sdr["submitRoleId"]);

            sdr.Close();
            sdr.Dispose();
            sqliteCommand.Dispose();

            return roleTask;
        }
        else
        {
            Debug.LogError("逻辑错误，查无数据 GetRoleTask taskId : " + taskId);
            return null;
        }
    }

    /**
     * 查询某个已经触发的任务
      **/
    public RoleTask GetTriggedRoleTask(int taskId)
    {
        SqliteCommand sqliteCommand = this.mSqliteConnection.CreateCommand();
        sqliteCommand.CommandText = $"select * from role_tasks_rw a left join tasks_r b on a.taskId=b.taskId where a.taskId={taskId}";
        SqliteDataReader sdr = sqliteCommand.ExecuteReader();
        if (sdr.Read())
        {
            RoleTask roleTask = new RoleTask();
            roleTask.taskId = taskId;
            roleTask.taskState = (int)((Int64)sdr["taskState"]);
            roleTask.remark = (string)sdr["remark"];
            roleTask.isMainTask = (int)((Int64)sdr["isMainTask"]);
            roleTask.storyLineIndex = (int)((Int64)sdr["storyLineIndex"]);
            roleTask.triggerRoleId = (int)((Int64)sdr["triggerRoleId"]);
            roleTask.submitRoleId = (int)((Int64)sdr["submitRoleId"]);

            sdr.Close();
            sdr.Dispose();
            sqliteCommand.Dispose();

            return roleTask;
        }
        else
        {
            return null;
        }
    }

    //获取某个角色能触发的所有任务
    public List<RoleTask> GetRoleTasks(int roleId)
    {
        SqliteCommand sqliteCommand = this.mSqliteConnection.CreateCommand();
        sqliteCommand.CommandText = $"select * from tasks_r a left join role_tasks_rw b on a.taskId=b.taskId where a.triggerRoleId={roleId}";
        SqliteDataReader sdr = sqliteCommand.ExecuteReader();
        List<RoleTask> results = new List<RoleTask>();
        while (sdr.Read())
        {
            RoleTask roleTask = new RoleTask();
            roleTask.taskId = (int)((Int64)sdr["taskId"]); 
            roleTask.taskState = sdr["taskState"].Equals(DBNull.Value) ? (int)FRTaskState.Untrigger : ((int)((Int64)sdr["taskState"]));
            roleTask.remark = (string)sdr["remark"];
            roleTask.isMainTask = (int)((Int64)sdr["isMainTask"]);
            roleTask.storyLineIndex = (int)((Int64)sdr["storyLineIndex"]);
            roleTask.triggerRoleId = roleId;
            roleTask.submitRoleId = (int)((Int64)sdr["submitRoleId"]);

            results.Add(roleTask);
        }
        sdr.Close();
        sdr.Dispose();
        sqliteCommand.Dispose();
        return results;
    }

    public bool UpdateRoleTaskState(int taskId, FRTaskState taskState)
    {
        SqliteCommand sqliteCommand = this.mSqliteConnection.CreateCommand();
        sqliteCommand.CommandText = $"update role_tasks_rw set taskState={((int)taskState)} where taskId={taskId}";
        bool result = sqliteCommand.ExecuteNonQuery() == 1;
        sqliteCommand.Dispose();
        return result;
    }

    //查询所有进行中的任务
    public List<RoleTask> GetAllLeaderActorInProgressTasks()
    {
        SqliteCommand sqliteCommand = this.mSqliteConnection.CreateCommand();
        sqliteCommand.CommandText = $"select * from role_tasks_rw a left join tasks_r b on a.taskId = b.taskId where a.taskState={((int)FRTaskState.InProgress)}";
        SqliteDataReader sdr = sqliteCommand.ExecuteReader();

        List<RoleTask> roleTasks = new List<RoleTask>();
        while (sdr.Read())
        {
            RoleTask roleTask = new RoleTask();
            roleTask.taskId = (int)((Int64)sdr["taskId"]);
            roleTask.taskState = (int)((Int64)sdr["taskState"]);
            roleTask.remark = (string)sdr["remark"];
            roleTask.isMainTask = (int)((Int64)sdr["isMainTask"]);
            roleTask.storyLineIndex = (int)((Int64)sdr["storyLineIndex"]);
            roleTask.triggerRoleId = (int)((Int64)sdr["triggerRoleId"]);
            roleTask.submitRoleId = (int)((Int64)sdr["submitRoleId"]);

            roleTasks.Add(roleTask);
        }
        sdr.Close();
        sdr.Dispose();
        sqliteCommand.Dispose();
        return roleTasks;
    }

    //查询某NPC能触发的所有任务
    public List<RoleTask> GetAllLeaderActorWithNPCTriggerTasks(int triggerNPCRoleId)
    {
        SqliteCommand sqliteCommand = this.mSqliteConnection.CreateCommand();
        //sqliteCommand.CommandText = $"select * from role_tasks_rw a left join tasks_r b on a.taskId = b.taskId where a.taskState={((int)state)} and b.triggerRoleId={triggerNPCRoleId}";
        sqliteCommand.CommandText = $"select * from tasks_r a left join role_tasks_rw b on a.taskId = b.taskId where a.triggerRoleId={triggerNPCRoleId}";
        SqliteDataReader sdr = sqliteCommand.ExecuteReader();
        List<RoleTask> roleTasks = new List<RoleTask>();
        while (sdr.Read())
        {
            RoleTask roleTask = new RoleTask();
            roleTask.taskId = (int)((Int64)sdr["taskId"]);
            roleTask.taskState = sdr["taskState"].Equals(DBNull.Value) ? (int)FRTaskState.Untrigger : ((int)((Int64)sdr["taskState"]));
            roleTask.remark = (string)sdr["remark"];
            roleTask.isMainTask = (int)((Int64)sdr["isMainTask"]);
            roleTask.storyLineIndex = (int)((Int64)sdr["storyLineIndex"]);
            roleTask.triggerRoleId = triggerNPCRoleId;
            roleTask.submitRoleId = (int)((Int64)sdr["submitRoleId"]);

            roleTasks.Add(roleTask);
        }
        sdr.Close();
        sdr.Dispose();
        sqliteCommand.Dispose();
        return roleTasks;
    }

    //查询某NPC能提交的所有任务
    public List<RoleTask> GetAllLeaderActorWithNPCSubmitTasks(int submitNPCRoleId)
    {
        SqliteCommand sqliteCommand = this.mSqliteConnection.CreateCommand();
        //sqliteCommand.CommandText = $"select * from role_tasks_rw a left join tasks_r b on a.taskId = b.taskId where a.taskState={((int)state)} and b.triggerRoleId={triggerNPCRoleId}";
        sqliteCommand.CommandText = $"select * from tasks_r a left join role_tasks_rw b on a.taskId = b.taskId where a.submitRoleId={submitNPCRoleId}";
        SqliteDataReader sdr = sqliteCommand.ExecuteReader();
        List<RoleTask> roleTasks = new List<RoleTask>();
        while (sdr.Read())
        {
            RoleTask roleTask = new RoleTask();
            roleTask.taskId = (int)((Int64)sdr["taskId"]);
            roleTask.taskState = sdr["taskState"].Equals(DBNull.Value) ? (int)FRTaskState.Untrigger : ((int)((Int64)sdr["taskState"]));
            roleTask.remark = (string)sdr["remark"];
            roleTask.isMainTask = (int)((Int64)sdr["isMainTask"]);
            roleTask.storyLineIndex = (int)((Int64)sdr["storyLineIndex"]);
            roleTask.triggerRoleId = (int)((Int64)sdr["triggerRoleId"]);
            roleTask.submitRoleId = (int)((Int64)sdr["submitRoleId"]);

            roleTasks.Add(roleTask);
        }
        sdr.Close();
        sdr.Dispose();
        sqliteCommand.Dispose();
        return roleTasks;
    }

    public bool AddItemToBag(int itemId, int addCount)
    {
        SqliteCommand sqliteCommand = null;
        SqliteCommand sqliteCommand2 = null;
        SqliteDataReader sdr = null;
        try
        {
            sqliteCommand = this.mSqliteConnection.CreateCommand();
            sqliteCommand.CommandText = $"select * from role_bag_rw where itemId={itemId}";
            sdr = sqliteCommand.ExecuteReader();

            sqliteCommand2 = this.mSqliteConnection.CreateCommand();
            if (sdr.Read()) //包里已经有该道具，只需要增加数量即可
            {
                Int64 originCount = (Int64)sdr["itemCount"];
                Int64 resultCount = originCount + addCount;
                sqliteCommand2.CommandText = $"update role_bag_rw set itemCount={resultCount} where itemId={itemId}";
            }
            else
            {
                //insert
                sqliteCommand2.CommandText = $"insert into role_bag_rw (itemId, itemCount) values ({itemId}, {addCount})";
            }
            bool result = sqliteCommand2.ExecuteNonQuery() == 1;
            return result;
        }
        catch(Exception e)
        {
            Debug.LogError("AddItemToBag, " + e.ToString());
            return false;
        }
        finally
        {
            if(sdr != null)
            {
                sdr.Close();
                sdr.Dispose();
            }
            if(sqliteCommand != null)
            {
                sqliteCommand.Dispose();
            }
            if (sqliteCommand2 != null)
            {
                sqliteCommand2.Dispose();
            }
        }
    }

    public bool DeleteItemInBag(int itemId, int deleteCount, int ownCount)
    {
        SqliteCommand sqliteCommand = null;
        try
        {
            sqliteCommand = this.mSqliteConnection.CreateCommand();
            if(deleteCount == ownCount)
            {
                sqliteCommand.CommandText = $"delete from role_bag_rw where itemId={itemId}";
            }
            else if(deleteCount < ownCount)
            {
                sqliteCommand.CommandText = $"update role_bag_rw set itemCount={ownCount-deleteCount} where itemId={itemId}";
            }
            else
            {
                Debug.LogError("DeleteItemInBag 逻辑错误");
                return false;
            }
            return sqliteCommand.ExecuteNonQuery() == 1;
        }
        catch (Exception e)
        {
            Debug.LogError("AddItemToBag, " + e.ToString());
            return false;
        }
        finally
        {
            if (sqliteCommand != null)
            {
                sqliteCommand.Dispose();
            }
        }
    }

}

//角色任务
public class RoleTask
{
    public int taskId;
    public int taskState;
    public string remark;
    public int isMainTask;
    public int storyLineIndex;
    public int triggerRoleId;
    public int submitRoleId;
}

//角色信息
public class RoleInfo
{
    public int roleId;
    public string roleName;
    public int currentHp;
    public int maxHp;
    public int currentMp;
    public int maxMp;
    public int speed;
    public int gongJiLi;
    public int fangYuLi;
    public string roleAvatar;

    public string canGetItemId;
    public string canGetItemPercent;

    public List<int> CanGetItemIdList()
    {
        if (canGetItemId == null || canGetItemId.Trim().Length == 0) return null;
        List<int> result = new List<int>();
        string[] idStringArray = canGetItemId.Split(",");
        foreach(string itemId in idStringArray)
        {
            result.Add(int.Parse(itemId));
        }
        return result;
    }

    public List<float> CanGetItemIdPercentList()
    {
        if (canGetItemPercent == null || canGetItemPercent.Trim().Length == 0) return null;
        List<float> result = new List<float>();
        string[] percentStringArray = canGetItemPercent.Split(",");
        foreach (string gainPercent in percentStringArray)
        {
            result.Add(float.Parse(gainPercent));
        }
        return result;
    }
}

//角色拥有的物品
public class RoleItem
{
    public int itemId;
    public int itemType;
    public int itemCount;
    public string itemName;
    public int addPhyAck;
    public int addPhyDef;
    public int price;
    public string imageName;
    public string itemDesc;
    public int scarceLevel;
}

//任务状态
public enum FRTaskState
{
    InProgress = 1, //进行种
    Finished = 2, //完成
    Fail = 3, //失败
    Untrigger = 0 //还没有触发
}

//物品类型
public enum FRItemType
{
    Fabao = 1,//法宝
    CaiLiao = 2,//材料
    LingCao = 3,//灵草
    DanYao = 4,//丹药
    LingShou = 5,//灵兽
    LingChong = 6,//灵虫
    GongFa = 7,//功法
    DanFang = 8,//丹方
    Other = 9,//其他
    KuiLei = 10,//傀儡
    TianDiLingWu = 11,//天地灵物
    ShenTong = 12,//神通
    Story = 13//剧情
}