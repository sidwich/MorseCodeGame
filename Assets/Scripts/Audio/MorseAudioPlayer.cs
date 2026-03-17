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
        public float volume = 0.5f;
        
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
            _audioSource.volume = volume;
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
                    yield return PlayTone(dotDuration);
                    yield return new WaitForSeconds(dotDuration); // 符号间间隔
                }
                else if (symbol == '-' || symbol == '—')
                {
                    OnSymbolPlay?.Invoke('-');
                    yield return PlayTone(dotDuration * 3); // 划是点的3倍
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
        
        private IEnumerator PlayTone(float duration)
        {
            // 生成正弦波音效
            int sampleRate = 44100;
            int sampleLength = (int)(sampleRate * duration);
            float[] samples = new float[sampleLength];
            
            for (int i = 0; i < sampleLength; i++)
            {
                samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * i / sampleRate);
            }
            
            AudioClip toneClip = AudioClip.Create("MorseTone", sampleLength, 1, sampleRate, false);
            toneClip.SetData(samples, 0);
            
            _audioSource.PlayOneShot(toneClip, volume);
            yield return new WaitForSeconds(duration);
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
            _audioSource.Stop();
            _isPlaying = false;
        }
    }
}
