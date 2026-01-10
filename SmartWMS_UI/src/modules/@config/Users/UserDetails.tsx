import { useState } from 'react';
import { useIntl } from 'react-intl';
import { useNavigate, useParams } from 'react-router-dom';
import {
  useGetUserByIdQuery,
  useUpdateUserMutation,
  useDeactivateUserMutation,
  useActivateUserMutation,
  useResetPasswordMutation,
  useDeleteUserMutation,
  type UpdateUserRequest,
} from '../../../api/modules/users';
import { CONFIG } from '../../../constants/routes';
import { UserForm, type UserFormData } from './UserForm';
import { FullscreenModal, ModalSection } from '../../../components/FullscreenModal';

export function UserDetails() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();

  const [resetPasswordModalOpen, setResetPasswordModalOpen] = useState(false);
  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);
  const [newPassword, setNewPassword] = useState('');

  const { data: userResponse, isLoading: isLoadingUser } = useGetUserByIdQuery(
    id!,
    { skip: !id }
  );

  const [updateUser, { isLoading: isUpdating }] = useUpdateUserMutation();
  const [deactivateUser, { isLoading: isDeactivating }] = useDeactivateUserMutation();
  const [activateUser, { isLoading: isActivating }] = useActivateUserMutation();
  const [resetPassword, { isLoading: isResettingPassword }] = useResetPasswordMutation();
  const [deleteUser, { isLoading: isDeleting }] = useDeleteUserMutation();

  const user = userResponse?.data;

  const handleBack = () => {
    navigate(CONFIG.USERS);
  };

  const handleSubmit = async (data: UserFormData) => {
    if (!id) return;

    const updateData: UpdateUserRequest = {
      email: data.email,
      firstName: data.firstName,
      lastName: data.lastName,
      roleName: data.roleName || undefined,
    };

    await updateUser({ id, data: updateData }).unwrap();
    navigate(CONFIG.USERS);
  };

  const handleToggleStatus = async () => {
    if (!user || !id) return;

    if (user.isActive) {
      await deactivateUser(id).unwrap();
    } else {
      await activateUser(id).unwrap();
    }
    navigate(CONFIG.USERS);
  };

  const handleResetPassword = async () => {
    if (!id || !newPassword) return;

    await resetPassword({ id, data: { newPassword } }).unwrap();
    setResetPasswordModalOpen(false);
    setNewPassword('');
  };

  const handleDelete = async () => {
    if (!id) return;

    await deleteUser(id).unwrap();
    setDeleteConfirmOpen(false);
    navigate(CONFIG.USERS);
  };

  if (isLoadingUser) {
    return (
      <div className="detail-page">
        <div className="detail-page__loading">{t('common.loading', 'Loading...')}</div>
      </div>
    );
  }

  if (!user) {
    return (
      <div className="detail-page">
        <div className="detail-page__error">
          <h2>{t('users.notFound', 'User not found')}</h2>
          <button className="btn btn-secondary" onClick={handleBack}>
            {t('users.backToList', 'Back to Users')}
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="detail-page">
      <header className="detail-page__header">
        <div className="detail-page__header-left">
          <button className="btn btn-ghost" onClick={handleBack}>
            <span className="btn__icon">&larr;</span>
            {t('common.back', 'Back')}
          </button>
          <div className="detail-page__title-section">
            <h1 className="detail-page__title">
              {user.firstName} {user.lastName}
            </h1>
            <div className="detail-page__meta">
              <span className={`status-badge status-badge--${user.isActive ? 'success' : 'neutral'}`}>
                {user.isActive ? t('status.active', 'Active') : t('status.inactive', 'Inactive')}
              </span>
            </div>
          </div>
        </div>
      </header>

      <div className="detail-page__content detail-page__content--with-sidebar">
        <div className="detail-page__main">
          <UserForm
            initialData={{
              email: user.email,
              firstName: user.firstName,
              lastName: user.lastName,
              roleName: user.roleName || '',
            }}
            onSubmit={handleSubmit}
            loading={isUpdating}
            isEditMode
          />
        </div>

        <aside className="detail-page__sidebar">
          <section className="sidebar-section">
            <h3 className="sidebar-section__title">{t('users.actions', 'Actions')}</h3>
            <div className="sidebar-section__content">
              <button
                className={`btn btn-block ${user.isActive ? 'btn-warning' : 'btn-success'}`}
                onClick={handleToggleStatus}
                disabled={isDeactivating || isActivating}
              >
                {user.isActive ? t('users.deactivate', 'Deactivate User') : t('users.activate', 'Activate User')}
              </button>
              <button
                className="btn btn-block btn-secondary"
                onClick={() => setResetPasswordModalOpen(true)}
              >
                {t('users.resetPassword', 'Reset Password')}
              </button>
            </div>
          </section>

          <section className="sidebar-section">
            <h3 className="sidebar-section__title">{t('users.info', 'Information')}</h3>
            <div className="sidebar-section__content">
              <dl className="info-list">
                <div className="info-list__item">
                  <dt>{t('users.createdAt', 'Created')}</dt>
                  <dd>{new Date(user.createdAt).toLocaleDateString()}</dd>
                </div>
                <div className="info-list__item">
                  <dt>{t('users.lastLogin', 'Last Login')}</dt>
                  <dd>{user.lastLoginAt ? new Date(user.lastLoginAt).toLocaleDateString() : 'Never'}</dd>
                </div>
              </dl>
            </div>
          </section>

          <section className="sidebar-section sidebar-section--danger">
            <h3 className="sidebar-section__title">{t('common.dangerZone', 'Danger Zone')}</h3>
            <div className="sidebar-section__content">
              <p className="sidebar-section__text">
                {t('users.deleteWarning', 'Deleting this user will remove them permanently.')}
              </p>
              <button
                className="btn btn-danger btn-block"
                onClick={() => setDeleteConfirmOpen(true)}
                disabled={isDeleting}
              >
                {t('users.deleteUser', 'Delete User')}
              </button>
            </div>
          </section>
        </aside>
      </div>

      <FullscreenModal
        open={resetPasswordModalOpen}
        onClose={() => {
          setResetPasswordModalOpen(false);
          setNewPassword('');
        }}
        title={t('users.resetPassword', 'Reset Password')}
        subtitle={`${user.firstName} ${user.lastName}`}
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

      <FullscreenModal
        open={deleteConfirmOpen}
        onClose={() => setDeleteConfirmOpen(false)}
        title={t('users.confirmDelete', 'Confirm Delete')}
        subtitle={`${user.firstName} ${user.lastName}`}
        onSave={handleDelete}
        saveLabel={t('common.delete', 'Delete')}
        loading={isDeleting}
        maxWidth="sm"
      >
        <ModalSection title="">
          <p>{t('users.deleteConfirmText', 'Are you sure you want to delete this user? This action cannot be undone.')}</p>
        </ModalSection>
      </FullscreenModal>
    </div>
  );
}

export default UserDetails;
