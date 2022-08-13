#if Common_Unity
using System;
using MessagePack;
using Newtonsoft.Json;
using UnityEngine;

namespace Common.Unity.Net.WWW
{
    using Pipeline;

    public class HttpMessagePipeline<T> : IHttpMessageHandler
    {
        public readonly RawResponsePipeline RawResponsePipeline;
        public readonly IPreProcessPipeline PreProcessPipeline;
        public readonly ProcessPipeline<T> ProcessPipeline;
        public readonly PostProcessPipeline PostProcessPipeline;
        public readonly string API;

        public HttpMessagePipeline(RawResponsePipeline rawResponsePipeline, IPreProcessPipeline preProcessPipeline, ProcessPipeline<T> processPipeline, PostProcessPipeline postProcessPipeline, string api)
        {
            RawResponsePipeline = rawResponsePipeline;
            PreProcessPipeline = preProcessPipeline;
            ProcessPipeline = processPipeline;
            PostProcessPipeline = postProcessPipeline;
            API = api;
        }

        public void Handle(string rawMessage, int elapsedMs)
        {
            RawResponsePipeline?.PreProcess(rawMessage);

            T message = JsonConvert.DeserializeObject<T>(rawMessage);
#if UNITY_EDITOR
            UnityEngine.Debug.Log($"[WWW][{API}][{elapsedMs}ms] Json : {rawMessage}");
#endif
            Handle(message);
        }

        public void Handle(ReadOnlyMemory<byte> rawMessage, int elapsedMs)
        {
            RawResponsePipeline?.PreProcess(rawMessage);

            T message = MessagePackSerializer.Deserialize<T>(rawMessage);
            string jsonStr = string.Empty;
            try
            {
                jsonStr = JsonConvert.SerializeObject(message);
            }
            catch (Exception e)
            {
                // Json Serialize 도중 AOT 관련 Exception이 뜰 수 있음.
                // ExecutionEngineException: Attempting to call method 'Newtonsoft.Json.Utilities.DictionaryWrapper`2[[Dash.Types.AccountColumns, Dash, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null],[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]::.ctor' for which no ahead of time (AOT) code was generated.

                Debug.LogException(e);
            }

#if UNITY_EDITOR
            Debug.Log($"[WWW][{API}][{elapsedMs}ms] MsgPack : {jsonStr}");
#endif

            Handle(message);
        }

        private void Handle(T message)
        {

            bool preProcessResult = PreProcessPipeline?.PreProcess(message) ?? true;
            if (preProcessResult == false)
            {
                return;
            }

            HttpMessageProcessResult result = ProcessPipeline.Process(message);

            PostProcessPipeline?.PostProcess(result);
        }
    }
}
#endif