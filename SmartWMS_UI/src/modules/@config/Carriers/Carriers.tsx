import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslate } from '@/hooks';
import { DataTable, createColumns } from '@/components/DataTable';
import type { PaginationState, SortingState } from '@/components/DataTable';
import { useGetCarriersQuery } from '@/api/modules/carriers';
import type { CarrierListDto, CarrierIntegrationType } from '@/api/modules/carriers';
import { CONFIG } from '@/constants/routes';

const INTEGRATION_LABELS: Record<CarrierIntegrationType, string> = {
  Manual: 'Manual',
  API: 'API',
  EDI: 'EDI',
  File: 'File Import',
};

export function Carriers() {
  const t = useTranslate();
  const navigate = useNavigate();

  const [searchQuery, setSearchQuery] = useState('');
  const [activeFilter, setActiveFilter] = useState<'' | 'true' | 'false'>('');
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 });
  const [sorting, setSorting] = useState<SortingState>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const { data: response, isLoading } = useGetCarriersQuery({
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
    search: searchQuery || undefined,
    isActive: activeFilter === '' ? undefined : activeFilter === 'true',
  });

  const carriers = response?.data?.items || [];
  const totalRows = response?.data?.totalCount || 0;

  const columnHelper = createColumns<CarrierListDto>();

  const columns = useMemo(
    () => [
      columnHelper.accessor('code', {
        header: t('carriers.carrierCode', 'Code'),
        size: 100,
        cell: ({ getValue }) => (
          <span className="carriers__code">{getValue()}</span>
        ),
      }),
      columnHelper.accessor('name', {
        header: t('carriers.carrierName', 'Name'),
        size: 180,
        cell: ({ getValue }) => (
          <span className="carriers__name">{getValue()}</span>
        ),
      }),
      columnHelper.accessor('integrationType', {
        header: t('carriers.integrationType', 'Integration'),
        size: 120,
        cell: ({ getValue }) => {
          const type = getValue();
          return INTEGRATION_LABELS[type] || type;
        },
      }),
      columnHelper.accessor('serviceCount', {
        header: t('carriers.services', 'Services'),
        size: 100,
        cell: ({ getValue }) => `${getValue()} services`,
      }),
      columnHelper.accessor('isActive', {
        header: t('common.status', 'Status'),
        size: 100,
        cell: ({ getValue }) => {
          const isActive = getValue();
          return (
            <span className={`status-badge status-badge--${isActive ? 'success' : 'neutral'}`}>
              {isActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
            </span>
          );
        },
      }),
      columnHelper.accessor('createdAt', {
        header: t('common.createdAt', 'Created'),
        size: 110,
        cell: ({ getValue }) => new Date(getValue()).toLocaleDateString(),
      }),
    ],
    [columnHelper, t]
  );

  const handleRowClick = (carrier: CarrierListDto) => {
    setSelectedId(carrier.id);
    navigate(`${CONFIG.CARRIERS}/${carrier.id}`);
  };

  const handleCreateCarrier = () => {
    navigate(CONFIG.CARRIER_CREATE);
  };

  return (
    <div className="page">
      <header className="page__header">
        <div className="page__title-section">
          <h1 className="page__title">{t('carriers.title', 'Carriers')}</h1>
          <p className="page__subtitle">
            {t('carriers.subtitle', 'Manage shipping carriers and services')}
          </p>
        </div>
        <div className="page__actions">
          <button className="btn btn--primary" onClick={handleCreateCarrier}>
            {t('carriers.addCarrier', 'Add Carrier')}
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
        <div className="page-filters">
          <select
            className="page-filter__select"
            value={activeFilter}
            onChange={(e) => setActiveFilter(e.target.value as '' | 'true' | 'false')}
          >
            <option value="">{t('carriers.allStatuses', 'All Statuses')}</option>
            <option value="true">{t('common.active', 'Active')}</option>
            <option value="false">{t('common.inactive', 'Inactive')}</option>
          </select>
        </div>
      </div>

      <div className="page__content">
        <DataTable
          data={carriers}
          columns={columns}
          pagination={pagination}
          onPaginationChange={setPagination}
          sorting={sorting}
          onSortingChange={setSorting}
          totalRows={totalRows}
          selectedRowId={selectedId}
          onRowClick={handleRowClick}
          getRowId={(row) => row.id}
          emptyMessage={t('carriers.noCarriers', 'No carriers found')}
          loading={isLoading}
        />
      </div>
    </div>
  );
}

export default Carriers;
