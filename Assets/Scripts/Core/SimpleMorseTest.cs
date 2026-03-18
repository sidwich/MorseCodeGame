using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace MorseCodeGame
{
    /// <summary>
    /// 简单的摩斯电码测试场景控制器
    /// 支持关卡消息系统
    /// </summary>
    public class SimpleMorseTest : MonoBehaviour
    {
        [Header("组件")]
        public MorseAudioPlayer audioPlayer;
        public MorseInputHandler inputHandler;
        public MorseCodeData morseData;
        public MessageDatabase messageDatabase;
        
        private Text statusText;
        private Text inputText;
        private Text resultText;
        private Text levelInfoText;
        private Image progressImage;
        private Transform canvasTransform;
        
        private List<Button> messageButtons = new List<Button>();
        private int currentLevelIndex = 0;
        
        private void Start()
        {
            SetupUI();
            SetupComponents();
            BindEvents();
            
            ShowMessage("摩斯电码测试\n点击按钮输入\n短按=· 长按=-");
        }
        
        private void SetupUI()
        {
            // 创建EventSystem（UI点击必需）
            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                GameObject eventSystemGO = new GameObject("EventSystem");
                eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
            
            // 创建Canvas
            GameObject canvasGO = new GameObject("Canvas");
            canvasTransform = canvasGO.transform;
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            
            // 创建状态文本
            GameObject statusGO = CreateText(canvasTransform, "StatusText", "摩斯电码测试", 0, 350, 30);
            statusText = statusGO.GetComponent<Text>();
            
            // 创建关卡信息文本
            GameObject levelInfoGO = CreateText(canvasTransform, "LevelInfoText", "", 0, 300, 20);
            levelInfoText = levelInfoGO.GetComponent<Text>();
            levelInfoText.color = Color.cyan;
            
            // 创建输入显示文本
            GameObject inputGO = CreateText(canvasTransform, "InputText", "", 0, 150, 40);
            inputText = inputGO.GetComponent<Text>();
            inputText.color = Color.yellow;
            
            // 创建结果文本
            GameObject resultGO = CreateText(canvasTransform, "ResultText", "", 0, 80, 24);
            resultText = resultGO.GetComponent<Text>();
            resultText.color = Color.green;
            
            // 创建进度条
            CreateProgressBar(canvasTransform);
            
            // 创建输入按钮
            GameObject inputBtnGO = CreateButton(canvasTransform, "InputButton", "按住输入", 0, 0);
            Button inputBtn = inputBtnGO.GetComponent<Button>();
            MorseInputButton inputBtnComp = inputBtnGO.AddComponent<MorseInputButton>();
            inputBtnComp.handler = inputHandler;
            
            // 创建播放控制按钮行
            CreatePlaybackControls(canvasTransform);
            
            // 创建关卡切换按钮
            CreateLevelSwitchButtons(canvasTransform);
            
            // 创建消息列表区域
            CreateMessageList(canvasTransform);
            
            // 创建清空按钮
            GameObject clearBtnGO = CreateButton(canvasTransform, "ClearButton", "清空", 200, -250, 100, 40);
            Button clearBtn = clearBtnGO.GetComponent<Button>();
            clearBtn.onClick.AddListener(() => {
                if (inputHandler != null) {
                    inputHandler.ClearInput();
                    UpdateInputDisplay("");
                    ShowResult("");
                }
            });
        }
        
        private void CreateProgressBar(Transform parent)
        {
            // 进度条背景
            GameObject progressBG = new GameObject("ProgressBG");
            progressBG.transform.SetParent(parent);
            RectTransform bgRect = progressBG.AddComponent<RectTransform>();
            bgRect.anchoredPosition = new Vector2(0, 50);
            bgRect.sizeDelta = new Vector2(202, 22);
            Image bgImg = progressBG.AddComponent<Image>();
            bgImg.color = Color.gray;
            
            // 进度条
            GameObject progressGO = new GameObject("ProgressBar");
            progressGO.transform.SetParent(progressBG.transform);
            RectTransform progressRect = progressGO.AddComponent<RectTransform>();
            progressRect.anchoredPosition = Vector2.zero;
            progressRect.sizeDelta = new Vector2(200, 20);
            progressImage = progressGO.AddComponent<Image>();
            progressImage.color = Color.cyan;
            progressImage.type = Image.Type.Filled;
            progressImage.fillMethod = Image.FillMethod.Horizontal;
            progressImage.fillAmount = 0;
        }
        
        private void CreatePlaybackControls(Transform parent)
        {
            // 播放当前消息按钮
            GameObject playCurrentBtnGO = CreateButton(parent, "PlayCurrentBtn", "播放当前", -200, -100, 100, 40);
            Button playCurrentBtn = playCurrentBtnGO.GetComponent<Button>();
            playCurrentBtn.onClick.AddListener(() => {
                if (audioPlayer != null) {
                    audioPlayer.PlayCurrentMessage();
                }
            });
            
            // 播放下一条按钮
            GameObject playNextBtnGO = CreateButton(parent, "PlayNextBtn", "下一条", -50, -100, 100, 40);
            Button playNextBtn = playNextBtnGO.GetComponent<Button>();
            playNextBtn.onClick.AddListener(() => {
                if (audioPlayer != null) {
                    audioPlayer.PlayNextMessage();
                }
            });
            
            // 随机播放按钮
            GameObject playRandomBtnGO = CreateButton(parent, "PlayRandomBtn", "随机", 100, -100, 100, 40);
            Button playRandomBtn = playRandomBtnGO.GetComponent<Button>();
            playRandomBtn.onClick.AddListener(() => {
                if (audioPlayer != null) {
                    audioPlayer.PlayRandomMessage();
                }
            });
        }
        
        private void CreateLevelSwitchButtons(Transform parent)
        {
            // 上一关按钮
            GameObject prevLevelBtnGO = CreateButton(parent, "PrevLevelBtn", "上一关", -250, -160, 80, 40);
            Button prevLevelBtn = prevLevelBtnGO.GetComponent<Button>();
            prevLevelBtn.onClick.AddListener(() => SwitchLevel(-1));
            
            // 关卡名称显示
            GameObject levelNameGO = CreateText(parent, "LevelNameText", "关卡 1", -130, -160, 18);
            levelNameGO.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 40);
            
            // 下一关按钮
            GameObject nextLevelBtnGO = CreateButton(parent, "NextLevelBtn", "下一关", -30, -160, 80, 40);
            Button nextLevelBtn = nextLevelBtnGO.GetComponent<Button>();
            nextLevelBtn.onClick.AddListener(() => SwitchLevel(1));
        }
        
        private void CreateMessageList(Transform parent)
        {
            // 消息列表标题
            GameObject listTitleGO = CreateText(parent, "ListTitle", "消息列表", 180, -100, 18);
            listTitleGO.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 30);
            
            // 创建可滚动区域（简化版，先创建几个固定按钮）
            float startY = -140;
            float spacing = 45;
            
            for (int i = 0; i < 5; i++)
            {
                int index = i;
                GameObject msgBtnGO = CreateButton(parent, $"MsgBtn_{i}", $"消息 {i + 1}", 180, startY - i * spacing, 140, 40);
                Button msgBtn = msgBtnGO.GetComponent<Button>();
                msgBtn.onClick.AddListener(() => PlayMessageByIndex(index));
                messageButtons.Add(msgBtn);
                
                // 初始隐藏
                msgBtnGO.SetActive(false);
            }
        }
        
        private void SwitchLevel(int delta)
        {
            if (MessageDatabase.Instance == null) return;
            
            var levels = MessageDatabase.Instance.GetAllLevels();
            if (levels.Count == 0) return;
            
            currentLevelIndex = Mathf.Clamp(currentLevelIndex + delta, 0, levels.Count - 1);
            var level = levels[currentLevelIndex];
            
            // 设置音频播放器关卡
            if (audioPlayer != null)
            {
                audioPlayer.SetLevel(level.levelId);
            }
            
            // 更新UI
            UpdateLevelInfo(level);
            UpdateMessageButtons(level);
            
            ShowMessage($"切换到: {level.levelName}");
        }
        
        private void UpdateLevelInfo(LevelData level)
        {
            if (levelInfoText != null)
            {
                levelInfoText.text = $"[{level.levelId}] {level.levelName}\n{level.description}";
            }
            
            // 更新关卡名称按钮文本
            var levelNameText = GameObject.Find("LevelNameText")?.GetComponent<Text>();
            if (levelNameText != null)
            {
                levelNameText.text = level.levelName;
            }
        }
        
        private void UpdateMessageButtons(LevelData level)
        {
            if (level.messages == null) return;
            
            for (int i = 0; i < messageButtons.Count; i++)
            {
                if (i < level.messages.Count)
                {
                    var msg = level.messages[i];
                    messageButtons[i].gameObject.SetActive(true);
                    var text = messageButtons[i].GetComponentInChildren<Text>();
                    if (text != null)
                    {
                        text.text = $"{i + 1}. {msg.description}";
                    }
                }
                else
                {
                    messageButtons[i].gameObject.SetActive(false);
                }
            }
        }
        
        private void PlayMessageByIndex(int index)
        {
            if (audioPlayer != null)
            {
                audioPlayer.PlayMessageByIndex(index);
            }
        }
        
        private GameObject CreateText(Transform parent, string name, string content, float x, float y, int fontSize)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(600, 100);
            
            Text text = go.AddComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            
            return go;
        }
        
        private GameObject CreateButton(Transform parent, string name, string text, float x, float y, float width = 120, float height = 50)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
            
            Image img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.5f, 0.8f);
            
            Button btn = go.AddComponent<Button>();
            
            GameObject textGO = CreateText(go.transform, "Text", text, 0, 0, Mathf.Max(16, (int)(height * 0.4f)));
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(width, height);
            
            return go;
        }
        
        private void SetupComponents()
        {
            // 创建或获取 MessageDatabase
            if (messageDatabase == null)
            {
                var existingDB = FindObjectOfType<MessageDatabase>();
                if (existingDB != null)
                {
                    messageDatabase = existingDB;
                }
                else
                {
                    GameObject dbGO = new GameObject("MessageDatabase");
                    messageDatabase = dbGO.AddComponent<MessageDatabase>();
                }
            }
            
            // 创建音频播放器
            if (audioPlayer == null)
            {
                GameObject audioGO = new GameObject("MorseAudioPlayer");
                audioPlayer = audioGO.AddComponent<MorseAudioPlayer>();
            }
            
            // 创建输入处理器
            if (inputHandler == null)
            {
                GameObject inputGO = new GameObject("MorseInputHandler");
                inputHandler = inputGO.AddComponent<MorseInputHandler>();
            }
            
            // 关联音频播放器到输入处理器
            if (inputHandler != null && audioPlayer != null)
            {
                inputHandler.audioPlayer = audioPlayer;
            }
            
            // 创建数据
            if (morseData == null)
            {
                morseData = ScriptableObject.CreateInstance<MorseCodeData>();
            }
            
            audioPlayer.morseData = morseData;
        }
        
        private void BindEvents()
        {
            // 等待数据库加载完成
            if (messageDatabase != null)
            {
                messageDatabase.OnDataLoaded += OnDatabaseLoaded;
                messageDatabase.OnLoadError += (error) => {
                    ShowMessage($"数据加载失败: {error}");
                };
            }
            
            if (inputHandler != null)
            {
                inputHandler.OnDotInput += () => ShowMessage("输入: 点(·)");
                inputHandler.OnDashInput += () => ShowMessage("输入: 划(-)");
                inputHandler.OnMorseInput += (code) => UpdateInputDisplay(code);
                inputHandler.OnCharacterSubmit += (code) => {
                    char? ch = morseData.GetCharacter(code);
                    string result = ch.HasValue ? $"解码: {ch.Value}" : "未知";
                    ShowResult($"[{code}] = {result}");
                    UpdateInputDisplay("");
                };
            }
            
            if (audioPlayer != null)
            {
                audioPlayer.OnPlaybackStart += () => ShowMessage("正在播放...");
                audioPlayer.OnPlaybackEnd += () => ShowMessage("播放完成");
                audioPlayer.OnMessageStart += (msg) => ShowMessage($"播放: {msg.description}");
            }
        }
        
        private void OnDatabaseLoaded()
        {
            // 数据库加载完成，初始化第一关
            SwitchLevel(0);
        }
        
        private void Update()
        {
            // 更新进度条
            if (progressImage != null && inputHandler != null && inputHandler.IsPressed)
            {
                float progress = inputHandler.GetCurrentPressDuration() / inputHandler.dashThreshold;
                progressImage.fillAmount = Mathf.Clamp01(progress);
                progressImage.color = progress >= 1f ? Color.red : Color.cyan;
            }
            else if (progressImage != null)
            {
                progressImage.fillAmount = 0;
            }
        }
        
        private void ShowMessage(string msg)
        {
            if (statusText != null)
                statusText.text = msg;
            Debug.Log($"[MorseTest] {msg}");
        }
        
        private void UpdateInputDisplay(string code)
        {
            if (inputText != null)
                inputText.text = code;
        }
        
        private void ShowResult(string result)
        {
            if (resultText != null)
                resultText.text = result;
        }
    }
    
    /// <summary>
    /// 按钮输入桥接器
    /// </summary>
    public class MorseInputButton : MonoBehaviour, UnityEngine.EventSystems.IPointerDownHandler, UnityEngine.EventSystems.IPointerUpHandler
    {
        public MorseInputHandler handler;
        
        public void OnPointerDown(UnityEngine.EventSystems.PointerEventData eventData)
        {
            handler?.OnPointerDown(eventData);
        }
        
        public void OnPointerUp(UnityEngine.EventSystems.PointerEventData eventData)
        {
            handler?.OnPointerUp(eventData);
        }
    }
}
