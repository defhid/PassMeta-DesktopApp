using System.Diagnostics.CodeAnalysis;

namespace PassMeta.DesktopApp.Common.Models
{
    public readonly struct Result
    {
        public readonly bool Ok;
        [AllowNull] public readonly string Message;

        public bool Failure => !Ok;
        
        public Result(bool ok, string message = null)
        {
            Ok = ok;
            Message = message;
        }
    }
    
    public readonly struct Result<TData>
    {
        public readonly bool Ok;
        [AllowNull] public readonly string Message;
        [AllowNull] public readonly TData Data;
        
        public bool Failure => !Ok;

        public Result(TData data, string message = null)
        {
            Ok = true;
            Message = message;
            Data = data;
        }
        
        public Result(object data, string message = null)
        {
            Ok = true;
            Message = message;
            Data = (TData)data;
        }
        
        public Result(bool ok, string message = null)
        {
            Ok = ok;
            Message = message;
            Data = default;
        }
    }
}