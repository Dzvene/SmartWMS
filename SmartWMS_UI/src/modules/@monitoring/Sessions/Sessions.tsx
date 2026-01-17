import { useState, useMemo } from 'react';
import { DataTable, createColumns } from '@/components/DataTable';
import { useTranslate } from '@/hooks';
import type { PaginationState, SortingState } from '@/components/DataTable';
import { useGetLoginAttemptsQuery } from '@/api/modules/sessions';
import type { LoginAttemptDto } from '@/api/modules/sessions';

/**
 * Sessions Module
 * Monitors login attempts and user sessions across the system.
 */
export function Sessions() {
  const t = useTranslate();

  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<'all' | 'success' | 'failed'>('all');
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 50 });
  const [sorting, setSorting] = useState<SortingState>([{ id: 'createdAt', desc: true }]);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const { data: response, isLoading } = useGetLoginAttemptsQuery({
    params: {
      page: pagination.pageIndex + 1,
      pageSize: pagination.pageSize,
      success: statusFilter === 'all' ? undefined : statusFilter === 'success',
    },
  });

  const loginAttempts = response?.data?.items || [];
  const totalRows = response?.data?.totalCount || 0;

  const columnHelper = createColumns<LoginAttemptDto>();

  const columns = useMemo(
    () => [
      columnHelper.accessor('createdAt', {
        header: t('sessions.loginTime', 'Login Time'),
        size: 170,
        cell: ({ getValue }) => new Date(getValue()).toLocaleString(),
      }),
      columnHelper.accessor('userName', {
        header: t('sessions.username', 'Username'),
        size: 140,
        cell: ({ getValue }) => (
          <span className="code">{getValue() || '-'}</span>
        ),
      }),
      columnHelper.accessor('success', {
        header: t('common.status', 'Status'),
        size: 100,
        cell: ({ getValue }) => {
          const success = getValue();
          return (
            <span className={`status-badge status-badge--${success ? 'success' : 'error'}`}>
              {success ? 'Success' : 'Failed'}
            </span>
          );
        },
      }),
      columnHelper.accessor('failureReason', {
        header: t('sessions.failureReason', 'Failure Reason'),
        size: 180,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('ipAddress', {
        header: t('sessions.ipAddress', 'IP Address'),
        size: 130,
        cell: ({ getValue }) => (
          <span className="text-muted">{getValue() || '-'}</span>
        ),
      }),
      columnHelper.accessor('location', {
        header: t('sessions.location', 'Location'),
        size: 140,
        cell: ({ getValue }) => getValue() || '-',
      }),
    ],
    [columnHelper, t]
  );

  const handleRowClick = (attempt: LoginAttemptDto) => {
    setSelectedId(attempt.id);
  };

  return (
    <div className="page">
      <header className="page__header">
        <div className="page__title-section">
          <h1 className="page__title">{t('sessions.title', 'Login Activity')}</h1>
          <p className="page__subtitle">{t('sessions.subtitle', 'Monitor login attempts and user sessions')}</p>
        </div>
      </header>

      <div className="page-toolbar">
        <div className="page-search">
          <input
            type="text"
            className="page-search__input"
            placeholder={t('sessions.searchUser', 'Search by username...')}
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
        </div>
        <div className="page-filters">
          <select
            className="page-filter__select"
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value as 'all' | 'success' | 'failed')}
          >
            <option value="all">{t('sessions.allAttempts', 'All Attempts')}</option>
            <option value="success">{t('sessions.successOnly', 'Successful Only')}</option>
            <option value="failed">{t('sessions.failedOnly', 'Failed Only')}</option>
          </select>
        </div>
      </div>

      <div className="page__content">
        <DataTable
          data={loginAttempts}
          columns={columns}
          pagination={pagination}
          onPaginationChange={setPagination}
          sorting={sorting}
          onSortingChange={setSorting}
          totalRows={totalRows}
          onRowClick={handleRowClick}
          selectedRowId={selectedId}
          getRowId={(row) => row.id}
          emptyMessage={t('sessions.noAttempts', 'No login attempts found')}
          loading={isLoading}
        />
      </div>
    </div>
  );
}

export default Sessions;
