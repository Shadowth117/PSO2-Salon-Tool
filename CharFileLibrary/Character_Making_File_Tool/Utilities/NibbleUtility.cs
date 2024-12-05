using System;

namespace Character_Making_File_Tool
{
    public class NibbleUtility
    {
        public static byte SignednibblePack(sbyte left, sbyte right)
        {
            return (byte)(SetupXXPnibble(left) * 0x10 + SetupXXPnibble(right));
        }

        public static void SignednibbleUnpack(byte signednibbles, out sbyte left, out sbyte right)
        {
            int tempLeft = signednibbles / 0x10;
            int tempRight = signednibbles % 0x10;

            left = Convert.ToSByte(SetupIntFromXXPnibble(tempLeft));
            right = Convert.ToSByte(SetupIntFromXXPnibble(tempRight));
        }

        //XXP V5 and V6 store accessory sliders in nibbles. 1-7 is postive while 8-E is negative, but 8-E's magnitude goes up going from 8, -1, to E, -7
        //Therefore, we must convert from a normal signed value to suit this format.
        public static int SetupXXPnibble(int nyb)
        {
            if (nyb < 0)
            {
                nyb = Math.Max(nyb, -126);
                nyb = Math.Abs(nyb) + 126;
                //Correct potential underflow to max positive on division and round appropriately
                if (nyb < 135)
                {
                    nyb = 0;
                }
                else if (nyb < 144)
                {
                    nyb = 144;
                }
            }
            nyb /= 18;

            return nyb;
        }

        public static int SetupIntFromXXPnibble(int nyb)
        {
            if (nyb > 7)
            {
                nyb = (nyb - 7) * -1;
            }
            nyb *= 18;

            return nyb;
        }
    }
}
