import { useState } from 'react';
import { useIntl } from 'react-intl';
import {
  useGetMyStatsQuery,
  useGetTaskQueueQuery,
  useGetActiveSessionsQuery,
  useStartSessionMutation,
  useEndSessionMutation,
} from '@/api/modules/operationHub';
import { useGetWarehousesQuery } from '@/api/modules/warehouses';
import './OperationHub.scss';

/**
 * Operation Hub
 *
 * Main operator dashboard for warehouse operations.
 * Shows session status, task queue, and quick actions.
 */
export function OperationHub() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });

  const [selectedWarehouseId, setSelectedWarehouseId] = useState<string>('');

  const { data: warehousesResponse } = useGetWarehousesQuery();
  const warehouses = warehousesResponse?.data?.items || [];

  const { data: myStatsResponse, isLoading: statsLoading } = useGetMyStatsQuery();
  const myStats = myStatsResponse?.data;

  const { data: taskQueueResponse, isLoading: tasksLoading } = useGetTaskQueueQuery(
    selectedWarehouseId ? { warehouseId: selectedWarehouseId, pageSize: 10 } : { pageSize: 10 }
  );
  const tasks = taskQueueResponse?.data?.items || [];

  const { data: activeSessionsResponse } = useGetActiveSessionsQuery(
    selectedWarehouseId ? { warehouseId: selectedWarehouseId } : undefined
  );
  const activeSessions = activeSessionsResponse?.data || [];

  const [startSession] = useStartSessionMutation();
  const [endSession] = useEndSessionMutation();

  const handleStartSession = async () => {
    if (!selectedWarehouseId) return;
    try {
      await startSession({
        warehouseId: selectedWarehouseId,
        deviceType: 'Desktop',
      }).unwrap();
    } catch (error) {
      console.error('Failed to start session:', error);
    }
  };

  const handleEndSession = async () => {
    if (!myStats?.currentSession?.id) return;
    try {
      await endSession(myStats.currentSession.id).unwrap();
    } catch (error) {
      console.error('Failed to end session:', error);
    }
  };

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'active':
        return 'success';
      case 'onbreak':
        return 'warning';
      case 'idle':
        return 'muted';
      default:
        return '';
    }
  };

  const getTaskTypeIcon = (taskType: string) => {
    switch (taskType.toLowerCase()) {
      case 'pick':
        return '📦';
      case 'pack':
        return '📋';
      case 'putaway':
        return '📥';
      case 'cyclecount':
        return '🔢';
      default:
        return '📝';
    }
  };

  const getPriorityClass = (priority: number) => {
    if (priority >= 8) return 'priority--high';
    if (priority >= 5) return 'priority--medium';
    return 'priority--low';
  };

  return (
    <div className="operation-hub">
      <header className="operation-hub__header">
        <div className="operation-hub__title-section">
          <h1 className="operation-hub__title">{t('operations.hub.title', 'Operation Hub')}</h1>
          <p className="operation-hub__subtitle">{t('operations.hub.subtitle', 'Warehouse Operations Dashboard')}</p>
        </div>
        <div className="operation-hub__actions">
          <select
            value={selectedWarehouseId}
            onChange={(e) => setSelectedWarehouseId(e.target.value)}
            className="operation-hub__select"
          >
            <option value="">{t('common.selectWarehouse', 'Select Warehouse')}</option>
            {warehouses.map((wh) => (
              <option key={wh.id} value={wh.id}>
                {wh.name}
              </option>
            ))}
          </select>
        </div>
      </header>

      <div className="operation-hub__grid">
        {/* Session Status Card */}
        <div className="operation-hub__card operation-hub__card--session">
          <div className="operation-hub__card-header">
            <h3>{t('operations.mySession', 'My Session')}</h3>
          </div>
          <div className="operation-hub__card-content">
            {statsLoading ? (
              <div className="loading-spinner" />
            ) : myStats?.currentSession ? (
              <div className="session-info">
                <div className={`session-status session-status--${getStatusColor(myStats.currentSession.status)}`}>
                  <span className="session-status__dot" />
                  <span className="session-status__label">{myStats.currentSession.status}</span>
                </div>
                <div className="session-info__details">
                  <div className="session-info__item">
                    <span className="session-info__label">{t('operations.warehouse', 'Warehouse')}</span>
                    <span className="session-info__value">{myStats.currentSession.warehouseName}</span>
                  </div>
                  <div className="session-info__item">
                    <span className="session-info__label">{t('operations.duration', 'Duration')}</span>
                    <span className="session-info__value">{Math.floor(myStats.currentSession.sessionDurationMinutes / 60)}h {myStats.currentSession.sessionDurationMinutes % 60}m</span>
                  </div>
                  {myStats.currentSession.currentZone && (
                    <div className="session-info__item">
                      <span className="session-info__label">{t('operations.zone', 'Zone')}</span>
                      <span className="session-info__value">{myStats.currentSession.currentZone}</span>
                    </div>
                  )}
                </div>
                <button className="btn btn-danger" onClick={handleEndSession}>
                  {t('operations.endSession', 'End Session')}
                </button>
              </div>
            ) : (
              <div className="no-session">
                <p>{t('operations.noActiveSession', 'No active session')}</p>
                <button
                  className="btn btn-primary"
                  onClick={handleStartSession}
                  disabled={!selectedWarehouseId}
                >
                  {t('operations.startSession', 'Start Session')}
                </button>
              </div>
            )}
          </div>
        </div>

        {/* Today's Stats Card */}
        <div className="operation-hub__card operation-hub__card--stats">
          <div className="operation-hub__card-header">
            <h3>{t('operations.todayStats', "Today's Stats")}</h3>
          </div>
          <div className="operation-hub__card-content">
            {myStats ? (
              <div className="today-stats">
                <div className="today-stats__item">
                  <span className="today-stats__value">{myStats.today.tasksCompleted}</span>
                  <span className="today-stats__label">{t('operations.tasksCompleted', 'Tasks')}</span>
                </div>
                <div className="today-stats__item">
                  <span className="today-stats__value">{myStats.today.unitsProcessed.toLocaleString()}</span>
                  <span className="today-stats__label">{t('operations.unitsProcessed', 'Units')}</span>
                </div>
                <div className="today-stats__item">
                  <span className="today-stats__value">{myStats.today.accuracyRate}%</span>
                  <span className="today-stats__label">{t('operations.accuracy', 'Accuracy')}</span>
                </div>
                <div className="today-stats__item">
                  <span className="today-stats__value">{myStats.today.tasksPerHour.toFixed(1)}</span>
                  <span className="today-stats__label">{t('operations.tasksPerHour', 'Tasks/hr')}</span>
                </div>
              </div>
            ) : (
              <div className="today-stats today-stats--empty">
                <p>{t('operations.noStats', 'Start a session to track stats')}</p>
              </div>
            )}
          </div>
        </div>

        {/* Current Task Card */}
        <div className="operation-hub__card operation-hub__card--task">
          <div className="operation-hub__card-header">
            <h3>{t('operations.currentTask', 'Current Task')}</h3>
          </div>
          <div className="operation-hub__card-content">
            {myStats?.currentTask ? (
              <div className="current-task">
                <div className="current-task__header">
                  <span className="current-task__type">{getTaskTypeIcon(myStats.currentTask.taskType)} {myStats.currentTask.taskType}</span>
                  <span className="current-task__number">{myStats.currentTask.taskNumber}</span>
                </div>
                <div className="current-task__details">
                  {myStats.currentTask.sku && (
                    <div className="current-task__item">
                      <span className="current-task__label">{t('operations.sku', 'SKU')}</span>
                      <code>{myStats.currentTask.sku}</code>
                    </div>
                  )}
                  {myStats.currentTask.sourceLocation && (
                    <div className="current-task__item">
                      <span className="current-task__label">{t('operations.from', 'From')}</span>
                      <code>{myStats.currentTask.sourceLocation}</code>
                    </div>
                  )}
                  {myStats.currentTask.destinationLocation && (
                    <div className="current-task__item">
                      <span className="current-task__label">{t('operations.to', 'To')}</span>
                      <code>{myStats.currentTask.destinationLocation}</code>
                    </div>
                  )}
                  {myStats.currentTask.quantity && (
                    <div className="current-task__item">
                      <span className="current-task__label">{t('operations.quantity', 'Qty')}</span>
                      <span className="current-task__qty">{myStats.currentTask.quantity} {myStats.currentTask.uom}</span>
                    </div>
                  )}
                </div>
              </div>
            ) : (
              <div className="no-task">
                <p>{t('operations.noCurrentTask', 'No task in progress')}</p>
                <button className="btn btn-secondary" disabled={!myStats?.currentSession}>
                  {t('operations.getNextTask', 'Get Next Task')}
                </button>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Task Queue */}
      <div className="operation-hub__section">
        <div className="operation-hub__section-header">
          <h2>{t('operations.taskQueue', 'Task Queue')}</h2>
          <span className="operation-hub__count">{taskQueueResponse?.data?.totalCount || 0} {t('common.total', 'total')}</span>
        </div>
        <div className="operation-hub__task-list">
          {tasksLoading ? (
            <div className="loading-spinner" />
          ) : tasks.length > 0 ? (
            <table className="operation-hub__table">
              <thead>
                <tr>
                  <th>{t('operations.type', 'Type')}</th>
                  <th>{t('operations.task', 'Task')}</th>
                  <th>{t('operations.location', 'Location')}</th>
                  <th>{t('operations.product', 'Product')}</th>
                  <th>{t('operations.quantity', 'Qty')}</th>
                  <th>{t('operations.priority', 'Priority')}</th>
                  <th>{t('operations.assignedTo', 'Assigned')}</th>
                </tr>
              </thead>
              <tbody>
                {tasks.map((task) => (
                  <tr key={task.id}>
                    <td>
                      <span className="task-type">
                        {getTaskTypeIcon(task.taskType)} {task.taskType}
                      </span>
                    </td>
                    <td><code>{task.taskNumber}</code></td>
                    <td>
                      {task.sourceLocation && <span>{task.sourceLocation}</span>}
                      {task.sourceLocation && task.destinationLocation && <span> → </span>}
                      {task.destinationLocation && <span>{task.destinationLocation}</span>}
                    </td>
                    <td>{task.sku || '-'}</td>
                    <td>{task.quantity ? `${task.quantity} ${task.uom || ''}` : '-'}</td>
                    <td>
                      <span className={`priority ${getPriorityClass(task.priority)}`}>{task.priority}</span>
                    </td>
                    <td>{task.assignedToUserName || <span className="text-muted">{t('operations.unassigned', 'Unassigned')}</span>}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          ) : (
            <div className="operation-hub__empty">
              <p>{t('operations.noTasks', 'No tasks in queue')}</p>
            </div>
          )}
        </div>
      </div>

      {/* Active Operators */}
      <div className="operation-hub__section">
        <div className="operation-hub__section-header">
          <h2>{t('operations.activeOperators', 'Active Operators')}</h2>
          <span className="operation-hub__count">{activeSessions.length} {t('common.online', 'online')}</span>
        </div>
        <div className="operation-hub__operators">
          {activeSessions.length > 0 ? (
            <div className="operators-grid">
              {activeSessions.map((session) => (
                <div key={session.id} className="operator-card">
                  <div className={`operator-card__status operator-card__status--${getStatusColor(session.status)}`} />
                  <div className="operator-card__info">
                    <span className="operator-card__name">{session.userName}</span>
                    <span className="operator-card__zone">{session.currentZone || session.warehouseName}</span>
                  </div>
                  <div className="operator-card__task">
                    {session.currentTaskType ? (
                      <span>{getTaskTypeIcon(session.currentTaskType)} {session.currentTaskType}</span>
                    ) : (
                      <span className="text-muted">{t('operations.idle', 'Idle')}</span>
                    )}
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="operation-hub__empty">
              <p>{t('operations.noActiveOperators', 'No active operators')}</p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default OperationHub;
