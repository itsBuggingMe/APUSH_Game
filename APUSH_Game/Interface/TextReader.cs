using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APUSH_Game.Interface
{
    internal class TextReader
    {
        private StringBuilder _buffer = new StringBuilder();
        Keys[] pressedKeys = Array.Empty<Keys>();

        private int maxCharCount;

        public TextReader(int maxCharCount)
        {
            this.maxCharCount = maxCharCount;
        }

        public void Update()
        {
            KeyboardState state = Keyboard.GetState();
            Keys[] currentPressedKeys = state.GetPressedKeys();

            foreach (Keys key in currentPressedKeys)
            {
                if (!pressedKeys.Contains(key))
                {
                    char character = ConvertKey(key, state.IsKeyDown(Keys.LeftShift) || state.IsKeyDown(Keys.RightShift));

                    if (key == Keys.Back && _buffer.Length != 0)
                    {
                        _buffer.Remove(_buffer.Length - 1, 1);
                    }
                    else if (_buffer.Length <= maxCharCount && character != '\0')
                    {
                        _buffer.Append(character);
                    }
                }
            }
            pressedKeys = currentPressedKeys;
        }

        private char ConvertKey(Keys key, bool shift)
        {
            char transformed = key switch
            {
                >= Keys.D0 and <= Keys.D9 => shift ? SymbolsForShiftedNumbers[key - Keys.D0] : (char)(key - Keys.D0 + '0'),
                >= Keys.NumPad0 and <= Keys.NumPad9 => (char)(key - Keys.NumPad0 + '0'),
                >= Keys.A and <= Keys.Z => (char)(key - Keys.A + (shift ? 'A' : 'a')),
                Keys.Space => ' ',
                Keys.OemQuotes => shift ? '"' : '\'',
                Keys.OemQuestion => shift ? '/' : '?',
                Keys.OemPlus => shift ? '+' : '=',
                Keys.OemSemicolon => shift ? ':' : ';',
                Keys.OemTilde => shift ? '`' : '~',
                Keys.OemPipe => shift ? '|' : '\\',
                Keys.OemCloseBrackets => shift ? '}' : ']',
                Keys.OemOpenBrackets => shift ? '{' : '[',
                Keys.OemMinus => shift ? '_' : '-',
                Keys.OemBackslash => shift ? '|' : '\\',
                _ => '\0',
            };

            return transformed;
        }

        public override string ToString()
        {
            return _buffer.ToString();
        }

        private static readonly char[] SymbolsForShiftedNumbers = { ')', '!', '@', '#', '$', '%', '^', '&', '*', '(' };
    }

}
