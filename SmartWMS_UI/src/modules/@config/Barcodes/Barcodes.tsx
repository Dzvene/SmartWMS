import { useState, useMemo, useEffect } from 'react';
import { useIntl } from 'react-intl';
import { useForm } from 'react-hook-form';
import type { ColumnDef } from '@tanstack/react-table';

import { DataTable } from '@/components/DataTable';
import { FullscreenModal, ModalSection } from '@/components/FullscreenModal';
import { Modal } from '@/components';
import {
  useGetBarcodePrefixesQuery,
  useCreateBarcodePrefixMutation,
  useUpdateBarcodePrefixMutation,
  useDeleteBarcodePrefixMutation,
} from '@/api/modules/configuration';
import type { BarcodePrefixDto, BarcodePrefixType, CreateBarcodePrefixRequest, UpdateBarcodePrefixRequest } from '@/api/modules/configuration';

const typeLabels: Record<BarcodePrefixType, string> = {
  Product: 'Product',
  Location: 'Location',
  Container: 'Container',
  Pallet: 'Pallet',
  Order: 'Order',
  Transfer: 'Transfer',
  Receipt: 'Receipt',
  Shipment: 'Shipment',
  User: 'User',
  Equipment: 'Equipment',
  Other: 'Other',
};

const PREFIX_TYPES: BarcodePrefixType[] = [
  'Product', 'Location', 'Container', 'Pallet', 'Order',
  'Transfer', 'Receipt', 'Shipment', 'User', 'Equipment', 'Other'
];

interface BarcodeFormData {
  prefix: string;
  prefixType: BarcodePrefixType;
  description: string;
  isActive: boolean;
}

/**
 * Barcodes Configuration Module
 *
 * Manages barcode prefixes and scanning rules.
 */
