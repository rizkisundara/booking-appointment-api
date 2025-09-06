using Microsoft.AspNetCore.Mvc;
using ReaderServices;
using Entities.Response;
using System;
using System.Threading.Tasks;
using System.Globalization;
using Entities.Request;
using WriterServices;
using Microsoft.Extensions.Logging;

namespace take_home_assessment_api.Controllers
{
    [ApiController]
    [Route("api/appointment")]
    public class AppointmentController : ControllerBase
    {
        private readonly IReaderService _reader;
        private readonly IWriterService _writer;
        private readonly ILogger<AppointmentController> _logger;

        public AppointmentController(IReaderService reader, IWriterService writer, ILogger<AppointmentController> logger)
        {
            _reader = reader;
            _writer = writer;
            _logger = logger;
        }

        [HttpGet("agency/{agencyId}/date/{date}")]
        [ProducesResponseType(typeof(GlobalResponse<IReadOnlyList<GetAppointmentsByDateResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GlobalResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GlobalResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAppointmentsByDate(int agencyId, string date)
        {
            try
            {
                if (agencyId <= 0)
                {
                    _logger.LogWarning("[GetAppointmentsByDate] Invalid AgencyId={AgencyId}", agencyId);
                    return BadRequest(GlobalResponse<string>.Failed("Invalid Agency ID"));
                }

                if (!DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                {
                    _logger.LogWarning("[GetAppointmentsByDate] Invalid date format: {DateRaw}", date);
                    return BadRequest(GlobalResponse<string>.Failed("Invalid date format. Use yyyy-MM-dd."));
                }

                _logger.LogInformation("[GetAppointmentsByDate] Fetching appointments AgencyId={AgencyId}, Date={Date}", agencyId, parsedDate.ToString("yyyy-MM-dd"));

                var appointments = await _reader.GetAppointmentsByDateAsync(agencyId, parsedDate);

                if (appointments == null || !appointments.Any())
                {
                    _logger.LogInformation("[GetAppointmentsByDate] Not found AgencyId={AgencyId}, Date={Date}", agencyId, parsedDate.ToString("yyyy-MM-dd"));
                    return NotFound(GlobalResponse<string>.NotFound());
                }

                return Ok(GlobalResponse<IReadOnlyList<GetAppointmentsByDateResponse>>.Success(appointments, "Success"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetAppointmentsByDate] Unexpected error AgencyId={AgencyId}, DateRaw={DateRaw}", agencyId, date);
                return StatusCode(500, GlobalResponse<string>.InternalError("Unexpected error while fetching appointments"));
            }
        }

        [HttpGet("date/{date}")]
        [ProducesResponseType(typeof(GlobalResponse<IReadOnlyList<GetAllAppointmentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GlobalResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GlobalResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllAppointmentsByDate(string date)
        {
            try
            {
                if (!DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                {
                    _logger.LogWarning("[GetAllAppointmentsByDate] Invalid date format: {DateRaw}", date);
                    return BadRequest(GlobalResponse<string>.Failed("Invalid date format. Use yyyy-MM-dd."));
                }

                _logger.LogInformation("[GetAllAppointmentsByDate] Fetching all appointments for Date={Date}", parsedDate.ToString("yyyy-MM-dd"));

                var appointments = await _reader.GetAllAppointmentsByDateAsync(parsedDate);

                if (appointments == null || !appointments.Any())
                {
                    _logger.LogInformation("[GetAllAppointmentsByDate] Not found for Date={Date}", parsedDate.ToString("yyyy-MM-dd"));
                    return NotFound(GlobalResponse<string>.NotFound());
                }

                return Ok(GlobalResponse<IReadOnlyList<GetAllAppointmentResponse>>.Success(appointments, "Success"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetAllAppointmentsByDate] Unexpected error DateRaw={DateRaw}", date);
                return StatusCode(500, GlobalResponse<string>.InternalError("Unexpected error while fetching appointments"));
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(GlobalResponse<PostAppointmentResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(GlobalResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GlobalResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateAppointment([FromBody] PostAppointmentRequest request)
        {
            try
            {
                if (request.AgencyId <= 0)
                {
                    _logger.LogWarning("[CreateAppointment] Invalid AgencyId={AgencyId}", request.AgencyId);
                    return BadRequest(GlobalResponse<string>.Failed("Invalid Agency ID"));
                }

                if (request.CustomerId <= 0)
                {
                    _logger.LogWarning("[CreateAppointment] Invalid CustomerId={CustomerId}", request.CustomerId);
                    return BadRequest(GlobalResponse<string>.Failed("Invalid Customer ID"));
                }

                if (request.DesiredDate < DateTime.Today)
                {
                    _logger.LogWarning("[CreateAppointment] Past date requested: {DesiredDate}", request.DesiredDate);
                    return BadRequest(GlobalResponse<string>.Failed("Cannot book appointment for past dates"));
                }

                _logger.LogInformation("[CreateAppointment] Creating appointment AgencyId={AgencyId}, CustomerId={CustomerId}, Date={Date}",
                    request.AgencyId, request.CustomerId, request.DesiredDate.ToString("yyyy-MM-dd"));

                var result = await _writer.PostAppointmentAsync(request);

                return StatusCode(201, GlobalResponse<PostAppointmentResponse>.Success(result, "Appointment created successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "[CreateAppointment] Agency or customer not found");
                return NotFound(GlobalResponse<string>.NotFound(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CreateAppointment] Unexpected error");
                return StatusCode(500, GlobalResponse<string>.InternalError("Unexpected error while creating appointment"));
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(GlobalResponse<PostAppointmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GlobalResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GlobalResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GlobalResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAppointment(int id, [FromBody] UpdateAppointmentRequest request)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("[UpdateAppointment] Invalid AppointmentId={AppointmentId}", id);
                    return BadRequest(GlobalResponse<string>.Failed("Invalid Appointment ID"));
                }

                if (request.AppointmentDate.HasValue && request.AppointmentDate.Value < DateTime.Today)
                {
                    _logger.LogWarning("[UpdateAppointment] Past date requested: {AppointmentDate}", request.AppointmentDate.Value);
                    return BadRequest(GlobalResponse<string>.Failed("Cannot update appointment to a past date"));
                }

                _logger.LogInformation("[UpdateAppointment] Updating appointment with Id={AppointmentId}", id);

                var result = await _writer.UpdateAppointmentAsync(id, request);

                return Ok(GlobalResponse<PostAppointmentResponse>.Success(result, "Appointment updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "[UpdateAppointment] Appointment not found. Id={AppointmentId}", id);
                return NotFound(GlobalResponse<string>.NotFound(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdateAppointment] Unexpected error");
                return StatusCode(500, GlobalResponse<string>.InternalError("Unexpected error while updating appointment"));
            }
        }

        [HttpPatch("{id}/cancel")]
        [ProducesResponseType(typeof(GlobalResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GlobalResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GlobalResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GlobalResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("[CancelAppointment] Invalid AppointmentId={AppointmentId}", id);
                    return BadRequest(GlobalResponse<string>.Failed("Invalid Appointment ID"));
                }

                _logger.LogInformation("[CancelAppointment] Cancelling appointment with Id={AppointmentId}", id);

                var result = await _writer.UpdateAppointmentAsync(id, new UpdateAppointmentRequest
                {
                    Status = "Cancelled"
                });

                return Ok(GlobalResponse<string>.Success("Appointment cancelled successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "[CancelAppointment] Appointment not found. Id={AppointmentId}", id);
                return NotFound(GlobalResponse<string>.NotFound(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CancelAppointment] Unexpected error");
                return StatusCode(500, GlobalResponse<string>.InternalError("Unexpected error while cancelling appointment"));
            }
        }
    }
}
