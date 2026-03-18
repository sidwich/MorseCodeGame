using UnityEngine;
using UnityEngine.UI;

namespace MorseCodeGame
{
    /// <summary>
    /// 简单的摩斯电码测试场景控制器
    /// 自动创建UI元素
    /// </summary>
    public class SimpleMorseTest : MonoBehaviour
    {
        [Header("组件")]
        public MorseAudioPlayer audioPlayer;
        public MorseInputHandler inputHandler;
        public MorseCodeData morseData;
        
        private Text statusText;
        private Text inputText;
        private Text resultText;
        private Image progressImage;
        
        private void Start()
        {
            SetupUI();
            SetupComponents();
            BindEvents();
            
            ShowMessage("摩斯电码测试\n点击按钮输入\n短按=· 长按=-");
        }
        
        private void SetupUI()
        {
            // 创建Canvas
            GameObject canvasGO = new GameObject("Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            
            // 创建状态文本
            GameObject statusGO = CreateText(canvasGO.transform, "StatusText", "摩斯电码测试", 0, 200, 30);
            statusText = statusGO.GetComponent<Text>();
            
            // 创建输入显示文本
            GameObject inputGO = CreateText(canvasGO.transform, "InputText", "", 0, 100, 40);
            inputText = inputGO.GetComponent<Text>();
            inputText.color = Color.yellow;
            
            // 创建结果文本
            GameObject resultGO = CreateText(canvasGO.transform, "ResultText", "", 0, 50, 24);
            resultText = resultGO.GetComponent<Text>();
            resultText.color = Color.green;
            
            // 创建进度条背景
            GameObject progressBG = new GameObject("ProgressBG");
            progressBG.transform.SetParent(canvasGO.transform);
            RectTransform bgRect = progressBG.AddComponent<RectTransform>();
            bgRect.anchoredPosition = new Vector2(0, -50);
            bgRect.sizeDelta = new Vector2(202, 22);
            Image bgImg = progressBG.AddComponent<Image>();
            bgImg.color = Color.gray;
            
            // 创建进度条
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
            
            // 创建输入按钮
            GameObject inputBtnGO = CreateButton(canvasGO.transform, "InputButton", "按住输入", 0, -150);
            Button inputBtn = inputBtnGO.GetComponent<Button>();
            
            // 添加输入处理器到按钮
            MorseInputButton inputBtnComp = inputBtnGO.AddComponent<MorseInputButton>();
            inputBtnComp.handler = inputHandler;
            
            // 创建播放按钮
            GameObject playBtnGO = CreateButton(canvasGO.transform, "PlayButton", "播放SOS", -150, -250);
            Button playBtn = playBtnGO.GetComponent<Button>();
            playBtn.onClick.AddListener(() => {
                if (audioPlayer != null) {
                    audioPlayer.PlayText("SOS");
                    ShowMessage("播放: SOS");
                }
            });
            
            // 创建清空按钮
            GameObject clearBtnGO = CreateButton(canvasGO.transform, "ClearButton", "清空", 150, -250);
            Button clearBtn = clearBtnGO.GetComponent<Button>();
            clearBtn.onClick.AddListener(() => {
                if (inputHandler != null) {
                    inputHandler.ClearInput();
                    UpdateInputDisplay("");
                    ShowResult("");
                }
            });
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
            
            // 使用默认字体
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            
            return go;
        }
        
        private GameObject CreateButton(Transform parent, string name, string text, float x, float y)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(120, 50);
            
            Image img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.5f, 0.8f);
            
            Button btn = go.AddComponent<Button>();
            
            // 创建子对象显示文字
            GameObject textGO = CreateText(go.transform, "Text", text, 0, 0, 20);
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(120, 50);
            
            return go;
        }
        
        private void SetupComponents()
        {
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
            
            // 关联音频播放器到输入处理器，提供输入反馈音效
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
            }
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
