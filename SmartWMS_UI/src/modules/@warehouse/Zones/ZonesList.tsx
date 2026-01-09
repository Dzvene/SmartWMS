import { useState } from 'react';
import { useIntl } from 'react-intl';
import { useNavigate } from 'react-router-dom';
import { useGetZonesQuery } from '@/api/modules/zones';
import type { ZoneDto, ZoneType } from '@/api/modules/zones';
import { DataTable, type ColumnDef } from '@/components/DataTable';
import { WAREHOUSE } from '@/constants/routes';
import './Zones.scss';

const zoneTypeClass: Record<ZoneType, string> = {
  Storage: 'zone-type-badge--storage',
  Picking: 'zone-type-badge--picking',
  Packing: 'zone-type-badge--packing',
  Staging: 'zone-type-badge--staging',
  Shipping: 'zone-type-badge--shipping',
  Receiving: 'zone-type-badge--receiving',
  Returns: 'zone-type-badge--returns',
};

export function ZonesList() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();

  const [searchQuery, setSearchQuery] = useState('');

  // tenantId is automatically injected by baseApi
  const { data: zonesResponse, isLoading, isFetching } = useGetZonesQuery(
    { search: searchQuery || undefined }
  );

  const zones = zonesResponse?.data?.items || [];

  const columns: ColumnDef<ZoneDto>[] = [
    {
      id: 'code',
      header: t('common.code', 'Code'),
      accessorKey: 'code',
      size: 120,
      cell: ({ row }) => <span className="zone-code">{row.original.code}</span>,
    },
    {
      id: 'name',
      header: t('common.name', 'Name'),
      accessorKey: 'name',
      cell: ({ row }) => (
        <div className="zone-name-cell">
          <span className="zone-name">{row.original.name}</span>
        </div>
      ),
    },
    {
      id: 'warehouseName',
      header: t('zone.warehouse', 'Warehouse'),
      accessorKey: 'warehouseName',
      size: 180,
    },
    {
      id: 'zoneType',
      header: t('common.type', 'Type'),
      accessorKey: 'zoneType',
      size: 120,
      cell: ({ row }) => (
        <span className={`zone-type-badge ${zoneTypeClass[row.original.zoneType] || ''}`}>
          {row.original.zoneType}
        </span>
      ),
    },
    {
      id: 'pickSequence',
      header: t('zone.pickSequence', 'Pick Seq.'),
      accessorKey: 'pickSequence',
      size: 100,
      cell: ({ row }) => row.original.pickSequence ?? '—',
    },
    {
      id: 'locationCount',
      header: t('zone.locations', 'Locations'),
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

  const handleRowClick = (zone: ZoneDto) => {
    navigate(`${WAREHOUSE.ZONES}/${zone.id}`);
  };

  const handleAddZone = () => {
    navigate(`${WAREHOUSE.ZONES}/new`);
  };

  return (
    <div className="zones">
      <header className="page-header">
        <h1 className="page-header__title">{t('zone.title', 'Zones')}</h1>
        <p className="page-header__subtitle">
          {t('zone.subtitle', 'Manage warehouse zones for storage and operations')}
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
          <button className="btn btn-primary" onClick={handleAddZone}>
            {t('zone.addZone', 'Add Zone')}
          </button>
        </div>
      </div>

      <div className="zones__content">
        <DataTable
          data={zones}
          columns={columns}
          loading={isLoading || isFetching}
          onRowClick={handleRowClick}
          emptyMessage={t('zone.noZones', 'No zones found')}
        />
      </div>
    </div>
  );
}

export default ZonesList;
