import { useState, useMemo, useEffect } from 'react';
import { useIntl } from 'react-intl';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { DataTable, createColumns } from '@/components/DataTable';
import type { PaginationState, SortingState } from '@/components/DataTable';
import { FullscreenModal, ModalSection } from '@/components/FullscreenModal';
import { Modal } from '@/components';
import {
  useGetPutawayTasksQuery,
  useCreatePutawayTaskMutation,
  useCancelPutawayTaskMutation,
  useAssignPutawayTaskMutation,
} from '@/api/modules/putaway';
import type { PutawayTaskDto, PutawayTaskStatus, CreatePutawayTaskRequest } from '@/api/modules/putaway';
import { useGetProductsQuery } from '@/api/modules/products';
import { useGetLocationsQuery } from '@/api/modules/locations';
import { INBOUND } from '@/constants/routes';

const STATUS_COLORS: Record<PutawayTaskStatus, string> = {
  Pending: 'warning',
  Assigned: 'info',
  InProgress: 'info',
  Complete: 'success',
  Cancelled: 'error',
};

interface PutawayFormData {
  productId: string;
  fromLocationId: string;
  quantity: number;
  batchNumber: string;
  serialNumber: string;
  expiryDate: string;
  suggestedLocationId: string;
  priority: number;
  notes: string;
}

/**
 * Putaway Module
 * Manages putaway tasks for received inventory.
 */
