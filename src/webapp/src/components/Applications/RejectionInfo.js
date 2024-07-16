import React, { memo } from 'react';
import { createUseStyles } from 'react-jss';
import { useTranslation } from 'react-i18next';
import { printLocalizedDate } from '../../helpers/dates';

const styles = createUseStyles({
    fieldset: {
        border: "none",
        "& legend": {
            color: "var(--rejected)",
            fontSize: "larger",
            fontVariantCaps: "all-small-caps",
            fontWeight: "bold"
        }
    }
})

const RejectionModal = ({rejection}) =>
{
    const classes = styles();
    const { t } = useTranslation();

    return (
        <React.Fragment>
            {rejection.rejectedUTC &&
                <fieldset className={classes.fieldset}>
                    <legend>{t("application.rejected")}</legend>
                    <div>
                        {printLocalizedDate(rejection.rejectedUTC)}
                    </div>
                    <div>
                        {t("application.rejection-method")}: {rejection.method}
                    </div>
                    <div>
                        {t("application.rejection-reason")}: {rejection.reason}
                    </div>
                </fieldset>
            }
        </React.Fragment>
    )
}

export default memo(RejectionModal);