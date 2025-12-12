using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using HybridToolkit.LogicPipeline.Internal;

namespace HybridToolkit.LogicPipeline
{
    public static class PipelineExtensions
    {
        // --------------------------------------------------------------------
        // [连接] 同步 -> 异步 (Sync -> Async)
        // --------------------------------------------------------------------
        public static IAsyncProcessor<TIn, TNext> ThenAsync<TIn, TOut, TNext>(
            this IProcessor<TIn, TOut> first,
            IAsyncProcessor<TOut, TNext> second)
        {
            var adapter = new SyncToAsyncAdapter<TIn, TOut>(first);
            return new CombinedAsyncProcessor<TIn, TOut, TNext>(adapter, second);
        }

        // 支持 Lambda: .ThenAsync((input, ct) => ...)
        public static IAsyncProcessor<TIn, TNext> ThenAsync<TIn, TOut, TNext>(
            this IProcessor<TIn, TOut> first,
            Func<TOut, CancellationToken, UniTask<TNext>> func)
        {
            // 这里使用了 Internal 命名空间下的 DelegateAsyncProcessor
            return first.ThenAsync(new DelegateAsyncProcessor<TOut, TNext>(func));
        }

        // --------------------------------------------------------------------
        // [连接] 异步 -> 异步 (Async -> Async)
        // --------------------------------------------------------------------
        public static IAsyncProcessor<TIn, TNext> ThenAsync<TIn, TOut, TNext>(
            this IAsyncProcessor<TIn, TOut> first,
            IAsyncProcessor<TOut, TNext> second)
        {
            return new CombinedAsyncProcessor<TIn, TOut, TNext>(first, second);
        }

        // 支持 Lambda
        public static IAsyncProcessor<TIn, TNext> ThenAsync<TIn, TOut, TNext>(
            this IAsyncProcessor<TIn, TOut> first,
            Func<TOut, CancellationToken, UniTask<TNext>> func)
        {
            return first.ThenAsync(new DelegateAsyncProcessor<TOut, TNext>(func));
        }

        // --------------------------------------------------------------------
        // [连接] 异步 -> 同步 (Async -> Sync)
        // --------------------------------------------------------------------
        public static IAsyncProcessor<TIn, TNext> Then<TIn, TOut, TNext>(
            this IAsyncProcessor<TIn, TOut> first,
            IProcessor<TOut, TNext> second)
        {
            return new CombinedAsyncSyncProcessor<TIn, TOut, TNext>(first, second);
        }

        // 支持 Lambda
        public static IAsyncProcessor<TIn, TNext> Then<TIn, TOut, TNext>(
            this IAsyncProcessor<TIn, TOut> first,
            Func<TOut, TNext> func)
        {
            return new CombinedAsyncSyncProcessor<TIn, TOut, TNext>(first, new DelegateSyncProcessor<TOut, TNext>(func));
        }

        // --------------------------------------------------------------------
        // [编译]
        // --------------------------------------------------------------------
        public static Func<TIn, CancellationToken, UniTask<TOut>> Compile<TIn, TOut>(
            this IAsyncProcessor<TIn, TOut> processor)
        {
            return processor.ProcessAsync;
        }
    }
}