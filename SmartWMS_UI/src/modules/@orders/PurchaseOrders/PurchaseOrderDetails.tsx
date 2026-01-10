import { useState } from 'react';
import { useIntl } from 'react-intl';
import { useNavigate, useParams } from 'react-router-dom';
import { ORDERS } from '@/constants/routes';
import { PurchaseOrderForm, type PurchaseOrderFormData } from './PurchaseOrderForm';
import {
  useGetPurchaseOrderByIdQuery,
  useUpdatePurchaseOrderMutation,
  useDeletePurchaseOrderMutation,
  useUpdatePurchaseOrderStatusMutation,
} from '@/api/modules/orders';
import type { PurchaseOrderStatus } from '@/api/modules/orders/orders.types';
import { Modal } from '@/components';

const STATUS_COLORS: Record<PurchaseOrderStatus, string> = {
  Draft: 'neutral',
  Pending: 'warning',
  Confirmed: 'info',
  PartiallyReceived: 'warning',
  Received: 'success',
  Completed: 'success',
  Cancelled: 'error',
  OnHold: 'warning',
};

export function PurchaseOrderDetails() {
  const { id } = useParams<{ id: string }>();
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();

  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);
  const [statusModalOpen, setStatusModalOpen] = useState(false);
  const [newStatus, setNewStatus] = useState<PurchaseOrderStatus | ''>('');

  const { data: orderResponse, isLoading: isLoadingOrder } = useGetPurchaseOrderByIdQuery(id!, {
    skip: !id,
  });

  const [updateOrder, { isLoading: isUpdating }] = useUpdatePurchaseOrderMutation();
  const [deleteOrder, { isLoading: isDeleting }] = useDeletePurchaseOrderMutation();
  const [updateStatus, { isLoading: isUpdatingStatus }] = useUpdatePurchaseOrderStatusMutation();

  const order = orderResponse?.data;

  const handleBack = () => {
    navigate(ORDERS.PURCHASE_ORDERS);
  };

  const handleSubmit = async (data: PurchaseOrderFormData) => {
    if (!id || !order) return;

    try {
      await updateOrder({
        id,
        body: {
          externalReference: data.externalReference || undefined,
          supplierId: data.supplierId,
          warehouseId: data.warehouseId,
          expectedDate: data.expectedDate || undefined,
          receivingDockId: data.receivingDockId || undefined,
          notes: data.notes || undefined,
          internalNotes: data.internalNotes || undefined,
        },
      }).unwrap();
    } catch (error) {
      console.error('Failed to update purchase order:', error);
    }
  };

  const handleDelete = async () => {
    if (!id) return;

    try {
      await deleteOrder(id).unwrap();
      setDeleteConfirmOpen(false);
      navigate(ORDERS.PURCHASE_ORDERS);
    } catch (error) {
      console.error('Failed to delete purchase order:', error);
    }
  };

  const handleStatusChange = async () => {
    if (!id || !newStatus) return;

    try {
      await updateStatus({
        id,
        body: { status: newStatus },
      }).unwrap();
      setStatusModalOpen(false);
      setNewStatus('');
    } catch (error) {
      console.error('Failed to update status:', error);
    }
  };

  if (isLoadingOrder) {
    return (
      <div className="detail-page">
        <div className="detail-page__loading">
          {t('common.loading', 'Loading...')}
        </div>
      </div>
    );
  }

  if (!order) {
    return (
      <div className="detail-page">
        <div className="detail-page__error">
          {t('orders.orderNotFound', 'Order not found')}
          <button className="btn btn-primary" onClick={handleBack}>
            {t('common.backToList', 'Back to List')}
          </button>
        </div>
      </div>
    );
  }

  const canEdit = ['Draft', 'Pending'].includes(order.status);
  const canDelete = order.status === 'Draft';

  return (
    <div className="detail-page">
      <header className="detail-page__header">
        <div className="detail-page__header-left">
          <button className="btn btn-ghost" onClick={handleBack}>
            <span>&larr;</span>
            {t('common.back', 'Back')}
          </button>
          <div className="detail-page__title-section">
            <h1 className="detail-page__title">
              {t('orders.purchaseOrder', 'Purchase Order')} #{order.orderNumber}
            </h1>
            <div className="detail-page__meta">
              <span className={`status-badge status-badge--${STATUS_COLORS[order.status]}`}>
                {order.status}
              </span>
              <span className="detail-page__subtitle">
                {order.supplierName}
              </span>
            </div>
          </div>
        </div>

        <div className="detail-page__header-actions">
          <button
            className="btn btn-secondary"
            onClick={() => setStatusModalOpen(true)}
          >
            {t('orders.changeStatus', 'Change Status')}
          </button>
          {canDelete && (
            <button
              className="btn btn-danger"
              onClick={() => setDeleteConfirmOpen(true)}
            >
              {t('common.delete', 'Delete')}
            </button>
          )}
        </div>
      </header>

      <div className="detail-page__content">
        {canEdit ? (
          <PurchaseOrderForm
            initialData={order}
            onSubmit={handleSubmit}
            loading={isUpdating}
            isEditMode={true}
          />
        ) : (
          <div className="detail-page__sections">
            <div className="detail-page__notice">
              {t('orders.readonlyNotice', 'This order cannot be edited in its current status.')}
            </div>

            <div className="detail-section">
              <div className="detail-section__header">
                <h3 className="detail-section__title">{t('orders.orderInfo', 'Order Information')}</h3>
              </div>
              <div className="detail-section__content">
                <dl className="info-list">
                  <dt>{t('orders.orderNumber', 'Order Number')}</dt>
                  <dd>{order.orderNumber}</dd>
                  <dt>{t('orders.supplier', 'Supplier')}</dt>
                  <dd>{order.supplierName}</dd>
                  <dt>{t('orders.warehouse', 'Warehouse')}</dt>
                  <dd>{order.warehouseName}</dd>
                  <dt>{t('orders.orderDate', 'Order Date')}</dt>
                  <dd>{new Date(order.orderDate).toLocaleDateString()}</dd>
                  {order.expectedDate && (
                    <>
                      <dt>{t('orders.expectedDate', 'Expected Date')}</dt>
                      <dd>{new Date(order.expectedDate).toLocaleDateString()}</dd>
                    </>
                  )}
                </dl>
              </div>
            </div>

            <div className="detail-section">
              <div className="detail-section__header">
                <h3 className="detail-section__title">{t('orders.progress', 'Progress')}</h3>
              </div>
              <div className="detail-section__content">
                <div className="stats-grid">
                  <div className="stat-item">
                    <span className="stat-item__label">{t('orders.totalLines', 'Lines')}</span>
                    <span className="stat-item__value">{order.totalLines}</span>
                  </div>
                  <div className="stat-item">
                    <span className="stat-item__label">{t('orders.ordered', 'Ordered')}</span>
                    <span className="stat-item__value">{order.totalQuantity}</span>
                  </div>
                  <div className="stat-item">
                    <span className="stat-item__label">{t('orders.received', 'Received')}</span>
                    <span className="stat-item__value">{order.receivedQuantity}</span>
                  </div>
                </div>
              </div>
            </div>

            {order.lines && order.lines.length > 0 && (
              <div className="detail-section">
                <div className="detail-section__header">
                  <h3 className="detail-section__title">{t('orders.orderLines', 'Order Lines')}</h3>
                </div>
                <div className="detail-section__content">
                  <table className="data-table">
                    <thead>
                      <tr>
                        <th>{t('orders.sku', 'SKU')}</th>
                        <th>{t('orders.product', 'Product')}</th>
                        <th>{t('orders.ordered', 'Ordered')}</th>
                        <th>{t('orders.received', 'Received')}</th>
                        <th>{t('orders.outstanding', 'Outstanding')}</th>
                      </tr>
                    </thead>
                    <tbody>
                      {order.lines.map((line) => (
                        <tr key={line.id}>
                          <td>{line.sku}</td>
                          <td>{line.productName}</td>
                          <td>{line.quantityOrdered}</td>
                          <td>{line.quantityReceived}</td>
                          <td>{line.quantityOutstanding}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            )}
          </div>
        )}
      </div>

      {/* Delete Confirmation Modal */}
      <Modal
        open={deleteConfirmOpen}
        onClose={() => setDeleteConfirmOpen(false)}
        title={t('orders.deleteOrder', 'Delete Order')}
      >
        <div className="modal__body">
          <p>
            {t(
              'orders.deleteConfirmation',
              `Are you sure you want to delete order ${order.orderNumber}? This action cannot be undone.`
            )}
          </p>
        </div>
        <div className="modal__actions">
          <button className="btn btn-ghost" onClick={() => setDeleteConfirmOpen(false)}>
            {t('common.cancel', 'Cancel')}
          </button>
          <button className="btn btn-danger" onClick={handleDelete} disabled={isDeleting}>
            {isDeleting ? t('common.deleting', 'Deleting...') : t('common.delete', 'Delete')}
          </button>
        </div>
      </Modal>

      {/* Status Change Modal */}
      <Modal
        open={statusModalOpen}
        onClose={() => setStatusModalOpen(false)}
        title={t('orders.changeStatus', 'Change Status')}
      >
        <div className="modal__body">
          <div className="form-field">
            <label className="form-field__label">{t('orders.newStatus', 'New Status')}</label>
            <select
              className="form-field__select"
              value={newStatus}
              onChange={(e) => setNewStatus(e.target.value as PurchaseOrderStatus)}
            >
              <option value="">{t('orders.selectStatus', 'Select status...')}</option>
              <option value="Pending">Pending</option>
              <option value="Confirmed">Confirmed</option>
              <option value="OnHold">On Hold</option>
              <option value="Cancelled">Cancelled</option>
            </select>
          </div>
        </div>
        <div className="modal__actions">
          <button className="btn btn-ghost" onClick={() => setStatusModalOpen(false)}>
            {t('common.cancel', 'Cancel')}
          </button>
          <button
            className="btn btn-primary"
            onClick={handleStatusChange}
            disabled={!newStatus || isUpdatingStatus}
          >
            {isUpdatingStatus ? t('common.saving', 'Saving...') : t('common.save', 'Save')}
          </button>
        </div>
      </Modal>
    </div>
  );
}
