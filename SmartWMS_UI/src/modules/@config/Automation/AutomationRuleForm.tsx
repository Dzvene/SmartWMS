import { useEffect, useMemo, useCallback } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { useTranslate } from '@/hooks';
import type {
  AutomationRuleDetailDto,
  TriggerType,
  ActionType,
  ConditionOperator,
  CreateRuleConditionRequest,
} from '@/api/modules/automation';

// ============================================================================
// Constants
// ============================================================================

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

// ============================================================================
// Types
// ============================================================================

export interface AutomationRuleFormData {
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

interface AutomationRuleFormProps {
  initialData?: AutomationRuleDetailDto;
  onSubmit: (data: AutomationRuleFormData) => Promise<void>;
  loading?: boolean;
  isEditMode?: boolean;
}

// ============================================================================
// Form Component
// ============================================================================

export function AutomationRuleForm({
  initialData,
  onSubmit,
  loading,
  isEditMode,
}: AutomationRuleFormProps) {
  const t = useTranslate();

  const {
    register,
    handleSubmit,
    control,
    watch,
    setValue,
    reset,
    formState: { errors, isDirty },
  } = useForm<AutomationRuleFormData>({
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

  // Reset form when initialData changes
  useEffect(() => {
    if (initialData) {
      let parsedActionConfig = {};
      if (initialData.actionConfigJson) {
        try {
          parsedActionConfig = JSON.parse(initialData.actionConfigJson);
        } catch {
          // ignore parse errors
        }
      }

      reset({
        name: initialData.name,
        description: initialData.description || '',
        isActive: initialData.isActive,
        triggerType: initialData.triggerType,
        entityType: initialData.entityType || '',
        actionType: initialData.actionType,
        priority: initialData.priority,
        cronExpression: initialData.cronExpression || '',
        conditions: initialData.conditions
          ? initialData.conditions.map((c) => ({
              field: c.field,
              operator: c.operator,
              value: c.value,
              logicalOperator: c.logicalOperator,
              order: c.order,
            }))
          : [],
        actionConfig: parsedActionConfig,
      });
    }
  }, [initialData, reset]);

  const triggerType = watch('triggerType');
  const actionType = watch('actionType');
  const watchedConditions = watch('conditions');
  const conditions = useMemo(() => watchedConditions || [], [watchedConditions]);

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

  const handleFormSubmit = handleSubmit(async (data) => {
    await onSubmit(data);
  });

  return (
    <form className="automation-rule-form" onSubmit={handleFormSubmit}>
      {/* Section: Rule Details */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('automation.ruleDetails', 'Rule Details')}</h3>
        </div>
        <div className="form-section__content">
          <div className="form-grid form-grid--2col">
            <div className="form-field">
              <label className="form-field__label">
                {t('common.name', 'Name')} <span className="required">*</span>
              </label>
              <input
                type="text"
                className={`form-field__input ${errors.name ? 'form-field__input--error' : ''}`}
                placeholder={t('automation.namePlaceholder', 'Enter rule name')}
                {...register('name', { required: t('validation.required', 'Required') })}
              />
              {errors.name && <span className="form-field__error">{errors.name.message}</span>}
            </div>

            <div className="form-field">
              <label className="form-field__label">{t('automation.priority', 'Priority')}</label>
              <input
                type="number"
                className="form-field__input"
                min={0}
                max={100}
                {...register('priority', { valueAsNumber: true })}
              />
            </div>

            <div className="form-field form-field--full">
              <label className="form-field__label">
                {t('common.description', 'Description')}
              </label>
              <textarea
                className="form-field__textarea"
                placeholder={t('automation.descriptionPlaceholder', 'Describe what this rule does')}
                rows={2}
                {...register('description')}
              />
            </div>

            <div className="form-field">
              <label className="form-field__label">{t('common.status', 'Status')}</label>
              <label className="form-checkbox">
                <input type="checkbox" {...register('isActive')} />
                <span>{t('common.active', 'Active')}</span>
              </label>
            </div>
          </div>
        </div>
      </section>

      {/* Section: Trigger */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('automation.trigger', 'Trigger')}</h3>
        </div>
        <div className="form-section__content">
          <div className="form-grid form-grid--2col">
            <div className="form-field">
              <label className="form-field__label">
                {t('automation.triggerType', 'Trigger Type')} <span className="required">*</span>
              </label>
              <select
                className="form-field__select"
                {...register('triggerType', { required: true })}
              >
                {Object.entries(triggerLabels).map(([value, label]) => (
                  <option key={value} value={value}>
                    {label}
                  </option>
                ))}
              </select>
            </div>

            <div className="form-field">
              <label className="form-field__label">{t('automation.entityType', 'Entity Type')}</label>
              <select className="form-field__select" {...register('entityType')}>
                <option value="">{t('automation.selectEntity', 'Select entity...')}</option>
                {entityTypes.map((type) => (
                  <option key={type} value={type}>
                    {type}
                  </option>
                ))}
              </select>
            </div>

            {triggerType === 'Scheduled' && (
              <div className="form-field form-field--full">
                <label className="form-field__label">
                  {t('automation.cronExpression', 'Cron Expression')}
                </label>
                <input
                  type="text"
                  className="form-field__input"
                  placeholder="e.g. 0 6 * * * (every day at 6 AM)"
                  {...register('cronExpression')}
                />
                <span className="form-field__hint">
                  {t('automation.cronHint', 'Format: minute hour day-of-month month day-of-week')}
                </span>
              </div>
            )}
          </div>
        </div>
      </section>

      {/* Section: Conditions */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('automation.conditions', 'Conditions')}</h3>
          <button type="button" className="btn btn--sm btn--secondary" onClick={addCondition}>
            {t('automation.addCondition', '+ Add Condition')}
          </button>
        </div>
        <div className="form-section__content">
          {conditions.length === 0 ? (
            <p className="form-section__info">
              {t('automation.noConditions', 'No conditions. Rule will trigger for all matching events.')}
            </p>
          ) : (
            <div className="conditions-list">
              {conditions.map((_, index) => (
                <div key={index} className="condition-row">
                  {index > 0 && (
                    <Controller
                      name={`conditions.${index}.logicalOperator`}
                      control={control}
                      render={({ field }) => (
                        <select className="form-field__select form-field__select--sm" {...field}>
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
                        className="form-field__input"
                        placeholder={t('automation.fieldName', 'Field name')}
                        {...field}
                      />
                    )}
                  />
                  <Controller
                    name={`conditions.${index}.operator`}
                    control={control}
                    render={({ field }) => (
                      <select className="form-field__select" {...field}>
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
                        className="form-field__input"
                        placeholder={t('automation.value', 'Value')}
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
      </section>

      {/* Section: Action */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('automation.action', 'Action')}</h3>
        </div>
        <div className="form-section__content">
          <div className="form-grid form-grid--2col">
            <div className="form-field">
              <label className="form-field__label">
                {t('automation.actionType', 'Action Type')} <span className="required">*</span>
              </label>
              <select
                className="form-field__select"
                {...register('actionType', { required: true })}
              >
                {Object.entries(actionLabels).map(([value, label]) => (
                  <option key={value} value={value}>
                    {label}
                  </option>
                ))}
              </select>
            </div>
          </div>

          {/* Action-specific config */}
          <ActionConfigForm actionType={actionType} control={control} />
        </div>
      </section>

      {/* Form Actions */}
      <div className="form-actions">
        <button
          type="submit"
          className="btn btn-primary"
          disabled={loading || (!isDirty && isEditMode)}
        >
          {loading
            ? t('common.saving', 'Saving...')
            : isEditMode
              ? t('common.save', 'Save Changes')
              : t('automation.createRule', 'Create Rule')}
        </button>
      </div>
    </form>
  );
}

// ============================================================================
// Action Config Form Component
// ============================================================================

interface ActionConfigFormProps {
  actionType: ActionType;
  control: ReturnType<typeof useForm<AutomationRuleFormData>>['control'];
}

function ActionConfigForm({ actionType, control }: ActionConfigFormProps) {
  const t = useTranslate();

  switch (actionType) {
    case 'SendNotification':
      return (
        <div className="action-config">
          <div className="form-grid form-grid--2col">
            <div className="form-field">
              <label className="form-field__label">{t('automation.notificationTitle', 'Title')}</label>
              <Controller
                name="actionConfig.title"
                control={control}
                render={({ field }) => (
                  <input
                    type="text"
                    className="form-field__input"
                    placeholder={t('automation.titlePlaceholder', 'Notification title (supports {{field}} placeholders)')}
                    value={(field.value as string) || ''}
                    onChange={field.onChange}
                  />
                )}
              />
            </div>
            <div className="form-field">
              <label className="form-field__label">{t('automation.notificationPriority', 'Priority')}</label>
              <Controller
                name="actionConfig.priority"
                control={control}
                render={({ field }) => (
                  <select
                    className="form-field__select"
                    value={(field.value as string) || 'Normal'}
                    onChange={field.onChange}
                  >
                    <option value="Low">{t('automation.priorityLow', 'Low')}</option>
                    <option value="Normal">{t('automation.priorityNormal', 'Normal')}</option>
                    <option value="High">{t('automation.priorityHigh', 'High')}</option>
                    <option value="Urgent">{t('automation.priorityUrgent', 'Urgent')}</option>
                  </select>
                )}
              />
            </div>
            <div className="form-field form-field--full">
              <label className="form-field__label">{t('automation.message', 'Message')}</label>
              <Controller
                name="actionConfig.message"
                control={control}
                render={({ field }) => (
                  <textarea
                    className="form-field__textarea"
                    placeholder={t('automation.messagePlaceholder', 'Notification message (supports {{field}} placeholders)')}
                    rows={3}
                    value={(field.value as string) || ''}
                    onChange={field.onChange}
                  />
                )}
              />
            </div>
          </div>
        </div>
      );

    case 'SendEmail':
      return (
        <div className="action-config">
          <div className="form-grid form-grid--2col">
            <div className="form-field form-field--full">
              <label className="form-field__label">
                {t('automation.toAddresses', 'To Addresses (comma-separated)')}
              </label>
              <Controller
                name="actionConfig.toAddresses"
                control={control}
                render={({ field }) => (
                  <input
                    type="text"
                    className="form-field__input"
                    placeholder="email1@example.com, email2@example.com"
                    value={Array.isArray(field.value) ? (field.value as string[]).join(', ') : ''}
                    onChange={(e) => {
                      const addresses = e.target.value
                        .split(',')
                        .map((s) => s.trim())
                        .filter(Boolean);
                      field.onChange(addresses);
                    }}
                  />
                )}
              />
            </div>
            <div className="form-field form-field--full">
              <label className="form-field__label">{t('automation.subject', 'Subject')}</label>
              <Controller
                name="actionConfig.subject"
                control={control}
                render={({ field }) => (
                  <input
                    type="text"
                    className="form-field__input"
                    placeholder={t('automation.subjectPlaceholder', 'Email subject')}
                    value={(field.value as string) || ''}
                    onChange={field.onChange}
                  />
                )}
              />
            </div>
            <div className="form-field form-field--full">
              <label className="form-field__label">{t('automation.bodyTemplate', 'Body Template')}</label>
              <Controller
                name="actionConfig.bodyTemplate"
                control={control}
                render={({ field }) => (
                  <textarea
                    className="form-field__textarea"
                    placeholder={t('automation.bodyPlaceholder', 'Email body (supports {{field}} placeholders)')}
                    rows={5}
                    value={(field.value as string) || ''}
                    onChange={field.onChange}
                  />
                )}
              />
            </div>
          </div>
        </div>
      );

    case 'SendWebhook':
      return (
        <div className="action-config">
          <div className="form-grid form-grid--2col">
            <div className="form-field">
              <label className="form-field__label">
                {t('automation.webhookUrl', 'URL')} <span className="required">*</span>
              </label>
              <Controller
                name="actionConfig.url"
                control={control}
                render={({ field }) => (
                  <input
                    type="url"
                    className="form-field__input"
                    placeholder="https://api.example.com/webhook"
                    value={(field.value as string) || ''}
                    onChange={field.onChange}
                  />
                )}
              />
            </div>
            <div className="form-field">
              <label className="form-field__label">{t('automation.webhookMethod', 'Method')}</label>
              <Controller
                name="actionConfig.method"
                control={control}
                render={({ field }) => (
                  <select
                    className="form-field__select"
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
            <div className="form-field form-field--full">
              <label className="form-field__label">
                {t('automation.payloadTemplate', 'Payload Template (JSON)')}
              </label>
              <Controller
                name="actionConfig.payloadTemplate"
                control={control}
                render={({ field }) => (
                  <textarea
                    className="form-field__textarea form-field__textarea--monospace"
                    placeholder='{"orderId": "{{id}}", "status": "{{status}}"}'
                    rows={5}
                    value={(field.value as string) || ''}
                    onChange={field.onChange}
                  />
                )}
              />
            </div>
          </div>
        </div>
      );

    case 'CreateTask':
      return (
        <div className="action-config">
          <div className="form-grid form-grid--2col">
            <div className="form-field">
              <label className="form-field__label">{t('automation.taskType', 'Task Type')}</label>
              <Controller
                name="actionConfig.taskType"
                control={control}
                render={({ field }) => (
                  <select
                    className="form-field__select"
                    value={(field.value as string) || 'Pick'}
                    onChange={field.onChange}
                  >
                    <option value="Pick">{t('automation.taskTypePick', 'Pick Task')}</option>
                    <option value="Putaway">{t('automation.taskTypePutaway', 'Putaway Task')}</option>
                    <option value="CycleCount">{t('automation.taskTypeCycleCount', 'Cycle Count')}</option>
                  </select>
                )}
              />
            </div>
            <div className="form-field">
              <label className="form-field__label">{t('automation.taskPriority', 'Priority')}</label>
              <Controller
                name="actionConfig.priority"
                control={control}
                render={({ field }) => (
                  <input
                    type="number"
                    className="form-field__input"
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
        <div className="form-section__info">
          <p>
            {t(
              'automation.actionConfigInfo',
              `Action-specific configuration for ${actionLabels[actionType]} can be customized in future updates.`
            )}
          </p>
        </div>
      );
  }
}

export { actionLabels, triggerLabels };
