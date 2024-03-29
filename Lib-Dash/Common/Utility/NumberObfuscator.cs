﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Utility
{
    // Skip32 알고리즘을 사용해서 oidAccount를 변환한 후, Base36 encoding을 이용하여 유저에게 보여준다.
    // 최대 13글자까지 될 수 있으며, Base62를 사용할 경우 11글자까지 줄일 수 있지만 대소문자가 섞여버려서 가독성이 많이 떨어진다.
    public class NumberObfuscator // using Skip32 algorithm
    {
        private static string _charList = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static byte[] _key = new byte[16] { 68, 12, 237, 122, 2, 0, 18, 89, 44, 4, 14, 114, 141, 144, 214, 241 };

        private static byte[] FTABLE = { 0xa3, 0xd7, 0x09, 0x83, 0xf8, 0x48,
            0xf6, 0xf4, 0xb3, 0x21, 0x15, 0x78, 0x99, 0xb1, 0xaf, 0xf9, 0xe7,
            0x2d, 0x4d, 0x8a, 0xce, 0x4c, 0xca, 0x2e, 0x52, 0x95, 0xd9, 0x1e,
            0x4e, 0x38, 0x44, 0x28, 0x0a, 0xdf, 0x02, 0xa0, 0x17, 0xf1, 0x60,
            0x68, 0x12, 0xb7, 0x7a, 0xc3, 0xe9, 0xfa, 0x3d, 0x53, 0x96, 0x84,
            0x6b, 0xba, 0xf2, 0x63, 0x9a, 0x19, 0x7c, 0xae, 0xe5, 0xf5, 0xf7,
            0x16, 0x6a, 0xa2, 0x39, 0xb6, 0x7b, 0x0f, 0xc1, 0x93, 0x81, 0x1b,
            0xee, 0xb4, 0x1a, 0xea, 0xd0, 0x91, 0x2f, 0xb8, 0x55, 0xb9, 0xda,
            0x85, 0x3f, 0x41, 0xbf, 0xe0, 0x5a, 0x58, 0x80, 0x5f, 0x66, 0x0b,
            0xd8, 0x90, 0x35, 0xd5, 0xc0, 0xa7, 0x33, 0x06, 0x65, 0x69, 0x45,
            0x00, 0x94, 0x56, 0x6d, 0x98, 0x9b, 0x76, 0x97, 0xfc, 0xb2, 0xc2,
            0xb0, 0xfe, 0xdb, 0x20, 0xe1, 0xeb, 0xd6, 0xe4, 0xdd, 0x47, 0x4a,
            0x1d, 0x42, 0xed, 0x9e, 0x6e, 0x49, 0x3c, 0xcd, 0x43, 0x27, 0xd2,
            0x07, 0xd4, 0xde, 0xc7, 0x67, 0x18, 0x89, 0xcb, 0x30, 0x1f, 0x8d,
            0xc6, 0x8f, 0xaa, 0xc8, 0x74, 0xdc, 0xc9, 0x5d, 0x5c, 0x31, 0xa4,
            0x70, 0x88, 0x61, 0x2c, 0x9f, 0x0d, 0x2b, 0x87, 0x50, 0x82, 0x54,
            0x64, 0x26, 0x7d, 0x03, 0x40, 0x34, 0x4b, 0x1c, 0x73, 0xd1, 0xc4,
            0xfd, 0x3b, 0xcc, 0xfb, 0x7f, 0xab, 0xe6, 0x3e, 0x5b, 0xa5, 0xad,
            0x04, 0x23, 0x9c, 0x14, 0x51, 0x22, 0xf0, 0x29, 0x79, 0x71, 0x7e,
            0xff, 0x8c, 0x0e, 0xe2, 0x0c, 0xef, 0xbc, 0x72, 0x75, 0x6f, 0x37,
            0xa1, 0xec, 0xd3, 0x8e, 0x62, 0x8b, 0x86, 0x10, 0xe8, 0x08, 0x77,
            0x11, 0xbe, 0x92, 0x4f, 0x24, 0xc5, 0x32, 0x36, 0x9d, 0xcf, 0xf3,
            0xa6, 0xbb, 0xac, 0x5e, 0x6c, 0xa9, 0x13, 0x57, 0x25, 0xb5, 0xe3,
            0xbd, 0xa8, 0x3a, 0x01, 0x05, 0x59, 0x2a, 0x46 };

        private static int G(byte[] key, int k, int w)
        {
            int g1, g2, g3, g4, g5, g6;

            g1 = w >> 8;
            g2 = w & 0xff;

            g3 = FTABLE[g2 ^ (key[(4 * k) % 10] & 0xFF)] ^ g1;
            g4 = FTABLE[g3 ^ (key[(4 * k + 1) % 10] & 0xFF)] ^ g2;
            g5 = FTABLE[g4 ^ (key[(4 * k + 2) % 10] & 0xFF)] ^ g3;
            g6 = FTABLE[g5 ^ (key[(4 * k + 3) % 10] & 0xFF)] ^ g4;

            return (g5 << 8) + g6;
        }

        private static void Skip32(byte[] key, int[] buf, bool encrypt)
        {
            int k; /* round number */
            int i; /* round counter */
            int kstep;
            int wl, wr;

            /* sort out direction */
            if (encrypt)
            {
                kstep = 1;
                k = 0;
            }
            else
            {
                kstep = -1;
                k = 23;
            }

            /* pack into words */
            wl = (buf[0] << 8) + buf[1];
            wr = (buf[2] << 8) + buf[3];

            /* 24 feistel rounds, doubled up */
            for (i = 0; i < 24 / 2; ++i)
            {
                wr ^= G(key, k, wl) ^ k;
                k += kstep;
                wl ^= G(key, k, wr) ^ k;
                k += kstep;
            }

            /* implicitly swap halves while unpacking */
            buf[0] = (wr >> 8);
            buf[1] = (wr & 0xFF);
            buf[2] = (wl >> 8);
            buf[3] = (wl & 0xFF);
        }

        private static int Encrypt(int value, byte[] key)
        {
            int[] buf = new int[4];
            buf[0] = ((value >> 24) & 0xff);
            buf[1] = ((value >> 16) & 0xff);
            buf[2] = ((value >> 8) & 0xff);
            buf[3] = ((value >> 0) & 0xff);

            Skip32(key, buf, true);

            int output = ((buf[0]) << 24) | ((buf[1]) << 16) | ((buf[2]) << 8) | (buf[3]);

            return output;
        }

        private static int Decrypt(int value, byte[] key)
        {
            int[] buf = new int[4];

            buf[0] = ((value >> 24) & 0xff);
            buf[1] = ((value >> 16) & 0xff);
            buf[2] = ((value >> 8) & 0xff);
            buf[3] = ((value >> 0) & 0xff);

            Skip32(key, buf, false);

            int output = ((buf[0]) << 24) | ((buf[1]) << 16) | ((buf[2]) << 8) | (buf[3]);

            return output;
        }

        public static string BaseConvert(string number, int fromBase, int toBase)
        {
            int length = number.Length;

            string result = string.Empty;
            List<int> nibbles = number.Select(c => _charList.IndexOf(c)).ToList();
            int newlen;
            do
            {
                int value = 0;
                newlen = 0;
                for (var i = 0; i < length; ++i)
                {
                    value = value * fromBase + nibbles[i];
                    if (value >= toBase)
                    {
                        if (newlen == nibbles.Count)
                        {
                            nibbles.Add(0);
                        }
                        nibbles[newlen++] = value / toBase;
                        value %= toBase;
                    }
                    else if (newlen > 0)
                    {
                        if (newlen == nibbles.Count)
                        {
                            nibbles.Add(0);
                        }
                        nibbles[newlen++] = 0;
                    }
                }
                length = newlen;
                result = _charList[value] + result;
            }
            while (newlen != 0);
            return result;
        }

        public static string Obfuscate(ulong value)
        {
            int low = (int)(value & uint.MaxValue);
            int high = (int)value >> 32;

            low = Encrypt(low, _key);
            high = Encrypt(high, _key);

            return BaseConvert(((ulong)((high << 32) | low)).ToString(), 10, 36);
        }
        public static string ObfuscateInt(uint value)
        {
            int low = (int)(value & uint.MaxValue);
            int high = (int)value >> 32;

            low = Encrypt(low, _key);
            high = Encrypt(high, _key);

            return BaseConvert(((uint)((high << 32) | low)).ToString(), 10, 36);
        }
        public static ulong Deobfuscate(string obfuscatedValue)
        {
            ulong value = ulong.Parse(BaseConvert(obfuscatedValue, 36, 10));

            int low = (int)(value & uint.MaxValue);
            int high = (int)value >> 32;

            low = Decrypt(low, _key);
            high = Decrypt(high, _key);

            return (ulong)((high << 32) | low);
        }
        public static uint DeobfuscateInt(string obfuscatedValue)
        {
            uint value = uint.Parse(BaseConvert(obfuscatedValue, 36, 10));

            int low = (int)(value & uint.MaxValue);
            int high = (int)value >> 32;

            low = Decrypt(low, _key);
            high = Decrypt(high, _key);

            return (uint)((high << 32) | low);
        }
    }
}
