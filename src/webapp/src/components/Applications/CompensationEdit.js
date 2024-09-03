import React, { memo, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { createUseStyles } from 'react-jss';

const styles = createUseStyles({
    flex: {
        display: "flex",
        flexWrap: "wrap",
        justifyContent: "center"
    },
    control: {
        paddingRight: "6px",
        paddingLeft: "4px"
    },
    fieldLabel: {
        display: "inline-block",
        textAlign: "right",
        paddingRight: "10px"
    }
})

const CompensationEdit = ({application, onFieldChange }) =>
{
    const classes = styles();
    const { t } = useTranslation(null, { keyPrefix: "application" });

    const handleNumberChange = useCallback((e) => {
        e.target.value = e.target.value.replace(/\D/g, '');
        onFieldChange(e, true);
    },[onFieldChange]);

    return (
        <fieldset>
            <legend>{t("compensation")}</legend>
            <div className={classes.flex}>
                <div className={classes.control}>
                    <label>
                        <div className={classes.fieldLabel}>{t("compensation-type")}:</div>
                        <select name="compensationType" value={application.compensationType || ""} onChange={onFieldChange}>
                            <option value="Salary">{t("compensation-type-salary")}</option>
                            <option value="Hourly">{t("compensation-type-hourly")}</option>
                            <option value="OneTime">{t("compensation-type-one-time")}</option>
                        </select>
                    </label>
                </div>
                <div className={classes.control}>
                    <label>
                        <div className={classes.fieldLabel}>{t("compensation-min")}:</div>
                        <input name="compensationMin" type="text" autoComplete="off"
                            value={(application.compensationMin || "").toLocaleString()}
                            onChange={handleNumberChange} />
                    </label>
                </div>
                <div className={classes.control}>
                    <label>
                        <div className={classes.fieldLabel}>{t("compensation-max")}:</div>
                        <input name="compensationMax" type="text" autoComplete="off"
                            value={(application.compensationMax || "").toLocaleString()}
                            onChange={handleNumberChange} />
                    </label>
                </div>
            </div>
        </fieldset>
    )
}

export default memo(CompensationEdit);