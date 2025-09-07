using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Entities.Request;
using Entities.Response;
using ReaderServices;
using WriterServices;

namespace BookingAppointmentAPI.Controllers
{
    [ApiController]
    [Route("api/agency-settings")]
    public class AgencySettingsController : ControllerBase
    {
        private readonly IReaderService _reader;
        private readonly IWriterService _writer;
        private readonly ILogger<AgencySettingsController> _logger;

        public AgencySettingsController(IReaderService reader, IWriterService writer, ILogger<AgencySettingsController> logger)
        {
            _reader = reader;
            _writer = writer;
            _logger = logger;
        }

        [HttpPut("{agencyId}")]
        [ProducesResponseType(typeof(GlobalResponse<AgencySettingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GlobalResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GlobalResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GlobalResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAgencySettings(int agencyId, [FromBody] UpdateAgencySettingsRequest request)
        {
            try
            {
                if (agencyId <= 0)
                {
                    _logger.LogWarning("[UpdateAgencySettings] Invalid AgencyId={AgencyId}", agencyId);
                    return BadRequest(GlobalResponse<string>.Failed("Invalid Agency ID"));
                }

                if (request.MaxAppointments <= 0)
                {
                    _logger.LogWarning("[UpdateAgencySettings] Invalid MaxAppointments={MaxAppointments}", request.MaxAppointments);
                    return BadRequest(GlobalResponse<string>.Failed("Max appointments must be greater than 0"));
                }

                _logger.LogInformation("[UpdateAgencySettings] Updating settings for AgencyId={AgencyId}", agencyId);

                var result = await _writer.UpdateAgencySettingsAsync(agencyId, request);

                return Ok(GlobalResponse<AgencySettingResponse>.Success(result, "Agency settings updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "[UpdateAgencySettings] Agency not found for AgencyId={AgencyId}", agencyId);
                return NotFound(GlobalResponse<string>.NotFound($"Agency with ID {agencyId} not found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdateAgencySettings] Unexpected error AgencyId={AgencyId}", agencyId);
                return StatusCode(500, GlobalResponse<string>.InternalError("Unexpected error while updating agency settings"));
            }
        }

        [HttpGet("{agencyId}")]
        [ProducesResponseType(typeof(GlobalResponse<AgencySettingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GlobalResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GlobalResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAgencySettings(int agencyId)
        {
            try
            {
                if (agencyId <= 0)
                {
                    _logger.LogWarning("[GetAgencySettings] Invalid AgencyId={AgencyId}", agencyId);
                    return BadRequest(GlobalResponse<string>.Failed("Invalid Agency ID"));
                }

                _logger.LogInformation("[GetAgencySettings] Fetching settings for AgencyId={AgencyId}", agencyId);

                var settings = await _reader.GetAgencySettingAsync(agencyId);

                if (settings == null)
                {
                    _logger.LogInformation("[GetAgencySettings] No settings found for AgencyId={AgencyId}", agencyId);
                    return NotFound(GlobalResponse<string>.NotFound());
                }

                var response = new AgencySettingResponse
                {
                    AgencyId = settings.AgencyId,
                    MaxAppointments = settings.MaxAppointments
                };

                return Ok(GlobalResponse<AgencySettingResponse>.Success(response, "Success"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetAgencySettings] Unexpected error AgencyId={AgencyId}", agencyId);
                return StatusCode(500, GlobalResponse<string>.InternalError("Unexpected error while fetching agency settings"));
            }
        }
    }
}