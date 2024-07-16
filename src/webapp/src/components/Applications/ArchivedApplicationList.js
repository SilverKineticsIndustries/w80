import React from 'react';
import ApplicationList from './ApplicationList'
import { useTranslation } from 'react-i18next';
import { selectArchivedApplicationIds } from '../../store/applications/selectors';

export default function ArchivedApplicationList() {
    const { t } = useTranslation();
    return (
        <ApplicationList
            headerLabel={t("application.archived-applications")}
            selector={selectArchivedApplicationIds}
            allowEdit={false}
            allowDelete={true}
            allowArchive={false}
            allowUnarchive={true}
            allowReject={false}
            allowExpand={true}
            allowStateChange={false}  />
    )
}