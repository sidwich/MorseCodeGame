using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MorseCodeGame
{
    /// <summary>
    /// 消息数据库管理器
    /// 管理所有关卡和消息的加载、查询
    /// </summary>
    public class MessageDatabase : MonoBehaviour
    {
        [Header("配置")]
        [Tooltip("JSON配置文件路径，相对于StreamingAssets")]
        public string configFilePath = "GameData/LevelData.json";
        
        [Tooltip("默认关卡ID")]
        public string defaultLevelId = "LEVEL_001";
        
        // 数据存储
        private GameData _gameData;
        private Dictionary<string, LevelData> _levelDict;
        private Dictionary<string, Dictionary<string, MessageData>> _messageDict;
        
        // 事件
        public event Action OnDataLoaded;
        public event Action<string> OnLoadError;
        
        // 单例
        public static MessageDatabase Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            _levelDict = new Dictionary<string, LevelData>();
            _messageDict = new Dictionary<string, Dictionary<string, MessageData>>();
            
            LoadData();
        }
        
        /// <summary>
        /// 加载数据
        /// </summary>
        public void LoadData()
        {
            string fullPath = Path.Combine(Application.streamingAssetsPath, configFilePath);
            
            try
            {
                if (File.Exists(fullPath))
                {
                    string json = File.ReadAllText(fullPath);
                    ParseGameData(json);
                    Debug.Log($"[MessageDatabase] 数据加载成功: {_gameData.levels.Count} 个关卡");
                    OnDataLoaded?.Invoke();
                }
                else
                {
                    // 如果文件不存在，创建默认数据
                    CreateDefaultData();
                    SaveData();
                    OnDataLoaded?.Invoke();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[MessageDatabase] 加载数据失败: {e.Message}");
                OnLoadError?.Invoke(e.Message);
                // 使用默认数据
                CreateDefaultData();
            }
        }
        
        /// <summary>
        /// 从JSON解析数据
        /// </summary>
        private void ParseGameData(string json)
        {
            _gameData = JsonUtility.FromJson<GameData>(json);
            if (_gameData == null)
            {
                throw new Exception("JSON解析失败");
            }
            
            BuildDictionary();
        }
        
        /// <summary>
        /// 构建字典索引
        /// </summary>
        private void BuildDictionary()
        {
            _levelDict.Clear();
            _messageDict.Clear();
            
            foreach (var level in _gameData.levels)
            {
                if (string.IsNullOrEmpty(level.levelId)) continue;
                
                _levelDict[level.levelId] = level;
                
                var msgDict = new Dictionary<string, MessageData>();
                if (level.messages != null)
                {
                    foreach (var msg in level.messages)
                    {
                        if (!string.IsNullOrEmpty(msg.messageId))
                        {
                            msgDict[msg.messageId] = msg;
                        }
                    }
                }
                _messageDict[level.levelId] = msgDict;
            }
        }
        
        /// <summary>
        /// 创建默认数据
        /// </summary>
        private void CreateDefaultData()
        {
            _gameData = new GameData
            {
                version = "1.0",
                levels = new List<LevelData>
                {
                    new LevelData
                    {
                        levelId = "LEVEL_001",
                        levelName = "紧急求救",
                        description = "学习基础求救信号",
                        messages = new List<MessageData>
                        {
                            new MessageData 
                            { 
                                messageId = "MSG_001_SOS", 
                                text = "SOS", 
                                description = "国际求救信号",
                                delayBefore = 0.5f
                            },
                            new MessageData 
                            { 
                                messageId = "MSG_002_HELP", 
                                text = "HELP", 
                                description = "求助信号",
                                delayBefore = 0.5f
                            },
                            new MessageData 
                            { 
                                messageId = "MSG_003_MAYDAY", 
                                text = "MAYDAY", 
                                description = "无线电求救",
                                delayBefore = 0.5f
                            },
                            new MessageData 
                            { 
                                messageId = "MSG_004_RESCUE", 
                                text = "RESCUE", 
                                description = "需要救援",
                                delayBefore = 0.5f
                            }
                        }
                    },
                    new LevelData
                    {
                        levelId = "LEVEL_002",
                        levelName = "方位指引",
                        description = "方位和位置相关信号",
                        messages = new List<MessageData>
                        {
                            new MessageData 
                            { 
                                messageId = "MSG_101_NORTH", 
                                text = "NORTH", 
                                description = "北方",
                                delayBefore = 0.5f
                            },
                            new MessageData 
                            { 
                                messageId = "MSG_102_SOUTH", 
                                text = "SOUTH", 
                                description = "南方",
                                delayBefore = 0.5f
                            },
                            new MessageData 
                            { 
                                messageId = "MSG_103_LOCATION", 
                                text = "LOCATION", 
                                description = "位置",
                                delayBefore = 0.5f
                            }
                        }
                    }
                }
            };
            
            BuildDictionary();
            Debug.Log("[MessageDatabase] 创建默认数据");
        }
        
        /// <summary>
        /// 保存数据到文件
        /// </summary>
        public void SaveData()
        {
            string fullPath = Path.Combine(Application.streamingAssetsPath, configFilePath);
            string directory = Path.GetDirectoryName(fullPath);
            
            try
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                string json = JsonUtility.ToJson(_gameData, true);
                File.WriteAllText(fullPath, json);
                Debug.Log($"[MessageDatabase] 数据已保存: {fullPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[MessageDatabase] 保存数据失败: {e.Message}");
            }
        }
        
        #region 查询接口
        
        /// <summary>
        /// 获取所有关卡
        /// </summary>
        public List<LevelData> GetAllLevels() => _gameData?.levels ?? new List<LevelData>();
        
        /// <summary>
        /// 根据ID获取关卡
        /// </summary>
        public LevelData GetLevel(string levelId)
        {
            if (_levelDict.TryGetValue(levelId, out var level))
            {
                return level;
            }
            return null;
        }
        
        /// <summary>
        /// 获取关卡消息列表
        /// </summary>
        public List<MessageData> GetLevelMessages(string levelId)
        {
            var level = GetLevel(levelId);
            return level?.messages ?? new List<MessageData>();
        }
        
        /// <summary>
        /// 根据ID获取消息
        /// </summary>
        public MessageData GetMessage(string levelId, string messageId)
        {
            if (_messageDict.TryGetValue(levelId, out var msgDict))
            {
                if (msgDict.TryGetValue(messageId, out var message))
                {
                    return message;
                }
            }
            return null;
        }
        
        /// <summary>
        /// 根据索引获取消息
        /// </summary>
        public MessageData GetMessageByIndex(string levelId, int index)
        {
            var messages = GetLevelMessages(levelId);
            if (index >= 0 && index < messages.Count)
            {
                return messages[index];
            }
            return null;
        }
        
        /// <summary>
        /// 获取随机消息
        /// </summary>
        public MessageData GetRandomMessage(string levelId)
        {
            var messages = GetLevelMessages(levelId);
            if (messages.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, messages.Count);
                return messages[index];
            }
            return null;
        }
        
        /// <summary>
        /// 获取默认关卡
        /// </summary>
        public LevelData GetDefaultLevel() => GetLevel(defaultLevelId);
        
        /// <summary>
        /// 关卡是否存在
        /// </summary>
        public bool HasLevel(string levelId) => _levelDict.ContainsKey(levelId);
        
        /// <summary>
        /// 消息是否存在
        /// </summary>
        public bool HasMessage(string levelId, string messageId)
        {
            return GetMessage(levelId, messageId) != null;
        }
        
        #endregion
    }
}
