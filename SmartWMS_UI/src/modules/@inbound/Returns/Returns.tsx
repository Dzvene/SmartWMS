import { useState, useMemo, useEffect } from 'react';
import { useIntl } from 'react-intl';
import { useForm } from 'react-hook-form';
import { DataTable, createColumns } from '@/components/DataTable';
import type { PaginationState, SortingState } from '@/components/DataTable';
import { FullscreenModal, ModalSection } from '@/components/FullscreenModal';
import { Modal } from '@/components';
import {
  useGetReturnOrdersQuery,
  useCreateReturnOrderMutation,
  useUpdateReturnOrderMutation,
  useDeleteReturnOrderMutation,
} from '@/api/modules/returns';
import type {
  ReturnOrderListDto,
  ReturnOrderStatus,
  ReturnType,
  CreateReturnOrderRequest,
} from '@/api/modules/returns';
import { useGetCustomersQuery } from '@/api/modules/orders';
import { useGetLocationsQuery } from '@/api/modules/locations';

const STATUS_COLORS: Record<ReturnOrderStatus, string> = {
  Pending: 'warning',
  InTransit: 'info',
  Received: 'info',
  InProgress: 'info',
  Complete: 'success',
  Cancelled: 'error',
};

const RETURN_TYPE_LABELS: Record<ReturnType, string> = {
  CustomerReturn: 'Customer',
  SupplierReturn: 'Supplier',
  InternalTransfer: 'Internal',
  Damaged: 'Damaged',
  Recall: 'Recall',
};

const RETURN_TYPES: ReturnType[] = [
  'CustomerReturn',
  'SupplierReturn',
  'InternalTransfer',
  'Damaged',
  'Recall',
];

interface ReturnFormData {
  customerId: string;
  returnType: ReturnType;
  receivingLocationId: string;
  requestedDate: string;
  rmaNumber: string;
  rmaExpiryDate: string;
  reasonDescription: string;
  notes: string;
}

/**
 * Returns Module
 * Manages customer return orders and RMA processing.
 */
