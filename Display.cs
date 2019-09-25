using System;
using System.Text;

namespace c_ip8
{
    public class Display
    {
        private Memory Memory;
        public const int DISPLAY_WIDTH = 64;
        public const int DISPLAY_HEIGHT = 32;

        public Display(Memory memory)
        {
            Memory = memory;
            LoadFont();
        }
        private void LoadFont()
        {
            var charactersArray = new byte[] { 0xF0, 0x90, 0x90, 0x90, 0xF0, 0x20, 0x60, 0x20, 0x20, 0x70, 0xF0, 0x10, 0xF0, 0x80, 0xF0, 0xF0, 0x10, 0xF0, 0x10, 0xF0, 0x90, 0x90, 0xF0, 0x10, 0x10, 0xF0, 0x80, 0xF0, 0x10, 0xF0, 0xF0, 0x80, 0xF0, 0x90, 0xF0, 0xF0, 0x10, 0x20, 0x40, 0x40, 0xF0, 0x90, 0xF0, 0x90, 0xF0, 0xF0, 0x90, 0xF0, 0x10, 0xF0, 0xF0, 0x90, 0xF0, 0x90, 0x90, 0xE0, 0x90, 0xE0, 0x90, 0xE0, 0xF0, 0x80, 0x80, 0x80, 0xF0, 0xE0, 0x90, 0x90, 0x90, 0xE0, 0xF0, 0x80, 0xF0, 0x80, 0xF0, 0xF0, 0x80, 0xF0, 0x80, 0x80 };
            Array.Copy(charactersArray, Memory.RAM, charactersArray.Length);
        }

        public string Draw()
        {
            if (!Memory.IsVRAMDirty)
            {
                return string.Empty;
            }

            Memory.IsVRAMDirty = false;
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            
            StringBuilder sb = new StringBuilder();
            for (int y = 0; y < DISPLAY_HEIGHT; y++)
            {
                for (int x = 0; x < DISPLAY_WIDTH; x++)
                {
                    if (Memory.VRAM[x, y])
                    {
                        sb.Append("*");
                    }
                    else
                    {
                        sb.Append(" ");
                    }
                }
                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }
    }
}