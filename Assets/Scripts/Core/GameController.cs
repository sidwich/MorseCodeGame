using UnityEngine;
using System;
using System.Collections.Generic;

namespace MorseCodeGame
{
    /// <summary>
    /// 游戏主控制器 - 提供所有游戏功能的API接口
    /// 与UI解耦，方便自定义UI调用
    /// </summary>
    public class GameController : MonoBehaviour
    {
        [Header("Core Components")]
        public MorseAudioPlayer audioPlayer;
        public MorseInputHandler inputHandler;
        public InputRecorder inputRecorder;
        public MessageDatabase messageDatabase;
        public MorseCodeData morseData;
        
        // 当前状态
        private LevelData _currentLevel;
        private MessageData _currentMessage;
        private int _currentMessageIndex = 0;
        private bool _isPlaying = false;
        
        // 事件 - UI可订阅更新界面
        public event Action<LevelData> OnLevelStarted;           // 关卡开始
        public event Action<LevelData> OnLevelChanged;           // 关卡切换
        public event Action<MessageData> OnMessageSelected;      // 消息选中
        public event Action<MessageData> OnMessageStarted;       // 消息开始播放
        public event Action<MessageData> OnMessageCompleted;     // 消息播放完成
        public event Action<InputRecord> OnInputSubmitted;       // 输入提交
        public event Action<InputRecord> OnAnswerCorrect;        // 回答正确
        public event Action<InputRecord> OnAnswerWrong;          // 回答错误
        public event Action<LevelResult> OnLevelCompleted;       // 关卡完成
        public event Action<string> OnMorseInputChanged;         // 摩斯输入变化
        public event Action<bool> OnPlaybackStateChanged;        // 播放状态变化
        
        // 属性
        public LevelData CurrentLevel => _currentLevel;
        public MessageData CurrentMessage => _currentMessage;
        public bool IsPlaying => _isPlaying;
        public int CurrentMessageIndex => _currentMessageIndex;
        public LevelType CurrentLevelType => _currentLevel?.levelType ?? LevelType.ListenDecode;
        
        private void Awake()
        {
            InitializeComponents();
            BindInternalEvents();
        }
        
        #region Initialization
        
        private void InitializeComponents()
        {
            // AudioPlayer
            if (audioPlayer == null)
                audioPlayer = FindObjectOfType<MorseAudioPlayer>() ?? CreateComponent<MorseAudioPlayer>("MorseAudioPlayer");
            
            // InputHandler
            if (inputHandler == null)
                inputHandler = FindObjectOfType<MorseInputHandler>() ?? CreateComponent<MorseInputHandler>("MorseInputHandler");
            
            // InputRecorder
            if (inputRecorder == null)
                inputRecorder = FindObjectOfType<InputRecorder>() ?? CreateComponent<InputRecorder>("InputRecorder");
            
            // MessageDatabase
            if (messageDatabase == null)
                messageDatabase = FindObjectOfType<MessageDatabase>() ?? CreateComponent<MessageDatabase>("MessageDatabase");
            
            // MorseCodeData
            if (morseData == null)
            {
                morseData = ScriptableObject.CreateInstance<MorseCodeData>();
                audioPlayer.morseData = morseData;
                inputRecorder.morseData = morseData;
            }
            
            // Connect references
            inputHandler.audioPlayer = audioPlayer;
            inputRecorder.inputHandler = inputHandler;
            inputRecorder.audioPlayer = audioPlayer;
        }
        
        private T CreateComponent<T>(string name) where T : Component
        {
            GameObject go = new GameObject(name);
            return go.AddComponent<T>();
        }
        
        private void BindInternalEvents()
        {
            if (inputHandler != null)
            {
                inputHandler.OnMorseInput += (code) => OnMorseInputChanged?.Invoke(code);
            }
            
            if (audioPlayer != null)
            {
                audioPlayer.OnPlaybackStart += () => 
                {
                    _isPlaying = true;
                    OnPlaybackStateChanged?.Invoke(true);
                    OnMessageStarted?.Invoke(_currentMessage);
                };
                
                audioPlayer.OnPlaybackEnd += () => 
                {
                    _isPlaying = false;
                    OnPlaybackStateChanged?.Invoke(false);
                    OnMessageCompleted?.Invoke(_currentMessage);
                };
            }
            
            if (inputRecorder != null)
            {
                inputRecorder.OnInputSubmitted += (record) => OnInputSubmitted?.Invoke(record);
                inputRecorder.OnAnswerCorrect += (record) => OnAnswerCorrect?.Invoke(record);
                inputRecorder.OnAnswerWrong += (record) => OnAnswerWrong?.Invoke(record);
            }
        }
        
        #endregion
        
        #region Level Management
        
