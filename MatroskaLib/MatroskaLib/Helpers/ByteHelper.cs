using System;
using System.Collections.Generic;
using System.Linq;

namespace MatroskaLib
{
    public static class ByteHelper
    {
        public static List<byte> ToBytes(ulong value, bool removePaddingZeroes = true)
        {
            List<byte> lsBytes = BitConverter.GetBytes(value).ToList();
            
            if (BitConverter.IsLittleEndian) 
                lsBytes.Reverse();
            if (removePaddingZeroes) 
                RemoveLeftZeroes(lsBytes);
            return lsBytes;
        }

        public static void RemoveLeftZeroes(List<byte> lsValue)
        {
            int endPositionPadding;
            for (endPositionPadding = 0; endPositionPadding < lsValue.Count; endPositionPadding++)
            {
                if (lsValue[endPositionPadding] != 0x0) break;
            }
            lsValue.RemoveRange(0, endPositionPadding);
        }

        public static void ChangeLength(List<byte> lsBytes, int position, ulong elementId, int newAdition)
        {
            List<byte> elementIdBytes = ToBytes(elementId);
            
            List<byte> lsLengthBytes = new();
            for (int i = position-1; i >= 0; i--)
            {
                lsLengthBytes.Add(lsBytes[i]);
                if (lsBytes.GetRange(i - elementIdBytes.Count, elementIdBytes.Count).SequenceEqual(elementIdBytes))
                {
                    break;
                }
            }
            lsLengthBytes.Reverse();

            ChangeLength(lsBytes, position - lsLengthBytes.Count, lsLengthBytes, newAdition);
        }

        public static void ChangeLength(List<byte> lsBytes, int position, List<byte> lsLengthBytes, int newAdition)
        {
            ulong ret = FromBytesToUlong(lsLengthBytes);

            // Apply addition or negative
            if (newAdition > 0)
                ret += Convert.ToUInt32(newAdition);
            else 
                ret -= Convert.ToUInt32(newAdition * -1);

            // Convert new length to bytes and strip bytes
            List<byte> lsNewBytes = ToBytes(ret);
            if (lsNewBytes.Count != lsLengthBytes.Count) throw new Exception("New length doesn't fit into existing length element");
                
            // Replace old length with new length bytes
            lsBytes.RemoveRange(position, lsNewBytes.Count);
            lsBytes.InsertRange(position, lsNewBytes);
            
        }

        public static ulong FromBytesToUlong(List<byte> lsLengthBytes)
        {
            // Convert length byte array to int
            ulong ret = 0;
            for (int i = 0; i < 8 && i < lsLengthBytes.Count; i++)
            {
                ret <<= 8;
                ret |= (ulong) lsLengthBytes[i] & 0xFF;
            }

            return ret;
        }

        public static void ChangeVoidLength(List<byte> lsBytes, int voidPosition)
        {
            // Remove existing length
            lsBytes.RemoveRange(voidPosition + 1, 8);
            uint zeroCount = 0;

            // Count zeroes
            for (var i = voidPosition + 1; i < lsBytes.Count; i++)
            {
                if (lsBytes[i] != 0x0) break;
                zeroCount++;
            }

            List<byte> voidLengthBytes = GetLengthBytes(zeroCount, 8);
            lsBytes.InsertRange(voidPosition + 1, voidLengthBytes);
        }

        public static List<byte> GetLengthBytes(uint value, int maxLength) => 
            ToBytes(value | 1UL << (7 * maxLength));
        
        public static void ReplaceHashWithVoid(List<byte> lsBytes, int checkSumPosition)
        {
            lsBytes.RemoveRange(checkSumPosition, 6);
            lsBytes.InsertRange(checkSumPosition, new byte[] { 0xEC, 0x84, 0x0, 0x0, 0x0, 0x0 });
        }
    }
}