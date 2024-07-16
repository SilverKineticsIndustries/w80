import React from 'react';
import ApplicationList from './ApplicationList';
import { useTranslation } from 'react-i18next';
import { selectAcceptedApplicationIds } from '../../store/applications/selectors';

export default function AcceptedApplicationList() {
    const { t } = useTranslation();
    return (
        <ApplicationList
            headerLabel={t("application.accepted-applications")}
            selector={selectAcceptedApplicationIds}
            allowEdit={false}
            allowDelete={true}
            allowArchive={false}
            allowUnarchive={false}
            allowReject={false}
            allowExpand={true}
            allowStateChange={false}  />
    )
}