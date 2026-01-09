import { useIntl } from 'react-intl';
import { useNavigate } from 'react-router-dom';
import { useCreateRoleMutation } from '../../../api/modules/users';
import { CONFIG } from '../../../constants/routes';
import { RoleForm, type RoleFormData } from './RoleForm';
import './Roles.scss';

/**
 * RoleCreate - Container for creating new role
 *
 * Passes empty form to RoleForm.
 * Handles create mutation.
 */
export function RoleCreate() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();

  const [createRole, { isLoading: isCreating }] = useCreateRoleMutation();

  const handleBack = () => {
    navigate(CONFIG.ROLES);
  };

  const handleSubmit = async (data: RoleFormData) => {
    await createRole({
      name: data.name,
      description: data.description,
      permissions: data.permissions,
    }).unwrap();
    navigate(CONFIG.ROLES);
  };

  return (
    <div className="role-details">
      {/* Header with back button */}
      <header className="role-details__header">
        <div className="role-details__header-left">
          <button className="btn btn-ghost" onClick={handleBack}>
            <span className="btn__icon">&larr;</span>
            {t('common.back', 'Back')}
          </button>
          <div className="role-details__title-section">
            <h1 className="role-details__title">{t('roles.addRole', 'Add Role')}</h1>
          </div>
        </div>
      </header>

      {/* Main content */}
      <div className="role-details__content">
        <div className="role-details__form-container role-details__form-container--full">
          <RoleForm
            onSubmit={handleSubmit}
            loading={isCreating}
            isEditMode={false}
          />
        </div>
      </div>
    </div>
  );
}

export default RoleCreate;
