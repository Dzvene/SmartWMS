import { useState, useMemo, useEffect } from 'react';
import { useIntl } from 'react-intl';
import { useForm } from 'react-hook-form';
import type { ColumnDef } from '@tanstack/react-table';

import { DataTable } from '@/components/DataTable';
import { FullscreenModal, ModalSection } from '@/components/FullscreenModal';
import { Modal } from '@/components';
import {
  useGetCycleCountsQuery,
  useCreateCycleCountMutation,
  useCancelCycleCountMutation,
} from '@/api/modules/cycleCount';
import type {
  CycleCountSessionListDto,
  CycleCountStatus,
  CountType,
  CountScope,
  CreateCycleCountRequest,
} from '@/api/modules/cycleCount';
import { useGetWarehouseOptionsQuery } from '@/api/modules/warehouses';

const statusLabels: Record<CycleCountStatus, string> = {
  Scheduled: 'Scheduled',
  InProgress: 'In Progress',
  Completed: 'Completed',
  Cancelled: 'Cancelled',
  PartiallyCompleted: 'Partial',
};

const statusClasses: Record<CycleCountStatus, string> = {
  Scheduled: 'scheduled',
  InProgress: 'in_progress',
  Completed: 'completed',
  Cancelled: 'cancelled',
  PartiallyCompleted: 'review',
};

const COUNT_TYPES: { value: CountType; label: string }[] = [
  { value: 'Full', label: 'Full Count' },
  { value: 'Cycle', label: 'Cycle Count' },
  { value: 'Spot', label: 'Spot Check' },
  { value: 'ABC', label: 'ABC Analysis' },
  { value: 'Random', label: 'Random Sample' },
];

const COUNT_SCOPES: { value: CountScope; label: string }[] = [
  { value: 'Location', label: 'By Location' },
  { value: 'Product', label: 'By Product' },
  { value: 'Category', label: 'By Category' },
  { value: 'Zone', label: 'By Zone' },
  { value: 'Warehouse', label: 'Full Warehouse' },
];

interface CycleCountFormData {
  warehouseId: string;
  countType: CountType;
  countScope: CountScope;
  scheduledDate: string;
  notes: string;
}

