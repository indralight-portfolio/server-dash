using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Dash.Protocol;
using Microsoft.AspNetCore.Mvc;

namespace server_dash.Lobby.Controller.Lobby
{
    public class UpgradeController : LobbyAPIController
    {
        private Services.UpgradeService _service;
        public UpgradeController(Services.UpgradeService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<LevelUpEquipmentResponse>> LevelUpEquipment(ulong oidAccount, 
            [Required][FromForm]int itemSerial, [Required][FromForm]bool useJewel)
        {
            return Ok(await _service.LevelUpEquipment(oidAccount, itemSerial, useJewel));
        }
        
        [HttpPost]
        public async Task<ActionResult<UpgradeCharacterResponse>> UpgradeCharacter(ulong oidAccount,
            [Required][FromForm]int characterId, [Required][FromForm]bool useJewel)
        {
            return Ok(await _service.UpgradeCharacter(oidAccount, characterId, useJewel));
        }

        [HttpPost]
        public async Task<ActionResult<CombineEquipmentResponse>> CombineEquipment(ulong oidAccount, 
            [Required][FromForm]int targetItemSerial, [Required][FromForm]int consumeItemSerial1, [Required][FromForm]int consumeItemSerial2)
        {
            return Ok(await _service.CombineEquipment(oidAccount, targetItemSerial, consumeItemSerial1, consumeItemSerial2));
        }
    }
}
