import { useState } from 'react';
import { useIntl } from 'react-intl';
import { useNavigate, useParams } from 'react-router-dom';
import {
  useGetRoleByIdQuery,
  useUpdateRoleMutation,
  useDeleteRoleMutation,
} from '../../../api/modules/users';
import { CONFIG } from '../../../constants/routes';
import { RoleForm, type RoleFormData } from './RoleForm';
import { FullscreenModal, ModalSection } from '../../../components/FullscreenModal';

export function RoleDetails() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();

  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);

  const { data: roleResponse, isLoading: isLoadingRole } = useGetRoleByIdQuery(
    id!,
    { skip: !id }
  );

  const [updateRole, { isLoading: isUpdating }] = useUpdateRoleMutation();
  const [deleteRole, { isLoading: isDeleting }] = useDeleteRoleMutation();

  const role = roleResponse?.data;

  const handleBack = () => {
    navigate(CONFIG.ROLES);
  };

  const handleSubmit = async (data: RoleFormData) => {
    if (!id) return;

    await updateRole({
      id,
      data: {
        name: data.name,
        description: data.description,
        permissions: data.permissions,
      },
    }).unwrap();
    navigate(CONFIG.ROLES);
  };

  const handleDelete = async () => {
    if (!id) return;

    await deleteRole(id).unwrap();
    setDeleteConfirmOpen(false);
    navigate(CONFIG.ROLES);
  };

  if (isLoadingRole) {
    return (
      <div className="detail-page">
        <div className="detail-page__loading">{t('common.loading', 'Loading...')}</div>
      </div>
    );
  }

  if (!role) {
    return (
      <div className="detail-page">
        <div className="detail-page__error">
          <h2>{t('roles.notFound', 'Role not found')}</h2>
          <button className="btn btn-secondary" onClick={handleBack}>
            {t('roles.backToList', 'Back to Roles')}
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
            <h1 className="detail-page__title">{role.name}</h1>
            <div className="detail-page__meta">
              {role.isSystemRole && (
                <span className="status-badge status-badge--info">{t('roles.system', 'System')}</span>
              )}
            </div>
          </div>
        </div>
      </header>

      <div className="detail-page__content detail-page__content--with-sidebar">
        <div className="detail-page__main">
          <RoleForm
            initialData={{
              name: role.name,
              description: role.description || '',
              permissions: role.permissions || [],
            }}
            onSubmit={handleSubmit}
            loading={isUpdating}
            isEditMode
            isSystemRole={role.isSystemRole}
          />
        </div>

        <aside className="detail-page__sidebar">
          <section className="sidebar-section">
            <h3 className="sidebar-section__title">{t('roles.info', 'Information')}</h3>
            <div className="sidebar-section__content">
              <dl className="info-list">
                <div className="info-list__item">
                  <dt>{t('roles.assignedUsers', 'Assigned Users')}</dt>
                  <dd>{role.userCount}</dd>
                </div>
                <div className="info-list__item">
                  <dt>{t('roles.permissions', 'Permissions')}</dt>
                  <dd>{role.permissions?.length || 0}</dd>
                </div>
              </dl>
            </div>
          </section>

          {!role.isSystemRole && (
            <section className="sidebar-section sidebar-section--danger">
              <h3 className="sidebar-section__title">{t('common.dangerZone', 'Danger Zone')}</h3>
              <div className="sidebar-section__content">
                <p className="sidebar-section__text">
                  {t('roles.deleteWarning', 'Deleting this role will remove it permanently.')}
                </p>
                <button
                  className="btn btn-danger btn-block"
                  onClick={() => setDeleteConfirmOpen(true)}
                  disabled={isDeleting}
                >
                  {t('roles.deleteRole', 'Delete Role')}
                </button>
              </div>
            </section>
          )}
        </aside>
      </div>

      <FullscreenModal
        open={deleteConfirmOpen}
        onClose={() => setDeleteConfirmOpen(false)}
        title={t('roles.confirmDelete', 'Confirm Delete')}
        subtitle={role.name}
        onSave={handleDelete}
        saveLabel={t('common.delete', 'Delete')}
        loading={isDeleting}
        maxWidth="sm"
      >
        <ModalSection title="">
          <p>{t('roles.deleteConfirmText', 'Are you sure you want to delete this role? This action cannot be undone.')}</p>
        </ModalSection>
      </FullscreenModal>
    </div>
  );
}

export default RoleDetails;
