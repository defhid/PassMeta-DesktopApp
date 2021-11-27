namespace PassMeta.DesktopApp.Common.Models
{
    public readonly struct Result
    {
        public readonly bool Ok;
        public readonly string? Message;

        public bool Bad => !Ok;
        
        public Result(bool ok, string? message = null)
        {
            Ok = ok;
            Message = message;
        }
        
        public static Result Success(string? message = null) => new(true, message);
        public static Result Failure(string? message = null) => new(false, message);
        public static Result<TData> Success<TData>(TData data, string? message = null) => new(data, message);
        public static Result<TData> Failure<TData>(string? message = null) => new(false, message);
    }
    
    public readonly struct Result<TData>
    {
        public readonly bool Ok;
        public readonly string? Message;
        public readonly TData? Data;
        
        public bool Bad => !Ok;

        public Result(TData data, string? message = null)
        {
            Ok = true;
            Message = message;
            Data = data;
        }
        
        public Result(object data, string? message = null)
        {
            Ok = true;
            Message = message;
            Data = (TData)data;
        }
        
        public Result(bool ok, string? message = null)
        {
            Ok = ok;
            Message = message;
            Data = default;
        }
    }
}