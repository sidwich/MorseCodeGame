using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace MorseCodeGame
{
    /// <summary>
    /// 摩斯电码输入处理器
    /// 处理按钮点击：短按=点(·)，长按=划(-)
    /// </summary>
    public class MorseInputHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("时序配置")]
        [Tooltip("判断为划(-)的时间阈值(秒)，超过则为划")]
        public float dashThreshold = 0.2f;
        
        [Tooltip("字符自动提交的时间阈值(秒)")]
        public float charSubmitThreshold = 0.5f;
        
        [Header("输入模式")]
        public bool useKeyboard = true;
        public KeyCode inputKey = KeyCode.Space;
        
        // 事件
        public event Action OnDotInput;      // 点输入
        public event Action OnDashInput;     // 划输入
        public event Action<string> OnMorseInput; // 摩斯码输入变化(如 "·-")
        public event Action<string> OnCharacterSubmit; // 字符提交(如 "·-" 提交为A)
        
        private float _pressStartTime;
        private bool _isPressed = false;
        private float _lastReleaseTime;
        private string _currentInput = "";
        
        private void Update()
        {
            if (!useKeyboard) return;
            
            // 键盘输入支持
            if (Input.GetKeyDown(inputKey) && !_isPressed)
            {
                OnPointerDown(null);
            }
            if (Input.GetKeyUp(inputKey) && _isPressed)
            {
                OnPointerUp(null);
            }
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (_isPressed) return;
            
            _isPressed = true;
            _pressStartTime = Time.time;
            
            // 检查是否是新字符（间隔足够长）
            CheckForNewCharacter();
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_isPressed) return;
            
            _isPressed = false;
            float pressDuration = Time.time - _pressStartTime;
            _lastReleaseTime = Time.time;
            
            // 判断是点还是划
            if (pressDuration < dashThreshold)
            {
                // 点 (·)
                _currentInput += "·";
                OnDotInput?.Invoke();
                OnMorseInput?.Invoke(_currentInput);
            }
            else
            {
                // 划 (-)
                _currentInput += "-";
                OnDashInput?.Invoke();
                OnMorseInput?.Invoke(_currentInput);
            }
            
            // 重置自动提交计时
            StopAllCoroutines();
            StartCoroutine(AutoSubmitTimer());
        }
        
        private void CheckForNewCharacter()
        {
            if (_lastReleaseTime > 0 && _currentInput.Length > 0)
            {
                float gap = Time.time - _lastReleaseTime;
                if (gap >= charSubmitThreshold)
                {
                    // 间隔足够长，提交当前字符
                    SubmitCurrentCharacter();
                }
            }
        }
        
        private System.Collections.IEnumerator AutoSubmitTimer()
        {
            yield return new WaitForSeconds(charSubmitThreshold);
            
            // 超时自动提交
            if (_currentInput.Length > 0 && !_isPressed)
            {
                SubmitCurrentCharacter();
            }
        }
        
        /// <summary>
        /// 提交当前输入的摩斯码
        /// </summary>
        private void SubmitCurrentCharacter()
        {
            string submitted = _currentInput;
            _currentInput = "";
            OnCharacterSubmit?.Invoke(submitted);
        }
        
        /// <summary>
        /// 手动提交当前输入（如点击确认按钮）
        /// </summary>
        public void ManualSubmit()
        {
            if (_currentInput.Length > 0)
            {
                StopAllCoroutines();
                SubmitCurrentCharacter();
            }
        }
        
        /// <summary>
        /// 清空当前输入
        /// </summary>
        public void ClearInput()
        {
            StopAllCoroutines();
            _currentInput = "";
            OnMorseInput?.Invoke(_currentInput);
        }
        
        /// <summary>
        /// 获取当前输入的摩斯码
        /// </summary>
        public string GetCurrentInput() => _currentInput;
        
        /// <summary>
        /// 是否正在按下
        /// </summary>
        public bool IsPressed => _isPressed;
        
        /// <summary>
        /// 获取当前按住时长（用于UI显示进度条）
        /// </summary>
        public float GetCurrentPressDuration()
        {
            if (!_isPressed) return 0f;
            return Time.time - _pressStartTime;
        }
    }
}
