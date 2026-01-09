import { useState } from 'react';
import { useIntl } from 'react-intl';
import { useNavigate, useParams } from 'react-router-dom';
import {
  useGetGoodsReceiptByIdQuery,
  useStartReceivingMutation,
  useReceiveLineMutation,
  useCompleteGoodsReceiptMutation,
  useCancelGoodsReceiptMutation,
} from '@/api/modules/receiving';
import type { GoodsReceiptStatus, GoodsReceiptLineDto } from '@/api/modules/receiving';
import { Modal } from '@/components';
import { INBOUND } from '@/constants/routes';
import './ReceivingExecution.scss';

const STATUS_COLORS: Record<GoodsReceiptStatus, string> = {
  Draft: 'neutral',
  Pending: 'warning',
  InProgress: 'info',
  Completed: 'success',
  PartiallyReceived: 'warning',
  Cancelled: 'error',
};

export function ReceivingExecution() {
  const { id } = useParams<{ id: string }>();
  const { formatMessage } = useIntl();
  const t = (key: string, defaultMessage?: string) => formatMessage({ id: key, defaultMessage });
  const navigate = useNavigate();

  const [receiveModalOpen, setReceiveModalOpen] = useState(false);
  const [cancelModalOpen, setCancelModalOpen] = useState(false);
  const [cancelReason, setCancelReason] = useState('');
  const [selectedLine, setSelectedLine] = useState<GoodsReceiptLineDto | null>(null);
  const [receiveQuantity, setReceiveQuantity] = useState(0);
  const [batchNumber, setBatchNumber] = useState('');

  const { data: receiptResponse, isLoading, refetch } = useGetGoodsReceiptByIdQuery(id!, { skip: !id });
  const [startReceiving, { isLoading: isStarting }] = useStartReceivingMutation();
  const [receiveLine, { isLoading: isReceiving }] = useReceiveLineMutation();
  const [completeReceipt, { isLoading: isCompleting }] = useCompleteGoodsReceiptMutation();
  const [cancelReceipt, { isLoading: isCancelling }] = useCancelGoodsReceiptMutation();

  const receipt = receiptResponse?.data;

  const handleBack = () => {
    navigate(INBOUND.RECEIVING);
  };

  const handleStart = async () => {
    if (!id) return;
    try {
      await startReceiving(id).unwrap();
    } catch (error) {
      console.error('Failed to start receiving:', error);
    }
  };

  const handleOpenReceive = (line: GoodsReceiptLineDto) => {
    setSelectedLine(line);
    setReceiveQuantity(line.quantityExpected - line.quantityReceived);
    setBatchNumber('');
    setReceiveModalOpen(true);
  };

  const handleReceiveLine = async () => {
    if (!id || !selectedLine) return;
    try {
      await receiveLine({
        receiptId: id,
        lineId: selectedLine.id,
        body: {
          quantityReceived: receiveQuantity,
          batchNumber: batchNumber || undefined,
        },
      }).unwrap();
      setReceiveModalOpen(false);
      setSelectedLine(null);
      refetch();
    } catch (error) {
      console.error('Failed to receive line:', error);
    }
  };

  const handleComplete = async () => {
    if (!id) return;
    try {
      await completeReceipt(id).unwrap();
      navigate(INBOUND.RECEIVING);
    } catch (error) {
      console.error('Failed to complete receipt:', error);
    }
  };

  const handleCancel = async () => {
    if (!id) return;
    try {
      await cancelReceipt({ id, reason: cancelReason }).unwrap();
      setCancelModalOpen(false);
      navigate(INBOUND.RECEIVING);
    } catch (error) {
      console.error('Failed to cancel receipt:', error);
    }
  };

  if (isLoading) {
    return (
      <div className="receiving-execution">
        <div className="receiving-execution__loading">{t('common.loading', 'Loading...')}</div>
      </div>
    );
  }

  if (!receipt) {
    return (
      <div className="receiving-execution">
        <div className="receiving-execution__error">
          {t('receiving.receiptNotFound', 'Receipt not found')}
          <button className="btn btn-primary" onClick={handleBack}>
            {t('common.backToList', 'Back to List')}
          </button>
        </div>
      </div>
    );
  }

  const canStart = receipt.status === 'Pending';
  const canReceive = receipt.status === 'InProgress' || receipt.status === 'PartiallyReceived';
  const canComplete = canReceive && receipt.lines?.some(line => line.quantityReceived > 0);
  const canCancel = ['Draft', 'Pending', 'InProgress', 'PartiallyReceived'].includes(receipt.status);
  const isCompleted = receipt.status === 'Completed';

  return (
    <div className="receiving-execution">
      <header className="receiving-execution__header">
        <div className="receiving-execution__header-left">
          <button className="btn btn-ghost" onClick={handleBack}>
            <span>&larr;</span>
            {t('common.back', 'Back')}
          </button>
          <div className="receiving-execution__title-section">
            <h1 className="receiving-execution__title">
              {t('receiving.goodsReceipt', 'Goods Receipt')} #{receipt.receiptNumber}
            </h1>
            <span className={`status-badge status-badge--${STATUS_COLORS[receipt.status]}`}>
              {receipt.status}
            </span>
          </div>
        </div>
        <div className="receiving-execution__header-actions">
          {canCancel && (
            <button className="btn btn-danger" onClick={() => setCancelModalOpen(true)}>
              {t('common.cancel', 'Cancel')}
            </button>
          )}
        </div>
      </header>

      <div className="receiving-execution__content">
        {/* Receipt Info */}
        <div className="task-card">
          <div className="task-card__section">
            <h3 className="task-card__title">{t('receiving.receiptInfo', 'Receipt Information')}</h3>
            <div className="task-card__details">
              <div className="task-card__row">
                <span className="task-card__label">{t('receiving.poNumber', 'PO #')}</span>
                <span className="task-card__value task-card__value--code">{receipt.purchaseOrderNumber}</span>
              </div>
              <div className="task-card__row">
                <span className="task-card__label">{t('common.supplier', 'Supplier')}</span>
                <span className="task-card__value">{receipt.supplierName}</span>
              </div>
              <div className="task-card__row">
                <span className="task-card__label">{t('common.warehouse', 'Warehouse')}</span>
                <span className="task-card__value">{receipt.warehouseName}</span>
              </div>
              <div className="task-card__row">
                <span className="task-card__label">{t('receiving.receiptDate', 'Receipt Date')}</span>
                <span className="task-card__value">
                  {new Date(receipt.receiptDate).toLocaleDateString()}
                </span>
              </div>
            </div>
          </div>

          <div className="task-card__section">
            <h3 className="task-card__title">{t('receiving.progress', 'Progress')}</h3>
            <div className="task-card__progress">
              <div className="progress-bar">
                <div
                  className="progress-bar__fill"
                  style={{ width: `${receipt.progressPercent}%` }}
                />
              </div>
              <span className="task-card__progress-text">
                {receipt.totalQuantityReceived} / {receipt.totalQuantityExpected} ({receipt.progressPercent}%)
              </span>
            </div>
          </div>
        </div>

        {/* Lines to Receive */}
        {(canReceive || canStart) && receipt.lines && receipt.lines.length > 0 && (
          <div className="receiving-section">
            <div className="receiving-section__header">
              <h3>{t('receiving.linesToReceive', 'Lines to Receive')}</h3>
            </div>
            <div className="receiving-lines">
              <table className="receiving-table">
                <thead>
                  <tr>
                    <th>{t('products.sku', 'SKU')}</th>
                    <th>{t('common.product', 'Product')}</th>
                    <th>{t('receiving.expected', 'Expected')}</th>
                    <th>{t('receiving.received', 'Received')}</th>
                    <th>{t('receiving.remaining', 'Remaining')}</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  {receipt.lines.map((line) => {
                    const remaining = line.quantityExpected - line.quantityReceived;
                    const isFullyReceived = remaining === 0;

                    return (
                      <tr key={line.id} className={isFullyReceived ? 'receiving-table__row--complete' : ''}>
                        <td className="receiving-table__sku">{line.sku}</td>
                        <td>{line.productName}</td>
                        <td>{line.quantityExpected}</td>
                        <td className="receiving-table__received">{line.quantityReceived}</td>
                        <td className={remaining > 0 ? 'receiving-table__remaining' : ''}>
                          {remaining}
                        </td>
                        <td>
                          {canReceive && remaining > 0 && (
                            <button
                              className="btn btn-sm btn-primary"
                              onClick={() => handleOpenReceive(line)}
                            >
                              {t('receiving.receive', 'Receive')}
                            </button>
                          )}
                          {isFullyReceived && (
                            <span className="receiving-table__done">✓</span>
                          )}
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          </div>
        )}

        {/* Actions */}
        <div className="receiving-execution__actions">
          {canStart && (
            <button
              className="btn btn-primary btn-lg"
              onClick={handleStart}
              disabled={isStarting}
            >
              {isStarting ? t('common.starting', 'Starting...') : t('receiving.startReceiving', 'Start Receiving')}
            </button>
          )}

          {canComplete && (
            <button
              className="btn btn-success btn-lg"
              onClick={handleComplete}
              disabled={isCompleting}
            >
              {isCompleting
                ? t('common.completing', 'Completing...')
                : t('receiving.completeReceipt', 'Complete Receipt')}
            </button>
          )}
        </div>

        {isCompleted && (
          <div className="receiving-execution__completed">
            <div className="completed-message">
              <span className="completed-message__icon">✓</span>
              <span className="completed-message__text">
                {t('receiving.receiptCompleted', 'Goods receipt completed successfully')}
              </span>
              <span className="completed-message__details">
                {receipt.totalQuantityReceived} {t('receiving.unitsReceived', 'units received')}
              </span>
            </div>
          </div>
        )}
      </div>

      {/* Receive Line Modal */}
      <Modal
        open={receiveModalOpen}
        onClose={() => setReceiveModalOpen(false)}
        title={t('receiving.receiveLine', 'Receive Line')}
      >
        <div className="modal-body">
          {selectedLine && (
            <>
              <div className="receive-line-summary">
                <span className="receive-line-summary__sku">{selectedLine.sku}</span>
                <span className="receive-line-summary__name">{selectedLine.productName}</span>
                <span className="receive-line-summary__remaining">
                  {t('receiving.remainingToReceive', 'Remaining')}: {selectedLine.quantityExpected - selectedLine.quantityReceived}
                </span>
              </div>

              <div className="form-field">
                <label className="form-field__label">{t('receiving.quantityToReceive', 'Quantity to Receive')}</label>
                <input
                  type="number"
                  className="form-field__input form-field__input--lg"
                  min={1}
                  max={selectedLine.quantityExpected - selectedLine.quantityReceived}
                  value={receiveQuantity}
                  onChange={(e) => setReceiveQuantity(Number(e.target.value))}
                />
              </div>

              <div className="form-field">
                <label className="form-field__label">
                  {t('products.batchNumber', 'Batch Number')}
                  <span className="form-field__optional">{t('common.optional', '(optional)')}</span>
                </label>
                <input
                  type="text"
                  className="form-field__input"
                  value={batchNumber}
                  onChange={(e) => setBatchNumber(e.target.value)}
                  placeholder={t('receiving.enterBatch', 'Enter batch number...')}
                />
              </div>
            </>
          )}
        </div>
        <div className="modal-actions">
          <button className="btn btn-ghost" onClick={() => setReceiveModalOpen(false)}>
            {t('common.cancel', 'Cancel')}
          </button>
          <button
            className="btn btn-primary"
            onClick={handleReceiveLine}
            disabled={isReceiving || receiveQuantity <= 0}
          >
            {isReceiving ? t('common.saving', 'Saving...') : t('receiving.confirmReceive', 'Confirm Receive')}
          </button>
        </div>
      </Modal>

      {/* Cancel Modal */}
      <Modal
        open={cancelModalOpen}
        onClose={() => setCancelModalOpen(false)}
        title={t('receiving.cancelReceipt', 'Cancel Receipt')}
      >
        <div className="modal-body">
          <p>{t('receiving.cancelConfirmation', 'Are you sure you want to cancel this goods receipt?')}</p>
          <div className="form-field">
            <label className="form-field__label">{t('common.reason', 'Reason')}</label>
            <textarea
              className="form-field__textarea"
              rows={3}
              value={cancelReason}
              onChange={(e) => setCancelReason(e.target.value)}
              placeholder={t('receiving.cancelReasonPlaceholder', 'Enter cancellation reason...')}
            />
          </div>
        </div>
        <div className="modal-actions">
          <button className="btn btn-ghost" onClick={() => setCancelModalOpen(false)}>
            {t('common.back', 'Back')}
          </button>
          <button className="btn btn-danger" onClick={handleCancel} disabled={isCancelling}>
            {isCancelling ? t('common.cancelling', 'Cancelling...') : t('common.confirmCancel', 'Confirm Cancel')}
          </button>
        </div>
      </Modal>
    </div>
  );
}

export default ReceivingExecution;
