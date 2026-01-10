import { useState } from 'react';
import { useIntl } from 'react-intl';
import './SalesOrders.scss';

/**
 * Sales Orders Module
 *
 * Manages customer orders for outbound fulfillment.
 * Tracks order status from creation through shipment.
 */
export function SalesOrders() {
  const { formatMessage } = useIntl();
  const t = (id: string) => formatMessage({ id });
  const [searchQuery, setSearchQuery] = useState('');

  return (
    <div className="page">
      <header className="page__header">
        <div className="page__title-section">
          <h1 className="page__title">{t('orders.sales.title')}</h1>
        </div>
        <div className="page__actions">
          <button className="btn btn-secondary">
            {t('common.import')}
          </button>
          <button className="btn btn-primary">
            {t('common.create')}
          </button>
        </div>
      </header>

      <div className="page-toolbar">
        <div className="page-search">
          <input
            type="text"
            className="page-search__input"
            placeholder={t('common.search')}
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
        </div>
        <div className="page-filters">
          <button className="btn btn-secondary">
            {t('common.filter')}
          </button>
        </div>
      </div>

      <div className="page__content">
        <div className="sales-orders__table-container">
          <table className="sales-orders__table">
            <thead>
              <tr>
                <th>{t('orders.sales.orderNumber')}</th>
                <th>{t('orders.sales.customer')}</th>
                <th>{t('orders.sales.orderDate')}</th>
                <th>{t('orders.sales.requiredDate')}</th>
                <th>{t('orders.sales.totalLines')}</th>
                <th>{t('orders.sales.totalQty')}</th>
                <th>{t('common.status')}</th>
                <th>{t('common.actions')}</th>
              </tr>
            </thead>
            <tbody>
              <tr className="sales-orders__empty-row">
                <td colSpan={8}>
                  <div className="sales-orders__empty">
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

export default SalesOrders;
