import { useState, useMemo, useEffect } from 'react';
import { useIntl } from 'react-intl';
import { useForm } from 'react-hook-form';
import type { ColumnDef } from '@tanstack/react-table';

import { DataTable } from '@/components/DataTable';
import { FullscreenModal, ModalSection } from '@/components/FullscreenModal';
import { Modal } from '@/components';
import {
  useGetReasonCodesQuery,
  useCreateReasonCodeMutation,
  useUpdateReasonCodeMutation,
  useDeleteReasonCodeMutation,
} from '@/api/modules/configuration';
import type { ReasonCodeDto, ReasonCodeType, CreateReasonCodeRequest, UpdateReasonCodeRequest } from '@/api/modules/configuration';

const categoryLabels: Record<ReasonCodeType, string> = {
  Adjustment: 'Adjustment',
  Return: 'Return',
  Damage: 'Damage',
  Expiry: 'Expiry',
  QualityHold: 'Quality Hold',
  Transfer: 'Transfer',
  Scrap: 'Scrap',
  Found: 'Found',
  Lost: 'Lost',
  Other: 'Other',
};

const REASON_TYPES: ReasonCodeType[] = [
  'Adjustment', 'Return', 'Damage', 'Expiry', 'QualityHold',
  'Transfer', 'Scrap', 'Found', 'Lost', 'Other'
];

interface ReasonCodeFormData {
  code: string;
  name: string;
  description: string;
  reasonType: ReasonCodeType;
  requiresNotes: boolean;
  sortOrder: number;
  isActive: boolean;
}

/**
 * Reason Codes Configuration Module
 *
 * Manages reason codes for inventory adjustments and returns.
 */
