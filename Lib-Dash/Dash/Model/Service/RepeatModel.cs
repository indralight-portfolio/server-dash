using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace Dash.Model.Service
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RepeatType
    {
        None,
        Daily,
        Weekday,
    }
    public class RepeatModel
    {
        public RepeatType RepeatType { get; set; }
        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public List<DayOfWeek> DayOfWeeks { get; set; }
    }
}