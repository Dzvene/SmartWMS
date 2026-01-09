import { useState, useMemo } from 'react';
import { useIntl } from 'react-intl';
import { useForm } from 'react-hook-form';
import { DataTable, createColumns } from '../../../components/DataTable';
import { FullscreenModal, ModalSection } from '../../../components/FullscreenModal';
import type { PaginationState, SortingState } from '../../../components/DataTable';
import {
  useGetRolesQuery,
  useGetPermissionsQuery,
  useCreateRoleMutation,
  useUpdateRoleMutation,
  useDeleteRoleMutation,
  type RoleResponse,
} from '../../../api/modules/users';
import './Roles.scss';

interface RoleFormData {
  name: string;
  description: string;
  permissions: string[];
}

/**
 * Roles Module
 *
 * Role and permission management.
 * Connected to real backend API.
 */
export function Roles() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });

  const [searchQuery, setSearchQuery] = useState('');
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 });
  const [sorting, setSorting] = useState<SortingState>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [editingRole, setEditingRole] = useState<RoleResponse | null>(null);
  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);

  // API queries - tenantId is automatically injected by baseApi
  const { data: rolesResponse, isLoading, refetch } = useGetRolesQuery();

  const { data: permissionsResponse } = useGetPermissionsQuery();

  // API mutations
  const [createRole, { isLoading: isCreating }] = useCreateRoleMutation();
  const [updateRole, { isLoading: isUpdating }] = useUpdateRoleMutation();
  const [deleteRole, { isLoading: isDeleting }] = useDeleteRoleMutation();

  const roles = rolesResponse?.data || [];
  const availablePermissions = permissionsResponse?.data || [];

  const {
    register,
    handleSubmit,
    reset,
    watch,
    setValue,
    formState: { errors },
  } = useForm<RoleFormData>({
    defaultValues: {
      name: '',
      description: '',
      permissions: [],
    },
  });

  const selectedPermissions = watch('permissions') || [];

  // Group permissions by category
  const permissionGroups = useMemo(() => {
    const groups: Record<string, string[]> = {};
    availablePermissions.forEach((perm) => {
      const [category] = perm.split('.');
      if (!groups[category]) {
        groups[category] = [];
      }
      groups[category].push(perm);
    });
    return Object.entries(groups).map(([group, permissions]) => ({
      group: group.charAt(0).toUpperCase() + group.slice(1),
      permissions,
    }));
  }, [availablePermissions]);

  const columnHelper = createColumns<RoleResponse>();

  const columns = useMemo(
    () => [
      columnHelper.accessor('name', {
        header: t('roles.roleName', 'Role Name'),
        size: 160,
        cell: ({ getValue, row }) => (
          <div>
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
              {perms.length} {t('roles.permissions', 'permissions')}
            </span>
          );
        },
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

  const handleAddRole = () => {
    setEditingRole(null);
    reset({
      name: '',
      description: '',
      permissions: [],
    });
    setModalOpen(true);
  };

  const handleRowClick = (role: RoleResponse) => {
    setSelectedId(role.id);
    setEditingRole(role);
    reset({
      name: role.name,
      description: role.description || '',
      permissions: role.permissions || [],
    });
    setModalOpen(true);
  };

  const onSubmit = async (data: RoleFormData) => {
    try {
      if (editingRole) {
        // Update existing role
        await updateRole({
          id: editingRole.id,
          data: {
            name: data.name,
            description: data.description,
            permissions: data.permissions,
          },
        }).unwrap();
      } else {
        // Create new role
        await createRole({
          name: data.name,
          description: data.description,
          permissions: data.permissions,
        }).unwrap();
      }
      setModalOpen(false);
      refetch();
    } catch (error) {
      console.error('Failed to save role:', error);
    }
  };

  const handleDelete = async () => {
    if (!editingRole) return;

    try {
      await deleteRole(editingRole.id).unwrap();
      setDeleteConfirmOpen(false);
      setModalOpen(false);
      refetch();
    } catch (error) {
      console.error('Failed to delete role:', error);
    }
  };

  const togglePermission = (permission: string) => {
    const current = selectedPermissions || [];
    if (current.includes(permission)) {
      setValue('permissions', current.filter((p) => p !== permission));
    } else {
      setValue('permissions', [...current, permission]);
    }
  };

  const toggleGroup = (groupPermissions: string[]) => {
    const current = selectedPermissions || [];
    const allSelected = groupPermissions.every((p) => current.includes(p));

    if (allSelected) {
      setValue('permissions', current.filter((p) => !groupPermissions.includes(p)));
    } else {
      const newPerms = new Set([...current, ...groupPermissions]);
      setValue('permissions', Array.from(newPerms));
    }
  };

  return (
    <div className="roles">
      <header className="roles__header">
        <div className="roles__title-section">
          <h1 className="roles__title">{t('roles.title', 'Roles')}</h1>
        </div>
        <div className="roles__actions">
          <button className="btn btn-primary" onClick={handleAddRole}>
            {t('roles.addRole', 'Add Role')}
          </button>
        </div>
      </header>

      <div className="roles__toolbar">
        <div className="roles__search">
          <input
            type="search"
            className="roles__search-input"
            placeholder={t('common.search', 'Search...')}
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
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

      <FullscreenModal
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        title={editingRole ? t('roles.editRole', 'Edit Role') : t('roles.addRole', 'Add Role')}
        subtitle={editingRole ? editingRole.name : t('roles.createSubtitle', 'Create a new role with permissions')}
        onSave={handleSubmit(onSubmit)}
        saveLabel={editingRole ? t('common.save', 'Save') : t('common.create', 'Create')}
        loading={isCreating || isUpdating}
        maxWidth="lg"
      >
        <form onSubmit={handleSubmit(onSubmit)}>
          <ModalSection title={t('roles.details', 'Role Details')}>
            <div className="form-grid">
              <div className="form-field">
                <label className="form-field__label">{t('roles.roleName', 'Role Name')}</label>
                <input
                  type="text"
                  className={`form-field__input ${errors.name ? 'form-field__input--error' : ''}`}
                  {...register('name', { required: 'Role name is required' })}
                  placeholder="Enter role name"
                  disabled={editingRole?.isSystemRole}
                />
                {errors.name && <span className="form-field__error">{errors.name.message}</span>}
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('common.description', 'Description')}</label>
                <input
                  type="text"
                  className="form-field__input"
                  {...register('description')}
                  placeholder="Enter description"
                />
              </div>
            </div>
          </ModalSection>

          <ModalSection
            title={t('roles.permissions', 'Permissions')}
            description={t('roles.permissionsDesc', 'Select permissions for this role')}
          >
            <div className="permissions-grid">
              {permissionGroups.map((group) => {
                const allSelected = group.permissions.every((p) =>
                  selectedPermissions.includes(p)
                );

                return (
                  <div key={group.group} className="permission-group">
                    <div className="permission-group__header">
                      <label className="permission-checkbox permission-checkbox--group">
                        <input
                          type="checkbox"
                          checked={allSelected}
                          ref={undefined}
                          onChange={() => toggleGroup(group.permissions)}
                          disabled={editingRole?.isSystemRole}
                        />
                        <span className="permission-checkbox__label">
                          <strong>{group.group}</strong>
                        </span>
                      </label>
                    </div>
                    <div className="permission-group__items">
                      {group.permissions.map((perm) => (
                        <label key={perm} className="permission-checkbox">
                          <input
                            type="checkbox"
                            checked={selectedPermissions.includes(perm)}
                            onChange={() => togglePermission(perm)}
                            disabled={editingRole?.isSystemRole}
                          />
                          <span className="permission-checkbox__label">
                            {perm.split('.')[1]}
                          </span>
                        </label>
                      ))}
                    </div>
                  </div>
                );
              })}
            </div>
          </ModalSection>

          {editingRole && !editingRole.isSystemRole && (
            <ModalSection title={t('common.dangerZone', 'Danger Zone')}>
              <div className="danger-zone">
                <p className="danger-zone__text">
                  {t('roles.deleteWarning', 'Deleting this role will remove it permanently. Users assigned to this role will lose their permissions.')}
                </p>
                <button
                  type="button"
                  className="btn btn-danger"
                  onClick={() => setDeleteConfirmOpen(true)}
                  disabled={isDeleting}
                >
                  {t('roles.deleteRole', 'Delete Role')}
                </button>
              </div>
            </ModalSection>
          )}
        </form>
      </FullscreenModal>

      {/* Delete Confirmation Modal */}
      <FullscreenModal
        open={deleteConfirmOpen}
        onClose={() => setDeleteConfirmOpen(false)}
        title={t('roles.confirmDelete', 'Confirm Delete')}
        subtitle={editingRole?.name || ''}
        onSave={handleDelete}
        saveLabel={t('common.delete', 'Delete')}
        loading={isDeleting}
        maxWidth="sm"
      >
        <ModalSection title={t('roles.confirmDelete', 'Confirm Delete')}>
          <p>{t('roles.deleteConfirmText', 'Are you sure you want to delete this role? This action cannot be undone.')}</p>
        </ModalSection>
      </FullscreenModal>
    </div>
  );
}

export default Roles;
