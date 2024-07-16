import React, { memo } from 'react';
import { createUseStyles } from 'react-jss';
import { useTranslation } from 'react-i18next';
import { printLocalizedDate } from '../../helpers/dates';

const styles = createUseStyles({
    fieldset: {
        "& legend": {
            color: "var(--accepted)",
            fontSize: "larger",
            fontVariantCaps: "all-small-caps",
            fontWeight: "bold"
        }
    }
})

const AcceptanceModal = ({acceptance}) =>
{
    const classes = styles();
    const { t } = useTranslation();

    const getMethodLabel = (val) => {
        switch (val) {
            case "Email":
                return t("application.acceptance-method-email");
            case "Phone":
                return t("application.acceptance-method-phone");
            case "InPerson":
                return t("application.acceptance-method-in-person");
            default:
                return val;
        }
    }

    return (
        <React.Fragment>
            {acceptance.acceptedUTC &&
                <fieldset className={classes.fieldset}>
                    <legend>{t("application.accepted")}</legend>
                    <div className="center">
                        {printLocalizedDate(acceptance.acceptedUTC)}
                    </div>
                    <div className="center">
                        {t("application.acceptance-method")} {getMethodLabel(acceptance.method)}
                    </div>
                </fieldset>
            }
        </React.Fragment>
    )
}

export default memo(AcceptanceModal);