import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslate } from '@/hooks';
import { DataTable, createColumns } from '@/components/DataTable';
import type { PaginationState, SortingState } from '@/components/DataTable';
import { useGetCustomersQuery } from '@/api/modules/orders';
import type { CustomerDto } from '@/api/modules/orders';
import { ORDERS } from '@/constants/routes';
import './Customers.scss';

export function Customers() {
  const t = useTranslate();
  const navigate = useNavigate();

  const [searchQuery, setSearchQuery] = useState('');
  const [activeFilter, setActiveFilter] = useState<'' | 'true' | 'false'>('');
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 });
  const [sorting, setSorting] = useState<SortingState>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const { data: response, isLoading } = useGetCustomersQuery({
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
    search: searchQuery || undefined,
    isActive: activeFilter === '' ? undefined : activeFilter === 'true',
  });

  const customers = response?.data?.items || [];
  const totalRows = response?.data?.totalCount || 0;

  const columnHelper = createColumns<CustomerDto>();

  const columns = useMemo(
    () => [
      columnHelper.accessor('code', {
        header: t('customers.code', 'Code'),
        size: 100,
        cell: ({ getValue }) => (
          <span className="customers__code">{getValue()}</span>
        ),
      }),
      columnHelper.accessor('name', {
        header: t('customers.name', 'Name'),
        size: 180,
        cell: ({ getValue }) => (
          <span className="customers__name">{getValue()}</span>
        ),
      }),
      columnHelper.accessor('contactName', {
        header: t('customers.contact', 'Contact'),
        size: 140,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('email', {
        header: t('customers.email', 'Email'),
        size: 180,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('phone', {
        header: t('customers.phone', 'Phone'),
        size: 120,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('city', {
        header: t('customers.city', 'City'),
        size: 120,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('orderCount', {
        header: t('customers.orders', 'Orders'),
        size: 80,
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
    ],
    [columnHelper, t]
  );

  const handleRowClick = (customer: CustomerDto) => {
    setSelectedId(customer.id);
    navigate(`${ORDERS.CUSTOMERS}/${customer.id}`);
  };

  const handleCreateCustomer = () => {
    navigate(ORDERS.CUSTOMER_CREATE);
  };

  return (
    <div className="page">
      <header className="page__header">
        <div className="page__title-section">
          <h1 className="page__title">{t('customers.title', 'Customers')}</h1>
          <p className="page__subtitle">
            {t('customers.subtitle', 'Manage customer accounts and information')}
          </p>
        </div>
        <div className="page__actions">
          <button className="btn btn--primary" onClick={handleCreateCustomer}>
            {t('customers.createCustomer', 'Create Customer')}
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
            <option value="">{t('customers.allStatuses', 'All Statuses')}</option>
            <option value="true">{t('common.active', 'Active')}</option>
            <option value="false">{t('common.inactive', 'Inactive')}</option>
          </select>
        </div>
      </div>

      <div className="page__content">
        <DataTable
          data={customers}
          columns={columns}
          pagination={pagination}
          onPaginationChange={setPagination}
          sorting={sorting}
          onSortingChange={setSorting}
          totalRows={totalRows}
          selectedRowId={selectedId}
          onRowClick={handleRowClick}
          getRowId={(row) => row.id}
          emptyMessage={t('customers.noCustomers', 'No customers found')}
          loading={isLoading}
        />
      </div>
    </div>
  );
}

export default Customers;
