import React, { memo } from 'react';
import OpenUrl from './OpenUrl';
import { createUseStyles } from 'react-jss';
import { useTranslation } from 'react-i18next';
import CopyToClipboard from './CopyToClipboard';

const styles = createUseStyles({
    wrapper: {
        marginTop: "10px",
        marginBottom: "10px",
        textAlign: "center"
    },
    details: {
        backgroundColor: "var(--lighter)",
        border: "2px solid var(--info-border)",
        padding: "4px",
        marginTop: "8px",
        marginBottom: "8px",
        borderRadius: "8px"
    }
});

const InfoPanel = ({header, message, showCopyToClipboard, onHide}) =>
{
    const classes = styles();
    const { t } = useTranslation(null, { keyPrefix: "common"});

    return (
        <div className={classes.wrapper} data-test="info-panel">
            {header &&
                <div data-test="info-panel-header">
                    {header}
                </div>
            }
            <div className={classes.details} data-test="info-panel-details">
                {message}
                {showCopyToClipboard && <CopyToClipboard value={message} />}
                <OpenUrl value={message} />
            </div>
            <div className="center">
                <button className='editor-button' onClick={onHide} data-test="info-panel-ok">{t("ok")}</button>
            </div>
        </div>
    )
}

export default memo(InfoPanel);