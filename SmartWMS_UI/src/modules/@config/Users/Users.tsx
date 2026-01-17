import { useState, useMemo } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { useTranslate } from '@/hooks';
import { DataTable, createColumns } from '../../../components/DataTable';
import { FullscreenModal, ModalSection } from '../../../components/FullscreenModal';
import type { PaginationState, SortingState } from '../../../components/DataTable';
import {
  useGetUsersQuery,
  useGetRolesQuery,
  useCreateUserMutation,
  useUpdateUserMutation,
  useDeactivateUserMutation,
  useActivateUserMutation,
  useResetPasswordMutation,
  type UserResponse,
  type CreateUserRequest,
  type UpdateUserRequest,
} from '../../../api/modules/users';
import './Users.scss';

interface UserFormData {
  email: string;
  firstName: string;
  lastName: string;
  password: string;
  roleName: string;
}

/**
 * Users Module
 *
 * User management with role assignment.
 * Connected to real backend API.
 */
export function Users() {
  const t = useTranslate();

  const [searchQuery, setSearchQuery] = useState('');
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 });
  const [sorting, setSorting] = useState<SortingState>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [editingUser, setEditingUser] = useState<UserResponse | null>(null);
  const [resetPasswordModalOpen, setResetPasswordModalOpen] = useState(false);
  const [newPassword, setNewPassword] = useState('');

  // API queries - tenantId is automatically injected by baseApi
  const { data: usersResponse, isLoading, refetch } = useGetUsersQuery({
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
    search: searchQuery || undefined,
  });

  const { data: rolesResponse } = useGetRolesQuery();

  // API mutations
  const [createUser, { isLoading: isCreating }] = useCreateUserMutation();
  const [updateUser, { isLoading: isUpdating }] = useUpdateUserMutation();
  const [deactivateUser] = useDeactivateUserMutation();
  const [activateUser] = useActivateUserMutation();
  const [resetPassword, { isLoading: isResettingPassword }] = useResetPasswordMutation();

  const users = usersResponse?.data?.items || [];
  const totalRows = usersResponse?.data?.totalCount || 0;
  const roles = rolesResponse?.data || [];

  const {
    register,
    handleSubmit,
    reset,
    control,
    formState: { errors },
  } = useForm<UserFormData>();

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

  const handleAddUser = () => {
    setEditingUser(null);
    reset({
      email: '',
      firstName: '',
      lastName: '',
      password: '',
      roleName: '',
    });
    setModalOpen(true);
  };

  const handleRowClick = (user: UserResponse) => {
    setSelectedId(user.id);
    setEditingUser(user);
    reset({
      email: user.email,
      firstName: user.firstName,
      lastName: user.lastName,
      password: '',
      roleName: user.roleName || '',
    });
    setModalOpen(true);
  };

  const onSubmit = async (data: UserFormData) => {
    try {
      if (editingUser) {
        // Update existing user
        const updateData: UpdateUserRequest = {
          email: data.email,
          firstName: data.firstName,
          lastName: data.lastName,
          roleName: data.roleName || undefined,
        };
        await updateUser({ id: editingUser.id, data: updateData }).unwrap();
      } else {
        // Create new user
        const createData: CreateUserRequest = {
          email: data.email,
          firstName: data.firstName,
          lastName: data.lastName,
          password: data.password,
          roleName: data.roleName || undefined,
        };
        await createUser(createData).unwrap();
      }
      setModalOpen(false);
      refetch();
    } catch (error) {
      console.error('Failed to save user:', error);
    }
  };

  const handleToggleStatus = async () => {
    if (!editingUser) return;

    try {
      if (editingUser.isActive) {
        await deactivateUser(editingUser.id).unwrap();
      } else {
        await activateUser(editingUser.id).unwrap();
      }
      setModalOpen(false);
      refetch();
    } catch (error) {
      console.error('Failed to toggle user status:', error);
    }
  };

  const handleResetPassword = async () => {
    if (!editingUser || !newPassword) return;

    try {
      await resetPassword({ id: editingUser.id, data: { newPassword } }).unwrap();
      setResetPasswordModalOpen(false);
      setNewPassword('');
    } catch (error) {
      console.error('Failed to reset password:', error);
    }
  };

  return (
    <div className="users">
      <header className="users__header">
        <div className="users__title-section">
          <h1 className="users__title">{t('users.title', 'Users')}</h1>
        </div>
        <div className="users__actions">
          <button className="btn btn-primary" onClick={handleAddUser}>
            {t('users.addUser', 'Add User')}
          </button>
        </div>
      </header>

      <div className="users__toolbar">
        <div className="users__search">
          <input
            type="search"
            className="users__search-input"
            placeholder={t('common.search', 'Search...')}
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
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

      <FullscreenModal
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        title={editingUser ? t('users.editUser', 'Edit User') : t('users.addUser', 'Add User')}
        subtitle={editingUser ? `${editingUser.firstName} ${editingUser.lastName}` : t('users.createSubtitle', 'Create a new user account')}
        onSave={handleSubmit(onSubmit)}
        saveLabel={editingUser ? t('common.save', 'Save') : t('common.create', 'Create')}
        loading={isCreating || isUpdating}
        maxWidth="md"
      >
        <form onSubmit={handleSubmit(onSubmit)}>
          <ModalSection title={t('users.accountInfo', 'Account Information')} description={t('users.accountInfoDesc', 'Basic user credentials')}>
            <div className="form-grid">
              <div className="form-field">
                <label className="form-field__label">{t('users.email', 'Email')}</label>
                <input
                  type="email"
                  className={`form-field__input ${errors.email ? 'form-field__input--error' : ''}`}
                  {...register('email', { required: 'Email is required' })}
                  placeholder="Enter email"
                />
                {errors.email && <span className="form-field__error">{errors.email.message}</span>}
              </div>
              {!editingUser && (
                <div className="form-field">
                  <label className="form-field__label">{t('users.password', 'Password')}</label>
                  <input
                    type="password"
                    className={`form-field__input ${errors.password ? 'form-field__input--error' : ''}`}
                    {...register('password', { required: !editingUser ? 'Password is required' : false, minLength: { value: 8, message: 'Minimum 8 characters' } })}
                    placeholder="Enter password"
                  />
                  {errors.password && <span className="form-field__error">{errors.password.message}</span>}
                </div>
              )}
            </div>
          </ModalSection>

          <ModalSection title={t('users.personalDetails', 'Personal Details')}>
            <div className="form-grid">
              <div className="form-field">
                <label className="form-field__label">{t('users.firstName', 'First Name')}</label>
                <input
                  type="text"
                  className={`form-field__input ${errors.firstName ? 'form-field__input--error' : ''}`}
                  {...register('firstName', { required: 'First name is required' })}
                  placeholder="Enter first name"
                />
                {errors.firstName && <span className="form-field__error">{errors.firstName.message}</span>}
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('users.lastName', 'Last Name')}</label>
                <input
                  type="text"
                  className={`form-field__input ${errors.lastName ? 'form-field__input--error' : ''}`}
                  {...register('lastName', { required: 'Last name is required' })}
                  placeholder="Enter last name"
                />
                {errors.lastName && <span className="form-field__error">{errors.lastName.message}</span>}
              </div>
            </div>
          </ModalSection>

          <ModalSection title={t('users.accessSettings', 'Access Settings')} description={t('users.accessSettingsDesc', 'Role and permissions')}>
            <div className="form-grid--single">
              <div className="form-field">
                <label className="form-field__label">{t('users.role', 'Role')}</label>
                <Controller
                  name="roleName"
                  control={control}
                  render={({ field }) => (
                    <select className="form-field__select" {...field}>
                      <option value="">Select role...</option>
                      {roles.map((role) => (
                        <option key={role.id} value={role.name}>
                          {role.name}
                        </option>
                      ))}
                    </select>
                  )}
                />
              </div>
            </div>
          </ModalSection>

          {editingUser && (
            <ModalSection title={t('users.actions', 'Actions')}>
              <div className="users__modal-actions">
                <button
                  type="button"
                  className={`btn ${editingUser.isActive ? 'btn-warning' : 'btn-success'}`}
                  onClick={handleToggleStatus}
                >
                  {editingUser.isActive ? t('users.deactivate', 'Deactivate User') : t('users.activate', 'Activate User')}
                </button>
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={() => setResetPasswordModalOpen(true)}
                >
                  {t('users.resetPassword', 'Reset Password')}
                </button>
              </div>
            </ModalSection>
          )}
        </form>
      </FullscreenModal>

      {/* Reset Password Modal */}
      <FullscreenModal
        open={resetPasswordModalOpen}
        onClose={() => {
          setResetPasswordModalOpen(false);
          setNewPassword('');
        }}
        title={t('users.resetPassword', 'Reset Password')}
        subtitle={editingUser ? `${editingUser.firstName} ${editingUser.lastName}` : ''}
        onSave={handleResetPassword}
        saveLabel={t('users.resetPassword', 'Reset Password')}
        loading={isResettingPassword}
        maxWidth="sm"
      >
        <ModalSection title={t('users.newPassword', 'New Password')}>
          <div className="form-field">
            <label className="form-field__label">{t('users.password', 'Password')}</label>
            <input
              type="password"
              className="form-field__input"
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              placeholder="Enter new password"
              minLength={8}
            />
            <span className="form-field__hint">Minimum 8 characters</span>
          </div>
        </ModalSection>
      </FullscreenModal>
    </div>
  );
}

export default Users;
