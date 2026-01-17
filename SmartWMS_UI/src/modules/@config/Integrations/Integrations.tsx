import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslate } from '@/hooks';
import { DataTable, createColumns } from '@/components/DataTable';
import type { PaginationState, SortingState } from '@/components/DataTable';
import { useGetIntegrationsQuery } from '@/api/modules/integrations';
import type { IntegrationSummaryDto, IntegrationType, IntegrationStatus, SyncStatus } from '@/api/modules/integrations';
import { CONFIG } from '@/constants/routes';
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
  Active: 'success',
  Inactive: 'neutral',
  Error: 'error',
  Pending: 'warning',
  Disconnected: 'neutral',
};

/**
 * Integrations Module
 *
 * Manage external system connections and sync settings.
 */
export function Integrations() {
  const t = useTranslate();
  const navigate = useNavigate();

  const [searchQuery, setSearchQuery] = useState('');
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 });
  const [sorting, setSorting] = useState<SortingState>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const { data: response, isLoading } = useGetIntegrationsQuery({
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
    searchTerm: searchQuery || undefined,
  });

  const integrations = response?.data?.items || [];
  const totalRows = response?.data?.totalCount || 0;

  const columnHelper = createColumns<IntegrationSummaryDto>();

  const columns = useMemo(
    () => [
      columnHelper.accessor('name', {
        header: t('common.name', 'Name'),
        size: 180,
        cell: ({ getValue }) => (
          <span className="integrations__name">{getValue()}</span>
        ),
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
        cell: ({ getValue }) => getValue() || '-',
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
          if (!status) return <span className="text-muted">-</span>;
          const statusClass = status === 'Completed' ? 'success' : status === 'Failed' ? 'error' : 'warning';
          return <span className={`status-badge status-badge--${statusClass}`}>{status}</span>;
        },
      }),
    ],
    [columnHelper, t]
  );

  const handleRowClick = (integration: IntegrationSummaryDto) => {
    setSelectedId(integration.id);
    navigate(`${CONFIG.INTEGRATIONS}/${integration.id}`);
  };

  const handleCreateIntegration = () => {
    navigate(CONFIG.INTEGRATION_CREATE);
  };

  return (
    <div className="page">
      <header className="page__header">
        <div className="page__title-section">
          <h1 className="page__title">{t('integrations.title', 'Integrations')}</h1>
          <p className="page__subtitle">
            {t('integrations.subtitle', 'Connect external systems')}
          </p>
        </div>
        <div className="page__actions">
          <button className="btn btn--primary" onClick={handleCreateIntegration}>
            {t('integrations.addIntegration', 'Add Integration')}
          </button>
        </div>
      </header>

      <div className="page-toolbar">
        <div className="page-search">
          <input
            type="text"
            className="page-search__input"
            placeholder={t('common.search', 'Search...')}
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
        </div>
      </div>

      <div className="page__content">
        <DataTable
          data={integrations}
          columns={columns}
          pagination={pagination}
          onPaginationChange={setPagination}
          sorting={sorting}
          onSortingChange={setSorting}
          totalRows={totalRows}
          selectedRowId={selectedId}
          onRowClick={handleRowClick}
          getRowId={(row) => row.id}
          emptyMessage={t('integrations.noIntegrations', 'No integrations found')}
          loading={isLoading}
        />
      </div>
    </div>
  );
}

export default Integrations;
