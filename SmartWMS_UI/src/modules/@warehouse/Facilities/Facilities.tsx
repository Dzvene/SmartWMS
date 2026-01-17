import { useState, useMemo } from 'react';
import type { ColumnDef } from '@tanstack/react-table';
import { useTranslate } from '@/hooks';

import { DataTable } from '@/components/DataTable';
import { FullscreenModal } from '@/components/FullscreenModal';
import { useGetSitesQuery } from '@/api/modules/sites';
import type { SiteDto } from '@/api/modules/sites';

export function Facilities() {
  const t = useTranslate();
  const [selectedFacility, setSelectedFacility] = useState<SiteDto | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [paginationState, setPaginationState] = useState({ page: 1, pageSize: 25 });

  const { data, isLoading } = useGetSitesQuery({
    page: paginationState.page,
    pageSize: paginationState.pageSize,
  });

  const facilities = data?.data?.items ?? [];
  const totalRows = data?.data?.totalCount ?? 0;

  const columns = useMemo<ColumnDef<SiteDto, unknown>[]>(() => [
    { accessorKey: 'code', header: 'Code', size: 100, cell: ({ getValue }) => <span className="code">{getValue() as string}</span> },
    { accessorKey: 'name', header: t('common.name'), size: 160 },
    { accessorKey: 'city', header: 'City', size: 120, cell: ({ getValue }) => getValue() || '—' },
    { accessorKey: 'country', header: 'Country', size: 100, cell: ({ getValue }) => getValue() || '—' },
    { accessorKey: 'warehouseCount', header: 'Warehouses', size: 100, meta: { align: 'right' } },
    { accessorKey: 'isActive', header: t('common.status'), size: 80, cell: ({ getValue }) => <span className={`status-badge status-badge--${getValue() ? 'active' : 'inactive'}`}>{getValue() ? t('status.active') : t('status.inactive')}</span> },
  ], [t]);

  const handleRowClick = (facility: SiteDto) => { setSelectedFacility(facility); setIsModalOpen(true); };
  const handleAddNew = () => { setSelectedFacility(null); setIsModalOpen(true); };

  return (
    <div className="page">
      <div className="page__header">
        <h1 className="page__title">{t('nav.warehouse.facilities')}</h1>
        <div className="page__actions">
          <button className="btn btn--primary" onClick={handleAddNew}>Add Facility</button>
        </div>
      </div>
      <div className="page__content">
        <DataTable
          data={facilities}
          columns={columns}
          pagination={{ pageIndex: paginationState.page - 1, pageSize: paginationState.pageSize }}
          onPaginationChange={({ pageIndex, pageSize }) => { setPaginationState({ page: pageIndex + 1, pageSize }); }}
          totalRows={totalRows}
          onRowClick={handleRowClick}
          selectedRowId={selectedFacility?.id}
          getRowId={(row) => row.id}
          loading={isLoading}
          emptyMessage={t('common.noData')}
        />
      </div>
      <FullscreenModal open={isModalOpen} onClose={() => setIsModalOpen(false)} title={selectedFacility ? `Edit ${selectedFacility.name}` : 'Add Facility'}>
        <div className="form"><p>Facility form will be implemented here.</p></div>
      </FullscreenModal>
    </div>
  );
}

export default Facilities;
