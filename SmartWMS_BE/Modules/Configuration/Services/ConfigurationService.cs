using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Configuration.DTOs;
using SmartWMS.API.Modules.Configuration.Models;

namespace SmartWMS.API.Modules.Configuration.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly ApplicationDbContext _context;

    public ConfigurationService(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Barcode Prefixes

    public async Task<ApiResponse<List<BarcodePrefixDto>>> GetBarcodePrefixesAsync(
        Guid tenantId, BarcodePrefixType? prefixType = null)
    {
        var query = _context.BarcodePrefixes
            .Where(p => p.TenantId == tenantId);

        if (prefixType.HasValue)
            query = query.Where(p => p.PrefixType == prefixType.Value);

        var prefixes = await query
            .OrderBy(p => p.Prefix)
            .Select(p => MapBarcodePrefixToDto(p))
            .ToListAsync();

        return ApiResponse<List<BarcodePrefixDto>>.Ok(prefixes);
    }

    public async Task<ApiResponse<BarcodePrefixDto>> GetBarcodePrefixByIdAsync(Guid tenantId, Guid id)
    {
        var prefix = await _context.BarcodePrefixes
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == id);

        if (prefix == null)
            return ApiResponse<BarcodePrefixDto>.Fail("Barcode prefix not found");

        return ApiResponse<BarcodePrefixDto>.Ok(MapBarcodePrefixToDto(prefix));
    }

    public async Task<ApiResponse<BarcodePrefixDto>> CreateBarcodePrefixAsync(
        Guid tenantId, CreateBarcodePrefixRequest request)
    {
        var exists = await _context.BarcodePrefixes
            .AnyAsync(p => p.TenantId == tenantId && p.Prefix == request.Prefix);

        if (exists)
            return ApiResponse<BarcodePrefixDto>.Fail($"Prefix '{request.Prefix}' already exists");

        var prefix = new BarcodePrefix
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Prefix = request.Prefix,
            Name = request.Name,
            Description = request.Description,
            PrefixType = request.PrefixType,
            MinLength = request.MinLength,
            MaxLength = request.MaxLength,
            Pattern = request.Pattern,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.BarcodePrefixes.Add(prefix);
        await _context.SaveChangesAsync();

        return await GetBarcodePrefixByIdAsync(tenantId, prefix.Id);
    }

    public async Task<ApiResponse<BarcodePrefixDto>> UpdateBarcodePrefixAsync(
        Guid tenantId, Guid id, UpdateBarcodePrefixRequest request)
    {
        var prefix = await _context.BarcodePrefixes
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == id);

        if (prefix == null)
            return ApiResponse<BarcodePrefixDto>.Fail("Barcode prefix not found");

        prefix.Name = request.Name;
        prefix.Description = request.Description;
        prefix.MinLength = request.MinLength;
        prefix.MaxLength = request.MaxLength;
        prefix.Pattern = request.Pattern;
        prefix.IsActive = request.IsActive;
        prefix.Notes = request.Notes;
        prefix.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetBarcodePrefixByIdAsync(tenantId, id);
    }

    public async Task<ApiResponse<bool>> DeleteBarcodePrefixAsync(Guid tenantId, Guid id)
    {
        var prefix = await _context.BarcodePrefixes
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == id);

        if (prefix == null)
            return ApiResponse<bool>.Fail("Barcode prefix not found");

        _context.BarcodePrefixes.Remove(prefix);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Barcode prefix deleted");
    }

    public async Task<ApiResponse<BarcodePrefixType?>> IdentifyBarcodeTypeAsync(Guid tenantId, string barcode)
    {
        var prefixes = await _context.BarcodePrefixes
            .Where(p => p.TenantId == tenantId && p.IsActive)
            .OrderByDescending(p => p.Prefix.Length) // Match longest prefix first
            .ToListAsync();

        foreach (var prefix in prefixes)
        {
            if (!barcode.StartsWith(prefix.Prefix))
                continue;

            // Check length constraints
            if (prefix.MinLength.HasValue && barcode.Length < prefix.MinLength.Value)
                continue;
            if (prefix.MaxLength.HasValue && barcode.Length > prefix.MaxLength.Value)
                continue;

            // Check pattern if specified
            if (!string.IsNullOrEmpty(prefix.Pattern))
            {
                try
                {
                    if (!Regex.IsMatch(barcode, prefix.Pattern))
                        continue;
                }
                catch
                {
                    // Invalid regex, skip pattern check
                }
            }

            return ApiResponse<BarcodePrefixType?>.Ok(prefix.PrefixType);
        }

        return ApiResponse<BarcodePrefixType?>.Ok(null, "No matching prefix found");
    }

    #endregion

    #region Reason Codes

    public async Task<ApiResponse<List<ReasonCodeDto>>> GetReasonCodesAsync(
        Guid tenantId, ReasonCodeType? reasonType = null)
    {
        var query = _context.ReasonCodes
            .Where(r => r.TenantId == tenantId);

        if (reasonType.HasValue)
            query = query.Where(r => r.ReasonType == reasonType.Value);

        var codes = await query
            .OrderBy(r => r.SortOrder)
            .ThenBy(r => r.Name)
            .Select(r => MapReasonCodeToDto(r))
            .ToListAsync();

        return ApiResponse<List<ReasonCodeDto>>.Ok(codes);
    }

    public async Task<ApiResponse<ReasonCodeDto>> GetReasonCodeByIdAsync(Guid tenantId, Guid id)
    {
        var code = await _context.ReasonCodes
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id);

        if (code == null)
            return ApiResponse<ReasonCodeDto>.Fail("Reason code not found");

        return ApiResponse<ReasonCodeDto>.Ok(MapReasonCodeToDto(code));
    }

    public async Task<ApiResponse<ReasonCodeDto>> CreateReasonCodeAsync(
        Guid tenantId, CreateReasonCodeRequest request)
    {
        var exists = await _context.ReasonCodes
            .AnyAsync(r => r.TenantId == tenantId &&
                          r.Code == request.Code &&
                          r.ReasonType == request.ReasonType);

        if (exists)
            return ApiResponse<ReasonCodeDto>.Fail($"Reason code '{request.Code}' already exists for this type");

        var code = new ReasonCode
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            ReasonType = request.ReasonType,
            RequiresNotes = request.RequiresNotes,
            RequiresApproval = request.RequiresApproval,
            AffectsInventory = request.AffectsInventory,
            SortOrder = request.SortOrder,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.ReasonCodes.Add(code);
        await _context.SaveChangesAsync();

        return await GetReasonCodeByIdAsync(tenantId, code.Id);
    }

    public async Task<ApiResponse<ReasonCodeDto>> UpdateReasonCodeAsync(
        Guid tenantId, Guid id, UpdateReasonCodeRequest request)
    {
        var code = await _context.ReasonCodes
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id);

        if (code == null)
            return ApiResponse<ReasonCodeDto>.Fail("Reason code not found");

        code.Name = request.Name;
        code.Description = request.Description;
        code.RequiresNotes = request.RequiresNotes;
        code.RequiresApproval = request.RequiresApproval;
        code.AffectsInventory = request.AffectsInventory;
        code.IsActive = request.IsActive;
        code.SortOrder = request.SortOrder;
        code.Notes = request.Notes;
        code.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetReasonCodeByIdAsync(tenantId, id);
    }

    public async Task<ApiResponse<bool>> DeleteReasonCodeAsync(Guid tenantId, Guid id)
    {
        var code = await _context.ReasonCodes
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id);

        if (code == null)
            return ApiResponse<bool>.Fail("Reason code not found");

        _context.ReasonCodes.Remove(code);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Reason code deleted");
    }

    #endregion

    #region Number Sequences

    public async Task<ApiResponse<List<NumberSequenceDto>>> GetNumberSequencesAsync(Guid tenantId)
    {
        var sequences = await _context.NumberSequences
            .Where(s => s.TenantId == tenantId)
            .OrderBy(s => s.SequenceType)
            .ToListAsync();

        var dtos = sequences.Select(s => MapNumberSequenceToDto(s)).ToList();
        return ApiResponse<List<NumberSequenceDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<NumberSequenceDto>> GetNumberSequenceByTypeAsync(Guid tenantId, string sequenceType)
    {
        var sequence = await _context.NumberSequences
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.SequenceType == sequenceType);

        if (sequence == null)
            return ApiResponse<NumberSequenceDto>.Fail("Number sequence not found");

        return ApiResponse<NumberSequenceDto>.Ok(MapNumberSequenceToDto(sequence));
    }

    public async Task<ApiResponse<NumberSequenceDto>> CreateNumberSequenceAsync(
        Guid tenantId, CreateNumberSequenceRequest request)
    {
        var exists = await _context.NumberSequences
            .AnyAsync(s => s.TenantId == tenantId && s.SequenceType == request.SequenceType);

        if (exists)
            return ApiResponse<NumberSequenceDto>.Fail($"Sequence for '{request.SequenceType}' already exists");

        var sequence = new NumberSequence
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SequenceType = request.SequenceType,
            Prefix = request.Prefix,
            CurrentNumber = request.StartNumber - 1, // Will be incremented on first use
            IncrementBy = request.IncrementBy,
            MinDigits = request.MinDigits,
            Suffix = request.Suffix,
            IncludeDate = request.IncludeDate,
            DateFormat = request.DateFormat,
            ResetFrequency = request.ResetFrequency,
            CreatedAt = DateTime.UtcNow
        };

        _context.NumberSequences.Add(sequence);
        await _context.SaveChangesAsync();

        return await GetNumberSequenceByTypeAsync(tenantId, sequence.SequenceType);
    }

    public async Task<ApiResponse<NumberSequenceDto>> UpdateNumberSequenceAsync(
        Guid tenantId, Guid id, UpdateNumberSequenceRequest request)
    {
        var sequence = await _context.NumberSequences
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == id);

        if (sequence == null)
            return ApiResponse<NumberSequenceDto>.Fail("Number sequence not found");

        sequence.Prefix = request.Prefix;
        sequence.IncrementBy = request.IncrementBy;
        sequence.MinDigits = request.MinDigits;
        sequence.Suffix = request.Suffix;
        sequence.IncludeDate = request.IncludeDate;
        sequence.DateFormat = request.DateFormat;
        sequence.ResetFrequency = request.ResetFrequency;
        sequence.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetNumberSequenceByTypeAsync(tenantId, sequence.SequenceType);
    }

    public async Task<ApiResponse<string>> GetNextNumberAsync(Guid tenantId, string sequenceType)
    {
        var sequence = await _context.NumberSequences
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.SequenceType == sequenceType);

        if (sequence == null)
            return ApiResponse<string>.Fail($"Number sequence for '{sequenceType}' not found");

        // Check if reset is needed
        var now = DateTime.UtcNow;
        var needsReset = sequence.ResetFrequency switch
        {
            ResetFrequency.Daily => sequence.LastResetDate?.Date != now.Date,
            ResetFrequency.Weekly => GetWeekOfYear(sequence.LastResetDate) != GetWeekOfYear(now),
            ResetFrequency.Monthly => sequence.LastResetDate?.Month != now.Month ||
                                     sequence.LastResetDate?.Year != now.Year,
            ResetFrequency.Yearly => sequence.LastResetDate?.Year != now.Year,
            _ => false
        };

        if (needsReset)
        {
            sequence.CurrentNumber = 0;
            sequence.LastResetDate = now;
        }

        // Increment
        sequence.CurrentNumber += sequence.IncrementBy;
        sequence.UpdatedAt = now;

        await _context.SaveChangesAsync();

        // Format number
        var numberPart = sequence.CurrentNumber.ToString().PadLeft(sequence.MinDigits, '0');
        var datePart = sequence.IncludeDate ? now.ToString(sequence.DateFormat) : "";

        var result = $"{sequence.Prefix}{datePart}{numberPart}{sequence.Suffix ?? ""}";
        return ApiResponse<string>.Ok(result);
    }

    private static int GetWeekOfYear(DateTime? date)
    {
        if (!date.HasValue) return 0;
        var cal = System.Globalization.CultureInfo.InvariantCulture.Calendar;
        return cal.GetWeekOfYear(date.Value, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
    }

    #endregion

    #region System Settings

    public async Task<ApiResponse<List<SystemSettingDto>>> GetSystemSettingsAsync(
        Guid tenantId, string? category = null)
    {
        var query = _context.SystemSettings
            .Where(s => s.TenantId == tenantId);

        if (!string.IsNullOrEmpty(category))
            query = query.Where(s => s.Category == category);

        var settings = await query
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Key)
            .Select(s => MapSystemSettingToDto(s))
            .ToListAsync();

        return ApiResponse<List<SystemSettingDto>>.Ok(settings);
    }

    public async Task<ApiResponse<SystemSettingDto>> GetSystemSettingAsync(
        Guid tenantId, string category, string key)
    {
        var setting = await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.TenantId == tenantId &&
                                     s.Category == category &&
                                     s.Key == key);

        if (setting == null)
            return ApiResponse<SystemSettingDto>.Fail("Setting not found");

        return ApiResponse<SystemSettingDto>.Ok(MapSystemSettingToDto(setting));
    }

    public async Task<ApiResponse<SystemSettingDto>> CreateSystemSettingAsync(
        Guid tenantId, CreateSystemSettingRequest request)
    {
        var exists = await _context.SystemSettings
            .AnyAsync(s => s.TenantId == tenantId &&
                          s.Category == request.Category &&
                          s.Key == request.Key);

        if (exists)
            return ApiResponse<SystemSettingDto>.Fail($"Setting '{request.Category}.{request.Key}' already exists");

        var setting = new SystemSetting
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Category = request.Category,
            Key = request.Key,
            Value = request.Value,
            Description = request.Description,
            ValueType = request.ValueType,
            CreatedAt = DateTime.UtcNow
        };

        _context.SystemSettings.Add(setting);
        await _context.SaveChangesAsync();

        return await GetSystemSettingAsync(tenantId, setting.Category, setting.Key);
    }

    public async Task<ApiResponse<SystemSettingDto>> UpdateSystemSettingAsync(
        Guid tenantId, Guid id, UpdateSystemSettingRequest request)
    {
        var setting = await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == id);

        if (setting == null)
            return ApiResponse<SystemSettingDto>.Fail("Setting not found");

        setting.Value = request.Value;
        setting.Description = request.Description;
        setting.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetSystemSettingAsync(tenantId, setting.Category, setting.Key);
    }

    public async Task<ApiResponse<bool>> DeleteSystemSettingAsync(Guid tenantId, Guid id)
    {
        var setting = await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == id);

        if (setting == null)
            return ApiResponse<bool>.Fail("Setting not found");

        if (setting.IsSystem)
            return ApiResponse<bool>.Fail("Cannot delete system setting");

        _context.SystemSettings.Remove(setting);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Setting deleted");
    }

    #endregion

    #region Mappers

    private static BarcodePrefixDto MapBarcodePrefixToDto(BarcodePrefix prefix)
    {
        return new BarcodePrefixDto
        {
            Id = prefix.Id,
            Prefix = prefix.Prefix,
            Name = prefix.Name,
            Description = prefix.Description,
            PrefixType = prefix.PrefixType,
            MinLength = prefix.MinLength,
            MaxLength = prefix.MaxLength,
            Pattern = prefix.Pattern,
            IsActive = prefix.IsActive,
            Notes = prefix.Notes,
            CreatedAt = prefix.CreatedAt,
            UpdatedAt = prefix.UpdatedAt
        };
    }

    private static ReasonCodeDto MapReasonCodeToDto(ReasonCode code)
    {
        return new ReasonCodeDto
        {
            Id = code.Id,
            Code = code.Code,
            Name = code.Name,
            Description = code.Description,
            ReasonType = code.ReasonType,
            RequiresNotes = code.RequiresNotes,
            RequiresApproval = code.RequiresApproval,
            AffectsInventory = code.AffectsInventory,
            IsActive = code.IsActive,
            SortOrder = code.SortOrder,
            Notes = code.Notes,
            CreatedAt = code.CreatedAt,
            UpdatedAt = code.UpdatedAt
        };
    }

    private static NumberSequenceDto MapNumberSequenceToDto(NumberSequence sequence)
    {
        var now = DateTime.UtcNow;
        var nextNumber = sequence.CurrentNumber + sequence.IncrementBy;
        var numberPart = nextNumber.ToString().PadLeft(sequence.MinDigits, '0');
        var datePart = sequence.IncludeDate ? now.ToString(sequence.DateFormat) : "";

        return new NumberSequenceDto
        {
            Id = sequence.Id,
            SequenceType = sequence.SequenceType,
            Prefix = sequence.Prefix,
            CurrentNumber = sequence.CurrentNumber,
            IncrementBy = sequence.IncrementBy,
            MinDigits = sequence.MinDigits,
            Suffix = sequence.Suffix,
            IncludeDate = sequence.IncludeDate,
            DateFormat = sequence.DateFormat,
            LastResetDate = sequence.LastResetDate,
            ResetFrequency = sequence.ResetFrequency,
            NextNumber = $"{sequence.Prefix}{datePart}{numberPart}{sequence.Suffix ?? ""}",
            CreatedAt = sequence.CreatedAt,
            UpdatedAt = sequence.UpdatedAt
        };
    }

    private static SystemSettingDto MapSystemSettingToDto(SystemSetting setting)
    {
        return new SystemSettingDto
        {
            Id = setting.Id,
            Category = setting.Category,
            Key = setting.Key,
            Value = setting.Value,
            Description = setting.Description,
            ValueType = setting.ValueType,
            IsSystem = setting.IsSystem,
            CreatedAt = setting.CreatedAt,
            UpdatedAt = setting.UpdatedAt
        };
    }

    #endregion
}