        /// <summary>
        /// 获取所有可用关卡
        /// </summary>
        public List<LevelData> GetAllLevels()
        {
            return messageDatabase?.GetAllLevels() ?? new List<LevelData>();
        }
        
        /// <summary>
        /// 获取关卡数量
        /// </summary>
        public int GetLevelCount()
        {
            return messageDatabase?.GetAllLevels().Count ?? 0;
        }
        
        /// <summary>
        /// 开始指定关卡
        /// </summary>
        public void StartLevel(string levelId)
        {
            var level = messageDatabase?.GetLevel(levelId);
            if (level != null)
            {
                StartLevel(level);
            }
            else
            {
                Debug.LogWarning($"[GameController] Level not found: {levelId}");
            }
        }
        
        /// <summary>
        /// 开始指定关卡（通过LevelData）
        /// </summary>
        public void StartLevel(LevelData level)
        {
            _currentLevel = level;
            _currentMessageIndex = 0;
            
            audioPlayer?.SetLevel(level.levelId);
            inputRecorder?.StartLevel(level.levelId);
            
            OnLevelStarted?.Invoke(level);
            OnLevelChanged?.Invoke(level);
            
            // Auto select first message
            if (level.messages.Count > 0)
            {
                SelectMessage(0);
            }
            
            Debug.Log($"[GameController] Level started: {level.levelName}");
        }
        
        /// <summary>
        /// 切换到下一关
        /// </summary>
        public void NextLevel()
        {
            var levels = GetAllLevels();
            int currentIndex = levels.FindIndex(l => l.levelId == _currentLevel?.levelId);
            if (currentIndex >= 0 && currentIndex < levels.Count - 1)
            {
                StartLevel(levels[currentIndex + 1]);
            }
        }
        
        /// <summary>
        /// 切换到上一关
        /// </summary>
        public void PreviousLevel()
        {
            var levels = GetAllLevels();
            int currentIndex = levels.FindIndex(l => l.levelId == _currentLevel?.levelId);
            if (currentIndex > 0)
            {
                StartLevel(levels[currentIndex - 1]);
            }
        }
        
        /// <summary>
        /// 结束当前关卡并获取结果
        /// </summary>
        public LevelResult EndLevel()
        {
            var result = inputRecorder?.EndLevel();
            if (result != null)
            {
                inputRecorder?.SaveLevelResult(result);
                OnLevelCompleted?.Invoke(result);
                Debug.Log($"[GameController] Level completed: {result.accuracy:P0} accuracy, {result.starRating} stars");
            }
            return result;
        }
        
        /// <summary>
        /// 重新开始当前关卡
        /// </summary>
        public void RestartLevel()
        {
            if (_currentLevel != null)
            {
                StartLevel(_currentLevel);
            }
        }
        
        #endregion
        
        #region Message Management
        
        /// <summary>
        /// 获取当前关卡的消息列表
        /// </summary>
        public List<MessageData> GetCurrentLevelMessages()
        {
            return _currentLevel?.messages ?? new List<MessageData>();
        }
        
        /// <summary>
        /// 获取当前关卡消息数量
        /// </summary>
        public int GetCurrentLevelMessageCount()
        {
            return _currentLevel?.messages?.Count ?? 0;
        }
        
        /// <summary>
        /// 选择指定索引的消息
        /// </summary>
        public void SelectMessage(int index)
        {
            if (_currentLevel?.messages == null) return;
            if (index < 0 || index >= _currentLevel.messages.Count) return;
            
            _currentMessageIndex = index;
            _currentMessage = _currentLevel.messages[index];
            
            // Prepare recorder
            string answer = _currentMessage.GetAnswer();
            inputRecorder?.PrepareForMessage(_currentMessage.messageId, answer);
            
            OnMessageSelected?.Invoke(_currentMessage);
            
            Debug.Log($"[GameController] Message selected: {_currentMessage.description}");
        }
        
        /// <summary>
        /// 选择指定ID的消息
        /// </summary>
        public void SelectMessage(string messageId)
        {
            int index = _currentLevel?.messages?.FindIndex(m => m.messageId == messageId) ?? -1;
            if (index >= 0)
            {
                SelectMessage(index);
            }
        }
        
        /// <summary>
        /// 播放当前选中的消息
        /// </summary>
        public void PlayCurrentMessage()
        {
            if (_currentMessage != null)
            {
                audioPlayer?.PlayMessageByIndex(_currentMessageIndex);
            }
        }
        
        /// <summary>
        /// 播放指定索引的消息
        /// </summary>
        public void PlayMessage(int index)
        {
            SelectMessage(index);
            PlayCurrentMessage();
        }
        
        /// <summary>
        /// 播放下一条消息
        /// </summary>
        public void NextMessage()
        {
            if (_currentLevel?.messages == null) return;
            
            int nextIndex = _currentMessageIndex + 1;
            if (nextIndex < _currentLevel.messages.Count)
            {
                SelectMessage(nextIndex);
                PlayCurrentMessage();
            }
            else
            {
                // All messages completed
                EndLevel();
            }
        }
        
