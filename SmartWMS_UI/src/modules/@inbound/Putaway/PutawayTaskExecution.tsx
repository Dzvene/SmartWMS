import { useState, useEffect } from 'react';
import { useIntl } from 'react-intl';
import { useNavigate, useParams } from 'react-router-dom';
import {
  useGetPutawayTaskByIdQuery,
  useStartPutawayTaskMutation,
  useCompletePutawayTaskMutation,
  useCancelPutawayTaskMutation,
  useLazySuggestLocationQuery,
} from '@/api/modules/putaway';
import type { PutawayTaskStatus } from '@/api/modules/putaway';
import { Modal } from '@/components';
import { INBOUND } from '@/constants/routes';
import './PutawayTaskExecution.scss';

const STATUS_COLORS: Record<PutawayTaskStatus, string> = {
  Pending: 'warning',
  Assigned: 'info',
  InProgress: 'info',
  Complete: 'success',
  Cancelled: 'error',
};

export function PutawayTaskExecution() {
  const { id } = useParams<{ id: string }>();
  const { formatMessage } = useIntl();
  const t = (key: string, defaultMessage?: string) => formatMessage({ id: key, defaultMessage });
  const navigate = useNavigate();

  const [targetLocationId, setTargetLocationId] = useState('');
  const [targetLocationCode, setTargetLocationCode] = useState('');
  const [quantityPutaway, setQuantityPutaway] = useState<number>(0);
  const [cancelModalOpen, setCancelModalOpen] = useState(false);

  const { data: taskResponse, isLoading } = useGetPutawayTaskByIdQuery(id!, { skip: !id });
  const [startTask, { isLoading: isStarting }] = useStartPutawayTaskMutation();
  const [completeTask, { isLoading: isCompleting }] = useCompletePutawayTaskMutation();
  const [cancelTask, { isLoading: isCancelling }] = useCancelPutawayTaskMutation();
  const [suggestLocation, { data: suggestions, isLoading: isSuggesting }] = useLazySuggestLocationQuery();

  const task = taskResponse?.data;

  // Initialize form values when task loads
  useEffect(() => {
    if (task) {
      setQuantityPutaway(task.quantityToPutaway);
      if (task.suggestedLocationId) {
        setTargetLocationId(task.suggestedLocationId);
        setTargetLocationCode(task.suggestedLocationCode || '');
      }
    }
  }, [task]);

  // Update from suggestions
  useEffect(() => {
    if (suggestions?.data && suggestions.data.length > 0) {
      const best = suggestions.data[0];
      setTargetLocationId(best.locationId);
      setTargetLocationCode(best.locationCode || '');
    }
  }, [suggestions]);

  const handleBack = () => {
    navigate(INBOUND.PUTAWAY);
  };

  const handleStart = async () => {
    if (!id) return;
    try {
      await startTask(id).unwrap();
    } catch (error) {
      console.error('Failed to start putaway task:', error);
    }
  };

  const handleSuggestLocation = async () => {
    if (!task) return;
    try {
      await suggestLocation({
        productId: task.productId,
        quantity: task.quantityToPutaway,
        batchNumber: task.batchNumber,
        expiryDate: task.expiryDate,
      });
    } catch (error) {
      console.error('Failed to get location suggestion:', error);
    }
  };

  const handleComplete = async () => {
    if (!id || !task || !targetLocationId) return;
    try {
      await completeTask({
        id,
        data: {
          actualLocationId: targetLocationId,
          quantityPutaway: quantityPutaway || task.quantityToPutaway,
        },
      }).unwrap();
      navigate(INBOUND.PUTAWAY);
    } catch (error) {
      console.error('Failed to complete putaway task:', error);
    }
  };

  const handleCancel = async () => {
    if (!id) return;
    try {
      await cancelTask(id).unwrap();
      setCancelModalOpen(false);
      navigate(INBOUND.PUTAWAY);
    } catch (error) {
      console.error('Failed to cancel task:', error);
    }
  };

  if (isLoading) {
    return (
      <div className="putaway-task-execution">
        <div className="putaway-task-execution__loading">{t('common.loading', 'Loading...')}</div>
      </div>
    );
  }

  if (!task) {
    return (
      <div className="putaway-task-execution">
        <div className="putaway-task-execution__error">
          {t('putaway.taskNotFound', 'Task not found')}
          <button className="btn btn-primary" onClick={handleBack}>
            {t('common.backToList', 'Back to List')}
          </button>
        </div>
      </div>
    );
  }

  const canStart = task.status === 'Assigned';
  const canComplete = task.status === 'InProgress';
  const canCancel = ['Pending', 'Assigned', 'InProgress'].includes(task.status);
  const isCompleted = task.status === 'Complete';

  return (
    <div className="putaway-task-execution">
      <header className="putaway-task-execution__header">
        <div className="putaway-task-execution__header-left">
          <button className="btn btn-ghost" onClick={handleBack}>
            <span>&larr;</span>
            {t('common.back', 'Back')}
          </button>
          <div className="putaway-task-execution__title-section">
            <h1 className="putaway-task-execution__title">
              {t('putaway.putawayTask', 'Putaway Task')} #{task.taskNumber}
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

      <div className="putaway-task-execution__content">
        {/* Task Info Card */}
        <div className="task-card">
          <div className="task-card__section">
            <h3 className="task-card__title">{t('putaway.productInfo', 'Product Information')}</h3>
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
            <h3 className="task-card__title">{t('putaway.locations', 'Locations')}</h3>
            <div className="task-card__locations">
              <div className="task-card__location task-card__location--from">
                <span className="task-card__location-label">{t('putaway.fromLocation', 'From')}</span>
                <span className="task-card__location-code">{task.fromLocationCode}</span>
              </div>
              <div className="task-card__arrow">→</div>
              <div className="task-card__location task-card__location--to">
                <span className="task-card__location-label">{t('putaway.suggestedLocation', 'Suggested')}</span>
                <span className="task-card__location-code">
                  {task.suggestedLocationCode || '-'}
                </span>
              </div>
            </div>
          </div>

          <div className="task-card__section">
            <h3 className="task-card__title">{t('putaway.quantity', 'Quantity')}</h3>
            <div className="task-card__quantity">
              <div className="task-card__quantity-required">
                <span className="task-card__quantity-label">{t('putaway.toPutaway', 'To Putaway')}</span>
                <span className="task-card__quantity-value">{task.quantityToPutaway}</span>
              </div>
              {task.quantityPutaway > 0 && (
                <div className="task-card__quantity-completed">
                  <span className="task-card__quantity-label">{t('putaway.completed', 'Completed')}</span>
                  <span className="task-card__quantity-value">{task.quantityPutaway}</span>
                </div>
              )}
            </div>
          </div>
        </div>

        {/* Actions */}
        {!isCompleted && (
          <div className="putaway-task-execution__actions">
            {canStart && (
              <button
                className="btn btn-primary btn-lg"
                onClick={handleStart}
                disabled={isStarting}
              >
                {isStarting ? t('common.starting', 'Starting...') : t('putaway.startPutaway', 'Start Putaway')}
              </button>
            )}

            {canComplete && (
              <div className="putaway-task-execution__complete-form">
                <div className="form-row">
                  <div className="form-field">
                    <label className="form-field__label">
                      {t('putaway.targetLocation', 'Target Location')}
                    </label>
                    <div className="form-field__with-button">
                      <input
                        type="text"
                        className="form-field__input form-field__input--lg"
                        value={targetLocationCode}
                        onChange={(e) => setTargetLocationCode(e.target.value)}
                        placeholder={t('putaway.scanOrEnterLocation', 'Scan or enter location code')}
                      />
                      <button
                        className="btn btn-secondary"
                        onClick={handleSuggestLocation}
                        disabled={isSuggesting}
                      >
                        {isSuggesting ? '...' : t('putaway.suggest', 'Suggest')}
                      </button>
                    </div>
                  </div>

                  <div className="form-field">
                    <label className="form-field__label">
                      {t('putaway.quantityPutaway', 'Quantity')}
                    </label>
                    <input
                      type="number"
                      className="form-field__input form-field__input--lg"
                      min={1}
                      max={task.quantityToPutaway}
                      value={quantityPutaway}
                      onChange={(e) => setQuantityPutaway(Number(e.target.value))}
                    />
                  </div>
                </div>

                <button
                  className="btn btn-primary btn-lg"
                  onClick={handleComplete}
                  disabled={isCompleting || !targetLocationId}
                >
                  {isCompleting
                    ? t('common.completing', 'Completing...')
                    : t('putaway.completePutaway', 'Complete Putaway')}
                </button>
              </div>
            )}
          </div>
        )}

        {isCompleted && (
          <div className="putaway-task-execution__completed">
            <div className="completed-message">
              <span className="completed-message__icon">✓</span>
              <span className="completed-message__text">
                {t('putaway.taskCompleted', 'Putaway task completed successfully')}
              </span>
              {task.actualLocationCode && (
                <span className="completed-message__details">
                  {t('putaway.putawayTo', 'Putaway to')}: {task.actualLocationCode}
                </span>
              )}
            </div>
          </div>
        )}
      </div>

      {/* Cancel Modal */}
      <Modal
        open={cancelModalOpen}
        onClose={() => setCancelModalOpen(false)}
        title={t('putaway.cancelTask', 'Cancel Task')}
      >
        <div className="modal-body">
          <p>{t('putaway.cancelConfirmation', 'Are you sure you want to cancel this putaway task?')}</p>
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

export default PutawayTaskExecution;
