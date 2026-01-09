import { useState, useMemo } from 'react';
import { useIntl } from 'react-intl';
import { DataTable, createColumns } from '@/components/DataTable';
import { FullscreenModal, ModalSection } from '@/components/FullscreenModal';
import { useGetIntegrationsQuery } from '@/api/modules/integrations';
import type { IntegrationSummaryDto, IntegrationType, IntegrationStatus, SyncStatus } from '@/api/modules/integrations';
import './Integrations.scss';

const typeLabels: Record<IntegrationType, string> = {
  ERP: 'ERP',
  Ecommerce: 'E-commerce',
  Carrier: 'Carrier',
  Accounting: 'Accounting',
  CRM: 'CRM',
  Custom: 'Custom',
};

const statusClassMap: Record<IntegrationStatus, string> = {
  Active: 'active',
  Inactive: 'inactive',
  Error: 'error',
  Pending: 'warning',
  Disconnected: 'inactive',
};

/**
 * Integrations Module
 *
 * Manage external system connections and sync settings.
 */
export function Integrations() {
  const { formatMessage } = useIntl();
  const t = (id: string) => formatMessage({ id });

  const [searchQuery, setSearchQuery] = useState('');
  const [paginationState, setPaginationState] = useState({ page: 1, pageSize: 25 });
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [editingIntegration, setEditingIntegration] = useState<IntegrationSummaryDto | null>(null);

  const { data, isLoading } = useGetIntegrationsQuery({
    page: paginationState.page,
    pageSize: paginationState.pageSize,
    searchTerm: searchQuery || undefined,
  });

  const integrations = data?.data?.items ?? [];
  const totalRows = data?.data?.totalCount ?? 0;

  const columnHelper = createColumns<IntegrationSummaryDto>();

  const columns = useMemo(
    () => [
      columnHelper.accessor('name', {
        header: t('common.name'),
        size: 180,
        cell: ({ getValue }) => <strong>{getValue()}</strong>,
      }),
      columnHelper.accessor('integrationType', {
        header: t('common.type'),
        size: 120,
        cell: ({ getValue }) => (
          <span className="integration-type">{typeLabels[getValue()]}</span>
        ),
      }),
      columnHelper.accessor('providerName', {
        header: 'Provider',
        size: 140,
        cell: ({ getValue }) => getValue() || '—',
      }),
      columnHelper.accessor('status', {
        header: t('integrations.status'),
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
        header: t('integrations.lastSync'),
        size: 160,
        cell: ({ getValue }) => {
          const value = getValue();
          if (!value) return <span className="text-muted">Never</span>;
          return new Date(value).toLocaleString();
        },
      }),
      columnHelper.accessor('lastSyncStatus', {
        header: 'Sync Status',
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

  const handleSave = () => {
    setModalOpen(false);
  };

  return (
    <div className="integrations">
      <header className="integrations__header">
        <div className="integrations__title-section">
          <h1 className="integrations__title">{t('integrations.title')}</h1>
        </div>
        <div className="integrations__actions">
          <button className="btn btn-primary" onClick={handleAdd}>
            {t('common.add')}
          </button>
        </div>
      </header>

      <div className="integrations__toolbar">
        <div className="integrations__search">
          <input
            type="search"
            className="integrations__search-input"
            placeholder={t('common.search')}
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
          emptyMessage={t('common.noData')}
        />
      </div>

      <FullscreenModal
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        title={editingIntegration ? 'Edit Integration' : 'Add Integration'}
        subtitle={editingIntegration ? `Editing ${editingIntegration.name}` : 'Configure external system connection'}
        onSave={handleSave}
        maxWidth="lg"
      >
        <ModalSection title="Connection Details">
          <div className="form-grid">
            <div className="form-field">
              <label className="form-field__label">{t('common.name')}</label>
              <input
                type="text"
                className="form-field__input"
                defaultValue={editingIntegration?.name}
                placeholder="Integration name"
              />
            </div>
            <div className="form-field">
              <label className="form-field__label">{t('common.type')}</label>
              <select className="form-field__select" defaultValue={editingIntegration?.integrationType || ''}>
                <option value="">Select type...</option>
                <option value="ERP">ERP</option>
                <option value="Ecommerce">E-commerce</option>
                <option value="Carrier">Carrier</option>
                <option value="Accounting">Accounting</option>
                <option value="CRM">CRM</option>
                <option value="Custom">Custom</option>
              </select>
            </div>
          </div>
        </ModalSection>

        <ModalSection title="API Configuration">
          <div className="form-grid--single">
            <div className="form-field">
              <label className="form-field__label">{t('integrations.apiEndpoint')}</label>
              <input
                type="url"
                className="form-field__input"
                placeholder="https://api.example.com"
              />
            </div>
          </div>
          <div className="form-grid">
            <div className="form-field">
              <label className="form-field__label">API Key</label>
              <input
                type="password"
                className="form-field__input"
                placeholder="Enter API key"
              />
            </div>
            <div className="form-field">
              <label className="form-field__label">API Secret</label>
              <input
                type="password"
                className="form-field__input"
                placeholder="Enter API secret"
              />
            </div>
          </div>
        </ModalSection>

        <ModalSection title="Sync Settings" description="Configure synchronization frequency" collapsible defaultExpanded={false}>
          <div className="form-grid">
            <div className="form-field">
              <label className="form-field__label">Sync Frequency</label>
              <select className="form-field__select" defaultValue="30">
                <option value="5">Every 5 minutes</option>
                <option value="15">Every 15 minutes</option>
                <option value="30">Every 30 minutes</option>
                <option value="60">Every hour</option>
                <option value="1440">Daily</option>
              </select>
            </div>
            <div className="form-field">
              <label className="form-field__label">Retry Attempts</label>
              <input
                type="number"
                className="form-field__input"
                defaultValue={3}
                min={1}
                max={10}
              />
            </div>
          </div>
        </ModalSection>
      </FullscreenModal>
    </div>
  );
}

export default Integrations;
