import React, { memo } from 'react';
import { useTranslation } from 'react-i18next';
import ApplicationContainerList from './ApplicationContainerList';
import { selectAcceptedApplicationIds } from '../../store/applications/selectors';

const AcceptedApplicationList = () => {

    const { t } = useTranslation(null, { keyPrefix: "application" });

    return (
        <ApplicationContainerList
            headerLabel={t("accepted-applications")}
            selector={selectAcceptedApplicationIds}
            allowNew={false}
            allowEdit={false}
            allowDelete={true}
            allowArchive={false}
            allowUnarchive={false}
            allowReject={false}
            allowExpand={true}
            allowCollapse={true}
            allowStateChange={false}  />
    )
}

export default memo(AcceptedApplicationList);