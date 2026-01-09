using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.API.Modules.Configuration.DTOs;
using SmartWMS.API.Modules.Configuration.Models;
using SmartWMS.API.Modules.Configuration.Services;

namespace SmartWMS.API.Modules.Configuration.Controllers;

[ApiController]
[Route("api/v1/tenant/{tenantId}/configuration")]
[Authorize]
public class ConfigurationController : ControllerBase
{
    private readonly IConfigurationService _configService;

    public ConfigurationController(IConfigurationService configService)
    {
        _configService = configService;
    }

    #region Barcode Prefixes

    [HttpGet("barcode-prefixes")]
    public async Task<IActionResult> GetBarcodePrefixes(
        Guid tenantId, [FromQuery] BarcodePrefixType? prefixType = null)
    {
        var result = await _configService.GetBarcodePrefixesAsync(tenantId, prefixType);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("barcode-prefixes/{id}")]
    public async Task<IActionResult> GetBarcodePrefixById(Guid tenantId, Guid id)
    {
        var result = await _configService.GetBarcodePrefixByIdAsync(tenantId, id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("barcode-prefixes")]
    public async Task<IActionResult> CreateBarcodePrefix(
        Guid tenantId, [FromBody] CreateBarcodePrefixRequest request)
    {
        var result = await _configService.CreateBarcodePrefixAsync(tenantId, request);
        return result.Success
            ? CreatedAtAction(nameof(GetBarcodePrefixById), new { tenantId, id = result.Data!.Id }, result)
            : BadRequest(result);
    }

    [HttpPut("barcode-prefixes/{id}")]
    public async Task<IActionResult> UpdateBarcodePrefix(
        Guid tenantId, Guid id, [FromBody] UpdateBarcodePrefixRequest request)
    {
        var result = await _configService.UpdateBarcodePrefixAsync(tenantId, id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("barcode-prefixes/{id}")]
    public async Task<IActionResult> DeleteBarcodePrefix(Guid tenantId, Guid id)
    {
        var result = await _configService.DeleteBarcodePrefixAsync(tenantId, id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("barcode-prefixes/identify")]
    public async Task<IActionResult> IdentifyBarcode(Guid tenantId, [FromBody] string barcode)
    {
        var result = await _configService.IdentifyBarcodeTypeAsync(tenantId, barcode);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Reason Codes

    [HttpGet("reason-codes")]
    public async Task<IActionResult> GetReasonCodes(
        Guid tenantId, [FromQuery] ReasonCodeType? reasonType = null)
    {
        var result = await _configService.GetReasonCodesAsync(tenantId, reasonType);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("reason-codes/{id}")]
    public async Task<IActionResult> GetReasonCodeById(Guid tenantId, Guid id)
    {
        var result = await _configService.GetReasonCodeByIdAsync(tenantId, id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("reason-codes")]
    public async Task<IActionResult> CreateReasonCode(
        Guid tenantId, [FromBody] CreateReasonCodeRequest request)
    {
        var result = await _configService.CreateReasonCodeAsync(tenantId, request);
        return result.Success
            ? CreatedAtAction(nameof(GetReasonCodeById), new { tenantId, id = result.Data!.Id }, result)
            : BadRequest(result);
    }

    [HttpPut("reason-codes/{id}")]
    public async Task<IActionResult> UpdateReasonCode(
        Guid tenantId, Guid id, [FromBody] UpdateReasonCodeRequest request)
    {
        var result = await _configService.UpdateReasonCodeAsync(tenantId, id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("reason-codes/{id}")]
    public async Task<IActionResult> DeleteReasonCode(Guid tenantId, Guid id)
    {
        var result = await _configService.DeleteReasonCodeAsync(tenantId, id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Number Sequences

    [HttpGet("number-sequences")]
    public async Task<IActionResult> GetNumberSequences(Guid tenantId)
    {
        var result = await _configService.GetNumberSequencesAsync(tenantId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("number-sequences/{sequenceType}")]
    public async Task<IActionResult> GetNumberSequenceByType(Guid tenantId, string sequenceType)
    {
        var result = await _configService.GetNumberSequenceByTypeAsync(tenantId, sequenceType);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("number-sequences")]
    public async Task<IActionResult> CreateNumberSequence(
        Guid tenantId, [FromBody] CreateNumberSequenceRequest request)
    {
        var result = await _configService.CreateNumberSequenceAsync(tenantId, request);
        return result.Success
            ? CreatedAtAction(nameof(GetNumberSequenceByType), new { tenantId, sequenceType = result.Data!.SequenceType }, result)
            : BadRequest(result);
    }

    [HttpPut("number-sequences/{id}")]
    public async Task<IActionResult> UpdateNumberSequence(
        Guid tenantId, Guid id, [FromBody] UpdateNumberSequenceRequest request)
    {
        var result = await _configService.UpdateNumberSequenceAsync(tenantId, id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("number-sequences/{sequenceType}/next")]
    public async Task<IActionResult> GetNextNumber(Guid tenantId, string sequenceType)
    {
        var result = await _configService.GetNextNumberAsync(tenantId, sequenceType);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region System Settings

    [HttpGet("settings")]
    public async Task<IActionResult> GetSystemSettings(Guid tenantId, [FromQuery] string? category = null)
    {
        var result = await _configService.GetSystemSettingsAsync(tenantId, category);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("settings/{category}/{key}")]
    public async Task<IActionResult> GetSystemSetting(Guid tenantId, string category, string key)
    {
        var result = await _configService.GetSystemSettingAsync(tenantId, category, key);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("settings")]
    public async Task<IActionResult> CreateSystemSetting(
        Guid tenantId, [FromBody] CreateSystemSettingRequest request)
    {
        var result = await _configService.CreateSystemSettingAsync(tenantId, request);
        return result.Success
            ? CreatedAtAction(nameof(GetSystemSetting), new { tenantId, category = result.Data!.Category, key = result.Data.Key }, result)
            : BadRequest(result);
    }

    [HttpPut("settings/{id}")]
    public async Task<IActionResult> UpdateSystemSetting(
        Guid tenantId, Guid id, [FromBody] UpdateSystemSettingRequest request)
    {
        var result = await _configService.UpdateSystemSettingAsync(tenantId, id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("settings/{id}")]
    public async Task<IActionResult> DeleteSystemSetting(Guid tenantId, Guid id)
    {
        var result = await _configService.DeleteSystemSettingAsync(tenantId, id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion
}
