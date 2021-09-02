using System.Diagnostics.CodeAnalysis;

namespace PassMeta.DesktopApp.Common.Models
{
    public readonly struct Result
    {
        public readonly bool Ok;
        
        public Result(bool ok)
        {
            Ok = ok;
        }
    }
    
    public readonly struct Result<TData>
    {
        public readonly bool Ok;
        
        [AllowNull] public readonly TData Data;

        public Result(bool ok, TData data)
        {
            Ok = ok;
            Data = data;
        }

        public Result(bool ok)
        {
            Ok = ok;
            Data = default;
        }
    }
}