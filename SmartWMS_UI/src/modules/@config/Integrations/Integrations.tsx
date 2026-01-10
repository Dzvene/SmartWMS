import { useState, useMemo, useEffect } from 'react';
import { useIntl } from 'react-intl';
import { useForm } from 'react-hook-form';
import { DataTable, createColumns } from '@/components/DataTable';
import { FullscreenModal, ModalSection } from '@/components/FullscreenModal';
import { Modal } from '@/components';
import {
  useGetIntegrationsQuery,
  useCreateIntegrationMutation,
  useUpdateIntegrationMutation,
  useDeleteIntegrationMutation,
  useActivateIntegrationMutation,
  useDeactivateIntegrationMutation,
  useTestIntegrationConnectionMutation,
} from '@/api/modules/integrations';
import type {
  IntegrationSummaryDto,
  IntegrationType,
  IntegrationStatus,
  SyncStatus,
  CreateIntegrationRequest,
  UpdateIntegrationRequest,
} from '@/api/modules/integrations';
import './Integrations.scss';

const typeLabels: Record<IntegrationType, string> = {
  ERP: 'ERP',
  Ecommerce: 'E-commerce',
  Carrier: 'Carrier',
  Accounting: 'Accounting',
  CRM: 'CRM',
  Custom: 'Custom',
};

const INTEGRATION_TYPES: IntegrationType[] = ['ERP', 'Ecommerce', 'Carrier', 'Accounting', 'CRM', 'Custom'];

const statusClassMap: Record<IntegrationStatus, string> = {
  Active: 'active',
  Inactive: 'inactive',
  Error: 'error',
  Pending: 'warning',
  Disconnected: 'inactive',
};

interface IntegrationFormData {
  name: string;
  integrationType: IntegrationType;
  providerName: string;
  description: string;
  baseUrl: string;
  apiVersion: string;
  authType: string;
  syncSchedule: string;
}

/**
 * Integrations Module
 *
 * Manage external system connections and sync settings.
 */