export function ReasonCodes() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });

  const [selectedCode, setSelectedCode] = useState<ReasonCodeDto | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);
  const [paginationState, setPaginationState] = useState({ page: 1, pageSize: 25 });

  // API
  const { data, isLoading } = useGetReasonCodesQuery({
    page: paginationState.page,
    pageSize: paginationState.pageSize,
  });

  const [createReasonCode, { isLoading: isCreating }] = useCreateReasonCodeMutation();
  const [updateReasonCode, { isLoading: isUpdating }] = useUpdateReasonCodeMutation();
  const [deleteReasonCode, { isLoading: isDeleting }] = useDeleteReasonCodeMutation();

  const reasonCodes = data?.data?.items ?? [];
  const totalRows = data?.data?.totalCount ?? 0;

  // Form
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<ReasonCodeFormData>({
    defaultValues: {
      code: '',
      name: '',
      description: '',
      reasonType: 'Adjustment',
      requiresNotes: false,
      sortOrder: 0,
      isActive: true,
    },
  });

  // Reset form when editing changes
  useEffect(() => {
    if (selectedCode) {
      reset({
        code: selectedCode.code,
        name: selectedCode.name,
        description: selectedCode.description || '',
        reasonType: selectedCode.reasonType,
        requiresNotes: selectedCode.requiresNotes,
        sortOrder: selectedCode.sortOrder,
        isActive: selectedCode.isActive,
      });
    } else {
      reset({
        code: '',
        name: '',
        description: '',
        reasonType: 'Adjustment',
        requiresNotes: false,
        sortOrder: 0,
        isActive: true,
      });
    }
  }, [selectedCode, reset]);

  const columns = useMemo<ColumnDef<ReasonCodeDto, unknown>[]>(
    () => [
      {
        accessorKey: 'code',
        header: t('reasonCodes.code', 'Code'),
        size: 100,
        cell: ({ getValue }) => (
          <span className="code">{getValue() as string}</span>
        ),
      },
      {
        accessorKey: 'name',
        header: t('common.name', 'Name'),
        size: 160,
      },
      {
        accessorKey: 'description',
        header: t('reasonCodes.description', 'Description'),
        size: 200,
        cell: ({ getValue }) => getValue() || '—',
      },
      {
        accessorKey: 'reasonType',
        header: t('reasonCodes.category', 'Category'),
        size: 120,
        cell: ({ getValue }) => {
          const type = getValue() as ReasonCodeType;
          return (
            <span className={`tag tag--${type.toLowerCase()}`}>
              {categoryLabels[type]}
            </span>
          );
        },
      },
      {
        accessorKey: 'requiresNotes',
        header: t('reasonCodes.requiresNote', 'Requires Note'),
        size: 110,
        cell: ({ getValue }) => (getValue() ? 'Yes' : 'No'),
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
    ],
    [t]
  );

  const handleRowClick = (code: ReasonCodeDto) => {
    setSelectedCode(code);
    setIsModalOpen(true);
  };

  const handleAddNew = () => {
    setSelectedCode(null);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setSelectedCode(null);
  };

  const onSubmit = async (data: ReasonCodeFormData) => {
    try {
      if (selectedCode) {
        const updateData: UpdateReasonCodeRequest = {
          code: data.code,
          name: data.name,
          description: data.description || undefined,
          requiresNotes: data.requiresNotes,
          sortOrder: data.sortOrder,
          isActive: data.isActive,
        };
        await updateReasonCode({ id: selectedCode.id, data: updateData }).unwrap();
      } else {
        const createData: CreateReasonCodeRequest = {
          code: data.code,
          name: data.name,
          description: data.description || undefined,
          reasonType: data.reasonType,
          requiresNotes: data.requiresNotes,
          sortOrder: data.sortOrder,
          isActive: data.isActive,
        };
        await createReasonCode(createData).unwrap();
      }
      handleCloseModal();
    } catch (error) {
      console.error('Failed to save reason code:', error);
    }
  };

  const handleDelete = async () => {
    if (!selectedCode) return;

    try {
      await deleteReasonCode(selectedCode.id).unwrap();
      setDeleteConfirmOpen(false);
      handleCloseModal();
    } catch (error) {
      console.error('Failed to delete reason code:', error);
    }
  };

  const isSaving = isCreating || isUpdating;

  return (
    <div className="page">
      <div className="page__header">
        <h1 className="page__title">{t('reasonCodes.title', 'Reason Codes')}</h1>
        <div className="page__actions">
          <button className="btn btn--primary" onClick={handleAddNew}>
            {t('reasonCodes.addCode', 'Add Reason Code')}
          </button>
        </div>
      </div>

      <div className="page__content">
        <DataTable
          data={reasonCodes}
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
          selectedRowId={selectedCode?.id}
          getRowId={(row) => row.id}
          loading={isLoading}
          emptyMessage={t('common.noData', 'No data found')}
        />
      </div>

      {/* Reason Code Form Modal */}
      <FullscreenModal
        open={isModalOpen}
        onClose={handleCloseModal}
        title={selectedCode ? t('reasonCodes.editCode', 'Edit Reason Code') : t('reasonCodes.addCode', 'Add Reason Code')}
        subtitle={selectedCode ? `${selectedCode.code} - ${selectedCode.name}` : undefined}
        onSave={handleSubmit(onSubmit)}
        saveLabel={isSaving ? t('common.saving', 'Saving...') : t('common.save', 'Save')}
        saveDisabled={isSaving}
      >
        <form>
          <ModalSection title={t('reasonCodes.details', 'Details')}>
            <div className="form-grid">
              <div className="form-field">
                <label className="form-field__label">
                  {t('reasonCodes.code', 'Code')} <span className="required">*</span>
                </label>
                <input
                  type="text"
                  className={`form-field__input ${errors.code ? 'form-field__input--error' : ''}`}
                  placeholder="ADJ-01"
                  {...register('code', { required: t('validation.required', 'Required') })}
                />
                {errors.code && <span className="form-field__error">{errors.code.message}</span>}
              </div>
              <div className="form-field">
                <label className="form-field__label">
                  {t('common.name', 'Name')} <span className="required">*</span>
                </label>
                <input
                  type="text"
                  className={`form-field__input ${errors.name ? 'form-field__input--error' : ''}`}
                  placeholder="Inventory Adjustment"
                  {...register('name', { required: t('validation.required', 'Required') })}
                />
                {errors.name && <span className="form-field__error">{errors.name.message}</span>}
              </div>
              <div className="form-field">
                <label className="form-field__label">
                  {t('reasonCodes.category', 'Category')} <span className="required">*</span>
                </label>
                <select
                  className="form-field__select"
                  disabled={!!selectedCode}
                  {...register('reasonType', { required: true })}
                >
                  {REASON_TYPES.map((type) => (
                    <option key={type} value={type}>
                      {categoryLabels[type]}
                    </option>
                  ))}
                </select>
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('reasonCodes.sortOrder', 'Sort Order')}</label>
                <input
                  type="number"
                  min="0"
                  className="form-field__input"
                  {...register('sortOrder', { valueAsNumber: true })}
                />
              </div>
              <div className="form-field form-field--full">
                <label className="form-field__label">{t('reasonCodes.description', 'Description')}</label>
                <textarea
                  className="form-field__textarea"
                  rows={2}
                  placeholder={t('reasonCodes.descriptionPlaceholder', 'Optional description...')}
                  {...register('description')}
                />
              </div>
            </div>
          </ModalSection>

          <ModalSection title={t('reasonCodes.settings', 'Settings')}>
            <div className="form-grid">
              <div className="form-field">
                <label className="form-checkbox">
                  <input type="checkbox" {...register('requiresNotes')} />
                  <span>{t('reasonCodes.requiresNotes', 'Requires Notes')}</span>
                </label>
                <p className="form-field__hint">
                  {t('reasonCodes.requiresNotesHint', 'When enabled, users must provide a note when using this reason code')}
                </p>
              </div>
              <div className="form-field">
                <label className="form-checkbox">
                  <input type="checkbox" {...register('isActive')} />
                  <span>{t('common.active', 'Active')}</span>
                </label>
              </div>
            </div>
          </ModalSection>

          {selectedCode && (
            <ModalSection title={t('common.dangerZone', 'Danger Zone')}>
              <div className="danger-zone">
                <p>{t('reasonCodes.deleteWarning', 'Deleting a reason code may affect historical records.')}</p>
                <button
                  type="button"
                  className="btn btn--danger"
                  onClick={() => setDeleteConfirmOpen(true)}
                >
                  {t('reasonCodes.deleteCode', 'Delete Reason Code')}
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
        title={t('reasonCodes.deleteCode', 'Delete Reason Code')}
      >
        <div className="modal__body">
          <p>
            {t(
              'reasonCodes.deleteConfirmation',
              `Are you sure you want to delete "${selectedCode?.name}"? This action cannot be undone.`
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

export default ReasonCodes;
