using UnityEngine;
using System.Collections;

namespace MorseCodeGame
{
    /// <summary>
    /// 摩斯电码音频播放器
    /// 播放预设的摩斯电码音频序列
    /// </summary>
    public class MorseAudioPlayer : MonoBehaviour
    {
        [Header("音频设置")]
        [Tooltip("摩斯电码频率(Hz)，标准800Hz")]
        [Range(400, 1000)]
        public float frequency = 800f;
        
        [Tooltip("点(·)的持续时间(秒)")]
        public float dotDuration = 0.1f;
        
        [Tooltip("音频音量")]
        [Range(0f, 1f)]
        public float volume = 0.8f;
        
        [Header("参考数据")]
        public MorseCodeData morseData;
        
        private AudioSource _audioSource;
        private AudioClip _dotClip;
        private AudioClip _dashClip;
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
            
            // 预生成音频片段
            GenerateClips();
        }
        
        private void OnValidate()
        {
            if (_audioSource != null)
            {
                _audioSource.volume = volume;
            }
        }
        
        /// <summary>
        /// 预生成点和划的音频片段
        /// </summary>
        private void GenerateClips()
        {
            _dotClip = CreateToneClip(dotDuration);
            _dashClip = CreateToneClip(dotDuration * 3);
        }
        
        /// <summary>
        /// 创建正弦波音效片段
        /// </summary>
        private AudioClip CreateToneClip(float duration)
        {
            int sampleRate = 44100;
            int sampleLength = (int)(sampleRate * duration);
            float[] samples = new float[sampleLength];
            
            // 生成正弦波，添加淡入淡出避免爆音
            for (int i = 0; i < sampleLength; i++)
            {
                float t = (float)i / sampleRate;
                float sine = Mathf.Sin(2 * Mathf.PI * frequency * t);
                
                // 淡入淡出窗口（各5%）
                float fadeIn = Mathf.Clamp01(i / (sampleLength * 0.05f));
                float fadeOut = Mathf.Clamp01((sampleLength - i) / (sampleLength * 0.05f));
                
                samples[i] = sine * fadeIn * fadeOut * 0.9f; // 0.9f 防止削波
            }
            
            AudioClip clip = AudioClip.Create($"MorseTone_{duration:F3}s", sampleLength, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
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
                    yield return PlayTone(_dotClip);
                    yield return new WaitForSeconds(dotDuration); // 符号间间隔
                }
                else if (symbol == '-' || symbol == '—')
                {
                    OnSymbolPlay?.Invoke('-');
                    yield return PlayTone(_dashClip);
                    yield return new WaitForSeconds(dotDuration); // 符号间间隔
                }
                else if (symbol == ' ')
                {
                    yield return new WaitForSeconds(dotDuration * 2); // 字符间额外间隔(总共3t)
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
                    yield return new WaitForSeconds(dotDuration * 4); // 单词间隔(7t，已含字符间隔3t)
                }
                else
                {
                    string code = morseData.GetCode(c);
                    if (!string.IsNullOrEmpty(code))
                    {
                        yield return StartCoroutine(PlayCodeSequence(code));
                        yield return new WaitForSeconds(dotDuration * 2); // 字符间间隔
                    }
                }
            }
            
            _isPlaying = false;
            OnPlaybackEnd?.Invoke();
        }
        
        private IEnumerator PlayTone(AudioClip clip)
        {
            if (clip == null || _audioSource == null) yield break;
            
            _audioSource.clip = clip;
            _audioSource.Play();
            
            // 等待音频播放完成
            yield return new WaitForSeconds(clip.length);
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
        /// 测试播放一个点（调试用）
        /// </summary>
        [ContextMenu("Test Play Dot")]
        private void TestPlayDot()
        {
            if (_dotClip != null)
            {
                _audioSource.clip = _dotClip;
                _audioSource.Play();
                Debug.Log("Playing dot sound");
            }
            else
            {
                Debug.LogError("Dot clip not generated!");
            }
        }
    }
}
