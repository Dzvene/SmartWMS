import { useEffect } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { useTranslate } from '@/hooks';
import type {
  NotificationTemplateDto,
  NotificationType,
  NotificationChannel,
} from '@/api/modules/notifications';

const typeLabels: Record<NotificationType, string> = {
  Info: 'Info',
  Warning: 'Warning',
  Error: 'Error',
  Success: 'Success',
  Alert: 'Alert',
  Reminder: 'Reminder',
  Task: 'Task',
  System: 'System',
};

const NOTIFICATION_TYPES: NotificationType[] = [
  'Info', 'Warning', 'Error', 'Success', 'Alert', 'Reminder', 'Task', 'System'
];

const CHANNELS: { value: NotificationChannel; label: string }[] = [
  { value: 'InApp', label: 'In-App' },
  { value: 'Email', label: 'Email' },
  { value: 'SMS', label: 'SMS' },
  { value: 'Push', label: 'Push Notification' },
  { value: 'Webhook', label: 'Webhook' },
];

export interface NotificationFormData {
  name: string;
  type: NotificationType;
  subject: string;
  bodyTemplate: string;
  channels: NotificationChannel[];
  isActive: boolean;
}

interface NotificationFormProps {
  initialData?: NotificationTemplateDto;
  onSubmit: (data: NotificationFormData) => Promise<void>;
  loading?: boolean;
  isEditMode?: boolean;
}

export function NotificationForm({ initialData, onSubmit, loading, isEditMode }: NotificationFormProps) {
  const t = useTranslate();

  const {
    register,
    handleSubmit,
    reset,
    control,
    formState: { errors, isDirty },
  } = useForm<NotificationFormData>({
    defaultValues: {
      name: '',
      type: 'Info',
      subject: '',
      bodyTemplate: '',
      channels: ['InApp'],
      isActive: true,
    },
  });

  // Reset form when initialData changes
  useEffect(() => {
    if (initialData) {
      reset({
        name: initialData.name,
        type: initialData.type,
        subject: initialData.subject,
        bodyTemplate: initialData.bodyTemplate,
        channels: initialData.channels,
        isActive: initialData.isActive,
      });
    }
  }, [initialData, reset]);

  const handleFormSubmit = handleSubmit(async (data) => {
    await onSubmit(data);
  });

  return (
    <form className="notification-form" onSubmit={handleFormSubmit}>
      {/* Section: Basic Information */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('notifications.basicInfo', 'Basic Information')}</h3>
        </div>
        <div className="form-section__content">
          <div className="form-grid form-grid--2col">
            <div className="form-field">
              <label className="form-field__label">
                {t('notifications.name', 'Name')} <span className="required">*</span>
              </label>
              <input
                type="text"
                className={`form-field__input ${errors.name ? 'form-field__input--error' : ''}`}
                placeholder="Order Confirmation"
                {...register('name', { required: t('validation.required', 'Required') })}
              />
              {errors.name && <span className="form-field__error">{errors.name.message}</span>}
            </div>

            <div className="form-field">
              <label className="form-field__label">
                {t('common.type', 'Type')} <span className="required">*</span>
              </label>
              <select
                className="form-field__select"
                disabled={isEditMode}
                {...register('type')}
              >
                {NOTIFICATION_TYPES.map((type) => (
                  <option key={type} value={type}>
                    {typeLabels[type]}
                  </option>
                ))}
              </select>
            </div>

            <div className="form-field form-field--full">
              <label className="form-field__label">
                {t('notifications.subject', 'Subject')} <span className="required">*</span>
              </label>
              <input
                type="text"
                className={`form-field__input ${errors.subject ? 'form-field__input--error' : ''}`}
                placeholder="Your order {{orderNumber}} has been confirmed"
                {...register('subject', { required: t('validation.required', 'Required') })}
              />
              {errors.subject && <span className="form-field__error">{errors.subject.message}</span>}
              <p className="form-field__hint">
                {t('notifications.subjectHint', "Use '{{variableName}}' for dynamic content")}
              </p>
            </div>

            <div className="form-field">
              <label className="form-checkbox">
                <input type="checkbox" {...register('isActive')} />
                <span>{t('common.active', 'Active')}</span>
              </label>
            </div>
          </div>
        </div>
      </section>

      {/* Section: Delivery Channels */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('notifications.channels', 'Delivery Channels')}</h3>
        </div>
        <div className="form-section__content">
          <div className="form-grid form-grid--2col">
            <div className="form-field form-field--full">
              <p className="form-field__hint" style={{ marginBottom: '1rem' }}>
                {t('notifications.channelsHint', 'Select one or more channels for delivering this notification')}
              </p>
              <Controller
                name="channels"
                control={control}
                rules={{ validate: (value) => value.length > 0 || t('validation.selectAtLeastOne', 'Select at least one channel') }}
                render={({ field }) => (
                  <div className="checkbox-group">
                    {CHANNELS.map((channel) => (
                      <label key={channel.value} className="form-checkbox">
                        <input
                          type="checkbox"
                          checked={field.value.includes(channel.value)}
                          onChange={(e) => {
                            if (e.target.checked) {
                              field.onChange([...field.value, channel.value]);
                            } else {
                              field.onChange(field.value.filter((v) => v !== channel.value));
                            }
                          }}
                        />
                        <span>{channel.label}</span>
                      </label>
                    ))}
                  </div>
                )}
              />
              {errors.channels && <span className="form-field__error">{errors.channels.message}</span>}
            </div>
          </div>
        </div>
      </section>

      {/* Section: Template Body */}
      <section className="form-section">
        <div className="form-section__header">
          <h3 className="form-section__title">{t('notifications.template', 'Template Body')}</h3>
        </div>
        <div className="form-section__content">
          <div className="form-grid form-grid--2col">
            <div className="form-field form-field--full">
              <label className="form-field__label">
                {t('notifications.bodyTemplate', 'Body Template')} <span className="required">*</span>
              </label>
              <textarea
                className={`form-field__textarea form-field__textarea--code ${errors.bodyTemplate ? 'form-field__input--error' : ''}`}
                rows={10}
                placeholder={`Dear {{customerName}},

Your order {{orderNumber}} has been confirmed and is being processed.

Order Details:
- Order Date: {{orderDate}}
- Total: {{orderTotal}}

Thank you for your business!`}
                {...register('bodyTemplate', { required: t('validation.required', 'Required') })}
              />
              {errors.bodyTemplate && <span className="form-field__error">{errors.bodyTemplate.message}</span>}
              <p className="form-field__hint">
                {t('notifications.bodyHint', "Use '{{variableName}}' for dynamic placeholders. HTML is supported for email channels.")}
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* Form Actions */}
      <div className="form-actions">
        <button
          type="submit"
          className="btn btn-primary"
          disabled={loading || (!isDirty && isEditMode)}
        >
          {loading
            ? t('common.saving', 'Saving...')
            : isEditMode
              ? t('common.save', 'Save Changes')
              : t('notifications.createTemplate', 'Create Template')}
        </button>
      </div>
    </form>
  );
}
