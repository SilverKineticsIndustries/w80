import React, { memo } from 'react';
import { createUseStyles } from 'react-jss';
import { useTranslation } from 'react-i18next';

const styles = createUseStyles({
    wrapper: {
        padding: "10px",
    },
    header: {
        color: "--var(error-header)"
    },
    details: {
        color: "--var(error-text)"
    }
})

const ErrorPanel = ({msg, headerMsg='', dontDisplayHeader=false}) =>
{
    const classes = styles();
    const { t } = useTranslation(null, { keyPrefix: "common"});

    return (
       <div className={classes.wrapper} data-test="error-panel">
            {!dontDisplayHeader &&
                <div className={classes.header} data-test="error-header">
                    { headerMsg || t("error-occured") }
                </div>
            }
            <div className={classes.details} data-test="error-details">
                { msg }
            </div>
       </div>
    )
}

export default memo(ErrorPanel);