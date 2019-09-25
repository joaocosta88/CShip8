using System;
using System.Collections.Generic;
using System.IO;
using SDL2;

namespace c_ip8
{
    class Program
    {
        static void Main(string[] args)
        {
               var cpu = new CPU();

            cpu.LoadFont();
            using (var reader = new BinaryReader(new FileStream("roms/games/PONG", FileMode.Open)))
            {
                var program = new List<byte>();
                while (reader.BaseStream.Position < reader.BaseStream.Length - 1)
                {
                    program.Add(reader.ReadByte());
                }

                cpu.LoadProgram(program.ToArray());
            }

            while (true)
            {
                cpu.Step();
                cpu.DrawDisplay();
            }
        }
    }
}
