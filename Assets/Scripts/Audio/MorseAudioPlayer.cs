using UnityEngine;
using System.Collections;

namespace MorseCodeGame
{
    /// <summary>
    /// 摩斯电码音频播放器
    /// 使用预制音频文件播放摩斯电码
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
        
        private AudioSource _audioSource;
        private bool _isPlaying = false;
        
        // 事件
        public System.Action OnPlaybackStart;
        public System.Action OnPlaybackEnd;
        public System.Action<char> OnSymbolPlay; // 正在播放的符号(·或-)
        
        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // 配置 AudioSource
            _audioSource.playOnAwake = false;
            _audioSource.volume = volume;
            _audioSource.spatialBlend = 0f; // 2D音效
        }
        
        private void OnValidate()
        {
            if (_audioSource != null)
            {
                _audioSource.volume = volume;
            }
        }
        
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
        /// 播放摩斯码字符串(如 "·-")
        /// </summary>
        public void PlayMorseCode(string morseCode)
        {
            if (!string.IsNullOrEmpty(morseCode))
            {
                StartCoroutine(PlayCodeSequence(morseCode));
            }
        }
        
        /// <summary>
        /// 播放文本（自动转换为摩斯码）
        /// </summary>
        public void PlayText(string text)
        {
            if (string.IsNullOrEmpty(text) || morseData == null) return;
            StartCoroutine(PlayTextSequence(text));
        }
        
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
        /// 测试播放点音频
        /// </summary>
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
        
        /// <summary>
        /// 测试播放划音频
        /// </summary>
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
    }
}
