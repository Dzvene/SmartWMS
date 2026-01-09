import { useState, useMemo } from 'react';
import { useIntl } from 'react-intl';
import { useNavigate } from 'react-router-dom';
import { DataTable, createColumns } from '../../../components/DataTable';
import type { PaginationState, SortingState } from '../../../components/DataTable';
import { useGetUsersQuery, type UserResponse } from '../../../api/modules/users';
import { CONFIG } from '../../../constants/routes';
import './Users.scss';

/**
 * UsersList - Table view of all users
 *
 * Clicking a row navigates to /config/users/:id
 */
export function UsersList() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();

  const [searchQuery, setSearchQuery] = useState('');
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 });
  const [sorting, setSorting] = useState<SortingState>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const { data: usersResponse, isLoading } = useGetUsersQuery({
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
    search: searchQuery || undefined,
  });

  const users = usersResponse?.data?.items || [];
  const totalRows = usersResponse?.data?.totalCount || 0;

  const columnHelper = createColumns<UserResponse>();

  const columns = useMemo(
    () => [
      columnHelper.accessor('email', {
        header: t('users.email', 'Email'),
        size: 200,
      }),
      columnHelper.accessor('firstName', {
        header: t('users.firstName', 'First Name'),
        size: 120,
      }),
      columnHelper.accessor('lastName', {
        header: t('users.lastName', 'Last Name'),
        size: 120,
      }),
      columnHelper.accessor('roleName', {
        header: t('users.role', 'Role'),
        size: 140,
        cell: ({ getValue }) => getValue() || '-',
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
      columnHelper.accessor('lastLoginAt', {
        header: t('users.lastLogin', 'Last Login'),
        size: 140,
        cell: ({ getValue }) => {
          const value = getValue();
          if (!value) return <span className="text-muted">Never</span>;
          return new Date(value).toLocaleDateString();
        },
      }),
    ],
    [columnHelper, t]
  );

  const handleRowClick = (user: UserResponse) => {
    setSelectedId(user.id);
    navigate(`${CONFIG.USERS}/${user.id}`);
  };

  const handleAddUser = () => {
    navigate(`${CONFIG.USERS}/new`);
  };

  return (
    <div className="users">
      <header className="users__header">
        <div className="users__title-section">
          <h1 className="users__title">{t('users.title', 'Users')}</h1>
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
        <div className="page-actions">
          <button className="btn btn-primary" onClick={handleAddUser}>
            {t('users.addUser', 'Add User')}
          </button>
        </div>
      </div>

      <div className="users__content">
        <DataTable
          data={users}
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

export default UsersList;
