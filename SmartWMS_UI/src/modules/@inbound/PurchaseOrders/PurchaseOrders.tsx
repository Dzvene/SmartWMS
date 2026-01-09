import { useState } from 'react';
import { useIntl } from 'react-intl';
import './PurchaseOrders.scss';

/**
 * Purchase Orders Module
 *
 * Manages supplier orders for inbound receiving.
 * Tracks expected deliveries and received quantities.
 */
export function PurchaseOrders() {
  const { formatMessage } = useIntl();
  const t = (id: string) => formatMessage({ id });
  const [searchQuery, setSearchQuery] = useState('');

  return (
    <div className="purchase-orders">
      <header className="purchase-orders__header">
        <div className="purchase-orders__title-section">
          <h1 className="purchase-orders__title">{t('orders.purchase.title')}</h1>
        </div>
        <div className="purchase-orders__actions">
          <button className="btn btn-secondary">
            {t('common.import')}
          </button>
          <button className="btn btn-primary">
            {t('common.create')}
          </button>
        </div>
      </header>

      <div className="purchase-orders__toolbar">
        <div className="purchase-orders__search">
          <input
            type="search"
            className="purchase-orders__search-input"
            placeholder={t('common.search')}
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
        </div>
        <div className="purchase-orders__filters">
          <button className="btn btn-secondary">
            {t('common.filter')}
          </button>
        </div>
      </div>

      <div className="purchase-orders__content">
        <div className="purchase-orders__table-container">
          <table className="purchase-orders__table">
            <thead>
              <tr>
                <th>{t('orders.purchase.orderNumber')}</th>
                <th>{t('orders.purchase.supplier')}</th>
                <th>{t('orders.purchase.orderDate')}</th>
                <th>{t('orders.purchase.expectedDate')}</th>
                <th>{t('orders.purchase.totalLines')}</th>
                <th>{t('orders.purchase.totalQty')}</th>
                <th>{t('orders.purchase.receivedQty')}</th>
                <th>{t('common.status')}</th>
              </tr>
            </thead>
            <tbody>
              <tr className="purchase-orders__empty-row">
                <td colSpan={8}>
                  <div className="purchase-orders__empty">
                    {t('common.noData')}
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}

export default PurchaseOrders;
