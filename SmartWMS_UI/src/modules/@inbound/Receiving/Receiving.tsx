import { useState, useMemo, useEffect } from 'react';
import { useIntl } from 'react-intl';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { DataTable, createColumns } from '@/components/DataTable';
import type { PaginationState, SortingState } from '@/components/DataTable';
import { FullscreenModal, ModalSection } from '@/components/FullscreenModal';
import { Modal } from '@/components';
import {
  useGetGoodsReceiptsQuery,
  useCreateGoodsReceiptMutation,
  useUpdateGoodsReceiptMutation,
  useDeleteGoodsReceiptMutation,
} from '@/api/modules/receiving';
import type { GoodsReceiptDto, GoodsReceiptStatus, CreateGoodsReceiptRequest, UpdateGoodsReceiptRequest } from '@/api/modules/receiving';
import { useGetWarehouseOptionsQuery } from '@/api/modules/warehouses';
import { useGetSuppliersQuery } from '@/api/modules/orders';
import { INBOUND } from '@/constants/routes';

const STATUS_COLORS: Record<GoodsReceiptStatus, string> = {
  Draft: 'neutral',
  Pending: 'warning',
  InProgress: 'info',
  Completed: 'success',
  PartiallyReceived: 'warning',
  Cancelled: 'error',
};

interface ReceivingFormData {
  warehouseId: string;
  supplierId: string;
  carrierName: string;
  trackingNumber: string;
  deliveryNote: string;
  notes: string;
}

/**
 * Receiving Module
 * Manages inbound receipt processing from purchase orders.
 */
