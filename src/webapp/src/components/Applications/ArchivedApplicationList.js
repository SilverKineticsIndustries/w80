import React, { memo } from 'react';
import ApplicationContainerList from './ApplicationContainerList'
import { useTranslation } from 'react-i18next';
import { selectArchivedApplicationIds } from '../../store/applications/selectors';

const ArchivedApplicationList = () => {

    const { t } = useTranslation(null, { keyPrefix: "application" });

    return (
        <ApplicationContainerList
            headerLabel={t("archived-applications")}
            selector={selectArchivedApplicationIds}
            allowNew={false}
            allowEdit={false}
            allowDelete={true}
            allowArchive={false}
            allowUnarchive={true}
            allowReject={false}
            allowExpand={true}
            allowCollapse={true}
            allowStateChange={false}  />
    )
}

export default memo(ArchivedApplicationList);