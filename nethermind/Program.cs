using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Nethermind.Evm.Test;

namespace EVMConsoleApp
{
    class Program : VirtualMachineTestsBase
    {
        static void Main(string[] args)
        {
            var program = new Program();


            program.Setup();

            string contractCodeHex = string.Empty;
            string calldataHex = string.Empty;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--code" && i + 1 < args.Length)
                {
                    contractCodeHex = args[++i];
                }
                else if (args[i] == "--input" && i + 1 < args.Length)
                {
                    calldataHex = args[++i];
                }
            }

            if (string.IsNullOrEmpty(contractCodeHex) || string.IsNullOrEmpty(calldataHex))
            {
                Console.WriteLine("Usage: nethermind--code <contract-code> --input <calldata>");
                return;
            }

            var contractCode = Enumerable.Range(0, contractCodeHex.Length / 2)
                .Select(x => Convert.ToByte(contractCodeHex.Substring(x * 2, 2), 16))
                .ToArray();

            var calldata = Enumerable.Range(0, calldataHex.Length / 2)
                .Select(x => Convert.ToByte(calldataHex.Substring(x * 2, 2), 16))
                .ToArray();

            var traceResult = program.ExecuteAndTrace(contractCode);

            foreach (var entry in traceResult.Entries)
            {
                string jsonOutput = JsonSerializer.Serialize(entry);
                Console.WriteLine(jsonOutput);
            }
            
            var outputData = traceResult.ReturnValue;
            
            long totalGasUsed = 0;

            foreach (var entry in traceResult.Entries)
            {
              if (entry.Opcode != null)
              {
                totalGasUsed += entry.GasCost;
              }
            }

            var finalOutput = new
            {
                output = Convert.ToHexString(outputData),
                gasUsed = $"0x{totalGasUsed:x}"
            };

            string jsonFinalOutput = JsonSerializer.Serialize(finalOutput);
            Console.WriteLine(jsonFinalOutput);

            program.TearDown(); 
        }
    }
}
