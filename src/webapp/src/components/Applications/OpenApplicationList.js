import React, { memo } from 'react';
import ApplicationContainerList from './ApplicationContainerList';
import { useTranslation } from 'react-i18next';
import { selectOpenApplicationIds } from '../../store/applications/selectors';

const OpenApplicationList = () => {

    const { t } = useTranslation(null, { keyPrefix: "application" });

    return (
        <ApplicationContainerList
            headerLabel={t("open-applications")}
            selector={selectOpenApplicationIds}
            allowNew={true}
            allowEdit={true}
            allowDelete={true}
            allowArchive={true}
            allowUnarchive={false}
            allowReject={true}
            allowExpand={true}
            allowAccept={true}
            allowStateChange={true} />
    )
}

export default memo(OpenApplicationList);