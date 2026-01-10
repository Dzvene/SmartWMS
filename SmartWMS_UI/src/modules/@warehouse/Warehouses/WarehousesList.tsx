import { useState } from 'react';
import { useIntl } from 'react-intl';
import { useNavigate } from 'react-router-dom';
import { useGetWarehousesQuery } from '@/api/modules/warehouses';
import type { WarehouseDto } from '@/api/modules/warehouses';
import { DataTable, type ColumnDef } from '@/components/DataTable';
import { WAREHOUSE } from '@/constants/routes';
import './Warehouses.scss';

export function WarehousesList() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();

  const [searchQuery, setSearchQuery] = useState('');

  // tenantId is automatically injected by baseApi
  const { data: warehousesResponse, isLoading, isFetching } = useGetWarehousesQuery(
    { search: searchQuery || undefined }
  );

  const warehouses = warehousesResponse?.data?.items || [];

  const columns: ColumnDef<WarehouseDto>[] = [
    {
      id: 'code',
      header: t('warehouse.code', 'Code'),
      accessorKey: 'code',
      size: 120,
      cell: ({ row }) => <span className="code">{row.original.code}</span>,
    },
    {
      id: 'name',
      header: t('warehouse.name', 'Name'),
      accessorKey: 'name',
      cell: ({ row }) => (
        <div className="warehouse-name-cell">
          <strong>{row.original.name}</strong>
          {row.original.isPrimary && (
            <span className="badge badge--primary">{t('warehouse.primary', 'Primary')}</span>
          )}
        </div>
      ),
    },
    {
      id: 'city',
      header: t('warehouse.city', 'City'),
      accessorKey: 'city',
      size: 140,
    },
    {
      id: 'zoneCount',
      header: t('warehouse.zones', 'Zones'),
      accessorKey: 'zoneCount',
      size: 80,
    },
    {
      id: 'locationCount',
      header: t('warehouse.locations', 'Locations'),
      accessorKey: 'locationCount',
      size: 100,
    },
    {
      id: 'isActive',
      header: t('common.status', 'Status'),
      accessorKey: 'isActive',
      size: 100,
      cell: ({ row }) => (
        <span className={`status-badge status-badge--${row.original.isActive ? 'active' : 'inactive'}`}>
          {row.original.isActive ? t('status.active', 'Active') : t('status.inactive', 'Inactive')}
        </span>
      ),
    },
  ];

  const handleRowClick = (warehouse: WarehouseDto) => {
    navigate(`${WAREHOUSE.WAREHOUSES}/${warehouse.id}`);
  };

  const handleAddWarehouse = () => {
    navigate(`${WAREHOUSE.WAREHOUSES}/new`);
  };

  return (
    <div className="page">
      <header className="page__header">
        <div className="page__title-section">
          <h1 className="page__title">{t('warehouse.warehouses', 'Warehouses')}</h1>
        </div>
        <div className="page__actions">
          <button className="btn btn--primary" onClick={handleAddWarehouse}>
            {t('warehouse.addWarehouse', 'Add Warehouse')}
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
          data={warehouses}
          columns={columns}
          loading={isLoading || isFetching}
          onRowClick={handleRowClick}
          emptyMessage={t('warehouse.noWarehouses', 'No warehouses found')}
        />
      </div>
    </div>
  );
}

export default WarehousesList;
