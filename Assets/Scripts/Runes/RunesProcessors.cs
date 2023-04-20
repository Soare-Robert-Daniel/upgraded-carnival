using System.Collections.Generic;
using System.Linq;
using GameEntities;

namespace Runes
{
    public enum ProcessorType
    {
        Attack,
        Slow
    }

    public class RunesProcessors
    {


        public ProcessorType ProcessorKind { get; set; }

        private static RunesHandlerForRoom.QueryResultRunProcessing SumRunes(List<RuneToken> tokens)
        {
            var result = new RunesHandlerForRoom.QueryResultRunProcessing();

            // Sum all the values using a single loop.
            foreach (var token in tokens)
            {
                result.damage += token.runeValue.damage;
                result.slow += token.runeValue.slow;
            }

            return result;
        }

        private static RunesHandlerForRoom.QueryResultRunProcessing SumAttackRunes(List<RuneToken> tokens)
        {
            // Filter out all tokens that are not of type Attack.
            tokens = tokens.Where(token => token.runeType == RuneType.Attack).ToList();

            return SumRunes(tokens);
        }

        private static RunesHandlerForRoom.QueryResultRunProcessing SumSlowRunes(List<RuneToken> tokens)
        {
            // Filter out all tokens that are not of type Slow.
            tokens = tokens.Where(token => token.runeType == RuneType.Slow).ToList();

            return SumRunes(tokens);
        }

        public RunesHandlerForRoom.QueryResultRunProcessing BuildQuery(List<RuneToken> tokens)
        {
            return ProcessorKind switch
            {
                ProcessorType.Attack => SumAttackRunes(tokens),
                ProcessorType.Slow => SumSlowRunes(tokens),
                _ => new RunesHandlerForRoom.QueryResultRunProcessing()
            };
        }
    }

    public static class RunesProcessorFactory
    {
        public static RunesProcessors CreateProcessor(ProcessorType processorType)
        {
            return new RunesProcessors
            {
                ProcessorKind = processorType
            };
        }
    }
}