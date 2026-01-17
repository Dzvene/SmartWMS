import { useNavigate } from 'react-router-dom';
import { useTranslate } from '@/hooks';
import { CONFIG } from '@/constants/routes';
import { BarcodeForm, type BarcodeFormData } from './BarcodeForm';
import { useCreateBarcodePrefixMutation } from '@/api/modules/configuration';
import type { CreateBarcodePrefixRequest } from '@/api/modules/configuration';

export function BarcodeCreate() {
  const t = useTranslate();
  const navigate = useNavigate();

  const [createBarcodePrefix, { isLoading: isCreating }] = useCreateBarcodePrefixMutation();

  const handleBack = () => {
    navigate(CONFIG.BARCODES);
  };

  const handleSubmit = async (data: BarcodeFormData) => {
    try {
      const createData: CreateBarcodePrefixRequest = {
        prefix: data.prefix,
        prefixType: data.prefixType,
        description: data.description || undefined,
        isActive: data.isActive,
      };

      const result = await createBarcodePrefix(createData).unwrap();

      if (result.data?.id) {
        navigate(`${CONFIG.BARCODES}/${result.data.id}`);
      } else {
        navigate(CONFIG.BARCODES);
      }
    } catch (error) {
      console.error('Failed to create barcode prefix:', error);
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
              {t('barcodes.createBarcode', 'Create Barcode Prefix')}
            </h1>
            <span className="detail-page__subtitle">
              {t('barcodes.createBarcodeSubtitle', 'Define a new barcode prefix for scanning')}
            </span>
          </div>
        </div>
      </header>

      <div className="detail-page__content">
        <BarcodeForm onSubmit={handleSubmit} loading={isCreating} isEditMode={false} />
      </div>
    </div>
  );
}

export default BarcodeCreate;
