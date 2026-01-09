import { useState, useMemo } from 'react';
import { useIntl } from 'react-intl';
import { useNavigate } from 'react-router-dom';
import { DataTable, createColumns } from '../../../components/DataTable';
import type { PaginationState, SortingState } from '../../../components/DataTable';
import { useGetRolesQuery, type RoleResponse } from '../../../api/modules/users';
import { CONFIG } from '../../../constants/routes';
import './Roles.scss';

/**
 * RolesList - Table view of all roles
 *
 * Clicking a row navigates to /config/roles/:id
 */
export function RolesList() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();

  const [searchQuery, setSearchQuery] = useState('');
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 });
  const [sorting, setSorting] = useState<SortingState>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const { data: rolesResponse, isLoading } = useGetRolesQuery();

  const roles = rolesResponse?.data || [];

  const columnHelper = createColumns<RoleResponse>();

  const columns = useMemo(
    () => [
      columnHelper.accessor('name', {
        header: t('roles.roleName', 'Role Name'),
        size: 160,
        cell: ({ getValue, row }) => (
          <div className="role-name-cell">
            <strong>{getValue()}</strong>
            {row.original.isSystemRole && (
              <span className="badge badge--system">{t('roles.system', 'System')}</span>
            )}
          </div>
        ),
      }),
      columnHelper.accessor('description', {
        header: t('common.description', 'Description'),
        size: 250,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('permissions', {
        header: t('roles.permissions', 'Permissions'),
        size: 120,
        cell: ({ getValue }) => {
          const perms = getValue() || [];
          return (
            <span className="text-muted">
              {perms.length} {perms.length === 1 ? 'permission' : 'permissions'}
            </span>
          );
        },
      }),
      columnHelper.accessor('userCount', {
        header: t('roles.assignedUsers', 'Users'),
        size: 100,
        cell: ({ getValue }) => (
          <span className="text-muted">{getValue()}</span>
        ),
      }),
    ],
    [columnHelper, t]
  );

  // Filter roles by search query
  const filteredData = useMemo(() => {
    if (!searchQuery) return roles;
    const query = searchQuery.toLowerCase();
    return roles.filter(
      (r) =>
        r.name.toLowerCase().includes(query) ||
        (r.description?.toLowerCase().includes(query) ?? false)
    );
  }, [roles, searchQuery]);

  const handleRowClick = (role: RoleResponse) => {
    setSelectedId(role.id);
    navigate(`${CONFIG.ROLES}/${role.id}`);
  };

  const handleAddRole = () => {
    navigate(`${CONFIG.ROLES}/new`);
  };

  return (
    <div className="roles">
      <header className="roles__header">
        <div className="roles__title-section">
          <h1 className="roles__title">{t('roles.title', 'Roles')}</h1>
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
          <button className="btn btn-primary" onClick={handleAddRole}>
            {t('roles.addRole', 'Add Role')}
          </button>
        </div>
      </div>

      <div className="roles__content">
        <DataTable
          data={filteredData}
          columns={columns}
          pagination={pagination}
          onPaginationChange={setPagination}
          sorting={sorting}
          onSortingChange={setSorting}
          totalRows={filteredData.length}
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

export default RolesList;
