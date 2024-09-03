import React, { memo } from 'react';
import clipboard from '../assets/clipboard.png';
import ToolButton from './ToolButton';
import { useTranslation } from 'react-i18next';

const setToClipboard = (val, nosupport) => {
    navigator.clipboard.writeText(val).then(
        () => { },
        () => { alert(nosupport); }
    );
}

const CopyToClipboard = ({value}) => {

    const { t } = useTranslation(null, { keyPrefix: "common"});

    return (
        <ToolButton onClick={() => setToClipboard(value, t("browser-no-support"))} img={clipboard}
            tooltip={t("copy-to-clipboard")} height="20px" width="20px" data-test="copy-to-clipboard" />
    )
}

export default memo(CopyToClipboard);