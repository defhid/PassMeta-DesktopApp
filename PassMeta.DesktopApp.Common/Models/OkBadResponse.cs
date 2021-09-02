using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PassMeta.DesktopApp.Common.Interfaces.Services;
using PassMeta.DesktopApp.Core;

namespace PassMeta.DesktopApp.Common.Models
{
    public class OkBadResponse
    {
        [JsonProperty("message")]
        public string Message { get; set; }
        
        [JsonProperty("what")]
        public string What { get; set; }
        
        [JsonProperty("sub")]
        public List<OkBadResponse> Sub { get; set; }
        
        [JsonProperty("more")]
        public OkBadMore More { get; set; }

        public bool Success => Message == "OK";

        public bool Failure => Message != "OK";

        public string ToFullLocalizedString([AllowNull] IOkBadService okBadService)
        {
            var message = okBadService?.GetLocalizedMessage(Message) ?? Message;
            
            var builder = new List<string> { What is null ? message : (message + $" ({What})") };

            if (More is not null)
                builder.Add(More.ToString());

            if (Sub is not null)
                builder.AddRange(Sub.Select(okBad => "--" + Environment.NewLine + okBad.ToFullLocalizedString(okBadService)));

            return string.Join(Environment.NewLine, builder);
        }
    }

    public class OkBadResponse<TData> : OkBadResponse
    {
        [JsonProperty("data")]
        public TData Data { get; set; }
    }
    
    public class OkBadMore
    {
        [JsonProperty("text")]
        public string Text { get; set; }
            
        [JsonProperty("info")]
        public JObject Info { get; set; }
            
        [JsonProperty("allowed")]
        public JArray Allowed { get; set; }
            
        [JsonProperty("disallowed")]
        public JArray Disallowed { get; set; }

        [JsonProperty("required")]
        public JArray Required { get; set; }

        [JsonProperty("min_allowed")]
        public double? MinAllowed { get; set; }
            
        [JsonProperty("max_allowed")]
        public double? MaxAllowed { get; set; }

        public override string ToString()
        {
            var builder = new List<string>();
            
            if (Text is not null)
                builder.Add(Text);
            if (Info is not null) 
                builder.Add($"{Resources.OKBAD_MORE__INFO}: {Info}");
            if (Allowed is not null) 
                builder.Add($"{Resources.OKBAD_MORE__ALLOWED}: {string.Join(", ", Allowed)}");
            if (Disallowed is not null) 
                builder.Add($"{Resources.OKBAD_MORE__DISALLOWED}: {string.Join(", ", Disallowed)}");
            if (Required is not null) 
                builder.Add($"{Resources.OKBAD_MORE__REQUIRED}: {string.Join(", ", Required)}");
            if (MinAllowed is not null) 
                builder.Add($"{Resources.OKBAD_MORE__MIN_ALLOWED}: {MinAllowed.Value.ToString(Resources.Culture)}");
            if (MaxAllowed is not null) 
                builder.Add($"{Resources.OKBAD_MORE__MAX_ALLOWED}: {MaxAllowed.Value.ToString(Resources.Culture)}");

            return string.Join(Environment.NewLine, builder);
        }
    }
}