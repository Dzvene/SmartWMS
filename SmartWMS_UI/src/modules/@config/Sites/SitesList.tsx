import { useState } from 'react';
import { useIntl } from 'react-intl';
import { useNavigate } from 'react-router-dom';
import { useGetSitesQuery } from '@/api/modules/sites';
import type { SiteDto } from '@/api/modules/sites';
import { DataTable, type ColumnDef } from '@/components/DataTable';
import { CONFIG } from '@/constants/routes';
import './Sites.scss';

export function SitesList() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();

  const [searchQuery, setSearchQuery] = useState('');

  // tenantId is automatically injected by baseApi
  const { data: sitesResponse, isLoading, isFetching } = useGetSitesQuery(
    { search: searchQuery || undefined }
  );

  const sites = sitesResponse?.data?.items || [];

  const columns: ColumnDef<SiteDto>[] = [
    {
      id: 'code',
      header: t('common.code', 'Code'),
      accessorKey: 'code',
      size: 120,
      cell: ({ row }) => <span className="site-code">{row.original.code}</span>,
    },
    {
      id: 'name',
      header: t('common.name', 'Name'),
      accessorKey: 'name',
      cell: ({ row }) => (
        <div className="site-name-cell">
          <span className="site-name">{row.original.name}</span>
          {row.original.isPrimary && (
            <span className="badge badge--primary">{t('site.primary', 'Primary')}</span>
          )}
        </div>
      ),
    },
    {
      id: 'city',
      header: t('site.city', 'City'),
      accessorKey: 'city',
      size: 150,
    },
    {
      id: 'warehousesCount',
      header: t('site.warehouses', 'Warehouses'),
      accessorKey: 'warehousesCount',
      size: 120,
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

  const handleRowClick = (site: SiteDto) => {
    navigate(`${CONFIG.SITES}/${site.id}`);
  };

  const handleAddSite = () => {
    navigate(`${CONFIG.SITES}/new`);
  };

  return (
    <div className="page">
      <header className="page__header">
        <div className="page__title-section">
          <h1 className="page__title">{t('site.title', 'Sites')}</h1>
          <p className="page__subtitle">
            {t('site.subtitle', 'Manage company sites and their settings')}
          </p>
        </div>
        <div className="page__actions">
          <button className="btn btn--primary" onClick={handleAddSite}>
            {t('site.addSite', 'Add Site')}
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
          data={sites}
          columns={columns}
          loading={isLoading || isFetching}
          onRowClick={handleRowClick}
          emptyMessage={t('site.noSites', 'No sites found')}
        />
      </div>
    </div>
  );
}

export default SitesList;
