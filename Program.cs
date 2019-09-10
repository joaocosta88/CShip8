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
}
