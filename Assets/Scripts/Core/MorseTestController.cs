using UnityEngine;
using UnityEngine.UI;

namespace MorseCodeGame
{
    /// <summary>
    /// 摩斯电码功能测试控制器
    /// 用于测试播放和输入功能
    /// </summary>    
    public class MorseTestController : MonoBehaviour
    {
        [Header("组件引用")]
        public MorseAudioPlayer audioPlayer;
        public MorseInputHandler inputHandler;
        public MorseCodeData morseData;
        
        [Header("UI元素")]
        public Button playButton;
        public Button inputButton;
        public Button submitButton;
        public Button clearButton;
        public Text displayText;
        public Text statusText;
        public Image progressBar;
        
        private void Start()
        {
            // 确保有数据
            if (morseData == null)
            {
                morseData = ScriptableObject.CreateInstance<MorseCodeData>();
            }
            if (audioPlayer != null)
            {
                audioPlayer.morseData = morseData;
            }
            
            // 关联音频播放器到输入处理器，提供输入反馈音效
            if (inputHandler != null && audioPlayer != null)
            {
                inputHandler.audioPlayer = audioPlayer;
            }
            
            // 绑定按钮事件
            if (playButton != null)
            {
                playButton.onClick.AddListener(OnPlayButtonClick);
            }
            if (submitButton != null)
            {
                submitButton.onClick.AddListener(OnSubmitClick);
            }
            if (clearButton != null)
            {
                clearButton.onClick.AddListener(OnClearClick);
            }
            
            // 绑定输入事件
            if (inputHandler != null)
            {
                inputHandler.OnDotInput += () => UpdateStatus("输入: 点(·)");
                inputHandler.OnDashInput += () => UpdateStatus("输入: 划(-)");
                inputHandler.OnMorseInput += OnMorseInputChanged;
                inputHandler.OnCharacterSubmit += OnCharacterSubmitted;
            }
            
            // 绑定播放事件
            if (audioPlayer != null)
            {
                audioPlayer.OnPlaybackStart += () => UpdateStatus("正在播放...");
                audioPlayer.OnPlaybackEnd += () => UpdateStatus("播放完成");
            }
            
            UpdateDisplay();
        }
        
        private void Update()
        {
            // 更新进度条（显示长按进度）
            if (progressBar != null && inputHandler != null)
            {
                if (inputHandler.IsPressed)
                {
                    float progress = inputHandler.GetCurrentPressDuration() / inputHandler.dashThreshold;
                    progressBar.fillAmount = Mathf.Clamp01(progress);
                }
                else
                {
                    progressBar.fillAmount = 0f;
                }
            }
        }
        
        private void OnPlayButtonClick()
        {
            if (audioPlayer == null) return;
            
            // 测试播放SOS
            audioPlayer.PlayText("SOS");
            UpdateStatus("播放: SOS");
        }
        
        private void OnSubmitClick()
        {
            if (inputHandler != null)
            {
                inputHandler.ManualSubmit();
            }
        }
        
        private void OnClearClick()
        {
            if (inputHandler != null)
            {
                inputHandler.ClearInput();
            }
            UpdateDisplay();
        }
        
        private void OnMorseInputChanged(string morseCode)
        {
            UpdateDisplay();
        }
        
        private void OnCharacterSubmitted(string morseCode)
        {
            if (morseData != null)
            {
                char? character = morseData.GetCharacter(morseCode);
                string result = character.HasValue ? $"解码: {character.Value}" : "未知字符";
                UpdateStatus($"提交 [{morseCode}] {result}");
            }
            UpdateDisplay();
        }
        
        private void UpdateDisplay()
        {
            if (displayText != null && inputHandler != null)
            {
                string input = inputHandler.GetCurrentInput();
                displayText.text = string.IsNullOrEmpty(input) ? "点击并按住按钮输入" : $"当前输入: {input}";
            }
        }
        
        private void UpdateStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
            Debug.Log($"[MorseTest] {message}");
        }
    }
}
