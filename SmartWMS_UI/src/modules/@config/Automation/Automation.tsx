import { useState, useMemo, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import type { ColumnDef } from '@tanstack/react-table';

import { useTranslate } from '@/hooks';
import { DataTable } from '@/components/DataTable';
import { CONFIG } from '@/constants/routes';
import {
  useGetAutomationRulesQuery,
  useToggleAutomationRuleMutation,
  type AutomationRuleDto,
  type AutomationRuleFilters,
  type TriggerType,
  type ActionType,
} from '@/api/modules/automation';

const triggerLabels: Record<TriggerType, string> = {
  EntityCreated: 'Entity Created',
  EntityUpdated: 'Entity Updated',
  EntityDeleted: 'Entity Deleted',
  StatusChanged: 'Status Changed',
  ThresholdCrossed: 'Threshold Crossed',
  Scheduled: 'Scheduled',
  WebhookReceived: 'Webhook Received',
  Manual: 'Manual',
};

const actionLabels: Record<ActionType, string> = {
  SendNotification: 'Send Notification',
  SendEmail: 'Send Email',
  SendWebhook: 'Send Webhook',
  CreateTask: 'Create Task',
  UpdateEntityStatus: 'Update Status',
  UpdateEntityField: 'Update Field',
  GenerateReport: 'Generate Report',
  TriggerSync: 'Trigger Sync',
  CreateAdjustment: 'Create Adjustment',
  CreateTransfer: 'Create Transfer',
};

/**
 * Automation Rules Configuration Module - List Page
 */
export function Automation() {
  const t = useTranslate();
  const navigate = useNavigate();

  const [selectedRuleId, setSelectedRuleId] = useState<string | null>(null);
  const [filters, setFilters] = useState<AutomationRuleFilters>({
    page: 1,
    pageSize: 25,
  });

  const { data, isLoading } = useGetAutomationRulesQuery(filters);
  const [toggleRule] = useToggleAutomationRuleMutation();

  const rules = data?.data?.items ?? [];
  const totalRows = data?.data?.totalCount ?? 0;

  const handleToggle = useCallback(
    async (rule: AutomationRuleDto, e: React.MouseEvent) => {
      e.stopPropagation();
      await toggleRule(rule.id);
    },
    [toggleRule]
  );

  const columns = useMemo<ColumnDef<AutomationRuleDto, unknown>[]>(
    () => [
      {
        accessorKey: 'name',
        header: t('common.name', 'Name'),
        size: 200,
        cell: ({ row }) => (
          <div>
            <div className="font-medium">{row.original.name}</div>
            {row.original.description && (
              <div className="text-sm text-muted">{row.original.description}</div>
            )}
          </div>
        ),
      },
      {
        accessorKey: 'triggerType',
        header: t('automation.trigger', 'Trigger'),
        size: 130,
        cell: ({ getValue }) => {
          const val = getValue() as TriggerType;
          return <span className="badge badge--info">{triggerLabels[val] || val}</span>;
        },
      },
      {
        accessorKey: 'entityType',
        header: t('automation.entity', 'Entity'),
        size: 120,
        cell: ({ getValue }) => getValue() || '-',
      },
      {
        accessorKey: 'actionType',
        header: t('automation.action', 'Action'),
        size: 140,
        cell: ({ getValue }) => {
          const val = getValue() as ActionType;
          return <span className="badge badge--secondary">{actionLabels[val] || val}</span>;
        },
      },
      {
        accessorKey: 'lastTriggeredAt',
        header: t('automation.lastRun', 'Last Run'),
        size: 150,
        cell: ({ getValue }) =>
          getValue() ? new Date(getValue() as string).toLocaleString() : '-',
      },
      {
        accessorKey: 'executionCount',
        header: t('automation.runs', 'Runs'),
        size: 80,
        meta: { align: 'right' },
      },
      {
        accessorKey: 'priority',
        header: t('automation.priority', 'Priority'),
        size: 70,
        meta: { align: 'right' },
      },
      {
        accessorKey: 'isActive',
        header: t('common.status', 'Status'),
        size: 100,
        cell: ({ row }) => (
          <button
            className={`status-badge status-badge--${row.original.isActive ? 'active' : 'inactive'}`}
            onClick={(e) => handleToggle(row.original, e)}
            title={
              row.original.isActive
                ? t('automation.clickToDeactivate', 'Click to deactivate')
                : t('automation.clickToActivate', 'Click to activate')
            }
          >
            {row.original.isActive
              ? t('status.active', 'Active')
              : t('status.inactive', 'Inactive')}
          </button>
        ),
      },
    ],
    [t, handleToggle]
  );

  const handleRowClick = (rule: AutomationRuleDto) => {
    setSelectedRuleId(rule.id);
    navigate(`${CONFIG.AUTOMATION}/${rule.id}`);
  };

  const handleCreateRule = () => {
    navigate(CONFIG.AUTOMATION_CREATE);
  };

  return (
    <div className="page">
      <header className="page__header">
        <div className="page__title-section">
          <h1 className="page__title">{t('nav.config.automation', 'Automation')}</h1>
          <p className="page__subtitle">
            {t('automation.subtitle', 'Configure automation rules and triggers')}
          </p>
        </div>
        <div className="page__actions">
          <button className="btn btn--primary" onClick={handleCreateRule}>
            {t('automation.addRule', 'Add Rule')}
          </button>
        </div>
      </header>

      <div className="page__content">
        <DataTable
          data={rules}
          columns={columns}
          pagination={{
            pageIndex: (filters.page ?? 1) - 1,
            pageSize: filters.pageSize ?? 25,
          }}
          onPaginationChange={({ pageIndex, pageSize }) => {
            setFilters((prev) => ({ ...prev, page: pageIndex + 1, pageSize }));
          }}
          totalRows={totalRows}
          onRowClick={handleRowClick}
          selectedRowId={selectedRuleId}
          getRowId={(row) => row.id}
          loading={isLoading}
          emptyMessage={t('automation.noRules', 'No automation rules found')}
        />
      </div>
    </div>
  );
}

export default Automation;
