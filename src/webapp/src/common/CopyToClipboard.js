import React, { memo } from 'react';
import clipboard from '../assets/clipboard.png';
import ToolButton from './ToolButton';
import { useTranslation } from 'react-i18next';

const CopyToClipboard = ({value}) => {
    const { t } = useTranslation();
    const noSupport = t("common.browser-no-support");

    const setToClipboard = (val) => {
        navigator.clipboard.writeText(val).then(() => {}, () => {
            alert(noSupport);
        });
    }

    return (
        <ToolButton onClick={(e) => setToClipboard(value)} img={clipboard} tooltip={t('common.copy-to-clipboard')}
            height="20px" width="20px" data-test="copy-to-clipboard" />
    )
}

export default memo(CopyToClipboard);