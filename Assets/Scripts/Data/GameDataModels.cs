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
    /// 单条消息数据
    /// </summary>
    [Serializable]
    public class MessageData
    {
        public string messageId;      // 消息唯一ID
        public string text;           // 消息文本（摩斯码或明文）
        public string description;    // 描述/提示
        public float delayBefore;     // 播放前延迟（秒）
        public bool isMorseCode;      // 是否已经是摩斯码格式
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
}
