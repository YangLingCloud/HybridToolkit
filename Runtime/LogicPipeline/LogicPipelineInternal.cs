using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace HybridToolkit.LogicPipeline.Internal
{
    // -----------------------------------------------------------
    // [组合器] Async + Async (带热路径优化)
    // -----------------------------------------------------------
    internal sealed class CombinedAsyncProcessor<TIn, TMid, TOut> : IAsyncProcessor<TIn, TOut>
    {
        readonly IAsyncProcessor<TIn, TMid> _first;
        readonly IAsyncProcessor<TMid, TOut> _second;

        public CombinedAsyncProcessor(IAsyncProcessor<TIn, TMid> first, IAsyncProcessor<TMid, TOut> second)
        {
            _first = first;
            _second = second;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<TOut> ProcessAsync(TIn input, CancellationToken ct)
        {
            var task1 = _first.ProcessAsync(input, ct);

            // Hot-Path Optimization: 如果前置任务已同步完成，直接执行后续逻辑，跳过 await 状态机
            if (task1.Status == UniTaskStatus.Succeeded)
            {
                var mid = task1.GetAwaiter().GetResult();
                if (ct.IsCancellationRequested) return UniTask.FromCanceled<TOut>(ct);
                return _second.ProcessAsync(mid, ct);
            }

            // Slow-Path
            return ProcessAsyncInternal(task1, ct);
        }

        private async UniTask<TOut> ProcessAsyncInternal(UniTask<TMid> task1, CancellationToken ct)
        {
            var mid = await task1;
            if (ct.IsCancellationRequested) throw new OperationCanceledException(ct);
            return await _second.ProcessAsync(mid, ct);
        }
    }

    // -----------------------------------------------------------
    // [组合器] Async + Sync (带热路径优化)
    // -----------------------------------------------------------
    internal sealed class CombinedAsyncSyncProcessor<TIn, TMid, TOut> : IAsyncProcessor<TIn, TOut>
    {
        readonly IAsyncProcessor<TIn, TMid> _first;
        readonly IProcessor<TMid, TOut> _second;

        public CombinedAsyncSyncProcessor(IAsyncProcessor<TIn, TMid> first, IProcessor<TMid, TOut> second)
        {
            _first = first;
            _second = second;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<TOut> ProcessAsync(TIn input, CancellationToken ct)
        {
            var task1 = _first.ProcessAsync(input, ct);

            if (task1.Status == UniTaskStatus.Succeeded)
            {
                var mid = task1.GetAwaiter().GetResult();
                if (ct.IsCancellationRequested) return UniTask.FromCanceled<TOut>(ct);
                try
                {
                    return UniTask.FromResult(_second.Process(mid));
                }
                catch (Exception ex)
                {
                    return UniTask.FromException<TOut>(ex);
                }
            }

            return ProcessAsyncInternal(task1, ct);
        }

        private async UniTask<TOut> ProcessAsyncInternal(UniTask<TMid> task1, CancellationToken ct)
        {
            var mid = await task1;
            if (ct.IsCancellationRequested) throw new OperationCanceledException(ct);
            return _second.Process(mid);
        }
    }

    // -----------------------------------------------------------
    // [适配器] Sync -> Async
    // -----------------------------------------------------------
    internal sealed class SyncToAsyncAdapter<TIn, TOut> : IAsyncProcessor<TIn, TOut>
    {
        readonly IProcessor<TIn, TOut> _syncProcessor;
        public SyncToAsyncAdapter(IProcessor<TIn, TOut> p) => _syncProcessor = p;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<TOut> ProcessAsync(TIn input, CancellationToken ct)
        {
            if (ct.IsCancellationRequested) return UniTask.FromCanceled<TOut>(ct);
            try
            {
                return UniTask.FromResult(_syncProcessor.Process(input));
            }
            catch (Exception ex)
            {
                return UniTask.FromException<TOut>(ex);
            }
        }
    }

    // -----------------------------------------------------------
    // [委托] Async Lambda 包装器 (之前缺失的类)
    // -----------------------------------------------------------
    internal sealed class DelegateAsyncProcessor<TIn, TOut> : IAsyncProcessor<TIn, TOut>
    {
        readonly Func<TIn, CancellationToken, UniTask<TOut>> _func;
        public DelegateAsyncProcessor(Func<TIn, CancellationToken, UniTask<TOut>> func) => _func = func;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<TOut> ProcessAsync(TIn input, CancellationToken ct)
        {
            return _func(input, ct);
        }
    }

    // -----------------------------------------------------------
    // [委托] Sync Lambda 包装器
    // -----------------------------------------------------------
    internal sealed class DelegateSyncProcessor<TIn, TOut> : IProcessor<TIn, TOut>
    {
        readonly Func<TIn, TOut> _func;
        public DelegateSyncProcessor(Func<TIn, TOut> func) => _func = func;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TOut Process(TIn input)
        {
            return _func(input);
        }
    }
}