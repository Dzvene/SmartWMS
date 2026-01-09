import { useState } from 'react';
import { useIntl } from 'react-intl';
import { InventorySummary } from '../InventorySummary/InventorySummary';
import { StockMovements } from '../StockMovements/StockMovements';
import { OrderFulfillment } from '../OrderFulfillment/OrderFulfillment';
import { Receiving } from '../Receiving/Receiving';
import { WarehouseUtilization } from '../WarehouseUtilization/WarehouseUtilization';
import './Reports.scss';

type ReportType = 'inventory' | 'movements' | 'fulfillment' | 'receiving' | 'utilization';

interface ReportTab {
  id: ReportType;
  labelKey: string;
  icon: string;
}

const REPORT_TABS: ReportTab[] = [
  { id: 'inventory', labelKey: 'reports.inventorySummary', icon: '📦' },
  { id: 'movements', labelKey: 'reports.stockMovements', icon: '🔄' },
  { id: 'fulfillment', labelKey: 'reports.orderFulfillment', icon: '📋' },
  { id: 'receiving', labelKey: 'reports.receiving', icon: '📥' },
  { id: 'utilization', labelKey: 'reports.warehouseUtilization', icon: '🏭' },
];

/**
 * Reports Module
 *
 * Main reports page with tabs for different report types.
 */
export function Reports() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });

  const [activeReport, setActiveReport] = useState<ReportType>('inventory');

  const renderReport = () => {
    switch (activeReport) {
      case 'inventory':
        return <InventorySummary />;
      case 'movements':
        return <StockMovements />;
      case 'fulfillment':
        return <OrderFulfillment />;
      case 'receiving':
        return <Receiving />;
      case 'utilization':
        return <WarehouseUtilization />;
      default:
        return <InventorySummary />;
    }
  };

  return (
    <div className="reports">
      <header className="reports__header">
        <div className="reports__title-section">
          <h1 className="reports__title">{t('reports.title', 'Reports & Analytics')}</h1>
          <p className="reports__subtitle">{t('reports.subtitle', 'View warehouse performance and metrics')}</p>
        </div>
      </header>

      <div className="reports__tabs">
        {REPORT_TABS.map((tab) => (
          <button
            key={tab.id}
            className={`reports__tab ${activeReport === tab.id ? 'reports__tab--active' : ''}`}
            onClick={() => setActiveReport(tab.id)}
          >
            <span className="reports__tab-icon">{tab.icon}</span>
            <span className="reports__tab-label">{t(tab.labelKey, tab.id)}</span>
          </button>
        ))}
      </div>

      <div className="reports__content">
        {renderReport()}
      </div>
    </div>
  );
}

export default Reports;
