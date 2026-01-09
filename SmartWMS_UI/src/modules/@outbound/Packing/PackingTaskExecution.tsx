import { useState } from 'react';
import { useIntl } from 'react-intl';
import { useNavigate, useParams } from 'react-router-dom';
import {
  useGetPackingTaskByIdQuery,
  useStartPackingTaskMutation,
  useCompletePackingTaskMutation,
  useCancelPackingTaskMutation,
  useCreatePackageMutation,
} from '@/api/modules/packing';
import type { PackingTaskStatus, PackageDto } from '@/api/modules/packing';
import { Modal } from '@/components';
import { OUTBOUND } from '@/constants/routes';
import './PackingTaskExecution.scss';

const STATUS_COLORS: Record<PackingTaskStatus, string> = {
  Pending: 'warning',
  Assigned: 'info',
  InProgress: 'info',
  Completed: 'success',
  Cancelled: 'error',
};

export function PackingTaskExecution() {
  const { id } = useParams<{ id: string }>();
  const { formatMessage } = useIntl();
  const t = (key: string, defaultMessage?: string) => formatMessage({ id: key, defaultMessage });
  const navigate = useNavigate();

  const [createBoxModalOpen, setCreateBoxModalOpen] = useState(false);
  const [cancelModalOpen, setCancelModalOpen] = useState(false);
  const [cancelReason, setCancelReason] = useState('');
  const [newBoxWeight, setNewBoxWeight] = useState(1);
  const [newBoxType, setNewBoxType] = useState('Standard');

  const { data: taskResponse, isLoading, refetch } = useGetPackingTaskByIdQuery(id!, { skip: !id });
  const [startTask, { isLoading: isStarting }] = useStartPackingTaskMutation();
  const [completeTask, { isLoading: isCompleting }] = useCompletePackingTaskMutation();
  const [cancelTask, { isLoading: isCancelling }] = useCancelPackingTaskMutation();
  const [createPackage, { isLoading: isCreatingPackage }] = useCreatePackageMutation();

  const task = taskResponse?.data;

  const handleBack = () => {
    navigate(OUTBOUND.PACKING);
  };

  const handleStart = async () => {
    if (!id) return;
    try {
      await startTask(id).unwrap();
    } catch (error) {
      console.error('Failed to start packing task:', error);
    }
  };

  const handleCreateBox = async () => {
    if (!id) return;
    try {
      await createPackage({
        taskId: id,
        data: {
          weightKg: newBoxWeight,
          packagingType: newBoxType,
        },
      }).unwrap();
      setCreateBoxModalOpen(false);
      setNewBoxWeight(1);
      setNewBoxType('Standard');
      refetch();
    } catch (error) {
      console.error('Failed to create package:', error);
    }
  };

  const handleComplete = async () => {
    if (!id) return;
    try {
      await completeTask({ id }).unwrap();
      navigate(OUTBOUND.PACKING);
    } catch (error) {
      console.error('Failed to complete packing task:', error);
    }
  };

  const handleCancel = async () => {
    if (!id) return;
    try {
      await cancelTask(id).unwrap();
      setCancelModalOpen(false);
      navigate(OUTBOUND.PACKING);
    } catch (error) {
      console.error('Failed to cancel task:', error);
    }
  };

  if (isLoading) {
    return (
      <div className="packing-task-execution">
        <div className="packing-task-execution__loading">{t('common.loading', 'Loading...')}</div>
      </div>
    );
  }

  if (!task) {
    return (
      <div className="packing-task-execution">
        <div className="packing-task-execution__error">
          {t('packing.taskNotFound', 'Task not found')}
          <button className="btn btn-primary" onClick={handleBack}>
            {t('common.backToList', 'Back to List')}
          </button>
        </div>
      </div>
    );
  }

  const canStart = task.status === 'Assigned';
  const canPack = task.status === 'InProgress';
  const canCancel = ['Pending', 'Assigned', 'InProgress'].includes(task.status);
  const isCompleted = task.status === 'Completed';

  return (
    <div className="packing-task-execution">
      <header className="packing-task-execution__header">
        <div className="packing-task-execution__header-left">
          <button className="btn btn-ghost" onClick={handleBack}>
            <span>&larr;</span>
            {t('common.back', 'Back')}
          </button>
          <div className="packing-task-execution__title-section">
            <h1 className="packing-task-execution__title">
              {t('packing.packingTask', 'Packing Task')} #{task.taskNumber}
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

      <div className="packing-task-execution__content">
        {/* Order Info */}
        <div className="task-card">
          <div className="task-card__section">
            <h3 className="task-card__title">{t('packing.orderInfo', 'Order Information')}</h3>
            <div className="task-card__details">
              <div className="task-card__row">
                <span className="task-card__label">{t('orders.orderNumber', 'Order #')}</span>
                <span className="task-card__value task-card__value--code">{task.salesOrderNumber}</span>
              </div>
              <div className="task-card__row">
                <span className="task-card__label">{t('common.customer', 'Customer')}</span>
                <span className="task-card__value">{task.customerName}</span>
              </div>
              {task.packingStationCode && (
                <div className="task-card__row">
                  <span className="task-card__label">{t('packing.station', 'Station')}</span>
                  <span className="task-card__value">{task.packingStationCode}</span>
                </div>
              )}
            </div>
          </div>

          <div className="task-card__section">
            <h3 className="task-card__title">{t('packing.progress', 'Progress')}</h3>
            <div className="task-card__progress">
              <div className="progress-bar">
                <div
                  className="progress-bar__fill"
                  style={{ width: `${(task.packedItems / task.totalItems) * 100}%` }}
                />
              </div>
              <span className="task-card__progress-text">
                {task.packedItems} / {task.totalItems} {t('packing.itemsPacked', 'items packed')}
              </span>
            </div>
          </div>
        </div>

        {/* Packages/Boxes */}
        {canPack && (
          <div className="packing-section">
            <div className="packing-section__header">
              <h3>{t('packing.boxes', 'Packages')} ({task.packages?.length || 0})</h3>
              <button className="btn btn-primary btn-sm" onClick={() => setCreateBoxModalOpen(true)}>
                + {t('packing.createBox', 'Create Package')}
              </button>
            </div>
            {task.packages && task.packages.length > 0 ? (
              <div className="packages-list">
                {task.packages.map((pkg: PackageDto) => (
                  <div key={pkg.id} className="package-card">
                    <div className="package-card__header">
                      <span className="package-card__number">Package #{pkg.sequenceNumber}</span>
                      <span className="package-card__type">{pkg.packagingType || 'Standard'}</span>
                    </div>
                    <div className="package-card__details">
                      <span>Weight: {pkg.weightKg} kg</span>
                      {pkg.trackingNumber && (
                        <span>Tracking: {pkg.trackingNumber}</span>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <div className="packages-list__empty">
                {t('packing.noBoxes', 'No packages created yet. Create a package to start packing.')}
              </div>
            )}
          </div>
        )}

        {/* Actions */}
        <div className="packing-task-execution__actions">
          {canStart && (
            <button
              className="btn btn-primary btn-lg"
              onClick={handleStart}
              disabled={isStarting}
            >
              {isStarting ? t('common.starting', 'Starting...') : t('packing.startPacking', 'Start Packing')}
            </button>
          )}

          {canPack && task.packages && task.packages.length > 0 && (
            <button
              className="btn btn-success btn-lg"
              onClick={handleComplete}
              disabled={isCompleting}
            >
              {isCompleting
                ? t('common.completing', 'Completing...')
                : t('packing.completePacking', 'Complete Packing')}
            </button>
          )}
        </div>

        {isCompleted && (
          <div className="packing-task-execution__completed">
            <div className="completed-message">
              <span className="completed-message__icon">✓</span>
              <span className="completed-message__text">
                {t('packing.taskCompleted', 'Packing task completed successfully')}
              </span>
              <span className="completed-message__details">
                {task.boxCount} {t('packing.packagesCreated', 'packages created')}
              </span>
            </div>
          </div>
        )}
      </div>

      {/* Create Box Modal */}
      <Modal
        open={createBoxModalOpen}
        onClose={() => setCreateBoxModalOpen(false)}
        title={t('packing.createBox', 'Create Package')}
      >
        <div className="modal-body">
          <div className="form-field">
            <label className="form-field__label">{t('packing.packagingType', 'Packaging Type')}</label>
            <select
              className="form-field__select"
              value={newBoxType}
              onChange={(e) => setNewBoxType(e.target.value)}
            >
              <option value="Standard">Standard Box</option>
              <option value="Small">Small Box</option>
              <option value="Medium">Medium Box</option>
              <option value="Large">Large Box</option>
              <option value="Envelope">Envelope</option>
            </select>
          </div>
          <div className="form-field">
            <label className="form-field__label">{t('packing.weight', 'Weight (kg)')}</label>
            <input
              type="number"
              className="form-field__input"
              min={0.1}
              step={0.1}
              value={newBoxWeight}
              onChange={(e) => setNewBoxWeight(Number(e.target.value))}
            />
          </div>
        </div>
        <div className="modal-actions">
          <button className="btn btn-ghost" onClick={() => setCreateBoxModalOpen(false)}>
            {t('common.cancel', 'Cancel')}
          </button>
          <button
            className="btn btn-primary"
            onClick={handleCreateBox}
            disabled={isCreatingPackage}
          >
            {isCreatingPackage ? t('common.creating', 'Creating...') : t('common.create', 'Create')}
          </button>
        </div>
      </Modal>

      {/* Cancel Modal */}
      <Modal
        open={cancelModalOpen}
        onClose={() => setCancelModalOpen(false)}
        title={t('packing.cancelTask', 'Cancel Task')}
      >
        <div className="modal-body">
          <p>{t('packing.cancelConfirmation', 'Are you sure you want to cancel this packing task?')}</p>
          <div className="form-field">
            <label className="form-field__label">{t('common.reason', 'Reason')}</label>
            <textarea
              className="form-field__textarea"
              rows={3}
              value={cancelReason}
              onChange={(e) => setCancelReason(e.target.value)}
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

export default PackingTaskExecution;
