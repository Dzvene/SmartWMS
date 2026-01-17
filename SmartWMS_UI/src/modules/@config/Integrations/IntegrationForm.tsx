import { useEffect } from 'react';
import { useTranslate } from '@/hooks';
import { useForm } from 'react-hook-form';
import type { IntegrationDto, IntegrationType } from '@/api/modules/integrations';

export interface IntegrationFormData {
  name: string;
  integrationType: IntegrationType;
  providerName: string;
  description: string;
  baseUrl: string;
  apiVersion: string;
  authType: string;
  syncSchedule: string;
}

interface IntegrationFormProps {
  initialData?: IntegrationDto;
  onSubmit: (data: IntegrationFormData) => Promise<void>;
  loading?: boolean;
  isEditMode?: boolean;
  onTestConnection?: () => void;
  isTesting?: boolean;
  testResult?: { success: boolean; message: string } | null;
  onToggleStatus?: () => void;
}

const typeLabels: Record<IntegrationType, string> = {
  ERP: 'ERP',
  Ecommerce: 'E-commerce',
  Carrier: 'Carrier',
  Accounting: 'Accounting',
  CRM: 'CRM',
  Custom: 'Custom',
};

const INTEGRATION_TYPES: IntegrationType[] = ['ERP', 'Ecommerce', 'Carrier', 'Accounting', 'CRM', 'Custom'];

const statusClassMap: Record<string, string> = {
  Active: 'success',
  Inactive: 'neutral',
  Error: 'error',
  Pending: 'warning',
  Disconnected: 'neutral',
};