export function CycleCount() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });

  const [selectedTask, setSelectedTask] = useState<CycleCountSessionListDto | null>(null);
  const [paginationState, setPaginationState] = useState({ page: 1, pageSize: 25 });
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [cancelConfirmOpen, setCancelConfirmOpen] = useState(false);

  // API Queries
  const { data, isLoading } = useGetCycleCountsQuery({
    page: paginationState.page,
    pageSize: paginationState.pageSize,
  });
  const { data: warehousesData } = useGetWarehouseOptionsQuery();

  const tasks = data?.data?.items ?? [];
  const totalRows = data?.data?.totalCount ?? 0;
  const warehouses = warehousesData?.data || [];

  // API Mutations
  const [createCount, { isLoading: isCreating }] = useCreateCycleCountMutation();
  const [cancelCount, { isLoading: isCancelling }] = useCancelCycleCountMutation();

  // Form
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CycleCountFormData>({
    defaultValues: {
      warehouseId: '',
      countType: 'Cycle',
      countScope: 'Location',
      scheduledDate: '',
      notes: '',
    },
  });

  // Reset form when modal closes
  useEffect(() => {
    if (!isModalOpen) {
      reset({
        warehouseId: '',
        countType: 'Cycle',
        countScope: 'Location',
        scheduledDate: '',
        notes: '',
      });
    }
  }, [isModalOpen, reset]);

  const columns = useMemo<ColumnDef<CycleCountSessionListDto, unknown>[]>(() => [
    { accessorKey: 'countNumber', header: t('cycleCount.countNumber', 'Count #'), size: 100 },
    { accessorKey: 'countType', header: t('common.type', 'Type'), size: 80 },
    { accessorKey: 'warehouseName', header: t('common.warehouse', 'Warehouse'), size: 120, cell: ({ getValue }) => getValue() || '—' },
    {
      accessorKey: 'scheduledDate',
      header: t('cycleCount.scheduled', 'Scheduled'),
      size: 100,
      cell: ({ getValue }) => getValue() ? new Date(getValue() as string).toLocaleDateString() : '—',
    },
    {
      id: 'progress',
      header: t('cycleCount.progress', 'Progress'),
      size: 100,
      cell: ({ row }) => `${row.original.countedItems}/${row.original.totalItems}`,
    },
    { accessorKey: 'varianceItems', header: t('cycleCount.variance', 'Variance'), size: 80, meta: { align: 'right' } },
    { accessorKey: 'assignedUserName', header: t('cycleCount.assignedTo', 'Assigned To'), size: 120, cell: ({ getValue }) => getValue() || '—' },
    {
      accessorKey: 'status',
      header: t('common.status', 'Status'),
      size: 100,
      cell: ({ getValue }) => {
        const status = getValue() as CycleCountStatus;
        return <span className={`status-badge status-badge--${statusClasses[status]}`}>{statusLabels[status]}</span>;
      },
    },
  ], [t]);

  const handleAddNew = () => {
    setSelectedTask(null);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setSelectedTask(null);
  };

  const onSubmit = async (data: CycleCountFormData) => {
    try {
      const createData: CreateCycleCountRequest = {
        warehouseId: data.warehouseId,
        countType: data.countType,
        countScope: data.countScope,
        scheduledDate: data.scheduledDate || undefined,
        notes: data.notes || undefined,
      };
      await createCount(createData).unwrap();
      handleCloseModal();
    } catch (error) {
      console.error('Failed to create cycle count:', error);
    }
  };

  const handleCancel = async () => {
    if (!selectedTask) return;

    try {
      await cancelCount({ id: selectedTask.id }).unwrap();
      setCancelConfirmOpen(false);
      setSelectedTask(null);
    } catch (error) {
      console.error('Failed to cancel cycle count:', error);
    }
  };

  const isSaving = isCreating;

  return (
    <div className="page">
      <div className="page__header">
        <h1 className="page__title">{t('cycleCount.title', 'Cycle Count')}</h1>
        <div className="page__actions">
          <button className="btn btn--primary" onClick={handleAddNew}>
            {t('cycleCount.scheduleCount', 'Schedule Count')}
          </button>
        </div>
      </div>
      <div className="page__content">
        <DataTable
          data={tasks}
          columns={columns}
          pagination={{ pageIndex: paginationState.page - 1, pageSize: paginationState.pageSize }}
          onPaginationChange={({ pageIndex, pageSize }) => {
            setPaginationState({ page: pageIndex + 1, pageSize });
          }}
          totalRows={totalRows}
          onRowClick={setSelectedTask}
          selectedRowId={selectedTask?.id}
          getRowId={(row) => row.id}
          loading={isLoading}
          emptyMessage={t('common.noData', 'No data found')}
        />
      </div>

      {/* Create Cycle Count Modal */}
      <FullscreenModal
        open={isModalOpen}
        onClose={handleCloseModal}
        title={t('cycleCount.scheduleCount', 'Schedule Cycle Count')}
        onSave={handleSubmit(onSubmit)}
        saveLabel={isSaving ? t('common.saving', 'Saving...') : t('common.save', 'Save')}
        saveDisabled={isSaving}
        maxWidth="lg"
      >
        <form>
          <ModalSection title={t('cycleCount.countDetails', 'Count Details')}>
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
                <label className="form-field__label">{t('cycleCount.countType', 'Count Type')}</label>
                <select className="form-field__select" {...register('countType')}>
                  {COUNT_TYPES.map((type) => (
                    <option key={type.value} value={type.value}>
                      {type.label}
                    </option>
                  ))}
                </select>
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('cycleCount.countScope', 'Count Scope')}</label>
                <select className="form-field__select" {...register('countScope')}>
                  {COUNT_SCOPES.map((scope) => (
                    <option key={scope.value} value={scope.value}>
                      {scope.label}
                    </option>
                  ))}
                </select>
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('cycleCount.scheduledDate', 'Scheduled Date')}</label>
                <input
                  type="date"
                  className="form-field__input"
                  {...register('scheduledDate')}
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
                  placeholder={t('cycleCount.notesPlaceholder', 'Additional notes...')}
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
        title={t('cycleCount.cancelCount', 'Cancel Cycle Count')}
      >
        <div className="modal__body">
          <p>
            {t(
              'cycleCount.cancelConfirmation',
              `Are you sure you want to cancel count "${selectedTask?.countNumber}"?`
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

export default CycleCount;
