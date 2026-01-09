import { useState, useMemo } from 'react';
import { useIntl } from 'react-intl';
import type { ColumnDef } from '@tanstack/react-table';

import { DataTable } from '@/components/DataTable';
import { FullscreenModal } from '@/components/FullscreenModal';
import { useGetBarcodePrefixesQuery } from '@/api/modules/configuration';
import type { BarcodePrefixDto, BarcodePrefixType } from '@/api/modules/configuration';

const typeLabels: Record<BarcodePrefixType, string> = {
  Product: 'Product',
  Location: 'Location',
  Container: 'Container',
  Pallet: 'Pallet',
  Order: 'Order',
  Transfer: 'Transfer',
  Receipt: 'Receipt',
  Shipment: 'Shipment',
  User: 'User',
  Equipment: 'Equipment',
  Other: 'Other',
};

/**
 * Barcodes Configuration Module
 *
 * Manages barcode prefixes and scanning rules.
 */
export function Barcodes() {
  const { formatMessage } = useIntl();
  const t = (id: string) => formatMessage({ id });

  const [selectedBarcode, setSelectedBarcode] = useState<BarcodePrefixDto | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [paginationState, setPaginationState] = useState({ page: 1, pageSize: 25 });

  const { data, isLoading } = useGetBarcodePrefixesQuery({
    page: paginationState.page,
    pageSize: paginationState.pageSize,
  });

  const barcodes = data?.data?.items ?? [];
  const totalRows = data?.data?.totalCount ?? 0;

  const columns = useMemo<ColumnDef<BarcodePrefixDto, unknown>[]>(
    () => [
      {
        accessorKey: 'prefix',
        header: t('barcodes.prefix'),
        size: 100,
        cell: ({ getValue }) => (
          <span className="code">{getValue() as string}</span>
        ),
      },
      {
        accessorKey: 'prefixType',
        header: t('common.type'),
        size: 120,
        cell: ({ getValue }) => typeLabels[getValue() as BarcodePrefixType],
      },
      {
        accessorKey: 'description',
        header: 'Description',
        size: 200,
        cell: ({ getValue }) => getValue() || '—',
      },
      {
        accessorKey: 'isActive',
        header: t('common.status'),
        size: 80,
        cell: ({ getValue }) => (
          <span className={`status-badge status-badge--${getValue() ? 'active' : 'inactive'}`}>
            {getValue() ? t('status.active') : t('status.inactive')}
          </span>
        ),
      },
      {
        accessorKey: 'createdAt',
        header: 'Created',
        size: 120,
        cell: ({ getValue }) => new Date(getValue() as string).toLocaleDateString(),
      },
    ],
    [t]
  );

  const handleRowClick = (barcode: BarcodePrefixDto) => {
    setSelectedBarcode(barcode);
    setIsModalOpen(true);
  };

  const handleAddNew = () => {
    setSelectedBarcode(null);
    setIsModalOpen(true);
  };

  return (
    <div className="page">
      <div className="page__header">
        <h1 className="page__title">{t('barcodes.title')}</h1>
        <div className="page__actions">
          <button className="btn btn--secondary">
            {t('barcodes.restoreDefaults')}
          </button>
          <button className="btn btn--primary" onClick={handleAddNew}>
            {t('barcodes.addBarcode')}
          </button>
        </div>
      </div>

      <div className="page__content">
        <DataTable
          data={barcodes}
          columns={columns}
          pagination={{
            pageIndex: paginationState.page - 1,
            pageSize: paginationState.pageSize,
          }}
          onPaginationChange={({ pageIndex, pageSize }) => {
            setPaginationState({ page: pageIndex + 1, pageSize });
          }}
          totalRows={totalRows}
          onRowClick={handleRowClick}
          selectedRowId={selectedBarcode?.id}
          getRowId={(row) => row.id}
          loading={isLoading}
          emptyMessage={t('common.noData')}
        />
      </div>

      <FullscreenModal
        open={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={selectedBarcode ? 'Edit Barcode Configuration' : 'Add Barcode Configuration'}
      >
        <div className="form">
          <p>Barcode configuration form will be implemented here.</p>
        </div>
      </FullscreenModal>
    </div>
  );
}

export default Barcodes;
