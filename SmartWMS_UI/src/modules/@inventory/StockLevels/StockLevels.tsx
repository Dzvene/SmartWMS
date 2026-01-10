import { useState, useMemo } from 'react';
import { useIntl } from 'react-intl';
import type { ColumnDef } from '@tanstack/react-table';

import { DataTable } from '@/components/DataTable';
import { useGetStockLevelsQuery } from '@/api/modules/stock';
import type { StockLevelDto } from '@/api/modules/stock';
import './StockLevels.scss';

/**
 * Stock Levels Module
 *
 * Displays current inventory levels by SKU and location.
 * Shows on-hand, reserved, and available quantities.
 */
export function StockLevels() {
  const { formatMessage } = useIntl();
  const t = (id: string) => formatMessage({ id });
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedLevel, setSelectedLevel] = useState<StockLevelDto | null>(null);
  const [paginationState, setPaginationState] = useState({ page: 1, pageSize: 25 });

  const { data, isLoading } = useGetStockLevelsQuery({
    page: paginationState.page,
    pageSize: paginationState.pageSize,
    sku: searchQuery || undefined,
  });

  const stockLevels = data?.data?.items ?? [];
  const totalRows = data?.data?.totalCount ?? 0;

  const columns = useMemo<ColumnDef<StockLevelDto, unknown>[]>(() => [
    {
      accessorKey: 'sku',
      header: t('products.sku'),
      size: 120,
      cell: ({ getValue }) => <span className="sku">{getValue() as string || '—'}</span>,
    },
    {
      accessorKey: 'productName',
      header: t('products.name'),
      size: 180,
      cell: ({ getValue }) => getValue() || '—',
    },
    {
      accessorKey: 'locationCode',
      header: t('stock.location'),
      size: 120,
      cell: ({ getValue }) => <span className="code">{getValue() as string || '—'}</span>,
    },
    {
      accessorKey: 'warehouseName',
      header: 'Warehouse',
      size: 120,
      cell: ({ getValue }) => getValue() || '—',
    },
    {
      accessorKey: 'quantityOnHand',
      header: t('stock.onHand'),
      size: 100,
      meta: { align: 'right' },
      cell: ({ getValue }) => (getValue() as number).toLocaleString(),
    },
    {
      accessorKey: 'quantityReserved',
      header: t('stock.reserved'),
      size: 100,
      meta: { align: 'right' },
      cell: ({ getValue }) => {
        const v = getValue() as number;
        return v > 0 ? <span className="text-warning">{v.toLocaleString()}</span> : '0';
      },
    },
    {
      accessorKey: 'quantityAvailable',
      header: t('stock.available'),
      size: 100,
      meta: { align: 'right' },
      cell: ({ getValue }) => {
        const v = getValue() as number;
        return <span className={v > 0 ? 'text-success' : 'text-error'}>{v.toLocaleString()}</span>;
      },
    },
    {
      accessorKey: 'batchNumber',
      header: t('stock.batch'),
      size: 120,
      cell: ({ getValue }) => getValue() || '—',
    },
    {
      accessorKey: 'expiryDate',
      header: t('stock.expiry'),
      size: 100,
      cell: ({ getValue }) => {
        const date = getValue() as string | undefined;
        if (!date) return '—';
        const d = new Date(date);
        const isExpired = d < new Date();
        return (
          <span className={isExpired ? 'text-error' : ''}>
            {d.toLocaleDateString()}
          </span>
        );
      },
    },
  ], [t]);

  return (
    <div className="page">
      <header className="page__header">
        <div className="page__title-section">
          <h1 className="page__title">{t('stock.title')}</h1>
        </div>
        <div className="page__actions">
          <button className="btn btn-secondary">
            {t('common.export')}
          </button>
        </div>
      </header>

      <div className="page-toolbar">
        <div className="page-search">
          <input
            type="text"
            className="page-search__input"
            placeholder={t('common.search')}
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
        </div>
        <div className="page-filters">
          <button className="btn btn-secondary">
            {t('common.filter')}
          </button>
        </div>
      </div>

      <div className="page__content">
        <DataTable
          data={stockLevels}
          columns={columns}
          pagination={{ pageIndex: paginationState.page - 1, pageSize: paginationState.pageSize }}
          onPaginationChange={({ pageIndex, pageSize }) => {
            setPaginationState({ page: pageIndex + 1, pageSize });
          }}
          totalRows={totalRows}
          onRowClick={setSelectedLevel}
          selectedRowId={selectedLevel?.id}
          getRowId={(row) => row.id}
          loading={isLoading}
          emptyMessage={t('common.noData')}
        />
      </div>
    </div>
  );
}

export default StockLevels;