        /// <summary>
        /// 播放上一消息
        /// </summary>
        public void PreviousMessage()
        {
            if (_currentMessageIndex > 0)
            {
                SelectMessage(_currentMessageIndex - 1);
                PlayCurrentMessage();
            }
        }
        
        /// <summary>
        /// 随机播放一条消息
        /// </summary>
        public void PlayRandomMessage()
        {
            if (_currentLevel?.messages == null || _currentLevel.messages.Count == 0) return;
            
            int randomIndex = UnityEngine.Random.Range(0, _currentLevel.messages.Count);
            SelectMessage(randomIndex);
            PlayCurrentMessage();
        }
        
        #endregion
        
        #region Input & Answer
        
        /// <summary>
        /// 提交当前输入（自动根据关卡类型判断）
        /// </summary>
        public void SubmitInput()
        {
            if (_currentLevel == null) return;
            
            if (_currentLevel.levelType == LevelType.InputMorse)
            {
                SubmitMorseInput();
            }
            else
            {
                // For ListenDecode, use the text parameter version
                Debug.LogWarning("[GameController] Use SubmitTextInput(string) for ListenDecode levels");
            }
        }
        
        /// <summary>
        /// 提交摩斯码输入（InputMorse模式）
        /// </summary>
        public void SubmitMorseInput()
        {
            inputRecorder?.SubmitMorseInput();
        }
        
        /// <summary>
        /// 提交文本输入（ListenDecode模式）
        /// </summary>
        public void SubmitTextInput(string text)
        {
            inputRecorder?.SubmitTextInput(text);
        }
        
        /// <summary>
        /// 清空当前输入
        /// </summary>
        public void ClearInput()
        {
            inputHandler?.ClearInput();
        }
        
        /// <summary>
        /// 获取当前摩斯输入
        /// </summary>
        public string GetCurrentMorseInput()
        {
            return inputHandler?.GetCurrentInput() ?? "";
        }
        
        /// <summary>
        /// 将摩斯码转换为文本
        /// </summary>
        public string ConvertMorseToText(string morseCode)
        {
            return inputRecorder?.MorseToText(morseCode) ?? "";
        }
        
        /// <summary>
        /// 获取当前消息的提示文本
        /// </summary>
        public string GetCurrentPrompt()
        {
            if (_currentLevel == null || _currentMessage == null) return "";
            
            if (_currentLevel.levelType == LevelType.InputMorse)
            {
                return $"Listen: '{_currentMessage.text}'\nInput the Morse code";
            }
            else
            {
                return "Listen to the Morse code\nEnter what you heard";
            }
        }
        
        /// <summary>
        /// 获取当前正确答案
        /// </summary>
        public string GetCurrentAnswer()
        {
            return _currentMessage?.GetAnswer() ?? "";
        }
        
        #endregion
        
        #region Playback Control
        
        /// <summary>
        /// 停止播放
        /// </summary>
        public void StopPlayback()
        {
            audioPlayer?.Stop();
            _isPlaying = false;
        }
        
        /// <summary>
        /// 设置播放模式
        /// </summary>
        public void SetPlayMode(PlayMode mode)
        {
            audioPlayer?.SetPlayMode(mode);
        }
        
        /// <summary>
        /// 测试播放点音效
        /// </summary>
        public void TestPlayDot()
        {
            audioPlayer?.PlayMorseCode("·");
        }
        
        /// <summary>
        /// 测试播放划音效
        /// </summary>
        public void TestPlayDash()
        {
            audioPlayer?.PlayMorseCode("-");
        }
        
        #endregion
        
        #region Utility
        
        /// <summary>
        /// 检查当前关卡类型
        /// </summary>
        public bool IsInputMorseLevel()
        {
            return _currentLevel?.levelType == LevelType.InputMorse;
        }
        
        /// <summary>
        /// 检查当前关卡类型
        /// </summary>
        public bool IsListenDecodeLevel()
        {
            return _currentLevel?.levelType == LevelType.ListenDecode;
        }
        
        /// <summary>
        /// 获取当前关卡进度 (0.0 - 1.0)
        /// </summary>
        public float GetLevelProgress()
        {
            if (_currentLevel?.messages == null || _currentLevel.messages.Count == 0) return 0f;
            return (float)(_currentMessageIndex + 1) / _currentLevel.messages.Count;
        }
        
        /// <summary>
        /// 检查是否是最后一题
        /// </summary>
        public bool IsLastMessage()
        {
            if (_currentLevel?.messages == null) return false;
            return _currentMessageIndex >= _currentLevel.messages.Count - 1;
        }
        
        #endregion
    }
}
