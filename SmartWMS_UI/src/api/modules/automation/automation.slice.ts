/**
 * Automation API Slice
 */

import { baseApi } from '@/api/baseApi';
import type { ApiResponse } from '@/api/types';
import type {
  AutomationRuleListResponse,
  AutomationRuleDetailResponse,
  AutomationRuleResponse,
  AutomationRuleFilters,
  CreateAutomationRuleRequest,
  UpdateAutomationRuleRequest,
  RuleExecutionListResponse,
  RuleExecutionFilters,
  RuleExecutionResponse,
  AutomationStatsResponse,
  TestRuleResultResponse,
  TestRuleRequest,
  TriggerRuleRequest,
  EnumOptionsResponse,
} from './automation.types';

export const automationApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // ========================================================================
    // Rules CRUD
    // ========================================================================

    getAutomationRules: builder.query<AutomationRuleListResponse, AutomationRuleFilters | void>({
      query: (params) => ({
        url: `/automation/rules`,
        params: params || {},
      }),
      providesTags: ['AutomationRules'],
    }),

    getAutomationRuleById: builder.query<AutomationRuleDetailResponse, string>({
      query: (id) => `/automation/rules/${id}`,
      providesTags: (_r, _e, id) => [{ type: 'AutomationRules', id }],
    }),

    createAutomationRule: builder.mutation<AutomationRuleResponse, CreateAutomationRuleRequest>({
      query: (body) => ({
        url: `/automation/rules`,
        method: 'POST',
        body,
      }),
      invalidatesTags: ['AutomationRules', 'AutomationStats'],
    }),

    updateAutomationRule: builder.mutation<
      AutomationRuleResponse,
      { id: string; data: UpdateAutomationRuleRequest }
    >({
      query: ({ id, data }) => ({
        url: `/automation/rules/${id}`,
        method: 'PUT',
        body: data,
      }),
      invalidatesTags: ['AutomationRules', 'AutomationStats'],
    }),

    deleteAutomationRule: builder.mutation<ApiResponse<boolean>, string>({
      query: (id) => ({
        url: `/automation/rules/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['AutomationRules', 'AutomationStats'],
    }),

    toggleAutomationRule: builder.mutation<AutomationRuleResponse, string>({
      query: (id) => ({
        url: `/automation/rules/${id}/toggle`,
        method: 'POST',
      }),
      invalidatesTags: ['AutomationRules', 'AutomationStats'],
    }),

    // ========================================================================
    // Rule Execution
    // ========================================================================

    triggerRule: builder.mutation<RuleExecutionResponse, { id: string; data?: TriggerRuleRequest }>({
      query: ({ id, data }) => ({
        url: `/automation/rules/${id}/trigger`,
        method: 'POST',
        body: data || {},
      }),
      invalidatesTags: ['AutomationExecutions', 'AutomationStats'],
    }),

    testRule: builder.mutation<TestRuleResultResponse, { id: string; data: TestRuleRequest }>({
      query: ({ id, data }) => ({
        url: `/automation/rules/${id}/test`,
        method: 'POST',
        body: data,
      }),
    }),

    // ========================================================================
    // Executions History
    // ========================================================================

    getExecutions: builder.query<RuleExecutionListResponse, RuleExecutionFilters | void>({
      query: (params) => ({
        url: `/automation/executions`,
        params: params || {},
      }),
      providesTags: ['AutomationExecutions'],
    }),

    getRuleExecutions: builder.query<
      RuleExecutionListResponse,
      { ruleId: string; page?: number; pageSize?: number }
    >({
      query: ({ ruleId, page, pageSize }) => ({
        url: `/automation/rules/${ruleId}/executions`,
        params: { page, pageSize },
      }),
      providesTags: ['AutomationExecutions'],
    }),

    // ========================================================================
    // Stats
    // ========================================================================

    getAutomationStats: builder.query<AutomationStatsResponse, void>({
      query: () => `/automation/stats`,
      providesTags: ['AutomationStats'],
    }),

    // ========================================================================
    // Enum Options (for dropdowns)
    // ========================================================================

    getTriggerTypes: builder.query<EnumOptionsResponse, void>({
      query: () => `/automation/trigger-types`,
    }),

    getActionTypes: builder.query<EnumOptionsResponse, void>({
      query: () => `/automation/action-types`,
    }),

    getConditionOperators: builder.query<EnumOptionsResponse, void>({
      query: () => `/automation/condition-operators`,
    }),
  }),
});

export const {
  // Rules CRUD
  useGetAutomationRulesQuery,
  useGetAutomationRuleByIdQuery,
  useCreateAutomationRuleMutation,
  useUpdateAutomationRuleMutation,
  useDeleteAutomationRuleMutation,
  useToggleAutomationRuleMutation,
  // Rule Execution
  useTriggerRuleMutation,
  useTestRuleMutation,
  // Executions History
  useGetExecutionsQuery,
  useGetRuleExecutionsQuery,
  // Stats
  useGetAutomationStatsQuery,
  // Enum Options
  useGetTriggerTypesQuery,
  useGetActionTypesQuery,
  useGetConditionOperatorsQuery,
} = automationApi;
