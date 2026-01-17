import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslate } from '@/hooks';
import { DataTable, createColumns } from '@/components/DataTable';
import type { PaginationState, SortingState } from '@/components/DataTable';
import { useGetReasonCodesQuery } from '@/api/modules/configuration';
import type { ReasonCodeDto, ReasonCodeType } from '@/api/modules/configuration';
import { CONFIG } from '@/constants/routes';

const categoryLabels: Record<ReasonCodeType, string> = {
  Adjustment: 'Adjustment',
  Return: 'Return',
  Damage: 'Damage',
  Expiry: 'Expiry',
  QualityHold: 'Quality Hold',
  Transfer: 'Transfer',
  Scrap: 'Scrap',
  Found: 'Found',
  Lost: 'Lost',
  Other: 'Other',
};

/**
 * Reason Codes Configuration Module
 *
 * Manages reason codes for inventory adjustments and returns.
 */
export function ReasonCodes() {
  const t = useTranslate();
  const navigate = useNavigate();

  const [searchQuery, setSearchQuery] = useState('');
  const [activeFilter, setActiveFilter] = useState<'' | 'true' | 'false'>('');
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 });
  const [sorting, setSorting] = useState<SortingState>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const { data: response, isLoading } = useGetReasonCodesQuery({
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
    searchTerm: searchQuery || undefined,
    isActive: activeFilter === '' ? undefined : activeFilter === 'true',
  });

  const reasonCodes = response?.data?.items || [];
  const totalRows = response?.data?.totalCount || 0;

  const columnHelper = createColumns<ReasonCodeDto>();

  const columns = useMemo(
    () => [
      columnHelper.accessor('code', {
        header: t('reasonCodes.code', 'Code'),
        size: 100,
        cell: ({ getValue }) => (
          <span className="reason-codes__code">{getValue()}</span>
        ),
      }),
      columnHelper.accessor('name', {
        header: t('common.name', 'Name'),
        size: 160,
        cell: ({ getValue }) => (
          <span className="reason-codes__name">{getValue()}</span>
        ),
      }),
      columnHelper.accessor('description', {
        header: t('reasonCodes.description', 'Description'),
        size: 200,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('reasonType', {
        header: t('reasonCodes.category', 'Category'),
        size: 120,
        cell: ({ getValue }) => {
          const type = getValue();
          return (
            <span className={`tag tag--${type.toLowerCase()}`}>
              {categoryLabels[type]}
            </span>
          );
        },
      }),
      columnHelper.accessor('requiresNotes', {
        header: t('reasonCodes.requiresNote', 'Requires Note'),
        size: 110,
        cell: ({ getValue }) => (getValue() ? 'Yes' : 'No'),
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

  const handleRowClick = (reasonCode: ReasonCodeDto) => {
    setSelectedId(reasonCode.id);
    navigate(`${CONFIG.REASON_CODES}/${reasonCode.id}`);
  };

  const handleCreateReasonCode = () => {
    navigate(CONFIG.REASON_CODE_CREATE);
  };

  return (
    <div className="page">
      <header className="page__header">
        <div className="page__title-section">
          <h1 className="page__title">{t('reasonCodes.title', 'Reason Codes')}</h1>
          <p className="page__subtitle">
            {t('reasonCodes.subtitle', 'Manage reason codes for inventory adjustments and returns')}
          </p>
        </div>
        <div className="page__actions">
          <button className="btn btn--primary" onClick={handleCreateReasonCode}>
            {t('reasonCodes.createReasonCode', 'Create Reason Code')}
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
            <option value="">{t('reasonCodes.allStatuses', 'All Statuses')}</option>
            <option value="true">{t('common.active', 'Active')}</option>
            <option value="false">{t('common.inactive', 'Inactive')}</option>
          </select>
        </div>
      </div>

      <div className="page__content">
        <DataTable
          data={reasonCodes}
          columns={columns}
          pagination={pagination}
          onPaginationChange={setPagination}
          sorting={sorting}
          onSortingChange={setSorting}
          totalRows={totalRows}
          selectedRowId={selectedId}
          onRowClick={handleRowClick}
          getRowId={(row) => row.id}
          emptyMessage={t('reasonCodes.noReasonCodes', 'No reason codes found')}
          loading={isLoading}
        />
      </div>
    </div>
  );
}

export default ReasonCodes;
