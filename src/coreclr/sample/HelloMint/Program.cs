﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Reflection;
using System.Reflection.Emit;

namespace HelloMint
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (AppContext.TryGetSwitch("System.Private.Mint.Enable", out var enabled))
            {
                Console.WriteLine ("Hello, Mint is {0}", enabled ? "enabled": "disabled");
                try
                {
                    CreateDynamicMethod();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed with: {ex}");
                }
            }
            else
            {
                Console.WriteLine ($"Hello, System.Private.Mint.Enable is unset");
            }
        }

        private static bool voidVoidSample = false;

        private static void GenerateSample(ILGenerator ilgen)
        {
            if (voidVoidSample)
            {
                ilgen.Emit(OpCodes.Ldc_I4_S, (byte)23);
                ilgen.Emit(OpCodes.Pop);
                ilgen.Emit(OpCodes.Ret);

            }
            else
            {
                if (useSingleIntParam)
                {
                    ilgen.Emit(OpCodes.Ldarg_0);
                }
                else
                {
                    ilgen.Emit(OpCodes.Ldc_I4_S, (byte)40);
                }
                ilgen.Emit(OpCodes.Ldc_I4_S, (byte)2);
                ilgen.Emit(OpCodes.Add);
                if (useSingleIntParam)
                {
                    // this is redundant, but it will exercise the code path;
                    // and the Mint optimizer should eliminate all this code
                    ilgen.Emit(OpCodes.Starg_S, (byte)0);
                    ilgen.Emit(OpCodes.Ldarg_0);
                }
                ilgen.Emit(OpCodes.Ret);
            }
        }

        private static bool useSingleIntParam = true;
        private delegate int MeaningOfLife();
        static void CreateDynamicMethod()
        {
            var returnType = voidVoidSample ? typeof(void) : typeof(int);
            var paramTypes = useSingleIntParam ? new Type [] { typeof(int), typeof(double) } : Type.EmptyTypes;
            DynamicMethod dMethod = new DynamicMethod("MeaningOfLife", returnType, paramTypes, typeof(object).Module);
            if (dMethod is not null)
            {
                var mName = dMethod.Name;
                var mReturnType = dMethod.ReturnType;
                var mParams = dMethod.GetParameters();

                Console.WriteLine ($"DynamicMethod: '{dMethod.Name}'");
                Console.WriteLine ($"Return type: '{dMethod.ReturnType}'");
                Console.WriteLine ($"Has {mParams.Length} params:");
                int paramCnt = 0;
                foreach (var param in mParams)
                    Console.WriteLine ($"\tparam[{paramCnt++}] type: {param.ParameterType}");

                ILGenerator ilgen = dMethod.GetILGenerator();
                if (ilgen is null)
                    throw new Exception("ILGenerator is null");

                GenerateSample(ilgen);
                DumpILBytes(ilgen);

                RunSample(dMethod);
            }
            else
            {
                Console.WriteLine($"Failed to create a DynamicMethod");
            }
        }

        private static void RunSample(DynamicMethod dMethod)
        {
            if (voidVoidSample)
            {
                Action answer = (Action)dMethod.CreateDelegate(typeof(Action));
                if (answer is null)
                    throw new Exception("Delegate for the dynamic method is null");

                answer();
                Console.WriteLine("delegate returned");
            }
            else
            {
                MeaningOfLife answer = (MeaningOfLife)dMethod.CreateDelegate(typeof(MeaningOfLife));
                if (answer is null)
                    throw new Exception("Delegate for the dynamic method is null");

                var retVal = answer();
                Console.WriteLine($"The answer is: {retVal}");
            }
        }

        // Requires rooting DynamicILGenerator
        static void DumpILBytes(ILGenerator ilgen)
        {
            var ilBufferAccessor = ilgen.GetType().GetField("m_ILStream", BindingFlags.Instance | BindingFlags.NonPublic);
            var ilBufferLengthAccessor = ilgen.GetType().GetField("m_length", BindingFlags.Instance | BindingFlags.NonPublic);
            byte[] ilBuffer = ilBufferAccessor.GetValue(ilgen) as byte[];
            int ilBufferLength = (int)ilBufferLengthAccessor.GetValue(ilgen);

            Console.WriteLine("--------------------------");
            Console.WriteLine("ILBuffer contents: ");
            int i = 0;
            while (i < ilBufferLength)
            {
                if (i > 0 && i % 4 == 0)
                    Console.WriteLine();
                Console.Write(String.Format(" 0x{0:X}", ilBuffer[i]));
                i ++;
            }
            if (i % 4 != 1)
                Console.WriteLine();
            Console.WriteLine("--------------------------");
        }
    }
}
