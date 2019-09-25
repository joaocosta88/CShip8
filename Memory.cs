namespace c_ip8
{
    public class Memory
    {
        public byte DelayTimer;
        public byte SoundTimer;

        public byte[] RAM;
        public bool[,] VRAM;
        public bool IsVRAMDirty;

        public Memory()
        {
            RAM = new byte[4096];
            DelayTimer = default;
            SoundTimer = default;
            
            ClearVRAM();
        }

        public void LoadProgram(byte[] program)
        {
            RAM = new byte[4096];

            for (int i = 0; i < program.Length; i++)
            {
                RAM[512 + i] = program[i];
            }
        }

        public void ClearVRAM()
        {
            VRAM = new bool[Display.DISPLAY_WIDTH, Display.DISPLAY_HEIGHT];
        }
    }
}