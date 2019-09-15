using System;
using System.Collections.Generic;
using System.IO;

namespace c_ip8
{
    class Program
    {
        static void Main(string[] args)
        {
            var cpu = new CPU();
            using (var reader = new BinaryReader(new FileStream("roms/heart_monitor.ch8", FileMode.Open)))
            {
                var program = new List<byte>();
                while (reader.BaseStream.Position < reader.BaseStream.Length - 1)
                {
                    //program.Add((ushort)(reader.ReadByte() << 8 | reader.ReadByte()));
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
