using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Orders.DTOs;
using SmartWMS.API.Modules.Orders.Services;

namespace SmartWMS.API.Modules.Orders.Controllers;

[ApiController]
[Route("api/v1/tenant/{tenantId}/customers")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomersService _customersService;

    public CustomersController(ICustomersService customersService)
    {
        _customersService = customersService;
    }

    /// <summary>
    /// Get all customers with optional filters and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<CustomerDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomers(
        Guid tenantId,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? countryCode = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var filters = new CustomerFilters
        {
            Search = search,
            IsActive = isActive,
            CountryCode = countryCode
        };

        var result = await _customersService.GetCustomersAsync(tenantId, filters, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get customer by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomerById(Guid tenantId, Guid id)
    {
        var result = await _customersService.GetCustomerByIdAsync(tenantId, id);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Get customer by code
    /// </summary>
    [HttpGet("code/{code}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomerByCode(Guid tenantId, string code)
    {
        var result = await _customersService.GetCustomerByCodeAsync(tenantId, code);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Create new customer
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCustomer(Guid tenantId, [FromBody] CreateCustomerRequest request)
    {
        var result = await _customersService.CreateCustomerAsync(tenantId, request);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(
            nameof(GetCustomerById),
            new { tenantId, id = result.Data!.Id },
            result);
    }

    /// <summary>
    /// Update customer
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCustomer(Guid tenantId, Guid id, [FromBody] UpdateCustomerRequest request)
    {
        var result = await _customersService.UpdateCustomerAsync(tenantId, id, request);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete customer
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCustomer(Guid tenantId, Guid id)
    {
        var result = await _customersService.DeleteCustomerAsync(tenantId, id);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }
}
