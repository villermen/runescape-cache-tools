namespace Villermen.RuneScapeCacheTools.Cache
{
    public class Vector3
    {
        // private static int UnknownInteger;

        private static Vector3[] UnknownVector3Array = new Vector3[0];

        public Vector3()
        {
            this.Level = -1;
        }

        public Vector3(int level, int x, int y, int z)
        {
            this.Level = level;
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        private Vector3(Vector3 vector)
        {
            this.Level = vector.Level;
            this.X = vector.X;
            this.Y = vector.Y;
            this.Z = vector.Z;
        }

        private Vector3(int unknownInteger, bool unknownBoolean)
        {
            if (unknownInteger == -1)
            {
                this.Level = -1;
            }
            else
            {
                this.Level = (unknownInteger >> 28) & 3;
                this.X = ((unknownInteger >> 14) & 0x3fff) << 9;
                this.Y = 0;
                this.Z = (unknownInteger & 0x3fff) << 9;

                if (unknownBoolean)
                {
                    this.X += 256;
                    this.Z += 256;
                }
            }
        }

        public int Level { get; }
        public int X { get; }
        public int Y { get; }
        public int Z { get; }
    }
}