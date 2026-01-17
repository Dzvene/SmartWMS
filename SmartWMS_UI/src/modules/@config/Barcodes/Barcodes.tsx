import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslate } from '@/hooks';
import type { ColumnDef } from '@tanstack/react-table';

import { DataTable } from '@/components/DataTable';
import type { PaginationState, SortingState } from '@/components/DataTable';
import { useGetBarcodePrefixesQuery } from '@/api/modules/configuration';
import type { BarcodePrefixDto, BarcodePrefixType } from '@/api/modules/configuration';
import { CONFIG } from '@/constants/routes';

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
  const t = useTranslate();
  const navigate = useNavigate();

  const [searchQuery, setSearchQuery] = useState('');
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 });
  const [sorting, setSorting] = useState<SortingState>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const { data, isLoading } = useGetBarcodePrefixesQuery({
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
  });

  const barcodes = useMemo(() => data?.data?.items ?? [], [data?.data?.items]);
  const totalRows = data?.data?.totalCount ?? 0;

  const filteredBarcodes = useMemo(() => {
    if (!searchQuery) return barcodes;
    const query = searchQuery.toLowerCase();
    return barcodes.filter(
      (b) =>
        b.prefix.toLowerCase().includes(query) ||
        (b.description?.toLowerCase().includes(query) ?? false)
    );
  }, [barcodes, searchQuery]);

  const columns = useMemo<ColumnDef<BarcodePrefixDto, unknown>[]>(
    () => [
      {
        accessorKey: 'prefix',
        header: t('barcodes.prefix', 'Prefix'),
        size: 100,
        cell: ({ getValue }) => (
          <span className="code">{getValue() as string}</span>
        ),
      },
      {
        accessorKey: 'prefixType',
        header: t('common.type', 'Type'),
        size: 120,
        cell: ({ getValue }) => typeLabels[getValue() as BarcodePrefixType],
      },
      {
        accessorKey: 'description',
        header: t('barcodes.description', 'Description'),
        size: 200,
        cell: ({ getValue }) => getValue() || '—',
      },
      {
        accessorKey: 'isActive',
        header: t('common.status', 'Status'),
        size: 100,
        cell: ({ getValue }) => (
          <span className={`status-badge status-badge--${getValue() ? 'success' : 'neutral'}`}>
            {getValue() ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
          </span>
        ),
      },
      {
        accessorKey: 'createdAt',
        header: t('common.created', 'Created'),
        size: 120,
        cell: ({ getValue }) => new Date(getValue() as string).toLocaleDateString(),
      },
    ],
    [t]
  );

  const handleRowClick = (barcode: BarcodePrefixDto) => {
    setSelectedId(barcode.id);
    navigate(`${CONFIG.BARCODES}/${barcode.id}`);
  };

  const handleCreateBarcode = () => {
    navigate(CONFIG.BARCODE_CREATE);
  };

  return (
    <div className="page">
      <header className="page__header">
        <div className="page__title-section">
          <h1 className="page__title">{t('barcodes.title', 'Barcode Prefixes')}</h1>
          <p className="page__subtitle">
            {t('barcodes.subtitle', 'Manage barcode prefixes for scanning operations')}
          </p>
        </div>
        <div className="page__actions">
          <button className="btn btn--primary" onClick={handleCreateBarcode}>
            {t('barcodes.addBarcode', 'Add Prefix')}
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
          data={filteredBarcodes}
          columns={columns}
          pagination={pagination}
          onPaginationChange={setPagination}
          sorting={sorting}
          onSortingChange={setSorting}
          totalRows={totalRows}
          selectedRowId={selectedId}
          onRowClick={handleRowClick}
          getRowId={(row) => row.id}
          loading={isLoading}
          emptyMessage={t('barcodes.noBarcodes', 'No barcode prefixes found')}
        />
      </div>
    </div>
  );
}

export default Barcodes;
