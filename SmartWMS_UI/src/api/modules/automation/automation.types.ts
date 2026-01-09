/**
 * Automation Module Types
 */

import type { ApiResponse, PaginatedResponse } from '@/api/types';

// ============================================================================
// Enums
// ============================================================================

export type TriggerType =
  | 'EntityCreated'
  | 'EntityUpdated'
  | 'EntityDeleted'
  | 'StatusChanged'
  | 'ThresholdCrossed'
  | 'Scheduled'
  | 'WebhookReceived'
  | 'Manual';

export type ActionType =
  | 'SendNotification'
  | 'SendEmail'
  | 'SendWebhook'
  | 'CreateTask'
  | 'UpdateEntityStatus'
  | 'UpdateEntityField'
  | 'GenerateReport'
  | 'TriggerSync'
  | 'CreateAdjustment'
  | 'CreateTransfer';

export type ConditionOperator =
  | 'Equals'
  | 'NotEquals'
  | 'GreaterThan'
  | 'LessThan'
  | 'GreaterOrEqual'
  | 'LessOrEqual'
  | 'Contains'
  | 'NotContains'
  | 'StartsWith'
  | 'EndsWith'
  | 'IsNull'
  | 'IsNotNull'
  | 'In'
  | 'NotIn';

export type ExecutionStatus = 'Pending' | 'Running' | 'Success' | 'Failed' | 'Skipped';

// ============================================================================
// DTOs
// ============================================================================

export interface AutomationRuleDto {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
  triggerType: TriggerType;
  entityType?: string;
  actionType: ActionType;
  priority: number;
  lastTriggeredAt?: string;
  executionCount: number;
  createdAt: string;
  updatedAt?: string;
  createdBy?: string;
}

export interface AutomationRuleDetailDto extends AutomationRuleDto {
  triggerConfigJson?: string;
  conditionsJson?: string;
  actionConfigJson?: string;
  cronExpression?: string;
  conditions: RuleConditionDto[];
}

export interface RuleConditionDto {
  id: string;
  ruleId: string;
  field: string;
  operator: ConditionOperator;
  value: string;
  logicalOperator: 'And' | 'Or';
  order: number;
}

export interface RuleExecutionDto {
  id: string;
  ruleId: string;
  ruleName?: string;
  startedAt: string;
  completedAt?: string;
  durationMs?: number;
  status: ExecutionStatus;
  conditionsMet: boolean;
  triggerEntityType?: string;
  triggerEntityId?: string;
  resultData?: string;
  errorMessage?: string;
}

export interface AutomationStatsDto {
  totalRules: number;
  activeRules: number;
  inactiveRules: number;
  totalExecutionsToday: number;
  successfulExecutionsToday: number;
  failedExecutionsToday: number;
  avgExecutionTimeMs: number;
  rulesByTriggerType: Record<string, number>;
  rulesByActionType: Record<string, number>;
  recentExecutions: RuleExecutionDto[];
  topTriggeredRules: { ruleId: string; ruleName: string; count: number }[];
}

export interface TestRuleResponse {
  wouldTrigger: boolean;
  conditionResults: { field: string; operator: string; expected: string; actual: string; passed: boolean }[];
  simulatedAction: string;
  errors: string[];
}

export interface EnumOption {
  value: number;
  name: string;
}

// ============================================================================
// Request Types
// ============================================================================

export interface AutomationRuleFilters {
  search?: string;
  triggerType?: TriggerType;
  actionType?: ActionType;
  entityType?: string;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
}

export interface RuleExecutionFilters {
  ruleId?: string;
  status?: ExecutionStatus;
  dateFrom?: string;
  dateTo?: string;
  page?: number;
  pageSize?: number;
}

export interface CreateAutomationRuleRequest {
  name: string;
  description?: string;
  isActive?: boolean;
  triggerType: TriggerType;
  entityType?: string;
  actionType: ActionType;
  priority?: number;
  triggerConfigJson?: string;
  actionConfigJson?: string;
  cronExpression?: string;
  conditions?: CreateRuleConditionRequest[];
}

export interface CreateRuleConditionRequest {
  field: string;
  operator: ConditionOperator;
  value: string;
  logicalOperator?: 'And' | 'Or';
  order?: number;
}

export interface UpdateAutomationRuleRequest {
  name?: string;
  description?: string;
  isActive?: boolean;
  triggerType?: TriggerType;
  entityType?: string;
  actionType?: ActionType;
  priority?: number;
  triggerConfigJson?: string;
  actionConfigJson?: string;
  cronExpression?: string;
  conditions?: CreateRuleConditionRequest[];
}

export interface TriggerRuleRequest {
  testData?: Record<string, unknown>;
  entityId?: string;
}

export interface TestRuleRequest {
  testData: Record<string, unknown>;
}

// ============================================================================
// Action Config Types (for form builders)
// ============================================================================

export interface SendNotificationConfig {
  title?: string;
  message?: string;
  priority?: 'Low' | 'Normal' | 'High' | 'Urgent';
  userIds?: string[];
  roleIds?: string[];
}

export interface SendEmailConfig {
  toAddresses?: string[];
  ccAddresses?: string[];
  subject?: string;
  bodyTemplate?: string;
  isHtml?: boolean;
}

export interface SendWebhookConfig {
  url: string;
  method?: 'GET' | 'POST' | 'PUT';
  headers?: Record<string, string>;
  payloadTemplate?: string;
}

export interface CreateTaskConfig {
  taskType: string;
  priority?: number;
  assignToUserId?: string;
  assignToRoleId?: string;
  dueInMinutes?: number;
}

export interface UpdateEntityConfig {
  entityType: string;
  field: string;
  value: string;
}

// ============================================================================
// Response Types
// ============================================================================

export type AutomationRuleResponse = ApiResponse<AutomationRuleDto>;
export type AutomationRuleDetailResponse = ApiResponse<AutomationRuleDetailDto>;
export type AutomationRuleListResponse = ApiResponse<PaginatedResponse<AutomationRuleDto>>;
export type RuleExecutionResponse = ApiResponse<RuleExecutionDto>;
export type RuleExecutionListResponse = ApiResponse<PaginatedResponse<RuleExecutionDto>>;
export type AutomationStatsResponse = ApiResponse<AutomationStatsDto>;
export type TestRuleResultResponse = ApiResponse<TestRuleResponse>;
export type EnumOptionsResponse = ApiResponse<EnumOption[]>;
