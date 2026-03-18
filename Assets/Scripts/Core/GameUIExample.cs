using UnityEngine;
using UnityEngine.UI;

namespace MorseCodeGame
{
    /// <summary>
    /// 使用 GameController API 的示例 UI
    /// 展示如何调用各个功能函数
    /// </summary>
    public class GameUIExample : MonoBehaviour
    {
        [Header("UI References")]
        public Text statusText;
        public Text promptText;
        public Text resultText;
        public Text levelInfoText;
        public InputField textInput;
        public Button submitButton;
        public Button playButton;
        public Button nextButton;
        public Button clearButton;
        public Slider progressSlider;
        
        [Header("Controller")]
        public GameController gameController;
        
        private void Start()
        {
            // 获取或创建 GameController
            if (gameController == null)
            {
                gameController = FindObjectOfType<GameController>();
                if (gameController == null)
                {
                    GameObject go = new GameObject("GameController");
                    gameController = go.AddComponent<GameController>();
                }
            }
            
            // 订阅事件
            SubscribeToEvents();
            
            // 绑定按钮
            BindButtons();
            
            // 等待数据加载后自动开始
            if (gameController.messageDatabase != null)
            {
                gameController.messageDatabase.OnDataLoaded += () =>
                {
                    // 自动开始第一关
                    gameController.StartLevel("LEVEL_001");
                };
            }
        }
        
        private void SubscribeToEvents()
        {
            // 关卡事件
            gameController.OnLevelStarted += (level) =>
            {
                UpdateLevelInfo(level);
                ShowStatus($"Level: {level.levelName}");
            };
            
            // 消息事件
            gameController.OnMessageSelected += (msg) =>
            {
                promptText.text = gameController.GetCurrentPrompt();
                ShowStatus($"Selected: {msg.description}");
            };
            
            gameController.OnPlaybackStateChanged += (isPlaying) =>
            {
                playButton.interactable = !isPlaying;
                ShowStatus(isPlaying ? "Playing..." : "Playback complete");
            };
            
            // 答案事件
            gameController.OnAnswerCorrect += (record) =>
            {
                ShowResult($"Correct! '{record.playerInput}' = '{record.correctAnswer}'");
                ShowStatus("Correct!");
                
                // 延迟后自动下一题
                Invoke(nameof(GoToNext), 1.5f);
            };
            
            gameController.OnAnswerWrong += (record) =>
            {
                ShowResult($"Wrong! Your input: '{record.playerInput}'\nCorrect: '{record.correctAnswer}'");
                ShowStatus("Wrong! Try again");
            };
            
            // 关卡完成
            gameController.OnLevelCompleted += (result) =>
            {
                ShowStatus($"Level Complete! Stars: {result.starRating}");
                ShowResult($"Accuracy: {result.accuracy:P0}\n{result.correctCount}/{result.totalCount} correct");
            };
            
            // 输入变化
            gameController.OnMorseInputChanged += (code) =>
            {
                if (gameController.IsInputMorseLevel())
                {
                    resultText.text = $"Input: {code}";
                }
            };
        }
        
        private void BindButtons()
        {
            // 播放按钮 - 调用 PlayCurrentMessage()
            if (playButton != null)
                playButton.onClick.AddListener(() => gameController.PlayCurrentMessage());
            
            // 提交按钮 - 根据关卡类型调用不同方法
            if (submitButton != null)
                submitButton.onClick.AddListener(OnSubmit);
            
            // 下一题按钮 - 调用 NextMessage()
            if (nextButton != null)
                nextButton.onClick.AddListener(GoToNext);
            
            // 清空按钮 - 调用 ClearInput()
            if (clearButton != null)
                clearButton.onClick.AddListener(() => 
                {
                    gameController.ClearInput();
                    if (textInput != null) textInput.text = "";
                    ShowResult("");
                });
        }
        
        private void OnSubmit()
        {
            // 根据关卡类型调用对应提交方法
            if (gameController.IsInputMorseLevel())
            {
                // InputMorse 模式：提交摩斯码输入
                gameController.SubmitMorseInput();
            }
            else if (textInput != null)
            {
                // ListenDecode 模式：提交文本输入
                gameController.SubmitTextInput(textInput.text);
                textInput.text = "";
            }
        }
        
        private void GoToNext()
        {
            gameController.NextMessage();
        }
        
        private void Update()
        {
            // 更新进度条
            if (progressSlider != null)
            {
                progressSlider.value = gameController.GetLevelProgress();
            }
            
            // 根据关卡类型显示/隐藏输入方式
            if (gameController.CurrentLevel != null)
            {
                bool isInputMorse = gameController.IsInputMorseLevel();
                // 这里可以控制摩斯输入按钮和文本输入框的显示
            }
        }
        
        private void UpdateLevelInfo(LevelData level)
        {
            if (levelInfoText != null)
            {
                string typeStr = level.levelType == LevelType.InputMorse ? "[Input Morse]" : "[Listen & Decode]";
                levelInfoText.text = $"{typeStr} {level.levelName}\n{level.description}";
            }
        }
        
        private void ShowStatus(string msg)
        {
            if (statusText != null)
                statusText.text = msg;
            Debug.Log($"[GameUI] {msg}");
        }
        
        private void ShowResult(string result)
        {
            if (resultText != null)
                resultText.text = result;
        }
    }
}
