import { useState, useMemo, useEffect } from 'react';
import { useIntl } from 'react-intl';
import { useForm } from 'react-hook-form';
import type { ColumnDef } from '@tanstack/react-table';

import { DataTable } from '@/components/DataTable';
import { FullscreenModal, ModalSection } from '@/components/FullscreenModal';
import { Modal } from '@/components';
import {
  useGetAdjustmentsQuery,
  useCreateAdjustmentMutation,
  useCancelAdjustmentMutation,
} from '@/api/modules/adjustments';
import type {
  StockAdjustmentSummaryDto,
  AdjustmentStatus,
  AdjustmentType,
  CreateStockAdjustmentRequest,
} from '@/api/modules/adjustments';
import { useGetWarehouseOptionsQuery } from '@/api/modules/warehouses';

const statusLabels: Record<AdjustmentStatus, string> = {
  Draft: 'Draft',
  PendingApproval: 'Pending',
  Approved: 'Approved',
  Posted: 'Posted',
  Cancelled: 'Cancelled',
};

const statusClasses: Record<AdjustmentStatus, string> = {
  Draft: 'draft',
  PendingApproval: 'pending',
  Approved: 'approved',
  Posted: 'completed',
  Cancelled: 'cancelled',
};

const ADJUSTMENT_TYPES: { value: AdjustmentType; label: string }[] = [
  { value: 'Correction', label: 'Correction' },
  { value: 'CycleCount', label: 'Cycle Count' },
  { value: 'Damage', label: 'Damage' },
  { value: 'Scrap', label: 'Scrap' },
  { value: 'Found', label: 'Found' },
  { value: 'Lost', label: 'Lost' },
  { value: 'Expiry', label: 'Expiry' },
  { value: 'QualityHold', label: 'Quality Hold' },
  { value: 'Revaluation', label: 'Revaluation' },
  { value: 'Opening', label: 'Opening Balance' },
  { value: 'Other', label: 'Other' },
];

interface AdjustmentFormData {
  warehouseId: string;
  adjustmentType: AdjustmentType;
  reasonNotes: string;
  notes: string;
}

