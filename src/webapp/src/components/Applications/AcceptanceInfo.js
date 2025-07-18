import { memo, useContext } from 'react';
import { createUseStyles } from 'react-jss';
import { useTranslation } from 'react-i18next';
import { printLocalizedDate } from '../../helpers/dates';
import { UserContext } from '../../App';

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

const getMethodLabel = (t, val) => {
    switch (val) {
        case "Email":
            return t("acceptance-method-email");
        case "Phone":
            return t("acceptance-method-phone");
        case "InPerson":
            return t("acceptance-method-in-person");
        default:
            return val;
    }
}

const AcceptanceModal = ({acceptance}) =>
{
    const classes = styles();
    const { currentUser } = useContext(UserContext)
    const { t } = useTranslation(null, { keyPrefix: "application" });

    return (
        <>
            {acceptance.acceptedUTC &&
                <fieldset className={classes.fieldset}>
                    <legend>{t("accepted")}</legend>
                    <div className="center">
                        {printLocalizedDate(currentUser.culture, acceptance.acceptedUTC)}
                    </div>
                    <div className="center">
                        {t("acceptance-method")} {getMethodLabel(t, acceptance.method)}
                    </div>
                </fieldset>
            }
        </>
    )
}

export default memo(AcceptanceModal);