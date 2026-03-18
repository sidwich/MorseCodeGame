using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace MorseCodeGame
{
    /// <summary>
    /// 摩斯电码测试场景控制器
    /// 支持关卡消息系统和答案验证
    /// </summary>
    public class SimpleMorseTest : MonoBehaviour
    {
        [Header("Components")]
        public MorseAudioPlayer audioPlayer;
        public MorseInputHandler inputHandler;
        public MorseCodeData morseData;
        public MessageDatabase messageDatabase;
        public InputRecorder inputRecorder;
        
        private Text statusText;
        private Text inputText;
        private Text resultText;
        private Text levelInfoText;
        private Text promptText;
        private Image progressImage;
        private Transform canvasTransform;
        private InputField textInputField;
        
        private List<Button> messageButtons = new List<Button>();
        private int currentLevelIndex = 0;
        private LevelData currentLevel;
        private MessageData currentMessage;
        
        private void Start()
        {
            SetupUI();
            SetupComponents();
            BindEvents();
            
            ShowMessage("Morse Code Game\nSelect a level to start");
        }
        
        private void SetupUI()
        {
            // Create EventSystem
            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                GameObject eventSystemGO = new GameObject("EventSystem");
                eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
            
            // Create Canvas
            GameObject canvasGO = new GameObject("Canvas");
            canvasTransform = canvasGO.transform;
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            
            // Status text
            GameObject statusGO = CreateText(canvasTransform, "StatusText", "Morse Code Game", 0, 380, 28);
            statusText = statusGO.GetComponent<Text>();
            
            // Level info
            GameObject levelInfoGO = CreateText(canvasTransform, "LevelInfoText", "", 0, 330, 18);
            levelInfoText = levelInfoGO.GetComponent<Text>();
            levelInfoText.color = Color.cyan;
            
            // Prompt text (what player should do)
            GameObject promptGO = CreateText(canvasTransform, "PromptText", "", 0, 280, 20);
            promptText = promptGO.GetComponent<Text>();
            promptText.color = Color.yellow;
            
            // Input display
            GameObject inputGO = CreateText(canvasTransform, "InputText", "", 0, 180, 36);
            inputText = inputGO.GetComponent<Text>();
            inputText.color = Color.green;
            
            // Result text
            GameObject resultGO = CreateText(canvasTransform, "ResultText", "", 0, 120, 22);
            resultText = resultGO.GetComponent<Text>();
            resultText.color = Color.white;
            
            // Progress bar
            CreateProgressBar(canvasTransform);
            
            // Morse input button (for InputMorse mode)
            GameObject inputBtnGO = CreateButton(canvasTransform, "InputButton", "Hold to Input", 0, 20);
            Button inputBtn = inputBtnGO.GetComponent<Button>();
            MorseInputButton inputBtnComp = inputBtnGO.AddComponent<MorseInputButton>();
            inputBtnComp.handler = inputHandler;
            
            // Text input field (for ListenDecode mode)
            CreateTextInputField(canvasTransform);
            
            // Control buttons
            CreateControlButtons(canvasTransform);
            
            // Level switch buttons
            CreateLevelSwitchButtons(canvasTransform);
            
            // Message list
            CreateMessageList(canvasTransform);
        }
        
        private void CreateProgressBar(Transform parent)
        {
            GameObject progressBG = new GameObject("ProgressBG");
            progressBG.transform.SetParent(parent);
            RectTransform bgRect = progressBG.AddComponent<RectTransform>();
            bgRect.anchoredPosition = new Vector2(0, 70);
            bgRect.sizeDelta = new Vector2(202, 22);
            Image bgImg = progressBG.AddComponent<Image>();
            bgImg.color = Color.gray;
            
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
        
        private void CreateTextInputField(Transform parent)
        {
            // Container
            GameObject container = new GameObject("TextInputContainer");
            container.transform.SetParent(parent);
            RectTransform containerRect = container.AddComponent<RectTransform>();
            containerRect.anchoredPosition = new Vector2(0, -30);
            containerRect.sizeDelta = new Vector2(300, 60);
            
            // Background
            Image bg = container.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            
            // Input field
            GameObject inputGO = new GameObject("TextInput");
            inputGO.transform.SetParent(container.transform);
            RectTransform inputRect = inputGO.AddComponent<RectTransform>();
            inputRect.anchoredPosition = Vector2.zero;
            inputRect.sizeDelta = new Vector2(280, 40);
            
            textInputField = inputGO.AddComponent<InputField>();
            
            // Create text component for input field
            GameObject textGO = CreateText(inputGO.transform, "Text", "", 0, 0, 20);
            textInputField.textComponent = textGO.GetComponent<Text>();
            textInputField.textComponent.color = Color.white;
            textInputField.textComponent.alignment = TextAnchor.MiddleCenter;
            
            // Placeholder
            GameObject placeholderGO = CreateText(inputGO.transform, "Placeholder", "Enter answer...", 0, 0, 18);
            Text placeholderText = placeholderGO.GetComponent<Text>();
            placeholderText.color = new Color(0.5f, 0.5f, 0.5f);
            textInputField.placeholder = placeholderText;
            
            // Initially hidden
            container.SetActive(false);
        }
        
        private void CreateControlButtons(Transform parent)
        {
            // Submit button
            GameObject submitBtnGO = CreateButton(parent, "SubmitBtn", "Submit", -150, -100, 120, 45);
            Button submitBtn = submitBtnGO.GetComponent<Button>();
            submitBtn.onClick.AddListener(OnSubmitClick);
            
            // Play message button
            GameObject playBtnGO = CreateButton(parent, "PlayBtn", "Play", 0, -100, 120, 45);
            Button playBtn = playBtnGO.GetComponent<Button>();
            playBtn.onClick.AddListener(OnPlayCurrentMessage);
            
            // Clear button
            GameObject clearBtnGO = CreateButton(parent, "ClearBtn", "Clear", 150, -100, 120, 45);
            Button clearBtn = clearBtnGO.GetComponent<Button>();
            clearBtn.onClick.AddListener(OnClearClick);
            
            // Next message button
            GameObject nextBtnGO = CreateButton(parent, "NextBtn", "Next Message", 200, -160, 140, 40);
            Button nextBtn = nextBtnGO.GetComponent<Button>();
            nextBtn.onClick.AddListener(OnNextMessage);
        }
        
        private void CreateLevelSwitchButtons(Transform parent)
        {
            GameObject prevBtnGO = CreateButton(parent, "PrevLevelBtn", "Previous", -250, -160, 100, 40);
            Button prevBtn = prevBtnGO.GetComponent<Button>();
            prevBtn.onClick.AddListener(() => SwitchLevel(-1));
            
            GameObject nextBtnGO = CreateButton(parent, "NextLevelBtn", "Next", -100, -160, 100, 40);
            Button nextBtn = nextBtnGO.GetComponent<Button>();
            nextBtn.onClick.AddListener(() => SwitchLevel(1));
        }
        
        private void CreateMessageList(Transform parent)
        {
            GameObject listTitleGO = CreateText(parent, "ListTitle", "Messages", 250, -20, 18);
            listTitleGO.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 30);
            
            float startY = -60;
            float spacing = 45;
            
            for (int i = 0; i < 5; i++)
            {
                int index = i;
                GameObject msgBtnGO = CreateButton(parent, $"MsgBtn_{i}", $"Msg {i + 1}", 250, startY - i * spacing, 140, 40);
                Button msgBtn = msgBtnGO.GetComponent<Button>();
                msgBtn.onClick.AddListener(() => SelectMessage(index));
                messageButtons.Add(msgBtn);
                msgBtnGO.SetActive(false);
            }
        }
        
        private void OnSubmitClick()
        {
            if (currentLevel == null || currentMessage == null) return;
            
            if (currentLevel.levelType == LevelType.InputMorse)
            {
                // Submit morse code input
                inputRecorder.SubmitMorseInput();
            }
            else
            {
                // Submit text input
                if (textInputField != null)
                {
                    inputRecorder.SubmitTextInput(textInputField.text);
                    textInputField.text = "";
                }
            }
        }
        
        private void OnPlayCurrentMessage()
        {
            if (currentMessage != null)
            {
                audioPlayer.PlayMessageByIndex(GetCurrentMessageIndex());
            }
        }
        
        private void OnClearClick()
        {
            inputHandler?.ClearInput();
            if (textInputField != null) textInputField.text = "";
            UpdateInputDisplay("");
            ShowResult("");
        }
        
        private void OnNextMessage()
        {
            if (currentLevel == null) return;
            
            int nextIndex = GetCurrentMessageIndex() + 1;
            if (nextIndex < currentLevel.messages.Count)
            {
                SelectMessage(nextIndex);
            }
            else
            {
                // Level complete
                EndLevel();
            }
        }
        
        private int GetCurrentMessageIndex()
        {
            if (currentLevel?.messages == null) return 0;
            return currentLevel.messages.FindIndex(m => m.messageId == currentMessage?.messageId);
        }
        
        private void SelectMessage(int index)
        {
            if (currentLevel?.messages == null) return;
            if (index < 0 || index >= currentLevel.messages.Count) return;
            
            currentMessage = currentLevel.messages[index];
            
            // Prepare recorder
            string answer = currentMessage.GetAnswer();
            inputRecorder.PrepareForMessage(currentMessage.messageId, answer);
            
            // Update UI
            UpdatePromptText();
            ShowMessage($"Selected: {currentMessage.description}");
            
            // Auto play
            audioPlayer.PlayMessageByIndex(index);
        }
        
        private void UpdatePromptText()
        {
            if (currentLevel == null || currentMessage == null) return;
            
            if (currentLevel.levelType == LevelType.InputMorse)
            {
                promptText.text = $"Listen: '{currentMessage.text}'\nInput the Morse code";
            }
            else
            {
                promptText.text = $"Listen to the Morse code\nEnter what you heard";
            }
        }
        
        private void SwitchLevel(int delta)
        {
            if (MessageDatabase.Instance == null) return;
            
            var levels = MessageDatabase.Instance.GetAllLevels();
            if (levels.Count == 0) return;
            
            currentLevelIndex = Mathf.Clamp(currentLevelIndex + delta, 0, levels.Count - 1);
            StartLevel(levels[currentLevelIndex]);
        }
        
        private void StartLevel(LevelData level)
        {
            currentLevel = level;
            
            // Set audio player level
            if (audioPlayer != null)
            {
                audioPlayer.SetLevel(level.levelId);
            }
            
            // Start recording
            inputRecorder.StartLevel(level.levelId);
            
            // Update UI
            UpdateLevelInfo(level);
            UpdateMessageButtons(level);
            UpdateInputMode(level.levelType);
            
            // Auto select first message
            if (level.messages.Count > 0)
            {
                SelectMessage(0);
            }
            
            ShowMessage($"Level: {level.levelName}");
        }
        
        private void EndLevel()
        {
            var result = inputRecorder.EndLevel();
            inputRecorder.SaveLevelResult(result);
            
            ShowMessage($"Level Complete! Accuracy: {result.accuracy:P0} Stars: {result.starRating}");
            ShowResult($"Correct: {result.correctCount}/{result.totalCount}\nTime: {result.totalTime:F1}s");
        }
        
        private void UpdateInputMode(LevelType type)
        {
            // Show/hide input methods based on level type
            GameObject morseInput = GameObject.Find("InputButton");
            GameObject textInput = GameObject.Find("TextInputContainer");
            
            if (morseInput != null)
                morseInput.SetActive(type == LevelType.InputMorse);
            
            if (textInput != null)
                textInput.SetActive(type == LevelType.ListenDecode);
        }
        
        private void UpdateLevelInfo(LevelData level)
        {
            if (levelInfoText != null)
            {
                string typeText = level.levelType == LevelType.InputMorse ? "[Input Morse]" : "[Listen & Decode]";
                levelInfoText.text = $"{typeText} {level.levelName}\n{level.description}";
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
            // MessageDatabase
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
            
            // InputRecorder
            if (inputRecorder == null)
            {
                var existingRecorder = FindObjectOfType<InputRecorder>();
                if (existingRecorder != null)
                {
                    inputRecorder = existingRecorder;
                }
                else
                {
                    GameObject recorderGO = new GameObject("InputRecorder");
                    inputRecorder = recorderGO.AddComponent<InputRecorder>();
                }
            }
            
            // AudioPlayer
            if (audioPlayer == null)
            {
                GameObject audioGO = new GameObject("MorseAudioPlayer");
                audioPlayer = audioGO.AddComponent<MorseAudioPlayer>();
            }
            
            // InputHandler
            if (inputHandler == null)
            {
                GameObject inputGO = new GameObject("MorseInputHandler");
                inputHandler = inputGO.AddComponent<MorseInputHandler>();
            }
            
            // Connect references
            if (inputHandler != null && audioPlayer != null)
            {
                inputHandler.audioPlayer = audioPlayer;
            }
            
            if (inputRecorder != null)
            {
                inputRecorder.inputHandler = inputHandler;
                inputRecorder.audioPlayer = audioPlayer;
                inputRecorder.morseData = morseData;
            }
            
            if (morseData == null)
            {
                morseData = ScriptableObject.CreateInstance<MorseCodeData>();
            }
            
            audioPlayer.morseData = morseData;
        }
        
        private void BindEvents()
        {
            if (messageDatabase != null)
            {
                messageDatabase.OnDataLoaded += () => SwitchLevel(0);
            }
            
            if (inputHandler != null)
            {
                inputHandler.OnDotInput += () => UpdateInputDisplay(inputHandler.GetCurrentInput());
                inputHandler.OnDashInput += () => UpdateInputDisplay(inputHandler.GetCurrentInput());
                inputHandler.OnMorseInput += (code) => UpdateInputDisplay(code);
            }
            
            if (inputRecorder != null)
            {
                inputRecorder.OnAnswerCorrect += OnAnswerCorrect;
                inputRecorder.OnAnswerWrong += OnAnswerWrong;
            }
            
            if (audioPlayer != null)
            {
                audioPlayer.OnPlaybackStart += () => ShowMessage("Playing...");
                audioPlayer.OnPlaybackEnd += () => ShowMessage("Playback complete");
            }
        }
        
        private void OnAnswerCorrect(InputRecord record)
        {
            ShowMessage("Correct!");
            ShowResult($"Right! '{record.playerInput}' = '{record.correctAnswer}'");
            inputHandler?.ClearInput();
            
            // Auto advance after delay
            Invoke(nameof(OnNextMessage), 1.5f);
        }
        
        private void OnAnswerWrong(InputRecord record)
            
        {
            ShowMessage("Wrong! Try again");
            ShowResult($"Your input: '{record.playerInput}'\nCorrect: '{record.correctAnswer}'");
        }
        
        private void Update()
        {
            // Update progress bar
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
