using Dash.Protocol;
using Microsoft.AspNetCore.Mvc;
using server_dash.Lobby.Services;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace server_dash.Lobby.Controller.Lobby
{
    public class InventoryController : LobbyAPIController
    {
        private static NLog.Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        private InventoryService _service;
        public InventoryController(InventoryService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<UpdateUsingHeroResponse>> UpdateUsingHero(ulong oidAccount,
            [Required][FromForm]int heroId)
        {
            return Ok(await _service.UpdateUsingHero(oidAccount, heroId));
        }

        [HttpPost]
        public async Task<ActionResult<UpdateEquipmentSlotResponse>> UpdateEquipmentSlot(ulong oidAccount,
            [Required][FromForm]int equipmentSerial, [Required][FromForm]int slotIndex)
        {
            return Ok(await _service.UpdateEquipmentSlot(oidAccount, (uint)equipmentSerial, slotIndex));
        }

        [HttpPost]
        public async Task<ActionResult<UpdateEmblemResponse>> UpdateEmblem(ulong oidAccount,
            [Required][FromForm]int heroId, [Required][FromForm]int emblemId)
        {
            return Ok(await (_service.UpdateEmblem(oidAccount, heroId, emblemId)));
        }
    }
}