import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useTranslate } from '@/hooks';
import { CONFIG } from '@/constants/routes';
import { NotificationForm, type NotificationFormData } from './NotificationForm';
import {
  useGetNotificationTemplateByIdQuery,
  useUpdateNotificationTemplateMutation,
  useDeleteNotificationTemplateMutation,
} from '@/api/modules/notifications';
import type { UpdateNotificationTemplateRequest } from '@/api/modules/notifications';
import { Modal } from '@/components';

const typeLabels: Record<string, string> = {
  Info: 'Info',
  Warning: 'Warning',
  Error: 'Error',
  Success: 'Success',
  Alert: 'Alert',
  Reminder: 'Reminder',
  Task: 'Task',
  System: 'System',
};

export function NotificationDetails() {
  const { id } = useParams<{ id: string }>();
  const t = useTranslate();
  const navigate = useNavigate();

  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);

  // Fetch template data
  const { data: templateResponse, isLoading: isLoadingTemplate } = useGetNotificationTemplateByIdQuery(id!, {
    skip: !id,
  });

  // Mutations
  const [updateTemplate, { isLoading: isUpdating }] = useUpdateNotificationTemplateMutation();
  const [deleteTemplate, { isLoading: isDeleting }] = useDeleteNotificationTemplateMutation();

  const template = templateResponse?.data;

  const handleBack = () => {
    navigate(CONFIG.NOTIFICATIONS);
  };

  const handleSubmit = async (data: NotificationFormData) => {
    if (!id) return;

    try {
      const updateData: UpdateNotificationTemplateRequest = {
        name: data.name,
        subject: data.subject,
        bodyTemplate: data.bodyTemplate,
        channels: data.channels,
        isActive: data.isActive,
      };

      await updateTemplate({ id, data: updateData }).unwrap();
    } catch (error) {
      console.error('Failed to update notification template:', error);
    }
  };

  const handleDelete = async () => {
    if (!id) return;

    try {
      await deleteTemplate(id).unwrap();
      setDeleteConfirmOpen(false);
      navigate(CONFIG.NOTIFICATIONS);
    } catch (error) {
      console.error('Failed to delete notification template:', error);
    }
  };

  if (isLoadingTemplate) {
    return (
      <div className="detail-page">
        <div className="detail-page__loading">
          {t('common.loading', 'Loading...')}
        </div>
      </div>
    );
  }

  if (!template) {
    return (
      <div className="detail-page">
        <div className="detail-page__error">
          {t('notifications.templateNotFound', 'Notification template not found')}
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
              {template.name}
            </h1>
            <div className="detail-page__meta">
              <span className={`status-badge status-badge--${template.isActive ? 'success' : 'neutral'}`}>
                {template.isActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
              </span>
              <span className={`tag tag--${template.type.toLowerCase()}`}>
                {typeLabels[template.type]}
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
        <NotificationForm
          initialData={template}
          onSubmit={handleSubmit}
          loading={isUpdating}
          isEditMode={true}
        />
      </div>

      {/* Delete Confirmation Modal */}
      <Modal
        open={deleteConfirmOpen}
        onClose={() => setDeleteConfirmOpen(false)}
        title={t('notifications.deleteTemplate', 'Delete Template')}
      >
        <div className="modal__body">
          <p>
            {t(
              'notifications.deleteConfirmation',
              `Are you sure you want to delete "${template.name}"? This action cannot be undone.`
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
