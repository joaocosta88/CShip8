using System;
using System.Collections.Generic;
using System.IO;

namespace c_ip8
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var reader = new BinaryReader(new FileStream("roms/IBM Logo.ch8", FileMode.Open)))
            {
                var cpu = new CPU();
                while (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    var opCode = (ushort)(reader.ReadByte() << 8 | reader.ReadByte());
                    cpu.ExecuteOpCode(opCode);
                }
            }
        }
    }

    public class CPU
    {
        private const int DISPLAY_WIDTH = 64;
        private const int DISPLAY_HEIGHT = 32;

        public byte[] RAM = new byte[4096];
        public byte[] V = new byte[16]; //registers
        public ushort ProgramCounter;
        public ushort I;
        public Stack<ushort> Stack = new Stack<ushort>();
        public byte DelayTimer;
        public byte SoundTimer;
        public byte Input;
        public byte[] Display = new byte[DISPLAY_WIDTH * DISPLAY_HEIGHT];

private Random rndGenerator = new Random(Environment.TickCount);
        public void ExecuteOpCode(ushort opCode)
        {
            switch (opCode & 0xF000) //top 4 bites
            {
                case 0x0000:
                    if (opCode == 0x00E0)
                    { //clear screen
                        for (int i = 0; i < Display.Length; i++)
                        {
                            Display[i] = 0;
                        }
                    }
                    else if (opCode == 0x00EE)
                    {
                        ProgramCounter = Stack.Pop();
                    }
                    break;
                case 0x1000:
                    ProgramCounter = (ushort)(opCode & 0x0FFF);
                    break;
                case 0x2000:
                    Stack.Push(ProgramCounter);
                    ProgramCounter = (ushort)(opCode & 0x0FFF);
                    break;
                case 0x3000:
                    if (V[opCode & 0x0F00 >> 8] == (opCode & 0x00FF))
                        ProgramCounter += 2;
                    break;
                case 0x4000:
                    if (V[opCode & 0x0F00 >> 8] != (opCode & 0x00FF))
                        ProgramCounter += 2;
                    break;
                case 0x5000:
                    if (V[opCode & 0x0F00 >> 8] == V[opCode & 0x00F0 >> 4])
                        ProgramCounter += 2;
                    break;
                case 0x6000:
                    V[opCode & 0x0F00 >> 8] = (byte)(opCode & 0x00FF);
                    break;
                case 0x7000:
                    V[opCode & 0x0F00 >> 8] += (byte)(opCode & 0x00FF);
                    break;
                case 0x8000:
                    var Vx = (byte)(opCode & 0x0F00 >> 8);
                    var Vy = (byte)(opCode & 0x00F0 >> 4);
                    switch (opCode & 0x000F) //lowest 4 bites
                    {
                        case 0x0000:
                            V[Vx] = V[Vy];
                            break;
                        case 0x0001:
                            V[Vx] = (byte)(V[Vx] & V[Vy]);
                            break;
                        case 0x0002:
                            V[Vx] = (byte)(V[Vx] | V[Vy]);
                            break;
                        case 0x0003:
                            V[Vx] = (byte)(V[Vx] ^ V[Vy]);
                            break;
                        case 0x0004:
                            var sum = (byte)(V[Vx] + V[Vy]);
                            V[Vx] = (byte)(sum & 0x00FF);
                            V[15] = (byte)(sum > 255 ? 1 : 0); //carry flag
                            break;
                        case 0x0005:
                            V[15] = (byte)(V[Vx] > V[Vy] ? 1 : 0);
                            V[Vx] = (byte)((V[Vx] - V[Vy]) & 0x00FF);
                            break;
                        case 0x0006:
                            V[15] = (byte)((V[Vx] & 0x0001) == 1 ? 1 : 0);
                            V[Vx] = (byte)(V[Vx] >> 1);
                            break;
                        case 0x0007:
                            V[15] = (byte)(V[Vy] > V[Vx] ? 1 : 0);
                            V[Vx] = (byte)((V[Vy] - V[Vx]) & 0x00FF);
                            break;
                        case 0x000E:
                            V[15] = (byte)((V[Vx] & 0x1000) == 1 ? 1 : 0);
                            V[Vx] = (byte)(V[Vx] << 1);
                            break;
                        default:
                            Console.WriteLine($"unknown opcode {opCode.ToString("X4")}");
                            break;
                    }
                    break;
                case 0x9000:
                    if (V[opCode & 0x0F00 >> 8] != V[opCode & 0x00F0 >> 4])
                        ProgramCounter += 2;
                    break;
                case 0xA000:
                    I = (ushort)(opCode & 0x0FFF);
                    break;
                case 0xB000:
                    ProgramCounter = (ushort)(V[0] + (opCode & 0x0FFF));
                    break;
                case 0xC000:
                    var rnd = rndGenerator.Next(256);
                    V[opCode & 0x0F00] = (byte) (rnd & (opCode & 0x00FF));
                    break;
                default:
                    Console.WriteLine($"unknown opcode {opCode.ToString("X4")}");
                    break;
            }
        }
    }
}
