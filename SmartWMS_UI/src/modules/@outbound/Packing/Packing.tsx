import { useState, useMemo, useEffect } from 'react';
import { useIntl } from 'react-intl';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { DataTable, createColumns } from '@/components/DataTable';
import type { PaginationState, SortingState } from '@/components/DataTable';
import { FullscreenModal, ModalSection } from '@/components/FullscreenModal';
import { Modal } from '@/components';
import {
  useGetPackingTasksQuery,
  useCreatePackingTaskMutation,
  useCancelPackingTaskMutation,
  useGetPackingStationsQuery,
} from '@/api/modules/packing';
import type { PackingTaskListDto, PackingTaskStatus, CreatePackingTaskRequest } from '@/api/modules/packing';
import { useGetSalesOrdersQuery } from '@/api/modules/orders';
import { OUTBOUND } from '@/constants/routes';

const STATUS_COLORS: Record<PackingTaskStatus, string> = {
  Pending: 'warning',
  Assigned: 'info',
  InProgress: 'info',
  Completed: 'success',
  Cancelled: 'error',
};

interface PackingFormData {
  salesOrderId: string;
  packingStationId: string;
  priority: number;
  notes: string;
}

/**
 * Packing Module
 * Manages packing tasks for order fulfillment.
 */
export function Packing() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();

  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<PackingTaskStatus | ''>('');
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 });
  const [sorting, setSorting] = useState<SortingState>([]);
  const [selectedTask, setSelectedTask] = useState<PackingTaskListDto | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [cancelConfirmOpen, setCancelConfirmOpen] = useState(false);

  // API Queries
  const { data: response, isLoading } = useGetPackingTasksQuery({
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
    search: searchQuery || undefined,
    status: statusFilter || undefined,
  });
  const { data: salesOrdersData } = useGetSalesOrdersQuery({ page: 1, pageSize: 100 });
  const { data: stationsData } = useGetPackingStationsQuery();

  const tasks = response?.data?.items || [];
  const totalRows = response?.data?.totalCount || 0;
  const salesOrders = salesOrdersData?.data?.items || [];
  const stations = stationsData?.data?.items || [];

  // API Mutations
  const [createTask, { isLoading: isCreating }] = useCreatePackingTaskMutation();
  const [cancelTask, { isLoading: isCancelling }] = useCancelPackingTaskMutation();

  // Form
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<PackingFormData>({
    defaultValues: {
      salesOrderId: '',
      packingStationId: '',
      priority: 5,
      notes: '',
    },
  });

  // Reset form when modal closes
  useEffect(() => {
    if (!isModalOpen) {
      reset({
        salesOrderId: '',
        packingStationId: '',
        priority: 5,
        notes: '',
      });
    }
  }, [isModalOpen, reset]);

  const columnHelper = createColumns<PackingTaskListDto>();

  const columns = useMemo(
    () => [
      columnHelper.accessor('taskNumber', {
        header: t('packing.taskNumber', 'Task #'),
        size: 100,
        cell: ({ getValue }) => <span className="code">{getValue()}</span>,
      }),
      columnHelper.accessor('salesOrderNumber', {
        header: t('packing.orderNumber', 'Order #'),
        size: 120,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('customerName', {
        header: t('common.customer', 'Customer'),
        size: 140,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('packingStationCode', {
        header: t('packing.station', 'Station'),
        size: 100,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.display({
        id: 'progress',
        header: t('packing.progress', 'Progress'),
        size: 100,
        cell: ({ row }) => {
          const { packedItems, totalItems } = row.original;
          return `${packedItems}/${totalItems}`;
        },
      }),
      columnHelper.accessor('boxCount', {
        header: t('packing.boxes', 'Boxes'),
        size: 70,
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
        header: t('packing.assignedTo', 'Assigned To'),
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

  const handleRowClick = (task: PackingTaskListDto) => {
    setSelectedTask(task);
    navigate(`${OUTBOUND.PACKING}/${task.id}`);
  };

  const handleAddNew = () => {
    setSelectedTask(null);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setSelectedTask(null);
  };

  const onSubmit = async (data: PackingFormData) => {
    try {
      const createData: CreatePackingTaskRequest = {
        salesOrderId: data.salesOrderId,
        packingStationId: data.packingStationId || undefined,
        priority: data.priority,
        notes: data.notes || undefined,
      };
      await createTask(createData).unwrap();
      handleCloseModal();
    } catch (error) {
      console.error('Failed to create packing task:', error);
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

  const isSaving = isCreating;

  return (
    <div className="page">
      <header className="page__header">
        <div className="page__title-section">
          <h1 className="page__title">{t('packing.title', 'Packing')}</h1>
          <p className="page__subtitle">{t('packing.subtitle', 'Manage packing tasks')}</p>
        </div>
        <div className="page__actions">
          <button className="btn btn--primary" onClick={handleAddNew}>
            {t('packing.createTask', 'Create Task')}
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
            onChange={(e) => setStatusFilter(e.target.value as PackingTaskStatus | '')}
          >
            <option value="">{t('packing.allStatuses', 'All Statuses')}</option>
            <option value="Pending">Pending</option>
            <option value="Assigned">Assigned</option>
            <option value="InProgress">In Progress</option>
            <option value="Completed">Completed</option>
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
          emptyMessage={t('packing.noTasks', 'No packing tasks found')}
          loading={isLoading}
        />
      </div>

      {/* Create Packing Task Modal */}
      <FullscreenModal
        open={isModalOpen}
        onClose={handleCloseModal}
        title={t('packing.createTask', 'Create Packing Task')}
        onSave={handleSubmit(onSubmit)}
        saveLabel={isSaving ? t('common.saving', 'Saving...') : t('common.save', 'Save')}
        saveDisabled={isSaving}
        maxWidth="lg"
      >
        <form>
          <ModalSection title={t('packing.taskDetails', 'Task Details')}>
            <div className="form-grid">
              <div className="form-field">
                <label className="form-field__label">
                  {t('packing.salesOrder', 'Sales Order')} <span className="required">*</span>
                </label>
                <select
                  className={`form-field__select ${errors.salesOrderId ? 'form-field__select--error' : ''}`}
                  {...register('salesOrderId', { required: t('validation.required', 'Required') })}
                >
                  <option value="">{t('common.selectOrder', 'Select order...')}</option>
                  {salesOrders.map((order) => (
                    <option key={order.id} value={order.id}>
                      {order.orderNumber} - {order.customerName}
                    </option>
                  ))}
                </select>
                {errors.salesOrderId && <span className="form-field__error">{errors.salesOrderId.message}</span>}
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('packing.station', 'Packing Station')}</label>
                <select className="form-field__select" {...register('packingStationId')}>
                  <option value="">{t('packing.selectStation', 'Select station...')}</option>
                  {stations.map((station) => (
                    <option key={station.id} value={station.id}>
                      {station.code} - {station.name}
                    </option>
                  ))}
                </select>
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
                <p className="form-field__hint">{t('packing.priorityHint', '1-10, lower is higher priority')}</p>
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
                  placeholder={t('packing.notesPlaceholder', 'Internal notes...')}
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
        title={t('packing.cancelTask', 'Cancel Packing Task')}
      >
        <div className="modal__body">
          <p>
            {t(
              'packing.cancelConfirmation',
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

export default Packing;