export function Adjustments() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });

  const [selectedAdjustment, setSelectedAdjustment] = useState<StockAdjustmentSummaryDto | null>(null);
  const [paginationState, setPaginationState] = useState({ page: 1, pageSize: 25 });
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [cancelConfirmOpen, setCancelConfirmOpen] = useState(false);

  // API Queries
  const { data, isLoading } = useGetAdjustmentsQuery({
    page: paginationState.page,
    pageSize: paginationState.pageSize,
  });
  const { data: warehousesData } = useGetWarehouseOptionsQuery();

  const adjustments = data?.data?.items ?? [];
  const totalRows = data?.data?.totalCount ?? 0;
  const warehouses = warehousesData?.data || [];

  // API Mutations
  const [createAdjustment, { isLoading: isCreating }] = useCreateAdjustmentMutation();
  const [cancelAdjustment, { isLoading: isCancelling }] = useCancelAdjustmentMutation();

  // Form
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<AdjustmentFormData>({
    defaultValues: {
      warehouseId: '',
      adjustmentType: 'Correction',
      reasonNotes: '',
      notes: '',
    },
  });

  // Reset form when modal closes
  useEffect(() => {
    if (!isModalOpen) {
      reset({
        warehouseId: '',
        adjustmentType: 'Correction',
        reasonNotes: '',
        notes: '',
      });
    }
  }, [isModalOpen, reset]);

  const columns = useMemo<ColumnDef<StockAdjustmentSummaryDto, unknown>[]>(() => [
    { accessorKey: 'adjustmentNumber', header: t('adjustments.adjustmentNumber', 'Adj #'), size: 100 },
    { accessorKey: 'adjustmentType', header: t('common.type', 'Type'), size: 100 },
    { accessorKey: 'warehouseName', header: t('common.warehouse', 'Warehouse'), size: 120, cell: ({ getValue }) => getValue() || '—' },
    { accessorKey: 'reasonCodeName', header: t('adjustments.reason', 'Reason'), size: 120, cell: ({ getValue }) => getValue() || '—' },
    { accessorKey: 'totalLines', header: t('adjustments.lines', 'Lines'), size: 80, meta: { align: 'right' } },
    {
      accessorKey: 'totalQuantityChange',
      header: t('adjustments.qtyChange', 'Qty Change'),
      size: 100,
      meta: { align: 'right' },
      cell: ({ getValue }) => {
        const v = getValue() as number;
        return <span className={v > 0 ? 'text-success' : v < 0 ? 'text-error' : ''}>{v > 0 ? `+${v}` : v}</span>;
      },
    },
    { accessorKey: 'createdByUserName', header: t('adjustments.createdBy', 'Created By'), size: 120, cell: ({ getValue }) => getValue() || '—' },
    {
      accessorKey: 'createdAt',
      header: t('common.date', 'Date'),
      size: 100,
      cell: ({ getValue }) => new Date(getValue() as string).toLocaleDateString(),
    },
    {
      accessorKey: 'status',
      header: t('common.status', 'Status'),
      size: 100,
      cell: ({ getValue }) => {
        const status = getValue() as AdjustmentStatus;
        return <span className={`status-badge status-badge--${statusClasses[status]}`}>{statusLabels[status]}</span>;
      },
    },
  ], [t]);

  const handleAddNew = () => {
    setSelectedAdjustment(null);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setSelectedAdjustment(null);
  };

  const onSubmit = async (data: AdjustmentFormData) => {
    try {
      const createData: CreateStockAdjustmentRequest = {
        warehouseId: data.warehouseId,
        adjustmentType: data.adjustmentType,
        reasonNotes: data.reasonNotes || undefined,
        notes: data.notes || undefined,
      };
      await createAdjustment(createData).unwrap();
      handleCloseModal();
    } catch (error) {
      console.error('Failed to create adjustment:', error);
    }
  };

  const handleCancel = async () => {
    if (!selectedAdjustment) return;

    try {
      await cancelAdjustment(selectedAdjustment.id).unwrap();
      setCancelConfirmOpen(false);
      setSelectedAdjustment(null);
    } catch (error) {
      console.error('Failed to cancel adjustment:', error);
    }
  };

  const isSaving = isCreating;

  return (
    <div className="page">
      <div className="page__header">
        <h1 className="page__title">{t('adjustments.title', 'Adjustments')}</h1>
        <div className="page__actions">
          <button className="btn btn--primary" onClick={handleAddNew}>
            {t('adjustments.newAdjustment', 'New Adjustment')}
          </button>
        </div>
      </div>
      <div className="page__content">
        <DataTable
          data={adjustments}
          columns={columns}
          pagination={{ pageIndex: paginationState.page - 1, pageSize: paginationState.pageSize }}
          onPaginationChange={({ pageIndex, pageSize }) => {
            setPaginationState({ page: pageIndex + 1, pageSize });
          }}
          totalRows={totalRows}
          onRowClick={setSelectedAdjustment}
          selectedRowId={selectedAdjustment?.id}
          getRowId={(row) => row.id}
          loading={isLoading}
          emptyMessage={t('common.noData', 'No data found')}
        />
      </div>

      {/* Create Adjustment Modal */}
      <FullscreenModal
        open={isModalOpen}
        onClose={handleCloseModal}
        title={t('adjustments.newAdjustment', 'New Adjustment')}
        onSave={handleSubmit(onSubmit)}
        saveLabel={isSaving ? t('common.saving', 'Saving...') : t('common.save', 'Save')}
        saveDisabled={isSaving}
        maxWidth="lg"
      >
        <form>
          <ModalSection title={t('adjustments.adjustmentDetails', 'Adjustment Details')}>
            <div className="form-grid">
              <div className="form-field">
                <label className="form-field__label">
                  {t('common.warehouse', 'Warehouse')} <span className="required">*</span>
                </label>
                <select
                  className={`form-field__select ${errors.warehouseId ? 'form-field__select--error' : ''}`}
                  {...register('warehouseId', { required: t('validation.required', 'Required') })}
                >
                  <option value="">{t('common.selectWarehouse', 'Select warehouse...')}</option>
                  {warehouses.map((wh) => (
                    <option key={wh.id} value={wh.id}>
                      {wh.code} - {wh.name}
                    </option>
                  ))}
                </select>
                {errors.warehouseId && <span className="form-field__error">{errors.warehouseId.message}</span>}
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('adjustments.adjustmentType', 'Adjustment Type')}</label>
                <select className="form-field__select" {...register('adjustmentType')}>
                  {ADJUSTMENT_TYPES.map((type) => (
                    <option key={type.value} value={type.value}>
                      {type.label}
                    </option>
                  ))}
                </select>
              </div>
              <div className="form-field form-field--full">
                <label className="form-field__label">{t('adjustments.reason', 'Reason')}</label>
                <textarea
                  className="form-field__textarea"
                  rows={2}
                  placeholder={t('adjustments.reasonPlaceholder', 'Reason for adjustment...')}
                  {...register('reasonNotes')}
                />
              </div>
            </div>
          </ModalSection>

          <ModalSection title={t('common.notes', 'Notes')} collapsible defaultExpanded={false}>
            <div className="form-grid">
              <div className="form-field form-field--full">
                <label className="form-field__label">{t('common.notes', 'Notes')}</label>
                <textarea
                  className="form-field__textarea"
                  rows={3}
                  placeholder={t('adjustments.notesPlaceholder', 'Additional notes...')}
                  {...register('notes')}
                />
              </div>
            </div>
          </ModalSection>
        </form>
      </FullscreenModal>

      {/* Cancel Confirmation Modal */}
      <Modal
        open={cancelConfirmOpen}
        onClose={() => setCancelConfirmOpen(false)}
        title={t('adjustments.cancelAdjustment', 'Cancel Adjustment')}
      >
        <div className="modal__body">
          <p>
            {t(
              'adjustments.cancelConfirmation',
              `Are you sure you want to cancel adjustment "${selectedAdjustment?.adjustmentNumber}"?`
            )}
          </p>
        </div>
        <div className="modal__actions">
          <button className="btn btn-ghost" onClick={() => setCancelConfirmOpen(false)}>
            {t('common.no', 'No')}
          </button>
          <button className="btn btn--danger" onClick={handleCancel} disabled={isCancelling}>
            {isCancelling ? t('common.cancelling', 'Cancelling...') : t('common.yes', 'Yes, Cancel')}
          </button>
        </div>
      </Modal>
    </div>
  );
}

export default Adjustments;
