import { useState } from 'react';
import { useIntl } from 'react-intl';
import { useNavigate, useParams } from 'react-router-dom';
import {
  useGetPickTaskByIdQuery,
  useStartPickTaskMutation,
  useConfirmPickMutation,
  useShortPickMutation,
  useCancelPickTaskMutation,
} from '@/api/modules/fulfillment';
import type { PickTaskStatus } from '@/api/modules/fulfillment';
import { Modal } from '@/components';
import { OUTBOUND } from '@/constants/routes';
import './PickTaskExecution.scss';

const STATUS_COLORS: Record<PickTaskStatus, string> = {
  Pending: 'warning',
  Assigned: 'info',
  InProgress: 'info',
  Completed: 'success',
  ShortPicked: 'warning',
  Cancelled: 'error',
};

export function PickTaskExecution() {
  const { id } = useParams<{ id: string }>();
  const { formatMessage } = useIntl();
  const t = (key: string, defaultMessage?: string) => formatMessage({ id: key, defaultMessage });
  const navigate = useNavigate();

  const [quantityPicked, setQuantityPicked] = useState<number>(0);
  const [shortPickReason, setShortPickReason] = useState('');
  const [shortPickModalOpen, setShortPickModalOpen] = useState(false);
  const [cancelModalOpen, setCancelModalOpen] = useState(false);
  const [cancelReason, setCancelReason] = useState('');

  const { data: taskResponse, isLoading } = useGetPickTaskByIdQuery(id!, { skip: !id });
  const [startTask, { isLoading: isStarting }] = useStartPickTaskMutation();
  const [confirmPick, { isLoading: isConfirming }] = useConfirmPickMutation();
  const [shortPick, { isLoading: isShortPicking }] = useShortPickMutation();
  const [cancelTask, { isLoading: isCancelling }] = useCancelPickTaskMutation();

  const task = taskResponse?.data;

  const handleBack = () => {
    navigate(OUTBOUND.PICKING);
  };

  const handleStart = async () => {
    if (!id) return;
    try {
      await startTask(id).unwrap();
    } catch (error) {
      console.error('Failed to start pick task:', error);
    }
  };

  const handleConfirmPick = async () => {
    if (!id || !task) return;
    try {
      await confirmPick({
        id,
        body: {
          quantityPicked: quantityPicked || task.quantityRequired,
          toLocationId: task.toLocationId,
        },
      }).unwrap();
      navigate(OUTBOUND.PICKING);
    } catch (error) {
      console.error('Failed to confirm pick:', error);
    }
  };

  const handleShortPick = async () => {
    if (!id || !task) return;
    try {
      const shortQuantity = task.quantityRequired - quantityPicked;
      await shortPick({
        id,
        body: {
          quantityPicked,
          quantityShortPicked: shortQuantity,
          reason: shortPickReason,
        },
      }).unwrap();
      setShortPickModalOpen(false);
      navigate(OUTBOUND.PICKING);
    } catch (error) {
      console.error('Failed to short pick:', error);
    }
  };

  const handleCancel = async () => {
    if (!id) return;
    try {
      await cancelTask({ id, reason: cancelReason }).unwrap();
      setCancelModalOpen(false);
      navigate(OUTBOUND.PICKING);
    } catch (error) {
      console.error('Failed to cancel task:', error);
    }
  };

  if (isLoading) {
    return (
      <div className="pick-task-execution">
        <div className="pick-task-execution__loading">{t('common.loading', 'Loading...')}</div>
      </div>
    );
  }

  if (!task) {
    return (
      <div className="pick-task-execution">
        <div className="pick-task-execution__error">
          {t('picking.taskNotFound', 'Task not found')}
          <button className="btn btn-primary" onClick={handleBack}>
            {t('common.backToList', 'Back to List')}
          </button>
        </div>
      </div>
    );
  }

  const canStart = task.status === 'Assigned';
  const canPick = task.status === 'InProgress';
  const canCancel = ['Pending', 'Assigned', 'InProgress'].includes(task.status);
  const isCompleted = task.status === 'Completed' || task.status === 'ShortPicked';

  return (
    <div className="pick-task-execution">
      <header className="pick-task-execution__header">
        <div className="pick-task-execution__header-left">
          <button className="btn btn-ghost" onClick={handleBack}>
            <span>&larr;</span>
            {t('common.back', 'Back')}
          </button>
          <div className="pick-task-execution__title-section">
            <h1 className="pick-task-execution__title">
              {t('picking.pickTask', 'Pick Task')} #{task.taskNumber}
            </h1>
            <span className={`status-badge status-badge--${STATUS_COLORS[task.status]}`}>
              {task.status}
            </span>
          </div>
        </div>
        {canCancel && (
          <button className="btn btn-danger" onClick={() => setCancelModalOpen(true)}>
            {t('common.cancel', 'Cancel')}
          </button>
        )}
      </header>

      <div className="pick-task-execution__content">
        {/* Task Info Card */}
        <div className="task-card">
          <div className="task-card__section">
            <h3 className="task-card__title">{t('picking.productInfo', 'Product Information')}</h3>
            <div className="task-card__details">
              <div className="task-card__row">
                <span className="task-card__label">{t('products.sku', 'SKU')}</span>
                <span className="task-card__value task-card__value--code">{task.sku}</span>
              </div>
              <div className="task-card__row">
                <span className="task-card__label">{t('common.product', 'Product')}</span>
                <span className="task-card__value">{task.productName}</span>
              </div>
              {task.batchNumber && (
                <div className="task-card__row">
                  <span className="task-card__label">{t('products.batch', 'Batch')}</span>
                  <span className="task-card__value">{task.batchNumber}</span>
                </div>
              )}
            </div>
          </div>

          <div className="task-card__section task-card__section--locations">
            <h3 className="task-card__title">{t('picking.locations', 'Locations')}</h3>
            <div className="task-card__locations">
              <div className="task-card__location task-card__location--from">
                <span className="task-card__location-label">{t('picking.fromLocation', 'Pick From')}</span>
                <span className="task-card__location-code">{task.fromLocationCode}</span>
              </div>
              <div className="task-card__arrow">→</div>
              <div className="task-card__location task-card__location--to">
                <span className="task-card__location-label">{t('picking.toLocation', 'Move To')}</span>
                <span className="task-card__location-code">{task.toLocationCode || 'Staging'}</span>
              </div>
            </div>
          </div>

          <div className="task-card__section">
            <h3 className="task-card__title">{t('picking.quantity', 'Quantity')}</h3>
            <div className="task-card__quantity">
              <div className="task-card__quantity-required">
                <span className="task-card__quantity-label">{t('picking.required', 'Required')}</span>
                <span className="task-card__quantity-value">{task.quantityRequired}</span>
              </div>
              {task.quantityPicked > 0 && (
                <div className="task-card__quantity-picked">
                  <span className="task-card__quantity-label">{t('picking.picked', 'Picked')}</span>
                  <span className="task-card__quantity-value">{task.quantityPicked}</span>
                </div>
              )}
            </div>
          </div>

          {task.assignedToUserName && (
            <div className="task-card__section">
              <h3 className="task-card__title">{t('picking.assignedTo', 'Assigned To')}</h3>
              <span className="task-card__value">{task.assignedToUserName}</span>
            </div>
          )}
        </div>

        {/* Actions */}
        {!isCompleted && (
          <div className="pick-task-execution__actions">
            {canStart && (
              <button
                className="btn btn-primary btn-lg"
                onClick={handleStart}
                disabled={isStarting}
              >
                {isStarting ? t('common.starting', 'Starting...') : t('picking.startPicking', 'Start Picking')}
              </button>
            )}

            {canPick && (
              <div className="pick-task-execution__pick-form">
                <div className="form-field">
                  <label className="form-field__label">
                    {t('picking.quantityPicked', 'Quantity Picked')}
                  </label>
                  <input
                    type="number"
                    className="form-field__input form-field__input--lg"
                    min={0}
                    max={task.quantityRequired}
                    value={quantityPicked || task.quantityRequired}
                    onChange={(e) => setQuantityPicked(Number(e.target.value))}
                  />
                </div>

                <div className="pick-task-execution__pick-actions">
                  <button
                    className="btn btn-primary btn-lg"
                    onClick={handleConfirmPick}
                    disabled={isConfirming}
                  >
                    {isConfirming
                      ? t('common.confirming', 'Confirming...')
                      : t('picking.confirmPick', 'Confirm Pick')}
                  </button>

                  <button
                    className="btn btn-warning"
                    onClick={() => setShortPickModalOpen(true)}
                  >
                    {t('picking.shortPick', 'Short Pick')}
                  </button>
                </div>
              </div>
            )}
          </div>
        )}

        {isCompleted && (
          <div className="pick-task-execution__completed">
            <div className="completed-message">
              <span className="completed-message__icon">✓</span>
              <span className="completed-message__text">
                {task.status === 'Completed'
                  ? t('picking.taskCompleted', 'Task completed successfully')
                  : t('picking.taskShortPicked', 'Task completed with short pick')}
              </span>
            </div>
          </div>
        )}
      </div>

      {/* Short Pick Modal */}
      <Modal
        open={shortPickModalOpen}
        onClose={() => setShortPickModalOpen(false)}
        title={t('picking.shortPick', 'Short Pick')}
      >
        <div className="modal-body">
          <p>{t('picking.shortPickMessage', 'Please enter the actual quantity picked and a reason for the short pick.')}</p>
          <div className="form-field">
            <label className="form-field__label">{t('picking.actualQuantity', 'Actual Quantity')}</label>
            <input
              type="number"
              className="form-field__input"
              min={0}
              max={task.quantityRequired - 1}
              value={quantityPicked}
              onChange={(e) => setQuantityPicked(Number(e.target.value))}
            />
          </div>
          <div className="form-field">
            <label className="form-field__label">{t('common.reason', 'Reason')}</label>
            <textarea
              className="form-field__textarea"
              rows={3}
              value={shortPickReason}
              onChange={(e) => setShortPickReason(e.target.value)}
              placeholder={t('picking.shortPickReasonPlaceholder', 'Enter reason for short pick...')}
            />
          </div>
        </div>
        <div className="modal-actions">
          <button className="btn btn-ghost" onClick={() => setShortPickModalOpen(false)}>
            {t('common.cancel', 'Cancel')}
          </button>
          <button
            className="btn btn-warning"
            onClick={handleShortPick}
            disabled={isShortPicking || !shortPickReason}
          >
            {isShortPicking ? t('common.saving', 'Saving...') : t('picking.confirmShortPick', 'Confirm Short Pick')}
          </button>
        </div>
      </Modal>

      {/* Cancel Modal */}
      <Modal
        open={cancelModalOpen}
        onClose={() => setCancelModalOpen(false)}
        title={t('picking.cancelTask', 'Cancel Task')}
      >
        <div className="modal-body">
          <p>{t('picking.cancelConfirmation', 'Are you sure you want to cancel this pick task?')}</p>
          <div className="form-field">
            <label className="form-field__label">{t('common.reason', 'Reason')}</label>
            <textarea
              className="form-field__textarea"
              rows={3}
              value={cancelReason}
              onChange={(e) => setCancelReason(e.target.value)}
              placeholder={t('picking.cancelReasonPlaceholder', 'Enter cancellation reason...')}
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

export default PickTaskExecution;
