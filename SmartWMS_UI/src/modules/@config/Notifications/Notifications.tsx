import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslate } from '@/hooks';
import { DataTable, createColumns } from '@/components/DataTable';
import type { PaginationState, SortingState } from '@/components/DataTable';
import { useGetNotificationTemplatesQuery } from '@/api/modules/notifications';
import type {
  NotificationTemplateDto,
  NotificationType,
  NotificationChannel,
} from '@/api/modules/notifications';
import { CONFIG } from '@/constants/routes';

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

const CHANNELS: { value: NotificationChannel; label: string }[] = [
  { value: 'InApp', label: 'In-App' },
  { value: 'Email', label: 'Email' },
  { value: 'SMS', label: 'SMS' },
  { value: 'Push', label: 'Push Notification' },
  { value: 'Webhook', label: 'Webhook' },
];

/**
 * Notifications Configuration Module
 *
 * Manages notification templates and mailing settings.
 */
export function Notifications() {
  const t = useTranslate();
  const navigate = useNavigate();

  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 });
  const [sorting, setSorting] = useState<SortingState>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  // API
  const { data, isLoading } = useGetNotificationTemplatesQuery({
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
  });

  const templates = data?.data?.items ?? [];
  const totalRows = data?.data?.totalCount ?? 0;

  const columnHelper = createColumns<NotificationTemplateDto>();

  const columns = useMemo(
    () => [
      columnHelper.accessor('name', {
        header: t('notifications.listName', 'Name'),
        size: 160,
        cell: ({ getValue }) => (
          <span className="notifications__name">{getValue()}</span>
        ),
      }),
      columnHelper.accessor('subject', {
        header: t('notifications.subject', 'Subject'),
        size: 200,
        cell: ({ getValue }) => getValue() || '-',
      }),
      columnHelper.accessor('type', {
        header: t('common.type', 'Type'),
        size: 100,
        cell: ({ getValue }) => {
          const type = getValue() as NotificationType;
          return <span className={`tag tag--${type.toLowerCase()}`}>{typeLabels[type]}</span>;
        },
      }),
      columnHelper.accessor('channels', {
        header: t('notifications.channels', 'Channels'),
        size: 120,
        cell: ({ getValue }) => {
          const channels = getValue() as NotificationChannel[];
          return channels.map(ch => {
            const label = CHANNELS.find(c => c.value === ch)?.label || ch;
            return label;
          }).join(', ');
        },
      }),
      columnHelper.accessor('isActive', {
        header: t('common.status', 'Status'),
        size: 80,
        cell: ({ getValue }) => {
          const isActive = getValue();
          return (
            <span className={`status-badge status-badge--${isActive ? 'success' : 'neutral'}`}>
              {isActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
            </span>
          );
        },
      }),
      columnHelper.accessor('createdAt', {
        header: t('common.created', 'Created'),
        size: 120,
        cell: ({ getValue }) => new Date(getValue() as string).toLocaleDateString(),
      }),
    ],
    [columnHelper, t]
  );

  const handleRowClick = (template: NotificationTemplateDto) => {
    setSelectedId(template.id);
    navigate(`${CONFIG.NOTIFICATIONS}/${template.id}`);
  };

  const handleCreateTemplate = () => {
    navigate(CONFIG.NOTIFICATION_CREATE);
  };

  return (
    <div className="page">
      <header className="page__header">
        <div className="page__title-section">
          <h1 className="page__title">{t('notifications.title', 'Notification Templates')}</h1>
          <p className="page__subtitle">
            {t('notifications.subtitle', 'Manage notification templates and delivery settings')}
          </p>
        </div>
        <div className="page__actions">
          <button className="btn btn--primary" onClick={handleCreateTemplate}>
            {t('notifications.addTemplate', 'Add Template')}
          </button>
        </div>
      </header>

      <div className="page__content">
        <DataTable
          data={templates}
          columns={columns}
          pagination={pagination}
          onPaginationChange={setPagination}
          sorting={sorting}
          onSortingChange={setSorting}
          totalRows={totalRows}
          selectedRowId={selectedId}
          onRowClick={handleRowClick}
          getRowId={(row) => row.id}
          emptyMessage={t('notifications.noTemplates', 'No notification templates found')}
          loading={isLoading}
        />
      </div>
    </div>
  );
}

export default Notifications;