export function Barcodes() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });

  const [selectedBarcode, setSelectedBarcode] = useState<BarcodePrefixDto | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);
  const [paginationState, setPaginationState] = useState({ page: 1, pageSize: 25 });

  // API
  const { data, isLoading } = useGetBarcodePrefixesQuery({
    page: paginationState.page,
    pageSize: paginationState.pageSize,
  });

  const [createBarcodePrefix, { isLoading: isCreating }] = useCreateBarcodePrefixMutation();
  const [updateBarcodePrefix, { isLoading: isUpdating }] = useUpdateBarcodePrefixMutation();
  const [deleteBarcodePrefix, { isLoading: isDeleting }] = useDeleteBarcodePrefixMutation();

  const barcodes = data?.data?.items ?? [];
  const totalRows = data?.data?.totalCount ?? 0;

  // Form
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<BarcodeFormData>({
    defaultValues: {
      prefix: '',
      prefixType: 'Product',
      description: '',
      isActive: true,
    },
  });

  // Reset form when editing changes
  useEffect(() => {
    if (selectedBarcode) {
      reset({
        prefix: selectedBarcode.prefix,
        prefixType: selectedBarcode.prefixType,
        description: selectedBarcode.description || '',
        isActive: selectedBarcode.isActive,
      });
    } else {
      reset({
        prefix: '',
        prefixType: 'Product',
        description: '',
        isActive: true,
      });
    }
  }, [selectedBarcode, reset]);

  const columns = useMemo<ColumnDef<BarcodePrefixDto, unknown>[]>(
    () => [
      {
        accessorKey: 'prefix',
        header: t('barcodes.prefix', 'Prefix'),
        size: 100,
        cell: ({ getValue }) => (
          <span className="code">{getValue() as string}</span>
        ),
      },
      {
        accessorKey: 'prefixType',
        header: t('common.type', 'Type'),
        size: 120,
        cell: ({ getValue }) => typeLabels[getValue() as BarcodePrefixType],
      },
      {
        accessorKey: 'description',
        header: t('barcodes.description', 'Description'),
        size: 200,
        cell: ({ getValue }) => getValue() || '—',
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
        header: t('common.created', 'Created'),
        size: 120,
        cell: ({ getValue }) => new Date(getValue() as string).toLocaleDateString(),
      },
    ],
    [t]
  );

  const handleRowClick = (barcode: BarcodePrefixDto) => {
    setSelectedBarcode(barcode);
    setIsModalOpen(true);
  };

  const handleAddNew = () => {
    setSelectedBarcode(null);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setSelectedBarcode(null);
  };

  const onSubmit = async (data: BarcodeFormData) => {
    try {
      if (selectedBarcode) {
        const updateData: UpdateBarcodePrefixRequest = {
          prefix: data.prefix,
          description: data.description || undefined,
          isActive: data.isActive,
        };
        await updateBarcodePrefix({ id: selectedBarcode.id, data: updateData }).unwrap();
      } else {
        const createData: CreateBarcodePrefixRequest = {
          prefix: data.prefix,
          prefixType: data.prefixType,
          description: data.description || undefined,
          isActive: data.isActive,
        };
        await createBarcodePrefix(createData).unwrap();
      }
      handleCloseModal();
    } catch (error) {
      console.error('Failed to save barcode prefix:', error);
    }
  };

  const handleDelete = async () => {
    if (!selectedBarcode) return;

    try {
      await deleteBarcodePrefix(selectedBarcode.id).unwrap();
      setDeleteConfirmOpen(false);
      handleCloseModal();
    } catch (error) {
      console.error('Failed to delete barcode prefix:', error);
    }
  };

  const isSaving = isCreating || isUpdating;

  return (
    <div className="page">
      <div className="page__header">
        <h1 className="page__title">{t('barcodes.title', 'Barcode Prefixes')}</h1>
        <div className="page__actions">
          <button className="btn btn--primary" onClick={handleAddNew}>
            {t('barcodes.addBarcode', 'Add Prefix')}
          </button>
        </div>
      </div>

      <div className="page__content">
        <DataTable
          data={barcodes}
          columns={columns}
          pagination={{
            pageIndex: paginationState.page - 1,
            pageSize: paginationState.pageSize,
          }}
          onPaginationChange={({ pageIndex, pageSize }) => {
            setPaginationState({ page: pageIndex + 1, pageSize });
          }}
          totalRows={totalRows}
          onRowClick={handleRowClick}
          selectedRowId={selectedBarcode?.id}
          getRowId={(row) => row.id}
          loading={isLoading}
          emptyMessage={t('common.noData', 'No data found')}
        />
      </div>

      {/* Barcode Form Modal */}
      <FullscreenModal
        open={isModalOpen}
        onClose={handleCloseModal}
        title={selectedBarcode ? t('barcodes.editBarcode', 'Edit Barcode Prefix') : t('barcodes.addBarcode', 'Add Barcode Prefix')}
        subtitle={selectedBarcode ? `${selectedBarcode.prefix} - ${typeLabels[selectedBarcode.prefixType]}` : undefined}
        onSave={handleSubmit(onSubmit)}
        saveLabel={isSaving ? t('common.saving', 'Saving...') : t('common.save', 'Save')}
        saveDisabled={isSaving}
      >
        <form>
          <ModalSection title={t('barcodes.details', 'Details')}>
            <div className="form-grid">
              <div className="form-field">
                <label className="form-field__label">
                  {t('barcodes.prefix', 'Prefix')} <span className="required">*</span>
                </label>
                <input
                  type="text"
                  className={`form-field__input ${errors.prefix ? 'form-field__input--error' : ''}`}
                  placeholder="PRD"
                  {...register('prefix', { required: t('validation.required', 'Required') })}
                />
                {errors.prefix && <span className="form-field__error">{errors.prefix.message}</span>}
                <p className="form-field__hint">
                  {t('barcodes.prefixHint', 'Short prefix used to identify barcode type (e.g., PRD for products)')}
                </p>
              </div>
              <div className="form-field">
                <label className="form-field__label">
                  {t('common.type', 'Type')} <span className="required">*</span>
                </label>
                <select
                  className="form-field__select"
                  disabled={!!selectedBarcode}
                  {...register('prefixType', { required: true })}
                >
                  {PREFIX_TYPES.map((type) => (
                    <option key={type} value={type}>
                      {typeLabels[type]}
                    </option>
                  ))}
                </select>
              </div>
              <div className="form-field form-field--full">
                <label className="form-field__label">{t('barcodes.description', 'Description')}</label>
                <textarea
                  className="form-field__textarea"
                  rows={2}
                  placeholder={t('barcodes.descriptionPlaceholder', 'Optional description...')}
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

          {selectedBarcode && (
            <ModalSection title={t('common.dangerZone', 'Danger Zone')}>
              <div className="danger-zone">
                <p>{t('barcodes.deleteWarning', 'Deleting a barcode prefix may affect scanning operations.')}</p>
                <button
                  type="button"
                  className="btn btn--danger"
                  onClick={() => setDeleteConfirmOpen(true)}
                >
                  {t('barcodes.deletePrefix', 'Delete Prefix')}
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
        title={t('barcodes.deletePrefix', 'Delete Barcode Prefix')}
      >
        <div className="modal__body">
          <p>
            {t(
              'barcodes.deleteConfirmation',
              `Are you sure you want to delete prefix "${selectedBarcode?.prefix}"? This action cannot be undone.`
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

export default Barcodes;
