import { useState, useMemo } from 'react';
import { useIntl } from 'react-intl';
import type { ColumnDef } from '@tanstack/react-table';

import { DataTable } from '@/components/DataTable';
import { FullscreenModal } from '@/components/FullscreenModal';
import { useGetCarriersQuery } from '@/api/modules/carriers';
import type { CarrierListDto, CarrierIntegrationType } from '@/api/modules/carriers';

const INTEGRATION_LABELS: Record<CarrierIntegrationType, string> = {
  Manual: 'Manual',
  API: 'API',
  EDI: 'EDI',
  File: 'File Import',
};

/**
 * Carriers Configuration Module
 *
 * Manages shipping carriers and their services.
 */
export function Carriers() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });

  const [filters, setFilters] = useState({
    page: 1,
    pageSize: 20,
    search: '',
    isActive: undefined as boolean | undefined,
  });

  const { data: response, isLoading } = useGetCarriersQuery(filters);
  const carriers = response?.data?.items || [];
  const totalCount = response?.data?.totalCount || 0;

  const [selectedCarrier, setSelectedCarrier] = useState<CarrierListDto | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  const columns = useMemo<ColumnDef<CarrierListDto, unknown>[]>(
    () => [
      {
        accessorKey: 'code',
        header: t('carriers.carrierCode', 'Code'),
        size: 100,
        cell: ({ getValue }) => (
          <span className="code">{getValue() as string}</span>
        ),
      },
      {
        accessorKey: 'name',
        header: t('carriers.carrierName', 'Name'),
        size: 180,
      },
      {
        accessorKey: 'integrationType',
        header: t('carriers.integrationType', 'Integration'),
        size: 120,
        cell: ({ getValue }) => {
          const type = getValue() as CarrierIntegrationType;
          return INTEGRATION_LABELS[type] || type;
        },
      },
      {
        accessorKey: 'serviceCount',
        header: t('carriers.services', 'Services'),
        size: 100,
        meta: { align: 'right' },
        cell: ({ getValue }) => `${getValue()} services`,
      },
      {
        accessorKey: 'isActive',
        header: t('common.status', 'Status'),
        size: 80,
        cell: ({ getValue }) => (
          <span className={`status-badge status-badge--${getValue() ? 'active' : 'inactive'}`}>
            {getValue() ? t('status.active', 'Active') : t('status.inactive', 'Inactive')}
          </span>
        ),
      },
      {
        accessorKey: 'createdAt',
        header: t('common.createdAt', 'Created'),
        size: 110,
        cell: ({ getValue }) => new Date(getValue() as string).toLocaleDateString(),
      },
    ],
    [t]
  );

  const handleRowClick = (carrier: CarrierListDto) => {
    setSelectedCarrier(carrier);
    setIsModalOpen(true);
  };

  const handleAddNew = () => {
    setSelectedCarrier(null);
    setIsModalOpen(true);
  };

  const handlePageChange = (page: number) => {
    setFilters((prev) => ({ ...prev, page }));
  };

  const handleSearch = (value: string) => {
    setFilters((prev) => ({ ...prev, search: value, page: 1 }));
  };

  return (
    <div className="page">
      <header className="page__header">
        <div className="page__title-section">
          <h1 className="page__title">{t('carriers.title', 'Carriers')}</h1>
          <p className="page__subtitle">{t('carriers.subtitle', 'Manage shipping carriers and services')}</p>
        </div>
        <div className="page__actions">
          <button className="btn btn--primary" onClick={handleAddNew}>
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
            value={filters.search}
            onChange={(e) => handleSearch(e.target.value)}
          />
        </div>
        <div className="page-filters">
          <select
            className="page-filter__select"
            value={filters.isActive === undefined ? '' : filters.isActive.toString()}
            onChange={(e) =>
              setFilters((prev) => ({
                ...prev,
                isActive: e.target.value === '' ? undefined : e.target.value === 'true',
                page: 1,
              }))
            }
          >
            <option value="">{t('carriers.allStatuses', 'All')}</option>
            <option value="true">{t('status.active', 'Active')}</option>
            <option value="false">{t('status.inactive', 'Inactive')}</option>
          </select>
        </div>
      </div>

      <div className="page__content">
        <DataTable
          data={carriers}
          columns={columns}
          pagination={{
            pageIndex: filters.page - 1,
            pageSize: filters.pageSize,
          }}
          onPaginationChange={({ pageIndex }) => handlePageChange(pageIndex + 1)}
          totalRows={totalCount}
          onRowClick={handleRowClick}
          selectedRowId={selectedCarrier?.id}
          getRowId={(row) => row.id}
          emptyMessage={t('carriers.noCarriers', 'No carriers found')}
          loading={isLoading}
        />
      </div>

      <FullscreenModal
        open={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={selectedCarrier ? `Edit ${selectedCarrier.name}` : t('carriers.addCarrier', 'Add Carrier')}
      >
        <div className="form">
          {selectedCarrier ? (
            <div className="carrier-details">
              <div className="form-group">
                <label>{t('carriers.carrierCode', 'Code')}</label>
                <p>{selectedCarrier.code}</p>
              </div>
              <div className="form-group">
                <label>{t('carriers.carrierName', 'Name')}</label>
                <p>{selectedCarrier.name}</p>
              </div>
              <div className="form-group">
                <label>{t('carriers.integrationType', 'Integration Type')}</label>
                <p>{INTEGRATION_LABELS[selectedCarrier.integrationType]}</p>
              </div>
              <div className="form-group">
                <label>{t('carriers.services', 'Services')}</label>
                <p>{selectedCarrier.serviceCount} services configured</p>
              </div>
              <div className="form-group">
                <label>{t('common.status', 'Status')}</label>
                <p>{selectedCarrier.isActive ? 'Active' : 'Inactive'}</p>
              </div>
            </div>
          ) : (
            <p>{t('carriers.formPlaceholder', 'Carrier configuration form will be implemented here.')}</p>
          )}
        </div>
      </FullscreenModal>
    </div>
  );
}

export default Carriers;