export function Receiving() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();

  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<GoodsReceiptStatus | ''>('');
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 });
  const [sorting, setSorting] = useState<SortingState>([]);
  const [selectedReceipt, setSelectedReceipt] = useState<GoodsReceiptDto | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);

  // API Queries
  const { data: response, isLoading } = useGetGoodsReceiptsQuery({
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
    search: searchQuery || undefined,
    status: statusFilter || undefined,
  });
  const { data: warehousesData } = useGetWarehouseOptionsQuery();
  const { data: suppliersData } = useGetSuppliersQuery({ page: 1, pageSize: 100 });

  const receipts = response?.data?.items || [];
  const totalRows = response?.data?.totalCount || 0;
  const warehouses = warehousesData?.data || [];
  const suppliers = suppliersData?.data?.items || [];

  // API Mutations
  const [createReceipt, { isLoading: isCreating }] = useCreateGoodsReceiptMutation();
  const [updateReceipt, { isLoading: isUpdating }] = useUpdateGoodsReceiptMutation();
  const [deleteReceipt, { isLoading: isDeleting }] = useDeleteGoodsReceiptMutation();

  // Form
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<ReceivingFormData>({
    defaultValues: {
      warehouseId: '',
      supplierId: '',
      carrierName: '',
      trackingNumber: '',
      deliveryNote: '',
      notes: '',
    },
  });

  // Reset form when editing changes
  useEffect(() => {
    if (selectedReceipt) {
      reset({
        warehouseId: selectedReceipt.warehouseId,
        supplierId: selectedReceipt.supplierId || '',
        carrierName: selectedReceipt.carrierName || '',
        trackingNumber: selectedReceipt.trackingNumber || '',
        deliveryNote: selectedReceipt.deliveryNote || '',
        notes: selectedReceipt.notes || '',
      });
    } else {
      reset({
        warehouseId: '',
        supplierId: '',
        carrierName: '',
        trackingNumber: '',
        deliveryNote: '',
        notes: '',
      });
    }
  }, [selectedReceipt, reset]);

  const columnHelper = createColumns<GoodsReceiptDto>();

  const columns = useMemo(
    () => [
      columnHelper.accessor('receiptNumber', {
        header: t('receiving.receiptNumber', 'Receipt #'),
        size: 130,
        cell: ({ getValue }) => <span className="code">{getValue()}</span>,
      }),
      columnHelper.accessor('purchaseOrderNumber', {
        header: t('receiving.poNumber', 'PO #'),
        size: 120,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('supplierName', {
        header: t('common.supplier', 'Supplier'),
        size: 150,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('warehouseName', {
        header: t('common.warehouse', 'Warehouse'),
        size: 140,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('receiptDate', {
        header: t('receiving.receiptDate', 'Receipt Date'),
        size: 110,
        cell: ({ getValue }) => new Date(getValue()).toLocaleDateString(),
      }),
      columnHelper.accessor('status', {
        header: t('common.status', 'Status'),
        size: 120,
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
        header: t('receiving.progress', 'Progress'),
        size: 120,
        cell: ({ row }) => {
          const { totalQuantityReceived, totalQuantityExpected, progressPercent } = row.original;
          return (
            <div className="progress-cell">
              <span>{totalQuantityReceived}/{totalQuantityExpected}</span>
              <span className="progress-cell__percent">({progressPercent}%)</span>
            </div>
          );
        },
      }),
      columnHelper.accessor('totalLines', {
        header: t('receiving.lines', 'Lines'),
        size: 70,
      }),
    ],
    [columnHelper, t]
  );

  const handleRowClick = (receipt: GoodsReceiptDto) => {
    navigate(`${INBOUND.RECEIVING}/${receipt.id}`);
  };

  const handleAddNew = () => {
    setSelectedReceipt(null);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setSelectedReceipt(null);
  };

  const onSubmit = async (data: ReceivingFormData) => {
    try {
      if (selectedReceipt) {
        const updateData: UpdateGoodsReceiptRequest = {
          carrierName: data.carrierName || undefined,
          trackingNumber: data.trackingNumber || undefined,
          deliveryNote: data.deliveryNote || undefined,
          notes: data.notes || undefined,
        };
        await updateReceipt({ id: selectedReceipt.id, body: updateData }).unwrap();
      } else {
        const createData: CreateGoodsReceiptRequest = {
          warehouseId: data.warehouseId,
          supplierId: data.supplierId || undefined,
          carrierName: data.carrierName || undefined,
          trackingNumber: data.trackingNumber || undefined,
          deliveryNote: data.deliveryNote || undefined,
          notes: data.notes || undefined,
        };
        await createReceipt(createData).unwrap();
      }
      handleCloseModal();
    } catch (error) {
      console.error('Failed to save receipt:', error);
    }
  };

  const handleDelete = async () => {
    if (!selectedReceipt) return;

    try {
      await deleteReceipt(selectedReceipt.id).unwrap();
      setDeleteConfirmOpen(false);
      handleCloseModal();
    } catch (error) {
      console.error('Failed to delete receipt:', error);
    }
  };

  const isSaving = isCreating || isUpdating;

  return (
    <div className="page">
      <header className="page__header">
        <div className="page__title-section">
          <h1 className="page__title">{t('receiving.title', 'Receiving')}</h1>
          <p className="page__subtitle">{t('receiving.subtitle', 'Process inbound receipts')}</p>
        </div>
        <div className="page__actions">
          <button className="btn btn--primary" onClick={handleAddNew}>
            {t('receiving.startReceiving', 'Start Receiving')}
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
            onChange={(e) => setStatusFilter(e.target.value as GoodsReceiptStatus | '')}
          >
            <option value="">{t('receiving.allStatuses', 'All Statuses')}</option>
            <option value="Draft">Draft</option>
            <option value="Pending">Pending</option>
            <option value="InProgress">In Progress</option>
            <option value="PartiallyReceived">Partially Received</option>
            <option value="Completed">Completed</option>
            <option value="Cancelled">Cancelled</option>
          </select>
        </div>
      </div>

      <div className="page__content">
        <DataTable
          data={receipts}
          columns={columns}
          pagination={pagination}
          onPaginationChange={setPagination}
          sorting={sorting}
          onSortingChange={setSorting}
          totalRows={totalRows}
          onRowClick={handleRowClick}
          selectedRowId={selectedReceipt?.id}
          getRowId={(row) => row.id}
          emptyMessage={t('receiving.noReceipts', 'No receipts found')}
          loading={isLoading}
        />
      </div>

      {/* Goods Receipt Form Modal */}
      <FullscreenModal
        open={isModalOpen}
        onClose={handleCloseModal}
        title={selectedReceipt ? t('receiving.editReceipt', 'Edit Receipt') : t('receiving.createReceipt', 'Create Receipt')}
        subtitle={selectedReceipt ? selectedReceipt.receiptNumber : undefined}
        onSave={handleSubmit(onSubmit)}
        saveLabel={isSaving ? t('common.saving', 'Saving...') : t('common.save', 'Save')}
        saveDisabled={isSaving}
        maxWidth="lg"
      >
        <form>
          <ModalSection title={t('receiving.receiptDetails', 'Receipt Details')}>
            <div className="form-grid">
              <div className="form-field">
                <label className="form-field__label">
                  {t('common.warehouse', 'Warehouse')} <span className="required">*</span>
                </label>
                <select
                  className={`form-field__select ${errors.warehouseId ? 'form-field__select--error' : ''}`}
                  disabled={!!selectedReceipt}
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
                <label className="form-field__label">{t('common.supplier', 'Supplier')}</label>
                <select
                  className="form-field__select"
                  disabled={!!selectedReceipt}
                  {...register('supplierId')}
                >
                  <option value="">{t('common.selectSupplier', 'Select supplier...')}</option>
                  {suppliers.map((supplier) => (
                    <option key={supplier.id} value={supplier.id}>
                      {supplier.code} - {supplier.name}
                    </option>
                  ))}
                </select>
              </div>
            </div>
          </ModalSection>

          <ModalSection title={t('receiving.deliveryInfo', 'Delivery Information')} collapsible defaultExpanded>
            <div className="form-grid">
              <div className="form-field">
                <label className="form-field__label">{t('receiving.carrierName', 'Carrier Name')}</label>
                <input
                  type="text"
                  className="form-field__input"
                  placeholder="UPS, FedEx, DHL..."
                  {...register('carrierName')}
                />
              </div>
              <div className="form-field">
                <label className="form-field__label">{t('receiving.trackingNumber', 'Tracking Number')}</label>
                <input
                  type="text"
                  className="form-field__input"
                  placeholder="1Z999AA10123456784"
                  {...register('trackingNumber')}
                />
              </div>
              <div className="form-field form-field--full">
                <label className="form-field__label">{t('receiving.deliveryNote', 'Delivery Note')}</label>
                <textarea
                  className="form-field__textarea"
                  rows={2}
                  placeholder={t('receiving.deliveryNotePlaceholder', 'Delivery note or reference...')}
                  {...register('deliveryNote')}
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
                  placeholder={t('receiving.notesPlaceholder', 'Internal notes...')}
                  {...register('notes')}
                />
              </div>
            </div>
          </ModalSection>

          {selectedReceipt && selectedReceipt.status === 'Draft' && (
            <ModalSection title={t('common.dangerZone', 'Danger Zone')}>
              <div className="danger-zone">
                <p>{t('receiving.deleteWarning', 'Deleting a receipt will remove all associated lines and data.')}</p>
                <button
                  type="button"
                  className="btn btn--danger"
                  onClick={() => setDeleteConfirmOpen(true)}
                >
                  {t('receiving.deleteReceipt', 'Delete Receipt')}
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
        title={t('receiving.deleteReceipt', 'Delete Receipt')}
      >
        <div className="modal__body">
          <p>
            {t(
              'receiving.deleteConfirmation',
              `Are you sure you want to delete receipt "${selectedReceipt?.receiptNumber}"? This action cannot be undone.`
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

export default Receiving;
