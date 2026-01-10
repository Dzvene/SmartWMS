import { useState, useMemo, useEffect } from 'react';
import { useIntl } from 'react-intl';
import { useForm } from 'react-hook-form';
import { DataTable, createColumns } from '@/components/DataTable';
import type { PaginationState, SortingState } from '@/components/DataTable';
import { FullscreenModal, ModalSection } from '@/components/FullscreenModal';
import { Modal } from '@/components';
import {
  useGetCustomersQuery,
  useCreateCustomerMutation,
  useUpdateCustomerMutation,
  useDeleteCustomerMutation,
} from '@/api/modules/orders';
import type { CustomerDto, CreateCustomerRequest, UpdateCustomerRequest } from '@/api/modules/orders';

interface CustomerFormData {
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
  isActive: boolean;
}

export function Customers() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });

  const [searchQuery, setSearchQuery] = useState('');
  const [activeFilter, setActiveFilter] = useState<'' | 'true' | 'false'>('');
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 });
  const [sorting, setSorting] = useState<SortingState>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  // Modal state
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingCustomer, setEditingCustomer] = useState<CustomerDto | null>(null);
  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);

  // API
  const { data: response, isLoading } = useGetCustomersQuery({
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
    search: searchQuery || undefined,
    isActive: activeFilter === '' ? undefined : activeFilter === 'true',
  });

  const [createCustomer, { isLoading: isCreating }] = useCreateCustomerMutation();
  const [updateCustomer, { isLoading: isUpdating }] = useUpdateCustomerMutation();
  const [deleteCustomer, { isLoading: isDeleting }] = useDeleteCustomerMutation();

  const customers = response?.data?.items || [];
  const totalRows = response?.data?.totalCount || 0;

  // Form
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CustomerFormData>({
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
      isActive: true,
    },
  });

  // Reset form when editing customer changes
  useEffect(() => {
    if (editingCustomer) {
      reset({
        code: editingCustomer.code,
        name: editingCustomer.name,
        contactName: editingCustomer.contactName || '',
        email: editingCustomer.email || '',
        phone: editingCustomer.phone || '',
        addressLine1: editingCustomer.addressLine1 || '',
        addressLine2: editingCustomer.addressLine2 || '',
        city: editingCustomer.city || '',
        region: editingCustomer.region || '',
        postalCode: editingCustomer.postalCode || '',
        countryCode: editingCustomer.countryCode || '',
        taxId: editingCustomer.taxId || '',
        paymentTerms: editingCustomer.paymentTerms || '',
        isActive: editingCustomer.isActive,
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
        isActive: true,
      });
    }
  }, [editingCustomer, reset]);

  const columnHelper = createColumns<CustomerDto>();

  const columns = useMemo(
    () => [
      columnHelper.accessor('code', {
        header: t('customers.code', 'Code'),
        size: 100,
        cell: ({ getValue }) => <span className="code">{getValue()}</span>,
      }),
      columnHelper.accessor('name', {
        header: t('customers.name', 'Name'),
        size: 180,
      }),
      columnHelper.accessor('contactName', {
        header: t('customers.contact', 'Contact'),
        size: 140,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('email', {
        header: t('customers.email', 'Email'),
        size: 180,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('phone', {
        header: t('customers.phone', 'Phone'),
        size: 120,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('city', {
        header: t('customers.city', 'City'),
        size: 120,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('orderCount', {
        header: t('customers.orders', 'Orders'),
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

  const handleRowClick = (customer: CustomerDto) => {
    setSelectedId(customer.id);
    setEditingCustomer(customer);
    setIsModalOpen(true);
  };

  const handleAddNew = () => {
    setEditingCustomer(null);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setEditingCustomer(null);
  };

  const onSubmit = async (data: CustomerFormData) => {
    try {
      if (editingCustomer) {
        // Update existing customer
        const updateData: UpdateCustomerRequest = {
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
          isActive: data.isActive,
        };
        await updateCustomer({ id: editingCustomer.id, body: updateData }).unwrap();
      } else {
        // Create new customer
        const createData: CreateCustomerRequest = {
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
          isActive: data.isActive,
        };
        await createCustomer(createData).unwrap();
      }
      handleCloseModal();
    } catch (error) {
      console.error('Failed to save customer:', error);
    }
  };

  const handleDelete = async () => {
    if (!editingCustomer) return;

    try {
      await deleteCustomer(editingCustomer.id).unwrap();
      setDeleteConfirmOpen(false);
      handleCloseModal();
    } catch (error) {
      console.error('Failed to delete customer:', error);
    }
  };

  const isSaving = isCreating || isUpdating;

  return (
    <div className="page">
      <header className="page__header">
        <div className="page__title-section">
          <h1 className="page__title">{t('customers.title', 'Customers')}</h1>
          <p className="page__subtitle">
            {t('customers.subtitle', 'Manage customer accounts')}
          </p>
        </div>
        <div className="page__actions">
          <button className="btn btn--primary" onClick={handleAddNew}>
            {t('customers.addCustomer', 'Add Customer')}
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
            <option value="">{t('customers.allStatuses', 'All')}</option>
            <option value="true">{t('common.active', 'Active')}</option>
            <option value="false">{t('common.inactive', 'Inactive')}</option>
          </select>
        </div>
      </div>

      <div className="page__content">
        <DataTable
          data={customers}
          columns={columns}
          pagination={pagination}
          onPaginationChange={setPagination}
          sorting={sorting}
          onSortingChange={setSorting}
          totalRows={totalRows}
          selectedRowId={selectedId}
          onRowClick={handleRowClick}
          getRowId={(row) => row.id}
          emptyMessage={t('customers.noCustomers', 'No customers found')}
          loading={isLoading}
        />
      </div>

      {/* Customer Form Modal */}
      <FullscreenModal
        open={isModalOpen}
        onClose={handleCloseModal}
        title={editingCustomer ? t('customers.editCustomer', 'Edit Customer') : t('customers.addCustomer', 'Add Customer')}
        subtitle={editingCustomer ? `${editingCustomer.code} - ${editingCustomer.name}` : t('customers.addCustomerSubtitle', 'Fill in customer details')}
        onSave={handleSubmit(onSubmit)}
        saveLabel={isSaving ? t('common.saving', 'Saving...') : t('common.save', 'Save')}
        saveDisabled={isSaving}
        maxWidth="lg"
      >
        <form>
          <ModalSection title={t('customers.basicInfo', 'Basic Information')}>
            <div className="form-grid">
              <div className="form-field">
                <label className="form-field__label">
                  {t('customers.code', 'Code')} <span className="required">*</span>
                </label>
                <input
                  type="text"
                  className={`form-field__input ${errors.code ? 'form-field__input--error' : ''}`}
                  placeholder="CUST001"
                  disabled={!!editingCustomer}
                  {...register('code', { required: t('validation.required', 'Required') })}
                />
                {errors.code && <span className="form-field__error">{errors.code.message}</span>}
              </div>
              <div className="form-field">
                <label className="form-field__label">
                  {t('customers.name', 'Name')} <span className="required">*</span>
                </label>
                <input
                  type="text"
                  className={`form-field__input ${errors.name ? 'form-field__input--error' : ''}`}
                  placeholder="Customer Name"
                  {...register('name', { required: t('validation.required', 'Required') })}
                />
                {errors.name && <span className="form-field__error">{errors.name.message}</span>}
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('customers.contact', 'Contact Person')}</label>
                <input
                  type="text"
                  className="form-field__input"
                  placeholder="John Doe"
                  {...register('contactName')}
                />
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('customers.email', 'Email')}</label>
                <input
                  type="email"
                  className="form-field__input"
                  placeholder="email@example.com"
                  {...register('email')}
                />
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('customers.phone', 'Phone')}</label>
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

          <ModalSection title={t('customers.address', 'Address')} collapsible defaultExpanded>
            <div className="form-grid">
              <div className="form-field form-field--full">
                <label className="form-field__label">{t('customers.addressLine1', 'Address Line 1')}</label>
                <input
                  type="text"
                  className="form-field__input"
                  placeholder="123 Main Street"
                  {...register('addressLine1')}
                />
              </div>
              <div className="form-field form-field--full">
                <label className="form-field__label">{t('customers.addressLine2', 'Address Line 2')}</label>
                <input
                  type="text"
                  className="form-field__input"
                  placeholder="Suite 100"
                  {...register('addressLine2')}
                />
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('customers.city', 'City')}</label>
                <input
                  type="text"
                  className="form-field__input"
                  placeholder="New York"
                  {...register('city')}
                />
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('customers.region', 'Region/State')}</label>
                <input
                  type="text"
                  className="form-field__input"
                  placeholder="NY"
                  {...register('region')}
                />
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('customers.postalCode', 'Postal Code')}</label>
                <input
                  type="text"
                  className="form-field__input"
                  placeholder="10001"
                  {...register('postalCode')}
                />
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('customers.country', 'Country')}</label>
                <input
                  type="text"
                  className="form-field__input"
                  placeholder="US"
                  {...register('countryCode')}
                />
              </div>
            </div>
          </ModalSection>

          <ModalSection title={t('customers.businessInfo', 'Business Information')} collapsible defaultExpanded={false}>
            <div className="form-grid">
              <div className="form-field">
                <label className="form-field__label">{t('customers.taxId', 'Tax ID')}</label>
                <input
                  type="text"
                  className="form-field__input"
                  placeholder="XX-XXXXXXX"
                  {...register('taxId')}
                />
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('customers.paymentTerms', 'Payment Terms')}</label>
                <input
                  type="text"
                  className="form-field__input"
                  placeholder="Net 30"
                  {...register('paymentTerms')}
                />
              </div>
            </div>
          </ModalSection>

          {editingCustomer && (
            <ModalSection title={t('common.dangerZone', 'Danger Zone')}>
              <div className="danger-zone">
                <p>{t('customers.deleteWarning', 'Deleting a customer will remove all associated data.')}</p>
                <button
                  type="button"
                  className="btn btn--danger"
                  onClick={() => setDeleteConfirmOpen(true)}
                >
                  {t('customers.deleteCustomer', 'Delete Customer')}
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
        title={t('customers.deleteCustomer', 'Delete Customer')}
      >
        <div className="modal__body">
          <p>
            {t(
              'customers.deleteConfirmation',
              `Are you sure you want to delete ${editingCustomer?.name}? This action cannot be undone.`
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

export default Customers;
