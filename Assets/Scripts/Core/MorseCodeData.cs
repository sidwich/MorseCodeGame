using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MorseCodeGame
{
    /// <summary>
    /// 摩斯电码定义
    /// </summary>    
    [CreateAssetMenu(fileName = "MorseCodeData", menuName = "Morse/Morse Code Data")]
    public class MorseCodeData : ScriptableObject
    {
        [System.Serializable]
        public class MorseSymbol
        {
            public char character;
            public string code; // 如 "·-" 表示A
        }
        
        public List<MorseSymbol> symbols = new List<MorseSymbol>
        {
            new MorseSymbol { character = 'A', code = "·-" },
            new MorseSymbol { character = 'B', code = "-···" },
            new MorseSymbol { character = 'C', code = "-·-·" },
            new MorseSymbol { character = 'D', code = "-··" },
            new MorseSymbol { character = 'E', code = "·" },
            new MorseSymbol { character = 'F', code = "··-·" },
            new MorseSymbol { character = 'G', code = "--·" },
            new MorseSymbol { character = 'H', code = "····" },
            new MorseSymbol { character = 'I', code = "··" },
            new MorseSymbol { character = 'J', code = "·---" },
            new MorseSymbol { character = 'K', code = "-·-" },
            new MorseSymbol { character = 'L', code = "·-··" },
            new MorseSymbol { character = 'M', code = "--" },
            new MorseSymbol { character = 'N', code = "-·" },
            new MorseSymbol { character = 'O', code = "---" },
            new MorseSymbol { character = 'P', code = "·--·" },
            new MorseSymbol { character = 'Q', code = "--·-" },
            new MorseSymbol { character = 'R', code = "·-·" },
            new MorseSymbol { character = 'S', code = "···" },
            new MorseSymbol { character = 'T', code = "-" },
            new MorseSymbol { character = 'U', code = "··-" },
            new MorseSymbol { character = 'V', code = "···-" },
            new MorseSymbol { character = 'W', code = "·--" },
            new MorseSymbol { character = 'X', code = "-··-" },
            new MorseSymbol { character = 'Y', code = "-·--" },
            new MorseSymbol { character = 'Z', code = "--··" },
            new MorseSymbol { character = '0', code = "-----" },
            new MorseSymbol { character = '1', code = "·----" },
            new MorseSymbol { character = '2', code = "··---" },
            new MorseSymbol { character = '3', code = "···--" },
            new MorseSymbol { character = '4', code = "····-" },
            new MorseSymbol { character = '5', code = "·····" },
            new MorseSymbol { character = '6', code = "-····" },
            new MorseSymbol { character = '7', code = "--···" },
            new MorseSymbol { character = '8', code = "---··" },
            new MorseSymbol { character = '9', code = "----·" }
        };
        
        /// <summary>
        /// 获取字符对应的摩斯码
        /// </summary>
        public string GetCode(char character)
        {
            var symbol = symbols.Find(s => char.ToUpper(s.character) == char.ToUpper(character));
            return symbol?.code ?? "";
        }
        
        /// <summary>
        /// 根据摩斯码获取字符
        /// </summary>
        public char? GetCharacter(string code)
        {
            var symbol = symbols.Find(s => s.code == code);
            return symbol?.character;
        }
    }
}