export function Integrations() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });

  const [searchQuery, setSearchQuery] = useState('');
  const [paginationState, setPaginationState] = useState({ page: 1, pageSize: 25 });
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [editingIntegration, setEditingIntegration] = useState<IntegrationSummaryDto | null>(null);
  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);
  const [testResult, setTestResult] = useState<{ success: boolean; message: string } | null>(null);

  // API
  const { data, isLoading } = useGetIntegrationsQuery({
    page: paginationState.page,
    pageSize: paginationState.pageSize,
    searchTerm: searchQuery || undefined,
  });

  const integrations = data?.data?.items ?? [];
  const totalRows = data?.data?.totalCount ?? 0;

  const [createIntegration, { isLoading: isCreating }] = useCreateIntegrationMutation();
  const [updateIntegration, { isLoading: isUpdating }] = useUpdateIntegrationMutation();
  const [deleteIntegration, { isLoading: isDeleting }] = useDeleteIntegrationMutation();
  const [activateIntegration] = useActivateIntegrationMutation();
  const [deactivateIntegration] = useDeactivateIntegrationMutation();
  const [testConnection, { isLoading: isTesting }] = useTestIntegrationConnectionMutation();

  // Form
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
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

  // Reset form when editing changes
  useEffect(() => {
    if (editingIntegration) {
      reset({
        name: editingIntegration.name,
        integrationType: editingIntegration.integrationType,
        providerName: editingIntegration.providerName,
        description: '',
        baseUrl: '',
        apiVersion: '',
        authType: 'ApiKey',
        syncSchedule: '',
      });
    } else {
      reset({
        name: '',
        integrationType: 'ERP',
        providerName: '',
        description: '',
        baseUrl: '',
        apiVersion: '',
        authType: 'ApiKey',
        syncSchedule: '',
      });
    }
    setTestResult(null);
  }, [editingIntegration, reset]);

  const columnHelper = createColumns<IntegrationSummaryDto>();

  const columns = useMemo(
    () => [
      columnHelper.accessor('name', {
        header: t('common.name', 'Name'),
        size: 180,
        cell: ({ getValue }) => <strong>{getValue()}</strong>,
      }),
      columnHelper.accessor('integrationType', {
        header: t('common.type', 'Type'),
        size: 120,
        cell: ({ getValue }) => (
          <span className="integration-type">{typeLabels[getValue()]}</span>
        ),
      }),
      columnHelper.accessor('providerName', {
        header: t('integrations.provider', 'Provider'),
        size: 140,
        cell: ({ getValue }) => getValue() || '—',
      }),
      columnHelper.accessor('status', {
        header: t('integrations.status', 'Status'),
        size: 120,
        cell: ({ getValue }) => {
          const status = getValue();
          return (
            <span className={`status-badge status-badge--${statusClassMap[status]}`}>
              {status}
            </span>
          );
        },
      }),
      columnHelper.accessor('lastSyncAt', {
        header: t('integrations.lastSync', 'Last Sync'),
        size: 160,
        cell: ({ getValue }) => {
          const value = getValue();
          if (!value) return <span className="text-muted">Never</span>;
          return new Date(value).toLocaleString();
        },
      }),
      columnHelper.accessor('lastSyncStatus', {
        header: t('integrations.syncStatus', 'Sync Status'),
        size: 120,
        cell: ({ getValue }) => {
          const status = getValue() as SyncStatus | undefined;
          if (!status) return <span className="text-muted">—</span>;
          const statusClass = status === 'Completed' ? 'active' : status === 'Failed' ? 'error' : 'warning';
          return <span className={`status-badge status-badge--${statusClass}`}>{status}</span>;
        },
      }),
    ],
    [columnHelper, t]
  );

  const handleAdd = () => {
    setEditingIntegration(null);
    setModalOpen(true);
  };

  const handleRowClick = (integration: IntegrationSummaryDto) => {
    setSelectedId(integration.id);
    setEditingIntegration(integration);
    setModalOpen(true);
  };

  const handleCloseModal = () => {
    setModalOpen(false);
    setEditingIntegration(null);
    setTestResult(null);
  };

  const onSubmit = async (data: IntegrationFormData) => {
    try {
      if (editingIntegration) {
        const updateData: UpdateIntegrationRequest = {
          name: data.name,
          description: data.description || undefined,
          baseUrl: data.baseUrl || undefined,
          apiVersion: data.apiVersion || undefined,
          syncSchedule: data.syncSchedule || undefined,
        };
        await updateIntegration({ id: editingIntegration.id, data: updateData }).unwrap();
      } else {
        const createData: CreateIntegrationRequest = {
          name: data.name,
          integrationType: data.integrationType,
          providerName: data.providerName,
          description: data.description || undefined,
          baseUrl: data.baseUrl || undefined,
          apiVersion: data.apiVersion || undefined,
          authType: data.authType || undefined,
          syncSchedule: data.syncSchedule || undefined,
        };
        await createIntegration(createData).unwrap();
      }
      handleCloseModal();
    } catch (error) {
      console.error('Failed to save integration:', error);
    }
  };

  const handleDelete = async () => {
    if (!editingIntegration) return;

    try {
      await deleteIntegration(editingIntegration.id).unwrap();
      setDeleteConfirmOpen(false);
      handleCloseModal();
    } catch (error) {
      console.error('Failed to delete integration:', error);
    }
  };

  const handleToggleStatus = async () => {
    if (!editingIntegration) return;

    try {
      if (editingIntegration.status === 'Active') {
        await deactivateIntegration(editingIntegration.id).unwrap();
      } else {
        await activateIntegration(editingIntegration.id).unwrap();
      }
      handleCloseModal();
    } catch (error) {
      console.error('Failed to toggle integration status:', error);
    }
  };

  const handleTestConnection = async () => {
    if (!editingIntegration) return;

    try {
      const result = await testConnection(editingIntegration.id).unwrap();
      setTestResult(result.data || { success: false, message: 'Unknown error' });
    } catch (error) {
      setTestResult({ success: false, message: 'Connection test failed' });
    }
  };

  const isSaving = isCreating || isUpdating;

  return (
    <div className="integrations">
      <header className="integrations__header">
        <div className="integrations__title-section">
          <h1 className="integrations__title">{t('integrations.title', 'Integrations')}</h1>
          <p className="integrations__subtitle">{t('integrations.subtitle', 'Connect external systems')}</p>
        </div>
        <div className="integrations__actions">
          <button className="btn btn--primary" onClick={handleAdd}>
            {t('integrations.addIntegration', 'Add Integration')}
          </button>
        </div>
      </header>

      <div className="integrations__toolbar">
        <div className="integrations__search">
          <input
            type="search"
            className="integrations__search-input"
            placeholder={t('common.search', 'Search...')}
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
        </div>
      </div>

      <div className="integrations__content">
        <DataTable
          data={integrations}
          columns={columns}
          pagination={{
            pageIndex: paginationState.page - 1,
            pageSize: paginationState.pageSize,
          }}
          onPaginationChange={({ pageIndex, pageSize }) => {
            setPaginationState({ page: pageIndex + 1, pageSize });
          }}
          totalRows={totalRows}
          selectedRowId={selectedId}
          onRowClick={handleRowClick}
          getRowId={(row) => row.id}
          loading={isLoading}
          emptyMessage={t('common.noData', 'No data found')}
        />
      </div>

      {/* Integration Form Modal */}
      <FullscreenModal
        open={modalOpen}
        onClose={handleCloseModal}
        title={editingIntegration ? t('integrations.editIntegration', 'Edit Integration') : t('integrations.addIntegration', 'Add Integration')}
        subtitle={editingIntegration ? `${editingIntegration.providerName} - ${editingIntegration.name}` : t('integrations.configureConnection', 'Configure external system connection')}
        onSave={handleSubmit(onSubmit)}
        saveLabel={isSaving ? t('common.saving', 'Saving...') : t('common.save', 'Save')}
        saveDisabled={isSaving}
        maxWidth="lg"
      >
        <form>
          <ModalSection title={t('integrations.connectionDetails', 'Connection Details')}>
            <div className="form-grid">
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
                  disabled={!!editingIntegration}
                  {...register('integrationType')}
                >
                  {INTEGRATION_TYPES.map(type => (
                    <option key={type} value={type}>{typeLabels[type]}</option>
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
                  disabled={!!editingIntegration}
                  {...register('providerName', { required: !editingIntegration ? t('validation.required', 'Required') : false })}
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
          </ModalSection>

          <ModalSection title={t('integrations.apiConfiguration', 'API Configuration')}>
            <div className="form-grid">
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

            {editingIntegration && (
              <div className="form-actions" style={{ marginTop: '1rem' }}>
                <button
                  type="button"
                  className="btn btn--secondary"
                  onClick={handleTestConnection}
                  disabled={isTesting}
                >
                  {isTesting ? t('integrations.testing', 'Testing...') : t('integrations.testConnection', 'Test Connection')}
                </button>
                {testResult && (
                  <span className={`test-result ${testResult.success ? 'test-result--success' : 'test-result--error'}`}>
                    {testResult.success ? '✓ ' : '✗ '}{testResult.message}
                  </span>
                )}
              </div>
            )}
          </ModalSection>

          <ModalSection title={t('integrations.syncSettings', 'Sync Settings')} collapsible defaultExpanded={false}>
            <div className="form-grid">
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
          </ModalSection>

          {editingIntegration && (
            <>
              <ModalSection title={t('integrations.status', 'Status')}>
                <div className="form-grid">
                  <div className="form-field">
                    <p>
                      {t('integrations.currentStatus', 'Current Status')}: {' '}
                      <span className={`status-badge status-badge--${statusClassMap[editingIntegration.status]}`}>
                        {editingIntegration.status}
                      </span>
                    </p>
                    <button
                      type="button"
                      className={`btn ${editingIntegration.status === 'Active' ? 'btn--warning' : 'btn--success'}`}
                      onClick={handleToggleStatus}
                      style={{ marginTop: '1rem' }}
                    >
                      {editingIntegration.status === 'Active'
                        ? t('integrations.deactivate', 'Deactivate')
                        : t('integrations.activate', 'Activate')}
                    </button>
                  </div>
                </div>
              </ModalSection>

              <ModalSection title={t('common.dangerZone', 'Danger Zone')}>
                <div className="danger-zone">
                  <p>{t('integrations.deleteWarning', 'Deleting an integration will remove all sync history and webhooks.')}</p>
                  <button
                    type="button"
                    className="btn btn--danger"
                    onClick={() => setDeleteConfirmOpen(true)}
                  >
                    {t('integrations.deleteIntegration', 'Delete Integration')}
                  </button>
                </div>
              </ModalSection>
            </>
          )}
        </form>
      </FullscreenModal>

      {/* Delete Confirmation Modal */}
      <Modal
        open={deleteConfirmOpen}
        onClose={() => setDeleteConfirmOpen(false)}
        title={t('integrations.deleteIntegration', 'Delete Integration')}
      >
        <div className="modal__body">
          <p>
            {t(
              'integrations.deleteConfirmation',
              `Are you sure you want to delete "${editingIntegration?.name}"? This will remove all sync history and webhooks.`
            )}
          </p>
        </div>
        <div className="modal__actions">
          <button className="btn btn-ghost" onClick={() => setDeleteConfirmOpen(false)}>
            {t('common.cancel', 'Cancel')}
          </button>
          <button className="btn btn--danger" onClick={handleDelete} disabled={isDeleting}>
            {isDeleting ? t('common.deleting', 'Deleting...') : t('common.delete', 'Delete')}
          </button>
        </div>
      </Modal>
    </div>
  );
}

export default Integrations;
