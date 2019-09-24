using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace c_ip8
{
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
        public HashSet<byte> PressedKeys = new HashSet<byte>();
        public bool[,] Display = new bool[DISPLAY_WIDTH, DISPLAY_HEIGHT];
        bool needsRedraw = false;
        public void LoadProgram(byte[] program)
        {
            RAM = new byte[4096];

            for (int i = 0; i < program.Length; i++)
            {
                RAM[512 + i] = program[i];
            }

            ProgramCounter = 512;
        }

        public void LoadFont() {
            var charactersArray = new byte[] {0xF0,0x90,0x90,0x90,0xF0,0x20,0x60,0x20,0x20,0x70,0xF0,0x10,0xF0,0x80,0xF0,0xF0,0x10,0xF0,0x10,0xF0,0x90,0x90,0xF0,0x10,0x10,0xF0,0x80,0xF0,0x10,0xF0,0xF0,0x80,0xF0,0x90,0xF0,0xF0,0x10,0x20,0x40,0x40,0xF0,0x90,0xF0,0x90,0xF0,0xF0,0x90,0xF0,0x10,0xF0,0xF0,0x90,0xF0,0x90,0x90,0xE0,0x90,0xE0,0x90,0xE0,0xF0,0x80,0x80,0x80,0xF0,0xE0,0x90,0x90,0x90,0xE0,0xF0,0x80,0xF0,0x80,0xF0,0xF0,0x80,0xF0,0x80,0x80};
            Array.Copy(charactersArray, RAM, charactersArray.Length);
        }

        private Random rndGenerator = new Random(Environment.TickCount);

        private Stopwatch sw = new Stopwatch(); 
        public void Step()
        {
            if (!sw.IsRunning) 
            {
                sw.Start();
            }
            if (sw.ElapsedMilliseconds > 16) 
            {
                if (DelayTimer > 0)
                    DelayTimer--;
                if (SoundTimer > 0)
                    SoundTimer--;

                sw.Restart();
            }

            var opCode = (ushort)(RAM[ProgramCounter++] << 8 | RAM[ProgramCounter++]);
            var data = CreateOpCodeData(opCode);

            switch (data.MSB) //top 4 bites
            {
                case 0x0:
                    if (data.NN == 0xE0)
                    {
                        for (int i = 0; i < DISPLAY_WIDTH; i++)
                        {
                            for (int j = 0; j < DISPLAY_HEIGHT; j++)
                            {
                                Display[i, j] = false;
                            }
                        }
                    }
                    else if (data.NN == 0xEE)
                    {
                        ProgramCounter = Stack.Pop();
                    }
                    break;
                case 0x1:
                    ProgramCounter = data.NNN;
                    break;
                case 0x2:
                    Stack.Push(ProgramCounter);
                    ProgramCounter = data.NNN;
                    break;
                case 0x3:
                    if (V[data.X] == data.NN)
                        ProgramCounter += 2;
                    break;
                case 0x4:
                    if (V[data.X] != data.NN)
                        ProgramCounter += 2;
                    break;
                case 0x5:
                    if (V[data.X] == V[data.Y])
                        ProgramCounter += 2;
                    break;
                case 0x6:
                    V[data.X] = data.NN;
                    break;
                case 0x7:
                    V[data.X] += data.NN;
                    break;
                case 0x8:
                    switch (data.N)
                    {
                        case 0x0:
                            V[data.X] = V[data.Y];
                            break;
                        case 0x1:
                            V[data.X] |= V[data.Y];
                            break;
                        case 0x2:
                            V[data.X] &= V[data.Y];
                            break;
                        case 0x3:
                            V[data.X] ^= V[data.Y];
                            break;
                        case 0x4:
                            V[15] = (byte)(V[data.X] + V[data.Y] > 0xFF ? 1 : 0);
                            V[data.X] += V[data.Y];
                            break;
                        case 0x5:
                            V[15] = (byte)(V[data.X] > V[data.Y] ? 1 : 0);
                            V[data.X] -= V[data.Y];
                            break;
                        case 0x6:
                            V[15] = (byte)((V[data.X] & 0x1) == 1 ? 1 : 0);
                            V[data.X] = (byte)(V[data.X] >> 1);
                            break;
                        case 0x7:
                            V[15] = (byte)(V[data.Y] > V[data.X] ? 1 : 0);
                            V[data.Y] -= V[data.X];
                            break;
                        case 0xE:
                            V[15] = (byte)((V[data.X] & 0xF) == 1 ? 1 : 0);
                            V[data.X] = (byte)(V[data.X] << 1);
                            break;
                        default:
                            Console.WriteLine($"unknown opcode {opCode.ToString("X4")}");
                            break;
                    }
                    break;
                case 0x9:
                    if (V[data.X] != V[data.Y])
                        ProgramCounter += 2;
                    break;
                case 0xA:
                    I = data.NNN;
                    break;
                case 0xB:
                    ProgramCounter = (ushort)(V[0] + data.NNN);
                    break;
                case 0xC:
                    var rnd = rndGenerator.Next(0, 256);
                    V[data.X] = (byte)(rnd & data.NN);
                    break;
                case 0xD: //TODO: this is probably wrong
                    var startX = V[data.X];
                    var startY = V[data.Y];

                    V[0xF] = 0;
                    for (var i = 0; i < data.N; i++)
                    {
                        var spriteLine = RAM[I + i]; // A line of the sprite to render

                        for (var bit = 0; bit < 8; bit++)
                        {
                            var x = (startX + bit) % DISPLAY_WIDTH;
                            var y = (startY + i) % DISPLAY_HEIGHT;

                            var spriteBit = ((spriteLine >> (7 - bit)) & 1);
                            var oldBit = Display[x, y] ? 1 : 0;

                            if (oldBit != spriteBit)
                                needsRedraw = true;

                            // New bit is XOR of existing and new.
                            var newBit = oldBit ^ spriteBit;
                            Display[x, y] = newBit != 0;

                            // If we wiped out a pixel, set flag for collission.
                            if (oldBit != 0 && newBit == 0)
                                V[0xF] = 1;
                        }
                    }
                    break;
                case 0xE:
                    if ((data.NN == 0x9E && PressedKeys.Contains(V[data.X]))
                        || (data.NN == 0xA1 && !PressedKeys.Contains(V[data.X])))
                    {
                        ProgramCounter += 2;
                    }
                    break;
                case 0xF:
                    switch (data.NN)
                    {
                        case 0x07:
                            V[data.X] = DelayTimer;
                            break;
                        case 0x0A: //waits for key by looping current instruction
                            if (PressedKeys.Count == 0)
                                ProgramCounter -= 2;

                            V[data.X] = PressedKeys.First();
                            break;
                        case 0x15:
                            DelayTimer = V[data.X];
                            break;
                        case 0x18:
                            SoundTimer = V[data.X];
                            break;
                        case 0x1E:
                            I += V[data.X];
                            break;
                        case 0x29:
                            // throw new Exception("not not handling system font");
                            break;
                        case 0x33:
                            RAM[I] = (byte)((V[data.X] / 100) % 10);
                            RAM[I + 1] = (byte)((V[data.X] / 10) / 10);
                            RAM[I + 2] = (byte)(V[data.X] % 10);
                            break;
                        case 0x55:
                            for (int i = 0; i <= data.X; i++)
                            {
                                RAM[I + i] = V[i];
                            }
                            break;
                        case 0x65:
                            for (int i = 0; i <= data.X; i++)
                            {
                                V[i] = RAM[I + 1];
                            }
                            break;

                        default:
                            throw new Exception($"opcode not supported - {opCode}");
                    }

                    break;
                default:
                    throw new Exception($"opcode not supported - {opCode}");
            }

            // ProgramCounter += 2;
        }

        public void DrawDisplay()
        {
            if (!needsRedraw)
                return;
            needsRedraw = false;

            Console.Clear();
            Console.SetCursorPosition(0, 0);
            StringBuilder sb;
            for (int y = 0; y < DISPLAY_HEIGHT; y++)
            {
                sb = new StringBuilder();
                for (int x = 0; x < DISPLAY_WIDTH; x++)
                {
                    if (Display[x, y])
                    {
                        sb.Append("*");
                    }
                    else
                    {
                        sb.Append(" ");
                    }
                }

                Console.WriteLine(sb.ToString());
            }
            Thread.Sleep(50);
        }

        private static OpCodeData CreateOpCodeData(ushort opCode)
        {
            return new OpCodeData()
            {
                OriginalOpCode = opCode,
                MSB = (byte)((opCode & 0xF000) >> 12),
                NNN = (ushort)(opCode & 0x0FFF),
                NN = (byte)(opCode & 0x00FF),
                N = (byte)(opCode & 0x000F),
                X = (byte)((opCode & 0x0F00) >> 8),
                Y = (byte)((opCode & 0x00F0) >> 4),
            };
        }
    }
}