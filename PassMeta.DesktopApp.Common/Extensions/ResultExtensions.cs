using System.Diagnostics;
using System.Runtime.CompilerServices;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Models;

namespace PassMeta.DesktopApp.Common.Extensions;

/// <summary>
/// Extension methods for <see cref="IResult"/> and its submodels.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Cast to <see cref="IResult{TData}"/> with null data.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IResult<TData> WithNullData<TData>(this IResult result) 
        => new ResultModel<TData>(result.Ok);

    /// <summary>
    /// Cast to <see cref="IDetailedResult{TData}"/> with null data.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDetailedResult<TData> WithNullData<TData>(this IDetailedResult result) 
        => new ResultModel<TData>(result.Ok, result.Message);

    /// <summary>
    /// Cast to <see cref="IDetailedResult"/> without message.
    /// </summary>
    /// <remarks>Result must be <see cref="IResult.Ok"/>!</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDetailedResult AsDetailed(this IResult result)
    {
        Debug.Assert(result.Ok);
        return new ResultModel(result.Ok);
    }
        
    /// <summary>
    /// Cast to <see cref="IDetailedResult"/> without message.
    /// </summary>
    /// <remarks>Result must be <see cref="IResult.Ok"/>!</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDetailedResult AsDetailed<TData>(this IResult<TData> result)
    {
        Debug.Assert(result.Ok);
        return new ResultModel<TData>(result.Data!);
    }
}