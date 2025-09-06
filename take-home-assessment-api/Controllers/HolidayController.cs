using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Entities.Request;
using Entities.Response;
using ReaderServices;
using WriterServices;

namespace take_home_assessment_api.Controllers
{
    [ApiController]
    [Route("api/holiday")]
    public class HolidayController : ControllerBase
    {
        private readonly IReaderService _reader;
        private readonly IWriterService _writer;
        private readonly ILogger<HolidayController> _logger;

        public HolidayController(IReaderService reader, IWriterService writer, ILogger<HolidayController> logger)
        {
            _reader = reader;
            _writer = writer;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(GlobalResponse<HolidayResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(GlobalResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GlobalResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GlobalResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateHoliday([FromBody] CreateHolidayRequest request)
        {
            try
            {
                if (request.AgencyId <= 0)
                {
                    _logger.LogWarning("[CreateHoliday] Invalid AgencyId={AgencyId}", request.AgencyId);
                    return BadRequest(GlobalResponse<string>.Failed("Invalid Agency ID"));
                }

                if (request.OffDate < DateTime.Today)
                {
                    _logger.LogWarning("[CreateHoliday] Past date requested: {OffDate}", request.OffDate);
                    return BadRequest(GlobalResponse<string>.Failed("Cannot create holiday for past dates"));
                }

                _logger.LogInformation("[CreateHoliday] Creating holiday for AgencyId={AgencyId}, Date={Date}",
                    request.AgencyId, request.OffDate.ToString("yyyy-MM-dd"));

                var result = await _writer.CreateHolidayAsync(request);

                return StatusCode(201, GlobalResponse<HolidayResponse>.Success(result, "Holiday created successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "[CreateHoliday] Agency not found for AgencyId={AgencyId}", request.AgencyId);
                return NotFound(GlobalResponse<string>.NotFound($"Agency with ID {request.AgencyId} not found"));
            }
            catch (Exception ex) when (ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(ex, "[CreateHoliday] Holiday already exists for AgencyId={AgencyId}, Date={Date}",
                    request.AgencyId, request.OffDate.ToString("yyyy-MM-dd"));
                return Conflict(GlobalResponse<string>.Failed(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CreateHoliday] Unexpected error");
                return StatusCode(500, GlobalResponse<string>.InternalError("Unexpected error while creating holiday"));
            }
        }

        [HttpGet("agency/{agencyId}")]
        [ProducesResponseType(typeof(GlobalResponse<IReadOnlyList<HolidayResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GlobalResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GlobalResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetHolidays(int agencyId)
        {
            try
            {
                if (agencyId <= 0)
                {
                    _logger.LogWarning("[GetHolidays] Invalid AgencyId={AgencyId}", agencyId);
                    return BadRequest(GlobalResponse<string>.Failed("Invalid Agency ID"));
                }

                _logger.LogInformation("[GetHolidays] Fetching holidays for AgencyId={AgencyId}", agencyId);

                var holidays = await _reader.GetHolidaysAsync(agencyId);

                if (holidays == null || !holidays.Any())
                {
                    _logger.LogInformation("[GetHolidays] No holidays found for AgencyId={AgencyId}", agencyId);
                    return NotFound(GlobalResponse<string>.NotFound());
                }

                return Ok(GlobalResponse<IReadOnlyList<HolidayResponse>>.Success(holidays, "Success"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetHolidays] Unexpected error AgencyId={AgencyId}", agencyId);
                return StatusCode(500, GlobalResponse<string>.InternalError("Unexpected error while fetching holidays"));
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(GlobalResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GlobalResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GlobalResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteHoliday(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("[DeleteHoliday] Invalid HolidayId={HolidayId}", id);
                    return BadRequest(GlobalResponse<string>.Failed("Invalid Holiday ID"));
                }

                _logger.LogInformation("[DeleteHoliday] Deleting holiday with Id={HolidayId}", id);

                var result = await _writer.DeleteHolidayAsync(id);

                if (result)
                {
                    return Ok(GlobalResponse<string>.Success("Holiday deleted successfully"));
                }
                else
                {
                    return NotFound(GlobalResponse<string>.NotFound());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteHoliday] Unexpected error HolidayId={HolidayId}", id);
                return StatusCode(500, GlobalResponse<string>.InternalError("Unexpected error while deleting holiday"));
            }
        }
    }
}