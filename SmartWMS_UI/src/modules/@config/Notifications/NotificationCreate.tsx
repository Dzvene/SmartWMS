import { useNavigate } from 'react-router-dom';
import { useTranslate } from '@/hooks';
import { CONFIG } from '@/constants/routes';
import { NotificationForm, type NotificationFormData } from './NotificationForm';
import { useCreateNotificationTemplateMutation } from '@/api/modules/notifications';
import type { CreateNotificationTemplateRequest } from '@/api/modules/notifications';

export function NotificationCreate() {
  const t = useTranslate();
  const navigate = useNavigate();

  const [createTemplate, { isLoading: isCreating }] = useCreateNotificationTemplateMutation();

  const handleBack = () => {
    navigate(CONFIG.NOTIFICATIONS);
  };

  const handleSubmit = async (data: NotificationFormData) => {
    try {
      const createData: CreateNotificationTemplateRequest = {
        name: data.name,
        type: data.type,
        subject: data.subject,
        bodyTemplate: data.bodyTemplate,
        channels: data.channels,
        isActive: data.isActive,
      };

      const result = await createTemplate(createData).unwrap();

      // Navigate to the created template or back to list
      if (result.data?.id) {
        navigate(`${CONFIG.NOTIFICATIONS}/${result.data.id}`);
      } else {
        navigate(CONFIG.NOTIFICATIONS);
      }
    } catch (error) {
      console.error('Failed to create notification template:', error);
    }
  };

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
              {t('notifications.createTemplate', 'Create Template')}
            </h1>
            <span className="detail-page__subtitle">
              {t('notifications.createTemplateSubtitle', 'Fill in the details to create a new notification template')}
            </span>
          </div>
        </div>
      </header>

      <div className="detail-page__content">
        <NotificationForm onSubmit={handleSubmit} loading={isCreating} isEditMode={false} />
      </div>
    </div>
  );
}
