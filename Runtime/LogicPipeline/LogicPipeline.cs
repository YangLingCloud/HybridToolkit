using System.Threading;
using Cysharp.Threading.Tasks;

namespace HybridToolkit.LogicPipeline
{
    /// <summary>
    /// 同步处理单元接口
    /// </summary>
    public interface IProcessor<TIn, TOut>
    {
        TOut Process(TIn input);
    }

    /// <summary>
    /// 异步处理单元接口 (UniTask版本)
    /// </summary>
    public interface IAsyncProcessor<TIn, TOut>
    {
        // 移除 in/out 协变以支持 struct UniTask 的 0GC
        UniTask<TOut> ProcessAsync(TIn input, CancellationToken ct);
    }

}