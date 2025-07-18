import { memo, useContext } from 'react';
import { createUseStyles } from 'react-jss';
import { useTranslation } from 'react-i18next';
import { printLocalizedDate } from '../../helpers/dates';
import { UserContext } from '../../App';

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
    const { currentUser } = useContext(UserContext);
    const { t } = useTranslation(null, { keyPrefix: "application" });

    return (
        <>
            {rejection.rejectedUTC &&
                <fieldset className={classes.fieldset}>
                    <legend>{t("rejected")}</legend>
                    <div>
                        {printLocalizedDate(currentUser.culture, rejection.rejectedUTC)}
                    </div>
                    <div>
                        {t("rejection-method")}: {rejection.method}
                    </div>
                    <div>
                        {t("rejection-reason")}: {rejection.reason}
                    </div>
                </fieldset>
            }
        </>
    )
}

export default memo(RejectionModal);