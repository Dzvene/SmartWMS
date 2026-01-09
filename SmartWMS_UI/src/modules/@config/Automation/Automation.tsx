import { useState, useMemo, useCallback } from 'react';
import { useIntl } from 'react-intl';
import type { ColumnDef } from '@tanstack/react-table';
import { useForm, Controller } from 'react-hook-form';

import { DataTable } from '@/components/DataTable';
import { FullscreenModal } from '@/components/FullscreenModal';
import {
  useGetAutomationRulesQuery,
  useGetAutomationRuleByIdQuery,
  useCreateAutomationRuleMutation,
  useUpdateAutomationRuleMutation,
  useToggleAutomationRuleMutation,
  useDeleteAutomationRuleMutation,
  type AutomationRuleDto,
  type AutomationRuleFilters,
  type CreateAutomationRuleRequest,
  type UpdateAutomationRuleRequest,
  type TriggerType,
  type ActionType,
  type ConditionOperator,
  type CreateRuleConditionRequest,
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

const entityTypes = [
  'SalesOrder',
  'PurchaseOrder',
  'Product',
  'Location',
  'Inventory',
  'GoodsReceipt',
  'Shipment',
  'PickTask',
  'PutawayTask',
  'StockAdjustment',
  'StockTransfer',
];

const conditionOperators: { value: ConditionOperator; label: string }[] = [
  { value: 'Equals', label: '= Equals' },
  { value: 'NotEquals', label: '!= Not Equals' },
  { value: 'GreaterThan', label: '> Greater Than' },
  { value: 'LessThan', label: '< Less Than' },
  { value: 'GreaterOrEqual', label: '>= Greater Or Equal' },
  { value: 'LessOrEqual', label: '<= Less Or Equal' },
  { value: 'Contains', label: 'Contains' },
  { value: 'NotContains', label: 'Not Contains' },
  { value: 'StartsWith', label: 'Starts With' },
  { value: 'EndsWith', label: 'Ends With' },
  { value: 'IsNull', label: 'Is Null' },
  { value: 'IsNotNull', label: 'Is Not Null' },
  { value: 'In', label: 'In List' },
  { value: 'NotIn', label: 'Not In List' },
];

interface FormData {
  name: string;
  description: string;
  isActive: boolean;
  triggerType: TriggerType;
  entityType: string;
  actionType: ActionType;
  priority: number;
  cronExpression: string;
  conditions: CreateRuleConditionRequest[];
  actionConfig: Record<string, unknown>;
}

/**
 * Automation Rules Configuration Module
 */
export function Automation() {
  const { formatMessage } = useIntl();
  const t = (id: string) => formatMessage({ id });

  const [selectedRuleId, setSelectedRuleId] = useState<string | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [filters, setFilters] = useState<AutomationRuleFilters>({
    page: 1,
    pageSize: 25,
  });

  const { data, isLoading } = useGetAutomationRulesQuery(filters);
  const [toggleRule] = useToggleAutomationRuleMutation();
  const [deleteRule] = useDeleteAutomationRuleMutation();

  const rules = data?.data?.items ?? [];
  const totalRows = data?.data?.totalCount ?? 0;

  const handleToggle = async (rule: AutomationRuleDto, e: React.MouseEvent) => {
    e.stopPropagation();
    await toggleRule(rule.id);
  };

  const handleDelete = async (rule: AutomationRuleDto, e: React.MouseEvent) => {
    e.stopPropagation();
    if (confirm(`Are you sure you want to delete rule "${rule.name}"?`)) {
      await deleteRule(rule.id);
    }
  };

  const columns = useMemo<ColumnDef<AutomationRuleDto, unknown>[]>(
    () => [
      {
        accessorKey: 'name',
        header: t('common.name'),
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
        header: 'Trigger',
        size: 130,
        cell: ({ getValue }) => {
          const val = getValue() as TriggerType;
          return (
            <span className="badge badge--info">
              {triggerLabels[val] || val}
            </span>
          );
        },
      },
      {
        accessorKey: 'entityType',
        header: 'Entity',
        size: 120,
        cell: ({ getValue }) => getValue() || '—',
      },
      {
        accessorKey: 'actionType',
        header: 'Action',
        size: 140,
        cell: ({ getValue }) => {
          const val = getValue() as ActionType;
          return (
            <span className="badge badge--secondary">
              {actionLabels[val] || val}
            </span>
          );
        },
      },
      {
        accessorKey: 'lastTriggeredAt',
        header: 'Last Run',
        size: 150,
        cell: ({ getValue }) =>
          getValue() ? new Date(getValue() as string).toLocaleString() : '—',
      },
      {
        accessorKey: 'executionCount',
        header: 'Runs',
        size: 80,
        meta: { align: 'right' },
      },
      {
        accessorKey: 'priority',
        header: 'Priority',
        size: 70,
        meta: { align: 'right' },
      },
      {
        accessorKey: 'isActive',
        header: t('common.status'),
        size: 100,
        cell: ({ row }) => (
          <button
            className={`status-badge status-badge--${row.original.isActive ? 'active' : 'inactive'}`}
            onClick={(e) => handleToggle(row.original, e)}
            title={row.original.isActive ? 'Click to deactivate' : 'Click to activate'}
          >
            {row.original.isActive ? t('status.active') : t('status.inactive')}
          </button>
        ),
      },
      {
        id: 'actions',
        header: '',
        size: 50,
        cell: ({ row }) => (
          <button
            className="btn btn--icon btn--danger-ghost"
            onClick={(e) => handleDelete(row.original, e)}
            title="Delete rule"
          >
            <span className="icon icon--delete" />
          </button>
        ),
      },
    ],
    [t]
  );

  const handleRowClick = (rule: AutomationRuleDto) => {
    setSelectedRuleId(rule.id);
    setIsModalOpen(true);
  };

  const handleAddNew = () => {
    setSelectedRuleId(null);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setSelectedRuleId(null);
  };

  return (
    <div className="page">
      <div className="page__header">
        <h1 className="page__title">{t('nav.config.automation')}</h1>
        <div className="page__actions">
          <button className="btn btn--primary" onClick={handleAddNew}>
            Add Rule
          </button>
        </div>
      </div>
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
          emptyMessage={t('common.noData')}
        />
      </div>
      <FullscreenModal
        open={isModalOpen}
        onClose={handleCloseModal}
        title={selectedRuleId ? 'Edit Automation Rule' : 'Create Automation Rule'}
      >
        <AutomationRuleForm ruleId={selectedRuleId} onClose={handleCloseModal} />
      </FullscreenModal>
    </div>
  );
}

interface AutomationRuleFormProps {
  ruleId: string | null;
  onClose: () => void;
}

function AutomationRuleForm({ ruleId, onClose }: AutomationRuleFormProps) {
  const { formatMessage } = useIntl();
  const t = (id: string) => formatMessage({ id });

  const isEdit = Boolean(ruleId);

  // Fetch rule details if editing
  const { data: ruleData, isLoading: isLoadingRule } = useGetAutomationRuleByIdQuery(ruleId!, {
    skip: !ruleId,
  });
  const rule = ruleData?.data;

  const [createRule, { isLoading: isCreating }] = useCreateAutomationRuleMutation();
  const [updateRule, { isLoading: isUpdating }] = useUpdateAutomationRuleMutation();

  const isSubmitting = isCreating || isUpdating;

  const {
    register,
    handleSubmit,
    control,
    watch,
    setValue,
    formState: { errors },
  } = useForm<FormData>({
    defaultValues: {
      name: '',
      description: '',
      isActive: true,
      triggerType: 'EntityCreated',
      entityType: '',
      actionType: 'SendNotification',
      priority: 0,
      cronExpression: '',
      conditions: [],
      actionConfig: {},
    },
  });

  // Update form when rule data loads
  useState(() => {
    if (rule) {
      setValue('name', rule.name);
      setValue('description', rule.description || '');
      setValue('isActive', rule.isActive);
      setValue('triggerType', rule.triggerType);
      setValue('entityType', rule.entityType || '');
      setValue('actionType', rule.actionType);
      setValue('priority', rule.priority);
      setValue('cronExpression', rule.cronExpression || '');
      if (rule.conditions) {
        setValue(
          'conditions',
          rule.conditions.map((c) => ({
            field: c.field,
            operator: c.operator,
            value: c.value,
            logicalOperator: c.logicalOperator,
            order: c.order,
          }))
        );
      }
      if (rule.actionConfigJson) {
        try {
          setValue('actionConfig', JSON.parse(rule.actionConfigJson));
        } catch {
          // ignore parse errors
        }
      }
    }
  });

  const triggerType = watch('triggerType');
  const actionType = watch('actionType');
  const conditions = watch('conditions') || [];

  const addCondition = useCallback(() => {
    const newCondition: CreateRuleConditionRequest = {
      field: '',
      operator: 'Equals',
      value: '',
      logicalOperator: 'And',
      order: conditions.length,
    };
    setValue('conditions', [...conditions, newCondition]);
  }, [conditions, setValue]);

  const removeCondition = useCallback(
    (index: number) => {
      const newConditions = conditions.filter((_, i) => i !== index);
      setValue('conditions', newConditions);
    },
    [conditions, setValue]
  );

  const onSubmit = async (data: FormData) => {
    try {
      const actionConfigJson =
        Object.keys(data.actionConfig || {}).length > 0
          ? JSON.stringify(data.actionConfig)
          : undefined;

      if (isEdit && ruleId) {
        const updateData: UpdateAutomationRuleRequest = {
          name: data.name,
          description: data.description || undefined,
          isActive: data.isActive,
          triggerType: data.triggerType,
          entityType: data.entityType || undefined,
          actionType: data.actionType,
          priority: data.priority,
          cronExpression: data.cronExpression || undefined,
          conditions: data.conditions.length > 0 ? data.conditions : undefined,
          actionConfigJson,
        };
        await updateRule({ id: ruleId, data: updateData }).unwrap();
      } else {
        const createData: CreateAutomationRuleRequest = {
          name: data.name,
          description: data.description || undefined,
          isActive: data.isActive,
          triggerType: data.triggerType,
          entityType: data.entityType || undefined,
          actionType: data.actionType,
          priority: data.priority,
          cronExpression: data.cronExpression || undefined,
          conditions: data.conditions.length > 0 ? data.conditions : undefined,
          actionConfigJson,
        };
        await createRule(createData).unwrap();
      }
      onClose();
    } catch (error) {
      console.error('Failed to save rule:', error);
    }
  };

  if (isLoadingRule) {
    return <div className="loading">Loading rule...</div>;
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="form">
      {/* Basic Info */}
      <div className="form__section">
        <h3 className="form__section-title">Rule Details</h3>

        <div className="form__row">
          <div className="form__field">
            <label className="form__label">{t('common.name')} *</label>
            <input
              type="text"
              className={`form__input ${errors.name ? 'form__input--error' : ''}`}
              placeholder="Enter rule name"
              {...register('name', { required: 'Name is required' })}
            />
            {errors.name && <span className="form__error">{errors.name.message}</span>}
          </div>
          <div className="form__field">
            <label className="form__label">Priority</label>
            <input
              type="number"
              className="form__input"
              min={0}
              max={100}
              {...register('priority', { valueAsNumber: true })}
            />
          </div>
          <div className="form__field form__field--checkbox">
            <label className="form__checkbox">
              <input type="checkbox" {...register('isActive')} />
              <span>Active</span>
            </label>
          </div>
        </div>

        <div className="form__field">
          <label className="form__label">Description</label>
          <textarea
            className="form__textarea"
            placeholder="Describe what this rule does"
            rows={2}
            {...register('description')}
          />
        </div>
      </div>

      {/* Trigger */}
      <div className="form__section">
        <h3 className="form__section-title">Trigger</h3>

        <div className="form__row">
          <div className="form__field">
            <label className="form__label">Trigger Type *</label>
            <select className="form__select" {...register('triggerType', { required: true })}>
              {Object.entries(triggerLabels).map(([value, label]) => (
                <option key={value} value={value}>
                  {label}
                </option>
              ))}
            </select>
          </div>
          <div className="form__field">
            <label className="form__label">Entity Type</label>
            <select className="form__select" {...register('entityType')}>
              <option value="">Select entity...</option>
              {entityTypes.map((type) => (
                <option key={type} value={type}>
                  {type}
                </option>
              ))}
            </select>
          </div>
        </div>

        {triggerType === 'Scheduled' && (
          <div className="form__field">
            <label className="form__label">Cron Expression</label>
            <input
              type="text"
              className="form__input"
              placeholder="e.g. 0 6 * * * (every day at 6 AM)"
              {...register('cronExpression')}
            />
            <span className="form__hint">
              Format: minute hour day-of-month month day-of-week
            </span>
          </div>
        )}
      </div>

      {/* Conditions */}
      <div className="form__section">
        <h3 className="form__section-title">
          Conditions
          <button type="button" className="btn btn--sm btn--secondary" onClick={addCondition}>
            + Add Condition
          </button>
        </h3>

        {conditions.length === 0 ? (
          <p className="form__info">No conditions. Rule will trigger for all matching events.</p>
        ) : (
          <div className="conditions-list">
            {conditions.map((_, index) => (
              <div key={index} className="condition-row">
                {index > 0 && (
                  <Controller
                    name={`conditions.${index}.logicalOperator`}
                    control={control}
                    render={({ field }) => (
                      <select className="form__select form__select--sm" {...field}>
                        <option value="And">AND</option>
                        <option value="Or">OR</option>
                      </select>
                    )}
                  />
                )}
                <Controller
                  name={`conditions.${index}.field`}
                  control={control}
                  render={({ field }) => (
                    <input
                      type="text"
                      className="form__input"
                      placeholder="Field name"
                      {...field}
                    />
                  )}
                />
                <Controller
                  name={`conditions.${index}.operator`}
                  control={control}
                  render={({ field }) => (
                    <select className="form__select" {...field}>
                      {conditionOperators.map((op) => (
                        <option key={op.value} value={op.value}>
                          {op.label}
                        </option>
                      ))}
                    </select>
                  )}
                />
                <Controller
                  name={`conditions.${index}.value`}
                  control={control}
                  render={({ field }) => (
                    <input
                      type="text"
                      className="form__input"
                      placeholder="Value"
                      {...field}
                    />
                  )}
                />
                <button
                  type="button"
                  className="btn btn--icon btn--danger-ghost"
                  onClick={() => removeCondition(index)}
                >
                  <span className="icon icon--delete" />
                </button>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Action */}
      <div className="form__section">
        <h3 className="form__section-title">Action</h3>

        <div className="form__field">
          <label className="form__label">Action Type *</label>
          <select className="form__select" {...register('actionType', { required: true })}>
            {Object.entries(actionLabels).map(([value, label]) => (
              <option key={value} value={value}>
                {label}
              </option>
            ))}
          </select>
        </div>

        {/* Action-specific config */}
        <ActionConfigForm actionType={actionType} control={control} register={register} />
      </div>

      {/* Form Actions */}
      <div className="form__actions">
        <button type="button" className="btn btn--secondary" onClick={onClose}>
          {t('common.cancel')}
        </button>
        <button type="submit" className="btn btn--primary" disabled={isSubmitting}>
          {isSubmitting ? 'Saving...' : isEdit ? t('common.save') : 'Create Rule'}
        </button>
      </div>
    </form>
  );
}

interface ActionConfigFormProps {
  actionType: ActionType;
  control: ReturnType<typeof useForm<FormData>>['control'];
  register: ReturnType<typeof useForm<FormData>>['register'];
}

function ActionConfigForm({ actionType, control }: ActionConfigFormProps) {
  switch (actionType) {
    case 'SendNotification':
      return (
        <div className="action-config">
          <div className="form__row">
            <div className="form__field">
              <label className="form__label">Title</label>
              <Controller
                name="actionConfig.title"
                control={control}
                render={({ field }) => (
                  <input
                    type="text"
                    className="form__input"
                    placeholder="Notification title (supports {{field}} placeholders)"
                    value={(field.value as string) || ''}
                    onChange={field.onChange}
                  />
                )}
              />
            </div>
            <div className="form__field">
              <label className="form__label">Priority</label>
              <Controller
                name="actionConfig.priority"
                control={control}
                render={({ field }) => (
                  <select
                    className="form__select"
                    value={(field.value as string) || 'Normal'}
                    onChange={field.onChange}
                  >
                    <option value="Low">Low</option>
                    <option value="Normal">Normal</option>
                    <option value="High">High</option>
                    <option value="Urgent">Urgent</option>
                  </select>
                )}
              />
            </div>
          </div>
          <div className="form__field">
            <label className="form__label">Message</label>
            <Controller
              name="actionConfig.message"
              control={control}
              render={({ field }) => (
                <textarea
                  className="form__textarea"
                  placeholder="Notification message (supports {{field}} placeholders)"
                  rows={3}
                  value={(field.value as string) || ''}
                  onChange={field.onChange}
                />
              )}
            />
          </div>
        </div>
      );

    case 'SendEmail':
      return (
        <div className="action-config">
          <div className="form__field">
            <label className="form__label">To Addresses (comma-separated)</label>
            <Controller
              name="actionConfig.toAddresses"
              control={control}
              render={({ field }) => (
                <input
                  type="text"
                  className="form__input"
                  placeholder="email1@example.com, email2@example.com"
                  value={Array.isArray(field.value) ? (field.value as string[]).join(', ') : ''}
                  onChange={(e) => {
                    const addresses = e.target.value.split(',').map((s) => s.trim()).filter(Boolean);
                    field.onChange(addresses);
                  }}
                />
              )}
            />
          </div>
          <div className="form__field">
            <label className="form__label">Subject</label>
            <Controller
              name="actionConfig.subject"
              control={control}
              render={({ field }) => (
                <input
                  type="text"
                  className="form__input"
                  placeholder="Email subject"
                  value={(field.value as string) || ''}
                  onChange={field.onChange}
                />
              )}
            />
          </div>
          <div className="form__field">
            <label className="form__label">Body Template</label>
            <Controller
              name="actionConfig.bodyTemplate"
              control={control}
              render={({ field }) => (
                <textarea
                  className="form__textarea"
                  placeholder="Email body (supports {{field}} placeholders)"
                  rows={5}
                  value={(field.value as string) || ''}
                  onChange={field.onChange}
                />
              )}
            />
          </div>
        </div>
      );

    case 'SendWebhook':
      return (
        <div className="action-config">
          <div className="form__row">
            <div className="form__field" style={{ flex: 2 }}>
              <label className="form__label">URL *</label>
              <Controller
                name="actionConfig.url"
                control={control}
                render={({ field }) => (
                  <input
                    type="url"
                    className="form__input"
                    placeholder="https://api.example.com/webhook"
                    value={(field.value as string) || ''}
                    onChange={field.onChange}
                  />
                )}
              />
            </div>
            <div className="form__field">
              <label className="form__label">Method</label>
              <Controller
                name="actionConfig.method"
                control={control}
                render={({ field }) => (
                  <select
                    className="form__select"
                    value={(field.value as string) || 'POST'}
                    onChange={field.onChange}
                  >
                    <option value="GET">GET</option>
                    <option value="POST">POST</option>
                    <option value="PUT">PUT</option>
                  </select>
                )}
              />
            </div>
          </div>
          <div className="form__field">
            <label className="form__label">Payload Template (JSON)</label>
            <Controller
              name="actionConfig.payloadTemplate"
              control={control}
              render={({ field }) => (
                <textarea
                  className="form__textarea form__textarea--monospace"
                  placeholder='{"orderId": "{{id}}", "status": "{{status}}"}'
                  rows={5}
                  value={(field.value as string) || ''}
                  onChange={field.onChange}
                />
              )}
            />
          </div>
        </div>
      );

    case 'CreateTask':
      return (
        <div className="action-config">
          <div className="form__row">
            <div className="form__field">
              <label className="form__label">Task Type</label>
              <Controller
                name="actionConfig.taskType"
                control={control}
                render={({ field }) => (
                  <select
                    className="form__select"
                    value={(field.value as string) || 'Pick'}
                    onChange={field.onChange}
                  >
                    <option value="Pick">Pick Task</option>
                    <option value="Putaway">Putaway Task</option>
                    <option value="CycleCount">Cycle Count</option>
                  </select>
                )}
              />
            </div>
            <div className="form__field">
              <label className="form__label">Priority</label>
              <Controller
                name="actionConfig.priority"
                control={control}
                render={({ field }) => (
                  <input
                    type="number"
                    className="form__input"
                    min={1}
                    max={10}
                    value={(field.value as number) || 5}
                    onChange={(e) => field.onChange(parseInt(e.target.value) || 5)}
                  />
                )}
              />
            </div>
          </div>
        </div>
      );

    default:
      return (
        <div className="form__info">
          <p>
            Action-specific configuration for <strong>{actionLabels[actionType]}</strong> can be
            customized in future updates.
          </p>
        </div>
      );
  }
}

export default Automation;