export function Returns() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });

  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<ReturnOrderStatus | ''>('');
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 });
  const [sorting, setSorting] = useState<SortingState>([]);
  const [selectedReturn, setSelectedReturn] = useState<ReturnOrderListDto | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);

  // API Queries
  const { data: response, isLoading } = useGetReturnOrdersQuery({
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
    status: statusFilter || undefined,
    rmaNumber: searchQuery || undefined,
  });
  const { data: customersData } = useGetCustomersQuery({ page: 1, pageSize: 100 });
  const { data: locationsData } = useGetLocationsQuery({ page: 1, pageSize: 100 });

  const returns = response?.data?.items || [];
  const totalRows = response?.data?.totalCount || 0;
  const customers = customersData?.data?.items || [];
  const locations = locationsData?.data?.items || [];

  // API Mutations
  const [createReturn, { isLoading: isCreating }] = useCreateReturnOrderMutation();
  const [_updateReturn, { isLoading: isUpdating }] = useUpdateReturnOrderMutation();
  const [deleteReturn, { isLoading: isDeleting }] = useDeleteReturnOrderMutation();

  // Form
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<ReturnFormData>({
    defaultValues: {
      customerId: '',
      returnType: 'CustomerReturn',
      receivingLocationId: '',
      requestedDate: '',
      rmaNumber: '',
      rmaExpiryDate: '',
      reasonDescription: '',
      notes: '',
    },
  });

  // Reset form when modal opens/closes
  useEffect(() => {
    if (!isModalOpen) {
      reset({
        customerId: '',
        returnType: 'CustomerReturn',
        receivingLocationId: '',
        requestedDate: '',
        rmaNumber: '',
        rmaExpiryDate: '',
        reasonDescription: '',
        notes: '',
      });
    }
  }, [isModalOpen, reset]);

  const columnHelper = createColumns<ReturnOrderListDto>();

  const columns = useMemo(
    () => [
      columnHelper.accessor('returnNumber', {
        header: t('returns.returnNumber', 'Return #'),
        size: 120,
        cell: ({ getValue }) => <span className="code">{getValue()}</span>,
      }),
      columnHelper.accessor('rmaNumber', {
        header: t('returns.rmaNumber', 'RMA #'),
        size: 110,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('originalSalesOrderNumber', {
        header: t('returns.originalOrder', 'Original Order'),
        size: 120,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('customerName', {
        header: t('common.customer', 'Customer'),
        size: 140,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('returnType', {
        header: t('returns.type', 'Type'),
        size: 90,
        cell: ({ getValue }) => RETURN_TYPE_LABELS[getValue()] || getValue(),
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
      columnHelper.display({
        id: 'progress',
        header: t('returns.progress', 'Progress'),
        size: 100,
        cell: ({ row }) => {
          const { totalQuantityExpected, totalQuantityReceived } = row.original;
          const percent = totalQuantityExpected > 0
            ? Math.round((totalQuantityReceived / totalQuantityExpected) * 100)
            : 0;
          return (
            <span>
              {totalQuantityReceived}/{totalQuantityExpected} ({percent}%)
            </span>
          );
        },
      }),
      columnHelper.accessor('totalLines', {
        header: t('returns.lines', 'Lines'),
        size: 70,
      }),
      columnHelper.accessor('requestedDate', {
        header: t('returns.requestedDate', 'Requested'),
        size: 100,
        cell: ({ getValue }) => {
          const date = getValue();
          return date ? new Date(date).toLocaleDateString() : '-';
        },
      }),
      columnHelper.accessor('receivedDate', {
        header: t('returns.receivedDate', 'Received'),
        size: 100,
        cell: ({ getValue }) => {
          const date = getValue();
          return date ? new Date(date).toLocaleDateString() : '-';
        },
      }),
      columnHelper.accessor('createdAt', {
        header: t('common.createdAt', 'Created'),
        size: 100,
        cell: ({ getValue }) => new Date(getValue()).toLocaleDateString(),
      }),
    ],
    [columnHelper, t]
  );

  const handleRowClick = (returnOrder: ReturnOrderListDto) => {
    setSelectedReturn(returnOrder);
  };

  const handleAddNew = () => {
    setSelectedReturn(null);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setSelectedReturn(null);
  };

  const onSubmit = async (data: ReturnFormData) => {
    try {
      const createData: CreateReturnOrderRequest = {
        customerId: data.customerId,
        returnType: data.returnType,
        receivingLocationId: data.receivingLocationId || undefined,
        requestedDate: data.requestedDate || undefined,
        rmaNumber: data.rmaNumber || undefined,
        rmaExpiryDate: data.rmaExpiryDate || undefined,
        reasonDescription: data.reasonDescription || undefined,
        notes: data.notes || undefined,
      };
      await createReturn(createData).unwrap();
      handleCloseModal();
    } catch (error) {
      console.error('Failed to create return:', error);
    }
  };

  const handleDelete = async () => {
    if (!selectedReturn) return;

    try {
      await deleteReturn(selectedReturn.id).unwrap();
      setDeleteConfirmOpen(false);
      setSelectedReturn(null);
    } catch (error) {
      console.error('Failed to delete return:', error);
    }
  };

  const isSaving = isCreating || isUpdating;

  return (
    <div className="page">
      <header className="page__header">
        <div className="page__title-section">
          <h1 className="page__title">{t('returns.title', 'Returns')}</h1>
          <p className="page__subtitle">{t('returns.subtitle', 'Manage return orders and RMA processing')}</p>
        </div>
        <div className="page__actions">
          <button className="btn btn--primary" onClick={handleAddNew}>
            {t('returns.createRMA', 'Create RMA')}
          </button>
        </div>
      </header>

      <div className="page-toolbar">
        <div className="page-search">
          <input
            type="text"
            className="page-search__input"
            placeholder={t('returns.searchRMA', 'Search by RMA #...')}
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
        </div>
        <div className="page-filters">
          <select
            className="page-filter__select"
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value as ReturnOrderStatus | '')}
          >
            <option value="">{t('returns.allStatuses', 'All Statuses')}</option>
            <option value="Pending">Pending</option>
            <option value="InTransit">In Transit</option>
            <option value="Received">Received</option>
            <option value="InProgress">In Progress</option>
            <option value="Complete">Complete</option>
            <option value="Cancelled">Cancelled</option>
          </select>
        </div>
      </div>

      <div className="page__content">
        <DataTable
          data={returns}
          columns={columns}
          pagination={pagination}
          onPaginationChange={setPagination}
          sorting={sorting}
          onSortingChange={setSorting}
          totalRows={totalRows}
          onRowClick={handleRowClick}
          selectedRowId={selectedReturn?.id}
          getRowId={(row) => row.id}
          emptyMessage={t('returns.noReturns', 'No return orders found')}
          loading={isLoading}
        />
      </div>

      {/* Create Return Order Modal */}
      <FullscreenModal
        open={isModalOpen}
        onClose={handleCloseModal}
        title={t('returns.createRMA', 'Create RMA')}
        onSave={handleSubmit(onSubmit)}
        saveLabel={isSaving ? t('common.saving', 'Saving...') : t('common.save', 'Save')}
        saveDisabled={isSaving}
        maxWidth="lg"
      >
        <form>
          <ModalSection title={t('returns.returnDetails', 'Return Details')}>
            <div className="form-grid">
              <div className="form-field">
                <label className="form-field__label">
                  {t('common.customer', 'Customer')} <span className="required">*</span>
                </label>
                <select
                  className={`form-field__select ${errors.customerId ? 'form-field__select--error' : ''}`}
                  {...register('customerId', { required: t('validation.required', 'Required') })}
                >
                  <option value="">{t('common.selectCustomer', 'Select customer...')}</option>
                  {customers.map((customer) => (
                    <option key={customer.id} value={customer.id}>
                      {customer.code} - {customer.name}
                    </option>
                  ))}
                </select>
                {errors.customerId && <span className="form-field__error">{errors.customerId.message}</span>}
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('returns.type', 'Return Type')}</label>
                <select className="form-field__select" {...register('returnType')}>
                  {RETURN_TYPES.map((type) => (
                    <option key={type} value={type}>
                      {RETURN_TYPE_LABELS[type]}
                    </option>
                  ))}
                </select>
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('returns.rmaNumber', 'RMA Number')}</label>
                <input
                  type="text"
                  className="form-field__input"
                  placeholder="RMA-2024-001"
                  {...register('rmaNumber')}
                />
                <p className="form-field__hint">{t('returns.rmaNumberHint', 'Leave empty to auto-generate')}</p>
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('returns.rmaExpiryDate', 'RMA Expiry Date')}</label>
                <input
                  type="date"
                  className="form-field__input"
                  {...register('rmaExpiryDate')}
                />
              </div>
            </div>
          </ModalSection>

          <ModalSection title={t('returns.shippingDetails', 'Shipping & Receiving')} collapsible defaultExpanded>
            <div className="form-grid">
              <div className="form-field">
                <label className="form-field__label">{t('returns.requestedDate', 'Requested Date')}</label>
                <input
                  type="date"
                  className="form-field__input"
                  {...register('requestedDate')}
                />
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('returns.receivingLocation', 'Receiving Location')}</label>
                <select className="form-field__select" {...register('receivingLocationId')}>
                  <option value="">{t('common.selectLocation', 'Select location...')}</option>
                  {locations.map((loc) => (
                    <option key={loc.id} value={loc.id}>
                      {loc.code} - {loc.name || loc.zoneName}
                    </option>
                  ))}
                </select>
              </div>
              <div className="form-field form-field--full">
                <label className="form-field__label">{t('returns.reasonDescription', 'Reason')}</label>
                <textarea
                  className="form-field__textarea"
                  rows={2}
                  placeholder={t('returns.reasonPlaceholder', 'Reason for return...')}
                  {...register('reasonDescription')}
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
                  placeholder={t('returns.notesPlaceholder', 'Internal notes...')}
                  {...register('notes')}
                />
              </div>
            </div>
          </ModalSection>
        </form>
      </FullscreenModal>

      {/* Delete Confirmation Modal */}
      <Modal
        open={deleteConfirmOpen}
        onClose={() => setDeleteConfirmOpen(false)}
        title={t('returns.deleteReturn', 'Delete Return')}
      >
        <div className="modal__body">
          <p>
            {t(
              'returns.deleteConfirmation',
              `Are you sure you want to delete return "${selectedReturn?.returnNumber}"? This action cannot be undone.`
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

export default Returns;
