import React, { memo } from 'react';
import openurl from '../assets/openurl.png';
import ToolButton from './ToolButton';
import { useTranslation } from 'react-i18next';
import { isValidHttpUrl } from '../helpers/common';

const OpenUrl = ({value}) => {

    const { t } = useTranslation(null, { keyPrefix: "common"});

    if (isValidHttpUrl(value))
        return (
            <ToolButton onClick={() => window.open(value)} img={openurl}
                tooltip={t("open-url")} height="20px" width="20px" dataTest="open-url" />
        )
    else
        return <></>;
}

export default memo(OpenUrl);