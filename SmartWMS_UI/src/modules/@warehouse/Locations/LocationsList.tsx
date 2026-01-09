import { useState } from 'react';
import { useIntl } from 'react-intl';
import { useNavigate } from 'react-router-dom';
import { useGetLocationsQuery } from '@/api/modules/locations';
import type { LocationDto, LocationType } from '@/api/modules/locations';
import { DataTable, type ColumnDef } from '@/components/DataTable';
import { WAREHOUSE } from '@/constants/routes';
import './Locations.scss';

const locationTypeClass: Record<LocationType, string> = {
  Bulk: 'location-type-badge--bulk',
  Pick: 'location-type-badge--pick',
  Staging: 'location-type-badge--staging',
  Receiving: 'location-type-badge--receiving',
  Shipping: 'location-type-badge--shipping',
  Returns: 'location-type-badge--returns',
  Quarantine: 'location-type-badge--quarantine',
  Reserve: 'location-type-badge--reserve',
};

export function LocationsList() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();

  const [searchQuery, setSearchQuery] = useState('');

  // tenantId is automatically injected by baseApi
  const { data: locationsResponse, isLoading, isFetching } = useGetLocationsQuery(
    { search: searchQuery || undefined }
  );

  const locations = locationsResponse?.data?.items || [];

  const columns: ColumnDef<LocationDto>[] = [
    {
      id: 'code',
      header: t('common.code', 'Code'),
      accessorKey: 'code',
      size: 140,
      cell: ({ row }) => <span className="location-code">{row.original.code}</span>,
    },
    {
      id: 'name',
      header: t('common.name', 'Name'),
      accessorKey: 'name',
      cell: ({ row }) => (
        <div className="location-name-cell">
          <span className="location-name">{row.original.name || '—'}</span>
        </div>
      ),
    },
    {
      id: 'warehouseName',
      header: t('location.warehouse', 'Warehouse'),
      accessorKey: 'warehouseName',
      size: 150,
    },
    {
      id: 'zoneName',
      header: t('location.zone', 'Zone'),
      accessorKey: 'zoneName',
      size: 120,
      cell: ({ row }) => row.original.zoneName || '—',
    },
    {
      id: 'locationType',
      header: t('common.type', 'Type'),
      accessorKey: 'locationType',
      size: 120,
      cell: ({ row }) => (
        <span className={`location-type-badge ${locationTypeClass[row.original.locationType] || ''}`}>
          {row.original.locationType}
        </span>
      ),
    },
    {
      id: 'position',
      header: t('location.position', 'Position'),
      accessorKey: 'aisle',
      size: 180,
      cell: ({ row }) => {
        const parts = [
          row.original.aisle,
          row.original.rack,
          row.original.level,
          row.original.position,
        ].filter(Boolean);
        return parts.length > 0 ? parts.join('-') : '—';
      },
    },
    {
      id: 'stockLevelCount',
      header: t('location.stockLevels', 'Stock'),
      accessorKey: 'stockLevelCount',
      size: 80,
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

  const handleRowClick = (location: LocationDto) => {
    navigate(`${WAREHOUSE.LOCATIONS}/${location.id}`);
  };

  const handleAddLocation = () => {
    navigate(`${WAREHOUSE.LOCATIONS}/new`);
  };

  return (
    <div className="locations">
      <header className="page-header">
        <h1 className="page-header__title">{t('location.title', 'Locations')}</h1>
        <p className="page-header__subtitle">
          {t('location.subtitle', 'Manage storage locations within your warehouse zones')}
        </p>
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
        <div className="page-actions">
          <button className="btn btn-primary" onClick={handleAddLocation}>
            {t('location.addLocation', 'Add Location')}
          </button>
        </div>
      </div>

      <div className="locations__content">
        <DataTable
          data={locations}
          columns={columns}
          loading={isLoading || isFetching}
          onRowClick={handleRowClick}
          emptyMessage={t('location.noLocations', 'No locations found')}
        />
      </div>
    </div>
  );
}

export default LocationsList;
