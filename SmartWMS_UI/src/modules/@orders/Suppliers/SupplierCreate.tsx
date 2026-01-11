import { useIntl } from 'react-intl';
import { useNavigate } from 'react-router-dom';
import { ORDERS } from '@/constants/routes';
import { SupplierForm, type SupplierFormData } from './SupplierForm';
import { useCreateSupplierMutation } from '@/api/modules/orders';
import type { CreateSupplierRequest } from '@/api/modules/orders';

export function SupplierCreate() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();

  const [createSupplier, { isLoading: isCreating }] = useCreateSupplierMutation();

  const handleBack = () => {
    navigate(ORDERS.SUPPLIERS);
  };

  const handleSubmit = async (data: SupplierFormData) => {
    try {
      const leadTimeDays = data.leadTimeDays === '' ? undefined : Number(data.leadTimeDays);

      const createData: CreateSupplierRequest = {
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
        leadTimeDays,
        isActive: data.isActive,
      };

      const result = await createSupplier(createData).unwrap();

      // Navigate to the created supplier or back to list
      if (result.data?.id) {
        navigate(`${ORDERS.SUPPLIERS}/${result.data.id}`);
      } else {
        navigate(ORDERS.SUPPLIERS);
      }
    } catch (error) {
      console.error('Failed to create supplier:', error);
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
              {t('suppliers.createSupplier', 'Create Supplier')}
            </h1>
            <span className="detail-page__subtitle">
              {t('suppliers.createSupplierSubtitle', 'Fill in the details to create a new supplier')}
            </span>
          </div>
        </div>
      </header>

      <div className="detail-page__content">
        <SupplierForm onSubmit={handleSubmit} loading={isCreating} isEditMode={false} />
      </div>
    </div>
  );
}
