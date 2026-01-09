import { useIntl } from 'react-intl';
import { useNavigate } from 'react-router-dom';
import { useCreateUserMutation, type CreateUserRequest } from '../../../api/modules/users';
import { CONFIG } from '../../../constants/routes';
import { UserForm, type UserFormData } from './UserForm';
import './Users.scss';

/**
 * UserCreate - Container for creating new user
 *
 * Passes empty form to UserForm.
 * Handles create mutation.
 */
export function UserCreate() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();

  const [createUser, { isLoading: isCreating }] = useCreateUserMutation();

  const handleBack = () => {
    navigate(CONFIG.USERS);
  };

  const handleSubmit = async (data: UserFormData) => {
    const createData: CreateUserRequest = {
      email: data.email,
      firstName: data.firstName,
      lastName: data.lastName,
      password: data.password,
      roleName: data.roleName || undefined,
    };

    await createUser(createData).unwrap();
    navigate(CONFIG.USERS);
  };

  return (
    <div className="user-details">
      {/* Header with back button */}
      <header className="user-details__header">
        <div className="user-details__header-left">
          <button className="btn btn-ghost" onClick={handleBack}>
            <span className="btn__icon">&larr;</span>
            {t('common.back', 'Back')}
          </button>
          <div className="user-details__title-section">
            <h1 className="user-details__title">{t('users.addUser', 'Add User')}</h1>
          </div>
        </div>
      </header>

      {/* Main content */}
      <div className="user-details__content">
        <div className="user-details__form-container user-details__form-container--full">
          <UserForm
            onSubmit={handleSubmit}
            loading={isCreating}
            isEditMode={false}
          />
        </div>
      </div>
    </div>
  );
}

export default UserCreate;
