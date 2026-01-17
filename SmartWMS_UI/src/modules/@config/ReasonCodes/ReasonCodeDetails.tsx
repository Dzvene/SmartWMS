import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useTranslate } from '@/hooks';
import { CONFIG } from '@/constants/routes';
import { ReasonCodeForm, type ReasonCodeFormData } from './ReasonCodeForm';
import {
  useGetReasonCodeByIdQuery,
  useUpdateReasonCodeMutation,
  useDeleteReasonCodeMutation,
} from '@/api/modules/configuration';
import type { UpdateReasonCodeRequest } from '@/api/modules/configuration';
import { Modal } from '@/components';

const categoryLabels: Record<string, string> = {
  Adjustment: 'Adjustment',
  Return: 'Return',
  Damage: 'Damage',
  Expiry: 'Expiry',
  QualityHold: 'Quality Hold',
  Transfer: 'Transfer',
  Scrap: 'Scrap',
  Found: 'Found',
  Lost: 'Lost',
  Other: 'Other',
};

export function ReasonCodeDetails() {
  const { id } = useParams<{ id: string }>();
  const t = useTranslate();
  const navigate = useNavigate();

  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);

  // Fetch reason code data
  const { data: reasonCodeResponse, isLoading: isLoadingReasonCode } = useGetReasonCodeByIdQuery(id!, {
    skip: !id,
  });

  // Mutations
  const [updateReasonCode, { isLoading: isUpdating }] = useUpdateReasonCodeMutation();
  const [deleteReasonCode, { isLoading: isDeleting }] = useDeleteReasonCodeMutation();

  const reasonCode = reasonCodeResponse?.data;

  const handleBack = () => {
    navigate(CONFIG.REASON_CODES);
  };

  const handleSubmit = async (data: ReasonCodeFormData) => {
    if (!id) return;

    try {
      const updateData: UpdateReasonCodeRequest = {
        code: data.code,
        name: data.name,
        description: data.description || undefined,
        requiresNotes: data.requiresNotes,
        sortOrder: data.sortOrder,
        isActive: data.isActive,
      };

      await updateReasonCode({ id, data: updateData }).unwrap();
    } catch (error) {
      console.error('Failed to update reason code:', error);
    }
  };

  const handleDelete = async () => {
    if (!id) return;

    try {
      await deleteReasonCode(id).unwrap();
      setDeleteConfirmOpen(false);
      navigate(CONFIG.REASON_CODES);
    } catch (error) {
      console.error('Failed to delete reason code:', error);
    }
  };

  if (isLoadingReasonCode) {
    return (
      <div className="detail-page">
        <div className="detail-page__loading">
          {t('common.loading', 'Loading...')}
        </div>
      </div>
    );
  }

  if (!reasonCode) {
    return (
      <div className="detail-page">
        <div className="detail-page__error">
          {t('reasonCodes.reasonCodeNotFound', 'Reason code not found')}
          <button className="btn btn-primary" onClick={handleBack}>
            {t('common.backToList', 'Back to List')}
          </button>
        </div>
      </div>
    );
  }

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
              {reasonCode.name}
            </h1>
            <div className="detail-page__meta">
              <span className={`status-badge status-badge--${reasonCode.isActive ? 'success' : 'neutral'}`}>
                {reasonCode.isActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
              </span>
              <span className="detail-page__subtitle">
                {reasonCode.code}
              </span>
              <span className={`tag tag--${reasonCode.reasonType.toLowerCase()}`}>
                {categoryLabels[reasonCode.reasonType] || reasonCode.reasonType}
              </span>
            </div>
          </div>
        </div>

        <div className="detail-page__header-actions">
          <button
            className="btn btn-danger"
            onClick={() => setDeleteConfirmOpen(true)}
          >
            {t('common.delete', 'Delete')}
          </button>
        </div>
      </header>

      <div className="detail-page__content">
        <ReasonCodeForm
          initialData={reasonCode}
          onSubmit={handleSubmit}
          loading={isUpdating}
          isEditMode={true}
        />
      </div>

      {/* Delete Confirmation Modal */}
      <Modal
        open={deleteConfirmOpen}
        onClose={() => setDeleteConfirmOpen(false)}
        title={t('reasonCodes.deleteReasonCode', 'Delete Reason Code')}
      >
        <div className="modal__body">
          <p>
            {t(
              'reasonCodes.deleteConfirmation',
              `Are you sure you want to delete "${reasonCode.name}"? This action cannot be undone.`
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
    </div>
  );
}
