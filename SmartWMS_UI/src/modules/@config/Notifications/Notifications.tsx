import { useState, useMemo } from 'react';
import { useIntl } from 'react-intl';
import type { ColumnDef } from '@tanstack/react-table';

import { DataTable } from '@/components/DataTable';
import { FullscreenModal } from '@/components/FullscreenModal';
import { useGetNotificationTemplatesQuery } from '@/api/modules/notifications';
import type { NotificationTemplateDto, NotificationType } from '@/api/modules/notifications';

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

/**
 * Notifications Configuration Module
 *
 * Manages notification templates and mailing settings.
 */
export function Notifications() {
  const { formatMessage } = useIntl();
  const t = (id: string) => formatMessage({ id });

  const [selectedTemplate, setSelectedTemplate] = useState<NotificationTemplateDto | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [paginationState, setPaginationState] = useState({ page: 1, pageSize: 25 });

  const { data, isLoading } = useGetNotificationTemplatesQuery({
    page: paginationState.page,
    pageSize: paginationState.pageSize,
  });

  const templates = data?.data?.items ?? [];
  const totalRows = data?.data?.totalCount ?? 0;

  const columns = useMemo<ColumnDef<NotificationTemplateDto, unknown>[]>(
    () => [
      {
        accessorKey: 'name',
        header: t('notifications.listName'),
        size: 160,
      },
      {
        accessorKey: 'subject',
        header: 'Subject',
        size: 200,
        cell: ({ getValue }) => getValue() || '—',
      },
      {
        accessorKey: 'type',
        header: t('common.type'),
        size: 100,
        cell: ({ getValue }) => {
          const type = getValue() as NotificationType;
          return <span className={`tag tag--${type.toLowerCase()}`}>{typeLabels[type]}</span>;
        },
      },
      {
        accessorKey: 'channels',
        header: t('notifications.events'),
        size: 120,
        cell: ({ getValue }) => {
          const channels = getValue() as string[];
          return `${channels.length} channel${channels.length !== 1 ? 's' : ''}`;
        },
      },
      {
        accessorKey: 'isActive',
        header: t('common.status'),
        size: 80,
        cell: ({ getValue }) => (
          <span className={`status-badge status-badge--${getValue() ? 'active' : 'inactive'}`}>
            {getValue() ? t('status.active') : t('status.inactive')}
          </span>
        ),
      },
      {
        accessorKey: 'createdAt',
        header: 'Created',
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

  return (
    <div className="page">
      <div className="page__header">
        <h1 className="page__title">{t('notifications.title')}</h1>
        <div className="page__actions">
          <button className="btn btn--primary" onClick={handleAddNew}>
            {t('notifications.addList')}
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
          emptyMessage={t('common.noData')}
        />
      </div>

      <FullscreenModal
        open={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={selectedTemplate ? `Edit ${selectedTemplate.name}` : 'Add Notification Template'}
      >
        <div className="form">
          <p>Notification template form will be implemented here.</p>
        </div>
      </FullscreenModal>
    </div>
  );
}

export default Notifications;
