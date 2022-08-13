using System;
using Dash.Command;
using Dash.Core;
using Dash.Core.Systems;
using Dash.Types;

namespace Dash
{
    public interface IBridge
    {
        /// <summary>
        /// 해당 Command를 자신 포함 연결된 Core들에서 실행되게끔 전송한다.
        /// </summary>
        void Put<T>(T command) where T : Command.ICommand;
        /// <summary>
        /// 해당 Command를 연결된 Core들에서 실행되게끔 전송한다.
        /// </summary>
        void Broadcast<T>(T command, int expectCoreId = int.MinValue) where T : Command.ICommand;
        /// <summary>
        /// 특정 Core에게 Command를 전송한다.(Client에서는 서버 Core만 지정 가능)
        /// </summary>
        void SendTo(int coreId, Command.ICommand command);
        /// <summary>
        /// 즉시 해당 Command를 실행시킨다.
        /// </summary>
        void ProcessImmediate(Command.ICommand command);

        /// <summary>
        /// Core 내부에서 사용하지 않는다.
        /// 외부로부터 들어온 Command를 자신 Core에서 실행시키게 한다.
        /// </summary>
        void ProcessReceivedCommand(int senderCoreId, Command.ICommand command);
        /// <summary>
        /// Core 내부에서 사용하지 않는다. ByteBuffer를 Flush해 소켓단에서 전송이 일어나게 한다.
        /// </summary>
        void FlushCommands();
        /// <summary>
        /// Core들간 연결을 시도한다.
        /// </summary>
        void Establish(Action<bool, BridgeEstablishErrorType> callback);
    }

    public class SandBoxBridge : IBridge
    {
        private CoreModule _coreModule;
        private CommandSystem _commandSystem;

        public SandBoxBridge(CoreModule coreModule)
        {
            _coreModule = coreModule;
            _commandSystem = coreModule.ManualSystemGroup.GetSystem<CommandSystem>();
        }

        public void Put<T>(T command) where T : ICommand
        {
            _commandSystem.ProcessSelfCommand(command);
        }

        public void Broadcast<T>(T command, int exceptCoreId) where T : ICommand
        {
            // nothing to do.
        }

        public void SendTo(int coreId, ICommand command)
        {
            // nothing to do.
        }

        public void ProcessReceivedCommand(int senderCoreId, ICommand command)
        {
            _commandSystem.ProcessReceivedCommand(senderCoreId, command);
        }

        public void ProcessImmediate(ICommand command)
        {
            _commandSystem.ProcessImmediate(command);
        }

        public void FlushCommands()
        {
        }

        public void Establish(Action<bool, BridgeEstablishErrorType> callback)
        {
            callback?.Invoke(true, BridgeEstablishErrorType.Success);
        }
    }
}