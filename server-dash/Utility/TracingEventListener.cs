using System.Diagnostics.Tracing;
using Common.Log;
using ILogger = NLog.ILogger;

namespace server_dash.Utility
{
    public class TracingEventListener : EventListener
    {
        private ILogger _logger = NLogUtility.GetCurrentClassLogger();
        private const int GC_KEYWORD = 0x0000001;

        public TracingEventListener()
        {
        }
     
        // Called whenever an EventSource is created.
        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            // Watch for the .NET runtime EventSource and enable all of its events.
            if (eventSource.Name.Equals("Microsoft-Windows-DotNETRuntime"))
            {
                EnableEvents(eventSource, EventLevel.Informational, (EventKeywords)(GC_KEYWORD));
            }
        }
     
        // Called whenever an event is written.
        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            // https://docs.microsoft.com/en-us/dotnet/framework/performance/garbage-collection-etw-events#gcheapstats_v1-event
            switch (eventData.EventName)
            {
                case "GCHeapStats_V1":
                    ProcessHeapStats(eventData);
                    break;
            }
        }

        private void ProcessHeapStats(EventWrittenEventArgs eventData)
        {
            GCEventTracer.ProcessHeapStats(eventData);
        }
    }
}

