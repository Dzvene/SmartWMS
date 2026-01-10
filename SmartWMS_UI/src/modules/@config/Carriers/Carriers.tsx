import { useState, useMemo, useEffect } from 'react';
import { useIntl } from 'react-intl';
import { useForm } from 'react-hook-form';
import type { ColumnDef } from '@tanstack/react-table';

import { DataTable } from '@/components/DataTable';
import { FullscreenModal, ModalSection } from '@/components/FullscreenModal';
import { Modal } from '@/components';
import {
  useGetCarriersQuery,
  useCreateCarrierMutation,
  useUpdateCarrierMutation,
  useDeleteCarrierMutation,
} from '@/api/modules/carriers';
import type {
  CarrierListDto,
  CarrierIntegrationType,
  CreateCarrierRequest,
  UpdateCarrierRequest,
} from '@/api/modules/carriers';

const INTEGRATION_LABELS: Record<CarrierIntegrationType, string> = {
  Manual: 'Manual',
  API: 'API',
  EDI: 'EDI',
  File: 'File Import',
};

const INTEGRATION_TYPES: CarrierIntegrationType[] = ['Manual', 'API', 'EDI', 'File'];

interface CarrierFormData {
  code: string;
  name: string;
  description: string;
  contactName: string;
  phone: string;
  email: string;
  website: string;
  accountNumber: string;
  integrationType: CarrierIntegrationType;
  defaultServiceCode: string;
  notes: string;
  isActive: boolean;
}

/**
 * Carriers Configuration Module
 *
 * Manages shipping carriers and their services.
 */
