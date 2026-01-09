import { useEffect } from 'react';
import { useIntl } from 'react-intl';
import { useForm, Controller } from 'react-hook-form';
import { useGetRolesQuery, type RoleResponse } from '../../../api/modules/users';

export interface UserFormData {
  email: string;
  firstName: string;
  lastName: string;
  password: string;
  roleName: string;
}

interface UserFormProps {
  initialData?: Partial<UserFormData>;
  onSubmit: (data: UserFormData) => Promise<void>;
  loading?: boolean;
  isEditMode?: boolean;
}

/**
 * UserForm - Pure form component
 *
 * Receives initialData from parent container.
 * Knows nothing about data fetching or API calls.
 */
export function UserForm({ initialData, onSubmit, loading, isEditMode }: UserFormProps) {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });

  // tenantId is automatically injected by baseApi
  const { data: rolesResponse } = useGetRolesQuery();
  const roles = rolesResponse?.data || [];

  const {
    register,
    handleSubmit,
    control,
    reset,
    formState: { errors, isDirty },
  } = useForm<UserFormData>({
    defaultValues: {
      email: '',
      firstName: '',
      lastName: '',
      password: '',
      roleName: '',
    },
  });

  // Reset form when initialData changes
  useEffect(() => {
    if (initialData) {
      reset({
        email: initialData.email || '',
        firstName: initialData.firstName || '',
        lastName: initialData.lastName || '',
        password: '',
        roleName: initialData.roleName || '',
      });
    }
  }, [initialData, reset]);

  const handleFormSubmit = handleSubmit(async (data) => {
    await onSubmit(data);
  });

  return (
    <form className="user-form" onSubmit={handleFormSubmit}>
      {/* Account Information */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('users.accountInfo', 'Account Information')}</h3>
          <p className="form-section__description">{t('users.accountInfoDesc', 'Basic user credentials')}</p>
        </div>
        <div className="form-section__content">
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
            {!isEditMode && (
              <div className="form-field">
                <label className="form-field__label">{t('users.password', 'Password')}</label>
                <input
                  type="password"
                  className={`form-field__input ${errors.password ? 'form-field__input--error' : ''}`}
                  {...register('password', {
                    required: !isEditMode ? 'Password is required' : false,
                    minLength: { value: 8, message: 'Minimum 8 characters' }
                  })}
                  placeholder="Enter password"
                />
                {errors.password && <span className="form-field__error">{errors.password.message}</span>}
              </div>
            )}
          </div>
        </div>
      </section>

      {/* Personal Details */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('users.personalDetails', 'Personal Details')}</h3>
        </div>
        <div className="form-section__content">
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
        </div>
      </section>

      {/* Access Settings */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('users.accessSettings', 'Access Settings')}</h3>
          <p className="form-section__description">{t('users.accessSettingsDesc', 'Role and permissions')}</p>
        </div>
        <div className="form-section__content">
          <div className="form-grid--single">
            <div className="form-field">
              <label className="form-field__label">{t('users.role', 'Role')}</label>
              <Controller
                name="roleName"
                control={control}
                render={({ field }) => (
                  <select className="form-field__select" {...field}>
                    <option value="">Select role...</option>
                    {roles.map((role: RoleResponse) => (
                      <option key={role.id} value={role.name}>
                        {role.name}
                      </option>
                    ))}
                  </select>
                )}
              />
            </div>
          </div>
        </div>
      </section>

      {/* Form Actions */}
      <div className="form-actions">
        <button
          type="submit"
          className="btn btn-primary"
          disabled={loading || (!isDirty && isEditMode)}
        >
          {loading ? t('common.saving', 'Saving...') : isEditMode ? t('common.save', 'Save') : t('common.create', 'Create')}
        </button>
      </div>
    </form>
  );
}

export default UserForm;
