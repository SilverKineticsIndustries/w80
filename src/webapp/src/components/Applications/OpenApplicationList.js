import React from 'react';
import ApplicationList from './ApplicationList';
import { useTranslation } from 'react-i18next';
import { selectCurrentApplicationIds } from '../../store/applications/selectors';

export default function OpenApplicationList() {
    const { t } = useTranslation();
    return (
        <ApplicationList
            headerLabel={t("application.open-applications")}
            selector={selectCurrentApplicationIds}
            allowUnarchive={false} />
    )
}