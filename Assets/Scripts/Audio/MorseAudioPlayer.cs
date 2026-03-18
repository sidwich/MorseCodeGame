using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MorseCodeGame
{
    /// <summary>
    /// 摩斯电码音频播放器
    /// 使用预制音频文件播放摩斯电码，支持关卡消息系统
    /// </summary>
    public class MorseAudioPlayer : MonoBehaviour
    {
        [Header("音频资源")]
        [Tooltip("点(·)的音频文件")]
        public AudioClip dotClip;
        
        [Tooltip("划(-)的音频文件")]
        public AudioClip dashClip;
        
        [Header("时序设置")]
        [Tooltip("点(·)的持续时间(秒)，如果为0则使用音频文件长度")]
        public float dotDuration = 0.1f;
        
        [Tooltip("字符间间隔(秒)")]
        public float symbolGap = 0.1f;
        
        [Header("音量")]
        [Range(0f, 1f)]
        public float volume = 0.8f;
        
        [Header("参考数据")]
        public MorseCodeData morseData;
        
        [Header("关卡消息")]
        [Tooltip("当前关卡ID")]
        public string currentLevelId = "LEVEL_001";
        
        [Tooltip("消息播放模式")]
        public PlayMode playMode = PlayMode.Sequential;
        
        private AudioSource _audioSource;
        private bool _isPlaying = false;
        
        // 关卡播放状态
        private int _currentMessageIndex = 0;
        private List<int> _shuffleQueue = new List<int>();
        private MessageData _currentMessage = null;
        
        // 事件
        public System.Action OnPlaybackStart;
        public System.Action OnPlaybackEnd;
        public System.Action<char> OnSymbolPlay;
        public System.Action<MessageData> OnMessageStart;
        public System.Action<MessageData> OnMessageEnd;
        
        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            _audioSource.playOnAwake = false;
            _audioSource.volume = volume;
            _audioSource.spatialBlend = 0f;
        }
        
        private void OnValidate()
        {
            if (_audioSource != null)
            {
                _audioSource.volume = volume;
            }
        }

        #region Basic Playback

        /// <summary>
        /// 播放单个字符的摩斯电码
        /// </summary>
        public void PlayCharacter(char character)
        {
            if (morseData == null) return;
            
            string code = morseData.GetCode(character);
            if (!string.IsNullOrEmpty(code))
            {
                StartCoroutine(PlayCodeSequence(code));
            }
        }
        
        /// <summary>
        /// 播放摩斯码字符串
        /// </summary>
        public void PlayMorseCode(string morseCode)
        {
            if (!string.IsNullOrEmpty(morseCode))
            {
                StartCoroutine(PlayCodeSequence(morseCode));
            }
        }
        
        /// <summary>
        /// 播放文本
        /// </summary>
        public void PlayText(string text)
        {
            if (string.IsNullOrEmpty(text) || morseData == null) return;
            StartCoroutine(PlayTextSequence(text));
        }

        #endregion

        #region Level Message Playback

        /// <summary>
        /// 设置当前关卡
        /// </summary>
        public void SetLevel(string levelId)
        {
            currentLevelId = levelId;
            _currentMessageIndex = 0;
            _shuffleQueue.Clear();
            Debug.Log($"[MorseAudioPlayer] 切换到关卡: {levelId}");
        }
        
        /// <summary>
        /// 设置播放模式
        /// </summary>
        public void SetPlayMode(PlayMode mode)
        {
            playMode = mode;
            if (mode == PlayMode.Shuffle)
            {
                BuildShuffleQueue();
            }
        }
        
        /// <summary>
        /// 播放指定消息
        /// </summary>
        public void PlayMessage(string messageId)
        {
            if (MessageDatabase.Instance == null)
            {
                Debug.LogWarning("[MorseAudioPlayer] MessageDatabase 未初始化");
                return;
            }
            
            var message = MessageDatabase.Instance.GetMessage(currentLevelId, messageId);
            if (message != null)
            {
                PlayMessageData(message);
            }
            else
            {
                Debug.LogWarning($"[MorseAudioPlayer] 消息未找到: {messageId}");
            }
        }
        
        /// <summary>
        /// 播放指定索引的消息
        /// </summary>
        public void PlayMessageByIndex(int index)
        {
            if (MessageDatabase.Instance == null) return;
            
            var message = MessageDatabase.Instance.GetMessageByIndex(currentLevelId, index);
            if (message != null)
            {
                _currentMessageIndex = index;
                PlayMessageData(message);
            }
        }
        
        /// <summary>
        /// 播放下一条消息（根据播放模式）
        /// </summary>
        public void PlayNextMessage()
        {
            if (MessageDatabase.Instance == null) return;
            
            var messages = MessageDatabase.Instance.GetLevelMessages(currentLevelId);
            if (messages.Count == 0) return;
            
            MessageData message = null;
            
            switch (playMode)
            {
                case PlayMode.Sequential:
                    _currentMessageIndex = (_currentMessageIndex + 1) % messages.Count;
                    message = messages[_currentMessageIndex];
                    break;
                    
                case PlayMode.Random:
                    _currentMessageIndex = Random.Range(0, messages.Count);
                    message = messages[_currentMessageIndex];
                    break;
                    
                case PlayMode.Repeat:
                    message = messages[_currentMessageIndex];
                    break;
                    
                case PlayMode.Shuffle:
                    if (_shuffleQueue.Count == 0)
                    {
                        BuildShuffleQueue();
                    }
                    if (_shuffleQueue.Count > 0)
                    {
                        _currentMessageIndex = _shuffleQueue[0];
                        _shuffleQueue.RemoveAt(0);
                        message = messages[_currentMessageIndex];
                    }
                    break;
            }
            
            if (message != null)
            {
                PlayMessageData(message);
            }
        }
        
        /// <summary>
        /// 播放当前消息
        /// </summary>
        public void PlayCurrentMessage()
        {
            PlayMessageByIndex(_currentMessageIndex);
        }
        
        /// <summary>
        /// 播放随机消息
        /// </summary>
        public void PlayRandomMessage()
        {
            if (MessageDatabase.Instance == null) return;
            
            var message = MessageDatabase.Instance.GetRandomMessage(currentLevelId);
            if (message != null)
            {
                PlayMessageData(message);
            }
        }
        
        /// <summary>
        /// 获取当前消息信息
        /// </summary>
        public MessageData GetCurrentMessage() => _currentMessage;
        
        /// <summary>
        /// 获取当前关卡所有消息
        /// </summary>
        public List<MessageData> GetCurrentLevelMessages()
        {
            if (MessageDatabase.Instance == null) return new List<MessageData>();
            return MessageDatabase.Instance.GetLevelMessages(currentLevelId);
        }

        #endregion

        #region Private Methods

        private void PlayMessageData(MessageData message)
        {
            if (message == null) return;
            
            _currentMessage = message;
            Stop();
            StartCoroutine(PlayMessageCoroutine(message));
        }
        
        private IEnumerator PlayMessageCoroutine(MessageData message)
        {
            OnMessageStart?.Invoke(message);
            
            // 延迟
            if (message.delayBefore > 0)
            {
                yield return new WaitForSeconds(message.delayBefore);
            }
            
            // 播放
            if (message.isMorseCode)
            {
                yield return StartCoroutine(PlayCodeSequence(message.text));
            }
            else
            {
                yield return StartCoroutine(PlayTextSequence(message.text));
            }
            
            OnMessageEnd?.Invoke(message);
        }
        
        private void BuildShuffleQueue()
        {
            _shuffleQueue.Clear();
            if (MessageDatabase.Instance == null) return;
            
            var messages = MessageDatabase.Instance.GetLevelMessages(currentLevelId);
            _shuffleQueue = Enumerable.Range(0, messages.Count).OrderBy(x => Random.value).ToList();
        }

        #endregion
        
        private IEnumerator PlayCodeSequence(string code)
        {
            _isPlaying = true;
            OnPlaybackStart?.Invoke();
            
            foreach (char symbol in code)
            {
                if (symbol == '·' || symbol == '.')
                {
                    OnSymbolPlay?.Invoke('·');
                    yield return PlayTone(dotClip, dotDuration);
                    yield return new WaitForSeconds(symbolGap);
                }
                else if (symbol == '-' || symbol == '—')
                {
                    OnSymbolPlay?.Invoke('-');
                    float dashDuration = dotDuration * 3;
                    yield return PlayTone(dashClip, dashDuration);
                    yield return new WaitForSeconds(symbolGap);
                }
                else if (symbol == ' ')
                {
                    yield return new WaitForSeconds(symbolGap * 2);
                }
            }
            
            _isPlaying = false;
            OnPlaybackEnd?.Invoke();
        }
        
        private IEnumerator PlayTextSequence(string text)
        {
            _isPlaying = true;
            OnPlaybackStart?.Invoke();
            
            foreach (char c in text.ToUpper())
            {
                if (c == ' ')
                {
                    yield return new WaitForSeconds(symbolGap * 4);
                }
                else
                {
                    string code = morseData.GetCode(c);
                    if (!string.IsNullOrEmpty(code))
                    {
                        yield return StartCoroutine(PlayCodeSequence(code));
                        yield return new WaitForSeconds(symbolGap * 2);
                    }
                }
            }
            
            _isPlaying = false;
            OnPlaybackEnd?.Invoke();
        }
        
        private IEnumerator PlayTone(AudioClip clip, float duration)
        {
            if (clip == null || _audioSource == null) 
            {
                // 如果没有音频文件，用等待模拟时长
                yield return new WaitForSeconds(duration);
                yield break;
            }
            
            // 使用音频文件播放
            _audioSource.clip = clip;
            _audioSource.Play();
            
            // 等待指定时长（可能短于音频长度，用于控制节奏）
            yield return new WaitForSeconds(duration);
            
            // 停止播放（如果音频比duration长）
            if (_audioSource.isPlaying && _audioSource.clip == clip)
            {
                _audioSource.Stop();
            }
        }
        
        /// <summary>
        /// 是否正在播放
        /// </summary>
        public bool IsPlaying => _isPlaying;
        
        /// <summary>
        /// 停止播放
        /// </summary>
        public void Stop()
        {
            StopAllCoroutines();
            if (_audioSource != null)
            {
                _audioSource.Stop();
            }
            _isPlaying = false;
        }
        
        /// <summary>
        /// 重置播放状态
        /// </summary>
        public void Reset()
        {
            Stop();
            _currentMessageIndex = 0;
            _shuffleQueue.Clear();
            _currentMessage = null;
        }

        #endregion

        #region Test Methods
        
        [ContextMenu("Test Play Dot")]
        private void TestPlayDot()
        {
            if (dotClip != null && _audioSource != null)
            {
                _audioSource.clip = dotClip;
                _audioSource.Play();
                Debug.Log("Playing dot audio");
            }
            else
            {
                Debug.LogWarning("Dot audio clip not assigned!");
            }
        }
        
        [ContextMenu("Test Play Dash")]
        private void TestPlayDash()
        {
            if (dashClip != null && _audioSource != null)
            {
                _audioSource.clip = dashClip;
                _audioSource.Play();
                Debug.Log("Playing dash audio");
            }
            else
            {
                Debug.LogWarning("Dash audio clip not assigned!");
            }
        }
        
        [ContextMenu("Test Play Current Level")]
        private void TestPlayCurrentLevel()
        {
            PlayCurrentMessage();
        }

        #endregion
    }
}
