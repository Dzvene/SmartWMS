using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Enums;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.CycleCount.Models;
using SmartWMS.API.Modules.Fulfillment.Models;
using SmartWMS.API.Modules.OperationHub.DTOs;
using SmartWMS.API.Modules.OperationHub.Models;
using SmartWMS.API.Modules.Packing.Models;
using SmartWMS.API.Modules.Putaway.Models;

namespace SmartWMS.API.Modules.OperationHub.Services;

public class OperationHubService : IOperationHubService
{
    private readonly ApplicationDbContext _context;

    public OperationHubService(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Session Management

    public async Task<ApiResponse<OperatorSessionDto>> StartSessionAsync(Guid tenantId, Guid userId, StartSessionRequest request)
    {
        // Check for existing active session
        var existingSession = await _context.Set<OperatorSession>()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId
                && s.UserId == userId
                && s.Status == OperatorSessionStatus.Active);

        if (existingSession != null)
        {
            // End the existing session
            existingSession.Status = OperatorSessionStatus.Ended;
            existingSession.EndedAt = DateTime.UtcNow;
        }

        var session = new OperatorSession
        {
            TenantId = tenantId,
            UserId = userId,
            WarehouseId = request.WarehouseId,
            DeviceId = request.DeviceId,
            DeviceType = request.DeviceType,
            DeviceName = request.DeviceName,
            ShiftCode = request.ShiftCode,
            Status = OperatorSessionStatus.Active,
            StartedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow
        };

        _context.Set<OperatorSession>().Add(session);
        await _context.SaveChangesAsync();

        return ApiResponse<OperatorSessionDto>.Ok(await MapSessionToDtoAsync(session));
    }

    public async Task<ApiResponse<OperatorSessionDto>> GetCurrentSessionAsync(Guid tenantId, Guid userId)
    {
        var session = await _context.Set<OperatorSession>()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId
                && s.UserId == userId
                && s.Status == OperatorSessionStatus.Active);

        if (session == null)
            return ApiResponse<OperatorSessionDto>.Fail("No active session found");

