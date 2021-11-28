namespace PassMeta.DesktopApp.Common.Models
{
    public readonly struct Result
    {
        /// <summary>
        /// Result is success.
        /// </summary>
        public readonly bool Ok;
        
        /// <summary>
        /// Result optional message;
        /// </summary>
        public readonly string? Message;

        /// <summary>
        /// Not <see cref="Ok"/>.
        /// </summary>
        public bool Bad => !Ok;
        
        private Result(bool ok, string? message = null)
        {
            Ok = ok;
            Message = message;
        }
        
        public static Result Success(string? message = null) => new(true, message);
        public static Result Failure(string? message = null) => new(false, message);
        
        public static Result<TData> Success<TData>(TData data, string? message = null) => new(data, message);
        public static Result<TData> Failure<TData>(string? message = null) => new(false, message);
        
        public static Result FromResponse(OkBadResponse? response) => response?.Success is true
            ? new Result()
            : new Result(false, response?.Message);
        public static Result<TData> FromResponse<TData>(OkBadResponse<TData>? response) => response?.Success is true
            ? new Result<TData>(response.Data!)
            : new Result<TData>(false, response?.Message);
        
        public static Result From(bool ok) => new(ok);
        public static Result<TData> From<TData>(bool ok, TData data) => ok 
            ? new Result<TData>(data) 
            : new Result<TData>(false);
    }
    
    public readonly struct Result<TData>
    {
        /// <summary>
        /// Result is success.
        /// </summary>
        public readonly bool Ok;
        
        /// <summary>
        /// Result optional message;
        /// </summary>
        public readonly string? Message;
        
        /// <summary>
        /// Result data.
        /// </summary>
        /// <remarks>Null if <see cref="Bad"/>.</remarks>
        public readonly TData? Data;
        
        /// <summary>
        /// Not <see cref="Ok"/>.
        /// </summary>
        public bool Bad => !Ok;

        internal Result(TData data, string? message = null)
        {
            Ok = true;
            Message = message;
            Data = data;
        }
        
        internal Result(object data, string? message = null)
        {
            Ok = true;
            Message = message;
            Data = (TData)data;
        }
        
        internal Result(bool ok, string? message = null)
        {
            Ok = ok;
            Message = message;
            Data = default;
        }
    }
}