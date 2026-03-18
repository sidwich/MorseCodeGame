using System;
using System.Collections.Generic;

namespace MorseCodeGame
{
    /// <summary>
    /// 消息播放模式
    /// </summary>
    public enum PlayMode
    {
        Sequential,  // 顺序播放
        Random,      // 随机播放
        Repeat,      // 单曲循环
        Shuffle      // 乱序播放一次
    }
    
    /// <summary>
    /// 关卡类型
    /// </summary>
    public enum LevelType
    {
        InputMorse,     // 玩家输入正确的摩斯电码（听明文，输入电码）
        ListenDecode    // 玩家听出电码中的信息（听电码，输入明文）
    }
    
    /// <summary>
    /// 单条消息数据
    /// </summary>
    [Serializable]
    public class MessageData
    {
        public string messageId;      // 消息唯一ID
        public string text;           // 消息文本（播放内容）
        public string answer;         // 正确答案（用于验证玩家输入）
        public string description;    // 描述/提示
        public float delayBefore;     // 播放前延迟（秒）
        public bool isMorseCode;      // 是否已经是摩斯码格式
        
        /// <summary>
        /// 获取正确答案（如果answer为空则返回text）
        /// </summary>
        public string GetAnswer()
        {
            return string.IsNullOrEmpty(answer) ? text : answer;
        }
    }
    
    /// <summary>
    /// 关卡数据
    /// </summary>
    [Serializable]
    public class LevelData
    {
        public string levelId;        // 关卡ID
        public string levelName;      // 关卡名称
        public string description;    // 关卡描述
        public LevelType levelType;   // 关卡类型（输入电码 / 听出信息）
        public List<MessageData> messages;  // 消息列表
    }
    
    /// <summary>
    /// 游戏数据根容器
    /// </summary>
    [Serializable]
    public class GameData
    {
        public string version;        // 数据版本
        public List<LevelData> levels;  // 所有关卡
    }
    
    /// <summary>
    /// 玩家输入记录
    /// </summary>
    [Serializable]
    public class InputRecord
    {
        public string messageId;      // 对应的消息ID
        public string playerInput;    // 玩家输入内容
        public string correctAnswer;  // 正确答案
        public bool isCorrect;        // 是否正确
        public float timestamp;       // 输入时间（从关卡开始计时）
        public int attemptCount;      // 尝试次数
    }
    
    /// <summary>
    /// 关卡完成数据
    /// </summary>
    [Serializable]
    public class LevelResult
    {
        public string levelId;                    // 关卡ID
        public List<InputRecord> records;         // 所有输入记录
        public float totalTime;                   // 总用时
        public int correctCount;                  // 正确数
        public int totalCount;                    // 总数
        public float accuracy;                    // 正确率
        public int starRating;                    // 星级评价（1-3）
        public DateTime completedAt;              // 完成时间
        
        public LevelResult()
        {
            records = new List<InputRecord>();
            completedAt = DateTime.Now;
        }
        
        /// <summary>
        /// 计算统计数据
        /// </summary>
        public void CalculateStats()
        {
            totalCount = records.Count;
            correctCount = 0;
            
            foreach (var record in records)
            {
                if (record.isCorrect) correctCount++;
            }
            
            accuracy = totalCount > 0 ? (float)correctCount / totalCount : 0f;
            
            // 计算星级：正确率>90%且全对3星，>70% 2星，>50% 1星
            if (accuracy >= 0.9f && correctCount == totalCount)
                starRating = 3;
            else if (accuracy >= 0.7f)
                starRating = 2;
            else if (accuracy >= 0.5f)
                starRating = 1;
            else
                starRating = 0;
        }
    }
}