        return ApiResponse<OperatorSessionDto>.Ok(await MapSessionToDtoAsync(session));
    }

    public async Task<ApiResponse<OperatorSessionDto>> UpdateSessionStatusAsync(Guid tenantId, Guid sessionId, UpdateSessionStatusRequest request)
    {
        var session = await _context.Set<OperatorSession>()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == sessionId);

        if (session == null)
            return ApiResponse<OperatorSessionDto>.Fail("Session not found");

        session.Status = request.Status;
        session.CurrentZone = request.CurrentZone ?? session.CurrentZone;
        session.CurrentLocation = request.CurrentLocation ?? session.CurrentLocation;
        session.LastActivityAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<OperatorSessionDto>.Ok(await MapSessionToDtoAsync(session));
    }

    public async Task<ApiResponse<OperatorSessionDto>> EndSessionAsync(Guid tenantId, Guid sessionId)
    {
        var session = await _context.Set<OperatorSession>()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == sessionId);

        if (session == null)
            return ApiResponse<OperatorSessionDto>.Fail("Session not found");

        session.Status = OperatorSessionStatus.Ended;
        session.EndedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<OperatorSessionDto>.Ok(await MapSessionToDtoAsync(session));
    }

    public async Task<ApiResponse<PaginatedResult<OperatorSessionDto>>> GetActiveSessionsAsync(Guid tenantId, Guid? warehouseId = null)
    {
        var query = _context.Set<OperatorSession>()
            .Where(s => s.TenantId == tenantId && s.Status == OperatorSessionStatus.Active);

        if (warehouseId.HasValue)
            query = query.Where(s => s.WarehouseId == warehouseId.Value);

        var sessions = await query.OrderByDescending(s => s.StartedAt).ToListAsync();
        var dtos = new List<OperatorSessionDto>();
        foreach (var session in sessions)
        {
            dtos.Add(await MapSessionToDtoAsync(session));
        }

        return ApiResponse<PaginatedResult<OperatorSessionDto>>.Ok(new PaginatedResult<OperatorSessionDto>
        {
            Items = dtos,
            TotalCount = dtos.Count,
            Page = 1,
            PageSize = dtos.Count
        });
    }

    #endregion

    #region Unified Task Queue

    public async Task<ApiResponse<PaginatedResult<UnifiedTaskDto>>> GetTaskQueueAsync(Guid tenantId, TaskQueueQueryParams query)
    {
        var tasks = new List<UnifiedTaskDto>();

        var taskType = query.TaskType?.ToLower();
        var includeAll = string.IsNullOrEmpty(taskType) || taskType == "all";

        // Pick Tasks
        if (includeAll || taskType == "pick")
        {
            var pickTasks = await GetPickTasksAsync(tenantId, query);
            tasks.AddRange(pickTasks);
        }

        // Putaway Tasks
        if (includeAll || taskType == "putaway")
        {
            var putawayTasks = await GetPutawayTasksAsync(tenantId, query);
            tasks.AddRange(putawayTasks);
        }

        // Packing Tasks
        if (includeAll || taskType == "pack")
        {
            var packTasks = await GetPackingTasksAsync(tenantId, query);
            tasks.AddRange(packTasks);
        }

        // Cycle Count Tasks
        if (includeAll || taskType == "cyclecount")
        {
            var cycleTasks = await GetCycleCountTasksAsync(tenantId, query);
            tasks.AddRange(cycleTasks);
        }

        // Sort
        tasks = query.SortBy?.ToLower() switch
        {
            "priority" => tasks.OrderBy(t => t.Priority).ThenBy(t => t.CreatedAt).ToList(),
            "dueby" => tasks.OrderBy(t => t.DueBy ?? DateTime.MaxValue).ThenBy(t => t.Priority).ToList(),
            _ => tasks.OrderBy(t => t.Priority).ThenBy(t => t.CreatedAt).ToList()
        };

        // Paginate
        var totalCount = tasks.Count;
        var items = tasks
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        return ApiResponse<PaginatedResult<UnifiedTaskDto>>.Ok(new PaginatedResult<UnifiedTaskDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }

    public async Task<ApiResponse<UnifiedTaskDto>> GetTaskByIdAsync(Guid tenantId, string taskType, Guid taskId)
    {
        UnifiedTaskDto? dto = null;

        switch (taskType.ToLower())
        {
            case "pick":
                var pickTask = await _context.Set<PickTask>()
                    .Include(t => t.Product)
                    .Include(t => t.FromLocation)
                    .Include(t => t.Order)
                    .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == taskId);
                if (pickTask != null) dto = MapPickTaskToUnified(pickTask);
                break;

            case "putaway":
                var putawayTask = await _context.Set<PutawayTask>()
                    .Include(t => t.Product)
                    .Include(t => t.FromLocation)
                    .Include(t => t.SuggestedLocation)
                    .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == taskId);
                if (putawayTask != null) dto = MapPutawayTaskToUnified(putawayTask);
                break;

            case "pack":
                var packTask = await _context.Set<PackingTask>()
                    .Include(t => t.SalesOrder)
                    .Include(t => t.PackingStation)
                    .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == taskId);
                if (packTask != null) dto = MapPackingTaskToUnified(packTask);
                break;

            case "cyclecount":
                var cycleCount = await _context.Set<CycleCountSession>()
                    .Include(t => t.Warehouse)
                    .Include(t => t.Zone)
                    .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == taskId);
                if (cycleCount != null) dto = MapCycleCountToUnified(cycleCount);
                break;
        }

        if (dto == null)
            return ApiResponse<UnifiedTaskDto>.Fail("Task not found");

        return ApiResponse<UnifiedTaskDto>.Ok(dto);
    }

    public async Task<ApiResponse<UnifiedTaskDto>> GetNextTaskAsync(Guid tenantId, Guid userId, Guid warehouseId, string? preferredTaskType = null)
    {
        var query = new TaskQueueQueryParams
        {
            WarehouseId = warehouseId,
            TaskType = preferredTaskType,
            Status = "Pending",
            UnassignedOnly = true,
            SortBy = "Priority",
            Page = 1,
            PageSize = 1
        };

        var result = await GetTaskQueueAsync(tenantId, query);

        if (!result.Success || result.Data == null || !result.Data.Items.Any())
            return ApiResponse<UnifiedTaskDto>.Fail("No tasks available");

        return ApiResponse<UnifiedTaskDto>.Ok(result.Data.Items.First());
    }

    public async Task<ApiResponse<UnifiedTaskDto>> AssignTaskAsync(Guid tenantId, AssignTaskRequest request)
    {
        var taskType = request.TaskType.ToLower();

        switch (taskType)
        {
            case "pick":
                var pickTask = await _context.Set<PickTask>()
                    .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == request.TaskId);
                if (pickTask == null) return ApiResponse<UnifiedTaskDto>.Fail("Pick task not found");
                pickTask.AssignedToUserId = request.UserId;
                pickTask.Status = PickTaskStatus.Assigned;
                await LogTaskAction(tenantId, request.UserId, "Pick", request.TaskId, pickTask.TaskNumber, TaskAction.Assigned);
                break;

            case "putaway":
                var putawayTask = await _context.Set<PutawayTask>()
                    .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == request.TaskId);
                if (putawayTask == null) return ApiResponse<UnifiedTaskDto>.Fail("Putaway task not found");
                putawayTask.AssignedToUserId = request.UserId;
                putawayTask.AssignedAt = DateTime.UtcNow;
                putawayTask.Status = PutawayTaskStatus.Assigned;
                await LogTaskAction(tenantId, request.UserId, "Putaway", request.TaskId, putawayTask.TaskNumber, TaskAction.Assigned);
                break;

            case "pack":
                var packTask = await _context.Set<PackingTask>()
                    .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == request.TaskId);
                if (packTask == null) return ApiResponse<UnifiedTaskDto>.Fail("Packing task not found");
                packTask.AssignedToUserId = request.UserId;
                packTask.AssignedAt = DateTime.UtcNow;
                packTask.Status = PackingTaskStatus.Assigned;
                await LogTaskAction(tenantId, request.UserId, "Pack", request.TaskId, packTask.TaskNumber, TaskAction.Assigned);
                break;

            case "cyclecount":
                var cycleCount = await _context.Set<CycleCountSession>()
                    .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == request.TaskId);
                if (cycleCount == null) return ApiResponse<UnifiedTaskDto>.Fail("Cycle count not found");
                cycleCount.AssignedToUserId = request.UserId;
                await LogTaskAction(tenantId, request.UserId, "CycleCount", request.TaskId, cycleCount.CountNumber, TaskAction.Assigned);
                break;

            default:
                return ApiResponse<UnifiedTaskDto>.Fail("Unknown task type");
        }

        await _context.SaveChangesAsync();

        return await GetTaskByIdAsync(tenantId, request.TaskType, request.TaskId);
    }

    public async Task<ApiResponse<UnifiedTaskDto>> StartTaskAsync(Guid tenantId, Guid userId, StartTaskRequest request)
    {
        var taskType = request.TaskType.ToLower();
        Guid warehouseId = Guid.Empty;

        switch (taskType)
        {
            case "pick":
                var pickTask = await _context.Set<PickTask>()
                    .Include(t => t.FromLocation)
                    .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == request.TaskId);
                if (pickTask == null) return ApiResponse<UnifiedTaskDto>.Fail("Pick task not found");
                pickTask.Status = PickTaskStatus.InProgress;
                pickTask.StartedAt = DateTime.UtcNow;
                warehouseId = pickTask.FromLocation?.WarehouseId ?? Guid.Empty;
                await LogTaskAction(tenantId, userId, "Pick", request.TaskId, pickTask.TaskNumber, TaskAction.Started);
                break;

            case "putaway":
                var putawayTask = await _context.Set<PutawayTask>()
                    .Include(t => t.FromLocation)
                    .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == request.TaskId);
                if (putawayTask == null) return ApiResponse<UnifiedTaskDto>.Fail("Putaway task not found");
                putawayTask.Status = PutawayTaskStatus.InProgress;
                putawayTask.StartedAt = DateTime.UtcNow;
                warehouseId = putawayTask.FromLocation?.WarehouseId ?? Guid.Empty;
                await LogTaskAction(tenantId, userId, "Putaway", request.TaskId, putawayTask.TaskNumber, TaskAction.Started);
                break;

            case "pack":
                var packTask = await _context.Set<PackingTask>()
                    .Include(t => t.PackingStation)
                    .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == request.TaskId);
                if (packTask == null) return ApiResponse<UnifiedTaskDto>.Fail("Packing task not found");
                packTask.Status = PackingTaskStatus.InProgress;
                packTask.StartedAt = DateTime.UtcNow;
                warehouseId = packTask.PackingStation?.WarehouseId ?? Guid.Empty;
                await LogTaskAction(tenantId, userId, "Pack", request.TaskId, packTask.TaskNumber, TaskAction.Started);
                break;

            case "cyclecount":
                var cycleCount = await _context.Set<CycleCountSession>()
                    .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == request.TaskId);
                if (cycleCount == null) return ApiResponse<UnifiedTaskDto>.Fail("Cycle count not found");
                cycleCount.Status = CycleCountStatus.InProgress;
                cycleCount.StartedAt = DateTime.UtcNow;
                warehouseId = cycleCount.WarehouseId;
                await LogTaskAction(tenantId, userId, "CycleCount", request.TaskId, cycleCount.CountNumber, TaskAction.Started);
                break;

            default:
                return ApiResponse<UnifiedTaskDto>.Fail("Unknown task type");
        }

        // Update session with current task
        await UpdateCurrentTaskInSession(tenantId, userId, request.TaskType, request.TaskId);

        await _context.SaveChangesAsync();

        return await GetTaskByIdAsync(tenantId, request.TaskType, request.TaskId);
    }

    public async Task<ApiResponse<UnifiedTaskDto>> CompleteTaskAsync(Guid tenantId, Guid userId, CompleteTaskRequest request)
    {
        var taskType = request.TaskType.ToLower();
        Guid warehouseId = Guid.Empty;

        switch (taskType)
        {
            case "pick":
                var pickTask = await _context.Set<PickTask>()
                    .Include(t => t.FromLocation)
                    .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == request.TaskId);
                if (pickTask == null) return ApiResponse<UnifiedTaskDto>.Fail("Pick task not found");

                if (request.ActualQuantity.HasValue && request.ActualQuantity < pickTask.QuantityRequired)
                {
                    pickTask.Status = PickTaskStatus.ShortPicked;
                    pickTask.QuantityPicked = request.ActualQuantity.Value;
                    pickTask.QuantityShortPicked = pickTask.QuantityRequired - request.ActualQuantity.Value;
                    pickTask.ShortPickReason = request.ReasonCode;
                }
                else
                {
                    pickTask.Status = PickTaskStatus.Complete;
                    pickTask.QuantityPicked = request.ActualQuantity ?? pickTask.QuantityRequired;
                }
                pickTask.CompletedAt = DateTime.UtcNow;
                warehouseId = pickTask.FromLocation?.WarehouseId ?? Guid.Empty;
                await LogTaskAction(tenantId, userId, "Pick", request.TaskId, pickTask.TaskNumber, TaskAction.Completed,
                    quantity: pickTask.QuantityPicked, notes: request.Notes, reasonCode: request.ReasonCode);
                break;

            case "putaway":
                var putawayTask = await _context.Set<PutawayTask>()
                    .Include(t => t.FromLocation)
                    .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == request.TaskId);
                if (putawayTask == null) return ApiResponse<UnifiedTaskDto>.Fail("Putaway task not found");
                putawayTask.Status = PutawayTaskStatus.Complete;
                putawayTask.QuantityPutaway = request.ActualQuantity ?? putawayTask.QuantityToPutaway;
                putawayTask.CompletedAt = DateTime.UtcNow;
                putawayTask.Notes = request.Notes;
                warehouseId = putawayTask.FromLocation?.WarehouseId ?? Guid.Empty;
                await LogTaskAction(tenantId, userId, "Putaway", request.TaskId, putawayTask.TaskNumber, TaskAction.Completed,
                    quantity: putawayTask.QuantityPutaway, notes: request.Notes);
                break;

            case "pack":
                var packTask = await _context.Set<PackingTask>()
                    .Include(t => t.PackingStation)
                    .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == request.TaskId);
                if (packTask == null) return ApiResponse<UnifiedTaskDto>.Fail("Packing task not found");
                packTask.Status = PackingTaskStatus.Complete;
                packTask.CompletedAt = DateTime.UtcNow;
                packTask.Notes = request.Notes;
                warehouseId = packTask.PackingStation?.WarehouseId ?? Guid.Empty;
                await LogTaskAction(tenantId, userId, "Pack", request.TaskId, packTask.TaskNumber, TaskAction.Completed, notes: request.Notes);
                break;

            case "cyclecount":
                var cycleCount = await _context.Set<CycleCountSession>()
                    .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == request.TaskId);
                if (cycleCount == null) return ApiResponse<UnifiedTaskDto>.Fail("Cycle count not found");
                cycleCount.Status = CycleCountStatus.Complete;
                cycleCount.CompletedAt = DateTime.UtcNow;
                cycleCount.Notes = request.Notes;
                warehouseId = cycleCount.WarehouseId;
                await LogTaskAction(tenantId, userId, "CycleCount", request.TaskId, cycleCount.CountNumber, TaskAction.Completed, notes: request.Notes);
                break;

            default:
                return ApiResponse<UnifiedTaskDto>.Fail("Unknown task type");
        }

        // Clear current task from session
        await ClearCurrentTaskFromSession(tenantId, userId);

        // Update productivity
        await UpdateProductivityAsync(tenantId, userId, warehouseId, request.TaskType);

        await _context.SaveChangesAsync();

        return await GetTaskByIdAsync(tenantId, request.TaskType, request.TaskId);
    }

    public async Task<ApiResponse<UnifiedTaskDto>> PauseTaskAsync(Guid tenantId, Guid userId, PauseTaskRequest request)
    {
        // Log the pause action
        await LogTaskAction(tenantId, userId, request.TaskType, request.TaskId, "", TaskAction.Paused, notes: request.Reason);

        // Clear current task from session
        await ClearCurrentTaskFromSession(tenantId, userId);

        await _context.SaveChangesAsync();

        return await GetTaskByIdAsync(tenantId, request.TaskType, request.TaskId);
    }

    public async Task<ApiResponse<UnifiedTaskDto>> ResumeTaskAsync(Guid tenantId, Guid userId, StartTaskRequest request)
    {
        await LogTaskAction(tenantId, userId, request.TaskType, request.TaskId, "", TaskAction.Resumed);

        // Update session with current task
        await UpdateCurrentTaskInSession(tenantId, userId, request.TaskType, request.TaskId);

        await _context.SaveChangesAsync();

        return await GetTaskByIdAsync(tenantId, request.TaskType, request.TaskId);
    }

    #endregion

    #region Barcode Scanning

    public async Task<ApiResponse<ScanResponse>> ProcessScanAsync(Guid tenantId, Guid userId, ScanRequest request)
    {
        var response = new ScanResponse { Success = true };

        // Get current session
        var session = await _context.Set<OperatorSession>()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.UserId == userId && s.Status == OperatorSessionStatus.Active);

        // Try to resolve the barcode
        var product = await _context.Set<Inventory.Models.Product>()
            .FirstOrDefaultAsync(p => p.TenantId == tenantId &&
                (p.Barcode == request.Barcode || p.Sku == request.Barcode));

        var location = await _context.Set<Warehouse.Models.Location>()
            .FirstOrDefaultAsync(l => l.TenantId == tenantId && l.Code == request.Barcode);

        // Determine what was scanned
        if (product != null)
        {
            response.EntityType = "Product";
            response.EntityId = product.Id;
            response.ResolvedSku = product.Sku;
            response.ResolvedProductName = product.Name;

            // Check stock if location context provided
            if (request.ExpectedLocation != null)
            {
                var loc = await _context.Set<Warehouse.Models.Location>()
                    .FirstOrDefaultAsync(l => l.TenantId == tenantId && l.Code == request.ExpectedLocation);
                if (loc != null)
                {
                    var stock = await _context.Set<Inventory.Models.StockLevel>()
                        .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.ProductId == product.Id && s.LocationId == loc.Id);
                    response.AvailableQuantity = stock?.QuantityOnHand ?? 0;
                }
            }
        }
        else if (location != null)
        {
            response.EntityType = "Location";
            response.EntityId = location.Id;
            response.ResolvedLocation = location.Code;
        }
        else
        {
            response.Success = false;
            response.ErrorCode = "BARCODE_NOT_FOUND";
            response.ErrorMessage = "Barcode not recognized";
        }

        // Validate against expected values
        if (response.Success)
        {
            if (!string.IsNullOrEmpty(request.ExpectedSku) && response.ResolvedSku != request.ExpectedSku)
            {
                response.MatchesExpected = false;
                response.ValidationMessage = $"Expected SKU {request.ExpectedSku}, scanned {response.ResolvedSku}";
            }
            else if (!string.IsNullOrEmpty(request.ExpectedLocation) && response.ResolvedLocation != request.ExpectedLocation)
            {
                response.MatchesExpected = false;
                response.ValidationMessage = $"Expected location {request.ExpectedLocation}, scanned {response.ResolvedLocation}";
            }
            else
            {
                response.MatchesExpected = true;
            }
        }

        // Determine next action hint
        response.NextAction = DetermineNextAction(request.Context, response);

        // Log the scan
        var scanLog = new ScanLog
        {
            TenantId = tenantId,
            UserId = userId,
            SessionId = session?.Id,
            WarehouseId = session?.WarehouseId ?? Guid.Empty,
            Barcode = request.Barcode,
            ScanType = request.ScanType,
            Context = request.Context,
            EntityType = response.EntityType,
            EntityId = response.EntityId,
            ResolvedSku = response.ResolvedSku,
            ResolvedLocation = response.ResolvedLocation,
            TaskType = request.TaskType,
            TaskId = request.TaskId,
            Success = response.Success,
            ErrorCode = response.ErrorCode,
            ErrorMessage = response.ErrorMessage,
            DeviceId = request.DeviceId,
            ScannedAt = DateTime.UtcNow
        };

        _context.Set<ScanLog>().Add(scanLog);

        // Update session activity
        if (session != null)
        {
            session.LastActivityAt = DateTime.UtcNow;
            session.CurrentLocation = response.ResolvedLocation ?? session.CurrentLocation;
        }

        await _context.SaveChangesAsync();

        return ApiResponse<ScanResponse>.Ok(response);
    }

    public async Task<ApiResponse<ScanResponse>> ValidateBarcodeAsync(Guid tenantId, string barcode, ScanContext context)
    {
        var request = new ScanRequest
        {
            Barcode = barcode,
            ScanType = ScanType.Barcode,
            Context = context
        };

        // Validate without logging
        var response = new ScanResponse { Success = true };

        var product = await _context.Set<Inventory.Models.Product>()
            .FirstOrDefaultAsync(p => p.TenantId == tenantId &&
                (p.Barcode == barcode || p.Sku == barcode));

        var location = await _context.Set<Warehouse.Models.Location>()
            .FirstOrDefaultAsync(l => l.TenantId == tenantId && l.Code == barcode);

        if (product != null)
        {
            response.EntityType = "Product";
            response.EntityId = product.Id;
            response.ResolvedSku = product.Sku;
            response.ResolvedProductName = product.Name;
        }
        else if (location != null)
        {
            response.EntityType = "Location";
            response.EntityId = location.Id;
            response.ResolvedLocation = location.Code;
        }
        else
        {
            response.Success = false;
            response.ErrorCode = "BARCODE_NOT_FOUND";
            response.ErrorMessage = "Barcode not recognized";
        }

        return ApiResponse<ScanResponse>.Ok(response);
    }

    public async Task<ApiResponse<PaginatedResult<ScanLogDto>>> GetScanLogsAsync(Guid tenantId, ScanLogQueryParams query)
    {
        var q = _context.Set<ScanLog>()
            .Where(s => s.TenantId == tenantId);

        if (query.WarehouseId.HasValue)
            q = q.Where(s => s.WarehouseId == query.WarehouseId.Value);

        if (query.UserId.HasValue)
            q = q.Where(s => s.UserId == query.UserId.Value);

        if (query.SessionId.HasValue)
            q = q.Where(s => s.SessionId == query.SessionId.Value);

        if (query.Context.HasValue)
            q = q.Where(s => s.Context == query.Context.Value);

        if (query.SuccessOnly.HasValue)
            q = q.Where(s => s.Success == query.SuccessOnly.Value);

        if (query.FromDate.HasValue)
            q = q.Where(s => s.ScannedAt >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            q = q.Where(s => s.ScannedAt <= query.ToDate.Value);

        var totalCount = await q.CountAsync();

        var logs = await q
            .OrderByDescending(s => s.ScannedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        var dtos = logs.Select(l => new ScanLogDto
        {
            Id = l.Id,
            UserId = l.UserId,
            SessionId = l.SessionId,
            Barcode = l.Barcode,
            ScanType = l.ScanType,
            Context = l.Context,
            EntityType = l.EntityType,
            ResolvedSku = l.ResolvedSku,
            ResolvedLocation = l.ResolvedLocation,
            TaskType = l.TaskType,
            TaskId = l.TaskId,
            Success = l.Success,
            ErrorCode = l.ErrorCode,
            ErrorMessage = l.ErrorMessage,
            DeviceId = l.DeviceId,
            ScannedAt = l.ScannedAt
        }).ToList();

        return ApiResponse<PaginatedResult<ScanLogDto>>.Ok(new PaginatedResult<ScanLogDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }

    #endregion

    #region Operator Productivity

    public async Task<ApiResponse<OperatorStatsDto>> GetOperatorStatsAsync(Guid tenantId, Guid userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return ApiResponse<OperatorStatsDto>.Fail("User not found");

        var session = await _context.Set<OperatorSession>()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.UserId == userId && s.Status == OperatorSessionStatus.Active);

        var today = DateTime.UtcNow.Date;
        var productivity = await _context.Set<OperatorProductivity>()
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.UserId == userId && p.Date == today);

        var dto = new OperatorStatsDto
        {
            UserId = userId,
            UserName = $"{user.FirstName} {user.LastName}".Trim(),
            WarehouseId = session?.WarehouseId ?? Guid.Empty,
            IsOnline = session != null,
            MinutesSinceLastActivity = session?.LastActivityAt != null
                ? (int)(DateTime.UtcNow - session.LastActivityAt.Value).TotalMinutes
                : 0,
            Today = new TodayStatsDto
            {
                TasksCompleted = productivity != null
                    ? productivity.PickTasksCompleted + productivity.PackTasksCompleted +
                      productivity.PutawayTasksCompleted + productivity.CycleCountsCompleted
                    : 0,
                UnitsProcessed = productivity?.TotalUnitsPicked + productivity?.TotalUnitsPacked +
                                 productivity?.TotalUnitsPutaway ?? 0,
                WorkMinutes = productivity?.TotalWorkMinutes ?? 0,
                IdleMinutes = productivity?.TotalIdleMinutes ?? 0,
                AccuracyRate = productivity?.AccuracyRate ?? 100,
                TasksPerHour = productivity?.TasksPerHour ?? 0
            }
        };

        if (session != null)
        {
            dto.CurrentSession = await MapSessionToDtoAsync(session);
        }

        // Get current task if any
        if (session?.CurrentTaskId != null && !string.IsNullOrEmpty(session.CurrentTaskType))
        {
            var taskResult = await GetTaskByIdAsync(tenantId, session.CurrentTaskType, session.CurrentTaskId.Value);
            if (taskResult.Success)
                dto.CurrentTask = taskResult.Data;
        }

        return ApiResponse<OperatorStatsDto>.Ok(dto);
    }

    public async Task<ApiResponse<OperatorProductivityDto>> GetOperatorProductivityAsync(Guid tenantId, Guid userId, DateTime date)
    {
        var productivity = await _context.Set<OperatorProductivity>()
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.UserId == userId && p.Date == date.Date);

        if (productivity == null)
            return ApiResponse<OperatorProductivityDto>.Fail("No productivity data for this date");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        return ApiResponse<OperatorProductivityDto>.Ok(new OperatorProductivityDto
        {
            Id = productivity.Id,
            UserId = productivity.UserId,
            UserName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : null,
            WarehouseId = productivity.WarehouseId,
            Date = productivity.Date,
            PickTasksCompleted = productivity.PickTasksCompleted,
            PackTasksCompleted = productivity.PackTasksCompleted,
            PutawayTasksCompleted = productivity.PutawayTasksCompleted,
            CycleCountsCompleted = productivity.CycleCountsCompleted,
            TotalTasksCompleted = productivity.PickTasksCompleted + productivity.PackTasksCompleted +
                                  productivity.PutawayTasksCompleted + productivity.CycleCountsCompleted,
            TotalUnitsPicked = productivity.TotalUnitsPicked,
            TotalUnitsPacked = productivity.TotalUnitsPacked,
            TotalUnitsPutaway = productivity.TotalUnitsPutaway,
            TotalLocationsVisited = productivity.TotalLocationsVisited,
            TotalWorkMinutes = productivity.TotalWorkMinutes,
            TotalIdleMinutes = productivity.TotalIdleMinutes,
            TotalBreakMinutes = productivity.TotalBreakMinutes,
            ProductiveTimePercent = productivity.TotalWorkMinutes > 0
                ? (decimal)productivity.TotalWorkMinutes / (productivity.TotalWorkMinutes + productivity.TotalIdleMinutes + productivity.TotalBreakMinutes) * 100
                : 0,
            TotalScans = productivity.TotalScans,
            CorrectScans = productivity.CorrectScans,
            ErrorScans = productivity.ErrorScans,
            AccuracyRate = productivity.AccuracyRate,
            PicksPerHour = productivity.PicksPerHour,
            UnitsPerHour = productivity.UnitsPerHour,
            TasksPerHour = productivity.TasksPerHour
        });
    }

    public async Task<ApiResponse<List<OperatorProductivityDto>>> GetProductivityHistoryAsync(Guid tenantId, ProductivityQueryParams query)
    {
        var q = _context.Set<OperatorProductivity>()
            .Where(p => p.TenantId == tenantId);

        if (query.WarehouseId.HasValue)
            q = q.Where(p => p.WarehouseId == query.WarehouseId.Value);

        if (query.UserId.HasValue)
            q = q.Where(p => p.UserId == query.UserId.Value);

        if (query.FromDate.HasValue)
        {
            var fromDateUtc = DateTime.SpecifyKind(query.FromDate.Value.Date, DateTimeKind.Utc);
            q = q.Where(p => p.Date >= fromDateUtc);
        }

        if (query.ToDate.HasValue)
        {
            var toDateUtc = DateTime.SpecifyKind(query.ToDate.Value.Date, DateTimeKind.Utc);
            q = q.Where(p => p.Date <= toDateUtc);
        }

        var productivities = await q.OrderByDescending(p => p.Date).ToListAsync();

        var dtos = productivities.Select(p => new OperatorProductivityDto
        {
            Id = p.Id,
            UserId = p.UserId,
            WarehouseId = p.WarehouseId,
            Date = p.Date,
            PickTasksCompleted = p.PickTasksCompleted,
            PackTasksCompleted = p.PackTasksCompleted,
            PutawayTasksCompleted = p.PutawayTasksCompleted,
            CycleCountsCompleted = p.CycleCountsCompleted,
            TotalTasksCompleted = p.PickTasksCompleted + p.PackTasksCompleted + p.PutawayTasksCompleted + p.CycleCountsCompleted,
            TotalUnitsPicked = p.TotalUnitsPicked,
            TotalUnitsPacked = p.TotalUnitsPacked,
            TotalUnitsPutaway = p.TotalUnitsPutaway,
            TotalLocationsVisited = p.TotalLocationsVisited,
            TotalWorkMinutes = p.TotalWorkMinutes,
            TotalIdleMinutes = p.TotalIdleMinutes,
            TotalBreakMinutes = p.TotalBreakMinutes,
            TotalScans = p.TotalScans,
            CorrectScans = p.CorrectScans,
            ErrorScans = p.ErrorScans,
            AccuracyRate = p.AccuracyRate,
            PicksPerHour = p.PicksPerHour,
            UnitsPerHour = p.UnitsPerHour,
            TasksPerHour = p.TasksPerHour
        }).ToList();

        return ApiResponse<List<OperatorProductivityDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<ProductivitySummaryDto>> GetProductivitySummaryAsync(Guid tenantId, ProductivityQueryParams query)
    {
        var fromDate = query.FromDate.HasValue
            ? DateTime.SpecifyKind(query.FromDate.Value.Date, DateTimeKind.Utc)
            : DateTime.UtcNow.AddDays(-7).Date;
        var toDate = query.ToDate.HasValue
            ? DateTime.SpecifyKind(query.ToDate.Value.Date, DateTimeKind.Utc)
            : DateTime.UtcNow.Date;

        var q = _context.Set<OperatorProductivity>()
            .Where(p => p.TenantId == tenantId && p.Date >= fromDate && p.Date <= toDate);

        if (query.WarehouseId.HasValue)
            q = q.Where(p => p.WarehouseId == query.WarehouseId.Value);

        var productivities = await q.ToListAsync();

        var summary = new ProductivitySummaryDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            TotalOperators = productivities.Select(p => p.UserId).Distinct().Count(),
            TotalWorkDays = productivities.Select(p => p.Date).Distinct().Count(),
            TotalTasksCompleted = productivities.Sum(p => p.PickTasksCompleted + p.PackTasksCompleted + p.PutawayTasksCompleted + p.CycleCountsCompleted),
            TotalUnitsProcessed = productivities.Sum(p => p.TotalUnitsPicked + p.TotalUnitsPacked + p.TotalUnitsPutaway),
            TotalWorkMinutes = productivities.Sum(p => p.TotalWorkMinutes)
        };

        if (summary.TotalOperators > 0 && summary.TotalWorkDays > 0)
        {
            summary.AvgTasksPerOperatorPerDay = (decimal)summary.TotalTasksCompleted / (summary.TotalOperators * summary.TotalWorkDays);
            summary.AvgUnitsPerOperatorPerDay = summary.TotalUnitsProcessed / (summary.TotalOperators * summary.TotalWorkDays);
            summary.AvgAccuracyRate = productivities.Any() ? productivities.Average(p => p.AccuracyRate) : 100;
            summary.AvgPicksPerHour = productivities.Any() ? productivities.Average(p => p.PicksPerHour) : 0;
        }

        // Top performers
        var topPerformers = productivities
            .GroupBy(p => p.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                Tasks = g.Sum(p => p.PickTasksCompleted + p.PackTasksCompleted + p.PutawayTasksCompleted + p.CycleCountsCompleted),
                Units = g.Sum(p => p.TotalUnitsPicked + p.TotalUnitsPacked + p.TotalUnitsPutaway),
                Accuracy = g.Average(p => p.AccuracyRate),
                AvgTasksPerHour = g.Average(p => p.TasksPerHour)
            })
            .OrderByDescending(x => x.Tasks)
            .Take(5)
            .ToList();

        foreach (var tp in topPerformers)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == tp.UserId);
            summary.TopPerformers.Add(new TopPerformerDto
            {
                UserId = tp.UserId,
                UserName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "Unknown",
                TasksCompleted = tp.Tasks,
                UnitsProcessed = tp.Units,
                AccuracyRate = tp.Accuracy,
                AvgTasksPerHour = tp.AvgTasksPerHour
            });
        }

        return ApiResponse<ProductivitySummaryDto>.Ok(summary);
    }

    public async Task<ApiResponse<WarehouseOperatorsOverviewDto>> GetWarehouseOperatorsOverviewAsync(Guid tenantId, Guid warehouseId)
    {
        var warehouse = await _context.Set<Warehouse.Models.Warehouse>()
            .FirstOrDefaultAsync(w => w.TenantId == tenantId && w.Id == warehouseId);

        if (warehouse == null)
            return ApiResponse<WarehouseOperatorsOverviewDto>.Fail("Warehouse not found");

        var sessions = await _context.Set<OperatorSession>()
            .Where(s => s.TenantId == tenantId && s.WarehouseId == warehouseId &&
                (s.Status == OperatorSessionStatus.Active || s.Status == OperatorSessionStatus.OnBreak || s.Status == OperatorSessionStatus.Idle))
            .ToListAsync();

        var today = DateTime.UtcNow.Date;
        var todayProductivity = await _context.Set<OperatorProductivity>()
            .Where(p => p.TenantId == tenantId && p.WarehouseId == warehouseId && p.Date == today)
            .ToListAsync();

        var dto = new WarehouseOperatorsOverviewDto
        {
            WarehouseId = warehouseId,
            WarehouseName = warehouse.Name,
            TotalOperators = sessions.Select(s => s.UserId).Distinct().Count(),
            OnlineOperators = sessions.Count(s => s.Status == OperatorSessionStatus.Active),
            OnBreakOperators = sessions.Count(s => s.Status == OperatorSessionStatus.OnBreak),
            IdleOperators = sessions.Count(s => s.Status == OperatorSessionStatus.Idle),
            TotalTasksCompletedToday = todayProductivity.Sum(p => p.PickTasksCompleted + p.PackTasksCompleted + p.PutawayTasksCompleted + p.CycleCountsCompleted),
            TotalUnitsProcessedToday = todayProductivity.Sum(p => p.TotalUnitsPicked + p.TotalUnitsPacked + p.TotalUnitsPutaway),
            AvgAccuracyToday = todayProductivity.Any() ? todayProductivity.Average(p => p.AccuracyRate) : 100
        };

        foreach (var session in sessions)
        {
            dto.ActiveSessions.Add(await MapSessionToDtoAsync(session));
        }

        return ApiResponse<WarehouseOperatorsOverviewDto>.Ok(dto);
    }

    #endregion

    #region Task Action Logs

    public async Task<ApiResponse<PaginatedResult<TaskActionLogDto>>> GetTaskActionLogsAsync(Guid tenantId, TaskActionLogQueryParams query)
    {
        var q = _context.Set<TaskActionLog>()
            .Where(l => l.TenantId == tenantId);

        if (query.WarehouseId.HasValue)
            q = q.Where(l => l.WarehouseId == query.WarehouseId.Value);

        if (query.UserId.HasValue)
            q = q.Where(l => l.UserId == query.UserId.Value);

        if (!string.IsNullOrEmpty(query.TaskType))
            q = q.Where(l => l.TaskType == query.TaskType);

        if (query.TaskId.HasValue)
            q = q.Where(l => l.TaskId == query.TaskId.Value);

        if (query.Action.HasValue)
            q = q.Where(l => l.Action == query.Action.Value);

        if (query.FromDate.HasValue)
            q = q.Where(l => l.ActionAt >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            q = q.Where(l => l.ActionAt <= query.ToDate.Value);

        var totalCount = await q.CountAsync();

        var logs = await q
            .OrderByDescending(l => l.ActionAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        var dtos = new List<TaskActionLogDto>();
        foreach (var log in logs)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == log.UserId);
            dtos.Add(new TaskActionLogDto
            {
                Id = log.Id,
                UserId = log.UserId,
                UserName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : null,
                TaskType = log.TaskType,
                TaskId = log.TaskId,
                TaskNumber = log.TaskNumber,
                Action = log.Action,
                ActionAt = log.ActionAt,
                FromStatus = log.FromStatus,
                ToStatus = log.ToStatus,
                LocationCode = log.LocationCode,
                ProductSku = log.ProductSku,
                Quantity = log.Quantity,
                DurationSeconds = log.DurationSeconds,
                Notes = log.Notes,
                ReasonCode = log.ReasonCode
            });
        }

        return ApiResponse<PaginatedResult<TaskActionLogDto>>.Ok(new PaginatedResult<TaskActionLogDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }

    public async Task<ApiResponse<List<TaskActionLogDto>>> GetTaskHistoryAsync(Guid tenantId, string taskType, Guid taskId)
    {
        var logs = await _context.Set<TaskActionLog>()
            .Where(l => l.TenantId == tenantId && l.TaskType == taskType && l.TaskId == taskId)
            .OrderBy(l => l.ActionAt)
            .ToListAsync();

        var dtos = new List<TaskActionLogDto>();
        foreach (var log in logs)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == log.UserId);
            dtos.Add(new TaskActionLogDto
            {
                Id = log.Id,
                UserId = log.UserId,
                UserName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : null,
                TaskType = log.TaskType,
                TaskId = log.TaskId,
                TaskNumber = log.TaskNumber,
                Action = log.Action,
                ActionAt = log.ActionAt,
                FromStatus = log.FromStatus,
                ToStatus = log.ToStatus,
                LocationCode = log.LocationCode,
                ProductSku = log.ProductSku,
                Quantity = log.Quantity,
                DurationSeconds = log.DurationSeconds,
                Notes = log.Notes,
                ReasonCode = log.ReasonCode
            });
        }

        return ApiResponse<List<TaskActionLogDto>>.Ok(dtos);
    }

    #endregion

    #region Private Helper Methods

    private async Task<OperatorSessionDto> MapSessionToDtoAsync(OperatorSession session)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == session.UserId);
        var warehouse = await _context.Set<Warehouse.Models.Warehouse>()
            .FirstOrDefaultAsync(w => w.Id == session.WarehouseId);

        var idleMinutes = session.LastActivityAt != null && session.Status == OperatorSessionStatus.Active
            ? (int)(DateTime.UtcNow - session.LastActivityAt.Value).TotalMinutes
            : 0;

        return new OperatorSessionDto
        {
            Id = session.Id,
            UserId = session.UserId,
            UserName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : null,
            WarehouseId = session.WarehouseId,
            WarehouseName = warehouse?.Name,
            DeviceId = session.DeviceId,
            DeviceType = session.DeviceType,
            DeviceName = session.DeviceName,
            Status = session.Status,
            StartedAt = session.StartedAt,
            EndedAt = session.EndedAt,
            LastActivityAt = session.LastActivityAt,
            CurrentTaskType = session.CurrentTaskType,
            CurrentTaskId = session.CurrentTaskId,
            CurrentZone = session.CurrentZone,
            CurrentLocation = session.CurrentLocation,
            ShiftCode = session.ShiftCode,
            ShiftStart = session.ShiftStart,
            ShiftEnd = session.ShiftEnd,
            SessionDurationMinutes = (int)(DateTime.UtcNow - session.StartedAt).TotalMinutes,
            IdleMinutes = idleMinutes
        };
    }

    private async Task<List<UnifiedTaskDto>> GetPickTasksAsync(Guid tenantId, TaskQueueQueryParams query)
    {
        var q = _context.Set<PickTask>()
            .Include(t => t.Product)
            .Include(t => t.FromLocation)
            .Include(t => t.ToLocation)
            .Include(t => t.Order)
            .Include(t => t.Batch)
            .Where(t => t.TenantId == tenantId);

        if (query.WarehouseId.HasValue)
            q = q.Where(t => t.FromLocation != null && t.FromLocation.WarehouseId == query.WarehouseId.Value);

        if (!string.IsNullOrEmpty(query.Status) && query.Status.ToLower() != "all")
        {
            if (Enum.TryParse<PickTaskStatus>(query.Status, true, out var status))
                q = q.Where(t => t.Status == status);
        }

        if (!string.IsNullOrEmpty(query.Zone))
            q = q.Where(t => t.FromLocation != null && t.FromLocation.Zone != null && t.FromLocation.Zone.Code == query.Zone);

        if (query.AssignedToUserId.HasValue)
            q = q.Where(t => t.AssignedToUserId == query.AssignedToUserId.Value);

        if (query.UnassignedOnly)
            q = q.Where(t => t.AssignedToUserId == null);

        var tasks = await q.ToListAsync();
        return tasks.Select(MapPickTaskToUnified).ToList();
    }

    private async Task<List<UnifiedTaskDto>> GetPutawayTasksAsync(Guid tenantId, TaskQueueQueryParams query)
    {
        var q = _context.Set<PutawayTask>()
            .Include(t => t.Product)
            .Include(t => t.FromLocation)
            .Include(t => t.SuggestedLocation)
            .Include(t => t.GoodsReceipt)
            .Where(t => t.TenantId == tenantId);

        if (query.WarehouseId.HasValue)
            q = q.Where(t => t.FromLocation != null && t.FromLocation.WarehouseId == query.WarehouseId.Value);

        if (!string.IsNullOrEmpty(query.Status) && query.Status.ToLower() != "all")
        {
            if (Enum.TryParse<PutawayTaskStatus>(query.Status, true, out var status))
                q = q.Where(t => t.Status == status);
        }

        if (query.AssignedToUserId.HasValue)
            q = q.Where(t => t.AssignedToUserId == query.AssignedToUserId.Value);

        if (query.UnassignedOnly)
            q = q.Where(t => t.AssignedToUserId == null);

        var tasks = await q.ToListAsync();
        return tasks.Select(MapPutawayTaskToUnified).ToList();
    }

    private async Task<List<UnifiedTaskDto>> GetPackingTasksAsync(Guid tenantId, TaskQueueQueryParams query)
    {
        var q = _context.Set<PackingTask>()
            .Include(t => t.SalesOrder)
            .Include(t => t.PackingStation)
            .Include(t => t.FulfillmentBatch)
            .Where(t => t.TenantId == tenantId);

        if (query.WarehouseId.HasValue)
            q = q.Where(t => t.PackingStation != null && t.PackingStation.WarehouseId == query.WarehouseId.Value);

        if (!string.IsNullOrEmpty(query.Status) && query.Status.ToLower() != "all")
        {
            if (Enum.TryParse<PackingTaskStatus>(query.Status, true, out var status))
                q = q.Where(t => t.Status == status);
        }

        if (query.AssignedToUserId.HasValue)
            q = q.Where(t => t.AssignedToUserId == query.AssignedToUserId.Value);

        if (query.UnassignedOnly)
            q = q.Where(t => t.AssignedToUserId == null);

        var tasks = await q.ToListAsync();
        return tasks.Select(MapPackingTaskToUnified).ToList();
    }

    private async Task<List<UnifiedTaskDto>> GetCycleCountTasksAsync(Guid tenantId, TaskQueueQueryParams query)
    {
        var q = _context.Set<CycleCountSession>()
            .Include(t => t.Warehouse)
            .Include(t => t.Zone)
            .Where(t => t.TenantId == tenantId);

        if (query.WarehouseId.HasValue)
            q = q.Where(t => t.WarehouseId == query.WarehouseId.Value);

        if (!string.IsNullOrEmpty(query.Status) && query.Status.ToLower() != "all")
        {
            if (Enum.TryParse<CycleCountStatus>(query.Status, true, out var status))
                q = q.Where(t => t.Status == status);
        }

        if (!string.IsNullOrEmpty(query.Zone))
            q = q.Where(t => t.Zone != null && t.Zone.Code == query.Zone);

        if (query.AssignedToUserId.HasValue)
            q = q.Where(t => t.AssignedToUserId == query.AssignedToUserId.Value);

        if (query.UnassignedOnly)
            q = q.Where(t => t.AssignedToUserId == null);

        var tasks = await q.ToListAsync();
        return tasks.Select(MapCycleCountToUnified).ToList();
    }

    private UnifiedTaskDto MapPickTaskToUnified(PickTask task)
    {
        return new UnifiedTaskDto
        {
            Id = task.Id,
            TaskType = "Pick",
            TaskNumber = task.TaskNumber,
            Status = task.Status.ToString(),
            Priority = task.Priority,
            SourceLocation = task.FromLocation?.Code,
            DestinationLocation = task.ToLocation?.Code,
            Zone = task.FromLocation?.Zone?.Code,
            Aisle = task.FromLocation?.Aisle,
            Sku = task.Sku,
            ProductName = task.Product?.Name,
            Quantity = task.QuantityRequired,
            AssignedToUserId = task.AssignedToUserId,
            CreatedAt = task.CreatedAt,
            StartedAt = task.StartedAt,
            CompletedAt = task.CompletedAt,
            OrderNumber = task.Order?.OrderNumber,
            BatchNumber = task.Batch?.BatchNumber
        };
    }

    private UnifiedTaskDto MapPutawayTaskToUnified(PutawayTask task)
    {
        return new UnifiedTaskDto
        {
            Id = task.Id,
            TaskType = "Putaway",
            TaskNumber = task.TaskNumber,
            Status = task.Status.ToString(),
            Priority = task.Priority,
            SourceLocation = task.FromLocation?.Code,
            DestinationLocation = task.SuggestedLocation?.Code ?? task.ActualLocation?.Code,
            Zone = task.SuggestedLocation?.Zone?.Code,
            Aisle = task.SuggestedLocation?.Aisle,
            Sku = task.Sku,
            ProductName = task.Product?.Name,
            Quantity = task.QuantityToPutaway,
            AssignedToUserId = task.AssignedToUserId,
            AssignedAt = task.AssignedAt,
            CreatedAt = task.CreatedAt,
            StartedAt = task.StartedAt,
            CompletedAt = task.CompletedAt
        };
    }

    private UnifiedTaskDto MapPackingTaskToUnified(PackingTask task)
    {
        return new UnifiedTaskDto
        {
            Id = task.Id,
            TaskType = "Pack",
            TaskNumber = task.TaskNumber,
            Status = task.Status.ToString(),
            Priority = task.Priority,
            DestinationLocation = task.PackingStation?.Code,
            Quantity = task.TotalItems,
            AssignedToUserId = task.AssignedToUserId,
            AssignedAt = task.AssignedAt,
            CreatedAt = task.CreatedAt,
            StartedAt = task.StartedAt,
            CompletedAt = task.CompletedAt,
            OrderNumber = task.SalesOrder?.OrderNumber,
            BatchNumber = task.FulfillmentBatch?.BatchNumber
        };
    }

    private UnifiedTaskDto MapCycleCountToUnified(CycleCountSession session)
    {
        return new UnifiedTaskDto
        {
            Id = session.Id,
            TaskType = "CycleCount",
            TaskNumber = session.CountNumber,
            Status = session.Status.ToString(),
            Priority = 5,
            Zone = session.Zone?.Code,
            Quantity = session.TotalLocations,
            AssignedToUserId = session.AssignedToUserId,
            CreatedAt = session.CreatedAt,
            StartedAt = session.StartedAt,
            CompletedAt = session.CompletedAt,
            DueBy = session.ScheduledDate
        };
    }

    private async Task LogTaskAction(Guid tenantId, Guid userId, string taskType, Guid taskId, string taskNumber,
        TaskAction action, string? locationCode = null, string? productSku = null, decimal? quantity = null,
        string? notes = null, string? reasonCode = null)
    {
        // Get warehouse from session
        var session = await _context.Set<OperatorSession>()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.UserId == userId && s.Status == OperatorSessionStatus.Active);

        // Calculate duration from last action
        int? durationSeconds = null;
        var lastAction = await _context.Set<TaskActionLog>()
            .Where(l => l.TenantId == tenantId && l.TaskType == taskType && l.TaskId == taskId)
            .OrderByDescending(l => l.ActionAt)
            .FirstOrDefaultAsync();

        if (lastAction != null)
        {
            durationSeconds = (int)(DateTime.UtcNow - lastAction.ActionAt).TotalSeconds;
        }

        var log = new TaskActionLog
        {
            TenantId = tenantId,
            UserId = userId,
            WarehouseId = session?.WarehouseId ?? Guid.Empty,
            TaskType = taskType,
            TaskId = taskId,
            TaskNumber = taskNumber,
            Action = action,
            ActionAt = DateTime.UtcNow,
            LocationCode = locationCode,
            ProductSku = productSku,
            Quantity = quantity,
            DurationSeconds = durationSeconds,
            Notes = notes,
            ReasonCode = reasonCode
        };

        _context.Set<TaskActionLog>().Add(log);
    }

    private async Task UpdateCurrentTaskInSession(Guid tenantId, Guid userId, string taskType, Guid taskId)
    {
        var session = await _context.Set<OperatorSession>()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.UserId == userId && s.Status == OperatorSessionStatus.Active);

        if (session != null)
        {
            session.CurrentTaskType = taskType;
            session.CurrentTaskId = taskId;
            session.LastActivityAt = DateTime.UtcNow;
        }
    }

    private async Task ClearCurrentTaskFromSession(Guid tenantId, Guid userId)
    {
        var session = await _context.Set<OperatorSession>()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.UserId == userId && s.Status == OperatorSessionStatus.Active);

        if (session != null)
        {
            session.CurrentTaskType = null;
            session.CurrentTaskId = null;
            session.LastActivityAt = DateTime.UtcNow;
        }
    }

    private async Task UpdateProductivityAsync(Guid tenantId, Guid userId, Guid warehouseId, string taskType)
    {
        var today = DateTime.UtcNow.Date;
        var productivity = await _context.Set<OperatorProductivity>()
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.UserId == userId && p.Date == today);

        if (productivity == null)
        {
            productivity = new OperatorProductivity
            {
                TenantId = tenantId,
                UserId = userId,
                WarehouseId = warehouseId,
                Date = today
            };
            _context.Set<OperatorProductivity>().Add(productivity);
        }

        switch (taskType.ToLower())
        {
            case "pick":
                productivity.PickTasksCompleted++;
                break;
            case "putaway":
                productivity.PutawayTasksCompleted++;
                break;
            case "pack":
                productivity.PackTasksCompleted++;
                break;
            case "cyclecount":
                productivity.CycleCountsCompleted++;
                break;
        }

        // Recalculate rates
        var totalTasks = productivity.PickTasksCompleted + productivity.PackTasksCompleted +
                         productivity.PutawayTasksCompleted + productivity.CycleCountsCompleted;
        if (productivity.TotalWorkMinutes > 0)
        {
            productivity.TasksPerHour = (decimal)totalTasks / (productivity.TotalWorkMinutes / 60m);
            productivity.PicksPerHour = (decimal)productivity.PickTasksCompleted / (productivity.TotalWorkMinutes / 60m);
        }
    }

    private string? DetermineNextAction(ScanContext context, ScanResponse response)
    {
        if (!response.Success) return "RescanOrEnterManually";

        return context switch
        {
            ScanContext.LocationVerify => response.MatchesExpected ? "ScanProduct" : "CorrectLocation",
            ScanContext.ProductPick => response.MatchesExpected ? "ConfirmQuantity" : "CorrectProduct",
            ScanContext.ProductPutaway => "ScanDestinationLocation",
            ScanContext.ProductPack => "AddToPackage",
            ScanContext.CycleCount => "EnterCount",
            _ => null
        };
    }

    #endregion
}
