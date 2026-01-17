import { useEffect, useMemo } from 'react';
import { useForm } from 'react-hook-form';
import { useTranslate } from '@/hooks';
import { useGetPermissionsQuery } from '../../../api/modules/users';

export interface RoleFormData {
  name: string;
  description: string;
  permissions: string[];
}

interface RoleFormProps {
  initialData?: Partial<RoleFormData>;
  onSubmit: (data: RoleFormData) => Promise<void>;
  loading?: boolean;
  isEditMode?: boolean;
  isSystemRole?: boolean;
}

/**
 * RoleForm - Pure form component
 *
 * Receives initialData from parent container.
 * Knows nothing about data fetching or API calls.
 */
export function RoleForm({ initialData, onSubmit, loading, isEditMode, isSystemRole }: RoleFormProps) {
  const t = useTranslate();

  const { data: permissionsResponse } = useGetPermissionsQuery();
  const availablePermissions = useMemo(() => permissionsResponse?.data || [], [permissionsResponse?.data]);

  const {
    register,
    handleSubmit,
    reset,
    watch,
    setValue,
    formState: { errors, isDirty },
  } = useForm<RoleFormData>({
    defaultValues: {
      name: '',
      description: '',
      permissions: [],
    },
  });

  const selectedPermissions = watch('permissions') || [];

  // Reset form when initialData changes
  useEffect(() => {
    if (initialData) {
      reset({
        name: initialData.name || '',
        description: initialData.description || '',
        permissions: initialData.permissions || [],
      });
    }
  }, [initialData, reset]);

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

  const handleFormSubmit = handleSubmit(async (data) => {
    await onSubmit(data);
  });

  const togglePermission = (permission: string) => {
    const current = selectedPermissions || [];
    if (current.includes(permission)) {
      setValue('permissions', current.filter((p) => p !== permission), { shouldDirty: true });
    } else {
      setValue('permissions', [...current, permission], { shouldDirty: true });
    }
  };

  const toggleGroup = (groupPermissions: string[]) => {
    const current = selectedPermissions || [];
    const allSelected = groupPermissions.every((p) => current.includes(p));

    if (allSelected) {
      setValue('permissions', current.filter((p) => !groupPermissions.includes(p)), { shouldDirty: true });
    } else {
      const newPerms = new Set([...current, ...groupPermissions]);
      setValue('permissions', Array.from(newPerms), { shouldDirty: true });
    }
  };

  return (
    <form className="role-form" onSubmit={handleFormSubmit}>
      {/* Role Details */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('roles.details', 'Role Details')}</h3>
        </div>
        <div className="form-section__content">
          <div className="form-grid">
            <div className="form-field">
              <label className="form-field__label">{t('roles.roleName', 'Role Name')}</label>
              <input
                type="text"
                className={`form-field__input ${errors.name ? 'form-field__input--error' : ''}`}
                {...register('name', { required: 'Role name is required' })}
                placeholder="Enter role name"
                disabled={isSystemRole}
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
                disabled={isSystemRole}
              />
            </div>
          </div>
        </div>
      </section>

      {/* Permissions */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('roles.permissions', 'Permissions')}</h3>
          <p className="form-section__description">{t('roles.permissionsDesc', 'Select permissions for this role')}</p>
        </div>
        <div className="form-section__content">
          <div className="permissions-grid">
            {permissionGroups.map((group) => {
              const allSelected = group.permissions.every((p) => selectedPermissions.includes(p));

              return (
                <div key={group.group} className="permission-group">
                  <div className="permission-group__header">
                    <label className="permission-checkbox permission-checkbox--group">
                      <input
                        type="checkbox"
                        checked={allSelected}
                        onChange={() => toggleGroup(group.permissions)}
                        disabled={isSystemRole}
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
                          disabled={isSystemRole}
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
        </div>
      </section>

      {/* Form Actions */}
      {!isSystemRole && (
        <div className="form-actions">
          <button
            type="submit"
            className="btn btn-primary"
            disabled={loading || (!isDirty && isEditMode)}
          >
            {loading ? t('common.saving', 'Saving...') : isEditMode ? t('common.save', 'Save') : t('common.create', 'Create')}
          </button>
        </div>
      )}
    </form>
  );
}

export default RoleForm;
