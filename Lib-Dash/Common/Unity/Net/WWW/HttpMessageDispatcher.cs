#if Common_Unity

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.Unity.Net.WWW;
using Common.Utility;

namespace Common.Unity.Net.WWW
{
    using Pipeline;

    public class HttpMessageDispatcher<T>
    {
        private readonly Dictionary<string, IHttpMessageHandler> _messageHandlers =
            new Dictionary<string, IHttpMessageHandler>();

        public readonly List<RawResponsePipeline.RawStringResponseProcessDelegate> RawStringResponseProcessDelegates = new List<RawResponsePipeline.RawStringResponseProcessDelegate>();
        public readonly List<RawResponsePipeline.RawBytesResponseProcessDelegate> RawBytesResponseProcessDelegates = new List<RawResponsePipeline.RawBytesResponseProcessDelegate>();

        public readonly List<PreProcessPipeline<T>.PreProcessDelegate> PreProcessDelegates = new List<PreProcessPipeline<T>.PreProcessDelegate>();
        public readonly List<PostProcessPipeline.PostProcessDelegate> PostProcessDelegates = new List<PostProcessPipeline.PostProcessDelegate>();

        private static readonly Func<MethodInfo, bool> _handlerMethodFilter = (method) => method.IsPublic == true && method.IsStatic == false;
        private static readonly Type _typeInfoHolderType = typeof(TypeInfoHolder<>);

        public void Init()
        {
            _messageHandlers.Clear();
            RawStringResponseProcessDelegates.Clear();
            RawBytesResponseProcessDelegates.Clear();
            PreProcessDelegates.Clear();
            PostProcessDelegates.Clear();
        }


        public void RegisterHandler(object target)
        {
            foreach (var method in (IReadOnlyList<MethodInfo>)_typeInfoHolderType.MakeGenericType(target.GetType()).GetMethod("GetMethods").Invoke(null, new object[] { _handlerMethodFilter }))
            {
                var attribute = (HttpMessageAttribute)method.GetCustomAttributes(typeof(HttpMessageAttribute), false)
                    .SingleOrDefault();

                if (attribute == null)
                {
                    continue;
                }

                ParameterInfo[] parameters = method.GetParameters();
                if (parameters == null || parameters.Length != 1)
                {
                    Log.Logger.Error($"Parameter Error : {method}");
                    continue;
                }

                Type paramType = parameters[0].ParameterType;
                Delegate func = method.CreateDelegate(typeof(Func<,>).MakeGenericType(paramType, typeof(bool)), target);

                RawResponsePipeline rawResponsePipeline = new RawResponsePipeline();
                RawStringResponseProcessDelegates.ForEach(p => rawResponsePipeline.Add(p));
                RawBytesResponseProcessDelegates.ForEach(p => rawResponsePipeline.Add(p));

                PreProcessPipeline<T> preProcessPipeline = new PreProcessPipeline<T>();
                PreProcessDelegates.ForEach(p => preProcessPipeline.Add(p));

                IProcessPipeline processPipeline = (IProcessPipeline)Activator.CreateInstance(typeof(ProcessPipeline<>).MakeGenericType(paramType));
                processPipeline.Add(func);

                PostProcessPipeline postProcessPipeline = new PostProcessPipeline();
                PostProcessDelegates.ForEach(p => postProcessPipeline.Add(p));


                var handler = (IHttpMessageHandler)Activator.CreateInstance(typeof(HttpMessagePipeline<>).MakeGenericType(paramType),
                    new object[] { rawResponsePipeline, preProcessPipeline, processPipeline, postProcessPipeline, attribute.API });

                // do not allow multiple-binding now.
                if (string.IsNullOrEmpty(attribute.API) == true)
                {
                    UnityEngine.Debug.Log($"attribute.API is null. {method}");
                }

                if (_messageHandlers.ContainsKey(attribute.API) == true)
                {
                    throw new AlreadyHttpMessageBindingException(attribute.API);
                }

#if UNITY_EDITOR
                var httpParamType = typeof(T);
                if (paramType != httpParamType && paramType.IsSubclassOf(httpParamType) == false)
                {
                    Log.Logger.Error($"paramType: {paramType} / httpParamType: {httpParamType}");
                    throw new InvalidProgramException($"Invalid paramType : {paramType.Name}");
                }
#endif

                _messageHandlers.Add(attribute.API, handler);
#if UNITY_EDITOR
                UnityEngine.Debug.Log($"RestfulApi : {attribute.API} Hash : {attribute.API.GetHashCode()}");
#endif
            }
        }

        public void UnregisterHandler(object target)
        {
            foreach (var method in target.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                var attribute = (HttpMessageAttribute)method.GetCustomAttributes(typeof(HttpMessageAttribute), false)
                    .SingleOrDefault();

                if (attribute == null)
                {
                    continue;
                }

                _messageHandlers.Remove(attribute.API);
            }
        }

        public void Dispatch(string restfulApi, string responseBody, int elapsedMs)
        {
            if (_messageHandlers.TryGetValue(restfulApi, out IHttpMessageHandler httpHandler))
            {
                httpHandler.Handle(responseBody, elapsedMs);
            }
            else
            {
                throw new Exception($"no bound api : {restfulApi}");
            }
        }

        public void Dispatch(string restfulApi, ReadOnlyMemory<byte> responseBody, int elapsedMs)
        {
            if (_messageHandlers.TryGetValue(restfulApi, out IHttpMessageHandler httpHandler))
            {
                httpHandler.Handle(responseBody, elapsedMs);
            }
            else
            {
                throw new Exception($"no bound api : {restfulApi}");
            }
        }
    }

    public class AlreadyHttpMessageBindingException : Exception
    {
        public AlreadyHttpMessageBindingException(string attributeRestfulApi)
            : base(attributeRestfulApi)
        {
        }
    }
}
#endif