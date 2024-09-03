import React, { memo } from 'react';
import ApplicationContainerList from './ApplicationContainerList';
import { useTranslation } from 'react-i18next';
import { selectRejectedApplicationIds } from '../../store/applications/selectors';

const RejectedApplicationList = () => {

    const { t } = useTranslation(null, { keyPrefix: "application" });

    return (
        <ApplicationContainerList
            headerLabel={t("rejected-applications")}
            selector={selectRejectedApplicationIds}
            allowNew={false}
            allowEdit={false}
            allowDelete={true}
            allowArchive={false}
            allowUnarchive={false}
            allowReject={false}
            allowExpand={true}
            allowAccept={false}
            allowStateChange={false} />
    )
}

export default memo(RejectedApplicationList);