export function Carriers() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });

  const [filters, setFilters] = useState({
    page: 1,
    pageSize: 20,
    search: '',
    isActive: undefined as boolean | undefined,
  });

  const { data: response, isLoading } = useGetCarriersQuery(filters);
  const carriers = response?.data?.items || [];
  const totalCount = response?.data?.totalCount || 0;

  const [selectedCarrier, setSelectedCarrier] = useState<CarrierListDto | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);

  // API mutations
  const [createCarrier, { isLoading: isCreating }] = useCreateCarrierMutation();
  const [updateCarrier, { isLoading: isUpdating }] = useUpdateCarrierMutation();
  const [deleteCarrier, { isLoading: isDeleting }] = useDeleteCarrierMutation();

  // Form
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CarrierFormData>({
    defaultValues: {
      code: '',
      name: '',
      description: '',
      contactName: '',
      phone: '',
      email: '',
      website: '',
      accountNumber: '',
      integrationType: 'Manual',
      defaultServiceCode: '',
      notes: '',
      isActive: true,
    },
  });

  // Reset form when editing changes
  useEffect(() => {
    if (selectedCarrier) {
      reset({
        code: selectedCarrier.code,
        name: selectedCarrier.name,
        description: '',
        contactName: '',
        phone: '',
        email: '',
        website: '',
        accountNumber: '',
        integrationType: selectedCarrier.integrationType,
        defaultServiceCode: '',
        notes: '',
        isActive: selectedCarrier.isActive,
      });
    } else {
      reset({
        code: '',
        name: '',
        description: '',
        contactName: '',
        phone: '',
        email: '',
        website: '',
        accountNumber: '',
        integrationType: 'Manual',
        defaultServiceCode: '',
        notes: '',
        isActive: true,
      });
    }
  }, [selectedCarrier, reset]);

  const columns = useMemo<ColumnDef<CarrierListDto, unknown>[]>(
    () => [
      {
        accessorKey: 'code',
        header: t('carriers.carrierCode', 'Code'),
        size: 100,
        cell: ({ getValue }) => (
          <span className="code">{getValue() as string}</span>
        ),
      },
      {
        accessorKey: 'name',
        header: t('carriers.carrierName', 'Name'),
        size: 180,
      },
      {
        accessorKey: 'integrationType',
        header: t('carriers.integrationType', 'Integration'),
        size: 120,
        cell: ({ getValue }) => {
          const type = getValue() as CarrierIntegrationType;
          return INTEGRATION_LABELS[type] || type;
        },
      },
      {
        accessorKey: 'serviceCount',
        header: t('carriers.services', 'Services'),
        size: 100,
        cell: ({ getValue }) => `${getValue()} services`,
      },
      {
        accessorKey: 'isActive',
        header: t('common.status', 'Status'),
        size: 80,
        cell: ({ getValue }) => (
          <span className={`status-badge status-badge--${getValue() ? 'active' : 'inactive'}`}>
            {getValue() ? t('status.active', 'Active') : t('status.inactive', 'Inactive')}
          </span>
        ),
      },
      {
        accessorKey: 'createdAt',
        header: t('common.createdAt', 'Created'),
        size: 110,
        cell: ({ getValue }) => new Date(getValue() as string).toLocaleDateString(),
      },
    ],
    [t]
  );

  const handleRowClick = (carrier: CarrierListDto) => {
    setSelectedCarrier(carrier);
    setIsModalOpen(true);
  };

  const handleAddNew = () => {
    setSelectedCarrier(null);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setSelectedCarrier(null);
  };

  const handlePageChange = (page: number) => {
    setFilters((prev) => ({ ...prev, page }));
  };

  const handleSearch = (value: string) => {
    setFilters((prev) => ({ ...prev, search: value, page: 1 }));
  };

  const onSubmit = async (data: CarrierFormData) => {
    try {
      if (selectedCarrier) {
        const updateData: UpdateCarrierRequest = {
          name: data.name,
          description: data.description || undefined,
          contactName: data.contactName || undefined,
          phone: data.phone || undefined,
          email: data.email || undefined,
          website: data.website || undefined,
          accountNumber: data.accountNumber || undefined,
          integrationType: data.integrationType,
          defaultServiceCode: data.defaultServiceCode || undefined,
          notes: data.notes || undefined,
          isActive: data.isActive,
        };
        await updateCarrier({ id: selectedCarrier.id, body: updateData }).unwrap();
      } else {
        const createData: CreateCarrierRequest = {
          code: data.code,
          name: data.name,
          description: data.description || undefined,
          contactName: data.contactName || undefined,
          phone: data.phone || undefined,
          email: data.email || undefined,
          website: data.website || undefined,
          accountNumber: data.accountNumber || undefined,
          integrationType: data.integrationType,
          defaultServiceCode: data.defaultServiceCode || undefined,
          notes: data.notes || undefined,
        };
        await createCarrier(createData).unwrap();
      }
      handleCloseModal();
    } catch (error) {
      console.error('Failed to save carrier:', error);
    }
  };

  const handleDelete = async () => {
    if (!selectedCarrier) return;

    try {
      await deleteCarrier(selectedCarrier.id).unwrap();
      setDeleteConfirmOpen(false);
      handleCloseModal();
    } catch (error) {
      console.error('Failed to delete carrier:', error);
    }
  };

  const isSaving = isCreating || isUpdating;

  return (
    <div className="page">
      <header className="page__header">
        <div className="page__title-section">
          <h1 className="page__title">{t('carriers.title', 'Carriers')}</h1>
          <p className="page__subtitle">{t('carriers.subtitle', 'Manage shipping carriers and services')}</p>
        </div>
        <div className="page__actions">
          <button className="btn btn--primary" onClick={handleAddNew}>
            {t('carriers.addCarrier', 'Add Carrier')}
          </button>
        </div>
      </header>

      <div className="page-toolbar">
        <div className="page-search">
          <input
            type="text"
            className="page-search__input"
            placeholder={t('common.search', 'Search...')}
            value={filters.search}
            onChange={(e) => handleSearch(e.target.value)}
          />
        </div>
        <div className="page-filters">
          <select
            className="page-filter__select"
            value={filters.isActive === undefined ? '' : filters.isActive.toString()}
            onChange={(e) =>
              setFilters((prev) => ({
                ...prev,
                isActive: e.target.value === '' ? undefined : e.target.value === 'true',
                page: 1,
              }))
            }
          >
            <option value="">{t('carriers.allStatuses', 'All')}</option>
            <option value="true">{t('status.active', 'Active')}</option>
            <option value="false">{t('status.inactive', 'Inactive')}</option>
          </select>
        </div>
      </div>

      <div className="page__content">
        <DataTable
          data={carriers}
          columns={columns}
          pagination={{
            pageIndex: filters.page - 1,
            pageSize: filters.pageSize,
          }}
          onPaginationChange={({ pageIndex }) => handlePageChange(pageIndex + 1)}
          totalRows={totalCount}
          onRowClick={handleRowClick}
          selectedRowId={selectedCarrier?.id}
          getRowId={(row) => row.id}
          emptyMessage={t('carriers.noCarriers', 'No carriers found')}
          loading={isLoading}
        />
      </div>

      {/* Carrier Form Modal */}
      <FullscreenModal
        open={isModalOpen}
        onClose={handleCloseModal}
        title={selectedCarrier ? t('carriers.editCarrier', 'Edit Carrier') : t('carriers.addCarrier', 'Add Carrier')}
        subtitle={selectedCarrier ? `${selectedCarrier.code} - ${selectedCarrier.name}` : undefined}
        onSave={handleSubmit(onSubmit)}
        saveLabel={isSaving ? t('common.saving', 'Saving...') : t('common.save', 'Save')}
        saveDisabled={isSaving}
        maxWidth="lg"
      >
        <form>
          <ModalSection title={t('carriers.basicInfo', 'Basic Information')}>
            <div className="form-grid">
              <div className="form-field">
                <label className="form-field__label">
                  {t('carriers.carrierCode', 'Code')} <span className="required">*</span>
                </label>
                <input
                  type="text"
                  className={`form-field__input ${errors.code ? 'form-field__input--error' : ''}`}
                  placeholder="UPS"
                  disabled={!!selectedCarrier}
                  {...register('code', { required: t('validation.required', 'Required') })}
                />
                {errors.code && <span className="form-field__error">{errors.code.message}</span>}
              </div>
              <div className="form-field">
                <label className="form-field__label">
                  {t('carriers.carrierName', 'Name')} <span className="required">*</span>
                </label>
                <input
                  type="text"
                  className={`form-field__input ${errors.name ? 'form-field__input--error' : ''}`}
                  placeholder="United Parcel Service"
                  {...register('name', { required: t('validation.required', 'Required') })}
                />
                {errors.name && <span className="form-field__error">{errors.name.message}</span>}
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('carriers.integrationType', 'Integration Type')}</label>
                <select className="form-field__select" {...register('integrationType')}>
                  {INTEGRATION_TYPES.map((type) => (
                    <option key={type} value={type}>
                      {INTEGRATION_LABELS[type]}
                    </option>
                  ))}
                </select>
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('carriers.accountNumber', 'Account Number')}</label>
                <input
                  type="text"
                  className="form-field__input"
                  placeholder="123456789"
                  {...register('accountNumber')}
                />
              </div>
              <div className="form-field form-field--full">
                <label className="form-field__label">{t('carriers.description', 'Description')}</label>
                <textarea
                  className="form-field__textarea"
                  rows={2}
                  placeholder={t('carriers.descriptionPlaceholder', 'Optional description...')}
                  {...register('description')}
                />
              </div>
              <div className="form-field">
                <label className="form-checkbox">
                  <input type="checkbox" {...register('isActive')} />
                  <span>{t('common.active', 'Active')}</span>
                </label>
              </div>
            </div>
          </ModalSection>

          <ModalSection title={t('carriers.contactInfo', 'Contact Information')} collapsible defaultExpanded>
            <div className="form-grid">
              <div className="form-field">
                <label className="form-field__label">{t('carriers.contactName', 'Contact Name')}</label>
                <input
                  type="text"
                  className="form-field__input"
                  placeholder="John Doe"
                  {...register('contactName')}
                />
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('carriers.phone', 'Phone')}</label>
                <input
                  type="tel"
                  className="form-field__input"
                  placeholder="+1 800 742 5877"
                  {...register('phone')}
                />
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('carriers.email', 'Email')}</label>
                <input
                  type="email"
                  className="form-field__input"
                  placeholder="support@carrier.com"
                  {...register('email')}
                />
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('carriers.website', 'Website')}</label>
                <input
                  type="url"
                  className="form-field__input"
                  placeholder="https://www.ups.com"
                  {...register('website')}
                />
              </div>
            </div>
          </ModalSection>

          <ModalSection title={t('carriers.settings', 'Settings')} collapsible defaultExpanded={false}>
            <div className="form-grid">
              <div className="form-field">
                <label className="form-field__label">{t('carriers.defaultService', 'Default Service Code')}</label>
                <input
                  type="text"
                  className="form-field__input"
                  placeholder="GROUND"
                  {...register('defaultServiceCode')}
                />
              </div>
              <div className="form-field form-field--full">
                <label className="form-field__label">{t('carriers.notes', 'Notes')}</label>
                <textarea
                  className="form-field__textarea"
                  rows={3}
                  placeholder={t('carriers.notesPlaceholder', 'Internal notes...')}
                  {...register('notes')}
                />
              </div>
            </div>
          </ModalSection>

          {selectedCarrier && (
            <ModalSection title={t('common.dangerZone', 'Danger Zone')}>
              <div className="danger-zone">
                <p>{t('carriers.deleteWarning', 'Deleting a carrier will remove all associated services.')}</p>
                <button
                  type="button"
                  className="btn btn--danger"
                  onClick={() => setDeleteConfirmOpen(true)}
                >
                  {t('carriers.deleteCarrier', 'Delete Carrier')}
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
        title={t('carriers.deleteCarrier', 'Delete Carrier')}
      >
        <div className="modal__body">
          <p>
            {t(
              'carriers.deleteConfirmation',
              `Are you sure you want to delete "${selectedCarrier?.name}"? This will also remove all associated services.`
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

export default Carriers;
