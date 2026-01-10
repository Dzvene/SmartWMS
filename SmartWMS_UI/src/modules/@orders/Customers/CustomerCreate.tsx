import { useIntl } from 'react-intl';
import { useNavigate } from 'react-router-dom';
import { ORDERS } from '@/constants/routes';
import { CustomerForm, type CustomerFormData } from './CustomerForm';
import { useCreateCustomerMutation } from '@/api/modules/orders';
import type { CreateCustomerRequest } from '@/api/modules/orders';

export function CustomerCreate() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();

  const [createCustomer, { isLoading: isCreating }] = useCreateCustomerMutation();

  const handleBack = () => {
    navigate(ORDERS.CUSTOMERS);
  };

  const handleSubmit = async (data: CustomerFormData) => {
    try {
      const createData: CreateCustomerRequest = {
        code: data.code,
        name: data.name,
        contactName: data.contactName || undefined,
        email: data.email || undefined,
        phone: data.phone || undefined,
        addressLine1: data.addressLine1 || undefined,
        addressLine2: data.addressLine2 || undefined,
        city: data.city || undefined,
        region: data.region || undefined,
        postalCode: data.postalCode || undefined,
        countryCode: data.countryCode || undefined,
        taxId: data.taxId || undefined,
        paymentTerms: data.paymentTerms || undefined,
        isActive: data.isActive,
      };

      const result = await createCustomer(createData).unwrap();

      // Navigate to the created customer or back to list
      if (result.data?.id) {
        navigate(`${ORDERS.CUSTOMERS}/${result.data.id}`);
      } else {
        navigate(ORDERS.CUSTOMERS);
      }
    } catch (error) {
      console.error('Failed to create customer:', error);
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
              {t('customers.createCustomer', 'Create Customer')}
            </h1>
            <span className="detail-page__subtitle">
              {t('customers.createCustomerSubtitle', 'Fill in the details to create a new customer')}
            </span>
          </div>
        </div>
      </header>

      <div className="detail-page__content">
        <CustomerForm onSubmit={handleSubmit} loading={isCreating} isEditMode={false} />
      </div>
    </div>
  );
}
