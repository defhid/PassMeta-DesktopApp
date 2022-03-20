namespace PassMeta.DesktopApp.Common.Interfaces
{
    /// <summary>
    /// Simple result.
    /// </summary>
    public interface IResult
    {
        /// <summary>
        /// Result is succes.
        /// </summary>
        bool Ok { get; }

        /// <summary>
        /// Result is failure.
        /// </summary>
        bool Bad { get; }
    }

    /// <summary>
    /// Simple result with data.
    /// </summary>
    public interface IResult<out TData> : IResult
    {
        /// <summary>
        /// Result optional data.
        /// </summary>
        /// <remarks>Must be not default when <see cref="IDetailedResult.Ok"/>.</remarks>
        TData? Data { get; }
    }

    /// <summary>
    /// Result with optional message.
    /// </summary>
    public interface IDetailedResult : IResult
    {
        /// <summary>
        /// Result optional message.
        /// </summary>
        /// <remarks>Must be not null when <see cref="IDetailedResult.Bad"/>.</remarks>
        string? Message { get; }
    }
    
    /// <summary>
    /// Result with data and optional failure message.
    /// </summary>
    public interface IDetailedResult<out TData> : IDetailedResult, IResult<TData>
    {
    }
}