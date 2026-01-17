import { useEffect, useMemo } from 'react';
import { useForm } from 'react-hook-form';
import { useTranslate } from '@/hooks';
import { useGetPermissionsQuery, type PermissionGroupDto } from '../../../api/modules/users';

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

  // Parse the nested permission structure from the API
  // Backend returns: [{ module, moduleName, categories: [{ category, categoryName, permissions: [{ code, name, ... }] }] }]
  const permissionModules = useMemo((): PermissionGroupDto[] => {
    return (permissionsResponse?.data || []) as PermissionGroupDto[];
  }, [permissionsResponse?.data]);

  // Flatten all permission codes for lookups
  const availablePermissions = useMemo(() => {
    const perms: string[] = [];
    permissionModules.forEach((mod) => {
      mod.categories?.forEach((cat) => {
        cat.permissions?.forEach((perm) => {
          perms.push(perm.code);
        });
      });
    });
    return perms;
  }, [permissionModules]);

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

  // Use the permission modules structure directly for rendering
  const permissionGroups = useMemo(() => {
    return permissionModules.map((mod) => ({
      moduleName: mod.moduleName,
      categories: mod.categories.map((cat) => ({
        categoryName: cat.categoryName,
        permissions: cat.permissions.map((perm) => ({
          code: perm.code,
          name: perm.name,
          description: perm.description,
        })),
      })),
    }));
  }, [permissionModules]);

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

  const toggleCategory = (categoryPermissions: Array<{ code: string }>) => {
    const current = selectedPermissions || [];
    const codes = categoryPermissions.map((p) => p.code);
    const allSelected = codes.every((p) => current.includes(p));

    if (allSelected) {
      setValue('permissions', current.filter((p) => !codes.includes(p)), { shouldDirty: true });
    } else {
      const newPerms = new Set([...current, ...codes]);
      setValue('permissions', Array.from(newPerms), { shouldDirty: true });
    }
  };

  const toggleModule = (module: typeof permissionGroups[0]) => {
    const current = selectedPermissions || [];
    const allCodes: string[] = [];
    module.categories.forEach((cat) => {
      cat.permissions.forEach((perm) => {
        allCodes.push(perm.code);
      });
    });
    const allSelected = allCodes.every((p) => current.includes(p));

    if (allSelected) {
      setValue('permissions', current.filter((p) => !allCodes.includes(p)), { shouldDirty: true });
    } else {
      const newPerms = new Set([...current, ...allCodes]);
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
          <div className="permissions-modules">
            {permissionGroups.map((mod) => {
              const allModulePerms: string[] = [];
              mod.categories.forEach((cat) => {
                cat.permissions.forEach((perm) => allModulePerms.push(perm.code));
              });
              const allModuleSelected = allModulePerms.every((p) => selectedPermissions.includes(p));

              return (
                <div key={mod.moduleName} className="permission-module">
                  <div className="permission-module__header">
                    <label className="permission-checkbox permission-checkbox--module">
                      <input
                        type="checkbox"
                        checked={allModuleSelected}
                        onChange={() => toggleModule(mod)}
                        disabled={isSystemRole}
                      />
                      <span className="permission-checkbox__label">
                        <strong>{mod.moduleName}</strong>
                      </span>
                    </label>
                  </div>
                  <div className="permission-module__categories">
                    {mod.categories.map((cat) => {
                      const allCatSelected = cat.permissions.every((p) => selectedPermissions.includes(p.code));

                      return (
                        <div key={cat.categoryName} className="permission-category">
                          <div className="permission-category__header">
                            <label className="permission-checkbox permission-checkbox--category">
                              <input
                                type="checkbox"
                                checked={allCatSelected}
                                onChange={() => toggleCategory(cat.permissions)}
                                disabled={isSystemRole}
                              />
                              <span className="permission-checkbox__label">{cat.categoryName}</span>
                            </label>
                          </div>
                          <div className="permission-category__items">
                            {cat.permissions.map((perm) => (
                              <label key={perm.code} className="permission-checkbox" title={perm.description}>
                                <input
                                  type="checkbox"
                                  checked={selectedPermissions.includes(perm.code)}
                                  onChange={() => togglePermission(perm.code)}
                                  disabled={isSystemRole}
                                />
                                <span className="permission-checkbox__label">{perm.name}</span>
                              </label>
                            ))}
                          </div>
                        </div>
                      );
                    })}
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
