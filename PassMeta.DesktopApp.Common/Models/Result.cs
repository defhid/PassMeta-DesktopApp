namespace PassMeta.DesktopApp.Common.Models
{
    using System.Runtime.CompilerServices;
    using Abstractions;
    using Utils.Extensions;

    /// <summary>
    /// Result factory.
    /// </summary>
    public static class Result
    {
        /// <summary>
        /// Make success result with optional message.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDetailedResult Success() => new ResultModel(true);
        
        /// <summary>
        /// Make success result with data.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDetailedResult<TData> Success<TData>(TData data) => new ResultModel<TData>(data);
        
        /// <summary>
        /// Make failure result.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IResult Failure() => new ResultModel(false);
        
        /// <summary>
        /// Make failure result with message.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDetailedResult Failure(string message) => new ResultModel(false, message);
        
        /// <summary>
        /// Make failure result with default data.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IResult<TData> Failure<TData>() => new ResultModel<TData>(false);
        
        /// <summary>
        /// Make failure result with default data and message.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDetailedResult<TData> Failure<TData>(string message) => new ResultModel<TData>(false, message);

        /// <summary>
        /// Make success/failure result from response.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDetailedResult FromResponse(OkBadResponse? response) 
            => response?.Success is true 
                ? new ResultModel() 
                : new ResultModel(false, response.GetFullMessage());

        /// <summary>
        /// Make success/failure result from response.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDetailedResult<TData> FromResponse<TData>(OkBadResponse<TData>? response)
            => response?.Success is true 
                ? new ResultModel<TData>(response.Data!) 
                : new ResultModel<TData>(false, response.GetFullMessage());

        /// <summary>
        /// Make success/failure result depending on boolean <paramref name="ok"/> value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IResult From(bool ok) => new ResultModel(ok);

        /// <summary>
        /// Make success/failure data result depending on boolean <paramref name="ok"/> value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IResult<TData> From<TData>(bool ok, TData data) where TData : notnull => ok
            ? new ResultModel<TData>(data) 
            : new ResultModel<TData>(false);
    }

    /// <inheritdoc cref="IDetailedResult"/>
    internal readonly struct ResultModel : IDetailedResult
    {
        /// <inheritdoc />
        public bool Ok { get; }

        /// <inheritdoc />
        public bool Bad => !Ok;

        /// <inheritdoc />
        public string? Message { get; }

        /// <summary></summary>
        internal ResultModel(bool ok, string? message = null)
        {
            Ok = ok;
            Message = message;
        }
    }

    /// <inheritdoc cref="IDetailedResult{TData}"/>
    internal readonly struct ResultModel<TData> : IDetailedResult<TData>
    {
        /// <inheritdoc />
        public bool Ok { get; }

        /// <inheritdoc />
        public bool Bad => !Ok;

        /// <inheritdoc />
        public string? Message { get; }

        /// <inheritdoc />
        public TData? Data { get; }

        internal ResultModel(TData data)
        {
            Ok = true;
            Message = null;
            Data = data;
        }

        internal ResultModel(bool ok, string? message = null)
        {
            Ok = ok;
            Message = message;
            Data = default;
        }
    }
}