export function IntegrationForm({
  initialData,
  onSubmit,
  loading,
  isEditMode,
  onTestConnection,
  isTesting,
  testResult,
  onToggleStatus,
}: IntegrationFormProps) {
  const t = useTranslate();

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isDirty },
  } = useForm<IntegrationFormData>({
    defaultValues: {
      name: '',
      integrationType: 'ERP',
      providerName: '',
      description: '',
      baseUrl: '',
      apiVersion: '',
      authType: 'ApiKey',
      syncSchedule: '',
    },
  });

  // Reset form when initialData changes
  useEffect(() => {
    if (initialData) {
      reset({
        name: initialData.name,
        integrationType: initialData.integrationType,
        providerName: initialData.providerName,
        description: initialData.description || '',
        baseUrl: initialData.baseUrl || '',
        apiVersion: initialData.apiVersion || '',
        authType: initialData.authType || 'ApiKey',
        syncSchedule: initialData.syncSchedule || '',
      });
    }
  }, [initialData, reset]);

  const handleFormSubmit = handleSubmit(async (data) => {
    await onSubmit(data);
  });

  return (
    <form className="integration-form" onSubmit={handleFormSubmit}>
      {/* Section: Connection Details */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('integrations.connectionDetails', 'Connection Details')}</h3>
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
                placeholder="My ERP Integration"
                {...register('name', { required: t('validation.required', 'Required') })}
              />
              {errors.name && <span className="form-field__error">{errors.name.message}</span>}
            </div>

            <div className="form-field">
              <label className="form-field__label">
                {t('common.type', 'Type')} <span className="required">*</span>
              </label>
              <select
                className="form-field__select"
                disabled={isEditMode}
                {...register('integrationType')}
              >
                {INTEGRATION_TYPES.map((type) => (
                  <option key={type} value={type}>
                    {typeLabels[type]}
                  </option>
                ))}
              </select>
            </div>

            <div className="form-field">
              <label className="form-field__label">
                {t('integrations.provider', 'Provider')} <span className="required">*</span>
              </label>
              <input
                type="text"
                className={`form-field__input ${errors.providerName ? 'form-field__input--error' : ''}`}
                placeholder="SAP, Shopify, etc."
                disabled={isEditMode}
                {...register('providerName', { required: !isEditMode ? t('validation.required', 'Required') : false })}
              />
              {errors.providerName && <span className="form-field__error">{errors.providerName.message}</span>}
            </div>

            <div className="form-field form-field--full">
              <label className="form-field__label">{t('integrations.description', 'Description')}</label>
              <textarea
                className="form-field__textarea"
                rows={2}
                placeholder={t('integrations.descriptionPlaceholder', 'Optional description...')}
                {...register('description')}
              />
            </div>
          </div>
        </div>
      </section>

      {/* Section: API Configuration */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('integrations.apiConfiguration', 'API Configuration')}</h3>
        </div>
        <div className="form-section__content">
          <div className="form-grid form-grid--2col">
            <div className="form-field form-field--full">
              <label className="form-field__label">{t('integrations.apiEndpoint', 'API Endpoint')}</label>
              <input
                type="url"
                className="form-field__input"
                placeholder="https://api.example.com"
                {...register('baseUrl')}
              />
            </div>

            <div className="form-field">
              <label className="form-field__label">{t('integrations.apiVersion', 'API Version')}</label>
              <input
                type="text"
                className="form-field__input"
                placeholder="v1"
                {...register('apiVersion')}
              />
            </div>

            <div className="form-field">
              <label className="form-field__label">{t('integrations.authType', 'Auth Type')}</label>
              <select className="form-field__select" {...register('authType')}>
                <option value="ApiKey">API Key</option>
                <option value="OAuth2">OAuth 2.0</option>
                <option value="Basic">Basic Auth</option>
                <option value="Bearer">Bearer Token</option>
                <option value="Custom">Custom</option>
              </select>
            </div>
          </div>

          {isEditMode && onTestConnection && (
            <div className="form-actions" style={{ marginTop: '1rem' }}>
              <button
                type="button"
                className="btn btn--secondary"
                onClick={onTestConnection}
                disabled={isTesting}
              >
                {isTesting ? t('integrations.testing', 'Testing...') : t('integrations.testConnection', 'Test Connection')}
              </button>
              {testResult && (
                <span className={`test-result ${testResult.success ? 'test-result--success' : 'test-result--error'}`}>
                  {testResult.success ? 'Success: ' : 'Failed: '}{testResult.message}
                </span>
              )}
            </div>
          )}
        </div>
      </section>

      {/* Section: Sync Settings */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('integrations.syncSettings', 'Sync Settings')}</h3>
        </div>
        <div className="form-section__content">
          <div className="form-grid form-grid--2col">
            <div className="form-field">
              <label className="form-field__label">{t('integrations.syncSchedule', 'Sync Schedule')}</label>
              <input
                type="text"
                className="form-field__input"
                placeholder="0 */6 * * *"
                {...register('syncSchedule')}
              />
              <p className="form-field__hint">
                {t('integrations.cronHint', 'Cron expression (e.g., 0 */6 * * * for every 6 hours)')}
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* Section: Status (Edit mode only) */}
      {isEditMode && initialData && onToggleStatus && (
        <section className="form-section">
          <div className="form-section__header">
            <h3 className="form-section__title">{t('integrations.status', 'Status')}</h3>
          </div>
          <div className="form-section__content">
            <div className="form-grid form-grid--2col">
              <div className="form-field">
                <p>
                  {t('integrations.currentStatus', 'Current Status')}:{' '}
                  <span className={`status-badge status-badge--${statusClassMap[initialData.status] || 'neutral'}`}>
                    {initialData.status}
                  </span>
                </p>
                <button
                  type="button"
                  className={`btn ${initialData.status === 'Active' ? 'btn--warning' : 'btn--success'}`}
                  onClick={onToggleStatus}
                  style={{ marginTop: '1rem' }}
                >
                  {initialData.status === 'Active'
                    ? t('integrations.deactivate', 'Deactivate')
                    : t('integrations.activate', 'Activate')}
                </button>
              </div>
            </div>
          </div>
        </section>
      )}

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
              : t('integrations.addIntegration', 'Add Integration')}
        </button>
      </div>
    </form>
  );
}
