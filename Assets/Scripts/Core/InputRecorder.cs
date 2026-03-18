using UnityEngine;
using System;
using System.Collections.Generic;

namespace MorseCodeGame
{
    /// <summary>
    /// 输入记录器 - 记录玩家所有输入并提供验证
    /// </summary>
    public class InputRecorder : MonoBehaviour
    {
        [Header("References")]
        public MorseInputHandler inputHandler;
        public MorseAudioPlayer audioPlayer;
        public MorseCodeData morseData;
        
        // 当前关卡记录
        private LevelResult _currentResult;
        private InputRecord _currentRecord;
        private float _levelStartTime;
        private string _currentMessageId;
        private string _correctAnswer;
        
        // 事件
        public event Action<InputRecord> OnInputSubmitted;
        public event Action<InputRecord> OnAnswerCorrect;
        public event Action<InputRecord> OnAnswerWrong;
        public event Action<LevelResult> OnLevelCompleted;
        
        // 状态
        public bool IsRecording { get; private set; }
        public LevelResult CurrentResult => _currentResult;
        
        private void Awake()
        {
            if (inputHandler == null)
                inputHandler = FindObjectOfType<MorseInputHandler>();
            if (audioPlayer == null)
                audioPlayer = FindObjectOfType<MorseAudioPlayer>();
        }
        
        /// <summary>
        /// 开始记录关卡
        /// </summary>
        public void StartLevel(string levelId)
        {
            _currentResult = new LevelResult { levelId = levelId };
            _levelStartTime = Time.time;
            IsRecording = true;
            Debug.Log($"[InputRecorder] 开始记录关卡: {levelId}");
        }
        
        /// <summary>
        /// 准备接收某个消息的输入
        /// </summary>
        public void PrepareForMessage(string messageId, string correctAnswer)
        {
            _currentMessageId = messageId;
            _correctAnswer = correctAnswer?.ToUpper().Trim();
            _currentRecord = new InputRecord
            {
                messageId = messageId,
                correctAnswer = _correctAnswer,
                timestamp = Time.time - _levelStartTime,
                attemptCount = 0
            };
            Debug.Log($"[InputRecorder] 准备接收输入 - 正确答案: {_correctAnswer}");
        }
        
        /// <summary>
        /// 提交玩家输入
        /// </summary>
        public void SubmitInput(string playerInput)
        {
            if (!IsRecording || _currentRecord == null) return;
            
            string normalizedInput = NormalizeInput(playerInput);
            _currentRecord.playerInput = normalizedInput;
            _currentRecord.attemptCount++;
            
            // 验证答案
            _currentRecord.isCorrect = ValidateAnswer(normalizedInput, _correctAnswer);
            
            // 记录到结果中
            _currentResult.records.Add(_currentRecord);
            
            // 触发事件
            OnInputSubmitted?.Invoke(_currentRecord);
            
            if (_currentRecord.isCorrect)
            {
                OnAnswerCorrect?.Invoke(_currentRecord);
                Debug.Log($"[InputRecorder] 回答正确: {normalizedInput}");
            }
            else
            {
                OnAnswerWrong?.Invoke(_currentRecord);
                Debug.Log($"[InputRecorder] 回答错误: {normalizedInput}, 正确: {_correctAnswer}");
            }
            
            // 创建新的记录用于下一次尝试（如果需要）
            _currentRecord = new InputRecord
            {
                messageId = _currentMessageId,
                correctAnswer = _correctAnswer,
                timestamp = Time.time - _levelStartTime,
                attemptCount = _currentRecord.attemptCount
            };
        }
        
        /// <summary>
        /// 提交当前输入处理器的摩斯码输入
        /// </summary>
        public void SubmitMorseInput()
        {
            if (inputHandler == null) return;
            
            string morseCode = inputHandler.GetCurrentInput();
            SubmitInput(morseCode);
            inputHandler.ClearInput();
        }
        
        /// <summary>
        /// 提交文本输入（用于听译模式）
        /// </summary>
        public void SubmitTextInput(string text)
        {
            SubmitInput(text);
        }
        
        /// <summary>
        /// 结束关卡并生成结果
        /// </summary>
        public LevelResult EndLevel()
        {
            if (!IsRecording) return null;
            
            IsRecording = false;
            _currentResult.totalTime = Time.time - _levelStartTime;
            _currentResult.CalculateStats();
            
            OnLevelCompleted?.Invoke(_currentResult);
            Debug.Log($"[InputRecorder] 关卡结束 - 正确率: {_currentResult.accuracy:P0}, 星级: {_currentResult.starRating}");
            
            return _currentResult;
        }
        
        /// <summary>
        /// 获取当前输入的实时摩斯码
        /// </summary>
        public string GetCurrentMorseInput()
        {
            return inputHandler?.GetCurrentInput() ?? "";
        }
        
        /// <summary>
        /// 将摩斯码转换为文本
        /// </summary>
        public string MorseToText(string morseCode)
        {
            if (morseData == null || string.IsNullOrEmpty(morseCode)) return "";
            
            char? ch = morseData.GetCharacter(morseCode);
            return ch?.ToString() ?? "";
        }
        
        /// <summary>
        /// 清空当前输入
        /// </summary>
        public void ClearInput()
        {
            inputHandler?.ClearInput();
        }
        
        /// <summary>
        /// 验证答案
        /// </summary>
        private bool ValidateAnswer(string input, string answer)
        {
            if (string.IsNullOrEmpty(answer)) return true;
            
            string normalizedInput = NormalizeInput(input);
            string normalizedAnswer = NormalizeInput(answer);
            
            return normalizedInput == normalizedAnswer;
        }
        
        /// <summary>
        /// 标准化输入（统一格式）
        /// </summary>
        private string NormalizeInput(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            
            // 转大写、去首尾空格、统一摩斯码符号
            return input.ToUpper()
                       .Trim()
                       .Replace(".", "·")
                       .Replace("—", "-")
                       .Replace("_", "-");
        }
        
        /// <summary>
        /// 获取关卡历史记录（可从存档读取）
        /// </summary>
        public List<LevelResult> GetLevelHistory()
        {
            // TODO: 从PlayerPrefs或文件读取历史记录
            return new List<LevelResult>();
        }
        
        /// <summary>
        /// 保存关卡结果
        /// </summary>
        public void SaveLevelResult(LevelResult result)
        {
            // TODO: 实现存档逻辑
            string json = JsonUtility.ToJson(result);
            PlayerPrefs.SetString($"LevelResult_{result.levelId}", json);
            PlayerPrefs.Save();
            Debug.Log($"[InputRecorder] 保存关卡结果: {result.levelId}");
        }
    }
}
