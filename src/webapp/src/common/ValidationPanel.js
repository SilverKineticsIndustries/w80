import React, { memo } from 'react';
import { useTranslation } from 'react-i18next';
import { createUseStyles } from 'react-jss';

const styles = createUseStyles({
    fieldset: {
        border: "1px solid var(--error-border)",
        marginTop: "8px",
        marginBottom: "8px"
    },
    headerMessage: {
        color: "var(--error-text)"
    },
    errorMessageList: {
        listStyleType: "none"
    },
    errorIcon: {
        color: "var(--error-text)",
        fontSize: "20px",
        fontWeight: "bold",
        paddingRight: "6px"
    },
    errorMessage: {
        color: "var(--error-text)"
    }
})

const getClientMessages = (data) => (data || []).filter(x => Object.hasOwn(x, 'clientMessage')).map(x => x["clientMessage"]);

const ValidationPanel = ({data}) =>
{
    const classes = styles();
    const messages = getClientMessages(data);
    const { t } = useTranslation(null, { keyPrefix: "common"});

    return (
        <>
            {messages?.length > 0 &&
                <div data-test="validation-panel">
                    <fieldset className={classes.fieldset}>
                    <legend className={classes.headerMessage}>{messages.length} {t("validation-errors-found")}</legend>
                        <ul className={classes.errorMessageList} data-test="validation-items-list">
                            {messages.map((x, idx) =>
                                <li key={idx}>
                                    <span className={classes.errorIcon}>&#9888;</span>
                                    <span className={classes.errorMessage} data-test="validation-item-text">{x}</span>
                                </li>
                            )}
                        </ul>
                    </fieldset>
                </div>
            }
        </>
    )
}

export default memo(ValidationPanel);