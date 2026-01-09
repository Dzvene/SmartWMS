import { useState } from 'react';
import { useIntl } from 'react-intl';
import { useNavigate, useParams } from 'react-router-dom';
import { useGetSiteByIdQuery, useUpdateSiteMutation, useDeleteSiteMutation } from '@/api/modules/sites';
import { CONFIG } from '@/constants/routes';
import { SiteForm, type SiteFormData } from './SiteForm';
import { FullscreenModal, ModalSection } from '@/components/FullscreenModal';
import './Sites.scss';

export function SiteDetails() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();

  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);

  // tenantId is automatically injected by baseApi
  const { data: siteResponse, isLoading: isLoadingSite } = useGetSiteByIdQuery(
    id || '',
    { skip: !id }
  );

  const [updateSite, { isLoading: isUpdating }] = useUpdateSiteMutation();
  const [deleteSite, { isLoading: isDeleting }] = useDeleteSiteMutation();

  const site = siteResponse?.data;

  const handleBack = () => {
    navigate(CONFIG.SITES);
  };

  const handleSubmit = async (data: SiteFormData) => {
    if (!id) return;

    try {
      await updateSite({ id, data }).unwrap();
      navigate(CONFIG.SITES);
    } catch (error) {
      console.error('Failed to update site:', error);
    }
  };

  const handleDelete = async () => {
    if (!id) return;

    try {
      await deleteSite(id).unwrap();
      setDeleteConfirmOpen(false);
      navigate(CONFIG.SITES);
    } catch (error) {
      console.error('Failed to delete site:', error);
    }
  };

  if (isLoadingSite) {
    return (
      <div className="site-details">
        <div className="site-details__loading">Loading...</div>
      </div>
    );
  }

  if (!site) {
    return (
      <div className="site-details">
        <div className="site-details__not-found">
          <h2>Site not found</h2>
          <button className="btn btn-secondary" onClick={handleBack}>
            Back to Sites
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="site-details">
      {/* Header with back button */}
      <header className="site-details__header">
        <div className="site-details__header-left">
          <button className="btn btn-ghost" onClick={handleBack}>
            <span className="btn__icon">&larr;</span>
            {t('common.back', 'Back')}
          </button>
          <div className="site-details__title-section">
            <h1 className="site-details__title">{site.name}</h1>
            {site.isPrimary && (
              <span className="badge badge--primary">{t('site.primary', 'Primary')}</span>
            )}
            <span className={`status-badge status-badge--${site.isActive ? 'active' : 'inactive'}`}>
              {site.isActive ? t('status.active', 'Active') : t('status.inactive', 'Inactive')}
            </span>
          </div>
        </div>
      </header>

      {/* Main content */}
      <div className="site-details__content">
        <div className="site-details__form-container">
          <SiteForm
            initialData={{
              code: site.code,
              name: site.name,
              description: site.description || '',
              addressLine1: site.addressLine1 || '',
              addressLine2: site.addressLine2 || '',
              city: site.city || '',
              region: site.region || '',
              postalCode: site.postalCode || '',
              countryCode: site.countryCode || '',
              timezone: site.timezone || '',
              isPrimary: site.isPrimary,
              isActive: site.isActive,
            }}
            onSubmit={handleSubmit}
            loading={isUpdating}
            isEditMode
          />
        </div>

        {/* Actions sidebar */}
        <aside className="site-details__sidebar">
          <section className="sidebar-section">
            <h3 className="sidebar-section__title">{t('site.statistics', 'Statistics')}</h3>
            <div className="sidebar-section__content">
              <dl className="info-list">
                <div className="info-list__item">
                  <dt>{t('site.warehouses', 'Warehouses')}</dt>
                  <dd>{site.warehousesCount}</dd>
                </div>
                <div className="info-list__item">
                  <dt>{t('site.createdAt', 'Created')}</dt>
                  <dd>{new Date(site.createdAt).toLocaleDateString()}</dd>
                </div>
              </dl>
            </div>
          </section>

          <section className="sidebar-section sidebar-section--danger">
            <h3 className="sidebar-section__title">{t('common.dangerZone', 'Danger Zone')}</h3>
            <div className="sidebar-section__content">
              <p className="sidebar-section__text">
                {t('site.deleteWarning', 'Deleting this site will remove all associated warehouses and locations.')}
              </p>
              <button
                className="btn btn-danger btn-block"
                onClick={() => setDeleteConfirmOpen(true)}
                disabled={isDeleting || site.isPrimary}
              >
                {t('site.deleteSite', 'Delete Site')}
              </button>
              {site.isPrimary && (
                <p className="sidebar-section__text" style={{ marginTop: '8px', fontSize: '0.75rem' }}>
                  {t('site.cannotDeletePrimary', 'Primary site cannot be deleted.')}
                </p>
              )}
            </div>
          </section>
        </aside>
      </div>

      {/* Delete Confirmation Modal */}
      <FullscreenModal
        open={deleteConfirmOpen}
        onClose={() => setDeleteConfirmOpen(false)}
        title={t('site.confirmDelete', 'Confirm Delete')}
        subtitle={site.name}
        onSave={handleDelete}
        saveLabel={t('common.delete', 'Delete')}
        loading={isDeleting}
        maxWidth="sm"
      >
        <ModalSection title="">
          <p>{t('site.deleteConfirmText', 'Are you sure you want to delete this site? This action cannot be undone.')}</p>
        </ModalSection>
      </FullscreenModal>
    </div>
  );
}

export default SiteDetails;
