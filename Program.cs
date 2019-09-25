using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace c_ip8
{
    class Program
    {
        static void Main(string[] args)
        {
            var memory = new Memory();

            var sound = new Sound();
            var display = new Display(memory);
            var cpu = new CPU(memory);

            using (var reader = new BinaryReader(new FileStream("roms/games/PONG", FileMode.Open)))
            {
                var program = new List<byte>();
                while (reader.BaseStream.Position < reader.BaseStream.Length - 1)
                {
                    program.Add(reader.ReadByte());
                }

                memory.LoadProgram(program.ToArray());
            }

            Stopwatch sw = new Stopwatch();
            while (true)
            {
                sw.Restart();

                if (memory.DelayTimer > 0)
                    memory.DelayTimer--;
                if (memory.SoundTimer > 0) {
                    sound.Beep();
                    memory.SoundTimer--;
                }

                cpu.Step();
                Console.Write(display.Draw());

                while (sw.ElapsedMilliseconds < 10) { }
            }
        }
    }
}
