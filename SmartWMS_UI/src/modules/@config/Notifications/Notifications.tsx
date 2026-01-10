import { useState, useMemo, useEffect } from 'react';
import { useIntl } from 'react-intl';
import { useForm, Controller } from 'react-hook-form';
import type { ColumnDef } from '@tanstack/react-table';

import { DataTable } from '@/components/DataTable';
import { FullscreenModal, ModalSection } from '@/components/FullscreenModal';
import { Modal } from '@/components';
import {
  useGetNotificationTemplatesQuery,
  useCreateNotificationTemplateMutation,
  useUpdateNotificationTemplateMutation,
  useDeleteNotificationTemplateMutation,
} from '@/api/modules/notifications';
import type {
  NotificationTemplateDto,
  NotificationType,
  NotificationChannel,
  CreateNotificationTemplateRequest,
  UpdateNotificationTemplateRequest,
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

interface NotificationFormData {
  name: string;
  type: NotificationType;
  subject: string;
  bodyTemplate: string;
  channels: NotificationChannel[];
  isActive: boolean;
}

/**
 * Notifications Configuration Module
 *
 * Manages notification templates and mailing settings.
 */
export function Notifications() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });

  const [selectedTemplate, setSelectedTemplate] = useState<NotificationTemplateDto | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);
  const [paginationState, setPaginationState] = useState({ page: 1, pageSize: 25 });

  // API
  const { data, isLoading } = useGetNotificationTemplatesQuery({
    page: paginationState.page,
    pageSize: paginationState.pageSize,
  });

  const [createTemplate, { isLoading: isCreating }] = useCreateNotificationTemplateMutation();
  const [updateTemplate, { isLoading: isUpdating }] = useUpdateNotificationTemplateMutation();
  const [deleteTemplate, { isLoading: isDeleting }] = useDeleteNotificationTemplateMutation();

  const templates = data?.data?.items ?? [];
  const totalRows = data?.data?.totalCount ?? 0;

  // Form
  const {
    register,
    handleSubmit,
    reset,
    control,
    formState: { errors },
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

  // Reset form when editing changes
  useEffect(() => {
    if (selectedTemplate) {
      reset({
        name: selectedTemplate.name,
        type: selectedTemplate.type,
        subject: selectedTemplate.subject,
        bodyTemplate: selectedTemplate.bodyTemplate,
        channels: selectedTemplate.channels,
        isActive: selectedTemplate.isActive,
      });
    } else {
      reset({
        name: '',
        type: 'Info',
        subject: '',
        bodyTemplate: '',
        channels: ['InApp'],
        isActive: true,
      });
    }
  }, [selectedTemplate, reset]);

  const columns = useMemo<ColumnDef<NotificationTemplateDto, unknown>[]>(
    () => [
      {
        accessorKey: 'name',
        header: t('notifications.listName', 'Name'),
        size: 160,
      },
      {
        accessorKey: 'subject',
        header: t('notifications.subject', 'Subject'),
        size: 200,
        cell: ({ getValue }) => getValue() || '—',
      },
      {
        accessorKey: 'type',
        header: t('common.type', 'Type'),
        size: 100,
        cell: ({ getValue }) => {
          const type = getValue() as NotificationType;
          return <span className={`tag tag--${type.toLowerCase()}`}>{typeLabels[type]}</span>;
        },
      },
      {
        accessorKey: 'channels',
        header: t('notifications.channels', 'Channels'),
        size: 120,
        cell: ({ getValue }) => {
          const channels = getValue() as NotificationChannel[];
          return channels.map(ch => {
            const label = CHANNELS.find(c => c.value === ch)?.label || ch;
            return label;
          }).join(', ');
        },
      },
      {
        accessorKey: 'isActive',
        header: t('common.status', 'Status'),
        size: 80,
        cell: ({ getValue }) => (
          <span className={`status-badge status-badge--${getValue() ? 'active' : 'inactive'}`}>
            {getValue() ? t('status.active', 'Active') : t('status.inactive', 'Inactive')}
          </span>
        ),
      },
      {
        accessorKey: 'createdAt',
        header: t('common.created', 'Created'),
        size: 120,
        cell: ({ getValue }) => new Date(getValue() as string).toLocaleDateString(),
      },
    ],
    [t]
  );

  const handleRowClick = (template: NotificationTemplateDto) => {
    setSelectedTemplate(template);
    setIsModalOpen(true);
  };

  const handleAddNew = () => {
    setSelectedTemplate(null);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setSelectedTemplate(null);
  };

  const onSubmit = async (data: NotificationFormData) => {
    try {
      if (selectedTemplate) {
        const updateData: UpdateNotificationTemplateRequest = {
          name: data.name,
          subject: data.subject,
          bodyTemplate: data.bodyTemplate,
          channels: data.channels,
          isActive: data.isActive,
        };
        await updateTemplate({ id: selectedTemplate.id, data: updateData }).unwrap();
      } else {
        const createData: CreateNotificationTemplateRequest = {
          name: data.name,
          type: data.type,
          subject: data.subject,
          bodyTemplate: data.bodyTemplate,
          channels: data.channels,
          isActive: data.isActive,
        };
        await createTemplate(createData).unwrap();
      }
      handleCloseModal();
    } catch (error) {
      console.error('Failed to save template:', error);
    }
  };

  const handleDelete = async () => {
    if (!selectedTemplate) return;

    try {
      await deleteTemplate(selectedTemplate.id).unwrap();
      setDeleteConfirmOpen(false);
      handleCloseModal();
    } catch (error) {
      console.error('Failed to delete template:', error);
    }
  };

  const isSaving = isCreating || isUpdating;

  return (
    <div className="page">
      <div className="page__header">
        <h1 className="page__title">{t('notifications.title', 'Notification Templates')}</h1>
        <div className="page__actions">
          <button className="btn btn--primary" onClick={handleAddNew}>
            {t('notifications.addList', 'Add Template')}
          </button>
        </div>
      </div>

      <div className="page__content">
        <DataTable
          data={templates}
          columns={columns}
          pagination={{
            pageIndex: paginationState.page - 1,
            pageSize: paginationState.pageSize,
          }}
          onPaginationChange={({ pageIndex, pageSize }) => {
            setPaginationState({ page: pageIndex + 1, pageSize });
          }}
          totalRows={totalRows}
          onRowClick={handleRowClick}
          selectedRowId={selectedTemplate?.id}
          getRowId={(row) => row.id}
          loading={isLoading}
          emptyMessage={t('common.noData', 'No data found')}
        />
      </div>

      {/* Notification Template Form Modal */}
      <FullscreenModal
        open={isModalOpen}
        onClose={handleCloseModal}
        title={selectedTemplate ? t('notifications.editTemplate', 'Edit Template') : t('notifications.addTemplate', 'Add Template')}
        subtitle={selectedTemplate ? selectedTemplate.name : undefined}
        onSave={handleSubmit(onSubmit)}
        saveLabel={isSaving ? t('common.saving', 'Saving...') : t('common.save', 'Save')}
        saveDisabled={isSaving}
        maxWidth="lg"
      >
        <form>
          <ModalSection title={t('notifications.basicInfo', 'Basic Information')}>
            <div className="form-grid">
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
                  disabled={!!selectedTemplate}
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
                  {t('notifications.subjectHint', 'Use {{variableName}} for dynamic content')}
                </p>
              </div>
              <div className="form-field">
                <label className="form-checkbox">
                  <input type="checkbox" {...register('isActive')} />
                  <span>{t('common.active', 'Active')}</span>
                </label>
              </div>
            </div>
          </ModalSection>

          <ModalSection title={t('notifications.channels', 'Delivery Channels')}>
            <div className="form-grid">
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
          </ModalSection>

          <ModalSection title={t('notifications.template', 'Template Body')}>
            <div className="form-grid">
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
                  {t('notifications.bodyHint', 'Use {{variableName}} for dynamic placeholders. HTML is supported for email channels.')}
                </p>
              </div>
            </div>
          </ModalSection>

          {selectedTemplate && (
            <ModalSection title={t('common.dangerZone', 'Danger Zone')}>
              <div className="danger-zone">
                <p>{t('notifications.deleteWarning', 'Deleting a template will affect automated notifications using this template.')}</p>
                <button
                  type="button"
                  className="btn btn--danger"
                  onClick={() => setDeleteConfirmOpen(true)}
                >
                  {t('notifications.deleteTemplate', 'Delete Template')}
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
        title={t('notifications.deleteTemplate', 'Delete Template')}
      >
        <div className="modal__body">
          <p>
            {t(
              'notifications.deleteConfirmation',
              `Are you sure you want to delete "${selectedTemplate?.name}"? This action cannot be undone.`
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

export default Notifications;
