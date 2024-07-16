import React from 'react';
import ApplicationList from './ApplicationList';
import { useTranslation } from 'react-i18next';
import { selectRejectedApplicationIds } from '../../store/applications/selectors';

export default function RejectedApplicationList() {
    const { t } = useTranslation();
    return (
        <ApplicationList
            headerLabel={t("application.rejected-applications")}
            selector={selectRejectedApplicationIds}
            allowEdit={false}
            allowDelete={true}
            allowArchive={false}
            allowUnarchive={false}
            allowReject={false}
            allowExpand={true}
            allowAccept={false} />
    )
}