import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslate } from '@/hooks';
import { DataTable, createColumns } from '@/components/DataTable';
import type { PaginationState, SortingState } from '@/components/DataTable';
import { useGetProductsQuery, type ProductResponse } from '@/api/modules/products';
import { INVENTORY } from '@/constants/routes';
import './ProductCatalog.scss';

/**
 * ProductCatalog - Table view of all products
 *
 * Clicking a row navigates to /inventory/catalog/:id
 */
export function ProductCatalog() {
  const t = useTranslate();
  const navigate = useNavigate();

  const [searchQuery, setSearchQuery] = useState('');
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 });
  const [sorting, setSorting] = useState<SortingState>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const { data: productsResponse, isLoading } = useGetProductsQuery({
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
    search: searchQuery || undefined,
  });

  const products = productsResponse?.data?.items || [];
  const totalRows = productsResponse?.data?.totalCount || 0;

  const columnHelper = createColumns<ProductResponse>();

  const columns = useMemo(
    () => [
      columnHelper.accessor('sku', {
        header: t('products.sku', 'SKU'),
        size: 120,
        cell: ({ getValue }) => <span className="sku">{getValue()}</span>,
      }),
      columnHelper.accessor('name', {
        header: t('products.name', 'Name'),
        size: 200,
      }),
      columnHelper.accessor('categoryName', {
        header: t('products.category', 'Category'),
        size: 140,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('barcode', {
        header: t('products.barcode', 'Barcode'),
        size: 140,
        cell: ({ getValue }) => {
          const value = getValue();
          return value ? <code className="code">{value}</code> : '-';
        },
      }),
      columnHelper.accessor('unitOfMeasure', {
        header: t('products.uom', 'UoM'),
        size: 80,
      }),
      columnHelper.accessor('totalOnHand', {
        header: t('products.onHand', 'On Hand'),
        size: 100,
        cell: ({ getValue }) => {
          const value = getValue();
          return value !== undefined && value !== null ? value.toLocaleString() : '-';
        },
      }),
      columnHelper.accessor('isActive', {
        header: t('common.status', 'Status'),
        size: 100,
        cell: ({ getValue }) => {
          const isActive = getValue();
          return (
            <span className={`status-badge status-badge--${isActive ? 'active' : 'inactive'}`}>
              {isActive ? t('status.active', 'Active') : t('status.inactive', 'Inactive')}
            </span>
          );
        },
      }),
    ],
    [columnHelper, t]
  );

  const handleRowClick = (product: ProductResponse) => {
    setSelectedId(product.id);
    navigate(`${INVENTORY.SKU_CATALOG}/${product.id}`);
  };

  const handleAddProduct = () => {
    navigate(`${INVENTORY.SKU_CATALOG}/new`);
  };

  return (
    <div className="page">
      <header className="page__header">
        <div className="page__title-section">
          <h1 className="page__title">{t('products.title', 'Product Catalog')}</h1>
        </div>
        <div className="page__actions">
          <button className="btn btn--primary" onClick={handleAddProduct}>
            {t('products.addProduct', 'Add Product')}
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
          data={products}
          columns={columns}
          pagination={pagination}
          onPaginationChange={setPagination}
          sorting={sorting}
          onSortingChange={setSorting}
          totalRows={totalRows}
          selectedRowId={selectedId}
          onRowClick={handleRowClick}
          getRowId={(row) => row.id}
          emptyMessage={t('common.noData', 'No data')}
          loading={isLoading}
        />
      </div>
    </div>
  );
}

export default ProductCatalog;