export function Putaway() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();

  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<PutawayTaskStatus | ''>('');
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 });
  const [sorting, setSorting] = useState<SortingState>([]);
  const [selectedTask, setSelectedTask] = useState<PutawayTaskDto | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [cancelConfirmOpen, setCancelConfirmOpen] = useState(false);

  // API Queries
  const { data: response, isLoading } = useGetPutawayTasksQuery({
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
    search: searchQuery || undefined,
    status: statusFilter || undefined,
  });
  const { data: productsData } = useGetProductsQuery({ page: 1, pageSize: 100 });
  const { data: locationsData } = useGetLocationsQuery({ page: 1, pageSize: 100 });

  const tasks = response?.data?.items || [];
  const totalRows = response?.data?.totalCount || 0;
  const products = productsData?.data?.items || [];
  const locations = locationsData?.data?.items || [];

  // API Mutations
  const [createTask, { isLoading: isCreating }] = useCreatePutawayTaskMutation();
  const [cancelTask, { isLoading: isCancelling }] = useCancelPutawayTaskMutation();
  const [_assignTask] = useAssignPutawayTaskMutation();

  // Form
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<PutawayFormData>({
    defaultValues: {
      productId: '',
      fromLocationId: '',
      quantity: 1,
      batchNumber: '',
      serialNumber: '',
      expiryDate: '',
      suggestedLocationId: '',
      priority: 5,
      notes: '',
    },
  });

  // Reset form when modal opens/closes
  useEffect(() => {
    if (!isModalOpen) {
      reset({
        productId: '',
        fromLocationId: '',
        quantity: 1,
        batchNumber: '',
        serialNumber: '',
        expiryDate: '',
        suggestedLocationId: '',
        priority: 5,
        notes: '',
      });
    }
  }, [isModalOpen, reset]);

  const columnHelper = createColumns<PutawayTaskDto>();

  const columns = useMemo(
    () => [
      columnHelper.accessor('taskNumber', {
        header: t('putaway.taskNumber', 'Task #'),
        size: 100,
        cell: ({ getValue }) => <span className="code">{getValue()}</span>,
      }),
      columnHelper.accessor('sku', {
        header: t('products.sku', 'SKU'),
        size: 100,
        cell: ({ getValue }) => <span className="sku">{getValue() || '-'}</span>,
      }),
      columnHelper.accessor('productName', {
        header: t('common.product', 'Product'),
        size: 150,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.display({
        id: 'quantity',
        header: t('common.quantity', 'Qty'),
        size: 100,
        cell: ({ row }) => {
          const { quantityPutaway, quantityToPutaway } = row.original;
          return (
            <span>
              {quantityPutaway}/{quantityToPutaway}
            </span>
          );
        },
      }),
      columnHelper.accessor('fromLocationCode', {
        header: t('putaway.fromLocation', 'From'),
        size: 100,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('suggestedLocationCode', {
        header: t('putaway.suggestedLocation', 'Suggested'),
        size: 110,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('status', {
        header: t('common.status', 'Status'),
        size: 100,
        cell: ({ getValue }) => {
          const status = getValue();
          return (
            <span className={`status-badge status-badge--${STATUS_COLORS[status]}`}>
              {status}
            </span>
          );
        },
      }),
      columnHelper.accessor('assignedToUserName', {
        header: t('putaway.assignedTo', 'Assigned To'),
        size: 120,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('priority', {
        header: t('common.priority', 'Priority'),
        size: 80,
      }),
    ],
    [columnHelper, t]
  );

  const handleRowClick = (task: PutawayTaskDto) => {
    navigate(`${INBOUND.PUTAWAY}/${task.id}`);
  };

  const handleAddNew = () => {
    setSelectedTask(null);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setSelectedTask(null);
  };

  const onSubmit = async (data: PutawayFormData) => {
    try {
      const createData: CreatePutawayTaskRequest = {
        productId: data.productId,
        fromLocationId: data.fromLocationId,
        quantity: data.quantity,
        batchNumber: data.batchNumber || undefined,
        serialNumber: data.serialNumber || undefined,
        expiryDate: data.expiryDate || undefined,
        suggestedLocationId: data.suggestedLocationId || undefined,
        priority: data.priority,
        notes: data.notes || undefined,
      };
      await createTask(createData).unwrap();
      handleCloseModal();
    } catch (error) {
      console.error('Failed to create task:', error);
    }
  };

  const handleCancel = async () => {
    if (!selectedTask) return;

    try {
      await cancelTask(selectedTask.id).unwrap();
      setCancelConfirmOpen(false);
      setSelectedTask(null);
    } catch (error) {
      console.error('Failed to cancel task:', error);
    }
  };

  const handleAutoAssign = () => {
    // This would trigger an API call to auto-assign pending tasks
    // For now, just show alert
    console.log('Auto-assign triggered');
  };

  const isSaving = isCreating;

  return (
    <div className="page">
      <header className="page__header">
        <div className="page__title-section">
          <h1 className="page__title">{t('putaway.title', 'Putaway')}</h1>
          <p className="page__subtitle">{t('putaway.subtitle', 'Manage putaway tasks')}</p>
        </div>
        <div className="page__actions">
          <button className="btn btn--secondary" onClick={handleAutoAssign}>
            {t('putaway.autoAssign', 'Auto Assign')}
          </button>
          <button className="btn btn--primary" onClick={handleAddNew}>
            {t('putaway.createTask', 'Create Task')}
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
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value as PutawayTaskStatus | '')}
          >
            <option value="">{t('putaway.allStatuses', 'All Statuses')}</option>
            <option value="Pending">Pending</option>
            <option value="Assigned">Assigned</option>
            <option value="InProgress">In Progress</option>
            <option value="Complete">Complete</option>
            <option value="Cancelled">Cancelled</option>
          </select>
        </div>
      </div>

      <div className="page__content">
        <DataTable
          data={tasks}
          columns={columns}
          pagination={pagination}
          onPaginationChange={setPagination}
          sorting={sorting}
          onSortingChange={setSorting}
          totalRows={totalRows}
          onRowClick={handleRowClick}
          selectedRowId={selectedTask?.id}
          getRowId={(row) => row.id}
          emptyMessage={t('putaway.noTasks', 'No putaway tasks found')}
          loading={isLoading}
        />
      </div>

      {/* Create Putaway Task Modal */}
      <FullscreenModal
        open={isModalOpen}
        onClose={handleCloseModal}
        title={t('putaway.createTask', 'Create Putaway Task')}
        onSave={handleSubmit(onSubmit)}
        saveLabel={isSaving ? t('common.saving', 'Saving...') : t('common.save', 'Save')}
        saveDisabled={isSaving}
        maxWidth="lg"
      >
        <form>
          <ModalSection title={t('putaway.taskDetails', 'Task Details')}>
            <div className="form-grid">
              <div className="form-field">
                <label className="form-field__label">
                  {t('common.product', 'Product')} <span className="required">*</span>
                </label>
                <select
                  className={`form-field__select ${errors.productId ? 'form-field__select--error' : ''}`}
                  {...register('productId', { required: t('validation.required', 'Required') })}
                >
                  <option value="">{t('common.selectProduct', 'Select product...')}</option>
                  {products.map((product) => (
                    <option key={product.id} value={product.id}>
                      {product.sku} - {product.name}
                    </option>
                  ))}
                </select>
                {errors.productId && <span className="form-field__error">{errors.productId.message}</span>}
              </div>
              <div className="form-field">
                <label className="form-field__label">
                  {t('putaway.fromLocation', 'From Location')} <span className="required">*</span>
                </label>
                <select
                  className={`form-field__select ${errors.fromLocationId ? 'form-field__select--error' : ''}`}
                  {...register('fromLocationId', { required: t('validation.required', 'Required') })}
                >
                  <option value="">{t('common.selectLocation', 'Select location...')}</option>
                  {locations.map((loc) => (
                    <option key={loc.id} value={loc.id}>
                      {loc.code} - {loc.name || loc.zoneName}
                    </option>
                  ))}
                </select>
                {errors.fromLocationId && <span className="form-field__error">{errors.fromLocationId.message}</span>}
              </div>
              <div className="form-field">
                <label className="form-field__label">
                  {t('common.quantity', 'Quantity')} <span className="required">*</span>
                </label>
                <input
                  type="number"
                  min="1"
                  className={`form-field__input ${errors.quantity ? 'form-field__input--error' : ''}`}
                  {...register('quantity', { required: t('validation.required', 'Required'), min: 1 })}
                />
                {errors.quantity && <span className="form-field__error">{errors.quantity.message}</span>}
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('common.priority', 'Priority')}</label>
                <input
                  type="number"
                  min="1"
                  max="10"
                  className="form-field__input"
                  {...register('priority')}
                />
                <p className="form-field__hint">{t('putaway.priorityHint', '1-10, lower is higher priority')}</p>
              </div>
            </div>
          </ModalSection>

          <ModalSection title={t('putaway.batchInfo', 'Batch Information')} collapsible defaultExpanded>
            <div className="form-grid">
              <div className="form-field">
                <label className="form-field__label">{t('putaway.batchNumber', 'Batch Number')}</label>
                <input
                  type="text"
                  className="form-field__input"
                  placeholder="BATCH-001"
                  {...register('batchNumber')}
                />
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('putaway.serialNumber', 'Serial Number')}</label>
                <input
                  type="text"
                  className="form-field__input"
                  placeholder="SN-12345"
                  {...register('serialNumber')}
                />
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('putaway.expiryDate', 'Expiry Date')}</label>
                <input
                  type="date"
                  className="form-field__input"
                  {...register('expiryDate')}
                />
              </div>
            </div>
          </ModalSection>

          <ModalSection title={t('putaway.putawayDetails', 'Putaway Details')} collapsible defaultExpanded>
            <div className="form-grid">
              <div className="form-field">
                <label className="form-field__label">{t('putaway.suggestedLocation', 'Suggested Location')}</label>
                <select
                  className="form-field__select"
                  {...register('suggestedLocationId')}
                >
                  <option value="">{t('putaway.noSuggestion', 'No suggestion')}</option>
                  {locations.map((loc) => (
                    <option key={loc.id} value={loc.id}>
                      {loc.code} - {loc.name || loc.zoneName}
                    </option>
                  ))}
                </select>
                <p className="form-field__hint">{t('putaway.suggestedLocationHint', 'Leave empty to auto-suggest')}</p>
              </div>
              <div className="form-field form-field--full">
                <label className="form-field__label">{t('common.notes', 'Notes')}</label>
                <textarea
                  className="form-field__textarea"
                  rows={3}
                  placeholder={t('putaway.notesPlaceholder', 'Internal notes...')}
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
        title={t('putaway.cancelTask', 'Cancel Putaway Task')}
      >
        <div className="modal__body">
          <p>
            {t(
              'putaway.cancelConfirmation',
              `Are you sure you want to cancel task "${selectedTask?.taskNumber}"?`
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

export default Putaway;
