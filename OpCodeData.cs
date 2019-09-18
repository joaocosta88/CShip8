namespace c_ip8
{
    struct OpCodeData
    {
        public ushort OriginalOpCode;
        public ushort NNN;
        public byte MSB, NN, N, X, Y;
    }
}