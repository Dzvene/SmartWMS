import { useState, useMemo } from 'react';
import { useIntl } from 'react-intl';
import { DataTable, createColumns } from '@/components/DataTable';
import type { PaginationState, SortingState } from '@/components/DataTable';
import { useGetSuppliersQuery } from '@/api/modules/orders';
import type { SupplierDto } from '@/api/modules/orders';
import './Suppliers.scss';

export function Suppliers() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });

  const [searchQuery, setSearchQuery] = useState('');
  const [activeFilter, setActiveFilter] = useState<'' | 'true' | 'false'>('');
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 });
  const [sorting, setSorting] = useState<SortingState>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const { data: response, isLoading } = useGetSuppliersQuery({
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
    search: searchQuery || undefined,
    isActive: activeFilter === '' ? undefined : activeFilter === 'true',
  });

  const suppliers = response?.data?.items || [];
  const totalRows = response?.data?.totalCount || 0;

  const columnHelper = createColumns<SupplierDto>();

  const columns = useMemo(
    () => [
      columnHelper.accessor('code', {
        header: t('suppliers.code', 'Code'),
        size: 100,
        cell: ({ getValue }) => <span className="code">{getValue()}</span>,
      }),
      columnHelper.accessor('name', {
        header: t('suppliers.name', 'Name'),
        size: 180,
      }),
      columnHelper.accessor('contactName', {
        header: t('suppliers.contact', 'Contact'),
        size: 140,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('email', {
        header: t('suppliers.email', 'Email'),
        size: 180,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('phone', {
        header: t('suppliers.phone', 'Phone'),
        size: 120,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('leadTimeDays', {
        header: t('suppliers.leadTime', 'Lead Time'),
        size: 100,
        cell: ({ getValue }) => {
          const days = getValue();
          return days ? `${days} days` : '-';
        },
      }),
      columnHelper.accessor('orderCount', {
        header: t('suppliers.orders', 'Orders'),
        size: 80,
      }),
      columnHelper.accessor('isActive', {
        header: t('common.status', 'Status'),
        size: 100,
        cell: ({ getValue }) => {
          const isActive = getValue();
          return (
            <span className={`status-badge status-badge--${isActive ? 'active' : 'inactive'}`}>
              {isActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
            </span>
          );
        },
      }),
    ],
    [columnHelper, t]
  );

  const handleRowClick = (supplier: SupplierDto) => {
    setSelectedId(supplier.id);
  };

  return (
    <div className="suppliers">
      <header className="suppliers__header">
        <div className="suppliers__title-section">
          <h1 className="suppliers__title">{t('suppliers.title', 'Suppliers')}</h1>
          <p className="suppliers__subtitle">
            {t('suppliers.subtitle', 'Manage supplier accounts')}
          </p>
        </div>
        <div className="suppliers__actions">
          <button className="btn btn--primary">
            {t('suppliers.addSupplier', 'Add Supplier')}
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
            <option value="">{t('suppliers.allStatuses', 'All')}</option>
            <option value="true">{t('common.active', 'Active')}</option>
            <option value="false">{t('common.inactive', 'Inactive')}</option>
          </select>
        </div>
      </div>

      <div className="suppliers__content">
        <DataTable
          data={suppliers}
          columns={columns}
          pagination={pagination}
          onPaginationChange={setPagination}
          sorting={sorting}
          onSortingChange={setSorting}
          totalRows={totalRows}
          selectedRowId={selectedId}
          onRowClick={handleRowClick}
          getRowId={(row) => row.id}
          emptyMessage={t('suppliers.noSuppliers', 'No suppliers found')}
          loading={isLoading}
        />
      </div>
    </div>
  );
}

export default Suppliers;
