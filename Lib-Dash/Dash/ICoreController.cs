using Dash.Model;
using Dash.State;
using System.Collections.Generic;

namespace Dash
{
    public interface ICoreController
    {
        void OnLeaveStage();
        void OnStageReady();
        void OnReviveRequest(int playerId);
        void OnGameEnd();
        void HandleSuspect(SuspectInfo suspectInfo);
        void OnDropPlayerByValidationFailed(int playerId);
        void ChangeSessionPlayer(int sessionPlayerId, int desPlayerId);
    }

    public class NullCoreController : ICoreController
    {
        void ICoreController.OnLeaveStage() { }
        void ICoreController.OnStageReady() { }
        void ICoreController.OnReviveRequest(int playerId) { }
        void ICoreController.OnGameEnd() { }
        void ICoreController.HandleSuspect(SuspectInfo suspectInfo) { }
        void ICoreController.OnDropPlayerByValidationFailed(int playerId) { }
        void ICoreController.ChangeSessionPlayer(int sessionPlayerId, int desPlayerId) { }
    }
}