import { useState, useMemo, useEffect } from 'react';
import { useIntl } from 'react-intl';
import { useForm } from 'react-hook-form';
import { DataTable, createColumns } from '@/components/DataTable';
import type { PaginationState, SortingState } from '@/components/DataTable';
import { FullscreenModal, ModalSection } from '@/components/FullscreenModal';
import { Modal } from '@/components';
import {
  useGetSuppliersQuery,
  useCreateSupplierMutation,
  useUpdateSupplierMutation,
  useDeleteSupplierMutation,
} from '@/api/modules/orders';
import type { SupplierDto, CreateSupplierRequest, UpdateSupplierRequest } from '@/api/modules/orders';

interface SupplierFormData {
  code: string;
  name: string;
  contactName: string;
  email: string;
  phone: string;
  addressLine1: string;
  addressLine2: string;
  city: string;
  region: string;
  postalCode: string;
  countryCode: string;
  taxId: string;
  paymentTerms: string;
  leadTimeDays: number | '';
  isActive: boolean;
}

export function Suppliers() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });

  const [searchQuery, setSearchQuery] = useState('');
  const [activeFilter, setActiveFilter] = useState<'' | 'true' | 'false'>('');
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 });
  const [sorting, setSorting] = useState<SortingState>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  // Modal state
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingSupplier, setEditingSupplier] = useState<SupplierDto | null>(null);
  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);

  // API
  const { data: response, isLoading } = useGetSuppliersQuery({
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
    search: searchQuery || undefined,
    isActive: activeFilter === '' ? undefined : activeFilter === 'true',
  });

  const [createSupplier, { isLoading: isCreating }] = useCreateSupplierMutation();
  const [updateSupplier, { isLoading: isUpdating }] = useUpdateSupplierMutation();
  const [deleteSupplier, { isLoading: isDeleting }] = useDeleteSupplierMutation();

  const suppliers = response?.data?.items || [];
  const totalRows = response?.data?.totalCount || 0;

  // Form
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<SupplierFormData>({
    defaultValues: {
      code: '',
      name: '',
      contactName: '',
      email: '',
      phone: '',
      addressLine1: '',
      addressLine2: '',
      city: '',
      region: '',
      postalCode: '',
      countryCode: '',
      taxId: '',
      paymentTerms: '',
      leadTimeDays: '',
      isActive: true,
    },
  });

  // Reset form when editing supplier changes
  useEffect(() => {
    if (editingSupplier) {
      reset({
        code: editingSupplier.code,
        name: editingSupplier.name,
        contactName: editingSupplier.contactName || '',
        email: editingSupplier.email || '',
        phone: editingSupplier.phone || '',
        addressLine1: editingSupplier.addressLine1 || '',
        addressLine2: editingSupplier.addressLine2 || '',
        city: editingSupplier.city || '',
        region: editingSupplier.region || '',
        postalCode: editingSupplier.postalCode || '',
        countryCode: editingSupplier.countryCode || '',
        taxId: editingSupplier.taxId || '',
        paymentTerms: editingSupplier.paymentTerms || '',
        leadTimeDays: editingSupplier.leadTimeDays || '',
        isActive: editingSupplier.isActive,
      });
    } else {
      reset({
        code: '',
        name: '',
        contactName: '',
        email: '',
        phone: '',
        addressLine1: '',
        addressLine2: '',
        city: '',
        region: '',
        postalCode: '',
        countryCode: '',
        taxId: '',
        paymentTerms: '',
        leadTimeDays: '',
        isActive: true,
      });
    }
  }, [editingSupplier, reset]);

  const columnHelper = createColumns<SupplierDto>();

  const columns = useMemo(
    () => [
      columnHelper.accessor('code', {
        header: t('suppliers.code', 'Code'),
        size: 100,
        cell: ({ getValue }) => <span className="code">{getValue()}</span>,
      }),
      columnHelper.accessor('name', {
        header: t('suppliers.name', 'Name'),
        size: 180,
      }),
      columnHelper.accessor('contactName', {
        header: t('suppliers.contact', 'Contact'),
        size: 140,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('email', {
        header: t('suppliers.email', 'Email'),
        size: 180,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('phone', {
        header: t('suppliers.phone', 'Phone'),
        size: 120,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('leadTimeDays', {
        header: t('suppliers.leadTime', 'Lead Time'),
        size: 100,
        cell: ({ getValue }) => {
          const days = getValue();
          return days ? `${days} days` : '-';
        },
      }),
      columnHelper.accessor('orderCount', {
        header: t('suppliers.orders', 'Orders'),
        size: 80,
      }),
      columnHelper.accessor('isActive', {
        header: t('common.status', 'Status'),
        size: 100,
        cell: ({ getValue }) => {
          const isActive = getValue();
          return (
            <span className={`status-badge status-badge--${isActive ? 'active' : 'inactive'}`}>
              {isActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
            </span>
          );
        },
      }),
    ],
    [columnHelper, t]
  );

  const handleRowClick = (supplier: SupplierDto) => {
    setSelectedId(supplier.id);
    setEditingSupplier(supplier);
    setIsModalOpen(true);
  };

  const handleAddNew = () => {
    setEditingSupplier(null);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setEditingSupplier(null);
  };

  const onSubmit = async (data: SupplierFormData) => {
    try {
      const leadTimeDays = data.leadTimeDays === '' ? undefined : Number(data.leadTimeDays);

      if (editingSupplier) {
        // Update existing supplier
        const updateData: UpdateSupplierRequest = {
          name: data.name,
          contactName: data.contactName || undefined,
          email: data.email || undefined,
          phone: data.phone || undefined,
          addressLine1: data.addressLine1 || undefined,
          addressLine2: data.addressLine2 || undefined,
          city: data.city || undefined,
          region: data.region || undefined,
          postalCode: data.postalCode || undefined,
          countryCode: data.countryCode || undefined,
          taxId: data.taxId || undefined,
          paymentTerms: data.paymentTerms || undefined,
          leadTimeDays,
          isActive: data.isActive,
        };
        await updateSupplier({ id: editingSupplier.id, body: updateData }).unwrap();
      } else {
        // Create new supplier
        const createData: CreateSupplierRequest = {
          code: data.code,
          name: data.name,
          contactName: data.contactName || undefined,
          email: data.email || undefined,
          phone: data.phone || undefined,
          addressLine1: data.addressLine1 || undefined,
          addressLine2: data.addressLine2 || undefined,
          city: data.city || undefined,
          region: data.region || undefined,
          postalCode: data.postalCode || undefined,
          countryCode: data.countryCode || undefined,
          taxId: data.taxId || undefined,
          paymentTerms: data.paymentTerms || undefined,
          leadTimeDays,
          isActive: data.isActive,
        };
        await createSupplier(createData).unwrap();
      }
      handleCloseModal();
    } catch (error) {
      console.error('Failed to save supplier:', error);
    }
  };

  const handleDelete = async () => {
    if (!editingSupplier) return;

    try {
      await deleteSupplier(editingSupplier.id).unwrap();
      setDeleteConfirmOpen(false);
      handleCloseModal();
    } catch (error) {
      console.error('Failed to delete supplier:', error);
    }
  };

  const isSaving = isCreating || isUpdating;

  return (
    <div className="page">
      <header className="page__header">
        <div className="page__title-section">
          <h1 className="page__title">{t('suppliers.title', 'Suppliers')}</h1>
          <p className="page__subtitle">
            {t('suppliers.subtitle', 'Manage supplier accounts')}
          </p>
        </div>
        <div className="page__actions">
          <button className="btn btn--primary" onClick={handleAddNew}>
            {t('suppliers.addSupplier', 'Add Supplier')}
          </button>
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
        <div className="page-filters">
          <select
            className="page-filter__select"
            value={activeFilter}
            onChange={(e) => setActiveFilter(e.target.value as '' | 'true' | 'false')}
          >
            <option value="">{t('suppliers.allStatuses', 'All')}</option>
            <option value="true">{t('common.active', 'Active')}</option>
            <option value="false">{t('common.inactive', 'Inactive')}</option>
          </select>
        </div>
      </div>

      <div className="page__content">
        <DataTable
          data={suppliers}
          columns={columns}
          pagination={pagination}
          onPaginationChange={setPagination}
          sorting={sorting}
          onSortingChange={setSorting}
          totalRows={totalRows}
          selectedRowId={selectedId}
          onRowClick={handleRowClick}
          getRowId={(row) => row.id}
          emptyMessage={t('suppliers.noSuppliers', 'No suppliers found')}
          loading={isLoading}
        />
      </div>

      {/* Supplier Form Modal */}
      <FullscreenModal
        open={isModalOpen}
        onClose={handleCloseModal}
        title={editingSupplier ? t('suppliers.editSupplier', 'Edit Supplier') : t('suppliers.addSupplier', 'Add Supplier')}
        subtitle={editingSupplier ? `${editingSupplier.code} - ${editingSupplier.name}` : t('suppliers.addSupplierSubtitle', 'Fill in supplier details')}
        onSave={handleSubmit(onSubmit)}
        saveLabel={isSaving ? t('common.saving', 'Saving...') : t('common.save', 'Save')}
        saveDisabled={isSaving}
        maxWidth="lg"
      >
        <form>
          <ModalSection title={t('suppliers.basicInfo', 'Basic Information')}>
            <div className="form-grid">
              <div className="form-field">
                <label className="form-field__label">
                  {t('suppliers.code', 'Code')} <span className="required">*</span>
                </label>
                <input
                  type="text"
                  className={`form-field__input ${errors.code ? 'form-field__input--error' : ''}`}
                  placeholder="SUPP001"
                  disabled={!!editingSupplier}
                  {...register('code', { required: t('validation.required', 'Required') })}
                />
                {errors.code && <span className="form-field__error">{errors.code.message}</span>}
              </div>
              <div className="form-field">
                <label className="form-field__label">
                  {t('suppliers.name', 'Name')} <span className="required">*</span>
                </label>
                <input
                  type="text"
                  className={`form-field__input ${errors.name ? 'form-field__input--error' : ''}`}
                  placeholder="Supplier Name"
                  {...register('name', { required: t('validation.required', 'Required') })}
                />
                {errors.name && <span className="form-field__error">{errors.name.message}</span>}
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('suppliers.contact', 'Contact Person')}</label>
                <input
                  type="text"
                  className="form-field__input"
                  placeholder="John Doe"
                  {...register('contactName')}
                />
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('suppliers.email', 'Email')}</label>
                <input
                  type="email"
                  className="form-field__input"
                  placeholder="email@example.com"
                  {...register('email')}
                />
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('suppliers.phone', 'Phone')}</label>
                <input
                  type="tel"
                  className="form-field__input"
                  placeholder="+1 234 567 8900"
                  {...register('phone')}
                />
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('common.status', 'Status')}</label>
                <label className="form-checkbox">
                  <input type="checkbox" {...register('isActive')} />
                  <span>{t('common.active', 'Active')}</span>
                </label>
              </div>
            </div>
          </ModalSection>

          <ModalSection title={t('suppliers.address', 'Address')} collapsible defaultExpanded>
            <div className="form-grid">
              <div className="form-field form-field--full">
                <label className="form-field__label">{t('suppliers.addressLine1', 'Address Line 1')}</label>
                <input
                  type="text"
                  className="form-field__input"
                  placeholder="123 Main Street"
                  {...register('addressLine1')}
                />
              </div>
              <div className="form-field form-field--full">
                <label className="form-field__label">{t('suppliers.addressLine2', 'Address Line 2')}</label>
                <input
                  type="text"
                  className="form-field__input"
                  placeholder="Suite 100"
                  {...register('addressLine2')}
                />
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('suppliers.city', 'City')}</label>
                <input
                  type="text"
                  className="form-field__input"
                  placeholder="New York"
                  {...register('city')}
                />
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('suppliers.region', 'Region/State')}</label>
                <input
                  type="text"
                  className="form-field__input"
                  placeholder="NY"
                  {...register('region')}
                />
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('suppliers.postalCode', 'Postal Code')}</label>
                <input
                  type="text"
                  className="form-field__input"
                  placeholder="10001"
                  {...register('postalCode')}
                />
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('suppliers.country', 'Country')}</label>
                <input
                  type="text"
                  className="form-field__input"
                  placeholder="US"
                  {...register('countryCode')}
                />
              </div>
            </div>
          </ModalSection>

          <ModalSection title={t('suppliers.businessInfo', 'Business Information')} collapsible defaultExpanded={false}>
            <div className="form-grid">
              <div className="form-field">
                <label className="form-field__label">{t('suppliers.taxId', 'Tax ID')}</label>
                <input
                  type="text"
                  className="form-field__input"
                  placeholder="XX-XXXXXXX"
                  {...register('taxId')}
                />
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('suppliers.paymentTerms', 'Payment Terms')}</label>
                <input
                  type="text"
                  className="form-field__input"
                  placeholder="Net 30"
                  {...register('paymentTerms')}
                />
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('suppliers.leadTimeDays', 'Lead Time (days)')}</label>
                <input
                  type="number"
                  min="0"
                  className="form-field__input"
                  placeholder="7"
                  {...register('leadTimeDays')}
                />
              </div>
            </div>
          </ModalSection>

          {editingSupplier && (
            <ModalSection title={t('common.dangerZone', 'Danger Zone')}>
              <div className="danger-zone">
                <p>{t('suppliers.deleteWarning', 'Deleting a supplier will remove all associated data.')}</p>
                <button
                  type="button"
                  className="btn btn--danger"
                  onClick={() => setDeleteConfirmOpen(true)}
                >
                  {t('suppliers.deleteSupplier', 'Delete Supplier')}
                </button>
              </div>
            </ModalSection>
          )}
        </form>
      </FullscreenModal>

      {/* Delete Confirmation Modal */}
      <Modal
        open={deleteConfirmOpen}
        onClose={() => setDeleteConfirmOpen(false)}
        title={t('suppliers.deleteSupplier', 'Delete Supplier')}
      >
        <div className="modal__body">
          <p>
            {t(
              'suppliers.deleteConfirmation',
              `Are you sure you want to delete ${editingSupplier?.name}? This action cannot be undone.`
            )}
          </p>
        </div>
        <div className="modal__actions">
          <button className="btn btn-ghost" onClick={() => setDeleteConfirmOpen(false)}>
            {t('common.cancel', 'Cancel')}
          </button>
          <button className="btn btn--danger" onClick={handleDelete} disabled={isDeleting}>
            {isDeleting ? t('common.deleting', 'Deleting...') : t('common.delete', 'Delete')}
          </button>
        </div>
      </Modal>
    </div>
  );
}

export default Suppliers;
